pragma solidity ^0.5.17;

import "./dETH.sol";

contract MakerOracleMock is IMakerOracle
{
    bytes32 dataConfig;

    function setData(uint256 data) external
    {
        dataConfig = bytes32(data);
    }

    function read()
        public 
        view 
        returns(bytes32)
    {
        return dataConfig;
    }
}

contract ChainLinkPriceOracleMock is IChainLinkPriceOracle
{
    int256 answerConfig;    

    function setData(int256 data) external
    {
        answerConfig = data;
    }

    function latestRoundData()
        external
        view
        returns (
            uint80 roundId,
            int256 answer,
            uint256 startedAt,
            uint256 updatedAt,
            uint80 answeredInRound)
    {
        return (0, answerConfig, 0, 0, 0);
    }
}