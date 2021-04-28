namespace DETH2.Contracts.PipLike.ContractDefinition

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

    
    
    type PipLikeDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = ""
        
        new() = PipLikeDeployment(BYTECODE)
        
    [<FunctionOutput>]
    type PeekOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("bytes32", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("bool", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<bool> with get, set
        
    
    [<Function("change")>]
    type ChangeFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "src_", 1)>]
            member val public Src_ = Unchecked.defaultof<string> with get, set
        
    
    [<Function("peek", typeof<PeekOutputDTO>)>]
    type PeekFunction() = 
        inherit FunctionMessage()
    

        
    
    
    
    

