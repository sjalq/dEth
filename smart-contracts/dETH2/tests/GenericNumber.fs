module GenericNumber

let inline FromZero() = LanguagePrimitives.GenericZero
let inline FromOne() = LanguagePrimitives.GenericOne
let inline FromInt32 (n:int) =
    let one : ^a = FromOne()
    let zero : ^a = FromZero()
    let n_incr = if n > 0 then 1 else -1
    let g_incr = if n > 0 then one else (zero - one)
    let rec loop i g = 
        if i = n then g
        else loop (i + n_incr) (g + g_incr)
    loop 0 zero