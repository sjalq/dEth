pragma solidity ^0.5.17;

import "./dETH.sol";
/*import "./SaverProxy.sol" as A;
import "./SaverProxyActions.sol" as B;

contract SaverScript
{
    event Log(address _contract);
    constructor() 
        public
    {
        emit Log(address(new A.MCDSaverProxy()));
        emit Log(address(new B.SaverProxyActions()));
    }
}*/

// contract KovandETH is dETH
// {
//     constructor()
//         public
//         dETH(
//             0x9FFa1ca74425A4504aeb39Fc35AcC0EB3a16A00A,             //_gulper,
//             0xb38cedE531C635E7AB5e2303aD2045CA843E110A,             //_proxyCache
//             2937,                                                   //_cdpId,

//             0x1476483dD8C35F25e568113C5f70249D3976ba21,             //KovanContracts.MANAGER_ADDRESS, //_makerManager,
//             0xd19A770F00F89e6Dd1F12E6D6E6839b95C084D85,             //_ethGemJoin,

//             IMCDSaverProxy(0x0C56862c666eA39bFe669fdF309A542bF9b28a34), // DefiSaver saverProxy 
//             0x5B846f336C08741526B4E18C28384AbbdF4B3e1d,                 // DefiSaver saverProxyActions

//             0x371DBbe4Be1D3201A98dD1b97A58E71cA5AB4b9C)             //_initialRecipient)
//     { }
// }