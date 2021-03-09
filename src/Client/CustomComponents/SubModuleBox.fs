module Customcomponents

open Fable.React
open Fable.React.Props

let subModuleBox children =
    div [
        Style [
            BorderLeft (sprintf "5px solid %s" NFDIColors.Mint.Base)
            Padding "0.25rem 1rem"
            MarginBottom "1rem"
        ]
    ] children