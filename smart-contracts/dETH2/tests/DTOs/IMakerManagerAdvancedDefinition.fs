namespace DETH2.Contracts.IMakerManagerAdvanced.ContractDefinition

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

    
    
    type IMakerManagerAdvancedDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = "608060405234801561001057600080fd5b506102b6806100206000396000f3fe608060405234801561001057600080fd5b50600436106100935760003560e01c806380c9419e1161006657806380c9419e1461012e5780638161b120146101645780639a816f7d14610181578063b3d178f2146101a7578063fc73d771146101af57610093565b806305d85eda146100985780632726b073146100d05780632c2cb9fd1461010957806336569e7714610126575b600080fd5b6100be600480360360208110156100ae57600080fd5b50356001600160a01b03166101d5565b60408051918252519081900360200190f35b6100ed600480360360208110156100e657600080fd5b50356101e7565b604080516001600160a01b039092168252519081900360200190f35b6100be6004803603602081101561011f57600080fd5b5035610202565b6100ed610214565b61014b6004803603602081101561014457600080fd5b5035610223565b6040805192835260208301919091528051918290030190f35b6100ed6004803603602081101561017a57600080fd5b503561023c565b6100be6004803603602081101561019757600080fd5b50356001600160a01b0316610257565b6100be610269565b6100be600480360360208110156101c557600080fd5b50356001600160a01b031661026f565b60086020526000908152604090205481565b6002602052600090815260409020546001600160a01b031681565b60056020526000908152604090205481565b6000546001600160a01b031681565b6003602052600090815260409020805460019091015482565b6004602052600090815260409020546001600160a01b031681565b60076020526000908152604090205481565b60015481565b6006602052600090815260409020548156fea265627a7a7231582074ea074e39b79e8a4e70b8ef1f353990d7764fb7a915470155b1dd61f78642a064736f6c63430005110032"
        
        new() = IMakerManagerAdvancedDeployment(BYTECODE)
        

    [<FunctionOutput>]
    type CdpiOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type CountOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type FirstOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type IlksOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("bytes32", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte[]> with get, set
        
    
    [<FunctionOutput>]
    type LastOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type ListOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "prev", 1)>]
            member val public Prev = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "next", 2)>]
            member val public Next = Unchecked.defaultof<BigInteger> with get, set
        
    
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
        
    
    [<Function("cdpi", "uint256")>]
    type CdpiFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("count", "uint256")>]
    type CountFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<Function("first", "uint256")>]
    type FirstFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<Function("ilks", "bytes32")>]
    type IlksFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("last", "uint256")>]
    type LastFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<Function("list", typeof<ListOutputDTO>)>]
    type ListFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("owns", "address")>]
    type OwnsFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("urns", "address")>]
    type UrnsFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("vat", "address")>]
    type VatFunction() = 
        inherit FunctionMessage()
    

        
    
    

