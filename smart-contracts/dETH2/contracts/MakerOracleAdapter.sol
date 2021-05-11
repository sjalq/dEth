pragma solidity ^0.5.17;

import "./dETH.sol";
import "./console.sol";

contract MakerOracleAdapter
{
    IMakerOracle public makerOracle;

    function getEthDaiPrice() 
        public
        view
        returns (uint _price)
    {
        uint makerEthUsdPrice = uint(makerOracle.read());
        console.log("result of reading", makerEthUsdPrice);

        return makerEthUsdPrice;
    }

    function setOracle(IMakerOracle makerOracle_) public
    {
        makerOracle = makerOracle_;
    }
}