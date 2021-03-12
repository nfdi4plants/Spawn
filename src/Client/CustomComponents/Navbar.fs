module CustomComponents.Navbar

open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome
open Fulma.Extensions.Wikiki

open WordColors
open Model
open Messages

type ShortCutIcon = {
    Description : string
    FaList      : ReactElement list
    Msg         : Msg
    Category    : string
} with
    static member create description faList msg category = {
        Description = description
        FaList      = faList
        Msg         = msg
        Category    = category
    }

let shortCutIconList model =
    [
        //ShortCutIcon.create
        //    "Add Building Block"
        //    [
        //        Fa.span [Fa.Solid.Plus][]
        //        Fa.span [Fa.Solid.Table][]
        //    ]
        //    (Process.CreateNewBuildingBlock)
        //    "Building Block"
        //ShortCutIcon.create
        //    "Autoformat Table"
        //    [
        //        Fa.i [Fa.Solid.SyncAlt][]
        //    ]
        //    (Msg.Batch [
        //        AutoFitTable |> ExcelInterop
        //        UpdateProtocolGroupHeader |> ExcelInterop
        //    ])
        //    "Formatting"
        //ShortCutIcon.create
        //    "Update Reference Columns"
        //    [
        //        Fa.span [Fa.Solid.EyeSlash][]
        //        span [][str model.ExcelState.FillHiddenColsStateStore.toReadableString]
        //        Fa.span [Fa.Solid.Pen][]
        //    ]
        //    (FillHiddenColsRequest |> ExcelInterop)
        //    "Formatting"
        //ShortCutIcon.create
        //    "Remove Building Block"
        //    [ 
        //        Fa.span [Fa.Solid.Minus; Fa.Props [Style [PaddingRight "0.15rem"]]][]
        //        Fa.span [Fa.Solid.Columns][]
        //    ]
        //    (RemoveAnnotationBlock |> ExcelInterop)
        //    "BuildingBlock"
        //ShortCutIcon.create
        //    "Get Building Block Information"
        //    [ 
        //        Fa.span [Fa.Solid.Question; Fa.Props [Style [PaddingRight "0.15rem"]]][]
        //        span [][str model.BuildingBlockDetailsState.CurrentRequestState.toStringMsg]
        //        Fa.span [Fa.Solid.Columns][]
        //    ]
        //    (GetSelectedBuildingBlockSearchTerms |> ExcelInterop)
        //    "BuildingBlock"
    ]
    
let navbarShortCutIconList model dispatch =
    let padding = "0.5rem"
    [
        for icon in shortCutIconList model do
            yield 
                Button.a [
                    Button.CustomClass (Tooltip.ClassName + " " + Tooltip.IsTooltipBottom + " " + Tooltip.IsMultiline)
                    Button.Props [ Style [BackgroundColor model.SiteStyleState.ColorMode.ElementBackground; PaddingLeft padding; PaddingRight padding]; Tooltip.dataTooltip (icon.Description) ]
                    Button.OnClick (fun _ ->
                        icon.Msg |> dispatch
                    )
                    Button.Color Color.IsWhite
                    Button.IsInverted
                ] icon.FaList
    ]

let dropdownShortCutIconList model dispatch =
    Table.table [
        Table.Props [Style [ BackgroundColor model.SiteStyleState.ColorMode.ElementBackground; Color "white"; Cursor "default" ]]
    ][
        tbody [][
            for icon in shortCutIconList model do
                yield
                    tr [][
                        td [][Help.help [][ str icon.Description]]
                        td [][
                            let padding = "0.5rem"
                            Button.a [
                                Button.Props [ Style [BackgroundColor model.SiteStyleState.ColorMode.ElementBackground; PaddingLeft padding; PaddingRight padding]]
                                Button.OnClick (fun _ ->
                                    icon.Msg |> dispatch
                                )
                                Button.Color Color.IsWhite
                                Button.IsInverted
                            ] icon.FaList
                        ]
                    ]
        ]
    ]

//let quickAccessDropdownElement model dispatch =
//    Navbar.Item.div [
//        Navbar.Item.Props [
//            OnClick (fun e -> ToggleQuickAcessIconsShown |> StyleChange |> dispatch)
//            Style [ Color model.SiteStyleState.ColorMode.Text]
//        ]
//        Navbar.Item.CustomClass "hideOver575px"
//    ] [
//        div [Style [
//            Position PositionOptions.Relative
//        ]] [
//            Button.a [
//                Button.Props [Style [BackgroundColor model.SiteStyleState.ColorMode.ElementBackground; PaddingLeft "0"; PaddingRight "0"]]
//                Button.Color Color.IsWhite
//                Button.IsInverted
//            ][
//                div [Style [
//                    Position PositionOptions.Relative
//                ]] [
//                    Fa.i [
//                        Fa.Props [Style [
//                            Position PositionOptions.Absolute
//                            Top "0"
//                            Left "0"
//                            Display DisplayOptions.Block
//                            Transition "opacity 0.25s, transform 0.25s"
//                            if model.SiteStyleState.QuickAcessIconsShown then Opacity "1" else Opacity "0"
//                            if model.SiteStyleState.QuickAcessIconsShown then Transform "rotate(-180deg)" else Transform "rotate(0deg)"
//                        ]]
//                        Fa.Solid.Times
//                    ][]
//                    Fa.i [
//                        Fa.Props [Style [
//                            Position PositionOptions.Absolute
//                            Top "0"
//                            Left "0"
//                            Display DisplayOptions.Block
//                            Transition "opacity 0.25s, transform 0.25s"
//                            if model.SiteStyleState.QuickAcessIconsShown then Opacity "0" else Opacity "1"
//                        ]]
//                        Fa.Solid.EllipsisH
//                    ][]
//                    // Invis placeholder to create correct space (Height, width, margin, padding, etc.)
//                    Fa.i [
//                        Fa.Props [Style [
//                            Display DisplayOptions.Block
//                            Opacity "0" 
//                        ]]
//                        Fa.Solid.EllipsisH
//                    ][]
//                ]
//            ]
//            div [
//                Class "arrow_box"
//                Style [
//                    Display (if model.SiteStyleState.QuickAcessIconsShown then DisplayOptions.Block else DisplayOptions.None)
//                    Position PositionOptions.Absolute
//                    ZIndex "20"
//                ]
//            ][
//                dropdownShortCutIconList model dispatch
//            ]
//        ]
//    ]

let quickAccessListElement model dispatch =
    Navbar.Item.div [
        Navbar.Item.Props [Style [ Color model.SiteStyleState.ColorMode.Text]]
        Navbar.Item.CustomClass "hideUnder575px"
    ] [
        yield! navbarShortCutIconList model dispatch 
    ]


let navbarComponent (model : Model) (dispatch : Msg -> unit) =
    Navbar.navbar [Navbar.Props [Props.Role "navigation"; AriaLabel "main navigation" ; WordColors.colorElement model.SiteStyleState.ColorMode]] [
        Navbar.Brand.a [] [
            Navbar.Item.a [Navbar.Item.Props [Props.Href "https://csb.bio.uni-kl.de/"; Target "_Blank"; Style [Width "100px"]]] [
                img [Props.Src @"assets\Spawn_logo_for_word_mint.svg"] //Spawn_logo_for_word
            ]

            quickAccessListElement model dispatch

            //quickAccessDropdownElement model dispatch

            Navbar.burger [
                Navbar.Burger.IsActive model.SiteStyleState.BurgerVisible
                Navbar.Burger.OnClick (fun e -> ToggleNavbarBurger |> dispatch)
                Navbar.Burger.Modifiers [Modifier.TextColor IsWhite]
                Navbar.Burger.Props [
                        Role "button"
                        AriaLabel "menu"
                        Props.AriaExpanded false

            ]] [
                span [AriaHidden true] [ ]
                span [AriaHidden true] [ ]
                span [AriaHidden true] [ ]
            ]
        ]
        Navbar.menu [Navbar.Menu.Props [Id "navbarMenu"; Class (if model.SiteStyleState.BurgerVisible then "navbar-menu is-active" else "navbar-menu") ; WordColors.colorControl model.SiteStyleState.ColorMode]] [
            Navbar.Dropdown.div [ ] [
                Navbar.Item.a [Navbar.Item.Props [Href @"https://github.com/nfdi4plants/Spawn/issues/new/choose"; Target "_Blank"; Style [ Color model.SiteStyleState.ColorMode.Text]]] [
                    str "Contact"
                ]
                Navbar.Item.a [Navbar.Item.Props [
                    Style [ Color model.SiteStyleState.ColorMode.Text]
                    OnClick (fun e ->
                        ToggleNavbarBurger |> dispatch
                        UpdateActivePage (Some Routing.Settings) |> dispatch
                    )
                ]] [
                    str "Settings"
                ]
                Navbar.Item.a [Navbar.Item.Props [
                    Style [ Color model.SiteStyleState.ColorMode.Text];
                    OnClick (fun e ->
                        ToggleNavbarBurger |> dispatch
                        UpdateActivePage (Some Routing.ActivityLog) |> dispatch
                    )
                ]] [
                    str "Activity Log"
                ]
            ]
        ]
    ]