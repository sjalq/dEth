pragma solidity ^0.5.17;

import "../../common.5/openzeppelin/token/ERC20/ERC20Detailed.sol";
import "../../common.5/openzeppelin/token/ERC20/ERC20Mintable.sol";
import "../../common.5/openzeppelin/token/ERC20/ERC20Burnable.sol";
import "../../common.5/openzeppelin/GSN/Context.sol";
import "./SaverProxy.sol" as A;
import "./SaverProxyActions.sol" as B;

contract LETH is Context, ERC20Detailed, ERC20Mintable, ERC20Burnable
{
    using SafeMath for uint;

    uint constant FEE_PERC = 900;
    uint constant ONE_PERC = 1000;
    uint constant HUNDRED_PERC = 100000;

    address payable public gulper;
    A.MCDSaverProxy public saverProxy;
    B.SaverProxyActions public saverProxyActions;
    A.DSProxy public cdpDSProxy;
    uint public cdpId;

    constructor(
            address payable _gulper,
            A.MCDSaverProxy _saverProxy,
            B.SaverProxyActions _saverProxyActions,
            A.DSProxy _cdpDSProxy,
            uint _cdpId)
        public
        ERC20Detailed("Levered Ether", "LETH", 18)
    { 
        _removeMinter(msg.sender);
        gulper = _gulper;
        saverProxy = _saverProxy;
        saverProxyActions = _saverProxyActions;
        cdpDSProxy = _cdpDSProxy;
        cdpId = _cdpId;
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

        (,,,bytes32 ilk) = saverProxy.getCdpDetailedInfo(cdpId);
        uint maxCollateral = saverProxy.getMaxCollateral(cdpId, ilk);
        
        // improve these...?
        uint proportion = msg.value.mul(HUNDRED_PERC).div(maxCollateral);
        uint LETHToIssue = totalSupply().mul(proportion).div(HUNDRED_PERC);
        uint fee = msg.value.mul(FEE_PERC).div(HUNDRED_PERC);
        uint ETHToLock = msg.value.sub(fee);

        bytes memory proxyCall = abi.encodeWithSignature(
            "lockETH(address,address,uint256)", 
            saverProxy.MANAGER_ADDRESS, 
            0xF8094e15c897518B5Ac5287d7070cA5850eFc6ff, 
            cdpId);
        cdpDSProxy.execute(address(saverProxyActions), proxyCall); //.value(ETHToLock)

        gulper.call.value(fee)("");
        _mint(_receiver, LETHToIssue);
    }

    function claim(uint _amount)
        public
    {
        // Goals:
        // 1. if the _amount being claimed does not drain the vault to below 160%
        // 2. pull out the amount of ether the senders' tokens entitle them to and send it to them

        (,,,bytes32 ilk) = saverProxy.getCdpDetailedInfo(cdpId);
        uint maxCollateral = saverProxy.getMaxCollateral(cdpId, ilk);
        
        // improve these...?
        uint proportion = _amount.mul(HUNDRED_PERC).div(totalSupply());
        uint ETHToFree = maxCollateral.mul(proportion).div(HUNDRED_PERC);
        uint fee = ETHToFree.mul(FEE_PERC).div(HUNDRED_PERC);
        uint ETHToPay = ETHToFree.sub(fee);

        bytes memory proxyCall = abi.encodeWithSignature(
            "freeETH(address,address,uint256,uint256)",
            saverProxy.MANAGER_ADDRESS,
            0xF8094e15c897518B5Ac5287d7070cA5850eFc6ff, 
            cdpId,
            ETHToFree);
        cdpDSProxy.execute(address(saverProxyActions), proxyCall);

        gulper.call.value(fee)("");
        _burn(msg.sender, _amount);
    }
}