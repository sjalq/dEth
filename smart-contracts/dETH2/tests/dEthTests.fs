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
open DETH2.Contracts.MCDSaverProxy.ContractDefinition
open System
open System.Linq;
open Nethereum.ABI
open DEth.Contracts.IPriceFeed.ContractDefinition
open DEth.Contracts.IMedianETHUSD.ContractDefinition
open Nethereum.RPC.TransactionManagers
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

let byte12ToInt a = BitConverter.ToInt32( System.ReadOnlySpan(Array.rev a) )
let bigintToByte size (a:BigInteger) = 
    let bytes = a.ToByteArray()
    bytes |> Array.ensureSize size |> Array.rev
let bigIntToByte12 = bigintToByte 12
let bigIntToByte32 = bigintToByte 32

let strToByte32 (str:string) = System.Text.Encoding.UTF8.GetBytes(str) |> Array.ensureSize 32

[<Specification("cdp", "bite", 0)>]
[<Fact>]
let ``biting of a CDP - should bite when collateral is < 150`` () = 
    let liquidationPrice = 14.88M
    let liquidationPriceFormat = toMakerPriceFormat liquidationPrice
    let cdpId = 19800
    let makerOracleMainnetContract = ContractPlug(ethConn, getABI "IMakerOracle", makerOracleMainnet)
    let nextBytes = makerOracleMainnetContract.Query<byte[]> "next" [||]

    let next = byte12ToInt nextBytes

    printfn "next: %A" next

    let median = "0x64de91f5a373cd4c28de3600cb34c7c6ce410c85"
    let medianOwner = "0xddb108893104de4e1c6d0e47c42237db4e617acc"
    let priceFeed = "0x20eD77585Be1b2BFD6056C64AEBaD41341E35907"
    let priceFeedOwner = "0x5e90e067242363be0b4004e1a60b1d877d3d5877"

    // TODO: DRY / find a cleaner way
    ethConn.Web3.Client.SendRequestAsync(new RpcRequest(1, "hardhat_impersonateAccount", priceFeedOwner)) |> runNowWithoutResult
    ethConn.Web3.Client.SendRequestAsync(new RpcRequest(1, "hardhat_impersonateAccount", medianOwner)) |> runNowWithoutResult

    // top up eth on these accounts
    ethConn.Web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(medianOwner, 1000M) |> runNow |> ignore
    ethConn.Web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(priceFeedOwner, 1000M) |> runNow |> ignore

    let abiEncode = new ABIEncode();
    let zzz = (uint debug.BlockTimestamp)  + (uint <| Constants.hours * 3UL)
    let priceFeedArg = PostFunction(Val_ = liquidationPriceFormat, Zzz_ =  zzz, Med_ = makerOracleMainnet)
    let priceFeedData = Web3.Sha3("post").Substring(0, 8) + abiEncode.GetSha3ABIParamsEncodedPacked(priceFeedArg).ToHex();
    let priceFeedTxInput = new TransactionInput(priceFeedData, addressTo = priceFeed, addressFrom = priceFeedOwner, gas = hexBigInt 9500000UL, value = hexBigInt 0UL);

    let medianArgWithouVRS = PokeFunctionWithoutRVS(Val_ = [|liquidationPriceFormat|].ToList(), Age_ = [|bigint zzz|].ToList())
    let medianDataWithoutVRS = Web3.Sha3("post").Substring(0, 8) + abiEncode.GetSha3ABIParamsEncodedPacked(medianArgWithouVRS).ToHex();
    let medianTxInput = new TransactionInput(medianDataWithoutVRS, addressTo = median, addressFrom = medianOwner, gas = hexBigInt 9500000UL, value = hexBigInt 0UL);    
    let medianTxWithoutVRS = ethConn.Web3.Eth.Transactions.SendTransaction.SendRequestAsync(medianTxInput) |> runNow
    let transactionRpc = ethConn.Web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(medianTxWithoutVRS) |> runNow
    
    //Getting the transaction from the chain

    let medianArg = PokeFunction(Val_ = [|liquidationPriceFormat|].ToList(), Age_ = [|bigint zzz|].ToList(), V = (strToByte32 transactionRpc.V).ToList(), R = [|(strToByte32 transactionRpc.R)|].ToList(), S = [|(strToByte32 transactionRpc.S)|].ToList())
    let medianData = Web3.Sha3("poke").Substring(0, 8) + abiEncode.GetSha3ABIParamsEncodedPacked(medianArg).ToHex();
    let medianTxInput = new TransactionInput(medianData, addressTo = median, addressFrom = medianOwner, gas = hexBigInt 9500000UL, value = hexBigInt 0UL);
   
    (Web3(hardhatURI)).TransactionManager.SendTransactionAsync(medianTxInput) |> runNow |> ignore
    (Web3(hardhatURI)).TransactionManager.SendTransactionAsync(priceFeedTxInput) |> runNow |> ignore

    let currentValue = makerOracleMainnetContract.Query<bigint> "read" [||]
    printfn "currentValue: %A" currentValue