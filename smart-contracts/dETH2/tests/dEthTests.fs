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
open DETH2.Contracts.DEth.ContractDefinition
open DETH2.Contracts.MCDSaverProxy.ContractDefinition;
open Nethereum.Contracts
open DETH2.Contracts.VatLike.ContractDefinition
open DETH2.Contracts.PipLike.ContractDefinition
open DETH2.Contracts.IFlipper.ContractDefinition
open DETH2.Contracts.ManagerLike.ContractDefinition
open Nethereum.Util
open System

type SpotterIlksOutputDTO = DETH2.Contracts.ISpotter.ContractDefinition.IlksOutputDTO
type VatIlksOutputDTO = DETH2.Contracts.VatLike.ContractDefinition.IlksOutputDTO
type VatUrnsOutputDTO = DETH2.Contracts.VatLike.ContractDefinition.UrnsOutputDTO

type System.String with
   member s1.icompare(s2: string) =
     System.String.Equals(s1, s2, System.StringComparison.CurrentCultureIgnoreCase);

module Array =
    let ensureSize size array =
        let paddingArray = Array.init size (fun _ -> byte 0)
        Array.concat [|array;paddingArray|] |> Array.take size

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
    let authority, contract = getDEthContractAndAuthority()

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

let strToByte32 (str:string) = System.Text.Encoding.UTF8.GetBytes(str) |> Array.ensureSize 32


// todo:
// fix the issue with hardhat_reset
// events emitted

let bigintToByte size (a:BigInteger) = 
    let bytes = a.ToByteArray()
    bytes |> Array.ensureSize size |> Array.rev

let doTimes x action = 
    for _ in 1..x do
        action ()

let inline toBigDecimal (x:BigInteger) : BigDecimal = BigDecimal(x, 0);
let inline toBigInt (x:BigDecimal) = x.Mantissa / BigInteger.Pow(bigint 10, -x.Exponent)

let bigintDifference a b (precision:int) =
    Math.Round(decimal <| toBigDecimal a / toBigDecimal b, precision)

// todo urns and other vat values checks

[<Specification("cdp", "bite", 0)>]
[<Fact>]
let ``biting of a CDP - should bite when collateral is < 150`` () =
    // set-up the test
    //ethConn.Web3.Client.SendRequestAsync(new RpcRequest(2, "hardhat_reset", [||])) |> runNowWithoutResult
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
    do callFunctionWithoutSigning ilkPIPAuthority pipAddress (KissFunction(A=oracleAdapter.Address)) |> ignore
    oracleAdapter.ExecuteFunction "setOracle" [|pipAddress|] |> shouldSucceed
    let (_, dEthContract) = getDEthContractFromOracle <| oracleAdapter

    // calculate price to make the ratio between total collateral and collateral denominated debt 145%
    let currentPrice = oracleAdapter.Query<bigint> "getEthDaiPrice" [||]
    let initialCollateralOutput = dEthContract.QueryObj<GetCollateralOutputDTO> "getCollateral" [||]
    let ratio = toBigDecimal initialCollateralOutput.TotalCollateral / toBigDecimal initialCollateralOutput.CollateralDenominatedDebt
    let wantedRatio = 1.45M
    let diff = ratio / BigDecimal wantedRatio
    let wantedPrice = (toBigDecimal currentPrice / BigDecimal(diff))
    let wantedPriceBigInt = toBigInt wantedPrice
    
    // transfer cdp from the mainnet deth to the new dEth contract
    do callFunctionWithoutSigning dEthMainnet makerManager (GiveFunction(Cdp = cdpId, Dst = dEthContract.Address)) |> ignore

    // set-up the test - end

    // STEP 1 - change price and check that excess collateral went down after price change
    let mockDSValueContract = getMockDSValueFormat wantedPriceBigInt
    do callFunctionWithoutSigning ilkPIPAuthority pipAddress (ChangeFunction(Src_ = mockDSValueContract.Address)) |> ignore

    // poke twice
    doTimes 2 <| (fun _ -> pokePIP pipAddress)
    spotterContract.ExecuteFunction "poke" [|ilk|] |> shouldSucceed

    let collateralOutputAfterPriceChange = dEthContract.QueryObj<GetCollateralOutputDTO> "getCollateral" [||]
    should be (lessThan <| initialCollateralOutput.ExcessCollateral) collateralOutputAfterPriceChange.ExcessCollateral
    should equal initialCollateralOutput.TotalCollateral collateralOutputAfterPriceChange.TotalCollateral
    let collateralDebtDiff = Math.Round(decimal (toBigDecimal collateralOutputAfterPriceChange.CollateralDenominatedDebt / toBigDecimal initialCollateralOutput.CollateralDenominatedDebt), 5)
    let priceDiff = Math.Round((decimal diff), 5)
    should equal priceDiff collateralDebtDiff

    // STEP 2 - bite, current excessCollateral should be within 0-30% of the excess collateral before biting. as the vat.grab() is called and the vault gets liquidated.
    catContract.ExecuteFunction "bite" [|ilk;urn|] |> shouldSucceed

    let collateralOutputAfterBite = dEthContract.QueryObj<GetCollateralOutputDTO> "getCollateral" [||]
    let percentDiff = bigintDifference collateralOutputAfterBite.Debt collateralOutputAfterPriceChange.Debt 4
    let percentTotalCollateral = bigintDifference collateralOutputAfterBite.TotalCollateral collateralOutputAfterPriceChange.TotalCollateral 4
    let percentTotalExcessCollateral = bigintDifference collateralOutputAfterBite.ExcessCollateral collateralOutputAfterPriceChange.ExcessCollateral 4
    let percentCollateralDenominatedDebt = bigintDifference collateralOutputAfterBite.CollateralDenominatedDebt collateralOutputAfterPriceChange.CollateralDenominatedDebt 4

    should lessThanOrEqualTo 35M percentDiff
    should equal percentDiff percentTotalCollateral
    should equal percentDiff percentTotalExcessCollateral
    should equal percentDiff percentCollateralDenominatedDebt    

    // STEP 3 - open auction to sell the ilk in cdp
    let maxAuctionLengthInSeconds = bigint 50
    let maxBidLengthInSeconds = bigint 20
    do callFunctionWithoutSigning ilkFlipperAuthority ilkFlipper (FileFunction(What = strToByte32 "tau", Data = maxAuctionLengthInSeconds)) |> ignore
    do callFunctionWithoutSigning ilkFlipperAuthority ilkFlipper (FileFunction(What = strToByte32 "ttl", Data = maxBidLengthInSeconds)) |> ignore

    let id = flipperContract.Query<bigint> "kicks" [||] // get the latest auction id
    
    let bidsOutputDTO = flipperContract.QueryObj<BidsOutputDTO> "bids" [|id|]
    let expectedLot = bidsOutputDTO.Lot - bidsOutputDTO.Lot / bigint 10M // bid for 10% less lot.

    // emit Tab DAI in the VAT for the account that is bidding.
    do callFunctionWithoutSigning spot vat <| SuckFunction(U = ethConn.Account.Address, V = ethConn.Account.Address, Rad = bidsOutputDTO.Tab) |> ignore

    vatContract.ExecuteFunction "hope" [|flipperContract.Address|] |> shouldSucceed
    flipperContract.ExecuteFunction "tend" [|id;bidsOutputDTO.Lot;bidsOutputDTO.Tab|] |> shouldSucceed
    flipperContract.ExecuteFunction "dent" [|id;expectedLot;bidsOutputDTO.Tab|] |> shouldSucceed

    ethConn.TimeTravel maxAuctionLengthInSeconds
    flipperContract.ExecuteFunction "deal" [|id|] |> shouldSucceed

    // after hope/tend/dent/deal - the values should drop to 35 percents of original or less
    // doesn't change
    let collateralOutputAfterAuctionEnd = dEthContract.QueryObj<GetCollateralOutputDTO> "getCollateral" [||]

    // STEP 4: MoveVatEthToCDP
    // need to check that vat eth was indeed moved and that excess collateral is up again.
    dEthContract.ExecuteFunction "moveVatEthToCDP" [||] |> shouldSucceed
    
    let collateralOutputAfterMoveVat = dEthContract.QueryObj<GetCollateralOutputDTO> "getCollateral" [||]
    ()
    ()