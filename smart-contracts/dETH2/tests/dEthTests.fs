module dEthTests

open TestBase
open Xunit
open FsUnit.Xunit
open Nethereum.Web3
open System.Numerics
open dEthTestsBase
open Nethereum.Hex.HexConvertors.Extensions
open Nethereum.Web3.Accounts
open System

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
    let (gulper, proxyCache, cdpId, makerManager, ethGemJoin, saverProxy, saverProxyActions, oracleContract, initialRecipient, _, foundryTreasury, contract) = getDEthContract()
       
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

[<Specification("dEth", "changeGulper", 0)>]
[<Fact>]
let ``can be changed by owner`` () =
    let (_, _, _, _, _, _, _, _, _, _, _, contract) = getDEthContract ()
    let randomAddress = makeAccount().Address
    contract.ExecuteFunction "changeGulper" [|randomAddress|] |> ignore
    shouldEqualIgnoringCase randomAddress (contract.Query<string> "gulper" [||])

[<Specification("dEth", "changeGulper", 1)>]
[<Fact>]
let ``cannot be changed by non-owner`` () = 
    let (_, _, _, _, _, _, _, _, _, _, _, contract) = getDEthContract ()
    let account = Account(hardhatPrivKey2)
    let oldGulper = contract.Query<string> "gulper" [||]
    try
        contract.ExecuteFunctionFrom "changeGulper" [|account.Address|] (EthereumConnection(hardhatURI, account.PrivateKey)) |> ignore
        should equal oldGulper (contract.Query<string> "gulper" [||])
    with 
    | a -> 
        match a.InnerException with
        | :? Nethereum.JsonRpc.Client.RpcResponseException -> ()
        | a -> raise a

[<Specification("dEth", "giveCDPToDSProxy", 0)>]
[<Fact>]
let ``dEth - giveCDPToDSProxy - can be called by owner`` () = 
    let (_, _, _, _, _, _, _, _, _, _, _, contract) = getDEthContract ()
    // should not throw | transaction reverted | message
    contract.ExecuteFunction "giveCDPToDSProxy" [|"0x732e0abd062e6bbd7e2a83d345d24f780a2abb06"|] |> ignore

[<Specification("dEth", "giveCDPToDSProxy", 0)>]
[<Fact>]
let ``dEth - giveCDPToDSProxy - cannot be called by owner`` () = 
    let (_, _, _, _, _, _, _, _, _, _, _, contract) = getDEthContract ()
        
    let account = Account(hardhatPrivKey2)
    contract.ExecuteFunctionFrom "giveCDPToDSProxy" [|"0x732e0abd062e6bbd7e2a83d345d24f780a2abb06"|] (EthereumConnection(hardhatURI, account.PrivateKey)) |> ignore