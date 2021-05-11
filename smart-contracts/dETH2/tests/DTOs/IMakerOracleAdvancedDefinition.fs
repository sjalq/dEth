namespace DEth.Contracts.IMakerOracleAdvanced.ContractDefinition

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

    
    
    type IMakerOracleAdvancedDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = ""
        
        new() = IMakerOracleAdvancedDeployment(BYTECODE)
        

        
    
    [<Function("poke")>]
    type PokeFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("set")>]
    type SetFunction2() =
        inherit FunctionMessage()
    
            [<Parameter("address", "wat", 1)>]
            member val public Wat = Unchecked.defaultof<string> with get, set
        
    
    [<Function("set")>]
    type SetFunction() =
        inherit FunctionMessage()
    
            [<Parameter("bytes12", "pos", 1)>]
            member val public Pos = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("address", "wat", 2)>]
            member val public Wat = Unchecked.defaultof<string> with get, set
        
    
    [<Function("setMin")>]
    type SetMinFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint96", "min_", 1)>]
            member val public Min_ = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("setNext")>]
    type SetNextFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("bytes12", "next_", 1)>]
            member val public Next_ = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Function("unset")>]
    type UnsetFunction2() =
        inherit FunctionMessage()
    
            [<Parameter("address", "wat", 1)>]
            member val public Wat = Unchecked.defaultof<string> with get, set
        
    
    [<Function("unset")>]
    type UnsetFunction() =
        inherit FunctionMessage()
    
            [<Parameter("bytes12", "pos", 1)>]
            member val public Pos = Unchecked.defaultof<byte[]> with get, set
        
    
    
    
    
    
    
    
    
    
    
    
    
    


