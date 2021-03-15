module AutocompleteSearch

open Model
open Messages
open Shared.SwateTypes
open Fable.React
open Fable.React.Props
open Fulma
open Fulma.Extensions.Wikiki
open Fable.FontAwesome
open WordColors
open Fable.Core.JsInterop

let createLinkOfAccession (accession:string) =
    a [
        let link = accession |> Shared.URLs.termAccessionUrlOfAccessionStr
        Href link
        Target "_Blank"
    ] [
        str accession
    ]

let loadingComponent =
    Fa.i [
        Fa.Solid.Spinner
        Fa.Pulse
        Fa.Size Fa.Fa4x
    ] []

type AutocompleteSuggestion = {
    Name            : string
    ID              : string
    TooltipText     : string
    Status          : string
    StatusIsWarning : bool
    Data            : DbDomain.Term
}
with
    static member ofTerm (term:DbDomain.Term) : AutocompleteSuggestion = {
        Name            = term.Name
        ID              = term.Accession
        TooltipText     = term.Definition
        Status          = if term.IsObsolete then "obsolete" else ""
        StatusIsWarning = term.IsObsolete
        Data            = term
    }

       
type AutocompleteParameters = {
    ModalId                 : string
    InputId                 : string
    /// This field relates the AutocompleteParameters to a given TermSearchState
    TermSearchType          : Model.TermSearchType

    QueryString             : string
    Suggestions             : AutocompleteSuggestion []
    DropDownIsVisible       : bool
    DropDownIsLoading       : bool

    IsSelectedTerm          : bool
    OnInputChangeMsg        : string -> Msg
    OnSuggestionSelect      : DbDomain.Term -> Msg

    HasAdvancedSearch       : bool
    AdvancedSearchLinkText  : string
    OnAdvancedSearch        : unit//(DbDomain.Term -> Msg)
} with
    static member ofTermSearchState (state:TermSearchState) (termType:TermSearchType): AutocompleteParameters = {
        ModalId                 = sprintf "TermSearch_%A" termType
        InputId                 = sprintf "TermSearchInput_%A" termType
        TermSearchType          = termType

        QueryString             = state.TermSearchText
        Suggestions             = state.TermSuggestions |> Array.map AutocompleteSuggestion.ofTerm

        IsSelectedTerm          = state.SelectedTerm.IsSome
        DropDownIsVisible       = state.ShowSuggestions
        DropDownIsLoading       = state.HasSuggestionsLoading

        OnInputChangeMsg        = fun queryStr -> TermSearch.SearchTermTextChange (queryStr, termType) |> TermSearchMsg 
        OnSuggestionSelect      = fun (term:DbDomain.Term) -> TermSearch.TermSuggestionUsed (term, termType) |> TermSearchMsg

        HasAdvancedSearch       = true
        AdvancedSearchLinkText  = "Cant find the Term you are looking for?"
        OnAdvancedSearch        = ()//(fun (term:DbDomain.Term) -> term |> TermSuggestionUsed |> TermSearch )
    }

let createAutocompleteSuggestions (dispatch: Msg -> unit) (colorMode:WordColors.ColorMode) (autocompleteParams:AutocompleteParameters) =

    let suggestions = 
        if autocompleteParams.Suggestions.Length > 0 then
            autocompleteParams.Suggestions
            |> Array.map (fun sugg ->
                tr [
                    OnClick (fun _ ->
                        let e = Browser.Dom.document.getElementById(autocompleteParams.InputId)
                        e?value <- sugg.Name
                        sugg.Data |> autocompleteParams.OnSuggestionSelect |> dispatch)
                    OnKeyDown (fun k -> if k.key = "Enter" then sugg.Data |> autocompleteParams.OnSuggestionSelect |> dispatch)
                    TabIndex 0
                    colorControl colorMode
                    Class "suggestion"
                ] [
                    td [Class (Tooltip.ClassName + " " + Tooltip.IsTooltipRight + " " + Tooltip.IsMultiline);Tooltip.dataTooltip sugg.TooltipText] [
                        Fa.i [Fa.Solid.InfoCircle] []
                    ]
                    td [] [
                        b [] [str sugg.Name]
                    ]
                    td [if sugg.StatusIsWarning then Style [Color NFDIColors.Red.Base]] [str sugg.Status]
                    td [
                        OnClick (
                            fun e ->
                                e.stopPropagation()
                        )
                        Style [FontWeight "light"]
                    ] [
                        small [] [
                            createLinkOfAccession sugg.ID
                    ] ]
                ])
            |> List.ofArray
        else
            [
                tr [] [
                    td [] [str "No terms found matching your input."]
                ]
            ]

    //let alternative =
    //    tr [
    //        colorControl colorMode
    //        Class "suggestion"
    //    ][
    //        td [ColSpan 4] [
    //            str (sprintf "%s " autocompleteParams.AdvancedSearchLinkText)
    //            a [OnClick (fun _ -> ToggleModal autocompleteParams.ModalId |> AdvancedSearch |> dispatch)] [
    //                str "Use Advanced Search"
    //            ] 
    //        ]
    //    ]

    let alternative2 =
        tr [
            colorControl colorMode
            Class "suggestion"
        ][
            td [ColSpan 4] [
                str ("You can also request a term by opening an ")
                a [Href Shared.URLs.Nfdi4psoOntologyUrl; Target "_Blank"] [
                    str "Issue"
                ]
                str "."
            ]
        ]

    suggestions @ (*[alternative] @ *) [alternative2]



let autocompleteDropdownComponent (dispatch:Msg -> unit) (colorMode:ColorMode) (isVisible: bool) (isLoading:bool) (suggestions: ReactElement list)  =
    Container.container[ ] [
        Dropdown.content [Props [
            Style [
                if isVisible then Display DisplayOptions.Block else Display DisplayOptions.None
                //if model.ShowFillSuggestions then Display DisplayOptions.Block else Display DisplayOptions.None
                ZIndex "20"
                Width "100%"
                MaxHeight "400px"
                Position PositionOptions.Absolute
                BackgroundColor colorMode.ControlBackground
                BorderColor     colorMode.ControlForeground
                MarginTop "-0.5rem"
                OverflowY OverflowOptions.Scroll
            ]]
        ] [
            Table.table [Table.IsFullWidth] [
                if isLoading then
                    tbody [] [
                        tr [] [
                            td [Style [TextAlign TextAlignOptions.Center]] [
                                loadingComponent
                                br []
                            ]
                        ]
                    ]
                else
                    tbody [] suggestions
            ]

        
        ]
    ]

open Fable.Core.JsInterop

let autocompleteTermSearchComponent
    (dispatch: Msg -> unit)
    (colorMode:ColorMode)
    (model:Model)
    (inputPlaceholderText   : string)
    (inputSize              : ISize option)
    (autocompleteParams     : AutocompleteParameters)
    (isDisabled:bool)
    = 
    Control.div [
        Control.IsExpanded
        Control.HasIconRight
    ] [
        //AdvancedSearch.advancedSearchModal model autocompleteParams.ModalId autocompleteParams.InputId dispatch autocompleteParams.OnAdvancedSearch
        Input.input [
            Input.Disabled isDisabled
            Input.Placeholder inputPlaceholderText
            Input.ValueOrDefault autocompleteParams.QueryString
            match inputSize with
            | Some size -> Input.Size size
            | _ -> ()
            Input.OnChange (
                fun e -> e.Value |> autocompleteParams.OnInputChangeMsg |> dispatch
            )
            Input.Props [
                Style [BorderColor WordColors.Colorfull.gray40]
                OnDoubleClick (fun e ->
                    let currentState = TermSearch.findRelatedTermSearchState model autocompleteParams.TermSearchType
                    let parentChildState = TermSearch.tryFindParentChildTermSearchState model autocompleteParams.TermSearchType
                    if parentChildState.IsSome && parentChildState.Value.TermSearchText <> "" && currentState.TermSearchText = "" then
                        let parentOntInfo =
                            if parentChildState.Value.SelectedTerm.IsSome then
                                parentChildState.Value.SelectedTerm.Value
                                |> fun parentOnt -> { Name = parentOnt.Name; TermAccession = parentOnt.Accession }
                            else {Name = parentChildState.Value.TermSearchText; TermAccession = "" }
                        TermSearch.GetAllTermsByParentChildTerm (parentOntInfo, autocompleteParams.TermSearchType) |> TermSearchMsg |> dispatch
                    else
                        let v = Browser.Dom.document.getElementById autocompleteParams.InputId
                        v?value |> autocompleteParams.OnInputChangeMsg |> dispatch
                )
            ]
            Input.Id autocompleteParams.InputId  
        ]
        if autocompleteParams.IsSelectedTerm then
            Icon.icon [
                Icon.Size IsSmall; Icon.IsRight
            ] [
                Fa.i [ Fa.Solid.Fingerprint ] [ ]
            ]
        autocompleteDropdownComponent
            dispatch
            colorMode
            autocompleteParams.DropDownIsVisible
            autocompleteParams.DropDownIsLoading
            (createAutocompleteSuggestions dispatch colorMode autocompleteParams)
    ]