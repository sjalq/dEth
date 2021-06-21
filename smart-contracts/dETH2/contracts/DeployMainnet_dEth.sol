pragma solidity ^0.5.0;

import "./dETH.sol";

contract DeployMainnet_dEth 
{
    event LogContracts(Oracle _oracle, dEth _dEth);

    constructor()
        public
    {
        Oracle oracle = new Oracle(
            IMakerOracle(0x729D19f657BD0614b4985Cf1D82531c67569197B),                 //IMakerOracle _makerOracle,
            IChainLinkPriceOracle(0xAed0c38402a5d19df6E4c03F4E2DceD6e29c1ee9),        //_daiUsdOracle
            IChainLinkPriceOracle(0x5f4eC3Df9cbd43714FE2740f5E3616155c5b8419));       //_ethUsdOracle

        dEth mainnet_dEth = new dEth(
            0xD7DFA44E3dfeB1A1E65544Dc54ee02B9CbE1e66d,                 //_gulper,
            18963,                                                      //_cdpId,
            oracle,                                                     //_oracle

            0xB7c6bB064620270F8c1daA7502bCca75fC074CF4,                 //_initialRecipient
            0x93fE7D1d24bE7CB33329800ba2166f4D28Eaa553);                //_foundryTreasury)

        mainnet_dEth.setOwner(msg.sender);

        emit LogContracts(oracle, mainnet_dEth);
    }
}