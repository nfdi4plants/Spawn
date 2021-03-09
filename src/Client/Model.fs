module Model

open Shared

module Logging =

    open Fable.React
    open Fable.React.Props

    type LogTypes =
    | LogError
    | LogResult
    | LogInfo
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
        LogType     : LogTypes
        Message     : string
        DateTime    : System.DateTime
    } with
        static member create logtype msg dateTime = {
            LogType     = logtype
            Message     = msg
            DateTime    = dateTime
        }

        member this.toTableRow =
            match this.LogType with
            | LogResult ->
                tr [] [
                    td [] [str (sprintf "[%s]" (this.DateTime.ToShortTimeString()))]
                    td [Style [Color NFDIColors.Mint.Base; FontWeight "bold"]] [str "Result"]
                    td [] [str this.Message]
                ]
            | LogInfo ->
                tr [] [
                    td [] [str (sprintf "[%s]" (this.DateTime.ToShortTimeString()))]
                    td [Style [Color NFDIColors.LightBlue.Base; FontWeight "bold"]] [str "info"]
                    td [] [str this.Message]
                ]
            | LogError ->
                tr [] [
                    td [] [str (sprintf "[%s]" (this.DateTime.ToShortTimeString()))]
                    td [Style [Color NFDIColors.Red.Base; FontWeight "bold"]] [str "ERROR"]
                    td [] [str this.Message]
                ]

type DevState = {
    Logs            : Logging.LogItem list
    LastFullError   : System.Exception option
} with
    static member init() = {
        Logs            = []
        LastFullError   = None
    }

type SiteStyleState = {
    BurgerVisible   : bool
    ColorMode       : WordColors.ColorMode
} with
    static member init() = {
        BurgerVisible   = false
        ColorMode       = WordColors.colorfullMode
    }

type PersistentStorage = {
    AppVersion: string
} with
    static member init() = {
        AppVersion = ""
    }

module WordInterop =

    type Model = {
        Loading: bool
        Debug: string option
    } with
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
    DevState                : DevState
    PersistentStorage       : PersistentStorage
    Todos                   : Todo list
    Input                   : string
    ActivePage              : Routing.Route option
    // Subpage models
    HomeModel               : Home.Model
    WordInteropModel        : WordInterop.Model
}

