module dEthTests

open TestBase
open FsCheck
open FsCheck.Xunit
open FsUnit.Xunit
open Nethereum.Web3
open GenericNumber
open type Nethereum.Util.UnitConversion
open System.Numerics
open Nethereum.Util
open Nethereum.Hex.HexTypes
open Nethereum.ABI.FunctionEncoding.Attributes
open Nethereum.Web3
open Nethereum.RPC.Eth.DTOs
open Nethereum.Contracts.CQS
open Nethereum.Contracts

[<FunctionOutput>]
type LatestRoundDataOutputDTO() =
    inherit FunctionOutputDTO() 
        [<Parameter("uint80", "roundId", 1)>]
        member val public RoundId = Unchecked.defaultof<BigInteger> with get, set
        [<Parameter("int256", "answer", 2)>]
        member val public Answer = Unchecked.defaultof<BigInteger> with get, set
        [<Parameter("uint256", "startedAt", 3)>]
        member val public StartedAt = Unchecked.defaultof<BigInteger> with get, set
        [<Parameter("uint256", "updatedAt", 4)>]
        member val public UpdatedAt = Unchecked.defaultof<BigInteger> with get, set
        [<Parameter("uint80", "answeredInRound", 5)>]
        member val public AnsweredInRound = Unchecked.defaultof<BigInteger> with get, set

type System.String with
   member s1.icompare(s2: string) =
     System.String.Equals(s1, s2, System.StringComparison.CurrentCultureIgnoreCase);;

let hexGenerator = Gen.elements ( [0..15] |> Seq.map (fun i -> i.ToString("X").[0]) )

let addressGenerator =
    gen {
        let! items = Gen.arrayOfLength 40 <| hexGenerator
        return items |> System.String
    }
type MyGenerators =
  static member string() =
      {new Arbitrary<string>() with
          override x.Generator = addressGenerator
          override x.Shrinker t = Seq.empty }

do Arb.register<MyGenerators>() |> ignore

let makerOracle = makeParameterlessContract <| Abi(__SOURCE_DIRECTORY__+ "/../build/contracts/MakerOracleMock.json")
let daiUsdOracle = makeParameterlessContract <| Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/ChainLinkPriceOracleMock.json")
let ethUsdOracle = makeParameterlessContract <| Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/ChainLinkPriceOracleMock.json")
let oracleContract = makeOracle makerOracle.Address daiUsdOracle.Address ethUsdOracle.Address

[<Specification("Oracle", "constructor", 0)>]
[<Property( Arbitrary = [|typeof<MyGenerators>|], QuietOnSuccess = true, MaxTest = 20 )>]
let ``inits to provided parameters`` (makerOracle:string) (daiUsdOracle:string) (ethUsdOracle:string) = 
    let contract = makeOracle makerOracle daiUsdOracle ethUsdOracle

    //printfn "Overriden: %A; New instances: %A" res.Overr4iddenInstances res.NewInstances

    let makerOracleC = contract.Query<string> "makerOracle" [||]
    let daiOracleC = contract.Query<string> "daiUsdOracle" [||]

    ("0x" + makerOracle).icompare (contract.Query<string> "makerOracle" [||]) &&
    ("0x" + daiUsdOracle).icompare (contract.Query<string> "daiUsdOracle" [||]) &&
    ("0x" + ethUsdOracle).icompare (contract.Query<string> "ethUsdOracle" [||])

// 18 places
let toMakerPriceFormatDecimal (a:decimal) = (new BigDecimal(a) * (BigDecimal.Pow(10.0, 18.0))).Mantissa
let toMakerPriceFormat = decimal >> toMakerPriceFormatDecimal

// 8 places
let toChainLinkPriceFormatDecimal (a:decimal) = (new BigDecimal(a) * (BigDecimal.Pow(10.0, 8.0))).Mantissa
    // this is to allow decimal points
let toChainLinkPriceFormatInt (a:int) = toChainLinkPriceFormatDecimal <| decimal a

[<Specification("Oracle", "getEthDaiPrice", 0)>]
[<Property>]
let ``price is correct given source prices within ten percents of one another`` () = 
    let priceMaker = 10 // poh
    let priceDaiUsd = 5 // poh
    let priceNonMakerDaiEth = (decimal priceMaker + (decimal priceMaker) * 0.1M) // ne poh
    let priceEthUsd = priceNonMakerDaiEth / decimal priceDaiUsd

    makerOracle.ExecuteFunction "setData" [|toMakerPriceFormat priceMaker|] |> ignore
    daiUsdOracle.ExecuteFunction "setData" [|toChainLinkPriceFormatInt priceDaiUsd|] |> ignore
    ethUsdOracle.ExecuteFunction "setData" [|toChainLinkPriceFormatDecimal priceEthUsd|] |> ignore  
 
    let a = makerOracle.Query "readUint" [||]
    let b = daiUsdOracle.Query "latestRoundDataValue" [||]
    let c = ethUsdOracle.Query "latestRoundDataValue" [||]

    printfn "a:%A b:%A c:%A" a b c

    let price = oracleContract.Query<bigint> "getEthDaiPrice" [||]

    printfn "res: %A" price

    should equal (toMakerPriceFormatDecimal priceNonMakerDaiEth) price

// let ``state after solidity function call equals to the state after fsharp function call changeGulper`` a = 
//     changeGulper a = 