module Update

open Elmish
open Fable.Remoting.Client

open Shared
open Model
open Messages
open Routing

let urlUpdate (route:Route option) (model:Model) =
    match route with
    | Some Route.Home ->
        let m, cmd = Home.init()
        let nextModel =
            { model with
                HomeModel = m
                ActivePage = Some Routing.Home }
        nextModel, Cmd.map HomeMsg cmd
    | Some Route.WordInterop ->
        let m, cmd = WordInterop.init()
        let nextModel =
            { model with
                WordInteropModel = m
                ActivePage = Some Routing.WordInterop }
        nextModel, Cmd.map WordInteropMsg cmd
    | None ->
        model, Cmd.ofMsg (exn("Could not find navigated route!") |> LogError )

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
    // Utils
    | Batch msgSeq ->
        let cmd =
            Cmd.batch [
                yield!
                    msgSeq |> Seq.map Cmd.ofMsg
            ]
        currentModel, cmd
    | LogResults str ->
        let newLog = Logging.LogItem.create Logging.LogResult str
        let nextModel = {
            currentModel with
                Logs = newLog::currentModel.Logs
        }
        nextModel, Cmd.none
    | LogInfo msg ->
        let newLog = Logging.LogItem.create Logging.LogInfo msg
        let nextModel = {
            currentModel with
                Logs = newLog::currentModel.Logs
        }
        nextModel, Cmd.none
    | LogError exn ->
        let newLog = Logging.LogItem.create Logging.LogTypes.LogError exn.Message
        let nextModel = {
            currentModel with
                Logs = newLog::currentModel.Logs
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
    | WordInteropMsg msg ->
        let m, cmd = WordInterop.update msg currentModel.WordInteropModel
        let nextModel = {
            currentModel with
                WordInteropModel = m
        }
        nextModel, cmd