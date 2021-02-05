namespace Foundry.Contracts.Debug.ContractDefinition

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

type DebugDeployment(byteCode: string) =
    inherit ContractDeploymentMessage(byteCode)
    
    static let BYTECODE = "0x608060405234801561001057600080fd5b5061042c806100206000396000f3fe608060405260043610610046576000357c0100000000000000000000000000000000000000000000000000000000900480636fadcf721461004b578063adb61832146101aa575b600080fd5b6101246004803603604081101561006157600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff1690602001909291908035906020019064010000000081111561009e57600080fd5b8201836020820111156100b057600080fd5b803590602001918460018302840111640100000000831117156100d257600080fd5b91908080601f016020809104026020016040519081016040528093929190818152602001838380828437600081840152601f19601f8201169050808301925050505050505091929192905050506101d5565b604051808315151515815260200180602001828103825283818151815260200191508051906020019080838360005b8381101561016e578082015181840152602081019050610153565b50505050905090810190601f16801561019b5780820380516001836020036101000a031916815260200191505b50935050505060405180910390f35b3480156101b657600080fd5b506101bf6103ef565b6040518082815260200191505060405180910390f35b60006060600060608573ffffffffffffffffffffffffffffffffffffffff1634866040518082805190602001908083835b602083106102295780518252602082019150602081019050602083039250610206565b6001836020036101000a03801982511681845116808217855250505050505090500191505060006040518083038185875af1925050503d806000811461028b576040519150601f19603f3d011682016040523d82523d6000602084013e610290565b606091505b50915091508573ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff167f0c991d70033760b32e5748ac2414f25b432c49b28384ae56735d92f79468e1f68734868660405180806020018581526020018415151515815260200180602001838103835287818151815260200191508051906020019080838360005b8381101561033c578082015181840152602081019050610321565b50505050905090810190601f1680156103695780820380516001836020036101000a031916815260200191505b50838103825284818151815260200191508051906020019080838360005b838110156103a2578082015181840152602081019050610387565b50505050905090810190601f1680156103cf5780820380516001836020036101000a031916815260200191505b50965050505050505060405180910390a381819350935050509250929050565b60004290509056fea265627a7a723158204dc9cee7c7bb22d87d57d90ba9e0389b38a265de896ec3499628fb5c89b8c26364736f6c63430005100032"
    
    new() = DebugDeployment(BYTECODE)

    
[<Function("blockTimestamp", "uint256")>]
type BlockTimestampFunction() = 
    inherit FunctionMessage()


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
    

[<Event("Forwarded")>]
type ForwardedEventDTO() =
    inherit EventDTO()
        [<Parameter("address", "_msgSender", 1, true )>]
        member val MsgSender = Unchecked.defaultof<string> with get, set
        [<Parameter("address", "_to", 2, true )>]
        member val To = Unchecked.defaultof<string> with get, set
        [<Parameter("bytes", "_data", 3, false )>]
        member val Data = Unchecked.defaultof<byte[]> with get, set
        [<Parameter("uint256", "_wei", 4, false )>]
        member val Wei = Unchecked.defaultof<BigInteger> with get, set
        [<Parameter("bool", "_success", 5, false )>]
        member val Success = Unchecked.defaultof<bool> with get, set
        [<Parameter("bytes", "_resultData", 6, false )>]
        member val ResultData = Unchecked.defaultof<byte[]> with get, set
    

[<FunctionOutput>]
type BlockTimestampOutputDTO() =
    inherit FunctionOutputDTO() 
        [<Parameter("uint256", "", 1)>]
        member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
    

