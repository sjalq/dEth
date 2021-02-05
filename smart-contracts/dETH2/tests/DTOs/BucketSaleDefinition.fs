namespace Foundry.Contracts.BucketSale.ContractDefinition

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

    
    
    type BucketSaleDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = "0x60806040526040518060e0016040528060be81526020016200198b60be91396000908051906020019062000035929190620001a2565b503480156200004357600080fd5b5060405162001a4938038062001a49833981810160405260e08110156200006957600080fd5b810190808051906020019092919080519060200190929190805190602001909291908051906020019092919080519060200190929190805190602001909291908051906020019092919050505086600460006101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff1602179055508560058190555084600681905550836007819055508260088190555081600a60006101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff16021790555080600b60006101000a81548173ffffffffffffffffffffffffffffffffffffffff021916908373ffffffffffffffffffffffffffffffffffffffff1602179055505050505050505062000251565b828054600181600116156101000203166002900490600052602060002090601f016020900481019282601f10620001e557805160ff191683800117855562000216565b8280016001018555821562000216579182015b8281111562000215578251825591602001919060010190620001f8565b5b50905062000225919062000229565b5090565b6200024e91905b808211156200024a57600081600090555060010162000230565b5090565b90565b61172a80620002616000396000f3fe608060405234801561001057600080fd5b5060043610610133576000357c01000000000000000000000000000000000000000000000000000000009004806397b2fd45116100bf578063c2f5673e1161008e578063c2f5673e14610478578063c4aaeb1a14610496578063cff40759146104b4578063f678462f14610502578063f838b8251461058557610133565b806397b2fd45146103485780639b51fb0d146103a0578063a03effd1146103e2578063a393718c1461040057610133565b80634f127aae116101065780634f127aae1461022057806360e6a4401461023e57806361d027b31461028857806389ce96c6146102d25780639361265f146102f057610133565b80633261933d1461013857806342f2b6ec1461015657806347e3baaa146101b85780634b42442e14610202575b600080fd5b6101406105ee565b6040518082815260200191505060405180910390f35b6101a26004803603604081101561016c57600080fd5b8101908080359060200190929190803573ffffffffffffffffffffffffffffffffffffffff16906020019092919050505061060b565b6040518082815260200191505060405180910390f35b6101c06106b0565b604051808273ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200191505060405180910390f35b61020a6106d6565b6040518082815260200191505060405180910390f35b6102286106dc565b6040518082815260200191505060405180910390f35b61024661070c565b604051808273ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200191505060405180910390f35b610290610732565b604051808273ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200191505060405180910390f35b6102da610758565b6040518082815260200191505060405180910390f35b6103326004803603602081101561030657600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff16906020019092919050505061075e565b6040518082815260200191505060405180910390f35b61038a6004803603602081101561035e57600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610844565b6040518082815260200191505060405180910390f35b6103cc600480360360208110156103b657600080fd5b810190808035906020019092919050505061085c565b6040518082815260200191505060405180910390f35b6103ea61087a565b6040518082815260200191505060405180910390f35b6104766004803603608081101561041657600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff1690602001909291908035906020019092919080359060200190929190803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610880565b005b610480610d37565b6040518082815260200191505060405180910390f35b61049e610d3d565b6040518082815260200191505060405180910390f35b610500600480360360408110156104ca57600080fd5b8101908080359060200190929190803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610d43565b005b61050a6110f6565b6040518080602001828103825283818151815260200191508051906020019080838360005b8381101561054a57808201518184015260208101905061052f565b50505050905090810190601f1680156105775780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b6105d16004803603604081101561059b57600080fd5b8101908080359060200190929190803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050611194565b604051808381526020018281526020019250505060405180910390f35b6000610606600a6103e86111c590919063ffffffff16565b905090565b60008060026000858152602001908152602001600020905060006001600086815260200190815260200160002060008573ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002090506106a6826000015461069883600001546007546111c590919063ffffffff16565b61124b90919063ffffffff16565b9250505092915050565b600b60009054906101000a900473ffffffffffffffffffffffffffffffffffffffff1681565b60095481565b60006107076006546106f96005544261129590919063ffffffff16565b61124b90919063ffffffff16565b905090565b600a60009054906101000a900473ffffffffffffffffffffffffffffffffffffffff1681565b600460009054906101000a900473ffffffffffffffffffffffffffffffffffffffff1681565b60075481565b60008073ffffffffffffffffffffffffffffffffffffffff168273ffffffffffffffffffffffffffffffffffffffff16141561079d576000905061083f565b60006107f9670de0b6b3a7640000600360008673ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000205461124b90919063ffffffff16565b90506000610825610816600a6103e86111c590919063ffffffff16565b836112df90919063ffffffff16565b90506000610836620186a083611367565b90508093505050505b919050565b60036020528060005260406000206000915090505481565b60026020528060005260406000206000915090508060000154905081565b60055481565b61088b838584611380565b6108dd82600360008473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020546112df90919063ffffffff16565b600360008373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020819055506000600b60009054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff166323b872dd33600460009054906101000a900473ffffffffffffffffffffffffffffffffffffffff16866040518463ffffffff167c0100000000000000000000000000000000000000000000000000000000028152600401808473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020018373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020018281526020019350505050602060405180830381600087803b158015610a3d57600080fd5b505af1158015610a51573d6000803e3d6000fd5b505050506040513d6020811015610a6757600080fd5b8101908080519060200190929190505050905080610aed576040517f08c379a00000000000000000000000000000000000000000000000000000000081526004018080602001828103825260158152602001807f656e746572207472616e73666572206661696c6564000000000000000000000081525060200191505060405180910390fd5b600073ffffffffffffffffffffffffffffffffffffffff168273ffffffffffffffffffffffffffffffffffffffff1614610c7c576000610b52620186a0610b44610b356105ee565b876111c590919063ffffffff16565b61124b90919063ffffffff16565b90506000610b86620186a0610b78610b698761075e565b886111c590919063ffffffff16565b61124b90919063ffffffff16565b9050610ba6610b9f6001886112df90919063ffffffff16565b8884611380565b610bc4610bbd6001886112df90919063ffffffff16565b8583611380565b8373ffffffffffffffffffffffffffffffffffffffff168773ffffffffffffffffffffffffffffffffffffffff167f161e0456cd3270520befa83f5fdd74084ad38cd70a096dcf24ccf7edc368b04f3389898787604051808673ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020018581526020018481526020018381526020018281526020019550505050505060405180910390a35050610d30565b600073ffffffffffffffffffffffffffffffffffffffff168573ffffffffffffffffffffffffffffffffffffffff167f161e0456cd3270520befa83f5fdd74084ad38cd70a096dcf24ccf7edc368b04f338787600080604051808673ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020018581526020018481526020018381526020018281526020019550505050505060405180910390a35b5050505050565b60085481565b60065481565b610d4b6106dc565b8210610da2576040517f08c379a00000000000000000000000000000000000000000000000000000000081526004018080602001828103825260248152602001806116d26024913960400191505060405180910390fd5b60006001600084815260200190815260200160002060008373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002090506000816000015411610e70576040517f08c379a000000000000000000000000000000000000000000000000000000000815260040180806020018281038252601e8152602001807f63616e2774206578697420696620796f75206469646e277420656e746572000081525060200191505060405180910390fd5b6000816001015414610eea576040517f08c379a000000000000000000000000000000000000000000000000000000000815260040180806020018281038252600e8152602001807f616c72656164792065786974656400000000000000000000000000000000000081525060200191505060405180910390fd5b610ef4838361060b565b8160010181905550610f1581600101546009546112df90919063ffffffff16565b6009819055506000600a60009054906101000a900473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff166340c10f198484600101546040518363ffffffff167c0100000000000000000000000000000000000000000000000000000000028152600401808373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200182815260200192505050602060405180830381600087803b158015610fe657600080fd5b505af1158015610ffa573d6000803e3d6000fd5b505050506040513d602081101561101057600080fd5b8101908080519060200190929190505050905080611096576040517f08c379a00000000000000000000000000000000000000000000000000000000081526004018080602001828103825260198152602001807f65786974206d696e742f7472616e73666572206661696c65640000000000000081525060200191505060405180910390fd5b8273ffffffffffffffffffffffffffffffffffffffff167f0808b45a422e0acd47a625c74fff3eb8d6d4dd063e0845deb1e57581c27b32f5858460010154604051808381526020018281526020019250505060405180910390a250505050565b60008054600181600116156101000203166002900480601f01602080910402602001604051908101604052809291908181526020018280546001816001161561010002031660029004801561118c5780601f106111615761010080835404028352916020019161118c565b820191906000526020600020905b81548152906001019060200180831161116f57829003601f168201915b505050505081565b6001602052816000526040600020602052806000526040600020600091509150508060000154908060010154905082565b6000808314156111d85760009050611245565b60008284029050828482816111e957fe5b0414611240576040517f08c379a000000000000000000000000000000000000000000000000000000000815260040180806020018281038252602181526020018061168e6021913960400191505060405180910390fd5b809150505b92915050565b600061128d83836040518060400160405280601a81526020017f536166654d6174683a206469766973696f6e206279207a65726f000000000000815250611507565b905092915050565b60006112d783836040518060400160405280601e81526020017f536166654d6174683a207375627472616374696f6e206f766572666c6f7700008152506115cd565b905092915050565b60008082840190508381101561135d576040517f08c379a000000000000000000000000000000000000000000000000000000000815260040180806020018281038252601b8152602001807f536166654d6174683a206164646974696f6e206f766572666c6f77000000000081525060200191505060405180910390fd5b8091505092915050565b60008183106113765781611378565b825b905092915050565b6113886106dc565b8310156113fd576040517f08c379a00000000000000000000000000000000000000000000000000000000081526004018080602001828103825260198152602001807f63616e6e6f7420656e7465722070617374206275636b6574730000000000000081525060200191505060405180910390fd5b6008548310611457576040517f08c379a00000000000000000000000000000000000000000000000000000000081526004018080602001828103825260238152602001806116af6023913960400191505060405180910390fd5b60006001600085815260200190815260200160002060008473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002090506114c28282600001546112df90919063ffffffff16565b816000018190555060006002600086815260200190815260200160002090506114f88382600001546112df90919063ffffffff16565b81600001819055505050505050565b600080831182906115b3576040517f08c379a00000000000000000000000000000000000000000000000000000000081526004018080602001828103825283818151815260200191508051906020019080838360005b8381101561157857808201518184015260208101905061155d565b50505050905090810190601f1680156115a55780820380516001836020036101000a031916815260200191505b509250505060405180910390fd5b5060008385816115bf57fe5b049050809150509392505050565b600083831115829061167a576040517f08c379a00000000000000000000000000000000000000000000000000000000081526004018080602001828103825283818151815260200191508051906020019080838360005b8381101561163f578082015181840152602081019050611624565b50505050905090810190601f16801561166c5780820380516001836020036101000a031916815260200191505b509250505060405180910390fd5b506000838503905080915050939250505056fe536166654d6174683a206d756c7469706c69636174696f6e206f766572666c6f77696e76616c6964206275636b65742069642d2d7061737420656e64206f662073616c6563616e206f6e6c7920657869742066726f6d20636f6e636c75646564206275636b657473a265627a7a723158206a7282e0c6a29a0053bf395b99c8c5ecb0b716cb5c1bbe7ebae9c7541800961564736f6c63430005100032427920696e746572616374696e672077697468207468697320636f6e74726163742c204920636f6e6669726d204920616d206e6f74206120555320636974697a656e206f72206120636974697a656e206f66207468652050656f706c6527732052657075626c6963206f66204368696e612e204920616772656520746f20626520626f756e6420627920746865207465726d7320666f756e642061742068747470733a2f2f666f756e64727964616f2e636f6d2f73616c652f7465726d73"
        
        new() = BucketSaleDeployment(BYTECODE)
        
            [<Parameter("address", "_treasury", 1)>]
            member val public Treasury = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "_startOfSale", 2)>]
            member val public StartOfSale = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "_bucketPeriod", 3)>]
            member val public BucketPeriod = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "_bucketSupply", 4)>]
            member val public BucketSupply = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "_bucketCount", 5)>]
            member val public BucketCount = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "_tokenOnSale", 6)>]
            member val public TokenOnSale = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "_tokenSoldFor", 7)>]
            member val public TokenSoldFor = Unchecked.defaultof<string> with get, set
        
    
    [<Function("bucketCount", "uint256")>]
    type BucketCountFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("bucketPeriod", "uint256")>]
    type BucketPeriodFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("bucketSupply", "uint256")>]
    type BucketSupplyFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("buckets", "uint256")>]
    type BucketsFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set


    [<FunctionOutput>]
    type BuysOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "valueEntered", 1)>]
            member val public ValueEntered = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "buyerTokensExited", 2)>]
            member val public BuyerTokensExited = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("buys", typeof<BuysOutputDTO>)>]
    type BuysFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "", 2)>]
            member val public ReturnValue2 = Unchecked.defaultof<string> with get, set
        
    
    [<Function("referredTotal", "uint256")>]
    type ReferredTotalFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<Function("startOfSale", "uint256")>]
    type StartOfSaleFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("termsAndConditions", "string")>]
    type TermsAndConditionsFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("tokenOnSale", "address")>]
    type TokenOnSaleFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("tokenSoldFor", "address")>]
    type TokenSoldForFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("totalExitedTokens", "uint256")>]
    type TotalExitedTokensFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("treasury", "address")>]
    type TreasuryFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("currentBucket", "uint256")>]
    type CurrentBucketFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("agreeToTermsAndConditionsListedInThisContractAndEnterSale")>]
    type AgreeToTermsAndConditionsListedInThisContractAndEnterSaleFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "_buyer", 1)>]
            member val public Buyer = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "_bucketId", 2)>]
            member val public BucketId = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "_amount", 3)>]
            member val public Amount = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "_referrer", 4)>]
            member val public Referrer = Unchecked.defaultof<string> with get, set
        
    
    [<Function("exit")>]
    type ExitFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "_bucketId", 1)>]
            member val public BucketId = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "_buyer", 2)>]
            member val public Buyer = Unchecked.defaultof<string> with get, set
        
    
    [<Function("buyerReferralRewardPerc", "uint256")>]
    type BuyerReferralRewardPercFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("referrerReferralRewardPerc", "uint256")>]
    type ReferrerReferralRewardPercFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "_referrerAddress", 1)>]
            member val public ReferrerAddress = Unchecked.defaultof<string> with get, set
        
    
    [<Function("calculateExitableTokens", "uint256")>]
    type CalculateExitableTokensFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "_bucketId", 1)>]
            member val public BucketId = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "_buyer", 2)>]
            member val public Buyer = Unchecked.defaultof<string> with get, set
        
    
    [<Event("Entered")>]
    type EnteredEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "_sender", 1, false )>]
            member val Sender = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "_bucketId", 2, false )>]
            member val BucketId = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "_buyer", 3, true )>]
            member val Buyer = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "_valueEntered", 4, false )>]
            member val ValueEntered = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "_buyerReferralReward", 5, false )>]
            member val BuyerReferralReward = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "_referrer", 6, true )>]
            member val Referrer = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "_referrerReferralReward", 7, false )>]
            member val ReferrerReferralReward = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Event("Exited")>]
    type ExitedEventDTO() =
        inherit EventDTO()
            [<Parameter("uint256", "_bucketId", 1, false )>]
            member val BucketId = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "_buyer", 2, true )>]
            member val Buyer = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "_tokensExited", 3, false )>]
            member val TokensExited = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type BucketCountOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type BucketPeriodOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type BucketSupplyOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type BucketsOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "totalValueEntered", 1)>]
            member val public TotalValueEntered = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type ReferredTotalOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type StartOfSaleOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type TermsAndConditionsOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("string", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type TokenOnSaleOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type TokenSoldForOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type TotalExitedTokensOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type TreasuryOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type CurrentBucketOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    
    
    
    
    [<FunctionOutput>]
    type BuyerReferralRewardPercOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type ReferrerReferralRewardPercOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type CalculateExitableTokensOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
    

