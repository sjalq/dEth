module dEthTests

open TestBase
open Xunit
open FsUnit.Xunit
open Nethereum.Web3
open System.Numerics
open dEthTestsBase
open Nethereum.Hex.HexConvertors.Extensions

type System.String with
   member s1.icompare(s2: string) =
     System.String.Equals(s1, s2, System.StringComparison.CurrentCultureIgnoreCase);

[<Specification("Oracle", "constructor", 0)>]
[<Fact>]
let ``inits to provided parameters`` () =
    let makerOracle = makeAccount().Address
    let daiUsdOracle = makeAccount().Address
    let ethUsdOracle = makeAccount().Address   
    let contract = makeOracle makerOracle daiUsdOracle ethUsdOracle

    shouldEqualIgnoringCase makerOracle (contract.Query<string> "makerOracle" [||])
    shouldEqualIgnoringCase daiUsdOracle (contract.Query<string> "daiUsdOracle" [||])
    shouldEqualIgnoringCase ethUsdOracle (contract.Query<string> "ethUsdOracle" [||])

[<Specification("Oracle", "getEthDaiPrice", 0)>]
[<Fact>]
let ``price is correct given source prices within ten percents of one another`` () =
    let (_, _, priceNonMakerDaiEth, _) = initOraclesDefault 0.1M

    let a = makerOracle.Query "readUint" [||]
    let b = daiUsdOracle.Query "latestRoundDataValue" [||]
    let c = ethUsdOracle.Query "latestRoundDataValue" [||]

    printfn "a:%A b:%A c:%A" a b c

    let price = oracleContract.Query<bigint> "getEthDaiPrice" [||]

    printfn "res: %A" price

    should equal (toMakerPriceFormatDecimal priceNonMakerDaiEth) price

[<Specification("dEth", "constructor", 0)>]
[<Fact>]
let ``initializes with correct values and rights assigned`` () = 
    let gulper = "0xa3cC915E9f1f81185c8C6efb00f16F100e7F07CA"
    let proxyCache = "0x271293c67E2D3140a0E9381EfF1F9b01E07B0795"
    let cdpId = bigint 18963 // https://defiexplore.com/cdp/18963
    let makerManager = "0x5ef30b9986345249bc32d8928B7ee64DE9435E39"
    let ethGemJoin = "0x2F0b23f53734252Bda2277357e97e1517d6B042A"
    let saverProxy = "0xC563aCE6FACD385cB1F34fA723f412Cc64E63D47"
    let saverProxyActions = "0x82ecD135Dce65Fbc6DbdD0e4237E0AF93FFD5038"
    let oracleContract = makeOracle "0x729D19f657BD0614b4985Cf1D82531c67569197B" "0xAed0c38402a5d19df6E4c03F4E2DceD6e29c1ee9" "0x5f4eC3Df9cbd43714FE2740f5E3616155c5b8419"
    let initialRecipient = "0xb7c6bb064620270f8c1daa7502bcca75fc074cf4"
    let dsGuardFactory = "0x5a15566417e6C1c9546523066500bDDBc53F88C7"
    let foundryTreasury = "0x93fE7D1d24bE7CB33329800ba2166f4D28Eaa553"

    initOraclesDefault 0.1M |> ignore

    let abi = Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/dEth.json")
    let contract = makeContract [|
        gulper;proxyCache;cdpId;makerManager;ethGemJoin;
        saverProxy;saverProxyActions;oracleContract.Address;
        initialRecipient;dsGuardFactory;foundryTreasury|] abi
       
    // check the rights
    let authorityAddress = contract.Query<string> "authority" [||]
    let authority = ContractPlug(ethConn, Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/DSAuthority.json"), authorityAddress)
    let functionName = Web3.Sha3("automate(uint256,uint256,uint256,uint256,uint256)").Substring(0, 8).HexToByteArray()
    let canCall = authority.Query<bool> "canCall" [|foundryTreasury; contract.Address; functionName |]

    // check the balance of initialRecipient
    let balanceOfInitialRecipient = contract.Query<bigint> "balanceOf" [|initialRecipient|]

    shouldEqualIgnoringCase gulper (contract.Query<string> "gulper" [||])
    shouldEqualIgnoringCase proxyCache (contract.Query<string> "cache" [||])
    should equal cdpId (contract.Query<bigint> "cdpId" [||])
    shouldEqualIgnoringCase makerManager (contract.Query<string> "makerManager" [||])
    shouldEqualIgnoringCase ethGemJoin (contract.Query<string> "ethGemJoin" [||])
    shouldEqualIgnoringCase saverProxy (contract.Query<string> "saverProxy" [||])
    shouldEqualIgnoringCase saverProxyActions (contract.Query<string> "saverProxyActions" [||])
    shouldEqualIgnoringCase oracleContract.Address (contract.Query<string> "oracle" [||])
    should equal true canCall
    should greaterThan BigInteger.Zero balanceOfInitialRecipient