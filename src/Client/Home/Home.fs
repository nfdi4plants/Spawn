module Home

open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome
open Shared

open Model.Home
open Messages.Home


let init () :  Model * Cmd<Msg> =
    let initialModel = {
        Loading = false
        Debug = None
    }
    initialModel, Cmd.none

let update (msg : Msg) (currentModel : Model) : Model * Cmd<Messages.Msg> =
    match msg with
    | UpdateDebug stringOpt ->
        let nextModel = {
            currentModel with
                Debug = stringOpt
        }
        nextModel, Cmd.none
    //| _ ->
    //    currentModel,Cmd.none

let mainElement (model:Model.Model) dispatch =
    Container.container [][
        div [][ str "This is the Spawn web host. For a preview click on the following link." ]
        a [ Href (Routing.Route.ActivityLog.toRouteUrl) ] [ str Routing.Route.ActivityLog.toStringRdbl ]
    ]

type Props = {
    Model: Model.Model
    Dispatch: Msg -> unit
}

let view props = 
    mainElement props.Model props.Dispatch