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
    | Some Route.Settings ->
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

    open Messages.TermSearch

    let update (incomingMsg:TermSearch.Msg) (currentModel: Model.Model): Model.Model * Cmd<Messages.Msg> =
        match incomingMsg with
        | SearchTermTextChange (queryString, id, termType) ->

            let triggerNewSearch = queryString.Length > 2
            let currentState = findRelatedTermSearchState currentModel id termType
            let parentChildStateOpt = tryFindParentChildTermSearchState currentModel id termType

            let (delay, bounceId, msgToBounce) =
                (System.TimeSpan.FromSeconds 0.5),
                "GetNewTermSuggestions",
                (
                    if triggerNewSearch then
                        match currentState.SearchByParentChildOntology, parentChildStateOpt with
                        | true, Some parentChildState ->
                            if parentChildState.TermSearchText <> "" then
                                let ontInfo = SwateTypes.OntologyInfo.create parentChildState.TermSearchText (if parentChildState.SelectedTerm.IsSome then parentChildState.SelectedTerm.Value.Accession else "")
                                (queryString, ontInfo, id, termType) |> (GetTermSuggestionsByParentTerm >> TermSearchMsg)
                            else
                                (queryString, id, termType)  |> (GetTermSuggestions >> TermSearchMsg)
                        | _, _ ->
                            (queryString, id, termType)  |> (GetTermSuggestions >> TermSearchMsg)
                    else
                        DoNothing
                )

            let nextModel =
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

        | UpdateSearchByParentChildOntology (b, id, termType) ->
            let nextModel =
                let currentState = findRelatedTermSearchState currentModel id termType
                let nextState = {
                    currentState with
                        SearchByParentChildOntology = b
                }
                updateRelatedTermSearchState currentModel id termType nextState
            nextModel, Cmd.none
        // Server
        | GetTermSuggestions (queryString, id, termType) ->
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

        | GetTermSuggestionsByParentTerm (queryString, ontInfo, id, termType) ->
            let cmd = 
                Cmd.OfAsync.either
                    Api.swateApiv1.getTermSuggestionsByParentTerm
                    (5,queryString,ontInfo)
                    (fun searchRes ->
                        Msg.Batch [
                            Dev.GenericInfo (sprintf "Requesting Terms (parent:%s) for column %i-%s: \"%s\"" ontInfo.Name id termType.toStrReadable queryString) |> DevMsg
                            GetTermSuggestionsResponse (searchRes, id, termType) |> TermSearchMsg
                        ]
                    )
                    (Dev.GenericError >> DevMsg)
            currentModel, cmd

        | GetAllTermsByParentTerm (ontInfo, id, termType) ->
            let cmd = 
                Cmd.OfAsync.either
                    Api.swateApiv1.getAllTermsByParentTerm
                    ontInfo
                    (fun searchRes ->
                        Msg.Batch [
                            Dev.GenericInfo (sprintf "Requesting all Terms (parent:%s) for column %i-%s" ontInfo.Name id termType.toStrReadable) |> DevMsg
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
    | ToggleColorMode ->
        let nextSiteStyleState = {
            currentModel.SiteStyleState with
                IsDarkMode      = currentModel.SiteStyleState.IsDarkMode |> not
                ColorMode       = if currentModel.SiteStyleState.IsDarkMode then WordColors.colorfullMode else WordColors.darkMode
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