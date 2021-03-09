module Model

open Shared


module Logging =

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
                | LogError   -> NFDIColors.Red.Base
                | LogResult  -> NFDIColors.Mint.Base
                | LogInfo    -> NFDIColors.LightBlue.Base

            static member ofString (str:string) =
                match str with
                | "Error" | "error"     -> LogError
                | "Info" | "info"       -> LogInfo
                | "Result"| "result"    -> LogResult
                | others -> failwith (sprintf "Swate found an unexpected log identifier: %s" others)

    type LogItem = {
        LogType : LogTypes
        Message : string
    } with
        static member create logtype msg = {
            LogType = logtype
            Message = msg
        }

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
    Logs                    : Logging.LogItem list
    ActivePage              : Routing.Route option
    // Subpage models
    HomeModel               : Home.Model
    WordInteropModel        : WordInterop.Model
}

