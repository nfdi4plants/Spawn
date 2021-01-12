module Messages

open Shared

module Home =

    type Msg =
    | UpdateDebug of string option

module WordInterop =

    type Msg =
    | UpdateDebug of string option
    | TryWord

type Msg =
| UpdateAppVersion      of string
// Style
| ToggleNavbarBurger
| UpdateActivePage      of Routing.Route option
// Utils
| Batch                 of seq<Msg>
| LogResults            of string
| LogError              of exn
// Submodel-Messages
| HomeMsg               of Home.Msg
| WordInteropMsg        of WordInterop.Msg