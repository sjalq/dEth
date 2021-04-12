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
    let dsGuardFactory = "0x5a15566417e6C1c9546523066500bDDBc53F88C7" // address from mainnet
    let saverProxy = "0xc563ace6facd385cb1f34fa723f412cc64e63d47" // address from mainnet
    let gulper = "0x98d619675b9e1441f2b87e6d7638eaedbf6e15fb" // random addresses
    let proxyCache = "0x271293c67e2d3140a0e9381eff1f9b01e07b0795"
    let makerManager = "0x5ef30b9986345249bc32d8928B7ee64DE9435E39"
    let ethGemJoin = "0x2f0b23f53734252bda2277357e97e1517d6b042a"
    let saverProxyActions = "0x82ecd135dce65fbc6dbdd0e4237e0af93ffd5038"
    let initialRecipient = makeAccount().Address
    let foundryTreasury = makeAccount().Address
    let cdpId = bigint 18963 // https://defiexplore.com/cdp/18963

    initOraclesDefault 0.1M |> ignore

    let abi = Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/dEth.json")
    let contract = makeContract [|gulper;proxyCache;cdpId;makerManager;ethGemJoin;saverProxy;saverProxyActions;oracleContract.Address;initialRecipient;dsGuardFactory;foundryTreasury|] abi
       
    // check the rights
    let authorityAddress = contract.Query<string> "authority" [||]
    let authority = ContractPlug(ethConn, Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/DSAuthority.json"), authorityAddress)
    let functionName = Web3.Sha3("automate(uint256,uint256,uint256,uint256,uint256)").Substring(0, 8).HexToByteArray()
    let canCall = authority.Query<bool> "canCall" [|foundryTreasury; contract.Address; functionName |]

    // check the balance of initialRecipient
    let balanceOfInitialRecipient = contract.Query<bigint> "balanceOf" [|initialRecipient|]

    shouldEqualIgnoringCase gulper (contract.Query<string> "gulper" [||])
    //shouldEqualIgnoringCase proxyCache (contract.Query<string> "cache" [||])
    should equal cdpId (contract.Query<bigint> "cdpId" [||])
    shouldEqualIgnoringCase makerManager (contract.Query<string> "makerManager" [||])
    shouldEqualIgnoringCase ethGemJoin (contract.Query<string> "ethGemJoin" [||])
    shouldEqualIgnoringCase saverProxy (contract.Query<string> "saverProxy" [||])
    shouldEqualIgnoringCase saverProxyActions (contract.Query<string> "saverProxyActions" [||])
    shouldEqualIgnoringCase oracleContract.Address (contract.Query<string> "oracle" [||])
    should equal true canCall
    should greaterThan BigInteger.Zero balanceOfInitialRecipient