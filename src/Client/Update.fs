module Update

open Elmish
open Fable.Remoting.Client

open Shared
open Model
open Messages
open Routing
open Thoth.Elmish

let urlUpdate (route:Route option) (model:Model) =
    match route with
    | Some Route.Home ->
        let m, cmd = Home.init()
        let nextModel =
            { model with
                HomeModel = m
                ActivePage = Some Routing.Home }
        nextModel, Cmd.map HomeMsg cmd
    | Some Route.Process ->
        let m, cmd = Process.init()
        let nextModel =
            { model with
                ProcessModel = m
                ActivePage = Some Routing.Process }
        nextModel, cmd
    | Some Route.ActivityLog ->
        model, Cmd.none
    | Some Route.Info ->
        model, Cmd.none
    | None ->
        model, Cmd.ofMsg (exn("Could not find navigated route!") |> Dev.GenericError |> DevMsg )

module WordInterop =

    open Messages.WordInterop

    let update (msg:WordInterop.Msg) (currentModel: Model.Model): Model.Model * Cmd<Messages.Msg> =
        match msg with
        | TryWord ->
            let cmd =
                Cmd.OfPromise.either
                    OfficeInterop.exampleExcelFunction
                    ()
                    (Messages.Dev.GenericResults >> Messages.DevMsg)
                    (Messages.Dev.GenericError >> Messages.DevMsg)
            currentModel, cmd

module Dev =

    open Messages.Dev

    let update (msg:Dev.Msg) (currentModel: DevState): DevState * Cmd<Messages.Msg> =
        match msg with
        | GenericResults str ->
            let newLog = Logging.LogItem.create Logging.LogResult str System.DateTime.UtcNow
            let nextModel = {
                currentModel with
                    Logs = newLog::currentModel.Logs
            }
            nextModel, Cmd.none
        | GenericInfo msg ->
            let newLog = Logging.LogItem.create Logging.LogInfo msg System.DateTime.UtcNow
            let nextModel = {
                currentModel with
                    Logs = newLog::currentModel.Logs
            }
            nextModel, Cmd.none
        | GenericError exn ->
            let newLog = Logging.LogItem.create Logging.LogTypes.LogError exn.Message System.DateTime.UtcNow
            let nextModel = {
                currentModel with
                    Logs = newLog::currentModel.Logs
            }
            nextModel, Cmd.ofMsg (UpdateLastFullError (Some exn) |> DevMsg)
        | UpdateLastFullError exnOpt ->
            let nextModel = {
                currentModel with
                    LastFullError = exnOpt
            }
            nextModel, Cmd.none

module SwateDB =

    open Messages.SwateDB

    let update (msg:SwateDB.Msg) (currentModel: Model.Model): Model.Model * Cmd<Messages.Msg> =
        match msg with
        | GetAllOntologiesRequest ->
            let cmd =
                Cmd.OfAsync.either
                    Api.swateApiv1.getAllOntologies
                    ()
                    (GetAllOntologiesResponse >> SwateDBMsg)
                    (Dev.GenericError >> DevMsg)
            currentModel, cmd
        | GetAllOntologiesResponse (newOntologolies) ->
            let nextModel = {
                currentModel with
                    PersistentStorage = { currentModel.PersistentStorage with AllOntologies = newOntologolies }
            }
            nextModel, Cmd.none

module TermSearch =

    let findRelatedTermSearchState currentModel (id:int) (termType:TermSearchType) =
        let currentInfo = currentModel.ProcessModel.BuildingBlockInfos |> List.find (fun x -> x.Id = id)
        let currentState =
            match termType with
            | TermSearchHeader  -> currentInfo.Header
            | TermSearchValues  -> currentInfo.Values
            | TermSearchUnit    -> currentInfo.Unit
        currentState

    let updateRelatedTermSearchState currentModel (id:int) (termType:TermSearchType) (nextState:TermSearchState) =
        let currentInfo = currentModel.ProcessModel.BuildingBlockInfos |> List.find (fun x -> x.Id = id)
        let nextInfo =
            match termType with
            | TermSearchHeader  -> {currentInfo with Header = nextState}
            | TermSearchValues  -> {currentInfo with Values = nextState}
            | TermSearchUnit    -> {currentInfo with Unit = nextState}
        let nextInfos =
            currentModel.ProcessModel.BuildingBlockInfos |> List.map (fun currentInfo -> if currentInfo.Id = id then nextInfo else currentInfo)
        let nextModel = {
            currentModel with
                ProcessModel = { currentModel.ProcessModel with BuildingBlockInfos = nextInfos }
        }
        nextModel

    open Messages.TermSearch

    let update (incomingMsg:TermSearch.Msg) (currentModel: Model.Model): Model.Model * Cmd<Messages.Msg> =
        match incomingMsg with
        | SearchTermTextChange (queryString, id, termType) ->

            let triggerNewSearch = queryString.Length > 2
           
            let (delay, bounceId, msgToBounce) =
                (System.TimeSpan.FromSeconds 0.5),
                "GetNewTermSuggestions",
                (
                    if triggerNewSearch then
                        //match currentState.ParentOntology, currentState.SearchByParentOntology with
                        //| Some parentOntology, true ->
                        //    (newTerm,parentOntology) |> (GetNewTermSuggestionsByParentTerm >> Request >> Api)
                        //| None,_ | _, false ->
                            (queryString, id, termType)  |> (GetTermSuggestionsRequest >> TermSearchMsg)
                    else
                        DoNothing
                )

            let nextModel =
                let currentState = findRelatedTermSearchState currentModel id termType
                let nextState = {
                    currentState with
                        TermSearchText = queryString
                        SelectedTerm = None
                        ShowSuggestions = triggerNewSearch
                        HasSuggestionsLoading = true
                }
                updateRelatedTermSearchState currentModel id termType nextState

            nextModel, ((delay, bounceId, msgToBounce) |> Bounce |> Cmd.ofMsg)

        | TermSuggestionUsed (selectedTerm, id, termType) ->
            let nextModel =
                let nextState = {
                    TermSearchState.init() with
                        SelectedTerm    = Some selectedTerm
                        TermSearchText  = selectedTerm.Name
                }
                updateRelatedTermSearchState currentModel id termType nextState
            nextModel, Cmd.none
        // Server
        | GetTermSuggestionsRequest (queryString, id, termType) ->
            let cmd = 
                Cmd.OfAsync.either
                    Api.swateApiv1.getTermSuggestions
                    (5,queryString)
                    (fun searchRes ->
                        Msg.Batch [
                            Dev.GenericInfo (sprintf "Requesting Terms for column %i-%s: \"%s\"" id termType.toStrReadable queryString) |> DevMsg
                            GetTermSuggestionsResponse (searchRes, id, termType) |> TermSearchMsg
                        ]
                    )
                    (Dev.GenericError >> DevMsg)
            currentModel, cmd
        | GetTermSuggestionsResponse (suggestions, id, termType) ->
            let msg = Dev.GenericResults (sprintf "Returning search results for column %i-%s: %i" id termType.toStrReadable suggestions.Length) |> DevMsg
            let nextModel =
                let currentState = findRelatedTermSearchState currentModel id termType
                let nextState = {
                    currentState with
                        TermSuggestions = suggestions
                        ShowSuggestions = true
                        HasSuggestionsLoading = false
                }
                updateRelatedTermSearchState currentModel id termType nextState
            nextModel, Cmd.ofMsg msg


let update (msg: Msg) (currentModel: Model): Model * Cmd<Msg> =
    match msg with
    | UpdateAppVersion versionOpt ->
        let nextPersistentStorage = {
            currentModel.PersistentStorage with
                AppVersion = versionOpt
        }
        let nextModel = {
            currentModel with
                PersistentStorage = nextPersistentStorage
        }
        nextModel, Cmd.none
    // Utils
    | Batch msgSeq ->
        let cmd =
            Cmd.batch [
                yield!
                    msgSeq |> Seq.map Cmd.ofMsg
            ]
        currentModel, cmd
    | Bounce (delay, bounceId, msgToBounce) ->
    
            let (debouncerModel, debouncerCmd) =
                currentModel.DebouncerState
                |> Debouncer.bounce delay bounceId msgToBounce
    
            let nextModel = {
                currentModel with
                    DebouncerState = debouncerModel
            }
    
            nextModel,Cmd.map DebouncerSelfMsg debouncerCmd
    
    | DebouncerSelfMsg debouncerMsg ->
        let nextDebouncerState, debouncerCmd =
            Debouncer.update debouncerMsg currentModel.DebouncerState
    
        let nextModel = {
            currentModel with
                DebouncerState = nextDebouncerState
        }
        nextModel, debouncerCmd
    | DoNothing -> currentModel,Cmd.none
    // Style
    | UpdateActivePage routingOption ->
        let nextModel = {
            currentModel with
                ActivePage = routingOption
        }
        nextModel, Cmd.none
    | ToggleNavbarBurger ->
        let nextSiteStyleState = {
            currentModel.SiteStyleState with
                BurgerVisible = currentModel.SiteStyleState.BurgerVisible |> not
        }
        let nextModel = {
            currentModel with
                SiteStyleState = nextSiteStyleState
        }
        nextModel, Cmd.none
    // Subpage-Messages
    | HomeMsg msg ->
        let m, cmd = Home.update msg currentModel.HomeModel
        let nextModel = {
            currentModel with
                HomeModel = m
        }
        nextModel, cmd
    | ProcessMsg msg ->
        let m, cmd = Process.update msg currentModel.ProcessModel
        let nextModel = {
            currentModel with
                ProcessModel = m
        }
        nextModel, cmd
    | DevMsg msg ->
        let m, cmd = Dev.update msg currentModel.DevState
        let nextModel = {
            currentModel with
                DevState = m
        }
        nextModel, cmd
    | SwateDBMsg msg ->
        let nextModel, cmd = SwateDB.update msg currentModel
        nextModel, cmd
    | WordInteropMsg msg ->
        let nextModel, cmd = WordInterop.update msg currentModel
        nextModel, cmd
    | TermSearchMsg msg ->
        let nextModel, cmd = TermSearch.update msg currentModel
        nextModel, cmd