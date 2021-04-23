namespace DETH2.Contracts.IMakerOracleValueMock.ContractDefinition

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

    
    
    type IMakerOracleValueMockDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = "608060405234801561001057600080fd5b5060b58061001f6000396000f3fe6080604052348015600f57600080fd5b506004361060325760003560e01c806359e02dd7146037578063da358a3c146056575b600080fd5b603d6072565b6040805192835290151560208301528051918290030190f35b607060048036036020811015606a57600080fd5b5035607b565b005b60005460019091565b60005556fea265627a7a72315820a81a12b440c381d084005df60b504ff4861b5edf6980485494f4726cbf4a781e64736f6c63430005110032"
        
        new() = IMakerOracleValueMockDeployment(BYTECODE)
        
    
    [<Function("setData")>]
    type SetDataFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("int256", "data", 1)>]
            member val public Data = Unchecked.defaultof<BigInteger> with get, set
        
    
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
    

        
    


