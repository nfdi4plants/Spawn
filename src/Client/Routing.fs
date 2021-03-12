module Routing

open Elmish
open Elmish.UrlParser
open Fable.FontAwesome
open Fulma.Extensions.Wikiki

type Route =
| Home
| Comment
| Process
| ActivityLog
| Settings
| Info

    member this.toRouteUrl =
        match this with
        | Route.Home        -> "/"
        | Route.Comment     -> "/Comment"
        | Route.ActivityLog -> "/ActivityLog"
        | Route.Settings    -> "/Settings"
        | Route.Process     -> "/Process"
        | Route.Info        -> "/Info"

    member this.toStringRdbl =
        match this with
        | Route.Home                -> ""
        | Route.Comment             -> "Add Comment"
        | Route.Process             -> "Create Process"
        | Route.ActivityLog         -> "Activity Log"
        | Route.Settings            -> "Settings"
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
        | Route.Comment             -> createElem [Fa.Solid.CommentAlt          ]   (p.toStringRdbl)
        | Route.Process             -> createElem [Fa.Solid.FileCode            ]   (p.toStringRdbl)
        | Route.Settings            -> createElem [Fa.Solid.Cog                 ]   (p.toStringRdbl)
        | Route.ActivityLog         -> createElem [Fa.Solid.History             ]   (p.toStringRdbl)
        | Route.Info                -> createElem [Fa.Solid.Question            ]   (p.toStringRdbl)

let curry f x y = f (x,y)

module Routing =

    open Elmish.UrlParser
    open Elmish.Navigation

    let route =
        oneOf [
            map Route.Home          (s ""               )
            map Route.Comment       (s "Comment"        )
            map Route.Settings      (s "Settings"       )  
            map Route.Process       (s "Process"        )
            map Route.ActivityLog   (s "ActivityLog"    )
            map Route.Info          (s "Info"           )
        ]

    let parsePath (location:Browser.Types.Location) : Route option = UrlParser.parsePath route location