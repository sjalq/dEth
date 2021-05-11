namespace DETH2.Contracts.ManagerLike.ContractDefinition

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

    
    
    type ManagerLikeDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = ""
        
        new() = ManagerLikeDeployment(BYTECODE)
        

        
    
    [<Function("cdpAllow")>]
    type CdpAllowFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "", 3)>]
            member val public ReturnValue3 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("cdpCan", "uint256")>]
    type CdpCanFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "", 1)>]
            member val public dp = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "", 3)>]
            member val public ReturnValue3 = Unchecked.defaultof<string> with get, set
        
    
    [<Function("enter")>]
    type EnterFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("exit")>]
    type ExitFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "", 3)>]
            member val public ReturnValue3 = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "", 4)>]
            member val public ReturnValue4 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("flux")>]
    type FluxFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "", 3)>]
            member val public ReturnValue3 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("frob")>]
    type FrobFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("int256", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("int256", "", 3)>]
            member val public ReturnValue3 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("give")>]
    type GiveFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public Cdp = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "", 2)>]
            member val public Dst = Unchecked.defaultof<string> with get, set
        
    
    [<Function("ilks", "bytes32")>]
    type IlksFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("move")>]
    type MoveFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "", 3)>]
            member val public ReturnValue3 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("open", "uint256")>]
    type OpenFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("bytes32", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("address", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<string> with get, set
        
    
    [<Function("owns", "address")>]
    type OwnsFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("quit")>]
    type QuitFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<string> with get, set
        
    
    [<Function("shift")>]
    type ShiftFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("urnAllow")>]
    type UrnAllowFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("urns", "address")>]
    type UrnsFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("vat", "address")>]
    type VatFunction() = 
        inherit FunctionMessage()
    

        
    
    
    
    [<FunctionOutput>]
    type CdpCanOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    
    
    
    
    
    
    
    
    
    
    [<FunctionOutput>]
    type IlksOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("bytes32", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte[]> with get, set
        
    
    
    
    
    
    [<FunctionOutput>]
    type OwnsOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    
    
    
    
    
    
    [<FunctionOutput>]
    type UrnsOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type VatOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
    

