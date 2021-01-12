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

let errorField model dispatch =
    div [][
        Notification.notification [
            Notification.Color IsDanger
            Notification.Props [Style [
                MaxWidth "900px";
                MarginTop "1rem"; MarginRight "0.35rem"; MarginLeft "0.35rem"
            ]]
        ] [
            Notification.delete [
                Props [OnClick (fun e -> UpdateDebug None |> dispatch )]
            ] [ ]
            str model.Debug.Value
        ]
    ]

let mainElement (model:Model) dispatch =
    Box.box' [][
        match model.Debug with
        | Some _ -> errorField model dispatch
        | None -> div [][]

        Content.content [][str "welcome Home"]
    ]

type Props = {
    Model: Model
    Dispatch: Msg -> unit
}

let view props = 
    mainElement props.Model props.Dispatch