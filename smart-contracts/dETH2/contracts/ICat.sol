pragma solidity ^0.5.17;

contract ICat {
    event Bite(
        bytes32 indexed ilk,
        address indexed urn,
        uint256 ink,
        uint256 art,
        uint256 tab,
        address flip,
        uint256 id
    );

    // --- CDP Liquidation ---
    function bite(bytes32 ilk, address urn) external returns (uint256 id);
}