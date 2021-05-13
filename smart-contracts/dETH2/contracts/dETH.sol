// Listen up degen!
// This token is called dETH (death get it) because it will kill your hard earned money!
// This contract has no tests, it was tested manually a little on Kovan!
// This contract has no audit, so you're just plain insane if you give this thing a cent!
// I know the guy who wrote this and I wouldn't trust him with mission critical code!

pragma solidity ^0.5.17;

import "../../common.5/openzeppelin/token/ERC20/ERC20Detailed.sol";
import "../../common.5/openzeppelin/token/ERC20/ERC20.sol";
import "../../common.5/openzeppelin/GSN/Context.sol";
import "./DSMath.sol";
import "./DSProxy.sol";
import "./console.sol";

contract IDSGuard is DSAuthority
{
    function permit(address src, address dst, bytes32 sig) public;
}

contract IDSGuardFactory {
    function newGuard() public returns (IDSGuard guard);
}

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

contract IPriceFeed
{
    function post(uint128 val_, uint32 zzz_, address med_) public;
}

contract IMedianETHUSD
{
    function poke(
    uint256[] calldata val_, uint256[] calldata age_,
    uint8[] calldata v, bytes32[] calldata r, bytes32[] calldata s) external;
}

contract IMakerOracle
{
    function read()
        public 
        view 
        returns(bytes32);
}

contract IVAT
{
    function urns(bytes32 ilk, address owner)
        public
        view
        returns(uint256 ink, uint256 art);

    function gem(bytes32 ilk, address owner)
        public
        view
        returns (uint);
}

contract IMakerManager 
{
    function vat()
        public
        view
        returns(IVAT);
        
    function ilks(uint256 cdpId)
        public
        view
        returns(bytes32 ilk);

    function urns (uint cdpId) 
        public 
        view
        returns(address);      // CDPId => UrnHandler        
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
    
        // if the differnce between the ethdai price from chainlink is more than 10% different from the
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

contract dEth is 
    Context, 
    ERC20Detailed, 
    ERC20,
    DSMath,
    DSProxy
{
    using SafeMath for uint;

    uint constant ONE_PERC = 10**16;                    //   1.0% 
    uint constant HUNDRED_PERC = 10**18;                // 100.0%

    uint constant PROTOCOL_FEE_PERC = 9*10**15;         //   0.9%
    
    address payable public gulper;
    uint public cdpId;
    
    address public makerManager;
    address public ethGemJoin;

    IMCDSaverProxy public saverProxy;
    address public saverProxyActions;
    Oracle public oracle;

    // automation variables
    uint public minRedemptionRatio;
    uint public automationFeePerc;
    uint public riskLimit; //sets the maximum amount of Eth the contract will risk, can also be used to retire the contract by setting it to 0
    
    constructor(
            address payable _gulper,
            address _proxyCache,
            uint _cdpId,

            address _makerManager,
            address _ethGemJoin,
            
            IMCDSaverProxy _saverProxy,
            address _saverProxyActions,
            Oracle _oracle,
            
            address _initialRecipient,
            
            address _DSGuardFactory,
            address _FoundryTreasury)
        public
        DSProxy(_proxyCache)
        ERC20Detailed("Derived Ether", "dEth", 18)
    {
        gulper = _gulper;
        cdpId = _cdpId;

        makerManager = _makerManager;
        ethGemJoin = _ethGemJoin;
        saverProxy = _saverProxy;
        saverProxyActions = _saverProxyActions;
        oracle = _oracle;
        minRedemptionRatio = 160;
        automationFeePerc = ONE_PERC;           // 1.0%
        riskLimit = 2000*10**18;      // sets an initial limit of 2000 ETH that the contract will risk. 

        
        uint excess = getExcessCollateral();
        _mint(_initialRecipient, excess);

        // set the relevant authorities to make sure the parameters can be adjusted later on
        IDSGuard guard = IDSGuardFactory(_DSGuardFactory).newGuard();
        guard.permit(
            _FoundryTreasury,
            address(this),
            bytes4(keccak256("automate(uint256,uint256,uint256,uint256,uint256,uint256)")));
        setAuthority(guard);

        require(
            authority.canCall(
                _FoundryTreasury, 
                address(this), 
                bytes4(keccak256("automate(uint256,uint256,uint256,uint256,uint256,uint256)"))),
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
        (_totalCollateral, _debt,,) = saverProxy.getCdpDetailedInfo(cdpId);
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
        (,,,bytes32 ilk) = saverProxy.getCdpDetailedInfo(cdpId);
        _ratio = saverProxy.getRatio(cdpId, ilk);
    }

    function getMinRedemptionRatio()
        public
        view
        returns(uint _minRatio)
    {
        // due to rdiv returning 10^9 less than one would intuitively expect, I've chosen to
        // set minRedemptionRatio to an integer value of discrete whole percentages for clarity 
        // and rather just multiply it by 10^9 here so that it is on the same order as getRatio() when comparing the two.
        _minRatio = rdiv(minRedemptionRatio.mul(10**9), 100);
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
        _actualCollateralAdded = _suppliedCollateral.sub(_protocolFee); // _protocolFee goes to the protocol 
        _accreditedCollateral = _actualCollateralAdded.sub(_automationFee); // _automationFee goes to the pool of funds in the cdp to offset gas implications
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

    function squanderMyEthForWorthlessBeans(address _receiver)
        payable
        public
    { 
        // Goals:
        // 1. deposits eth into the vault 
        // 2. gives the holder a claim on the vault for later withdrawal

        require(getExcessCollateral() < riskLimit.add(msg.Value), "risk limit exceeded");

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

        // note: the automationFee is left in the CDP to cover the gas implications of leaving or joining dEth

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
        require(getRatio() >= getMinRedemptionRatio(), "cannot violate collateral safety ratio");

        emit Redeemed(  
            msg.sender,
            _receiver, 
            _tokensToRedeem,
            protocolFee,
            automationFee,
            collateralToFree,
            collateralToReturn);
    }
    
    event AutomationSettingsChanged(
            uint _repaymentRatio,
            uint _targetRatio,
            uint _boostRatio,
            uint _minRedemptionRatio,
            uint _automationFeePerc,
            uint _riskLimit);

    // note: all values used by defisaver are in WAD format
    // we do not need that level of precision on this method
    // so for simplicity and readability they are all set in discrete percentages here
    function automate(
            uint _repaymentRatio,
            uint _targetRatio,
            uint _boostRatio,
            uint _minRedemptionRatio,
            uint _automationFeePerc,
            uint _riskLimit)
        public
        auth
    {
        // for reference - this function is called on the subscriptionsProxyV2: 
        // function subscribe(
        //     uint _cdpId, 
        //     uint128 _minRatio, 
        //     uint128 _maxRatio, 
        //     uint128 _optimalRatioBoost, 
        //     uint128 _optimalRatioRepay, 
        //     bool _boostEnabled, 
        //     bool _nextPriceEnabled, 
        //     address _subscriptions) 

        // since it's unclear if there's an official version of this on Kovan, this is hardcoded for mainnet
        address subscriptionsProxyV2 = 0xd6f2125bF7FE2bc793dE7685EA7DEd8bff3917DD;
        address subscriptions = 0xC45d4f6B6bf41b6EdAA58B01c4298B8d9078269a; 

        minRedemptionRatio = _minRedemptionRatio;
        automationFeePerc = _automationFeePerc;
        riskLimit = _riskLimit;

        bytes memory subscribeProxyCall = abi.encodeWithSignature(
            "subscribe(uint256,uint128,uint128,uint128,uint128,bool,bool,address)",
            cdpId, 
            _repaymentRatio * 10**16, 
            _boostRatio * 10**16,
            _targetRatio * 10**16,
            _targetRatio * 10**16,
            true,
            true,
            subscriptions);
        IDSProxy(address(this)).execute(subscriptionsProxyV2, subscribeProxyCall);
        
        emit AutomationSettingsChanged(
            _repaymentRatio,
            _targetRatio,
            _boostRatio,
            minRedemptionRatio,
            automationFeePerc,
            riskLimit);
    }
}