module Program

open dEthTestsBase

[<EntryPoint>]
let main _ =
    //let s = dEthTests.``dEth - automate - check that an authorised address can change the automation settings`` "owner" 180 220 220 1 1 1
    //let s = dEthTests.``dEth - automate - check that an authorised address can change the automation settings`` "contract" 180 220 220 1 1 1
    let s = dEthTests.``dEth - automate - check that an authorised address can change the automation settings`` foundryTreasury 180 220 220 1 1 1
    let id = TestBase.ethConn.MakeSnapshot ()
    TestBase.ethConn.RestoreSnapshot ()
    0