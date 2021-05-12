pragma solidity ^0.5.17;

contract VatLike {
    struct Urn {
        uint256 ink;   // Locked Collateral  [wad]
        uint256 art;   // Normalised Debt    [wad]
    }    

    function ilks(bytes32) external view returns (
        uint256 Art,  // [wad]
        uint256 rate, // [ray]
        uint256 spot, // [ray]
        uint256 line, // [rad]
        uint256 dust  // [rad]
    );

    function urns(bytes32, address) external view returns (
        uint256 ink,   // Locked Collateral  [wad]
        uint256 art);  // Normalised Debt    [wad]);

    mapping (bytes32 => mapping (address => uint)) public gem;  // [wad]

    function grab(bytes32,address,address,address,int256,int256) external;
    function hope(address) external;
    function nope(address) external;
    function suck(address u, address v, uint rad) external; // emit new dai
}