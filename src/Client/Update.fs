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
    | Some Route.ActivityLog ->
        let m = DevState.init()
        let nextModel =
            { model with
                DevState = m
                ActivePage = Some Routing.ActivityLog }
        nextModel, Cmd.none
    | None ->
        model, Cmd.ofMsg (exn("Could not find navigated route!") |> Dev.LogError |> DevMsg )

module Dev =

    open Messages.Dev

    let update (msg:Dev.Msg) (currentModel: DevState): DevState * Cmd<Messages.Msg> =
        match msg with
        | LogResults str ->
            let newLog = Logging.LogItem.create Logging.LogResult str System.DateTime.UtcNow
            let nextModel = {
                currentModel with
                    Logs = newLog::currentModel.Logs
            }
            nextModel, Cmd.none
        | LogInfo msg ->
            let newLog = Logging.LogItem.create Logging.LogInfo msg System.DateTime.UtcNow
            let nextModel = {
                currentModel with
                    Logs = newLog::currentModel.Logs
            }
            nextModel, Cmd.none
        | LogError exn ->
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
    | DevMsg msg ->
        let m, cmd = Dev.update msg currentModel.DevState
        let nextModel = {
            currentModel with
                DevState = m
        }
        nextModel, cmd
    | WordInteropMsg msg ->
        let m, cmd = WordInterop.update msg currentModel.WordInteropModel
        let nextModel = {
            currentModel with
                WordInteropModel = m
        }
        nextModel, cmd