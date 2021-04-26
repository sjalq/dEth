pragma solidity ^0.5.17;

contract DSValueMock
{
    uint128 val;    

    function peek() external view returns (bytes32, bool)
    {
        return (bytes32(uint256(val)), true);
    }

    function setData(uint128 _val) public
    {
        val = _val;
    }    
}