module Messages

open Shared

module Home =

    type Msg =
    | UpdateDebug of string option

module WordInterop =

    type Msg =
    | TryWord

module Dev =

    type Msg =
    | GenericResults        of string
    | GenericInfo           of string
    | GenericError          of exn
    | UpdateLastFullError   of exn option

module SwateDB =

    type Msg =
    | GetAllOntologiesRequest
    | GetAllOntologiesResponse  of SwateTypes.DbDomain.Ontology []

open Shared.SwateTypes

module TermSearch =

    open Model

    /// This is used to find the correct TermSearchState in the currentModel.
    /// This is necessary as there are multiple building block infos, each with 3 TermSearchStates
    let findRelatedTermSearchState currentModel (id:int) (termType:TermSearchType) =
        let currentInfo = currentModel.ProcessModel.BuildingBlockInfos |> List.find (fun x -> x.Id = id)
        let currentState =
            match termType with
            | TermSearchHeader  -> currentInfo.Header
            | TermSearchValues  -> currentInfo.Values
            | TermSearchUnit    -> currentInfo.Unit
        currentState

    let tryFindParentChildTermSearchState currentModel (id:int) (termType:TermSearchType) =
        let currentInfo = currentModel.ProcessModel.BuildingBlockInfos |> List.find (fun x -> x.Id = id)
        let currentState =
            match termType with
            | TermSearchHeader  -> Some currentInfo.Values
            | TermSearchValues  -> Some currentInfo.Header
            | TermSearchUnit    -> None
        currentState

    let updateRelatedTermSearchState currentModel (id:int) (termType:TermSearchType) (nextState:TermSearchState) =
        let currentInfo = currentModel.ProcessModel.BuildingBlockInfos |> List.find (fun x -> x.Id = id)
        let nextInfo =
            match termType with
            | TermSearchHeader  -> {currentInfo with Header = nextState}
            | TermSearchValues  -> {currentInfo with Values = nextState}
            | TermSearchUnit    -> {currentInfo with Unit = nextState}
        let nextInfos =
            currentModel.ProcessModel.BuildingBlockInfos |> List.map (fun currentInfo -> if currentInfo.Id = id then nextInfo else currentInfo)
        let nextModel = {
            currentModel with
                ProcessModel = { currentModel.ProcessModel with BuildingBlockInfos = nextInfos }
        }
        nextModel

    type Msg =
        | SearchTermTextChange                      of queryString:string * buildingBlockInfoId:int * termType:Model.TermSearchType
        | TermSuggestionUsed                        of DbDomain.Term * buildingBlockInfoId:int * termType:Model.TermSearchType
        | UpdateSearchByParentChildOntology         of bool * buildingBlockInfoId:int * termType:Model.TermSearchType
        // Server
        | GetTermSuggestions                        of queryString:string * buildingBlockInfoId:int * termType:Model.TermSearchType
        | GetTermSuggestionsByParentTerm            of queryString:string * parentTerm:OntologyInfo * buildingBlockInfoId:int * termType:Model.TermSearchType
        | GetAllTermsByParentTerm                   of parentTerm:OntologyInfo * buildingBlockInfoId:int * termType:Model.TermSearchType
        | GetTermSuggestionsResponse                of searchResults:SwateTypes.DbDomain.Term [] * buildingBlockInfoId:int * termType:Model.TermSearchType

module Process =

    type Msg =
    | CreateNewBuildingBlock
    | DeleteBuildingBlock                           of int
    | CloseSuggestions


type Msg =
| UpdateAppVersion          of string
// Utils
| Batch                     of seq<Msg>
| Bounce                    of (System.TimeSpan*string*Msg)
| DebouncerSelfMsg          of Thoth.Elmish.Debouncer.SelfMessage<Msg>
| DoNothing
// Style
| ToggleNavbarBurger
| ToggleColorMode
| UpdateActivePage          of Routing.Route option
// Submodel-Messages
| DevMsg                    of Dev.Msg
| HomeMsg                   of Home.Msg
| ProcessMsg                of Process.Msg
| WordInteropMsg            of WordInterop.Msg
| SwateDBMsg                of SwateDB.Msg
| TermSearchMsg             of TermSearch.Msg