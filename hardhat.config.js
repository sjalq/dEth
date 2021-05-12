require("@nomiclabs/hardhat-waffle");

// This is a sample Hardhat task. To learn how to create your own go to
// https://hardhat.org/guides/create-task.html
task("accounts", "Prints the list of accounts", async () => {
  const accounts = await ethers.getSigners();

  for (const account of accounts) {
    console.log(account.address);
  }
});

// You need to export an object to set up your config
// Go to https://hardhat.org/config/ to learn more

/**
 * @type import('hardhat/config').HardhatUserConfig
 */
module.exports = {
  solidity: "0.5.17",
  networks: {
    hardhat: {
      chainId: 1337,
      throwOnCallFailures: true,
      throwOnTransactionFailures: true,
      gasPrice: 0,
      forking: {
      	url: "https://eth-mainnet.alchemyapi.io/v2/5VaoQ3iNw3dVPD_PNwd5I69k3vMvdnNj",
      	blockNumber: 12330245
      }
    }
  }
};

