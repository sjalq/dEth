namespace DETH2.Contracts.IMCDSaverProxy.ContractDefinition

open System.Numerics
open Nethereum.ABI.FunctionEncoding.Attributes
open Nethereum.Contracts
    
    
    type IMCDSaverProxyDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = ""
        
        new() = IMCDSaverProxyDeployment(BYTECODE)
        
    [<FunctionOutput>]
    type GetCdpDetailedInfoOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "collateral", 1)>]
            member val public Collateral = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "debt", 2)>]
            member val public Debt = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "price", 3)>]
            member val public Price = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("bytes32", "ilk", 4)>]
            member val public Ilk = Unchecked.defaultof<byte[]> with get, set        
    
    [<Function("getCdpDetailedInfo", typeof<GetCdpDetailedInfoOutputDTO>)>]
    type GetCdpDetailedInfoFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "_cdpId", 1)>]
            member val public CdpId = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("getRatio", "uint256")>]
    type GetRatioFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "_cdpId", 1)>]
            member val public CdpId = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("bytes32", "_ilk", 2)>]
            member val public Ilk = Unchecked.defaultof<byte[]> with get, set
                  
    
    [<FunctionOutput>]
    type GetRatioOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
    

