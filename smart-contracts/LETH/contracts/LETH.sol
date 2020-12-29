pragma solidity ^0.5.17;

import "../../common.5/openzeppelin/token/ERC20/ERC20.sol";
import "../../common.5/openzeppelin/token/ERC20/ERC20Detailed.sol";
import "../../common.5/openzeppelin/GSN/Context.sol";

contract IDSProxy
{
    function execute(address _target, bytes memory _data) public payable returns (bytes32);
    function setOwner(address owner_) public;
}

contract IMCDSaverProxy
{
    function getCdpDetailedInfo(uint _cdpId) public view returns (uint collateral, uint debt, uint price, bytes32 ilk);
    function getMaxCollateral(uint _cdpId, bytes32 _ilk) public view returns (uint);
}

contract KovanContracts
{
    address public MANAGER_ADDRESS = 0x5ef30b9986345249bc32d8928B7ee64DE9435E39;
    address public ETH_GEM_JOIN = 0xd19a770f00f89e6dd1f12e6d6e6839b95c084d85;
}

contract MainnetContracts
{
    address public MANAGER_ADDRESS = 0x1476483dd8c35f25e568113c5f70249d3976ba21;
    address public ETH_GEM_JOIN = 0x08638ef1a205be6762a8b935f5da9b700cf7322c;
}

contract Ownable
{
    address owner; 

    event OwnerChanged(address _newOwner);

    constructor(address _owner)
        public
    {
        owner = _owner;
        emit OwnerChanged(_owner);
    }

    modifier onlyOwner
    {
        require(msg.sender == owner, "only owner may call");
        _;
    }

    function changeOwner(address _newOwner)
        public
        onlyOwner
    {
        // owner is burnable so no 0x00 check is included.
        owner = _newOwner;
        emit OwnerChanged(owner);
    }
}

contract LETH is 
    Context, 
    ERC20Detailed, 
    ERC20, 
    Ownable,
    KovanContracts
{
    using SafeMath for uint;

    uint constant FEE_PERC = 9**6;
    uint constant ONE_PERC = 10**7;
    uint constant HUNDRED_PERC = 10**9;

    address payable public gulper;
    IMCDSaverProxy public saverProxy;
    address public saverProxyActions;
    IDSProxy public cdpDSProxy;
    uint public cdpId;

    constructor(
            address _owner,
            address payable _gulper,
            IMCDSaverProxy _saverProxy,
            address _saverProxyActions,
            IDSProxy _cdpDSProxy,
            uint _cdpId,
            address _initialRecipient,
            uint _initialSupply)
        public
        ERC20Detailed("Levered Ether", "LETH", 18)
        Ownable(_owner)
    { 
        gulper = _gulper;
        saverProxy = _saverProxy;
        saverProxyActions = _saverProxyActions;
        cdpDSProxy = _cdpDSProxy;
        cdpId = _cdpId;
        _mint(_initialRecipient, _initialSupply);
    }

    function changeDSProxyOwner(address _newDSProxyOwner)
        public
        onlyOwner
    {
        cdpDSProxy.setOwner(_newDSProxyOwner);
    }

    function changeGulper(address payable _newGulper)
        public
        onlyOwner
    {
        gulper = _newGulper;
    }

    function calculateIssuanceAmount(uint _collateralAmount)
        public
        view
        returns (
            uint _actualCollateralAdded,
            uint _fee,
            uint _tokensIssued)
    {
        (,,,bytes32 ilk) = saverProxy.getCdpDetailedInfo(cdpId);
        uint maxCollateral = saverProxy.getMaxCollateral(cdpId, ilk);
        
        // improve these by using r and w math functions
        _fee = _collateralAmount.mul(FEE_PERC).div(HUNDRED_PERC);
        _actualCollateralAdded = _collateralAmount.sub(_fee);
        uint proportion = _actualCollateralAdded.mul(HUNDRED_PERC).div(maxCollateral);
        _tokensIssued = totalSupply().mul(proportion).div(HUNDRED_PERC);
    }

    event Issued(
        address _receiver, 
        uint _collateralProvided,
        uint _fee,
        uint _collateralLocked,
        uint _tokensIssued);

    function issue(address _receiver)
        payable
        public
    { 
        // Goals:
        // 1. deposits etg into the vault 
        // 2. gives the holder a claim on the vault for later withdrawal

        (uint collateralToLock, uint fee, uint tokensToIssue)  = calculateIssuanceAmount(msg.value);

        bytes memory proxyCall = abi.encodeWithSignature(
            "lockETH(address,address,uint256)", 
            MANAGER_ADDRESS, 
            ETH_GEM_JOIN, 
            cdpId);
        cdpDSProxy.execute.value(collateralToLock)(saverProxyActions, proxyCall);

        (bool feePaymentSuccess,) = gulper.call.value(fee)("");
        require(feePaymentSuccess, "fee transfer to gulper failed");
        _mint(_receiver, tokensToIssue);

        emit Issued(
            _receiver, 
            msg.value, 
            fee, 
            collateralToLock, 
            tokensToIssue);
    }

    function calculateRedemptionValue(uint _tokenAmount)
        public
        view
        returns (
            uint _totalValue, 
            uint _fee, 
            uint _finalValue)
    {
        (,,,bytes32 ilk) = saverProxy.getCdpDetailedInfo(cdpId);
        uint maxCollateral = saverProxy.getMaxCollateral(cdpId, ilk);
        
        // improve these by using r and w math functions
        uint proportion = _tokenAmount.mul(HUNDRED_PERC).div(totalSupply());
        _totalValue = maxCollateral.mul(proportion).div(HUNDRED_PERC);
        _fee = _totalValue.mul(FEE_PERC).div(HUNDRED_PERC);
        _finalValue = _totalValue.sub(_fee);
    }

    event Redeemed(
        address _receiver, 
        uint _tokensRedeemed,
        uint _fee,
        uint _collateralUnlocked,
        uint _collateralReturned);

    function redeem(uint _tokensToRedeem)
        public
    {
        // Goals:
        // 1. if the _tokensToRedeem being claimed does not drain the vault to below 160%
        // 2. pull out the amount of ether the senders' tokens entitle them to and send it to them

        (uint collateralToUnlock, uint fee, uint collateralToReturn) = calculateRedemptionValue(_tokensToRedeem);

        bytes memory proxyCall = abi.encodeWithSignature(
            "freeETH(address,address,uint256,uint256)",
            MANAGER_ADDRESS,
            ETH_GEM_JOIN, 
            cdpId,
            collateralToUnlock);
        cdpDSProxy.execute(saverProxyActions, proxyCall);

        (bool feePaymentSuccess,) = gulper.call.value(fee)("");
        require(feePaymentSuccess, "fee transfer to gulper failed");
        _burn(msg.sender, _tokensToRedeem);
        (bool payoutSuccess,) = msg.sender.call.value(collateralToReturn)("");
        require(payoutSuccess, "eth payment reverted");

        emit Redeemed(
            msg.sender, 
            _tokensToRedeem,
            fee,
            collateralToUnlock,
            collateralToReturn);
    }
}