module Program

open Microsoft.FSharp.Reflection
open System
open FsUnit.Xunit

open Newtonsoft.Json
open System.IO
open Newtonsoft.Json.Linq
open System.Linq

open TestBase
open System.Numerics
open Constants

open Nethereum.Contracts

open Foundry.Contracts.BucketSale.ContractDefinition

[<EntryPoint>]
let main _ =
    //EnterTests.``E6 - Can enter a bucket with no referrer``()

    Console.ReadLine() |> ignore
    0