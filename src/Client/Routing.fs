module Routing

open Elmish
open Elmish.UrlParser
open Fable.FontAwesome
open Fulma.Extensions.Wikiki

type Route =
| Home
| WordInterop
| ActivityLog
| Info

    member this.toRouteUrl =
        match this with
        | Route.Home        -> "/"
        | Route.ActivityLog -> "/ActivityLog"
        | Route.WordInterop -> "/WordInterop"
        | Route.Info        -> "/Info"

    member this.toStringRdbl =
        match this with
        | Route.Home                -> ""
        | Route.WordInterop         -> "Word Interop"
        | Route.ActivityLog         -> "Activity Log"
        | Route.Info                -> "Info"

    static member toIcon (p: Route)=
        let createElem icons name =
            Fable.React.Standard.span [
                Fable.React.Props.Class (Tooltip.ClassName + " " + Tooltip.IsTooltipBottom + " " + Tooltip.IsMultiline)
                Tooltip.dataTooltip (name)
            ] (
                icons
                |> List.map ( fun icon -> Fa.span [icon] [] )
            )

        match p with
        | Route.Home                -> createElem [Fa.Solid.Home                ]   (p.toStringRdbl)
        | Route.WordInterop         -> createElem [Fa.Solid.FileWord            ]   (p.toStringRdbl)
        | Route.ActivityLog         -> createElem [Fa.Solid.History             ]   (p.toStringRdbl)
        | Route.Info                -> createElem [Fa.Solid.Question            ]   (p.toStringRdbl)

let curry f x y = f (x,y)

module Routing =

    open Elmish.UrlParser
    open Elmish.Navigation

    let route =
        oneOf [
            map Route.Home          (s ""               )
            map Route.WordInterop   (s "WordInterop"    )
            map Route.ActivityLog   (s "ActivityLog"    )
            map Route.Info          (s "Info"           )
        ]

    let parsePath (location:Browser.Types.Location) : Route option = UrlParser.parsePath route location