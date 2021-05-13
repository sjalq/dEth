module dEthTestsBase

open TestBase
open Nethereum.Util
open System.Numerics
open DETH2.Contracts.MCDSaverProxy.ContractDefinition
open DETH2.Contracts.VatLike.ContractDefinition
open DEth.Contracts.IMakerOracleAdvanced.ContractDefinition
open Nethereum.JsonRpc.Client

type GiveFunctionCdp = DETH2.Contracts.ManagerLike.ContractDefinition.GiveFunction

module Array = 
    let removeFromEnd elem = Array.rev >> Array.skipWhile (fun i -> i = elem) >> Array.rev

let protocolFeePercent = bigint 9 * BigInteger.Pow(bigint 10, 15)
let hundredPerc = BigInteger.Pow(bigint 10, 18)

let RAY = BigInteger.Pow(bigint 10, 27);
let rdiv x y = (x * RAY + y / bigint 2) / y;

let WAD = BigInteger.Pow(bigint 10, 18);
let wmul x y = ((x * y) + WAD / bigint 2) / WAD 

let dEthMainnetOwner = "0xb7c6bb064620270f8c1daa7502bcca75fc074cf4"
let dEthMainnet = "0x5420dFecFaCcDAE68b406ce96079d37743Aa11Ae"

let gulper = "0xa3cC915E9f1f81185c8C6efb00f16F100e7F07CA"
let proxyCache = "0x271293c67E2D3140a0E9381EfF1F9b01E07B0795"
let cdpId = bigint 18963 // https://defiexplore.com/cdp/18963
let makerManager = "0x5ef30b9986345249bc32d8928B7ee64DE9435E39"
let ethGemJoin = "0x2F0b23f53734252Bda2277357e97e1517d6B042A"
let saverProxy = "0xC563aCE6FACD385cB1F34fA723f412Cc64E63D47"
let saverProxyActions = "0x82ecD135Dce65Fbc6DbdD0e4237E0AF93FFD5038"
let initialRecipient = "0xb7c6bb064620270f8c1daa7502bcca75fc074cf4"
[<Literal>]
let foundryTreasury = "0x93fE7D1d24bE7CB33329800ba2166f4D28Eaa553"
let dsGuardFactory = "0x5a15566417e6C1c9546523066500bDDBc53F88C7"
let cdpOwner = "0xBA1a28b8c69Bdb92d0c898A0938cd2814dc2cA5A"
let cat = "0xa5679c04fc3d9d8b0aab1f0ab83555b301ca70ea" // the contract that bites
let vat = "0x35d1b3f3d7966a1dfe207aa4514c12a259a0492b"
let spot = "0x65C79fcB50Ca1594B025960e539eD7A9a6D434A3"
let ilk = "ETH-A"
let ilkPIPAuthority = "0xBE8E3e3618f7474F8cB1d074A26afFef007E98FB"
let ilkFlipper = "0xF32836B9E1f47a0515c6Ec431592D5EbC276407f"
let ilkFlipperAuthority = "0xBE8E3e3618f7474F8cB1d074A26afFef007E98FB"
let daiMainnet = "0x6b175474e89094c44da98b954eedeac495271d0f"

let makeOracle makerOracle daiUsd ethUsd = makeContract [| makerOracle;daiUsd;ethUsd |] "Oracle"

let makerOracle = makeContract [||] "MakerOracleMock"
let daiUsdOracle = makeContract [||] "ChainLinkPriceOracleMock"
let ethUsdOracle = makeContract [||] "ChainLinkPriceOracleMock"
let makerOracleMainnet = "0x729D19f657BD0614b4985Cf1D82531c67569197B"
let daiUsdMainnet = "0xAed0c38402a5d19df6E4c03F4E2DceD6e29c1ee9"
let ethUsdMainnet = "0x5f4eC3Df9cbd43714FE2740f5E3616155c5b8419"
let oracleContract = makeOracle makerOracle.Address daiUsdOracle.Address ethUsdOracle.Address
let oracleContractMainnet = makeOracle makerOracleMainnet daiUsdMainnet ethUsdMainnet

let toMakerPriceFormatDecimal (a:decimal) = (new BigDecimal(a) * (BigDecimal.Pow(10.0, 18.0))).Mantissa
let toMakerPriceFormat = decimal >> toMakerPriceFormatDecimal

// 8 places
let toChainLinkPriceFormatDecimal (a:decimal) = (new BigDecimal(a) * (BigDecimal.Pow(10.0, 8.0))).Mantissa
let toChainLinkPriceFormatInt (a:int) = toChainLinkPriceFormatDecimal <| decimal a

let initOracles priceMaker priceDaiUsd priceEthUsd =
    makerOracle.ExecuteFunction "setData" [|toMakerPriceFormat priceMaker|] |> ignore
    daiUsdOracle.ExecuteFunction "setData" [|toChainLinkPriceFormatDecimal priceDaiUsd|] |> ignore
    ethUsdOracle.ExecuteFunction "setData" [|toChainLinkPriceFormatDecimal priceEthUsd|] |> ignore

// percent is normalized to range [0, 1]
let initOraclesDefault percentDiffNormalized =
    let priceMaker = 10 // can be random value
    let priceDaiUsd = 5 // can be random value
    let priceNonMakerDaiEth = (decimal priceMaker + (decimal priceMaker) * percentDiffNormalized)
    let priceEthUsd = priceNonMakerDaiEth / decimal priceDaiUsd
    
    initOracles (decimal priceMaker) (decimal priceDaiUsd) priceEthUsd

    decimal priceMaker, decimal priceDaiUsd, priceNonMakerDaiEth, priceEthUsd

let getDEthContractFromOracle (oracleContract:ContractPlug) initialRecipientIsTestAccount =
    let initialRecipient = if initialRecipientIsTestAccount then ethConn.Account.Address else initialRecipient

    let contract = makeContract [|
        gulper;proxyCache;cdpId;makerManager;ethGemJoin;
        saverProxy;saverProxyActions;oracleContract.Address;
        initialRecipient;dsGuardFactory;foundryTreasury|] "dEth"

    let authorityAddress = contract.Query<string> "authority" [||]
    let authority = ContractPlug(ethConn, getABI "DSAuthority", authorityAddress)

    authority, contract

let getDEthContractAndAuthority () = 
    getDEthContractFromOracle oracleContractMainnet false

let getDEthContract () = 
    let _, contract = getDEthContractAndAuthority ()
    contract

let getDEthContractEthConn () =
    let _, contract = getDEthContractFromOracle oracleContractMainnet true
    do ethConn.Web3.Client.SendRequestAsync(new RpcRequest(0, "hardhat_impersonateAccount", dEthMainnet)) |> runNowWithoutResult
    do callFunctionWithoutSigning dEthMainnet makerManager (GiveFunctionCdp(Cdp = cdpId, Dst = contract.Address)) |> ignore
    contract    

let getMockDSValueFormat (priceFormatted:BigInteger) =
    let mockDSValue = makeContract [||] "DSValueMock"
    mockDSValue.ExecuteFunction "setData" [|priceFormatted |] |> ignore
    mockDSValue

let getMockDSValue price = toMakerPriceFormat price |> getMockDSValueFormat

let getManuallyComputedCollateralValues (oracleContract: ContractPlug) saverProxy (cdpId:bigint) =
    let priceEthDai = (oracleContract.Query<bigint> "getEthDaiPrice") [||]
    let priceRay = BigInteger.Multiply(BigInteger.Pow(bigint 10, 9), priceEthDai)
    let saverProxy = ContractPlug(ethConn, getABI "MCDSaverProxy", saverProxy)
    let cdpDetailedInfoOutput = saverProxy.QueryObj<GetCdpDetailedInfoOutputDTO> "getCdpDetailedInfo" [|cdpId|]
    let collateralDenominatedDebt = rdiv cdpDetailedInfoOutput.Debt priceRay
    let excessCollateral = cdpDetailedInfoOutput.Collateral - collateralDenominatedDebt

    (priceEthDai, priceRay, saverProxy, cdpDetailedInfoOutput, collateralDenominatedDebt, excessCollateral)

let getInkAndUrnFromCdp (cdpManagerContract:ContractPlug) cdpId =
        let ilkBytes = cdpManagerContract.Query<byte[]> "ilks" [|cdpId|] |> Array.removeFromEnd (byte 0)
        let urn = cdpManagerContract.Query<string> "urns" [|cdpId|]
        (ilkBytes, urn)

let findActiveCDP ilkArg =
    let cdpManagerContract = ContractPlug(ethConn, getABI "IMakerManagerAdvanced", makerManager)
    let vatContract = ContractPlug(ethConn, getABI "VatLike", vat)
    let maxCdpId = cdpManagerContract.Query<bigint> "cdpi" [||]
    
    let cdpIds = ( Seq.initInfinite (fun i -> maxCdpId - bigint i) ) |> Seq.takeWhile (fun i -> i > BigInteger.Zero)

    let getInkAndUrnFromCdp = getInkAndUrnFromCdp cdpManagerContract

    let cdpId = Seq.findBack (fun cdpId ->
        let (ilkBytes, urn) = getInkAndUrnFromCdp cdpId
        let ilk = System.Text.Encoding.UTF8.GetString(ilkBytes)

        let urnsOutput = vatContract.QueryObj<UrnsOutputDTO> "urns" [|ilk;urn|]

        urnsOutput.Art <> bigint 0 && urnsOutput.Ink <> bigint 0 && ilk = ilkArg) cdpIds

    getInkAndUrnFromCdp cdpId

let pokePIP pipAddress = 
    do ethConn.TimeTravel <| Constants.hours * 2UL
    do callFunctionWithoutSigning ilkPIPAuthority pipAddress (PokeFunction()) |> ignore

let calculateRedemptionValue tokensToRedeem totalSupply excessCollateral automationFeePerc =
    let redeemTokenSupplyPerc = tokensToRedeem * hundredPerc / totalSupply
    let collateralAffected = excessCollateral * redeemTokenSupplyPerc / hundredPerc
    let protocolFee = collateralAffected * protocolFeePercent / hundredPerc
    let automationFee = collateralAffected * automationFeePerc / hundredPerc;
    let collateralRedeemed = collateralAffected - automationFee; // how much capital should exit the dEth contract
    let collateralReturned = collateralAffected - protocolFee - automationFee;

    (protocolFee, automationFee, collateralRedeemed, collateralReturned)

let getRedemptionValue (dEthContract:ContractPlug) tokensAmount =
    let dEthQuery name = dEthContract.Query<bigint> name [||]
    calculateRedemptionValue tokensAmount (dEthQuery "totalSupply") (dEthQuery "getExcessCollateral") (dEthQuery "automationFeePerc")

let calculateIssuanceAmount suppliedCollateral automationFeePerc excessCollateral totalSupply =
    let protocolFee = suppliedCollateral * protocolFeePercent / hundredPerc
    let automationFee = suppliedCollateral * automationFeePerc / hundredPerc
    let actualCollateralAdded = suppliedCollateral - protocolFee; // _protocolFee goes to the protocol 
    let accreditedCollateral = actualCollateralAdded - automationFee; // _automationFee goes to the pool of funds in the cdp to offset gas implications
    let newTokenSupplyPerc = accreditedCollateral * hundredPerc / excessCollateral
    let tokensIssued = totalSupply * newTokenSupplyPerc / hundredPerc
    (protocolFee, automationFee, actualCollateralAdded, accreditedCollateral, tokensIssued)

let impersonateAccount (address:string) =
    ethConn.Web3.Client.SendRequestAsync(new RpcRequest(0, "hardhat_impersonateAccount", address)) |> runNowWithoutResult

let getAddressFromArg arg contractAddress = 
    match arg with 
      | "owner" -> ethConn.Account.Address
      | "contract" -> contractAddress
      | _ -> arg