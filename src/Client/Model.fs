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
    AppVersion          : string
    AllOntologies       : Shared.SwateTypes.DbDomain.Ontology []
} with
    static member init() = {
        AppVersion      = ""
        AllOntologies   = [||]
    }

open Shared.SwateTypes
open Thoth.Elmish

type TermSearchType =
| TermSearchHeader
| TermSearchValues
| TermSearchUnit
    member this.toInputPlaceholderText =
        match this with
        | TermSearchHeader  -> "Search term for header .."
        | TermSearchValues  -> "Search term for value .."
        | TermSearchUnit    -> "Search term for unit .."
    member this.toStrReadable =
        match this with
        | TermSearchHeader  -> "header"
        | TermSearchValues  -> "values"
        | TermSearchUnit    -> "unit"

type TermSearchState = {

    TermSearchText                  : string

    SelectedTerm                    : DbDomain.Term option
    TermSuggestions                 : DbDomain.Term []

    SearchByParentChildOntology     : bool
    HasSuggestionsLoading           : bool
    ShowSuggestions                 : bool

} with
    static member init () = {
        TermSearchText              = ""

        SelectedTerm                = None
        TermSuggestions             = [||]

        SearchByParentChildOntology = true
        HasSuggestionsLoading       = false
        ShowSuggestions             = false
    }

type BuildingBlockInfoState = {
    Id      : int
    Header  : TermSearchState
    Unit    : TermSearchState
    Values  : TermSearchState
} with
    static member init (?id) = {
        Id      = if id.IsSome then id.Value else 0
        Header  = TermSearchState.init()
        Unit    = TermSearchState.init()
        Values  = TermSearchState.init()
    }

module Process =

    type Model = {
        Loading                 : bool
        BuildingBlockInfos      : BuildingBlockInfoState list
    } with
        static member init() = {
            Loading             = false
            BuildingBlockInfos  = []
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
    ///Debouncing
    DebouncerState          : Debouncer.State
    PersistentStorage       : PersistentStorage
    ActivePage              : Routing.Route option
    // Subpage models
    HomeModel               : Home.Model
    ProcessModel            : Process.Model
} 

