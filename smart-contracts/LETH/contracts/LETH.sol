pragma solidity ^0.5.17;

import "../../common.5/openzeppelin/token/ERC20/ERC20.sol";
import "../../common.5/openzeppelin/token/ERC20/ERC20Detailed.sol";
import "../../common.5/openzeppelin/GSN/Context.sol";
import "./SaverProxy.sol" as A;
import "./SaverProxyActions.sol" as B;

contract LETH is Context, ERC20Detailed, ERC20
{
    using SafeMath for uint;

    uint constant FEE_PERC = 900;
    uint constant ONE_PERC = 1000;
    uint constant HUNDRED_PERC = 100000;

    address payable public gulper;
    A.MCDSaverProxy public saverProxy;
    B.SaverProxyActions public saverProxyActions;
    B.DSProxy public cdpDSProxy;
    uint public cdpId;

    constructor(
            address payable _gulper,
            A.MCDSaverProxy _saverProxy,
            B.SaverProxyActions _saverProxyActions,
            B.DSProxy _cdpDSProxy,
            uint _cdpId)
        public
        ERC20Detailed("Levered Ether", "LETH", 18)
    { 
        // _removeMinter(msg.sender);
        gulper = _gulper;
        saverProxy = _saverProxy;
        saverProxyActions = _saverProxyActions;
        cdpDSProxy = _cdpDSProxy;
        cdpId = _cdpId;
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

    function issue(address _receiver)
        payable
        public
    { 
        // Goals:
        // 1. deposits it into the vault 
        // 2. gives the holder a claim on the vault for later withdrawal

        // Logic:
        // *check how much ether there is in the vault
        // *check how much debt the vault has
        // *calculate how much the vault is worth in Ether if it were closed now.
        // *deposit msg.balance into the vault - fee
        // *send fee to the gulper contract
        // *give the minter a  proportion of the LETH such that it represents their value add to the vault

        (uint ETHToLock, uint fee, uint LETHToIssue)  = calculateIssuanceAmount(msg.value);

        bytes memory proxyCall = abi.encodeWithSignature(
            "lockETH(address,address,uint256)", 
            saverProxy.MANAGER_ADDRESS, 
            0xF8094e15c897518B5Ac5287d7070cA5850eFc6ff, 
            cdpId);
        cdpDSProxy.execute.value(ETHToLock)(address(saverProxyActions), proxyCall);

        (bool feePaymentSuccess,) = gulper.call.value(fee)("");
        require(feePaymentSuccess, "fee transfer to gulper failed");
        _mint(_receiver, LETHToIssue);
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

    function claim(uint _amount)
        public
    {
        // Goals:
        // 1. if the _amount being claimed does not drain the vault to below 160%
        // 2. pull out the amount of ether the senders' tokens entitle them to and send it to them

        (uint ETHToFree, uint fee, uint ETHToPay) = calculateRedemptionValue(_amount);

        bytes memory proxyCall = abi.encodeWithSignature(
            "freeETH(address,address,uint256,uint256)",
            saverProxy.MANAGER_ADDRESS,
            0xF8094e15c897518B5Ac5287d7070cA5850eFc6ff, 
            cdpId,
            ETHToFree);
        cdpDSProxy.execute(address(saverProxyActions), proxyCall);

        (bool feePaymentSuccess,) = gulper.call.value(fee)("");
        require(feePaymentSuccess, "fee transfer to gulper failed");
        _burn(msg.sender, _amount);
        (bool payoutSuccess,) = msg.sender.call.value(ETHToPay)("");
        require(payoutSuccess, "eth payment reverted");
    }   
}