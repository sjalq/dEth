namespace DEth.Contracts.IPriceFeed.ContractDefinition

open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Numerics
open Nethereum.Hex.HexTypes
open Nethereum.ABI.FunctionEncoding.Attributes
open Nethereum.Contracts

    
    
    type IPriceFeedDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = ""
        
        new() = IPriceFeedDeployment(BYTECODE)
        

        
    
    [<Function("post")>]
    type PostFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint128", "val_", 1)>]
            member val public Val_ = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint32", "zzz_", 2)>]
            member val public Zzz_ = Unchecked.defaultof<uint> with get, set
            [<Parameter("address", "med_", 3)>]
            member val public Med_ = Unchecked.defaultof<string> with get, set
        
    


