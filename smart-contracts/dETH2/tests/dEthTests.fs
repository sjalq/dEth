module dEthTests

open TestBase
open Xunit
open FsUnit.Xunit
open Nethereum.Web3
open System.Numerics
open Nethereum.Util
type System.String with
   member s1.icompare(s2: string) =
     System.String.Equals(s1, s2, System.StringComparison.CurrentCultureIgnoreCase);;

let makeOracle makerOracle daiUsd ethUsd =
    let abi = Abi("../../../../build/contracts/Oracle.json")
    makeContract [| makerOracle;daiUsd;ethUsd |] abi

let makerOracle = makeContract [||] <| Abi(__SOURCE_DIRECTORY__+ "/../build/contracts/MakerOracleMock.json")
let daiUsdOracle = makeContract [||] <| Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/ChainLinkPriceOracleMock.json")
let ethUsdOracle = makeContract [||] <| Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/ChainLinkPriceOracleMock.json")
let oracleContract = makeOracle makerOracle.Address daiUsdOracle.Address ethUsdOracle.Address

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

// 18 places
let toMakerPriceFormatDecimal (a:decimal) = (new BigDecimal(a) * (BigDecimal.Pow(10.0, 18.0))).Mantissa
let toMakerPriceFormat = decimal >> toMakerPriceFormatDecimal

// 8 places
let toChainLinkPriceFormatDecimal (a:decimal) = (new BigDecimal(a) * (BigDecimal.Pow(10.0, 8.0))).Mantissa
let toChainLinkPriceFormatInt (a:int) = toChainLinkPriceFormatDecimal <| decimal a

let initOracles priceMaker priceDaiUsd priceEthUsd = 
    makerOracle.ExecuteFunction "setData" [|toMakerPriceFormat priceMaker|] |> ignore
    daiUsdOracle.ExecuteFunction "setData" [|toChainLinkPriceFormatInt priceDaiUsd|] |> ignore
    ethUsdOracle.ExecuteFunction "setData" [|toChainLinkPriceFormatDecimal priceEthUsd|] |> ignore

// percent is normalized to range [0, 1]
let initOraclesDefault percentDiffNormalized = 
    let priceMaker = 10 // can be random value
    let priceDaiUsd = 5 // can be random value
    let priceNonMakerDaiEth = (decimal priceMaker + (decimal priceMaker) * percentDiffNormalized)
    let priceEthUsd = priceNonMakerDaiEth / decimal priceDaiUsd
    
    initOracles priceMaker priceDaiUsd priceEthUsd

    priceMaker, priceDaiUsd, priceNonMakerDaiEth, priceEthUsd

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
    let saverProxy = "0xfDa65289b9e84B98c01d5c8B7B2fc6cbBc506a03" // address from mainnet
    let gulper = makeAccount().Address // random addresses
    let proxyCache = makeAccount().Address
    let makerManager = makeAccount().Address
    let ethGemJoin = makeAccount().Address
    let saverProxyActions = makeAccount().Address
    let initialRecipient = makeAccount().Address
    let foundryTreasury = makeAccount().Address
    let cdpId = bigint 18963 // https://defiexplore.com/cdp/18963

    initOraclesDefault 0.1M |> ignore

    let abi = Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/dETH.json")
    let contract = makeContract [|gulper;proxyCache;cdpId;makerManager;ethGemJoin;saverProxy;saverProxyActions;oracleContract.Address;initialRecipient;dsGuardFactory;foundryTreasury|] abi
       
    // check the rights
    let authorityAddress = contract.Query<string> "authority" [||]
    let authority = ContractPlug(ethConn, Abi(__SOURCE_DIRECTORY__ + "../build/contracts/DSAuthority.json"), authorityAddress)
    let functionName = Web3.Sha3("automate(uint256,uint256,uint256,uint256,uint256)")
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