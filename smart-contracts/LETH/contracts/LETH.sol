pragma solidity ^0.5.17;

import "../../common.5/openzeppelin/token/ERC20/ERC20Detailed.sol";
import "../../common.5/openzeppelin/token/ERC20/ERC20Mintable.sol";
import "../../common.5/openzeppelin/token/ERC20/ERC20Burnable.sol";
import "../../common.5/openzeppelin/GSN/Context.sol";
import "./DefiSaver.sol";

contract LETH is Context, ERC20Detailed, ERC20Mintable, ERC20Burnable
{
    using SafeMath for uint;

    uint constant FEE_PERC = 900;
    uint constant ONE_PERC = 1000;
    uint constant HUNDRED_PERC = 100000;

    address payable public gulper;
    MCDSaverProxy public saverProxy;
    bytes32 public vaultId;

    constructor(
            address payable _gulper,
            MCDSaverProxy _saverProxy,
            bytes32 _vaultId)
        public
        ERC20Detailed("Levered Ether", "LETH", 18)
    { 
        _removeMinter(msg.sender);
        gulper = _gulper;
        saverProxy = _saverProxy;
        vaultId = _vaultId;
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

        uint ethValue = vault.collateral().sub(vault.debt().div(vault.price()));
        uint proportion = msg.value.mul(HUNDRED_PERC).div(ethValue);
        uint LETHToIssue = totalSupply().mul(proportion).div(HUNDRED_PERC);
        uint fee = msg.value.div(HUNDRED_PERC).mul(FEE_PERC);
        vault.deposit()((msg.value.sub(fee));
        gulper.deposit()(fee);
        mint(_receiver, LETHToMind);
    }

    function claim(uint _amount)
        public
    {
        // Goals:
        // 1. if the _amount being claimed does not drain the vault to below 160%
        // 2. pull out the amount of ether the senders' tokens entitle them to and send it to them

        uint ethValue = vault.collateral().sub(vault.debt().div(vault.price()));
        uint proportion = _amount.mul(HUNDRED_PERC).div(this.totalSupply());
        uint ETHToClaim = ethValue.mul(proportion).div(HUNDRED_PERC);
        uint fee = ETHToClaim.div(HUNDRED_PERC).mul(FEE_PERC);
        vault.withdraw(ETHToClaim);
        burn(msg.sender, _amount);
        msg.sender.send(ETHToClaim.sub(fee));
        gulper.deposit()(fee);
    }
}