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

    type Msg =
        | SearchTermTextChange                      of queryString:string * buildingBlockInfoId:int * termType:Model.TermSearchType
        | TermSuggestionUsed                        of DbDomain.Term * buildingBlockInfoId:int * termType:Model.TermSearchType
        // Server
        | GetTermSuggestionsRequest                 of queryString:string * buildingBlockInfoId:int * termType:Model.TermSearchType
        | GetTermSuggestionsResponse                of searchResults:SwateTypes.DbDomain.Term [] * buildingBlockInfoId:int * termType:Model.TermSearchType
        //| ToggleSearchByParentOntology
        //| TermSuggestionUsed                    of DbDomain.Term
        //| NewSuggestions                        of DbDomain.Term []
        //| StoreParentOntologyFromOfficeInterop  of obj option
        //// Server
        //| GetAllTermsByParentTermRequest        of OntologyInfo 
        //| GetAllTermsByParentTermResponse       of DbDomain.Term []

module Process =

    type Msg =
    | CreateNewBuildingBlock
    | DeleteBuildingBlock   of int


type Msg =
| UpdateAppVersion          of string
// Utils
| Batch                     of seq<Msg>
| Bounce                    of (System.TimeSpan*string*Msg)
| DebouncerSelfMsg          of Thoth.Elmish.Debouncer.SelfMessage<Msg>
| DoNothing
// Style
| ToggleNavbarBurger
| UpdateActivePage          of Routing.Route option
// Submodel-Messages
| DevMsg                    of Dev.Msg
| HomeMsg                   of Home.Msg
| ProcessMsg                of Process.Msg
| WordInteropMsg            of WordInterop.Msg
| SwateDBMsg                of SwateDB.Msg
| TermSearchMsg             of TermSearch.Msg