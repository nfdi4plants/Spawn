module Process

open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome
open Shared

open Model.Process
open Messages.Process
open Messages

let init () :  Model * Cmd<Msg> =
    let initialModel = {
        Loading             = false
        BuildingBlockInfos  = []
    }
    initialModel, Cmd.none

let update (msg : Process.Msg) (currentModel : Model) : Model * Cmd<Messages.Msg> =
    match msg with
    | CreateNewBuildingBlock ->
        let nextID =
            if currentModel.BuildingBlockInfos.IsEmpty then
                0
            else
                currentModel.BuildingBlockInfos |> List.map (fun x -> x.Id) |> List.max |> (+) 1
        let newBuildingBlockInfo = Model.BuildingBlockInfoState.init(nextID)
        let nextModel = {
            currentModel with
                BuildingBlockInfos = newBuildingBlockInfo::currentModel.BuildingBlockInfos
        }
        nextModel, Cmd.none
    | DeleteBuildingBlock id ->
        let filterBuildingBlocks =
            let filtered = currentModel.BuildingBlockInfos |> List.filter (fun x -> x.Id <> id)
            filtered |> List.mapi (fun i x -> {x with Id = filtered.Length - i})
        let nextModel = {
            currentModel with
                BuildingBlockInfos = filterBuildingBlocks
        }
        nextModel, Cmd.none

    //| _ ->
    //    currentModel,Cmd.none

open Model

let mainElement (model:Model.Model) dispatch =
    div [][
        Label.label [Label.Size IsLarge; Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]] [str "Template"]
    ]

type Props = {
    Model: Model.Model
    Dispatch: Msg -> unit
}

let view props = 
    mainElement props.Model props.Dispatch