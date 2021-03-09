module Messages

open Shared

module Home =

    type Msg =
    | UpdateDebug of string option

module WordInterop =

    type Msg =
    | UpdateDebug of string option
    | TryWord

module Dev =

    type Msg =
    | LogResults            of string
    | LogInfo               of string
    | LogError              of exn
    | UpdateLastFullError   of exn option

type Msg =
| UpdateAppVersion      of string
// Utils
| Batch                 of seq<Msg>
// Style
| ToggleNavbarBurger
| UpdateActivePage      of Routing.Route option
// Submodel-Messages
| DevMsg                of Dev.Msg
| HomeMsg               of Home.Msg
| WordInteropMsg        of WordInterop.Msg