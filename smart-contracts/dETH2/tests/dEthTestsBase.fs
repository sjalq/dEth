module dEthTestsBase

open TestBase
open Xunit
open FsUnit.Xunit
open Nethereum.Web3
open System.Numerics
open Nethereum.Util

let makeOracle makerOracle daiUsd ethUsd =
    let abi = Abi("../../../../build/contracts/Oracle.json")
    makeContract [| makerOracle;daiUsd;ethUsd |] abi

let makerOracle = makeContract [||] <| Abi(__SOURCE_DIRECTORY__+ "/../build/contracts/MakerOracleMock.json")
let daiUsdOracle = makeContract [||] <| Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/ChainLinkPriceOracleMock.json")
let ethUsdOracle = makeContract [||] <| Abi(__SOURCE_DIRECTORY__ + "/../build/contracts/ChainLinkPriceOracleMock.json")
let oracleContract = makeOracle makerOracle.Address daiUsdOracle.Address ethUsdOracle.Address

// 18 places
let toMakerPriceFormatDecimal (a:decimal) = (new BigDecimal(a) * (BigDecimal.Pow(10.0, 18.0))).Mantissa
let toMakerPriceFormat = decimal >> toMakerPriceFormatDecimal

// 8 places
let toChainLinkPriceFormatDecimal (a:decimal) = (new BigDecimal(a) * (BigDecimal.Pow(10.0, 8.0))).Mantissa
let toChainLinkPriceFormatInt (a:int) = toChainLinkPriceFormatDecimal <| decimal a

let initOracles priceMaker priceDaiUsd priceEthUsd = 
    makerOracle.ExecuteFunction "setData" [|toMakerPriceFormat priceMaker|] |> ignore
    daiUsdOracle.ExecuteFunction "setData" [|toChainLinkPriceFormatInt priceDaiUsd|] |> ignore
    ethUsdOracle.ExecuteFunction "setData" [|toChainLinkPriceFormatDecimal priceEthUsd|] |> ignore

// percent is normalized to range [0, 1]
let initOraclesDefault percentDiffNormalized = 
    let priceMaker = 10 // can be random value
    let priceDaiUsd = 5 // can be random value
    let priceNonMakerDaiEth = (decimal priceMaker + (decimal priceMaker) * percentDiffNormalized)
    let priceEthUsd = priceNonMakerDaiEth / decimal priceDaiUsd
    
    initOracles priceMaker priceDaiUsd priceEthUsd

    priceMaker, priceDaiUsd, priceNonMakerDaiEth, priceEthUsd