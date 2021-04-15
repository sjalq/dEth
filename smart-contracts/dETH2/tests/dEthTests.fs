module dEthTests

open TestBase
open Xunit
open FsUnit.Xunit
open Nethereum.Web3
open System.Numerics
open dEthTestsBase
open Nethereum.Hex.HexConvertors.Extensions
open Nethereum.Web3.Accounts
open Nethereum.ABI.FunctionEncoding.Attributes
open Nethereum.JsonRpc.Client
open Nethereum.RPC.Eth.DTOs

type System.String with
   member s1.icompare(s2: string) =
     System.String.Equals(s1, s2, System.StringComparison.CurrentCultureIgnoreCase);

[<FunctionOutput>]
type GetCollateralOutputDTO() =
    inherit FunctionOutputDTO() 
        [<Parameter("uint256", "_priceRAY", 1)>]
        member val public PriceRAY = Unchecked.defaultof<BigInteger> with get, set
        [<Parameter("uint256", "_totalCollateral", 2)>]
        member val public TotalCollateral = Unchecked.defaultof<BigInteger> with get, set
        [<Parameter("uint256", "_debt", 3)>]
        member val public Debt = Unchecked.defaultof<BigInteger> with get, set
        [<Parameter("uint256", "_collateralDenominatedDebt", 4)>]
        member val public CollateralDenominatedDebt = Unchecked.defaultof<BigInteger> with get, set
        [<Parameter("uint256", "_excessCollateral", 5)>]
        member val public ExcessCollateral = Unchecked.defaultof<BigInteger> with get, set

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

[<Specification("Oracle", "constructor", 0)>]
[<Fact>]
let ``inits to provided parameters`` () =
    let makerOracle = makeAccount().Address
    let daiUsdOracle = makeAccount().Address
    let ethUsdOracle = makeAccount().Address   
    let contract = makeOracle makerOracle daiUsdOracle ethUsdOracle

    shouldEqualIgnoringCase makerOracle (contract.Query<string> "makerOracle" [||])
    shouldEqualIgnoringCase daiUsdOracle (contract.Query<string> "daiUsdOracle" [||])
    shouldEqualIgnoringCase ethUsdOracle (contract.Query<string> "ethUsdOracle" [||])

[<Specification("Oracle", "getEthDaiPrice", 0)>]
[<Theory>]
[<InlineData(0.08)>]
[<InlineData(0.1)>]
[<InlineData(0.12)>]
let ``price is correct given source prices within ten percents of one another`` differencePercent =
    let (priceMaker, priceDaiUsd, priceNonMakerDaiEth, priceEthUsd) = initOraclesDefault differencePercent

    let a = makerOracle.Query "readUint" [||]
    let b = daiUsdOracle.Query "latestRoundDataValue" [||]
    let c = ethUsdOracle.Query "latestRoundDataValue" [||]

    printfn "a:%A b:%A c:%A" a b c

    let price = oracleContract.Query<bigint> "getEthDaiPrice" [||]

    printfn "res: %A" price

    let expected = if differencePercent <= 0.1M then toMakerPriceFormatDecimal priceNonMakerDaiEth else toMakerPriceFormatDecimal priceMaker

    should equal expected price

[<Specification("dEth", "constructor", 0)>]
[<Fact>]
let ``initializes with correct values and rights assigned`` () = 
    let (gulper, proxyCache, cdpId, makerManager, ethGemJoin, saverProxy, saverProxyActions, oracleContract, initialRecipient, _, foundryTreasury, contract) = getDEthContract()
       
    // check the rights
    let authorityAddress = contract.Query<string> "authority" [||]
    let authority = ContractPlug(ethConn, Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/DSAuthority.json"), authorityAddress)
    let functionName = Web3.Sha3("automate(uint256,uint256,uint256,uint256,uint256)").Substring(0, 8).HexToByteArray()
    let canCall = authority.Query<bool> "canCall" [|foundryTreasury; contract.Address; functionName |]

    // check the balance of initialRecipient
    let balanceOfInitialRecipient = contract.Query<bigint> "balanceOf" [|initialRecipient|]

    shouldEqualIgnoringCase gulper (contract.Query<string> "gulper" [||])
    shouldEqualIgnoringCase proxyCache (contract.Query<string> "cache" [||])
    should equal cdpId (contract.Query<bigint> "cdpId" [||])
    shouldEqualIgnoringCase makerManager (contract.Query<string> "makerManager" [||])
    shouldEqualIgnoringCase ethGemJoin (contract.Query<string> "ethGemJoin" [||])
    shouldEqualIgnoringCase saverProxy (contract.Query<string> "saverProxy" [||])
    shouldEqualIgnoringCase saverProxyActions (contract.Query<string> "saverProxyActions" [||])
    shouldEqualIgnoringCase oracleContract.Address (contract.Query<string> "oracle" [||])
    should equal true canCall
    should greaterThan BigInteger.Zero balanceOfInitialRecipient

[<Specification("dEth", "changeGulper", 0)>]
[<Fact>]
let ``can be changed by owner`` () =
    let (_, _, _, _, _, _, _, _, _, _, _, contract) = getDEthContract ()
    let randomAddress = makeAccount().Address
    contract.ExecuteFunction "changeGulper" [|randomAddress|] |> ignore
    shouldEqualIgnoringCase randomAddress (contract.Query<string> "gulper" [||])

[<Specification("dEth", "changeGulper", 1)>]
[<Fact>]
let ``cannot be changed by non-owner`` () = 
    let (_, _, _, _, _, _, _, _, _, _, _, contract) = getDEthContract ()
    let account = Account(hardhatPrivKey2)
    let oldGulper = contract.Query<string> "gulper" [||]
    try
        contract.ExecuteFunctionFrom "changeGulper" [|account.Address|] (EthereumConnection(hardhatURI, account.PrivateKey)) |> ignore
        should equal oldGulper (contract.Query<string> "gulper" [||])
    with
    | a ->
        match a.InnerException with
        | :? RpcResponseException -> ()
        | a -> raise a

let giveCDPToDSProxyF privKey shouldThrow = 
    let (_, _, _, _, _, _, _, _, _, _, _, newContract) = getDEthContract ()
    
    let dEthMainnetOwner = "0xb7c6bb064620270f8c1daa7502bcca75fc074cf4"
    let dEthMainnet = "0x5420dFecFaCcDAE68b406ce96079d37743Aa11Ae"

    ethConn.Web3.Client.SendRequestAsync(new RpcRequest(1, "hardhat_impersonateAccount", dEthMainnetOwner))
        |> Async.AwaitTask |> Async.RunSynchronously

    let addressWithout0x = newContract.Address.Remove(0, 2)
    let bytesToPad = (32 - addressWithout0x.Length / 2)
    let paddedAddress = (Array.replicate (bytesToPad * 2) '0' |> System.String) + addressWithout0x
    let data = Web3.Sha3("giveCDPToDSProxy(address)").Substring(0, 8) + paddedAddress
    let txInput = new TransactionInput(data, addressTo = dEthMainnet, addressFrom = dEthMainnetOwner, gas = hexBigInt 9500000UL, value = hexBigInt 0UL);
    let txId =  (Web3(hardhatURI)).TransactionManager.SendTransactionAsync(txInput) |> Async.AwaitTask |> Async.RunSynchronously
    
    let txReceipt = (Web3(hardhatURI)).Eth.TransactionManager.TransactionReceiptService.PollForReceiptAsync(txId) |> Async.AwaitTask |> Async.RunSynchronously

    // return back to the old owner, and also check if we get any errors by calling that
    
    try 
        newContract.ExecuteFunctionFrom "giveCDPToDSProxy" [|dEthMainnet|] (EthereumConnection(hardhatURI, privKey)) |> ignore
    with 
    | :? System.AggregateException as aggr -> 
        
        // always return the ownership to the old owner to not leave an effect after the test executes
        try 
            newContract.ExecuteFunctionFrom "giveCDPToDSProxy" [|dEthMainnet|] (EthereumConnection(hardhatURI, hardhatPrivKey)) |> ignore
        with _ -> ()

        if not shouldThrow 
            then Ex.throwPreserve aggr
        else
            for inner in aggr.InnerExceptions do
                match inner with 
                | :? RpcResponseException as rpcEx -> printfn "eating exception in giveCDPToDSProxyF: %s" rpcEx.Message
                | _ -> Ex.throwPreserve aggr
      
[<Specification("dEth", "giveCDPToDSProxy", 0)>]
[<Fact>]
let ``dEth - giveCDPToDSProxy - can be called by owner`` () = giveCDPToDSProxyF hardhatPrivKey false

[<Specification("dEth", "giveCDPToDSProxy", 1)>]
[<Fact>]
let ``dEth - giveCDPToDSProxy - cannot be called by non-owner`` () = giveCDPToDSProxyF hardhatPrivKey2 true

[<Specification("dEth", "getCollateral", 0)>]
[<Fact>]
let ``dEth - getCollateral - returns similar values as those directly retrieved from the underlying contracts and calculated in F#`` () = 
    let (gulper, proxyCache, cdpId, makerManager, ethGemJoin, saverProxy, saverProxyActions, oracleContract, initialRecipient, dsGuardFactory, foundryTreasury, contract) = getDEthContract ()
    
    let priceEthDai = (oracleContract.Query<bigint> "getEthDaiPrice") [||]
    let priceRay = BigInteger.Multiply(BigInteger.Pow(bigint 10, 9), priceEthDai)
    let cdpDetailedInfoOutput = ContractPlug(ethConn, Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/MCDSaverProxy.json"), saverProxy).QueryObj<GetCdpDetailedInfoOutputDTO> "getCdpDetailedInfo" [|cdpId|]
    let collateralDenominatedDebt = rdiv cdpDetailedInfoOutput.Debt priceRay
    let excessCollateral = cdpDetailedInfoOutput.Collateral - collateralDenominatedDebt

    let getCollateralOutput = contract.QueryObj<GetCollateralOutputDTO> "getCollateral" [||]

    should equal priceRay getCollateralOutput.PriceRAY
    should equal cdpDetailedInfoOutput.Collateral getCollateralOutput.TotalCollateral
    should equal cdpDetailedInfoOutput.Debt getCollateralOutput.Debt
    should equal collateralDenominatedDebt getCollateralOutput.CollateralDenominatedDebt
    should equal excessCollateral getCollateralOutput.ExcessCollateral

[<Specification("dEth", "getCollateralPriceRAY", 0)>]
[<Fact>]
let ``dEth - getCollateralPriceRAY - returns similar values as those directly retrieved from the underlying contracts and calculated in F#`` () = 
    let (gulper, proxyCache, cdpId, makerManager, ethGemJoin, saverProxy, saverProxyActions, oracleContract, initialRecipient, dsGuardFactory, foundryTreasury, contract) = getDEthContract ()

    let ethDaiPrice = oracleContract.Query<bigint> "getEthDaiPrice" [||]
    let expectedRay = BigInteger.Pow(bigint 10, 9) * ethDaiPrice

    let actualRay = contract.Query<bigint> "getCollateralPriceRAY" [||]
    should equal expectedRay actualRay

[<Specification("dEth", "getExcessCollateral", 0)>]
[<Fact>]
let ``dEth - getExcessCollateral - returns similar values as those directly retrieved from the underlying contracts and calculated in F#`` () =
    let (gulper, proxyCache, cdpId, makerManager, ethGemJoin, saverProxy, saverProxyActions, oracleContract, initialRecipient, dsGuardFactory, foundryTreasury, contract) = getDEthContract ()
    let collateral = contract.QueryObj<GetCollateralOutputDTO> "getCollateral" [||]
    let expected = collateral.ExcessCollateral
    let actual = contract.Query<bigint> "getExcessCollateral" [||]
    should equal expected actual

[<Specification("dEth", "getRatio", 0)>]
[<Fact>]
let ``dEth - getRatio - returns similar values as those directly retrieved from the underlying contracts and calculated in F#`` () =
    let (gulper, proxyCache, cdpId, makerManager, ethGemJoin, saverProxy, saverProxyActions, oracleContract, initialRecipient, dsGuardFactory, foundryTreasury, contract) = getDEthContract ()
    let saverProxyContract = ContractPlug(ethConn, Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/MCDSaverProxy.json"), saverProxy)
    let cdpDetailedInfoOutput = saverProxyContract.QueryObj<GetCdpDetailedInfoOutputDTO> "getCdpDetailedInfo" [|cdpId|]
    let expected = saverProxyContract.Query<bigint> "getRatio" [|cdpId; cdpDetailedInfoOutput.Ilk|]
    let actual = contract.Query<bigint> "getRatio" [||]

    should equal expected actual