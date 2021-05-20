module Program

[<EntryPoint>]
let main _ =
    TestBase.ethConn.MakeSnapshot () |> ignore
    TestBase.ethConn.RestoreSnapshot ()
    0