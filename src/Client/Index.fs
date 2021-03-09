module Index

open Elmish
open Fable.Remoting.Client

open Shared

open Api
open Model
open Messages
open Update

let initializeAddIn () =
    OfficeInterop.Office.onReady()

let init _ : Model * Cmd<Msg> =
    let initialModel = {
        SiteStyleState      = SiteStyleState.init()
        DevState            = DevState.init()
        PersistentStorage   = PersistentStorage.init()
        // subpage models
        ActivePage          = None
        HomeModel           = Model.Home.Model.init()
        WordInteropModel    = Model.WordInterop.Model.init()
        //
        Todos               = []
        Input               = ""
    }
    let cmd1 =
        Cmd.OfAsync.perform
            Api.serviceApiv1.getAppVersion
            ()
            ( fun version -> Batch [
                Dev.Msg.LogResults (sprintf "Retrieved app version from server successfully.") |> DevMsg
                UpdateAppVersion version ] )
    let cmd2 =
        Cmd.OfPromise.either
            initializeAddIn
            ()
            ( fun x -> Dev.Msg.LogResults (sprintf "Established connection to word successfully: %A,%A" x.host x.platform) |> DevMsg ) 
            (Dev.Msg.LogError >> DevMsg)
    let route = Routing.Routing.parsePath Browser.Dom.document.location
    let model, initialCmd = urlUpdate route initialModel
    model, Cmd.batch [cmd1;cmd2; initialCmd]


open Fable.React
open Fable.React.Props
open Fulma

let view (model : Model) (dispatch : Msg -> unit) =

    match model.ActivePage with
    | Some Routing.Route.ActivityLog ->
        BaseView.baseViewComponent model dispatch [
            ActivityLog.activityLogComponent model dispatch
        ] [
            str ""
        ]

    | Some Routing.Route.WordInterop ->
        BaseView.baseViewComponent model dispatch [
            WordInterop.view <| {Model = model; Dispatch = (WordInteropMsg >> dispatch)}
        ][
            str ""
        ]

    | Some Routing.Route.Info ->
        BaseView.baseViewComponent model dispatch [
            Info.infoComponent model dispatch
        ][]

    | Some Routing.Route.Home | None ->
        BaseView.baseViewComponent model dispatch [
            Home.mainElement model dispatch
        ] [
            str ""
        ]
