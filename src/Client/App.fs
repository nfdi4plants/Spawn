module App

open Elmish
open Elmish.Navigation
open Elmish.React
open Routing

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram Index.init Update.update Index.view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.toNavigable Routing.parsePath Update.urlUpdate
|> Program.withReactBatched "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
