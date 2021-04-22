namespace DEth.Contracts.IMedianETHUSD.ContractDefinition

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

    
    
    type IMedianETHUSDDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = ""
        
        new() = IMedianETHUSDDeployment(BYTECODE)
        

        
    
    [<Function("poke")>]
    type PokeFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256[]", "val_", 1)>]
            member val public Val_ = Unchecked.defaultof<List<BigInteger>> with get, set
            [<Parameter("uint256[]", "age_", 2)>]
            member val public Age_ = Unchecked.defaultof<List<BigInteger>> with get, set
            [<Parameter("uint8[]", "v", 3)>]
            member val public V = Unchecked.defaultof<List<byte>> with get, set
            [<Parameter("bytes32[]", "r", 4)>]
            member val public R = Unchecked.defaultof<List<byte[]>> with get, set
            [<Parameter("bytes32[]", "s", 5)>]
            member val public S = Unchecked.defaultof<List<byte[]>> with get, set
        
    


