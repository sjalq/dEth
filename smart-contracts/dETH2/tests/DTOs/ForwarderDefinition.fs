namespace Foundry.Contracts.Forwarder.ContractDefinition

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

    
    
    type ForwarderDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = "0x608060405234801561001057600080fd5b5060405161078d38038061078d8339818101604052602081101561003357600080fd5b8101908080519060200190929190505050806000806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff160217905550506106f9806100946000396000f3fe608060405260043610610051576000357c0100000000000000000000000000000000000000000000000000000000900480638da5cb5b14610053578063a6f9dae1146100aa578063dc5fe025146100fb575b005b34801561005f57600080fd5b50610068610271565b604051808273ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200191505060405180910390f35b3480156100b657600080fd5b506100f9600480360360208110156100cd57600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610296565b005b34801561010757600080fd5b506101eb6004803603606081101561011e57600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff1690602001909291908035906020019064010000000081111561015b57600080fd5b82018360208201111561016d57600080fd5b8035906020019184600183028401116401000000008311171561018f57600080fd5b91908080601f016020809104026020016040519081016040528093929190818152602001838380828437600081840152601f19601f820116905080830192505050505050509192919290803590602001909291905050506103fe565b604051808315151515815260200180602001828103825283818151815260200191508051906020019080838360005b8381101561023557808201518184015260208101905061021a565b50505050905090810190601f1680156102625780820380516001836020036101000a031916815260200191505b50935050505060405180910390f35b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1681565b6000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff1614610358576040517f08c379a000000000000000000000000000000000000000000000000000000000815260040180806020018281038252600a8152602001807f6f6e6c79206f776e65720000000000000000000000000000000000000000000081525060200191505060405180910390fd5b806000806101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff1602179055507fa2ea9883a321a3e97b8266c2b078bfeec6d50c711ed71f874a90d500ae2eaf3681604051808273ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200191505060405180910390a150565b600060606000809054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff16146104c4576040517f08c379a000000000000000000000000000000000000000000000000000000000815260040180806020018281038252600a8152602001807f6f6e6c79206f776e65720000000000000000000000000000000000000000000081525060200191505060405180910390fd5b600060608673ffffffffffffffffffffffffffffffffffffffff1685876040518082805190602001908083835b6020831061051457805182526020820191506020810190506020830392506104f1565b6001836020036101000a03801982511681845116808217855250505050505090500191505060006040518083038185875af1925050503d8060008114610576576040519150601f19603f3d011682016040523d82523d6000602084013e61057b565b606091505b50915091508673ffffffffffffffffffffffffffffffffffffffff167f7b655daaeef97843e8691c590dde6d4925d0015d198c34570bb7b6f3e967f0558787858560405180806020018581526020018415151515815260200180602001838103835287818151815260200191508051906020019080838360005b838110156106105780820151818401526020810190506105f5565b50505050905090810190601f16801561063d5780820380516001836020036101000a031916815260200191505b50838103825284818151815260200191508051906020019080838360005b8381101561067657808201518184015260208101905061065b565b50505050905090810190601f1680156106a35780820380516001836020036101000a031916815260200191505b50965050505050505060405180910390a2818193509350505093509391505056fea265627a7a72315820588036bd71d0132076920f176eae67f038c8303281aceea61e7aa0d16fd3691064736f6c63430005100032"
        
        new() = ForwarderDeployment(BYTECODE)
        
            [<Parameter("address", "_owner", 1)>]
            member val public Owner = Unchecked.defaultof<string> with get, set
        
    
    [<Function("owner", "address")>]
    type OwnerFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("changeOwner")>]
    type ChangeOwnerFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "_newOwner", 1)>]
            member val public NewOwner = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type ForwardOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("bool", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<bool> with get, set
            [<Parameter("bytes", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<byte[]> with get, set

                
    [<Function("forward", typeof<ForwardOutputDTO>)>]
    type ForwardFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "_to", 1)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("bytes", "_data", 2)>]
            member val public Data = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("uint256", "_wei", 3)>]
            member val public Wei = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Event("Forwarded")>]
    type ForwardedEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "_to", 1, true )>]
            member val To = Unchecked.defaultof<string> with get, set
            [<Parameter("bytes", "_data", 2, false )>]
            member val Data = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("uint256", "_wei", 3, false )>]
            member val Wei = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("bool", "_success", 4, false )>]
            member val Success = Unchecked.defaultof<bool> with get, set
            [<Parameter("bytes", "_resultData", 5, false )>]
            member val ResultData = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Event("OwnerChanged")>]
    type OwnerChangedEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "_newOwner", 1, false )>]
            member val NewOwner = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type OwnerOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
    

