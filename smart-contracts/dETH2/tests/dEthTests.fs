module dEthTests

open TestBase
open Xunit
open FsUnit.Xunit
open Nethereum.Web3
open System.Numerics
open dEthTestsBase
open Nethereum.Hex.HexConvertors.Extensions
open Nethereum.Web3.Accounts
open Nethereum.JsonRpc.Client
open Nethereum.RPC.Eth.DTOs
open Nethereum.Contracts
open DETH2.Contracts.DEth.ContractDefinition
open DETH2.Contracts.MCDSaverProxy.ContractDefinition
open DETH2.Contracts.SaverProxyActions.ContractDefinition

type System.String with
   member s1.icompare(s2: string) =
     System.String.Equals(s1, s2, System.StringComparison.CurrentCultureIgnoreCase);

// TODO : please extend this to ensure that there is in fact a reading coming back from the underlying oracles and from
// the constructed oracle itself
[<Specification("Oracle", "constructor", 0)>]
[<Fact>]
let ``inits to provided parameters`` () =
    let (makerOracle, daiUsdOracle, ethUsdOracle) = (makeAccount().Address, makeAccount().Address, makeAccount().Address)
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
    let (priceMaker, _, priceNonMakerDaiEth, _) = initOraclesDefault differencePercent

    let price = oracleContract.Query<bigint> "getEthDaiPrice" [||]

    let expected =
        if differencePercent <= 0.1M 
            then toMakerPriceFormatDecimal priceNonMakerDaiEth 
            else toMakerPriceFormatDecimal priceMaker

    should equal expected price

[<Specification("dEth", "constructor", 0)>]
[<Fact>]
let ``initializes with correct values and rights assigned`` () =
    let (gulper, proxyCache, cdpId, makerManager, ethGemJoin, saverProxy, saverProxyActions, oracleContract, initialRecipient, foundryTreasury, authority, contract) = getDEthContractAndFields()

    // check the rights
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
    let contract = getDEthContract ()
    let randomAddress = makeAccount().Address
    contract.ExecuteFunction "changeGulper" [|randomAddress|] |> ignore
    shouldEqualIgnoringCase randomAddress <| contract.Query<string> "gulper" [||]

[<Specification("dEth", "changeGulper", 1)>]
[<Fact>]
let ``cannot be changed by non-owner`` () = 
    let contract = getDEthContract ()
    let account = Account(hardhatPrivKey2)
    let oldGulper = contract.Query<string> "gulper" [||]

    let debug = (Debug(EthereumConnection(hardhatURI, account.PrivateKey)))
    let receipt = contract.ExecuteFunctionFrom "changeGulper" [|account.Address|] debug
    let forwardEvent = debug.DecodeForwardedEvents receipt |> Seq.head
    forwardEvent |> shouldRevertWithUnknownMessage
    shouldEqualIgnoringCase oldGulper <| contract.Query<string> "gulper" [||]

let giveCDPToDSProxyTestBase shouldThrow = 
    let (_, _, cdpId, _, _, _, _, _, _, _, _, newContract) = getDEthContractAndFields ()

    // impersonate the owner of cdp owner i.e the owner of deth on mainnet
    ethConn.Web3.Client.SendRequestAsync(new RpcRequest(1, "hardhat_impersonateAccount", dEthMainnetOwner)) |> runNowWithoutResult

    // make a call from impersonated account
    let data = Web3.Sha3("giveCDPToDSProxy(address)").Substring(0, 8) + padAddress newContract.Address
    let txInput = new TransactionInput(data, addressTo = dEthMainnet, addressFrom = dEthMainnetOwner, gas = hexBigInt 9500000UL, value = hexBigInt 0UL);
    (Web3(hardhatURI)).TransactionManager.SendTransactionAsync(txInput) |> runNow |> ignore
  
    // return back to the old owner, and also check if we get any errors by calling that

    let executeGiveCDPFromPrivateKey privKey =
        let debug = (Debug(EthereumConnection(hardhatURI, privKey)))
        let giveCDPToDSProxyReceipt = newContract.ExecuteFunctionFrom "giveCDPToDSProxy" [|dEthMainnet|] debug
        giveCDPToDSProxyReceipt
    
    let giveCDPToDSProxyReceipt = executeGiveCDPFromPrivateKey (if shouldThrow then hardhatPrivKey2 else hardhatPrivKey)
    let cdpActionEvents = giveCDPToDSProxyReceipt.DecodeAllEvents<CDPActionEventDTO> ()

    if shouldThrow
        then
            // as a clean up, give cdp back from the valid owner
            executeGiveCDPFromPrivateKey hardhatPrivKey |> ignore

            let forwardEvent = debug.DecodeForwardedEvents giveCDPToDSProxyReceipt |> Seq.head
            shouldRevertWithUnknownMessage forwardEvent            
        else
            shouldSucceed giveCDPToDSProxyReceipt
            should equal 1 cdpActionEvents.Count
            should equal "give" cdpActionEvents.[0].Event.ReturnValue1
            should equal cdpId cdpActionEvents.[0].Event.ReturnValue2
            should equal 0 cdpActionEvents.[0].Event.ReturnValue3
            should equal 0 cdpActionEvents.[0].Event.ReturnValue4

[<Specification("dEth", "giveCDPToDSProxy", 0)>]
[<Fact>]
let ``dEth - giveCDPToDSProxy - can be called by owner`` () = giveCDPToDSProxyTestBase false

[<Specification("dEth", "giveCDPToDSProxy", 1)>]
[<Fact>]
let ``dEth - giveCDPToDSProxy - cannot be called by non-owner`` () = giveCDPToDSProxyTestBase true

[<Specification("dEth", "getCollateral", 0)>]
[<Fact>]
let ``dEth - getCollateral - returns similar values as those directly retrieved from the underlying contracts and calculated in F#`` () = 
    let (_, _, cdpId, _, _, saverProxy, _, oracleContract, _, _, _, contract) = getDEthContractAndFields ()
    
    let priceEthDai = (oracleContract.Query<bigint> "getEthDaiPrice") [||]
    let priceRay = BigInteger.Multiply(BigInteger.Pow(bigint 10, 9), priceEthDai)
    let saverProxy = ContractPlug(ethConn, getABI "MCDSaverProxy", saverProxy)
    let cdpDetailedInfoOutput = saverProxy.QueryObj<GetCdpDetailedInfoOutputDTO> "getCdpDetailedInfo" [|cdpId|]
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
    let (_, _, _, _, _, _, _, oracleContract, _, _, _, contract) = getDEthContractAndFields ()

    let ethDaiPrice = oracleContract.Query<bigint> "getEthDaiPrice" [||]
    let expectedRay = BigInteger.Pow(bigint 10, 9) * ethDaiPrice

    let actualRay = contract.Query<bigint> "getCollateralPriceRAY" [||]
    should equal expectedRay actualRay

[<Specification("dEth", "getExcessCollateral", 0)>]
[<Fact>]
let ``dEth - getExcessCollateral - returns similar values as those directly retrieved from the underlying contracts and calculated in F#`` () =
    let (_, _, cdpId, _, _, saverProxy, _, _, _, _, _, contract) = getDEthContractAndFields ()
    let saverProxyContract = ContractPlug(ethConn, Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/MCDSaverProxy.json"), saverProxy)
    let getCdpDetailedInfoOutputDTO =  saverProxyContract.QueryObj<GetCdpDetailedInfoOutputDTO> "getCdpDetailedInfo" [|cdpId|]
    let expected = getCdpDetailedInfoOutputDTO.Collateral - getCdpDetailedInfoOutputDTO.Debt
    let actual = contract.Query<bigint> "getExcessCollateral" [||]
    should equal expected actual

[<Specification("dEth", "getRatio", 0)>]
[<Fact>]
let ``dEth - getRatio - returns similar values as those directly retrieved from the underlying contracts and calculated in F#`` () =
    let (_, _, cdpId, _, _, saverProxy, _, _, _, _, _, contract) = getDEthContractAndFields ()
    let saverProxyContract = ContractPlug(ethConn, Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/MCDSaverProxy.json"), saverProxy)
    let manager = ContractPlug(ethConn, Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/ManagerLike.json"), "0x5ef30b9986345249bc32d8928B7ee64DE9435E39")

    let ilk = manager.Query<string> "ilks" [|cdpId|]
    let price = saverProxyContract.Query<bigint> "getPrice" [|ilk|]
    let getCdpInfoOutputDTO = saverProxyContract.QueryObj<GetCdpInfoOutputDTO> "getCdpInfo" [|manager.Address;cdpId;ilk|]

    let expected = if getCdpInfoOutputDTO.Debt = BigInteger.Zero 
                            then BigInteger.Zero 
                            else rdiv (wmul getCdpInfoOutputDTO.Collateral price) getCdpInfoOutputDTO.Debt

    let actual = contract.Query<bigint> "getRatio" [||]

    should equal expected actual