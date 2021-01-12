module Routing

open Elmish
open Elmish.UrlParser

type Route =
| Home
| WordInterop


let toRouteUrl route =
    match route with
    | Route.Home -> "/"
    | Route.WordInterop -> "/WordInterop"

let curry f x y = f (x,y)

module Routing =

    open Elmish.UrlParser
    open Elmish.Navigation

    let route =
        oneOf [
            map Route.Home (s "")
            map Route.WordInterop (s "WordInterop")
        ]

    let parsePath (location:Browser.Types.Location) : Route option = UrlParser.parsePath route location