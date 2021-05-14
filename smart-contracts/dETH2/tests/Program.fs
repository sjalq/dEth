module Program

[<EntryPoint>]
let main _ =
    let s = dEthTests.``dEth - automate - check that an authorised address can change the automation settings`` "owner" 180 220 220 1 1 1
    let s = dEthTests.``dEth - automate - check that an authorised address can change the automation settings`` "contract" 180 220 220 1 1 1
    0