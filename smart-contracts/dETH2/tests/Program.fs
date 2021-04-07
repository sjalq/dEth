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
    let a = "6C13F5A7A5C1169EAC2435CF8B56AED42B8BBD35"
    let b = "027D9BBE9055ADF4068D903CF9AB2126F6FF1795"
    let c = "6B1DFF2D469E1384AC1D41DDDEF656A0AD36BD3F"
    let isTrue = dEthTests.``price is correct given source prices within ten percents of one another`` ()  
    0