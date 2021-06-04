# dEth

## Testing

### Requirements:

- npm
- api key for alchemyapi.io (free!)

### Setup (on Linux):

Run setup with your Alchemy api key as the first argument:

`./setup.sh <alchemy api key>`

In the project root folder, run the hardhat server in a terminal you can keep open, again using your Alchemy api key:

```
npx hardhat node --fork https://eth-mainnet.alchemyapi.io/v2/<alchemy api key>
```

Finally, run the tests in the tests folder:

```
cd smart-contracts/dETH2/tests
dotnet test
```

The first run of the above command may fail; if so a second attempt should work ¯\_(ツ)_/¯