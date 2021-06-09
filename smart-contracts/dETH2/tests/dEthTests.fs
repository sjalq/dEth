module dEthTests

open System
open System.Numerics
open Xunit
open FsUnit.Xunit
open FsUnit.CustomMatchers
open Constants
open TestBase
open dEthTestsBase
open Nethereum.Web3
open Nethereum.Hex.HexConvertors.Extensions
open Nethereum.Web3.Accounts
open Nethereum.RPC.Eth.DTOs
open Nethereum.Contracts
open DETH2.Contracts.dEth.ContractDefinition
open DETH2.Contracts.MCDSaverProxy.ContractDefinition;

type SpotterIlksOutputDTO = DETH2.Contracts.ISpotter.ContractDefinition.IlksOutputDTO
type VatIlksOutputDTO = DETH2.Contracts.VatLike.ContractDefinition.IlksOutputDTO
type VatUrnsOutputDTO = DETH2.Contracts.VatLike.ContractDefinition.UrnsOutputDTO

type System.String with
   member s1.icompare(s2: string) =
     System.String.Equals(s1, s2, StringComparison.CurrentCultureIgnoreCase);

[<Specification("Oracle", "constructor", 0)>]
[<Fact>]
let ``inits to provided parameters`` () =
    restore ()

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
    restore ()

    let (priceMaker, _, priceNonMakerDaiEth, _) = initOraclesDefault differencePercent

    let price = oracleContract.Query<bigint> "getEthDaiPrice" [||]

    let expected =
        if differencePercent <= 0.1M
        then 
            toMakerPriceFormatDecimal priceNonMakerDaiEth
        else 
            toMakerPriceFormatDecimal priceMaker

    should equal expected price

[<Specification("dEth", "constructor", 0)>]
[<Fact>]
let ``initializes with correct values and rights assigned`` () =
    restore ()

    let authority, contract = getDEthContractAndAuthority()

    // check the rights
    let functionName = Web3.Sha3("automate(uint256,uint256,uint256,uint256,uint256,uint256)").Substring(0, 8).HexToByteArray()
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
    shouldEqualIgnoringCase oracleContractMainnet.Address (contract.Query<string> "oracle" [||])
    should equal true canCall
    should greaterThan BigInteger.Zero balanceOfInitialRecipient

[<Specification("dEth", "changeGulper", 0)>]
[<Fact>]
let ``can be changed by owner`` () =
    restore ()
    let contract = getDEthContract ()
    let randomAddress = makeAccount().Address
    contract.ExecuteFunction "changeGulper" [|randomAddress|] |> ignore
    shouldEqualIgnoringCase randomAddress <| contract.Query<string> "gulper" [||]

[<Specification("dEth", "changeGulper", 1)>]
[<Fact>]
let ``cannot be changed by non-owner`` () = 
    restore ()
    let contract = getDEthContract ()
    let account = Account(hardhatPrivKey2)
    let oldGulper = contract.Query<string> "gulper" [||]

    let debug = (Debug(EthereumConnection(hardhatURI, account.PrivateKey)))
    let receipt = contract.ExecuteFunctionFrom "changeGulper" [|account.Address|] debug
    let forwardEvent = debug.DecodeForwardedEvents receipt |> Seq.head
    forwardEvent |> shouldRevertWithUnknownMessage
    shouldEqualIgnoringCase oldGulper <| contract.Query<string> "gulper" [||]

let giveCDPToDSProxyTestBase shouldThrow = 
    restore ()
    let newContract = getDEthContract ()

    let executeGiveCDPFromPrivateKey shouldThrow =
        let ethConn = 
            if shouldThrow 
            then 
                (Debug(EthereumConnection(hardhatURI, hardhatPrivKey2)) :> IAsyncTxSender) 
            else 
                ethConn :> IAsyncTxSender

        let giveCDPToDSProxyReceipt = dEthContract.ExecuteFunctionFrom "giveCDPToDSProxy" [|newContract.Address|] ethConn
        giveCDPToDSProxyReceipt
    
    let giveCDPToDSProxyReceipt = executeGiveCDPFromPrivateKey shouldThrow

    if shouldThrow
        then
            let forwardEvent = debug.DecodeForwardedEvents giveCDPToDSProxyReceipt |> Seq.head
            forwardEvent |> shouldRevertWithUnknownMessage
        else
            giveCDPToDSProxyReceipt.Succeeded () |> should equal true
        

[<Specification("dEth", "giveCDPToDSProxy", 0)>]
[<Fact>]
let ``dEth - giveCDPToDSProxy - can be called by owner`` () = giveCDPToDSProxyTestBase false

[<Specification("dEth", "giveCDPToDSProxy", 1)>]
[<Fact>]
let ``dEth - giveCDPToDSProxy - cannot be called by non-owner`` () = giveCDPToDSProxyTestBase true

[<Specification("dEth", "getCollateral", 0)>]
[<Fact>]
let ``dEth - getCollateral - returns similar values as those directly retrieved from the underlying contracts and calculated in F#`` () = 
    restore ()
    let contract = getDEthContract ()

    let getCollateralOutput = contract.QueryObj<GetCollateralOutputDTO> "getCollateral" [||]
    let (_, priceRay, _, cdpDetailedInfoOutput, collateralDenominatedDebt, excessCollateral) = 
        getManuallyComputedCollateralValues oracleContractMainnet saverProxy cdpId
    
    should equal priceRay getCollateralOutput.PriceRAY
    should equal cdpDetailedInfoOutput.Collateral getCollateralOutput.TotalCollateral
    should equal cdpDetailedInfoOutput.Debt getCollateralOutput.Debt
    should equal collateralDenominatedDebt getCollateralOutput.CollateralDenominatedDebt
    should equal excessCollateral getCollateralOutput.ExcessCollateral

[<Specification("dEth", "getCollateralPriceRAY", 0)>]
[<Fact>]
let ``dEth - getCollateralPriceRAY - returns similar values as those directly retrieved from the underlying contracts and calculated in F#`` () = 
    restore ()
    let contract = getDEthContract ()

    let ethDaiPrice = oracleContractMainnet.Query<bigint> "getEthDaiPrice" [||]
    let expectedRay = BigInteger.Pow(bigint 10, 9) * ethDaiPrice

    let actualRay = contract.Query<bigint> "getCollateralPriceRAY" [||]
    should equal expectedRay actualRay

[<Specification("dEth", "getExcessCollateral", 0)>]
[<Fact>]
let ``dEth - getExcessCollateral - returns similar values as those directly retrieved from the underlying contracts and calculated in F#`` () =
    restore ()
    let contract = getDEthContract ()

    let (_, _, _, _, _, excessCollateral) = getManuallyComputedCollateralValues oracleContractMainnet saverProxy cdpId

    let actual = contract.Query<bigint> "getExcessCollateral" [||]
    should equal excessCollateral actual

[<Specification("dEth", "getRatio", 0)>]
[<Fact>]
let ``dEth - getRatio - returns similar values as those directly retrieved from the underlying contracts and calculated in F#`` () =
    restore ()
    let contract = getDEthContract ()
    let saverProxyContract = ContractPlug(ethConn, (getABI "MCDSaverProxy"), saverProxy)
    let manager = ContractPlug(ethConn, getABI "ManagerLike", makerManager)

    let ilk = manager.Query<string> "ilks" [|cdpId|]
    let price = saverProxyContract.Query<bigint> "getPrice" [|ilk|]
    let getCdpInfoOutputDTO = saverProxyContract.QueryObj<GetCdpInfoOutputDTO> "getCdpInfo" [|manager.Address;cdpId;ilk|]

    let expected = 
        if getCdpInfoOutputDTO.Debt = BigInteger.Zero 
        then 
            BigInteger.Zero 
        else 
            rdiv (wmul getCdpInfoOutputDTO.Collateral price) getCdpInfoOutputDTO.Debt

    let actual = contract.Query<bigint> "getRatio" [||]

    should equal expected actual

[<Specification("dEth", "automate", 0)>]
[<Theory>]
[<InlineData(foundryTreasury, 180, 220, 220, 1, 1, 1)>]
[<InlineData(ownerArg, 180, 220, 220, 1, 1, 1)>]
[<InlineData(contractArg, 180, 220, 220, 1, 1, 1)>]
let ``dEth - automate - an authorised address can change the automation settings`` (addressArgument:string) (repaymentRatioExpected:int) (targetRatioExpected:int) (boostRatioExpected:int) (minRedemptionRatioExpected:int) (automationFeePercExpected:int) (riskLimitExpected:int) =
    restore ()

    let automateTxr = 
        AutomateFunction(
            RepaymentRatio = bigint repaymentRatioExpected, 
            TargetRatio = bigint targetRatioExpected,
            BoostRatio = bigint boostRatioExpected, 
            MinRedemptionRatioPerc = bigint minRedemptionRatioExpected,
            AutomationFeePerc = bigint automationFeePercExpected, 
            RiskLimit = bigint riskLimitExpected)
        |> ethConn.MakeImpersonatedCallWithNoEther (mapInlineDataArgumentToAddress addressArgument dEthContract.Address) dEthContract.Address

    automateTxr |> shouldSucceed

    dEthContract.Query<bigint> "minRedemptionRatioPerc" [||] |> should equal (bigint minRedemptionRatioExpected)
    dEthContract.Query<bigint> "automationFeePerc" [||] |> should equal (bigint automationFeePercExpected)
    dEthContract.Query<bigint> "riskLimit" [||] |> should equal (bigint riskLimitExpected)

    let event = automateTxr.DecodeAllEvents<AutomationSettingsChangedEventDTO>() |> Seq.map (fun i -> i.Event) |> Seq.head
    event.RepaymentRatio |> should equal <| bigint repaymentRatioExpected
    event.TargetRatio |> should equal <| bigint targetRatioExpected
    event.BoostRatio |> should equal <| bigint boostRatioExpected
    event.MinRedemptionRatioPerc |> should equal <| bigint minRedemptionRatioExpected
    event.AutomationFeePerc |> should equal <| bigint automationFeePercExpected
    event.RiskLimit |> should equal <| bigint riskLimitExpected

[<Specification("dEth", "automate", 1)>]
[<Theory>]
[<InlineData(repaymentRatio, targetRatio, boostRatio, 1, 1, 1)>]
let ``dEth - automate - an unauthorised address cannot change the automation settings`` (repaymentRatioExpected:int) (targetRatioExpected:int) (boostRatioExpected:int) (minRedemptionRatioExpected:int) (automationFeePercExpected:int) (riskLimitExpected:int) = 
    restore ()

    Debug <| EthereumConnection(hardhatURI, makeAccountWithBalance().PrivateKey)
    |> dEthContract.ExecuteFunctionFrom "automate" [|repaymentRatioExpected;targetRatioExpected;boostRatioExpected;minRedemptionRatioExpected;automationFeePercExpected;riskLimitExpected|]
    |> debug.DecodeForwardedEvents
    |> Seq.head
    |> shouldRevertWithUnknownMessage // To clarify : We get no message because the auth code reverts without providing one

[<Specification("dEth", "redeem", 0)>]
[<Theory>]
[<InlineData(10.0, 7.0, false)>]
[<InlineData(1.0, 0.7, false)>]
[<InlineData(0.01, 0.005, false)>]
[<InlineData(0.001, 0.0005, false)>]
[<InlineData(10.0, 7.0, true)>]
[<InlineData(1.0, 0.05, true)>]
let ``dEth - redeem - someone with a positive balance of dEth can redeem the expected amount of Ether`` (tokensToMint:float) (tokensToRedeem:float) (riskLevelShouldBeExceeded:bool) =
    restore ()

    let redeemerConnection = EthereumConnection(hardhatURI, hardhatPrivKey2)

    let tokensToTransferBigInt = tokensToMint |> toE18
    let tokensToRedeemBigInt = tokensToRedeem |> toE18

    tokensToRedeemBigInt |> should lessThan tokensToTransferBigInt

    dEthContract.ExecuteFunction "transfer" [|redeemerConnection.Account.Address;tokensToTransferBigInt|] |> shouldSucceed

    let tokenBalanceBefore = balanceOf dEthContract redeemerConnection.Account.Address

    let gulperBalanceBefore = getGulperEthBalance ()

    if riskLevelShouldBeExceeded then
        makeRiskLimitLessThanExcessCollateral dEthContract |> shouldSucceed

    let receiverAddress = makeAccount().Address
    
    let (protocolFeeExpected, automationFeeExpected, collateralRedeemedExpected, collateralReturnedExpected) = 
        queryStateAndCalculateRedemptionValue dEthContract tokensToRedeemBigInt

    let redeemTx = redeemerConnection |> dEthContract.ExecuteFunctionFrom "redeem" [|receiverAddress;tokensToRedeemBigInt|]
    redeemTx |> shouldSucceed

    receiverAddress |> ethConn.GetEtherBalance |> should equal collateralReturnedExpected
    getGulperEthBalance () |> should equal (protocolFeeExpected + gulperBalanceBefore)

    balanceOf dEthContract redeemerConnection.Account.Address |> should equal (tokenBalanceBefore - tokensToRedeemBigInt)

    let event = redeemTx.DecodeAllEvents<RedeemedEventDTO>() |> Seq.map (fun i -> i.Event) |> Seq.head
    event.Redeemer |> shouldEqualIgnoringCase redeemerConnection.Account.Address
    event.Receiver |> shouldEqualIgnoringCase receiverAddress
    event.TokensRedeemed |> should equal tokensToRedeemBigInt
    event.ProtocolFee |> should equal protocolFeeExpected
    event.AutomationFee |> should equal automationFeeExpected
    event.CollateralRedeemed |> should equal collateralRedeemedExpected
    event.CollateralReturned |> should equal collateralReturnedExpected

[<Specification("dEth", "redeem", 1)>]
[<Theory>]
[<InlineData(10000)>]
let ``dEth - redeem - someone without a balance can never redeem Ether`` tokensAmount =
    restore ()

    Debug <| EthereumConnection(hardhatURI, makeAccountWithBalance().PrivateKey) // the balance is needed for gas vs for sending ether value.
    |> dEthContract.ExecuteFunctionFrom "redeem" [|makeAccount().Address;tokensAmount|]
    |> debug.DecodeForwardedEvents
    |> Seq.head
    |> shouldRevertWithMessage "ERC20: burn amount exceeds balance"

[<Specification("dEth", "squanderMyEthForWorthlessBeans", 1)>]
[<Theory>]
[<InlineData(100.0)>]
[<InlineData(10.0)>]
[<InlineData(1.0)>]
[<InlineData(0.01)>]
[<InlineData(0.001)>]
[<InlineData(0.0001)>]
[<InlineData(0.0)>] // a test case checking that no-one providing no ether can issue themselves any deth
let ``dEth - squanderMyEthForWorthlessBeans - anyone providing a positive balance of Ether can issue themselves the expected amount of dEth`` (providedCollateral:float) =
    restore ()

    let providedCollateralBigInt = bigint providedCollateral

    let inkBefore = getInk ()
    let gulperBalanceBefore = getGulperEthBalance ()
    
    dEthContract.Query<bigint> "getExcessCollateral" [||]
    |> should lessThan (dEthContract.Query<bigint> "riskLimit" [||] + providedCollateralBigInt)
    
    let (protocolFeeExpected, automationFeeExpected, actualCollateralAddedExpected, accreditedCollateralExpected, tokensIssuedExpected) = 
        queryStateAndCalculateIssuanceAmount dEthContract providedCollateralBigInt

    let dEthRecipientAddress = ethConn.Account.Address
    let balanceBefore = balanceOf dEthContract dEthRecipientAddress

    let squanderTxr = dEthContract.ExecuteFunctionFromAsyncWithValue providedCollateralBigInt "squanderMyEthForWorthlessBeans" [|dEthRecipientAddress|] ethConn |> runNow
    squanderTxr |> shouldSucceed

    balanceOf dEthContract dEthRecipientAddress |> should equal (balanceBefore + tokensIssuedExpected)
    getInk () |> should equal (inkBefore + actualCollateralAddedExpected)
    getGulperEthBalance () |> should equal (gulperBalanceBefore + protocolFeeExpected)

    let issuedEvent = squanderTxr.DecodeAllEvents<IssuedEventDTO>() |> Seq.map (fun i -> i.Event) |> Seq.head

    issuedEvent.Receiver |> shouldEqualIgnoringCase dEthRecipientAddress
    issuedEvent.SuppliedCollateral |> should equal providedCollateralBigInt
    issuedEvent.ProtocolFee |> should equal protocolFeeExpected
    issuedEvent.AutomationFee |> should equal automationFeeExpected
    issuedEvent.ActualCollateralAdded |> should equal actualCollateralAddedExpected
    issuedEvent.AccreditedCollateral |> should equal accreditedCollateralExpected
    issuedEvent.TokensIssued |> should equal tokensIssuedExpected

[<Specification("dEth", "squanderMyEthForWorthlessBeans", 2)>]
[<Fact>]
let ``dEth - squanderMyEthForWorthlessBeans - the riskLevel cannot be exceeded`` () =
    restore ()

    makeRiskLimitLessThanExcessCollateral dEthContract |> shouldSucceed

    debug
    |> dEthContract.ExecuteFunctionFrom "squanderMyEthForWorthlessBeans" [|makeAccount().Address|]
    |> debug.DecodeForwardedEvents
    |> Seq.head
    |> shouldRevertWithMessage "risk limit exceeded"