module SubpageTemplate
open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome
open Shared

/// this needs to be copy pasted in "Model.fs"

type Model = {
    Loading: bool
    Debug: string option
}
    with
        static member init() = {
            Loading = false
            Debug = None
        }

/// this needs to be copy pasted in "Messages.fs"

type Msg =
| UpdateDebug of string option

let init () :  Model * Cmd<Msg> =
    let initialModel = {
        Loading = false
        Debug = None
    }
    initialModel, Cmd.none

let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match msg with
    | UpdateDebug stringOpt ->
        let nextModel = {
            currentModel with
                Debug = stringOpt
        }
        nextModel, Cmd.none
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