namespace DETH2.Contracts.IFlipper.ContractDefinition

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

    
    
    type IFlipperDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = ""
        
        new() = IFlipperDeployment(BYTECODE)
        
    [<FunctionOutput>]
    type BidsOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "bid", 1)>]
            member val public Bid = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "lot", 2)>]
            member val public Lot = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "guy", 3)>]
            member val public Guy = Unchecked.defaultof<string> with get, set
            [<Parameter("uint48", "tic", 4)>]
            member val public Tic = Unchecked.defaultof<uint64> with get, set
            [<Parameter("uint48", "end", 5)>]
            member val public End = Unchecked.defaultof<uint64> with get, set
            [<Parameter("address", "usr", 6)>]
            member val public Usr = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "gal", 7)>]
            member val public Gal = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "tab", 8)>]
            member val public Tab = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("bids", typeof<BidsOutputDTO>)>]
    type BidsFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("file")>]
    type FileFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("bytes32", "what", 1)>]
            member val public What = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("uint256", "data", 2)>]
            member val public Data = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("tend")>]
    type TendFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "id", 1)>]
            member val public Id = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "lot", 2)>]
            member val public Lot = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "bid", 3)>]
            member val public Bid = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("tick")>]
    type TickFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "id", 1)>]
            member val public Id = Unchecked.defaultof<BigInteger> with get, set
        

        
    
    
    
    
    


