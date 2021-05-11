pragma solidity ^0.5.17;

contract IMakerOracleValueMock
{
    bytes32 val;
    
    function setData(int256 data) external
    {
        val = bytes32(data);
    }

    function peek() public view returns (bytes32, bool) {
        return (val,true);
    }
}