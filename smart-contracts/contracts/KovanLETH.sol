pragma solidity ^0.5.0;

import "./LETH.sol";
import "./SaverProxy.sol" as A;
import "./SaverProxyActions.sol" as B;

contract KovanLETH is LETH
{
    constructor()
        public
        LETH(
            0x98D619675B9E1441F2b87E6d7638eaeDbf6e15Fb,             //_owner,
            0x9FFa1ca74425A4504aeb39Fc35AcC0EB3a16A00A,             //_gulper,
            IDSProxy(0x5Ee84f016c3F99bE81000B5AB63E7E30Fc032C66),   //_cdpDSProxy,
            2935,                                                   //_cdpId,

            0x5ef30b9986345249bc32d8928B7ee64DE9435E39,             //KovanContracts.MANAGER_ADDRESS, //_makerManager,
            0xd19A770F00F89e6Dd1F12E6D6E6839b95C084D85,             //_ethGemJoin,

            IMCDSaverProxy(address(uint(0))),
            address(0),

            0x98D619675B9E1441F2b87E6d7638eaeDbf6e15Fb)  //_initialRecipient)
    { 
        saverProxy = IMCDSaverProxy(address(new A.MCDSaverProxy()));
        saverProxyActions = address(new B.SaverProxyActions());
    }
}