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

[<Specification("Oracle", "constructor", 0)>]
[<Property( Arbitrary = [|typeof<MyGenerators>|], QuietOnSuccess = true, MaxTest = 20 )>]
let ``inits to provided parameters`` (makerOracle:string) (daiUsdOracle:string) (ethUsdOracle:string) = 
    let abi = Abi("../../../../build/contracts/Oracle.json")
    let tx = ethConn.DeployContractAsync abi [| makerOracle;daiUsdOracle;ethUsdOracle |] |> runNow
    let contract = ContractPlug(ethConn, abi, tx.ContractAddress)

    //printfn "Overriden: %A; New instances: %A" res.Overr4iddenInstances res.NewInstances

    let makerOracleC = contract.Query<string> "makerOracle" [||]
    let daiOracleC = contract.Query<string> "daiUsdOracle" [||]

    ("0x" + makerOracle).icompare (contract.Query<string> "makerOracle" [||]) &&
    ("0x" + daiUsdOracle).icompare (contract.Query<string> "daiUsdOracle" [||]) &&
    ("0x" + ethUsdOracle).icompare (contract.Query<string> "ethUsdOracle" [||])


// let ``state after solidity function call equals to the state after fsharp function call changeGulper`` a = 
//     changeGulper a = 