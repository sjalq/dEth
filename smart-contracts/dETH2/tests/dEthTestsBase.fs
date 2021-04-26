module dEthTestsBase

open TestBase
open Nethereum.Util
open System.Numerics
open DETH2.Contracts.MCDSaverProxy.ContractDefinition

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
let dsGuardFactory = "0x5a15566417e6C1c9546523066500bDDBc53F88C7"
let foundryTreasury = "0x93fE7D1d24bE7CB33329800ba2166f4D28Eaa553"
let cdpOwner = "0xBA1a28b8c69Bdb92d0c898A0938cd2814dc2cA5A"

let makeOracle makerOracle daiUsd ethUsd = makeContract [| makerOracle;daiUsd;ethUsd |] "Oracle"

let makerOracle = makeContract [||] "MakerOracleMock"
let daiUsdOracle = makeContract [||] "ChainLinkPriceOracleMock"
let ethUsdOracle = makeContract [||] "ChainLinkPriceOracleMock"
let makerOracleMainnet = "0x729D19f657BD0614b4985Cf1D82531c67569197B"
let daiUsdMainnet = "0xAed0c38402a5d19df6E4c03F4E2DceD6e29c1ee9"
let ethUsdMainnet = "0x5f4eC3Df9cbd43714FE2740f5E3616155c5b8419"
let oracleContract = makeOracle makerOracle.Address daiUsdOracle.Address ethUsdOracle.Address
let oracleContractMainnet = makeOracle makerOracleMainnet daiUsdMainnet ethUsdMainnet

// 18 places
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

let getDEthContractFromOracle (oracleContract:ContractPlug) =     
    let contract = makeContract [|
        gulper;proxyCache;cdpId;makerManager;ethGemJoin;
        saverProxy;saverProxyActions;oracleContract.Address;
        initialRecipient;dsGuardFactory;foundryTreasury|] "dEth"

    let authorityAddress = contract.Query<string> "authority" [||]
    let authority = ContractPlug(ethConn, getABI "DSAuthority", authorityAddress)

    authority, contract

let getDEthContractAndAuthority () = 
    getDEthContractFromOracle oracleContractMainnet

let getDEthContract () = 
    let _, contract = getDEthContractAndAuthority ()
    contract

let getManuallyComputedCollateralValues (oracleContract: ContractPlug) saverProxy (cdpId:bigint) =
    let priceEthDai = (oracleContract.Query<bigint> "getEthDaiPrice") [||]
    let priceRay = BigInteger.Multiply(BigInteger.Pow(bigint 10, 9), priceEthDai)
    let saverProxy = ContractPlug(ethConn, getABI "MCDSaverProxy", saverProxy)
    let cdpDetailedInfoOutput = saverProxy.QueryObj<GetCdpDetailedInfoOutputDTO> "getCdpDetailedInfo" [|cdpId|]
    let collateralDenominatedDebt = rdiv cdpDetailedInfoOutput.Debt priceRay
    let excessCollateral = cdpDetailedInfoOutput.Collateral - collateralDenominatedDebt

    (priceEthDai, priceRay, saverProxy, cdpDetailedInfoOutput, collateralDenominatedDebt, excessCollateral)