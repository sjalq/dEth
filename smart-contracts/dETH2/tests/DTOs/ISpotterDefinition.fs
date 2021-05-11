namespace DETH2.Contracts.ISpotter.ContractDefinition

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

    
    
    type ISpotterDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = ""
        
        new() = ISpotterDeployment(BYTECODE)
        

    [<FunctionOutput>]
    type IlksOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "pip", 1)>]
            member val public Pip = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "mat", 2)>]
            member val public Mat = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("ilks", typeof<IlksOutputDTO>)>]
    type IlksFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("bytes32", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Function("poke")>]
    type PokeFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("bytes32", "ilk", 1)>]
            member val public Ilk = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Event("Poke")>]
    type PokeEventDTO() =
        inherit EventDTO()
            [<Parameter("bytes32", "ilk", 1, false )>]
            member val Ilk = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("bytes32", "val", 2, false )>]
            member val Val = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("uint256", "spot", 3, false )>]
            member val Spot = Unchecked.defaultof<BigInteger> with get, set
        
    
        
    


