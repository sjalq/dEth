module Ex

open System

    /// Modify the exception, preserve the stacktrace and add the current stack, then throw (.NET 2.0+).
    /// This puts the origin point of the exception on top of the stacktrace.
let inline throwPreserve ex =
    let preserveStackTrace = 
        typeof<Exception>.GetMethod("InternalPreserveStackTrace")

    (ex, null) 
    |> preserveStackTrace.Invoke  // alters the exn, preserves its stacktrace
    |> ignore

    raise ex