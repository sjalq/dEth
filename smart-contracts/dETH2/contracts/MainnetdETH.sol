pragma solidity ^0.5.0;

import "./dETH.sol";

contract DeployMainnetDEth 
{
    event LogContracts(Oracle _oracle, dETH _dEth);

    constructor()
        public
    {
        Oracle oracle = new Oracle(
            IMakerOracle(0x93fE7D1d24bE7CB33329800ba2166f4D28Eaa553),                 //IMakerOracle _makerOracle,
            IChainLinkPriceOracle(0xAed0c38402a5d19df6E4c03F4E2DceD6e29c1ee9),        //_daiUsdOracle
            IChainLinkPriceOracle(0x5f4eC3Df9cbd43714FE2740f5E3616155c5b8419));         //_ethUsdOracle

        dETH mainnetDeth = new dETH(
            0xa3cC915E9f1f81185c8C6efb00f16F100e7F07CA,                 //_gulper,
            0x271293c67E2D3140a0E9381EfF1F9b01E07B0795,                 //_proxyCache,
            18963, //test
            //18783,                                                    //_cdpId,

            0x5ef30b9986345249bc32d8928B7ee64DE9435E39,                 //_makerManager,
            0x2F0b23f53734252Bda2277357e97e1517d6B042A,                 //_ethGemJoin,

            IMCDSaverProxy(0xC563aCE6FACD385cB1F34fA723f412Cc64E63D47), //_saverProxy
            0x82ecD135Dce65Fbc6DbdD0e4237E0AF93FFD5038,                 //_saverProxyActions
            oracle,                                                     //_oracle

            0xB7c6bB064620270F8c1daA7502bCca75fC074CF4,                 //_initialRecipient
            
            0x5f4eC3Df9cbd43714FE2740f5E3616155c5b8419,                 //_dsGuardFactory
            0x93fE7D1d24bE7CB33329800ba2166f4D28Eaa553);                //_foundryTreasury)

        emit LogContracts(oracle, mainnetDeth);
    }
}