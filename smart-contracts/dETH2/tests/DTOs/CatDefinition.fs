namespace DETH2.Contracts.Cat.ContractDefinition

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

    
    
    type CatDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = ""
        
        new() = CatDeployment(BYTECODE)
        

        
    
    [<Function("bite", "uint256")>]
    type BiteFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("bytes32", "ilk", 1)>]
            member val public Ilk = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("address", "urn", 2)>]
            member val public Urn = Unchecked.defaultof<string> with get, set
        
    
    [<Event("Bite")>]
    type BiteEventDTO() =
        inherit EventDTO()
            [<Parameter("bytes32", "ilk", 1, true )>]
            member val Ilk = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("address", "urn", 2, true )>]
            member val Urn = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "ink", 3, false )>]
            member val Ink = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "art", 4, false )>]
            member val Art = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "tab", 5, false )>]
            member val Tab = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "flip", 6, false )>]
            member val Flip = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "id", 7, false )>]
            member val Id = Unchecked.defaultof<BigInteger> with get, set
        
    


