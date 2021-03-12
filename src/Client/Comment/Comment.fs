module Comment

open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome
open Shared

open Model.Comment
open Messages.Comment

let init () :  Model * Cmd<Messages.Msg> =
    Model.Comment.Model.init(), Cmd.none

let update (msg : Msg) (currentModel : Model) : Model * Cmd<Messages.Msg> =
    match msg with
    | CloseSuggestions ->
        let bb = currentModel.BuildingBlockInfo
        let newBuildingBlockInfos = {
            bb with
                Header  = { bb.Header with ShowSuggestions = false; TermSuggestions = [||]}
                Values  = { bb.Values with ShowSuggestions = false; TermSuggestions = [||]}
                Unit    = { bb.Unit with ShowSuggestions = false; TermSuggestions = [||]}
        }
        let nextModel = {
            currentModel with
                BuildingBlockInfo = newBuildingBlockInfos
        }
        nextModel, Cmd.none

let mainElement (model:Model.Model) dispatch =
    div [][
        Label.label [Label.Size IsLarge; Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]] [str "Add Annotation Comment"]
    
        //debugBox model dispatch
    
        Label.label [Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]] [str "Sub header"]

        CustomComponents.BuildingBlockInputEle.buildingBlockInfoEle model dispatch model.CommentModel.BuildingBlockInfo
    ]

type Props = {
    Model: Model.Model
    Dispatch: Messages.Msg -> unit
}

let view props = 
    mainElement props.Model props.Dispatch