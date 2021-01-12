module CustomComponents.Navbar


open Fable.React
open Fable.React.Props
open Fulma
open Model
open Messages

open Fable.FontAwesome

let navbarComponent (model : Model) (dispatch : Msg -> unit) =
    Navbar.navbar [
        Navbar.Color IsLink
        Navbar.Props [Props.Role "navigation"; AriaLabel "main navigation"];
    ] [
        Navbar.Brand.div [] [
            Navbar.Item.a [Navbar.Item.Props [Props.Href "https://csb.bio.uni-kl.de/"]] [
                img [Props.Src "../assets/CSB_Logo.png"]
            ]
            Navbar.burger [
                Navbar.Burger.IsActive model.SiteStyleState.BurgerVisible
                Navbar.Burger.OnClick (fun e -> ToggleNavbarBurger |> dispatch)
                Navbar.Burger.Props[
                        Role "button"
                        AriaLabel "menu"
                        Props.AriaExpanded false
            ] ] [
                span [AriaHidden true] []
                span [AriaHidden true] []
                span [AriaHidden true] []
            ]
        ]
        Navbar.menu [
            Navbar.Menu.Props [Id "navbarMenu"; Class (if model.SiteStyleState.BurgerVisible then "navbar-menu is-active" else "navbar-menu"); Style [ Background "0 0" ]]
        ] [
            Navbar.Start.div [] [
                Navbar.Item.a [Navbar.Item.Props [Style [ ]]] [
                    str "How to use"
                ]
            ]
            Navbar.End.div [] [
                Navbar.Item.a [Navbar.Item.Props [
                    Style [];
                    OnClick (fun e ->
                        ToggleNavbarBurger |> dispatch
                        UpdateActivePage (Some Routing.Route.Home) |> dispatch
                    )
                ]] [
                    str "Home"
                ]
                Navbar.Item.a [Navbar.Item.Props [
                    Style [];
                    OnClick (fun e ->
                        ToggleNavbarBurger |> dispatch
                        UpdateActivePage (Some Routing.Route.WordInterop) |> dispatch
                    )
                ]] [
                    str "WordInterop"
                ]
            ]
        ]
    ]