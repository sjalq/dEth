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
        
    [<Event("LogNote")>]
    type LogNoteEventDTO() =
        inherit EventDTO()
            [<Parameter("bytes4", "sig", 1, true )>]
            member val Sig = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("address", "guy", 2, true )>]
            member val Guy = Unchecked.defaultof<string> with get, set
            [<Parameter("bytes32", "foo", 3, true )>]
            member val Foo = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("bytes32", "bar", 4, true )>]
            member val Bar = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("uint256", "wad", 5, false )>]
            member val Wad = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("bytes", "fax", 6, false )>]
            member val Fax = Unchecked.defaultof<byte[]> with get, set
        
    
    [<FunctionOutput>]
    type PeekOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("bytes32", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("bool", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<bool> with get, set
        
        
    [<Function("peek", typeof<PeekOutputDTO>)>]
    type PeekFunction() = 
        inherit FunctionMessage()
    

    [<Function("change")>]
    type ChangeFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "src_", 1)>]
            member val public Src_ = Unchecked.defaultof<string> with get, set
        
    
    [<Function("kiss")>]
    type KissFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "a", 1)>]
            member val public A = Unchecked.defaultof<string> with get, set
        

        
    
    [<Function("rely")>]
    type RelyFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "usr", 1)>]
            member val public Usr = Unchecked.defaultof<string> with get, set
        

        
    


