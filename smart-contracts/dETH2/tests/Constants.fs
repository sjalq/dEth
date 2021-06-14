module Constants
open System.Numerics

type EthAddress(rawString: string) =
    static member Zero = "0x0000000000000000000000000000000000000000"
    member _.StringValue = rawString.ToLower()

let minutes = 60UL
let hours = 60UL * minutes
let days = 24UL * hours

let one = BigInteger.One

let E18 = BigInteger.Pow(bigint 10, 18)

[<Literal>]
let ownerArg = "owner"
[<Literal>]
let contractArg = "contract"