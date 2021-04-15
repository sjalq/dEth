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
    let s = dEthTests.``dEth - giveCDPToDSProxy - can be called by owner`` ()
    0