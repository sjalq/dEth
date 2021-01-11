pragma solidity ^0.5.0;

contract IERC20
{
    function balanceOf(address _account) public returns(uint);
    function totalSupply() public returns(uint);
}

contract IERC20Wrapper is IERC20
{
    function deposit(uint _amount) public;
}

contract BalancerPool is IERC20 
{
    function calcPoolOutGivenSingleIn(
            uint tokenBalanceIn,
            uint tokenWeightIn,
            uint poolSupply,
            uint totalWeight,
            uint tokenAmountIn,
            uint swapFee)
        public pure
        returns (uint poolAmountOut);

    function joinswapExternAmountIn(
            address tokenIn,
            uint256 tokenAmountIn,
            uint256 minPoolAmountOut)
        public;
}

contract Gulper
{
    // goal: a contract to receive funds in the form of eth and erc20s and spend them in predetermined ways

    constructor () public { }

    address public constant WETH;
    address public constant POOL;

    event permafrostGulped(uint _amount);
    function gulpEthToPermafrost() 
        public
    {
        // goals: 
        // 1. take the ether balance of address(this) and send it to the permafrost

        // logic:
        // *get the eth balance of this
        // *make wrapped ether
        // *calculate the min amount of pool tokens that we should receive for that much eth
        // *call joinswapExternAmountIn() for that amount of weth
        // *send the pool tokens to 0x01

        uint ethBalance = address(this).balance;
        IERC20Wrapper(WETH).deposit(ethBalance);
        uint wethPoolBalance = IERC20Wrapper(WETH).balanceOf(POOL);
        uint poolBalance = BalancerPool(POOL).totalSupply();
        uint minTokensToClaim = BalancerPool(POOL)
            .calcPoolOutGivenSingleIn(
                wethPoolBalance,
                5*10**18,
                poolBalance,
                10*10**18,
                ethBalance,
                10*17) * (100 * 10**9 / 95 * 10**9);

        BalancerPool(POOL).joinswapExternAmountIn(WETH, ethBalance, minTokensToClaim);
        uint poolTokensToBurn = BalancerPool(POOL).balanceOf(address(this)); 
        BalancerPool(POOL).transfer(address(1), poolTokensToBurn);
    }
}