pragma solidity ^0.5.17;

contract PipLike {
    event LogNote(
        bytes4 indexed sig,
        address indexed guy,
        bytes32 indexed foo,
        bytes32 indexed bar,
        uint256 wad,
        bytes fax
    ) anonymous;

    function change(address src_) public;
    function peek() external returns (bytes32, bool);
    function rely(address usr) public;
}

contract ISpotter {

    struct Ilk {
        PipLike pip;
        uint256 mat;
    }

    mapping (bytes32 => Ilk) public ilks;

    // --- Events ---
    event Poke(
      bytes32 ilk,
      bytes32 val,  // [wad]
      uint256 spot  // [ray]
    );

    // --- Update value ---
    function poke(bytes32 ilk) external;
}