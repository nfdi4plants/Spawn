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
    | NewCommentWithHeader str ->
        let bb = Model.BuildingBlockInfoState.init()
        let newTermSearchState = Model.TermSearchState.init()
        let nextSate= {
            bb with
                Header = {newTermSearchState with TermSearchText = str}
        }
        let nextModel = {
            currentModel with
                BuildingBlockInfo = Some nextSate
        }
        let msg = Messages.TermSearch.SearchTermTextChange (str,Model.TermSearchHeader) |> Messages.TermSearchMsg
        nextModel, Cmd.ofMsg msg
    | NewCommentWithValues str ->
        let bb = Model.BuildingBlockInfoState.init()
        let newTermSearchState = Model.TermSearchState.init()
        let nextSate= {
            bb with
                Values = {newTermSearchState with TermSearchText = str}
        }
        let nextModel = {
            currentModel with
                BuildingBlockInfo = Some nextSate
        }
        let msg = Messages.TermSearch.SearchTermTextChange (str,Model.TermSearchValues) |> Messages.TermSearchMsg
        nextModel, Cmd.ofMsg msg
    | CloseSuggestions ->
        if currentModel.BuildingBlockInfo.IsSome then
            let bb = currentModel.BuildingBlockInfo.Value
            let newBuildingBlockInfos = {
                bb with
                    Header  = { bb.Header with ShowSuggestions = false; TermSuggestions = [||]}
                    Values  = { bb.Values with ShowSuggestions = false; TermSuggestions = [||]}
                    Unit    = { bb.Unit with ShowSuggestions = false; TermSuggestions = [||]}
            }
            let nextModel = {
                currentModel with
                    BuildingBlockInfo = Some newBuildingBlockInfos
            }
            nextModel, Cmd.none
        else
            currentModel, Cmd.none
    | ResetBuildingBlockInfo ->
        let nextModel = {
            currentModel with
                BuildingBlockInfo = None
        }
        nextModel, Cmd.none


let mainElement (model:Model.Model) dispatch =
    div [][
        Label.label [Label.Size IsLarge; Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]] [str "Add Annotation Comment"]
    
        //debugBox model dispatch
    
        Label.label [Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]] [str "Sub header"]

        Customcomponents.subModuleBox [
            Field.div [][
                Help.help [][
                    b [] [str "Select text in your Word document and use it for ontology term search."]
                    str " You can then save your search results in ISA format in your document."
                ]
            ]
            if model.CommentModel.BuildingBlockInfo.IsSome then
                CustomComponents.BuildingBlockInputEle.buildingBlockInfoEle model dispatch model.CommentModel.BuildingBlockInfo.Value
                Field.div [][
                    Level.level [Level.Level.IsMobile][
                        Level.left [][]
                        Level.right [][
                            Level.item [][
                                Button.a [
                                    Button.Color IsDanger
                                    Button.OnClick (fun e ->
                                        Messages.Comment.ResetBuildingBlockInfo |> Messages.CommentMsg |> dispatch
                                    )
                                ][
                                    str "Reset"
                                ]
                            ]
                        ]
                    ]
                ]
            else
                Field.div [][
                    Level.level [Level.Level.IsMobile][
                        Level.left [][
                            Level.item [][
                                Button.a [
                                    Button.Color IsInfo
                                    Button.OnClick (fun e ->
                                        Messages.WordInterop.GetSelectedTextAsHeader |> Messages.WordInteropMsg |> dispatch
                                    )
                                ][
                                    str "Add selected as header"
                                ]
                            ]
                            Level.item [][
                                Button.a [
                                    Button.Color IsInfo
                                    Button.OnClick (fun e ->
                                        Messages.WordInterop.GetSelectedTextAsValues |> Messages.WordInteropMsg |> dispatch
                                    )
                                ][
                                    str "Add selected as values"
                                ]
                            ]
                        ]
                        Level.right [][
                        ]
                    ]
                ]
        ]
    ]

type Props = {
    Model: Model.Model
    Dispatch: Messages.Msg -> unit
}

let view props = 
    mainElement props.Model props.Dispatch