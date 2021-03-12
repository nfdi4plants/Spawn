module CustomComponents.BuildingBlockInputEle


open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome
open Shared

open Messages
open Model

let buildingBlockTermSearchEle (model:Model.Model) dispatch (termSearchState:TermSearchState) (termType:TermSearchType) =
    let parentChildTermStateOpt = Messages.TermSearch.tryFindParentChildTermSearchState model termType
    Field.div [Field.HasAddons][
        /// Choose if search is is_a directed
        match termType with
        | TermSearchHeader | TermSearchValues ->
            Control.div [][
                Button.a [
                    Button.OnClick (fun e ->
                        TermSearch.UpdateSearchByParentChildOntology (false, termType) |> TermSearchMsg |> dispatch
                    )
                    if termSearchState.SearchByParentChildOntology |> not then
                        Button.Color IsDanger
                ][ Fa.i [Fa.Solid.Times][] ]
            ]
            Control.div [][
                Button.a [
                    Button.OnClick (fun e ->
                        TermSearch.UpdateSearchByParentChildOntology (true, termType) |> TermSearchMsg |> dispatch
                    )
                    if termSearchState.SearchByParentChildOntology then
                        Button.Color IsSuccess
                ][ Fa.i [Fa.Solid.Check][] ]
            ]
        | TermSearchUnit ->
            Control.div [][
                Button.a [
                    Button.IsStatic true
                ][ str "Add unit"]
            ]
        /// better visualize if search is is_a directed
        if parentChildTermStateOpt.IsSome && termSearchState.SearchByParentChildOntology && parentChildTermStateOpt.Value.TermSearchText <> "" then
            Control.div [][
                Button.a [
                    Button.IsStatic true
                ][ str (sprintf "%s" parentChildTermStateOpt.Value.TermSearchText)]
            ]
        AutocompleteSearch.autocompleteTermSearchComponent
            dispatch
            model.SiteStyleState.ColorMode
            model
            termType.toInputPlaceholderText
            None
            (AutocompleteSearch.AutocompleteParameters.ofTermSearchState termSearchState termType)
            false
    ]

let buildingBlockInfoEle (model:Model.Model) dispatch (buildingBlockInfo:BuildingBlockInfoState) =
    Customcomponents.subModuleBox [
        Label.label [Label.Size IsSmall; Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]][str "Header"]
        buildingBlockTermSearchEle model dispatch buildingBlockInfo.Header TermSearchHeader
        Label.label [Label.Size IsSmall; Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]][str "Values"]
        buildingBlockTermSearchEle model dispatch buildingBlockInfo.Values TermSearchValues
        Label.label [Label.Size IsSmall; Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]][str "Unit"]
        buildingBlockTermSearchEle model dispatch buildingBlockInfo.Unit TermSearchUnit
    ]