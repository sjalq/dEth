pragma solidity ^0.5.17;

import "./dETH.sol";

contract IPriceFeed
{
    function post(uint128 val_, uint32 zzz_, address med_) public;
}

contract IMedianETHUSD
{
    function poke(
    uint256[] calldata val_, uint256[] calldata age_,
    uint8[] calldata v, bytes32[] calldata r, bytes32[] calldata s) external;
}

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
    function dent(uint256 id, uint256 lot, uint256 bid) external;
    function tick(uint256 id) external;
    function deal(uint256 id) external;
}

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

contract DSValueMock
{
    uint128 val;    

    function peek() external view returns (bytes32, bool)
    {
        return (bytes32(uint256(val)), true);
    }

    function setData(uint128 _val) public
    {
        val = _val;
    }    
}

contract IMakerManagerAdvanced 
{
    struct List {
        uint prev;
        uint next;
    }

    address                   public vat;
    uint                      public cdpi;      // Auto incremental
    mapping (uint => address) public urns;      // CDPId => UrnHandler
    mapping (uint => List)    public list;      // CDPId => Prev & Next CDPIds (double linked list)
    mapping (uint => address) public owns;      // CDPId => Owner
    mapping (uint => bytes32) public ilks;      // CDPId => Ilk

    mapping (address => uint) public first;     // Owner => First CDPId
    mapping (address => uint) public last;      // Owner => Last CDPId
    mapping (address => uint) public count;     // Owner => Amount of CDPs
}

contract IMakerOracleAdvanced
{
    function set(address wat) public;
    function set(bytes12 pos, address wat) public;
    function setMin(uint96 min_) public;
    function setNext(bytes12 next_) public;
    function unset(bytes12 pos) public;
    function unset(address wat) public;
    function poke() public;
}

contract IMakerOracleValueMock
{
    bytes32 val;
    
    function setData(int256 data) external
    {
        val = bytes32(data);
    }

    function peek() public view returns (bytes32, bool) {
        return (val,true);
    }
}

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
    function kiss(address a) external;
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

contract MakerOracleAdapter
{
    IMakerOracle public makerOracle;

    function getEthDaiPrice() 
        public
        view
        returns (uint _price)
    {
        uint makerEthUsdPrice = uint(makerOracle.read());
        return makerEthUsdPrice;
    }

    function setOracle(IMakerOracle makerOracle_) public
    {
        makerOracle = makerOracle_;
    }
}

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

contract MakerOracleMock is IMakerOracle
{
    bytes32 dataConfig;

    function setData(int256 data) external
    {
        dataConfig = bytes32(data);
    }

    function read()
        public 
        view 
        returns(bytes32)
    {
        return dataConfig;
    }

    function readUint()
        public 
        view 
        returns(uint)
    {
        return uint(dataConfig);
    }
}

contract ChainLinkPriceOracleMock is IChainLinkPriceOracle
{
    int256 answerConfig;    

    function setData(int256 data) external
    {
        answerConfig = data;
    }

    function latestRoundData()
        external
        view
        returns (
            uint80 roundId,
            int256 answer,
            uint256 startedAt,
            uint256 updatedAt,
            uint80 answeredInRound)
    {
        return (0, answerConfig, 0, 0, 0);
    }

    function latestRoundDataValue()
        external
        view
        returns (int256)
    {
        return answerConfig;
    }
}