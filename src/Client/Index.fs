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
        SiteStyleState = SiteStyleState.init()
        PersistentStorage = PersistentStorage.init()
        Todos = []
        Input = ""
        Logs = []
        ActivePage = None
        HomeModel = Model.Home.Model.init()
        WordInteropModel = Model.WordInterop.Model.init()
    }
    let cmd1 =
        Cmd.OfAsync.perform
            Api.serviceApiv1.getAppVersion
            ()
            ( fun version -> Batch [
                LogResults (sprintf "Retrieved app version from server successfully.")
                UpdateAppVersion version ] )
    let cmd2 =
        Cmd.OfPromise.either
            initializeAddIn
            ()
            ( fun x -> LogResults (sprintf "Established connection to word successfully: %A,%A" x.host x.platform) )
            (LogError)
    let route = Routing.Routing.parsePath Browser.Dom.document.location
    let model, initialCmd = urlUpdate route initialModel
    model, Cmd.batch [cmd1;cmd2; initialCmd]


open Fable.React
open Fable.React.Props
open Fulma

let navBrand =
    Navbar.Brand.div [ ] [
        Navbar.Item.a [
            Navbar.Item.Props [ Href "https://safe-stack.github.io/" ]
            Navbar.Item.IsActive true
        ] [
            img [
                Src "/favicon.png"
                Alt "Logo"
            ]
        ]
    ]

let footerContentStatic (model:Model) dispatch =
    div [][
        str "Swate Release Version "
        a [Href "https://github.com/nfdi4plants/Swate/releases"][str model.PersistentStorage.AppVersion]
    ]

let displayLogs (model:Model) dispatch =
    Container.container [][
        Table.table [Table.IsFullWidth][
            thead [][
                tr [][
                    th [][str "Log Type"]
                    th [][str "Message"]
                ]
            ]
            tbody [][
                for log in model.Logs do
                    let logType = fst log
                    yield tr [][
                        td [Style [Color logType.toColor]][
                            str logType.toReadableString
                        ]
                        td [][str (snd log)]
                    ]
            ]
        ]
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    Hero.hero [
        Hero.Color IsPrimary
        Hero.IsFullHeight
        Hero.Props [
            Style [
                Background """linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url("https://unsplash.it/1200/900?random") no-repeat center center fixed"""
                BackgroundSize "cover"
            ]
        ]
    ] [
        Hero.head [ ] [
            CustomComponents.Navbar.navbarComponent model dispatch
        ]

        Hero.body [ ] [
            Container.container [ ] [
                Column.column [
                    Column.Width (Screen.All, Column.Is6)
                    Column.Offset (Screen.All, Column.Is3)
                ] [
                    Heading.p [ Heading.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] [ str "SAFEOfficeAddIn" ]
                    match model.ActivePage with
                    | Some Routing.Route.Home ->
                        Home.view { Model = model.HomeModel; Dispatch = HomeMsg >> dispatch }
                    | Some Routing.Route.WordInterop ->
                        WordInterop.view { Model = model.WordInteropModel; Dispatch = WordInteropMsg >> dispatch }
                    | None ->
                        div [][
                            Heading.h1 [][str "404"]
                            str "Was not able to navigate to requested Page!"
                        ]
                ]

                displayLogs model dispatch
            ]
        ]
        div [Style [Position PositionOptions.Fixed; Bottom "0"; Width "100%"; TextAlign TextAlignOptions.Center; Color "grey"]][
            footerContentStatic model dispatch
        ]
    ]
