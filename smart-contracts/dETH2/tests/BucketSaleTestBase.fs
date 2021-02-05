module BucketSaleTestBase

open TestBase
open System.Numerics
open FsUnit.Xunit
open Foundry.Contracts.BucketSale.ContractDefinition
open Constants
open Foundry.Contracts.Debug.ContractDefinition

let makeToken name code receiver supply =
    let abi = Abi("../../../../build/contracts/TestToken.json")

    let deployTxReceipt =
        ethConn.DeployContractAsync abi
            [| name
               code
               receiver
               supply |]
        |> runNow

    let result = ContractPlug(ethConn, abi, deployTxReceipt.ContractAddress)
    result.Query "balanceOf" [| receiver |] |> should equal (supply * BigInteger(1000000000000000000UL))
    result


let DAI = makeToken "MCD DAI stable coin" "DAI" ethConn.Account.Address (bucketSupply * bucketCount * (BigInteger 100UL))


let FRY = makeToken "Foundry logistics token" "FRY" ethConn.Account.Address (BigInteger 1000000UL)


let referrerReward referrerAddress amount =
    if referrerAddress = EthAddress.Zero then
        BigInteger.Zero
    else
        ((amount / BigInteger 1000000000000000000UL) + BigInteger 10000UL)
        

let makeTreasury owner = 
    let abi = Abi("../../../../build/contracts/Forwarder.json")
    
    let deployTxReceipt =
        ethConn.DeployContractAsync abi
            [| owner |]
        |> runNow

    ContractPlug(ethConn, abi, deployTxReceipt.ContractAddress)


let treasury = makeTreasury ethConn.Account.Address


let makeBucketSale treasury startOfSale bucketPeriod bucketSupply bucketCount tokenOnSale tokenSoldFor timeTravel =
    let abi = Abi("../../../../build/contracts/BucketSale.json")
    
    let deployTxReceipt =
        ethConn.DeployContractAsync abi
            [| treasury; startOfSale; bucketPeriod; bucketSupply; bucketCount; tokenOnSale; tokenSoldFor |]
        |> runNow
    ethConn.TimeTravel timeTravel
    ContractPlug(ethConn, abi, deployTxReceipt.ContractAddress)


let bucketSale = 
    makeBucketSale 
        treasury.Address 
        startOfSale 
        bucketPeriod 
        bucketSupply 
        bucketCount 
        FRY.Address 
        DAI.Address
        (BigInteger(0UL * days))
    

let addFryMinter newMinter =
    let isMinter = FRY.Query "isMinter" [| newMinter |]
    if not isMinter then
        let addFryMinterTxReceipt =  FRY.ExecuteFunction "addMinter" [| newMinter |]
        addFryMinterTxReceipt |> shouldSucceed
    else
        ()

    FRY.Query "isMinter" [| newMinter |] |> should equal true

let renounceFryMinter minter =
    let renounceFryMinterTxReceipt =  FRY.ExecuteFunction "renounceMinter" [| minter |]
    renounceFryMinterTxReceipt |> shouldSucceed

let seedWithDAI (recipient:string) (amount:BigInteger) =
    let balanceBefore = DAI.Query "balanceOf" [| recipient |] 
    let transferDaiTxReceipt = DAI.ExecuteFunction "transfer" [| recipient; amount |]
    transferDaiTxReceipt |> shouldSucceed
    DAI.Query "balanceOf" [| recipient |] |> should equal (balanceBefore + amount)

let approveDAIFor (amount:BigInteger) (approvedAddress:string) (sender:IAsyncTxSender) =
    seedWithDAI debug.ContractPlug.Address amount
    let approveDaiTxReceipt = DAI.ExecuteFunctionFrom "approve" [| approvedAddress; amount |] sender
    approveDaiTxReceipt |> shouldSucceed

let enterBucketAndValidateState sender buyer bucketToEnter valueToEnter referrer =
    valueToEnter |> should greaterThan BigInteger.Zero
    seedWithDAI debug.ContractPlug.Address valueToEnter
    approveDAIFor valueToEnter bucketSale.Address debug

    let referrerReferredTotalBefore = bucketSale.Query "referredTotal" [| referrer |]
    let referrerRewardPercBefore = bucketSale.Query "referrerReferralRewardPerc" [| referrer |]
    let calculatedReferrerRewardPercBefore = referrerReward referrer referrerReferredTotalBefore
    referrerRewardPercBefore |> should equal calculatedReferrerRewardPercBefore
    let senderDaiBalanceBefore = DAI.Query "balanceOf" [| debug.ContractPlug.Address |]
    let treasuryDaiBalanceBefore = DAI.Query "balanceOf" [| treasury.Address |]
    let buyForBucketBefore = bucketSale.QueryObj<BuysOutputDTO> "buys" [| bucketToEnter; buyer |]
    let buyerRewardBuyBefore = bucketSale.QueryObj<BuysOutputDTO> "buys" [| bucketToEnter + BigInteger.One; buyer |]
    let referrerRewardBuyBefore = bucketSale.QueryObj<BuysOutputDTO> "buys" [| bucketToEnter + BigInteger.One; referrer |]
    let bucketBefore = bucketSale.QueryObj<BucketsOutputDTO> "buckets" [| bucketToEnter |]
    let referralBucketBefore = bucketSale.QueryObj<BucketsOutputDTO> "buckets" [| bucketToEnter + BigInteger.One |]

    // act
    let receipt = bucketSale.ExecuteFunctionFrom "agreeToTermsAndConditionsListedInThisContractAndEnterSale" [| buyer; bucketToEnter; valueToEnter; referrer |] debug

    // assert
    let forwardedEvent = decodeFirstEvent<ForwardedEventDTO> receipt
    let revertMessage = forwardedEvent.ResultAsRevertMessage
    forwardedEvent.Success |> should equal true
    receipt |> shouldSucceed

    // event validation
    let referredTotalAfter = bucketSale.Query "referredTotal" [| referrer |]
    referredTotalAfter |> should equal (referrerReferredTotalBefore + valueToEnter)
    let calculatedReferrerRewardPercAfter = referrerReward referrer referredTotalAfter
    let referrerRewardPercAfter = bucketSale.Query "referrerReferralRewardPerc" [| referrer |]
    referrerRewardPercAfter |> should equal calculatedReferrerRewardPercAfter
    let buyerReward = valueToEnter / BigInteger 10UL
    let referrerReward = valueToEnter * calculatedReferrerRewardPercAfter / BigInteger 100000

    let enteredEvent = receipt |> decodeFirstEvent<EnteredEventDTO>
    if referrer <> EthAddress.Zero then
        enteredEvent.BucketId |> should equal bucketToEnter
        enteredEvent.Buyer |> shouldEqualIgnoringCase buyer
        enteredEvent.BuyerReferralReward |> should equal buyerReward
        enteredEvent.Sender |> shouldEqualIgnoringCase debug.ContractPlug.Address
        enteredEvent.Referrer |> shouldEqualIgnoringCase referrer
        enteredEvent.ReferrerReferralReward |> should equal referrerReward
        enteredEvent.ValueEntered |> should equal valueToEnter
    else
        enteredEvent.BucketId |> should equal bucketToEnter
        enteredEvent.Buyer |> shouldEqualIgnoringCase buyer
        enteredEvent.BuyerReferralReward |> should equal BigInteger.Zero
        enteredEvent.Sender |> shouldEqualIgnoringCase debug.ContractPlug.Address
        enteredEvent.Referrer |> shouldEqualIgnoringCase EthAddress.Zero
        enteredEvent.ReferrerReferralReward |> should equal BigInteger.Zero
        enteredEvent.ValueEntered |> should equal valueToEnter

    // state validation
    bucketSale.Query "treasury" [||] |> shouldEqualIgnoringCase treasury.Address
    bucketSale.Query "startOfSale" [||] |> should equal startOfSale
    bucketSale.Query "bucketPeriod" [||] |> should equal bucketPeriod
    bucketSale.Query "bucketSupply" [||] |> should equal bucketSupply
    bucketSale.Query "bucketCount" [||] |> should equal bucketCount
    bucketSale.Query "tokenOnSale" [||] |> shouldEqualIgnoringCase FRY.Address
    bucketSale.Query "tokenSoldFor" [||] |> shouldEqualIgnoringCase DAI.Address

    let senderDaiBalanceAfter = DAI.Query "balanceOf" [| debug.ContractPlug.Address |]
    senderDaiBalanceAfter |> should equal (senderDaiBalanceBefore - valueToEnter)
    let treasuryDaiBalanceAfter = DAI.Query "balanceOf" [| treasury.Address |]
    treasuryDaiBalanceAfter |> should equal (treasuryDaiBalanceBefore + valueToEnter)

    let buyForBucketAfter = bucketSale.QueryObj<BuysOutputDTO> "buys" [| bucketToEnter; buyer |]
    buyForBucketAfter.ValueEntered |> should equal (buyForBucketBefore.ValueEntered + valueToEnter)
    buyForBucketAfter.BuyerTokensExited |> should equal BigInteger.Zero

    let bucketAfter = bucketSale.QueryObj<BucketsOutputDTO> "buckets" [| bucketToEnter |]
    bucketAfter.TotalValueEntered |> should equal (bucketBefore.TotalValueEntered + valueToEnter)

    if referrer <> EthAddress.Zero then
        let buyerRewardBuyAfter = bucketSale.QueryObj<BuysOutputDTO> "buys" [| bucketToEnter + BigInteger.One; buyer |]
        buyerRewardBuyAfter.ValueEntered |> should equal (buyerRewardBuyBefore.ValueEntered + buyerReward)
        buyerRewardBuyAfter.BuyerTokensExited |> should equal BigInteger.Zero

        let referrerRewardBuyForBucketAfter = bucketSale.QueryObj<BuysOutputDTO> "buys" [| bucketToEnter + BigInteger.One; referrer |]
        referrerRewardBuyForBucketAfter.ValueEntered |> should equal (referrerRewardBuyBefore.ValueEntered + referrerReward)
        referrerRewardBuyForBucketAfter.BuyerTokensExited |> should equal BigInteger.Zero

        let referralBucketAfter = bucketSale.QueryObj<BucketsOutputDTO> "buckets" [| bucketToEnter + BigInteger.One |]
        referralBucketAfter.TotalValueEntered |> should equal (referralBucketBefore.TotalValueEntered + referrerReward + buyerReward)
    else
        let buyerRewardBuyAfter = bucketSale.QueryObj<BuysOutputDTO> "buys" [| bucketToEnter + BigInteger.One; buyer |]
        buyerRewardBuyAfter.ValueEntered |> should equal buyerRewardBuyBefore.ValueEntered
        buyerRewardBuyAfter.BuyerTokensExited |> should equal BigInteger.Zero

        let referrerRewardBuyForBucketAfter = bucketSale.QueryObj<BuysOutputDTO> "buys" [| bucketToEnter + BigInteger.One; referrer |]
        referrerRewardBuyForBucketAfter.ValueEntered |> should equal referrerRewardBuyBefore.ValueEntered
        referrerRewardBuyForBucketAfter.BuyerTokensExited |> should equal BigInteger.Zero

        let referralBucketAfter = bucketSale.QueryObj<BucketsOutputDTO> "buckets" [| bucketToEnter + BigInteger.One |]
        referralBucketAfter.TotalValueEntered |> should equal (referralBucketBefore.TotalValueEntered)

        
let exitBucketAndValidateState buyer bucketToExit valueEntered =
    let buyerBalanceBefore = FRY.Query "balanceOf" [| buyer |]
    let totalSupplyBefore = FRY.Query "totalSupply" [||]
    let totalTokensExitedBefore = bucketSale.Query "totalExitedTokens" [||]

    let exitBucketReceipt = bucketSale.ExecuteFunction "exit" [| bucketToExit; buyer |] 
    exitBucketReceipt |> shouldSucceed

    let bucket = bucketSale.QueryObj<BucketsOutputDTO> "buckets" [| bucketToExit |]
    let amountToExit = 
        (bucketSupply * (BigInteger 100000UL) * valueEntered) / (bucket.TotalValueEntered * (BigInteger 100000UL))

    let exitEvent = exitBucketReceipt |> decodeFirstEvent<ExitedEventDTO>
    exitEvent.BucketId |> should equal bucketToExit
    exitEvent.Buyer |> shouldEqualIgnoringCase buyer
    exitEvent.TokensExited |> should equal amountToExit

    let buy = bucketSale.QueryObj<BuysOutputDTO> "buys" [| bucketToExit; buyer |]
    buy.BuyerTokensExited |> should equal amountToExit
    buy.ValueEntered |> should equal valueEntered

    let totalTokensExitedAfter = bucketSale.Query "totalExitedTokens" [||]
    totalTokensExitedAfter |> should equal (totalTokensExitedBefore + amountToExit) 
    
    let buyerBalanceAfter = FRY.Query "balanceOf" [| buyer |]
    buyerBalanceAfter |> should equal (buyerBalanceBefore + amountToExit)

    let totalSupplyAfter = FRY.Query "totalSupply" [||]
    totalSupplyAfter |> should equal (totalSupplyBefore + amountToExit)