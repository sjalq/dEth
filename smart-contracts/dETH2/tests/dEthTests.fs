module dEthTests

open TestBase
open FsCheck
open FsCheck.Xunit

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

let makerOracle = 
    let abi = Abi("../../../../build/contracts/MakerOracleMock.json")
    
    let deployTxReceipt =
        ethConn.DeployContractAsync abi
            [| owner |]
        |> runNow

    ContractPlug(ethConn, abi, deployTxReceipt.ContractAddress)

let daiUsdOracle = 
    let abi = Abi("../../../../build/contracts/ChainLinkPriceOracleMock.json")
    
    let deployTxReceipt =
        ethConn.DeployContractAsync abi
            [| owner |]
        |> runNow

    ContractPlug(ethConn, abi, deployTxReceipt.ContractAddress)

let ethUsdOracle = "0xD45727E3D7405C6Ab3B2b3A57474012e1f998483"

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

[<Specification("Oracle", "getEthDaiPrice", 0)>]
[<Property>]
let ``price is correct given source prices within ten percents of one another`` () = 
    let contract = makeOracle makerOracle daiUsdOracle ethUsdOracle

    let price = contract.Query<bigint> "getEthDaiPrice" [||]
    price > bigint 0

// let ``state after solidity function call equals to the state after fsharp function call changeGulper`` a = 
//     changeGulper a = 