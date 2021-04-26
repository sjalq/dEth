namespace DEth.Contracts.DSValueMock.ContractDefinition

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

    
    
    type DSValueMockDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = "608060405234801561001057600080fd5b5060ed8061001f6000396000f3fe6080604052348015600f57600080fd5b506004361060325760003560e01c806359e02dd7146037578063812d3983146056575b600080fd5b603d607b565b6040805192835290151560208301528051918290030190f35b607960048036036020811015606a57600080fd5b50356001600160801b0316608d565b005b6000546001600160801b031660019091565b600080546fffffffffffffffffffffffffffffffff19166001600160801b039290921691909117905556fea265627a7a723158204da940d2ae8a2f05110c6a8ad9f53adefca782282f85523a055eb9946ee679af64736f6c63430005110032"
        
        new() = DSValueMockDeployment(BYTECODE)
        

    [<FunctionOutput>]
    type PeekOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("bytes32", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("bool", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<bool> with get, set
        
    
    [<Function("peek", typeof<PeekOutputDTO>)>]
    type PeekFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("setData")>]
    type SetDataFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint128", "_val", 1)>]
            member val public Val = Unchecked.defaultof<BigInteger> with get, set
        
    
        
    


