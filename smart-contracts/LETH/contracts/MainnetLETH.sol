pragma solidity ^0.5.0;

import "./LETH.sol";

contract MainnetLETH is LETH
{
    constructor()
        public
        LETH(
            0x98D619675B9E1441F2b87E6d7638eaeDbf6e15Fb,                 //_owner,
            0x98D619675B9E1441F2b87E6d7638eaeDbf6e15Fb,                 //_gulper,
            IDSProxy(0x15282F5E014C3FCdCD5A184a924e830a46A4Fb34),       //_cdpDSProxy,
            18783,                                                      //_cdpId,

            0x5ef30b9986345249bc32d8928B7ee64DE9435E39,                 //_makerManager,
            0x2F0b23f53734252Bda2277357e97e1517d6B042A,                 //_ethGemJoin,

            IMCDSaverProxy(0xC563aCE6FACD385cB1F34fA723f412Cc64E63D47), //_saverProxy
            0x82ecD135Dce65Fbc6DbdD0e4237E0AF93FFD5038,                 //_saverProxyActions

            0x98D619675B9E1441F2b87E6d7638eaeDbf6e15Fb)                 //_initialRecipient)
    { }
}