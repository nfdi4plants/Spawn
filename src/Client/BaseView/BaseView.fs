module BaseView

open Fable.React
open Fable.React.Props
open Fulma
open WordColors
open Model
open Messages
open Browser
open CustomComponents

let createNavigationTab (pageLink: Routing.Route) (model:Model) (dispatch:Msg-> unit) =
    let isActive = model.ActivePage.IsSome && model.ActivePage.Value = pageLink
    Tabs.tab [Tabs.Tab.IsActive isActive] [
        a [ 
            Style [
                if isActive then
                    BorderColor model.SiteStyleState.ColorMode.Accent
                    BackgroundColor model.SiteStyleState.ColorMode.BodyBackground
                    Color model.SiteStyleState.ColorMode.Accent
                    BorderBottomColor model.SiteStyleState.ColorMode.BodyBackground
                else
                    BorderBottomColor model.SiteStyleState.ColorMode.Accent
            ]
            OnClick (fun e -> UpdateActivePage (Some pageLink) |> dispatch)
        ] [
            Text.span [] [
                span [Class "hideUnder775px"][str pageLink.toStringRdbl]
                span [Class "hideOver775px"][pageLink |> Routing.Route.toIcon]
            ]

        ]
    ]

let tabRow (model:Model) dispatch (tabs: seq<ReactElement>)=
    Tabs.tabs[
        Tabs.IsCentered; Tabs.IsFullWidth; Tabs.IsBoxed
        Tabs.Props [
            Style [
                BackgroundColor model.SiteStyleState.ColorMode.BodyBackground
                CSSProp.Custom ("overflow","visible")
            ]
        ]
    ] [
        yield! tabs
    ]

let firstRowTabs (model:Model) dispatch =
    tabRow model dispatch [
        createNavigationTab Routing.Route.Comment       model dispatch
        createNavigationTab Routing.Route.Process       model dispatch
        createNavigationTab Routing.Route.Info          model dispatch
        createNavigationTab Routing.Route.ActivityLog   model dispatch
    ]

let sndRowTabs (model:Model) dispatch =
    tabRow model dispatch [
        
    ]

let footerContentStatic (model:Model) dispatch =
    div [][
        str "Swate Release Version "
        a [Href "https://github.com/nfdi4plants/Swate/releases"][str model.PersistentStorage.AppVersion]
    ]

open Fable.Core.JsInterop
open Fable.FontAwesome

/// The base react component for all views in the app. contains the navbar and takes body and footer components to create the full view.
let baseViewComponent (model: Model) (dispatch: Msg -> unit) (bodyChildren: ReactElement list) (footerChildren: ReactElement list) =
    div [
        Style [MinHeight "100vh"; BackgroundColor model.SiteStyleState.ColorMode.BodyBackground; Color model.SiteStyleState.ColorMode.Text;]
        OnClick (fun e ->
            if model.ActivePage = Some Routing.Comment then
                Comment.CloseSuggestions |> CommentMsg |> dispatch
            else
                ()
        )
    ] [
        Navbar.navbarComponent model dispatch
        Container.container [
            Container.IsFluid
        ] [
            br []
            firstRowTabs model dispatch
            //sndRowTabs model dispatch

            // Error Modal element, not shown when no lastFullEror
            if model.DevState.LastFullError.IsSome then
                CustomComponents.ErrorModal.errorModal model dispatch

            yield! bodyChildren

            br []

            if footerChildren.IsEmpty |> not then
                Footer.footer [ Props [WordColors.colorControl model.SiteStyleState.ColorMode]] [
                    Content.content [
                        Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Left)]
                        Content.Props [WordColors.colorControl model.SiteStyleState.ColorMode] 
                    ] [
                        yield! footerChildren
                    ]
                ]
        ]

        div [Style [Position PositionOptions.Fixed; Bottom "0"; Width "100%"; TextAlign TextAlignOptions.Center; Color "grey"]][
            footerContentStatic model dispatch
        ]
    ]