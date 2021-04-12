module Generators

open FsCheck

let hexGenerator = Gen.elements ( [0..15] |> Seq.map (fun i -> i.ToString("X").[0]) )

let addressGenerator =
    gen {
        let! items = Gen.arrayOfLength 40 <| hexGenerator
        return items |> System.String
    }   

type AddressGenerator =
  static member string() =
      {new Arbitrary<string>() with
          override x.Generator = addressGenerator
          override x.Shrinker t = Seq.empty }