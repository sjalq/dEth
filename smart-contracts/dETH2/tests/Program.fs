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
    let d = "0x15d34aaf54267db7d7c367839aaf71a00a2c6a65"
    let e = "0x70997970c51812dc3a010c7d01b50e0d17dc79c8"
    let g = "0x90f79bf6eb2c4f870365e785982e1f101e93b906"
    let h = "0x14dc79964da2c08b23698b3d3cc7ca32193d9955"
    let i = "0x23618e81e3f5cdf7f54c3d65f7fbc0abf5b21e8f"

    let isTrue = dEthTests.``initializes with correct values and rights assigned`` a b (bigint 1) c d e g h
    0