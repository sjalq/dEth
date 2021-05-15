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
open Nethereum.Util
open Nethereum.Hex.HexConvertors.Extensions
open Nethereum.Web3.Accounts
open Nethereum.JsonRpc.Client
open Nethereum.RPC.Eth.DTOs
open Nethereum.Contracts
open DETH2.Contracts.dEth.ContractDefinition
open DETH2.Contracts.MCDSaverProxy.ContractDefinition;
open DETH2.Contracts.VatLike.ContractDefinition
open DETH2.Contracts.PipLike.ContractDefinition
open DETH2.Contracts.IFlipper.ContractDefinition
open DETH2.Contracts.ManagerLike.ContractDefinition

type SpotterIlksOutputDTO = DETH2.Contracts.ISpotter.ContractDefinition.IlksOutputDTO
type VatIlksOutputDTO = DETH2.Contracts.VatLike.ContractDefinition.IlksOutputDTO
type VatUrnsOutputDTO = DETH2.Contracts.VatLike.ContractDefinition.UrnsOutputDTO

type System.String with
   member s1.icompare(s2: string) =
     System.String.Equals(s1, s2, System.StringComparison.CurrentCultureIgnoreCase);

// ResolvedTODO : please extend this to ensure that there is in fact a reading coming back from the underlying oracles and from
// the constructed oracle itself
// Resolution: It's already tested in the getEthDaiPrice test
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
    let newContract = getDEthContract ()

    let cdpManagerContract = ContractPlug(ethConn, getABI "ManagerLike", makerManager)

    // impersonate the owner of cdp owner i.e the owner of deth on mainnet
    ethConn.Web3.Client.SendRequestAsync(new RpcRequest(1, "hardhat_impersonateAccount", dEthMainnetOwner)) |> runNowWithoutResult

    // make a call from impersonated account
    let data = Web3.Sha3("giveCDPToDSProxy(address)").Substring(0, 8) + padAddress newContract.Address
    let txInput = new TransactionInput(data, addressTo = dEthMainnet, addressFrom = dEthMainnetOwner, gas = hexBigInt 9500000UL, value = hexBigInt 0UL);
    (Web3(hardhatURI)).TransactionManager.SendTransactionAsync(txInput) |> runNow |> ignore

    let executeGiveCDPFromPrivateKey shouldThrow =
        let ethConn = if shouldThrow 
                            then (Debug(EthereumConnection(hardhatURI, hardhatPrivKey2)) :> IAsyncTxSender) 
                            else ethConn :> IAsyncTxSender

        let giveCDPToDSProxyReceipt = newContract.ExecuteFunctionFrom "giveCDPToDSProxy" [|dEthMainnet|] ethConn
        giveCDPToDSProxyReceipt
    
    let giveCDPToDSProxyReceipt = executeGiveCDPFromPrivateKey shouldThrow

    if shouldThrow
        then
            // as a clean up, give cdp back from the valid owner
            executeGiveCDPFromPrivateKey false |> ignore

            let forwardEvent = debug.DecodeForwardedEvents giveCDPToDSProxyReceipt |> Seq.head
            shouldRevertWithUnknownMessage forwardEvent
   
    cdpManagerContract.Query<string> "owns" [|cdpId|] |> shouldEqualIgnoringCase dEthMainnet

[<Specification("dEth", "giveCDPToDSProxy", 0)>]
[<Fact>]
let ``dEth - giveCDPToDSProxy - can be called by owner`` () = giveCDPToDSProxyTestBase false

[<Specification("dEth", "giveCDPToDSProxy", 1)>]
[<Fact>]
let ``dEth - giveCDPToDSProxy - cannot be called by non-owner`` () = giveCDPToDSProxyTestBase true

[<Specification("dEth", "getCollateral", 0)>]
[<Fact>]
let ``dEth - getCollateral - returns similar values as those directly retrieved from the underlying contracts and calculated in F#`` () = 
    let contract = getDEthContract ()

    let getCollateralOutput = contract.QueryObj<GetCollateralOutputDTO> "getCollateral" [||]
    let (_, priceRay, _, cdpDetailedInfoOutput, collateralDenominatedDebt, excessCollateral) = getManuallyComputedCollateralValues oracleContractMainnet saverProxy cdpId
    
    should equal priceRay getCollateralOutput.PriceRAY
    should equal cdpDetailedInfoOutput.Collateral getCollateralOutput.TotalCollateral
    should equal cdpDetailedInfoOutput.Debt getCollateralOutput.Debt
    should equal collateralDenominatedDebt getCollateralOutput.CollateralDenominatedDebt
    should equal excessCollateral getCollateralOutput.ExcessCollateral

[<Specification("dEth", "getCollateralPriceRAY", 0)>]
[<Fact>]
let ``dEth - getCollateralPriceRAY - returns similar values as those directly retrieved from the underlying contracts and calculated in F#`` () = 
    let contract = getDEthContract ()

    let ethDaiPrice = oracleContractMainnet.Query<bigint> "getEthDaiPrice" [||]
    let expectedRay = BigInteger.Pow(bigint 10, 9) * ethDaiPrice

    let actualRay = contract.Query<bigint> "getCollateralPriceRAY" [||]
    should equal expectedRay actualRay

[<Specification("dEth", "getExcessCollateral", 0)>]
[<Fact>]
let ``dEth - getExcessCollateral - returns similar values as those directly retrieved from the underlying contracts and calculated in F#`` () =
    let contract = getDEthContract ()

    let (_, _, _, _, _, excessCollateral) = getManuallyComputedCollateralValues oracleContractMainnet saverProxy cdpId

    let actual = contract.Query<bigint> "getExcessCollateral" [||]
    should equal excessCollateral actual

[<Specification("dEth", "getRatio", 0)>]
[<Fact>]
let ``dEth - getRatio - returns similar values as those directly retrieved from the underlying contracts and calculated in F#`` () =
    let contract = getDEthContract ()
    let saverProxyContract = ContractPlug(ethConn, (getABI "MCDSaverProxy"), saverProxy)
    let manager = ContractPlug(ethConn, getABI "ManagerLike", makerManager)

    let ilk = manager.Query<string> "ilks" [|cdpId|]
    let price = saverProxyContract.Query<bigint> "getPrice" [|ilk|]
    let getCdpInfoOutputDTO = saverProxyContract.QueryObj<GetCdpInfoOutputDTO> "getCdpInfo" [|manager.Address;cdpId;ilk|]

    let expected = if getCdpInfoOutputDTO.Debt = BigInteger.Zero 
                            then BigInteger.Zero 
                            else rdiv (wmul getCdpInfoOutputDTO.Collateral price) getCdpInfoOutputDTO.Debt

    let actual = contract.Query<bigint> "getRatio" [||]

    should equal expected actual

// SkipTODO:
// events emitted
// revise the logic
[<Specification("cdp", "bite", 0)>]
[<Fact(Skip="Ended up being too complex, was removed from contract")>]
let ``biting of a CDP - should bite when collateral is < 150`` () =
    // set-up the test
    ethConn.Web3.Client.SendRequestAsync(new RpcRequest(1, "hardhat_impersonateAccount", ilkPIPAuthority)) |> runNowWithoutResult
    ethConn.Web3.Client.SendRequestAsync(new RpcRequest(2, "hardhat_impersonateAccount", spot)) |> runNowWithoutResult
    ethConn.Web3.Client.SendRequestAsync(new RpcRequest(3, "hardhat_impersonateAccount", dEthMainnet)) |> runNowWithoutResult

    let catContract = ContractPlug(ethConn, getABI "ICat", cat)
    let spotterContract = ContractPlug(ethConn, getABI "ISpotter", spot)
    let flipperContract = ContractPlug(ethConn, getABI "IFlipper", ilkFlipper)
    let vatContract = ContractPlug(ethConn, getABI "VatLike", vat)
    let makerManagerAdvanced = ContractPlug(ethConn, getABI "IMakerManagerAdvanced", makerManager)
    let oracleAdapter = makeContract [||] "MakerOracleAdapter"

    let (ilk, urn) = getInkAndUrnFromCdp makerManagerAdvanced cdpId
    let pipAddress = (spotterContract.QueryObj<SpotterIlksOutputDTO> "ilks" [|ilk|]).Pip

    // set mock oracle to our dEth to lead to the relevant maker oracle and initialize dEth
    ethConn.MakeImpersonatedCallWithNoEther ilkPIPAuthority pipAddress (KissFunction(A=oracleAdapter.Address)) |> shouldSucceed
    oracleAdapter.ExecuteFunction "setOracle" [|pipAddress|] |> shouldSucceed
    let (_, dEthContract) = getDEthContractFromOracle oracleAdapter true

    // transfer some tokens to the debug account so we can call functions that rely on the token balance from it
    let debugTransferAmount = 10
    dEthContract.ExecuteFunction "transfer" [|debug.ContractPlug.Address;debugTransferAmount|] |> shouldSucceed

    // calculate price to make the ratio between total collateral and collateral denominated debt 145%
    let currentPrice = oracleAdapter.Query<bigint> "getEthDaiPrice" [||]
    let collateralOutputInitial = dEthContract.QueryObj<GetCollateralOutputDTO> "getCollateral" [||]
    let urnDTOInitial = vatContract.QueryObj<VatUrnsOutputDTO> "urns" [|ilk; urn|]
    let ratio = toBigDecimal collateralOutputInitial.TotalCollateral / toBigDecimal collateralOutputInitial.CollateralDenominatedDebt
    let wantedRatio = 1.45M
    let diff = ratio / BigDecimal wantedRatio
    let wantedPrice = (toBigDecimal currentPrice / BigDecimal(diff))
    let wantedPriceBigInt = toBigInt wantedPrice
    
    // transfer cdp from the mainnet deth to the new dEth contract
    ethConn.MakeImpersonatedCallWithNoEther dEthMainnet makerManager (GiveFunction(Cdp = cdpId, Dst = dEthContract.Address)) |> shouldSucceed

    // set-up the test - end

    // STEP 1 - change price and check that excess collateral went down after price change by the percent that price was divided by.
    let mockDSValueContract = getMockDSValueFormat wantedPriceBigInt
    ethConn.MakeImpersonatedCallWithNoEther ilkPIPAuthority pipAddress (ChangeFunction(Src_ = mockDSValueContract.Address)) |> shouldSucceed

    // poke twice
    doTimes 2 <| (fun _ -> pokePIP pipAddress)
    spotterContract.ExecuteFunction "poke" [|ilk|] |> shouldSucceed

    let collateralOutputAfterPriceChange = dEthContract.QueryObj<GetCollateralOutputDTO> "getCollateral" [||]
    let urnDTOAfterPriceChange = vatContract.QueryObj<VatUrnsOutputDTO> "urns" [|ilk; urn|]
    should be (lessThan <| collateralOutputInitial.ExcessCollateral) collateralOutputAfterPriceChange.ExcessCollateral
    should equal collateralOutputInitial.TotalCollateral collateralOutputAfterPriceChange.TotalCollateral
    let collateralDebtDiff = Math.Round(decimal (toBigDecimal collateralOutputAfterPriceChange.CollateralDenominatedDebt / toBigDecimal collateralOutputInitial.CollateralDenominatedDebt), 5)
    let priceDiff = Math.Round((decimal diff), 5)
    should equal priceDiff collateralDebtDiff

    should equal urnDTOInitial.Art urnDTOAfterPriceChange.Art
    should equal urnDTOInitial.Ink urnDTOAfterPriceChange.Ink

    let gemAmountBeforeBite = vatContract.Query<bigint> "gem" [|ilk; urn|]

    // STEP 2 - bite, current excessCollateral should be within 0-35% of the excess collateral before biting. as the vat.grab() is called and the vault gets liquidated.
    // kick is called, guy is CDP manager vs our account
    catContract.ExecuteFunction "bite" [|ilk;urn|] |> shouldSucceed

    let urnDTOAfterBite = vatContract.QueryObj<VatUrnsOutputDTO> "urns" [|ilk; urn|]
    let gemAmountAfterBite = vatContract.Query<bigint> "gem" [|ilk; urn|]
    let collateralOutputAfterBite = dEthContract.QueryObj<GetCollateralOutputDTO> "getCollateral" [||]
    let percentDiff = bigintDifference collateralOutputAfterBite.Debt collateralOutputAfterPriceChange.Debt 4
    let percentTotalCollateral = bigintDifference collateralOutputAfterBite.TotalCollateral collateralOutputAfterPriceChange.TotalCollateral 4
    let percentTotalExcessCollateral = bigintDifference collateralOutputAfterBite.ExcessCollateral collateralOutputAfterPriceChange.ExcessCollateral 4
    let percentCollateralDenominatedDebt = bigintDifference collateralOutputAfterBite.CollateralDenominatedDebt collateralOutputAfterPriceChange.CollateralDenominatedDebt 4
    let percentArt = bigintDifference urnDTOAfterBite.Art urnDTOAfterPriceChange.Art 4
    let percentInk = bigintDifference urnDTOAfterBite.Ink urnDTOAfterPriceChange.Ink 4

    should lessThanOrEqualTo 35M percentDiff
    should equal percentDiff percentTotalCollateral
    should equal percentDiff percentTotalExcessCollateral
    should equal percentDiff percentCollateralDenominatedDebt
    should equal percentDiff percentArt
    should equal percentDiff percentInk
    should equal gemAmountBeforeBite gemAmountAfterBite

    // redeem should revert
    let redeemFailTx = dEthContract.ExecuteFunctionFrom "redeem" [|ethConn.Account.Address;10|] debug
    debug.DecodeForwardedEvents redeemFailTx |> Seq.head |> shouldRevertWithMessage "cannot violate collateral safety ratio"

    // squander should revert as well
    //let squanderTx = dEthContract.ExecuteFunctionFromAsyncWithValue (BigInteger(500)) "squanderMyEthForWorthlessBeans" [|ethConn.Account.Address|] debug |> runNow
    //debug.DecodeForwardedEvents squanderTx |> Seq.head |> shouldRevertWithUnknownMessage

    // STEP 3 - open auction to sell the ilk in cdp
    let maxAuctionLengthInSeconds = bigint 50
    let maxBidLengthInSeconds = bigint 20
    ethConn.MakeImpersonatedCallWithNoEther ilkFlipperAuthority ilkFlipper (FileFunction(What = strToByte32 "tau", Data = maxAuctionLengthInSeconds)) |> shouldSucceed
    ethConn.MakeImpersonatedCallWithNoEther ilkFlipperAuthority ilkFlipper (FileFunction(What = strToByte32 "ttl", Data = maxBidLengthInSeconds)) |> shouldSucceed

    let id = flipperContract.Query<bigint> "kicks" [||] // get the latest auction id
    
    let bidsOutputDTO = flipperContract.QueryObj<BidsOutputDTO> "bids" [|id|]
    let expectedLot = bidsOutputDTO.Lot - bidsOutputDTO.Lot / bigint 10M // bid for 10% less lot.

    // emit Tab DAI in the VAT for the account that is bidding.
    ethConn.MakeImpersonatedCallWithNoEther spot vat <| SuckFunction(U = ethConn.Account.Address, V = ethConn.Account.Address, Rad = bidsOutputDTO.Tab) |> shouldSucceed

    vatContract.ExecuteFunction "hope" [|flipperContract.Address|] |> shouldSucceed
    flipperContract.ExecuteFunction "tend" [|id;bidsOutputDTO.Lot;bidsOutputDTO.Tab|] |> shouldSucceed
    flipperContract.ExecuteFunction "dent" [|id;expectedLot;bidsOutputDTO.Tab|] |> shouldSucceed

    // here bids guy should be our account
    // Flipper.tend moves the bid (DAI) amount to the VOW
    // Flipper.dent moves avaiable (10%) collateral from flipper to the usr - vault address, bids.lot - lot. (so, it should be 10% of the lot). It impacts gem mapping, but in the test we are quering urns.
    // Flipper.deal moves remaining (90%) collateral from flipper to bid.guy
    // bug ? - flux - transfers collateral between users, but doesn't update Urn.ink
    // but - it goes to the owner of the vault vs the vault itself (which makes sense as it's being liquidated)

    // retrieve the bids data before calling deal, because it is removed during deal execution.
    let bidsOutputDTOAfterAuction = flipperContract.QueryObj<BidsOutputDTO> "bids" [|id|]

    // end the auction
    ethConn.TimeTravel maxAuctionLengthInSeconds
    flipperContract.ExecuteFunction "deal" [|id|] |> shouldSucceed

    // after hope/tend/dent/deal - the gem amount should go up, collateral and urn shouldn't change.
    // The guy should be hardhat's first account's address.
    // We shouldn't be able to redeem.
    let urnDTOAfterAuctionEnd = vatContract.QueryObj<VatUrnsOutputDTO> "urns" [|ilk; urn|]
    let gemAmountAfterAuctionEnd = vatContract.Query<bigint> "gem" [|ilk; urn|]
    let collateralOutputAfterAuctionEnd = dEthContract.QueryObj<GetCollateralOutputDTO> "getCollateral" [||]

    should equal (gemAmountAfterBite + (bidsOutputDTO.Lot - bidsOutputDTOAfterAuction.Lot)) gemAmountAfterAuctionEnd
    should equal expectedLot bidsOutputDTOAfterAuction.Lot
    shouldEqualIgnoringCase ethConn.Account.Address bidsOutputDTOAfterAuction.Guy

    should equal collateralOutputAfterBite.ExcessCollateral collateralOutputAfterAuctionEnd.ExcessCollateral
    should equal urnDTOAfterBite.Art urnDTOAfterAuctionEnd.Art
    should equal urnDTOAfterBite.Ink urnDTOAfterAuctionEnd.Ink

    // redeem should revert
    let redeemFailTx = dEthContract.ExecuteFunctionFrom "redeem" [|Account(hardhatPrivKey).Address;10|] debug
    debug.DecodeForwardedEvents redeemFailTx |> Seq.head |> shouldRevertWithMessage "cannot violate collateral safety ratio"

    // squander should revert as well
    //let squanderTx = dEthContract.ExecuteFunctionFromAsyncWithValue (BigInteger(500)) "squanderMyEthForWorthlessBeans" [|ethConn.Account.Address|] debug |> runNow
    //debug.DecodeForwardedEvents squanderTx |> Seq.head |> shouldRevertWithUnknownMessage

    // STEP 4: MoveVatEthToCDP
    // need to check that vat eth was indeed moved and that excess collateral is up again.
    dEthContract.ExecuteFunction "moveVatEthToCDP" [||] |> shouldSucceed
    
    let collateralOutputAfterMoveVat = dEthContract.QueryObj<GetCollateralOutputDTO> "getCollateral" [||]
    let urnDTOAfterMoveVat = vatContract.QueryObj<VatUrnsOutputDTO> "urns" [|ilk; urn|]
    let gemAmountAfterMoveVat = vatContract.Query<bigint> "gem" [|ilk; urn|]

    should equal (urnDTOAfterAuctionEnd.Ink + gemAmountAfterAuctionEnd) urnDTOAfterMoveVat.Ink
    should equal urnDTOAfterAuctionEnd.Art urnDTOAfterMoveVat.Art
    should equal BigInteger.Zero gemAmountAfterMoveVat
    should greaterThan collateralOutputAfterAuctionEnd.TotalCollateral collateralOutputAfterMoveVat.TotalCollateral

    // STEP 5: check that we can redeem and that the account has ether after the redeem call
    let address = makeAccount().Address
    dEthContract.ExecuteFunction "redeem" [|address;(collateralOutputAfterMoveVat.TotalCollateral - collateralOutputAfterAuctionEnd.TotalCollateral)|] |> shouldSucceed
    should greaterThan (bigint 0) <| ethConn.GetEtherBalance(address)

    // check that we can squander and that we have received ERC20 tokens
    dEthContract.ExecuteFunctionFromAsyncWithValue (BigInteger(500)) "squanderMyEthForWorthlessBeans" [|address|] ethConn |> runNow |> shouldSucceed
    should greaterThan (bigint 0) <| dEthContract.Query<bigint> "balanceOf" [|address|]

[<Specification("dEth", "automate", 0)>]
[<Theory>]
[<InlineData(foundryTreasury, 180, 220, 220, 1, 1, 1)>]
[<InlineData(ownerArg, 180, 220, 220, 1, 1, 1)>]
[<InlineData(contractArg, 180, 220, 220, 1, 1, 1)>]
let ``dEth - automate - check that an authorised address can change the automation settings`` (addressArgument:string) (repaymentRatioExpected:int) (targetRatioExpected:int) (boostRatioExpected:int) (minRedemptionRatioExpected:int) (automationFeePercExpected:int) (riskLimitExpected:int) =
    restore ()

    // ResolvedTODO : Hoist into getDEthContract...

    let automateTxr = AutomateFunction(RepaymentRatio = bigint repaymentRatioExpected, TargetRatio = bigint targetRatioExpected,
                                BoostRatio = bigint boostRatioExpected, MinRedemptionRatio = bigint minRedemptionRatioExpected,
                                AutomationFeePerc = bigint automationFeePercExpected, RiskLimit = bigint riskLimitExpected)
                    |> ethConn.MakeImpersonatedCallWithNoEther (mapInlineDataArgumentToAddress addressArgument dEthContract.Address) dEthContract.Address

    automateTxr |> shouldSucceed

    dEthContract.Query<bigint> "minRedemptionRatio" [||] |> should equal (bigint minRedemptionRatioExpected)
    dEthContract.Query<bigint> "automationFeePerc" [||] |> should equal (bigint automationFeePercExpected)
    dEthContract.Query<bigint> "riskLimit" [||] |> should equal (bigint riskLimitExpected)

    let event = automateTxr.DecodeAllEvents<AutomationSettingsChangedEventDTO>() |> Seq.map (fun i -> i.Event) |> Seq.head
    event.RepaymentRatio |> should equal <| bigint repaymentRatioExpected
    event.TargetRatio |> should equal <| bigint targetRatioExpected
    event.BoostRatio |> should equal <| bigint boostRatioExpected
    event.MinRedemptionRatio |> should equal <| bigint minRedemptionRatioExpected
    event.AutomationFeePerc |> should equal <| bigint automationFeePercExpected
    event.RiskLimit |> should equal <| bigint riskLimitExpected

[<Specification("dEth", "automate", 1)>]
[<Theory>]
[<InlineData(repaymentRatio, targetRatio, boostRatio, 1, 1, 1)>]
let ``dEth - automate - check that an unauthorised address cannot change the automation settings`` (repaymentRatioExpected:int) (targetRatioExpected:int) (boostRatioExpected:int) (minRedemptionRatioExpected:int) (automationFeePercExpected:int) (riskLimitExpected:int) = 
    restore ()

    // ResolvedTODO:
    // 1. Catch the exception or check for it somehow, since the test is superfluous when calling via Debug
    // 2. Rather make an function to create a function that is seeded with some funds
    Debug <| EthereumConnection(hardhatURI, makeAccountWithBalance().PrivateKey)
    |> dEthContract.ExecuteFunctionFrom "automate" [|repaymentRatioExpected;targetRatioExpected;boostRatioExpected;minRedemptionRatioExpected;automationFeePercExpected;riskLimitExpected|]
    |> debug.DecodeForwardedEvents
    |> Seq.head
    |> shouldRevertWithUnknownMessage // To clarify : We get no message because the auth code reverts without providing one

// ResolvedTODO : 
// 1. Use numbers more on the order or E18
[<Specification("dEth", "redeem", 0)>]
[<Theory>]
[<InlineData(10.0, 7.0, false)>]
[<InlineData(1.0, 0.7, false)>]
[<InlineData(0.01, 0.005, false)>]
[<InlineData(0.001, 0.0005, false)>]
[<InlineData(10.0, 7.0, true)>]
[<InlineData(1.0, 0.05, true)>]
let ``dEth - redeem - check that someone with a positive balance of dEth can redeem the expected amount of Ether`` (tokensToMint:float) (tokensToRedeem:float) (riskLevelShouldBeExceeded:bool) =
    restore ()

    let redeemerConnection = EthereumConnection(hardhatURI, hardhatPrivKey2)

    let tokensToTransferBigInt = tokensToMint |> toE18
    let tokensToRedeemBigInt = tokensToRedeem |> toE18

    tokensToRedeemBigInt |> should lessThan tokensToTransferBigInt

    dEthContract.ExecuteFunction "transfer" [|redeemerConnection.Account.Address;tokensToTransferBigInt|] |> shouldSucceed

    let tokenBalanceBefore = balanceOf dEthContract redeemerConnection.Account.Address

    // ResolvedTODO:
    // 1. don't worry about adding "Redeem" since most tests ought to contain only one action
    let gulperBalanceBefore = getGulperEthBalance ()

    // ResolvedTODO:
    // 1. Fix this
    // 2. Mark says "change this to raio"
    // 3. riskLevelExceedCollateralRatio rename please

    if riskLevelShouldBeExceeded then
        makeRiskLimitLessThanExcessCollateral dEthContract |> shouldSucceed

    let receiverAddress = makeAccount().Address
    
    let redeemTx = redeemerConnection |> dEthContract.ExecuteFunctionFrom "redeem" [|receiverAddress;tokensToRedeemBigInt|]
    redeemTx |> shouldSucceed

    let (protocolFeeExpected, automationFeeExpected, collateralRedeemedExpected, collateralReturnedExpected) = 
        queryStateAndCalculateRedemptionValue dEthContract tokensToRedeemBigInt

    receiverAddress |> ethConn.GetEtherBalance |> should equal collateralReturnedExpected
    getGulperEthBalance () |> should equal (protocolFeeExpected + gulperBalanceBefore)

    // ResolvedTODO:
    // 1. please change this to check balanceBefore against balanceAfter + redeemedTokens
    // 2. add a new variable that is always smaller than totalTokens given to redeemingAccount
    balanceOf dEthContract redeemerConnection.Account.Address |> should equal (tokenBalanceBefore - tokensToRedeemBigInt)

    let event = redeemTx.DecodeAllEvents<RedeemedEventDTO>() |> Seq.map (fun i -> i.Event) |> Seq.head
    // ResolvedTODO: check that you check all the event values
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
let ``dEth - redeem - check that someone without a balance can never redeem Ether`` tokensAmount =
    restore ()

    // ResolvedTODO:
    // 1. This only checks that Debug cannot redeem tokens? 
    // Resolution: let's keep it this way, this way the code is much cleaner than if we do try-with stuff without Debug contract
    // We cannot change the shouldThrowOnTransactionFailures setting via RPC - so we can't do that in the runtime
    // So the only two options are:
    // 1. catching stuff with try with, squeezing internal exception out of AggregateException 
    // 2. going with Debug, and it seems more concise with Debug
    // And the functionality is the same in both cases. 
    Debug <| EthereumConnection(hardhatURI, makeAccountWithBalance().PrivateKey) // the balance is needed for gas vs for sending ether value.
    |> dEthContract.ExecuteFunctionFrom "redeem" [|makeAccount().Address;tokensAmount|]
    |> debug.DecodeForwardedEvents
    |> Seq.head
    |> shouldRevertWithMessage "ERC20: burn amount exceeds balance"

[<Specification("dEth", "squanderMyEthForWorthlessBeans", 1)>]
[<Theory>]
// ResolvedTODO:
// 1. use small values, but also use values on the E15->E20 order
// 2. Remove allowedInkDeviation
[<InlineData(100.0)>]
[<InlineData(0.001)>]
[<InlineData(0.0)>] // a test case checking that no-one providing no ether can issue themselves any deth
let ``dEth - squanderMyEthForWorthlessBeans - check that anyone providing a positive balance of Ether can issue themselves the expected amount of dEth`` (providedCollateral:float) =
    restore ()

    // ResolvedTODO:
    // 1.Consider creating *well constructed* helpers here
    // Resolution - I moved all repeating starting / helping code to the dEthTestsBase

    let providedCollateralBigInt = bigint providedCollateral

    let inkBefore = getInk ()
    let gulperBalanceBefore = getGulperEthBalance ()
    
    // ResolvedTODO:
    // 1. This will not be sufficient, you need to check the providedCollateralBigInt + excessCollateral against risk
    // 2. the code has been corrected in .sol, also correct here please. 
    dEthContract.Query<bigint> "getExcessCollateral" [||]
    |> should lessThan (dEthContract.Query<bigint> "riskLimit" [||] + providedCollateralBigInt)
    
    // ResolvedTODO:
    // 1. Make more explicit - is it explicit enough? just renamed the function
    // 2. Call before the squanderMyEthForWorthlessBeans
    let (protocolFeeExpected, automationFeeExpected, actualCollateralAddedExpected, accreditedCollateralExpected, tokensIssuedExpected) = 
        queryStateAndCalculateIssuanceAmount dEthContract providedCollateralBigInt

    let dEthRecipientAddress = ethConn.Account.Address
    let balanceBefore = balanceOf dEthContract dEthRecipientAddress

    // ResolvedTODO:
    // 1. name all tx's that are actually txrs to txr
    // 2. more descriptive name
    let squanderTxr = dEthContract.ExecuteFunctionFromAsyncWithValue providedCollateralBigInt "squanderMyEthForWorthlessBeans" [|dEthRecipientAddress|] ethConn |> runNow
    squanderTxr |> shouldSucceed

    // ResolvedTODO: (more of a preference)
    // 1. rather have before + expeced = balanceOf
    balanceOf dEthContract dEthRecipientAddress |> should equal (balanceBefore + tokensIssuedExpected)
    getInk () |> should equal (inkBefore + providedCollateralBigInt)
    getGulperEthBalance () |> should equal (gulperBalanceBefore + protocolFeeExpected)

    let issuedEvent = squanderTxr.DecodeAllEvents<IssuedEventDTO>() |> Seq.map (fun i -> i.Event) |> Seq.head

    // ResolvedTODO:
    // 1. Reorder in same order as .sol
    issuedEvent.Receiver |> shouldEqualIgnoringCase dEthRecipientAddress
    issuedEvent.SuppliedCollateral |> should equal providedCollateralBigInt
    issuedEvent.ProtocolFee |> should equal protocolFeeExpected
    issuedEvent.AutomationFee |> should equal automationFeeExpected
    issuedEvent.ActualCollateralAdded |> should equal actualCollateralAddedExpected
    issuedEvent.AccreditedCollateral |> should equal accreditedCollateralExpected
    issuedEvent.TokensIssued |> should equal tokensIssuedExpected

[<Specification("dEth", "squanderMyEthForWorthlessBeans", 2)>]
[<Fact>]
let ``dEth - squanderMyEthForWorthlessBeans - check that the riskLevel cannot be exceeded`` () =
    restore ()

    // ResolvedTODO:
    // Please write in a cleared method.

    makeRiskLimitLessThanExcessCollateral dEthContract |> shouldSucceed

    debug
    |> dEthContract.ExecuteFunctionFrom "squanderMyEthForWorthlessBeans" [|makeAccount().Address|]
    |> debug.DecodeForwardedEvents
    |> Seq.head
    |> shouldRevertWithMessage "risk limit exceeded"