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
    let isDarkMode =
        let cookies = Browser.Dom.document.cookie
        let cookiesSplit = cookies.Split([|";"|], System.StringSplitOptions.RemoveEmptyEntries)
        cookiesSplit
        |> Array.tryFind (fun x -> x.StartsWith (Cookies.IsDarkMode.toCookieString + "="))
        |> fun cookieOpt ->
            if cookieOpt.IsSome then
                cookieOpt.Value.Replace(Cookies.IsDarkMode.toCookieString + "=","")
                |> fun cookie ->
                    match cookie with
                    | "false"| "False"  -> false
                    | "true" | "True"   -> true
                    | anyElse -> false
            else
                false
    let initialModel = {
        SiteStyleState      = SiteStyleState.init(darkMode=isDarkMode)
        DevState            = DevState.init()
        ///Debouncing
        DebouncerState      = Thoth.Elmish.Debouncer.create ()
        PersistentStorage   = PersistentStorage.init()
        // subpage models
        ActivePage          = None
        HomeModel           = Model.Home.Model.init()
        ProcessModel        = Model.Process.Model.init()
        CommentModel        = Model.Comment.Model.init()
    }
    let cmd1 =
        Cmd.OfAsync.perform
            Api.serviceApiv1.getAppVersion
            ()
            ( fun version -> Batch [
                Dev.Msg.GenericResults (sprintf "Retrieved app version from server successfully.") |> DevMsg
                UpdateAppVersion version ] )
    let cmd2 =
        Cmd.OfPromise.either
            initializeAddIn
            ()
            ( fun x -> Dev.Msg.GenericResults (sprintf "Established connection to word successfully: %A,%A" x.host x.platform) |> DevMsg ) 
            ( Dev.Msg.GenericError >> DevMsg )
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

    | Some Routing.Route.Comment ->
        BaseView.baseViewComponent model dispatch [
            Process.view <| {Model = model; Dispatch = dispatch}
        ][
            str ""
        ]

    | Some Routing.Route.Process ->
        BaseView.baseViewComponent model dispatch [
            Comment.view <| {Model = model; Dispatch = dispatch}
        ][
            str ""
        ]

    | Some Routing.Route.Info ->
        BaseView.baseViewComponent model dispatch [
            Info.infoComponent model dispatch
        ][]

    | Some Routing.Route.Settings ->
        BaseView.baseViewComponent model dispatch [
            Settings.view <| {Model = model; Dispatch = dispatch}
        ][]

    | Some Routing.Route.Home | None ->
        BaseView.baseViewComponent model dispatch [
            Home.mainElement model dispatch
        ] [
            str ""
        ]
