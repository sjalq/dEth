namespace DETH2.Contracts.VatLike.ContractDefinition

open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Numerics
open Nethereum.Hex.HexTypes
open Nethereum.ABI.FunctionEncoding.Attributes
open Nethereum.Web3
open Nethereum.RPC.Eth.DTOs
open Nethereum.Contracts.CQS
open Nethereum.Contracts
open System.Threading

    
    
    type VatLikeDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = ""
        
        new() = VatLikeDeployment(BYTECODE)
        
    [<FunctionOutput>]
    type IlksOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "Art", 1)>]
            member val public Art = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "rate", 2)>]
            member val public Rate = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "spot", 3)>]
            member val public Spot = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "line", 4)>]
            member val public Line = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "dust", 5)>]
            member val public Dust = Unchecked.defaultof<BigInteger> with get, set
        
    
    
    
    [<FunctionOutput>]
    type UrnsOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "ink", 1)>]
            member val public Ink = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "art", 2)>]
            member val public Art = Unchecked.defaultof<BigInteger> with get, set

        
    
    [<Function("grab")>]
    type GrabFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("bytes32", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("address", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "", 3)>]
            member val public ReturnValue3 = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "", 4)>]
            member val public ReturnValue4 = Unchecked.defaultof<string> with get, set
            [<Parameter("int256", "", 5)>]
            member val public ReturnValue5 = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("int256", "", 6)>]
            member val public ReturnValue6 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("hope")>]
    type HopeFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<Function("ilks", typeof<IlksOutputDTO>)>]
    type IlksFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("bytes32", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Function("nope")>]
    type NopeFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<Function("urns", typeof<UrnsOutputDTO>)>]
    type UrnsFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("bytes32", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("address", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<string> with get, set