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

let pageManipulationButtons (model:Model) dispatch =
    Field.div [][
        Button.a [
            Button.Props [Title "Add Column"]
            Button.Color IsInfo
            Button.OnClick (fun e ->
                CustomComponents.ResponsiveFA.triggerResponsiveReturnEle "addColumn_Process"
                Process.CreateNewBuildingBlock |> ProcessMsg |> dispatch
            )
        ][
            CustomComponents.ResponsiveFA.responsiveReturnEle "addColumn_Process" Fa.Solid.Plus Fa.Solid.Check
        ]
    ]

let buildingBlockTermSearchEle (model:Model.Model) dispatch (termSearchState:TermSearchState) (termType:TermSearchType) (id:int)=
    Field.div [Field.HasAddons][
        match termType with
        | TermSearchHeader | TermSearchValues ->
            Control.div [][
                Button.a [
                    Button.Color IsDanger
                ][ Fa.i [Fa.Solid.Times][] ]
            ]
            Control.div [][
                Button.a [
                    Button.Color IsSuccess
                ][ Fa.i [Fa.Solid.Check][] ]
            ]
        | TermSearchUnit ->
            Control.div [][
                Button.a [
                    Button.IsStatic true
                ][ str "Add unit"]
            ]
        AutocompleteSearch.autocompleteTermSearchComponent
            dispatch
            model.SiteStyleState.ColorMode
            model
            termType.toInputPlaceholderText
            None
            (AutocompleteSearch.AutocompleteParameters.ofTermSearchState termSearchState termType id)
            false
    ]

let buildingBlockInfoEle (model:Model.Model) dispatch (buildingBlockInfo:BuildingBlockInfoState) =
    Customcomponents.subModuleBox [
        Label.label [Label.Size IsSmall; Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]][str "Header"]
        buildingBlockTermSearchEle model dispatch buildingBlockInfo.Header TermSearchHeader buildingBlockInfo.Id
        Label.label [Label.Size IsSmall; Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]][str "Values"]
        buildingBlockTermSearchEle model dispatch buildingBlockInfo.Values TermSearchValues buildingBlockInfo.Id
        Label.label [Label.Size IsSmall; Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]][str "Unit"]
        buildingBlockTermSearchEle model dispatch buildingBlockInfo.Unit TermSearchUnit buildingBlockInfo.Id
        Level.level [Level.Level.IsMobile][
            Level.left [] []
            Level.right [] [
                Level.item [][
                    Button.a [
                        Button.Props [Title "Remove Column"]
                        Button.Color IsDanger
                        Button.OnClick (fun e ->
                            Process.DeleteBuildingBlock buildingBlockInfo.Id |> ProcessMsg |> dispatch
                        )
                    ][
                        Fa.i [Fa.Solid.Times][]
                    ]
                ]
            ]
        ]
    ]

let displayAllBuildingBlocksInfosEle (model:Model.Model) dispatch =
    Container.container [] [
        for info in model.ProcessModel.BuildingBlockInfos do
            yield
                Label.label [Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]] [str (sprintf "Column: %i" info.Id)]
            yield
                buildingBlockInfoEle model dispatch info
    ]

let mainElement (model:Model.Model) dispatch =
    div [][
        Label.label [Label.Size IsLarge; Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]] [str "Template"]
    
        //debugBox model dispatch
        pageManipulationButtons model dispatch

        displayAllBuildingBlocksInfosEle model dispatch
        
        //Button.a [
        //    Button.IsFullWidth
        //    Button.Color IsInfo
        //    Button.OnClick (fun e -> SwateDB.GetAllOntologiesRequest |> SwateDBMsg |> dispatch)
        //][
        //    str "Get Ontologies"
        //]
    ]

type Props = {
    Model: Model.Model
    Dispatch: Msg -> unit
}

let view props = 
    mainElement props.Model props.Dispatch