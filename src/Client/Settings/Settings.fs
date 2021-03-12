module Settings

open Fulma
open Fable
open Fable.React
open Fable.React.Props
open Fable.FontAwesome
open Fable.Core.JS
open Fable.Core.JsInterop
open Elmish
open Fable.React
open Fable.React.Props
open Fulma

open Model
open Messages
open Browser.Types
open Fulma.Extensions.Wikiki

let toggleDarkModeElement (model:Model) dispatch =
    Level.level [Level.Level.IsMobile][
        Level.left [][
            str "Darkmode"
        ]
        Level.right [ Props [ Style [if model.SiteStyleState.IsDarkMode then Color model.SiteStyleState.ColorMode.Text else Color model.SiteStyleState.ColorMode.Fade]]] [
            Switch.switch [
                Switch.Id "DarkModeSwitch"
                Switch.Checked model.SiteStyleState.IsDarkMode
                Switch.IsOutlined
                Switch.Color IsPrimary
                Switch.OnChange (fun _ ->
                    Browser.Dom.document.cookie <-
                        let isDarkMode b =
                            let expire = System.DateTime.Now.AddYears 100
                            sprintf "%s=%b; expires=%A; path=/" Cookies.IsDarkMode.toCookieString b expire
                        not model.SiteStyleState.IsDarkMode |> isDarkMode
                    ToggleColorMode |> dispatch
                )
            ] []
        ]
    ]

let mainElement (model:Model.Model) dispatch =
    div [][
        Label.label [Label.Size IsLarge; Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]] [str "Spawn Settings"]
    
        //debugBox model dispatch
    
        Label.label [Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]][str "Customize Spawn"]
        toggleDarkModeElement model dispatch
    ]

type Props = {
    Model: Model.Model
    Dispatch: Messages.Msg -> unit
}

let view props = 
    mainElement props.Model props.Dispatch
