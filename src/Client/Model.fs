module Model

open Shared

//type PageModel =
//| HomeModel of Home.Model

type LogTypes =
| LogError
| LogResult
| LogInfo
    with
        member this.toReadableString =
            match this with
            | LogError   -> "Error"
            | LogResult  -> "Result"
            | LogInfo    -> "Info"

        member this.toColor =
            match this with
            | LogError   -> "hsl(348, 100%, 61%)"
            | LogResult  -> "hsl(141, 53%, 53%)"
            | LogInfo    -> "hsl(204, 86%, 53%)"

type SiteStyleState = {
    BurgerVisible: bool
}
    with
        static member init() = {
            BurgerVisible   = false
        }

type PersistentStorage = {
    AppVersion: string
}
    with
        static member init() = {
            AppVersion = ""
        }

module WordInterop =

    type Model = {
        Loading: bool
        Debug: string option
    }
        with
            static member init() = {
                Loading = false
                Debug = None
            }

module Home =

    type Model = {
        Loading: bool
        Debug: string option
    }
        with
            static member init() = {
                Loading = false
                Debug = None
            }

type Model = {
    SiteStyleState          : SiteStyleState
    PersistentStorage       : PersistentStorage
    Todos                   : Todo list
    Input                   : string
    Logs                    : (LogTypes*string) list
    ActivePage              : Routing.Route option
    // Subpage models
    HomeModel               : Home.Model
    WordInteropModel        : WordInterop.Model
}

