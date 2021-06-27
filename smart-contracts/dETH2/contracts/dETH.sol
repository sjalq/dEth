// todo:
// add disclaimer

pragma solidity ^0.5.17;

import "../../common.5/openzeppelin/token/ERC20/ERC20Detailed.sol";
import "../../common.5/openzeppelin/token/ERC20/ERC20.sol";
import "./DSMath.sol";
import "./DSProxy.sol";

// Number typing guide
// The subsystems we use, use different decimal systems
// Additionally we use different number assumptions for convenience
// RAY -    10**27 - Maker decimal for high precision calculation
// WAD -    10**18 - Maker decimal for token values
// PERC -   10**16 - 1% of a WAD, with 100% == 1 WAD
// CLP -    10**8  - Chainlink price format
// RATIO -  10**32 - Ratio from Maker for a CDP's debt to GDP ratio. 

contract IDSGuard is DSAuthority
{
    function permit(address src, address dst, bytes32 sig) public;
}

contract IDSGuardFactory 
{
    function newGuard() public returns (IDSGuard guard);
}

// Note:
// This is included to avoid method signature collisions between already imported 
// DSProxy's two execute functions. 
contract IDSProxy
{
    function execute(address _target, bytes memory _data) public payable returns (bytes32);
}

contract IMCDSaverProxy
{
    function getCdpDetailedInfo(uint _cdpId) public view returns (uint collateral, uint debt, uint price, bytes32 ilk);
    function getRatio(uint _cdpId, bytes32 _ilk) public view returns (uint);
}

contract IChainLinkPriceOracle
{
    function latestRoundData()
        external
        view
        returns (
            uint80 roundId,
            int256 answer,
            uint256 startedAt,
            uint256 updatedAt,
            uint80 answeredInRound);
}

contract IMakerOracle
{
    function read()
        public 
        view 
        returns(bytes32);
}

contract Oracle
{
    using SafeMath for uint256;

    uint constant ONE_PERC = 10**16; // 1.0%
    uint constant HUNDRED_PERC = 10**18; // 100.0%

    IMakerOracle public makerOracle;
    IChainLinkPriceOracle public daiUsdOracle;
    IChainLinkPriceOracle public ethUsdOracle;

    constructor (
            IMakerOracle _makerOracle, 
            IChainLinkPriceOracle _daiUsdOracle, 
            IChainLinkPriceOracle _ethUsdOracle) 
        public
    {
        makerOracle = _makerOracle;
        daiUsdOracle = _daiUsdOracle;
        ethUsdOracle = _ethUsdOracle;
    }   

    function getEthDaiPrice() 
        public
        view
        returns (uint _price)
    {
        // maker's price comes back as a decimal with 18 places
        uint makerEthUsdPrice = uint(makerOracle.read()); 

        // chainlink's price comes back as a decimal with 8 places
        (,int chainlinkEthUsdPrice,,,) = ethUsdOracle.latestRoundData();
        (,int chainlinkDaiUsdPrice,,,) = daiUsdOracle.latestRoundData();

        // chainlink's price comes back as a decimal with 8 places
        // multiplying two of them, produces 16 places
        // we need it in the WAD format which has 18, therefore .mul(10**2) at the end
        uint chainlinkEthDaiPrice = uint(chainlinkEthUsdPrice).mul(uint(chainlinkDaiUsdPrice)).mul(10**2);
    
        // if the differnce between the ethdai price from chainlink is more than 10% from the
        // maker oracle price, trust the maker oracle 
        uint percDiff = absDiff(makerEthUsdPrice, uint(chainlinkEthDaiPrice))
            .mul(HUNDRED_PERC)
            .div(makerEthUsdPrice);
        return percDiff > ONE_PERC.mul(10) ? 
            makerEthUsdPrice :
            chainlinkEthDaiPrice;
    }

    function absDiff(uint a, uint b)
        internal
        pure
        returns(uint)
    {
        return a > b ? a - b : b - a;
    }
}

// Description:
// This contract tokenizes ownership of a Maker CDP. It does so by allowing anyone to mint new
// tokens in exchange for collateral and issues tokens in proportion to the excess collateral
// that is already in the CDP. It also allows anyone with dEth tokens to redeem these tokens
// in exchange for the excess collateral in the CDP, proportional to their share of total dEth
// tokens.
// Furthermore the contract inherits from DSProxy which allows its CDP to be automated via the 
// DeFiSaver ecosystem. This automation is activated by calling the subscribe() method on the
// DeFiSaver SubscriptionsProxyV2 contract via the execute() method inherited from DSProxy.
// This automation will automatically increase the leverage of the CDP to a target ratio if the
// collateral increases in value and automatically decrease it to the target ratio if the 
// collateral falls in value. 
// SubscriptionsProxyV2 can be viewed here:
// https://etherscan.io/address/0xB78EbeD358Eb5a94Deb08Dc97846002F0632c99A#code
// An audit of the DeFiSaver system can be viewed here:
// https://github.com/DecenterApps/defisaver-contracts/blob/master/audits/Dedaub%20-%20DeFi%20Saver%20Automation%20Audit%20-%20February%202021.pdf

// When activate the automation makes the dEth contract a perpetually levered long position on
// the price of Ether in US dollars. 

// Details:
// The contract charges a protocol fee that is paid out to contract called the gulper. The fee
// is fixed at 0.9%. 
// Due to the sometimes extreme gas fees required to run the DefiSaver automations, an 
// additional automation fee is charged to anyone entering or exiting the system. This fee can 
// be increased or decreased as needed to compensate existing participants.
// There is a riskLimit parameter that prevents the system from acquiring too much collateral 
// before it has established a record of safety. This can also be used to prevent new 
// participants from minting new dEth in case an upgrade is necessary and dEth tokens need to 
// be migrated to a new version.
// The minRedemptionRatio parameter prevents too much collateral being removed at once from
// the CDP before DefiSaver has the opportunity to return the CDP to its target parameters. 

// Note: 
// What is not apparent explicitly in this contract is how calls to the "auth" methods are to
// be dealt with. All auth methods will initially be owned by the owner key of this contract. 
// The intent is to keep it under the control of the owner key until some history of use can be
// built up to increase confidence that the contract is indeed safe and stable in the wild.
// Thereafter the owner key will be given to an OpenZeppelin TimelockController contract with a
// 48 hour delay. The TimelockController in turn will be owned by the FoundryDAO and controlled
// via it's governance structures. This will give any participants at least 48 hours to take 
// action, should any change be unpalatable. 

// Note: 
// Since defisaver automation can be upgraded and since older versions of their subscription 
// contract are not guarenteed to be updated by their offchain services and since the calling 
// of the automation script involves passing in a custom contract to where a delgate call is
// made; it is safer to rather execute the automation script via an execute(_address, _data) 
// call inherited from DSProxy through the auth system.

contract dEth is 
    ERC20Detailed, 
    ERC20,
    DSMath,
    DSProxy
{
    using SafeMath for uint;

    string constant terms = "By interacting with this contract, you agree to be bound by the terms of service found at https://www.FoundryDao.com/dEthTerms/";

    uint constant ONE_PERC = 10**16;                    //   1.0% 
    uint constant HUNDRED_PERC = 10**18;                // 100.0%

    uint constant PROTOCOL_FEE_PERC = 9*10**15;         //   0.9%
    
    address payable public gulper;
    uint public cdpId;
    
    // Note:
    // Since these items are not available on test net and represent interactions
    // with the larger DeFi ecosystem, they are directly addressed here with the understanding
    // that testing occurs against simulated forks of the the Ethereum mainnet. 
    address constant public makerManager = 0x5ef30b9986345249bc32d8928B7ee64DE9435E39;
    address constant public ethGemJoin = 0x2F0b23f53734252Bda2277357e97e1517d6B042A;
    address constant public saverProxy = 0xC563aCE6FACD385cB1F34fA723f412Cc64E63D47;
    address constant public saverProxyActions = 0x82ecD135Dce65Fbc6DbdD0e4237E0AF93FFD5038;

    Oracle public oracle;

    // automation variables
    uint public minRedemptionRatio; // the min % excess collateral that must remain after any ETH redeem action
    uint public automationFeePerc;  // the fee that goes to the collateral pool, on entry or exit, to compensate for potentially triggering a boost or redeem
    
    // Note:
    // riskLimit sets the maximum amount of excess collateral Eth the contract will place at risk
    // When exceeded it is no longer possible to issue dEth via the squander function
    // This can also be used to retire the contract by setting it to 0
    uint public riskLimit; 
    
    constructor(
            address payable _gulper,
            uint _cdpId,
            Oracle _oracle,
            address _initialRecipient,
            address _automationAuthority)
        public
        DSProxy(0x271293c67E2D3140a0E9381EfF1F9b01E07B0795) //_proxyCache on mainnet
        ERC20Detailed("Derived Ether", "dEth", 18)
    {
        gulper = _gulper;
        cdpId = _cdpId;

        oracle = _oracle;

        // Initial values of automation variables
        minRedemptionRatio = uint(160).mul(ONE_PERC).mul(10**18);
        automationFeePerc = ONE_PERC; // 1.0%
        riskLimit = 1000*10**18;      // sets an initial limit of 1000 ETH that the contract will risk. 

        // distributes the initial supply of dEth to the initial recipient at 1 ETH to 1 dEth
        uint excess = getExcessCollateral();
        _mint(_initialRecipient, excess);

        // set the automation authority to make sure the parameters can be adjusted later on
        IDSGuard guard = IDSGuardFactory(0x5a15566417e6C1c9546523066500bDDBc53F88C7).newGuard(); // DSGuardFactory
        guard.permit(
            _automationAuthority,
            address(this),
            bytes4(keccak256("changeSettings(uint256,uint256,uint256)")));
        setAuthority(guard);

        require(
            authority.canCall(
                _automationAuthority, 
                address(this), 
                bytes4(keccak256("changeSettings(uint256,uint256,uint256)"))),
            "guard setting failed");
    }

    function changeGulper(address payable _newGulper)
        public
        auth
    {
        gulper = _newGulper;
    }

    function giveCDPToDSProxy(address _dsProxy)
        public
        auth
    {
        bytes memory giveProxyCall = abi.encodeWithSignature(
            "give(address,uint256,address)", 
            makerManager, 
            cdpId, 
            _dsProxy);
        
        IDSProxy(address(this)).execute(saverProxyActions, giveProxyCall);
    }

    function getCollateral()
        public
        view
        returns(uint _priceRAY, uint _totalCollateral, uint _debt, uint _collateralDenominatedDebt, uint _excessCollateral)
    {
        _priceRAY = getCollateralPriceRAY();
        (_totalCollateral, _debt,,) = IMCDSaverProxy(saverProxy).getCdpDetailedInfo(cdpId);
        _collateralDenominatedDebt = rdiv(_debt, _priceRAY);
        _excessCollateral = sub(_totalCollateral, _collateralDenominatedDebt);
    }

    function getCollateralPriceRAY()
        public
        view
        returns (uint _priceRAY)
    {
        // we multiply by 10^9 to cast the price to a RAY number as used by the Maker CDP
        _priceRAY = oracle.getEthDaiPrice().mul(10**9);
    }

    function getExcessCollateral()
        public
        view
        returns(uint _excessCollateral)
    {
        (,,,, _excessCollateral) = getCollateral();
    }

    function getRatio()
        public
        view
        returns(uint _ratio)
    {
        (,,,bytes32 ilk) = IMCDSaverProxy(saverProxy).getCdpDetailedInfo(cdpId);
        _ratio = IMCDSaverProxy(saverProxy).getRatio(cdpId, ilk);
    }

    function calculateIssuanceAmount(uint _suppliedCollateral)
        public
        view
        returns (
            uint _protocolFee,
            uint _automationFee,
            uint _actualCollateralAdded,
            uint _accreditedCollateral,
            uint _tokensIssued)
    {
        _protocolFee = _suppliedCollateral.mul(PROTOCOL_FEE_PERC).div(HUNDRED_PERC);
        _automationFee = _suppliedCollateral.mul(automationFeePerc).div(HUNDRED_PERC);
        _actualCollateralAdded = _suppliedCollateral.sub(_protocolFee); 
        _accreditedCollateral = _actualCollateralAdded.sub(_automationFee); 
        uint newTokenSupplyPerc = _accreditedCollateral.mul(HUNDRED_PERC).div(getExcessCollateral());
        _tokensIssued = totalSupply().mul(newTokenSupplyPerc).div(HUNDRED_PERC);
    }

    event Issued(
        address _receiver, 
        uint _suppliedCollateral,
        uint _protocolFee,
        uint _automationFee,
        uint _actualCollateralAdded,
        uint _accreditedCollateral,
        uint _tokensIssued);

    // Note: 
    // This method should have been called issue(address _receiver), but will remain this for meme value
    function squanderMyEthForWorthlessBeansAndAgreeToTerms(address _receiver)
        payable
        public
    { 
        // Goals:
        // 1. deposit eth into the vault 
        // 2. give the holder a claim on the vault for later withdrawal to the address they choose 
        // 3. pay the protocol

        require(getExcessCollateral() < riskLimit.add(msg.value), "risk limit exceeded");

        (uint protocolFee, 
        uint automationFee, 
        uint collateralToLock, 
        uint accreditedCollateral, 
        uint tokensToIssue)  = calculateIssuanceAmount(msg.value);

        bytes memory lockETHproxyCall = abi.encodeWithSignature(
            "lockETH(address,address,uint256)", 
            makerManager, 
            ethGemJoin,
            cdpId);
        IDSProxy(address(this)).execute.value(collateralToLock)(saverProxyActions, lockETHproxyCall);
        
        (bool protocolFeePaymentSuccess,) = gulper.call.value(protocolFee)("");
        require(protocolFeePaymentSuccess, "protocol fee transfer to gulper failed");

        // Note: 
        // The automationFee is left in the CDP to cover the gas implications of leaving or joining dEth
        // This is why it is not explicitly used in this method. 

        _mint(_receiver, tokensToIssue);
        
        emit Issued(
            _receiver, 
            msg.value, 
            protocolFee,
            automationFee, 
            collateralToLock, 
            accreditedCollateral,
            tokensToIssue);
    }

    function calculateRedemptionValue(uint _tokensToRedeem)
        public
        view
        returns (
            uint _protocolFee,
            uint _automationFee,
            uint _collateralRedeemed, 
            uint _collateralReturned)
    {
        // comment: a full check against the minimum ratio might be added in a future version
        // for now keep in mind that this function may return values greater than those that 
        // could be executed in one transaction. 
        require(_tokensToRedeem <= totalSupply(), "_tokensToRedeem exceeds totalSupply()");
        uint redeemTokenSupplyPerc = _tokensToRedeem.mul(HUNDRED_PERC).div(totalSupply());
        uint collateralAffected = getExcessCollateral().mul(redeemTokenSupplyPerc).div(HUNDRED_PERC);
        _protocolFee = collateralAffected.mul(PROTOCOL_FEE_PERC).div(HUNDRED_PERC);
        _automationFee = collateralAffected.mul(automationFeePerc).div(HUNDRED_PERC);
        _collateralRedeemed = collateralAffected.sub(_automationFee); // how much capital should exit the dEth contract
        _collateralReturned = collateralAffected.sub(_protocolFee).sub(_automationFee); // how much capital should return to the user
    }

    event Redeemed(
        address _redeemer,
        address _receiver, 
        uint _tokensRedeemed,
        uint _protocolFee,
        uint _automationFee,
        uint _collateralRedeemed,
        uint _collateralReturned);

    function redeem(address _receiver, uint _tokensToRedeem)
        public
    {
        // Goals:
        // 1. if the _tokensToRedeem being claimed does not drain the vault to below 160%
        // 2. pull out the amount of ether the senders' tokens entitle them to and send it to them

        (uint protocolFee, 
        uint automationFee, 
        uint collateralToFree,
        uint collateralToReturn) = calculateRedemptionValue(_tokensToRedeem);

        bytes memory freeETHProxyCall = abi.encodeWithSignature(
            "freeETH(address,address,uint256,uint256)",
            makerManager,
            ethGemJoin,
            cdpId,
            collateralToFree);
        IDSProxy(address(this)).execute(saverProxyActions, freeETHProxyCall);

        _burn(msg.sender, _tokensToRedeem);

        (bool protocolFeePaymentSuccess,) = gulper.call.value(protocolFee)("");
        require(protocolFeePaymentSuccess, "protocol fee transfer to gulper failed");

        // note: the automationFee is left in the CDP to cover the gas implications of leaving or joining dEth
        
        (bool payoutSuccess,) = _receiver.call.value(collateralToReturn)("");
        require(payoutSuccess, "eth send to receiver reverted");

        // this ensures that the CDP will be boostable by DefiSaver before it can be bitten
        // to prevent bites, getRatio() doesn't use oracle but the price set in the MakerCDP system 
        require(getRatio() >= minRedemptionRatio, "cannot violate collateral safety ratio");

        emit Redeemed(  
            msg.sender,
            _receiver, 
            _tokensToRedeem,
            protocolFee,
            automationFee,
            collateralToFree,
            collateralToReturn);
    }
    
    event SettingsChanged(
            uint _minRedemptionRatio,
            uint _automationFeePerc,
            uint _riskLimit);

    function changeSettings(
            uint _minRedemptionRatio,
            uint _automationFeePerc,
            uint _riskLimit)
        public
        auth
    {
        minRedemptionRatio = _minRedemptionRatio.mul(ONE_PERC).mul(10**18);
        automationFeePerc = _automationFeePerc;
        riskLimit = _riskLimit;

        emit SettingsChanged(
            minRedemptionRatio,
            automationFeePerc,
            riskLimit);
    }
}