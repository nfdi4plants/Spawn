module WordInterop

open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome
open Shared

open Model.WordInterop
open Messages.WordInterop

let init () :  Model * Cmd<Msg> =
    let initialModel = {
        Loading = false
        Debug = None
    }
    initialModel, Cmd.none

let update (msg : Msg) (currentModel : Model) : Model * Cmd<Messages.Msg> =
    match msg with
    | UpdateDebug stringOpt ->
        let nextModel = {
            currentModel with
                Debug = stringOpt
        }
        nextModel, Cmd.none
    // Word-Interop
    | TryWord ->
        let cmd =
            Cmd.OfPromise.either
                OfficeInterop.exampleExcelFunction
                ()
                (Messages.Dev.LogResults >> Messages.DevMsg)
                (Messages.Dev.LogError >> Messages.DevMsg)
        currentModel, cmd
    //| _ ->
    //    currentModel,Cmd.none

let mainElement (model:Model.Model) dispatch =
    div [][
        Label.label [Label.Size IsLarge; Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]] [str "Template"]
    
        //debugBox model dispatch
    
        Label.label [Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]] [str "Sub header"]
        Customcomponents.subModuleBox [
            str "Example"
        ]
    ]

type Props = {
    Model: Model.Model
    Dispatch: Msg -> unit
}

let view props = 
    mainElement props.Model props.Dispatch