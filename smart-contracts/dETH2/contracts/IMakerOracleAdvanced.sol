pragma solidity ^0.5.17;

contract IMakerOracleAdvanced
{
    function set(address wat) public;
    function set(bytes12 pos, address wat) public;
    function setMin(uint96 min_) public;
    function setNext(bytes12 next_) public;
    function unset(bytes12 pos) public;
    function unset(address wat) public;
    function poke() public;
}