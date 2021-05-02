pragma solidity ^0.5.17;

contract IFlipper
{
    struct Bid {
        uint256 bid;  // dai paid                 [rad]
        uint256 lot;  // gems in return for bid   [wad]
        address guy;  // high bidder
        uint48  tic;  // bid expiry time          [unix epoch time]
        uint48  end;  // auction expiry time      [unix epoch time]
        address usr;
        address gal;
        uint256 tab;  // total dai wanted         [rad]
    }

    uint256 public kicks;
    mapping (uint256 => Bid) public bids;
    function file(bytes32 what, uint256 data) external;
    function tend(uint256 id, uint256 lot, uint256 bid) external;
    function tick(uint256 id) external;
}