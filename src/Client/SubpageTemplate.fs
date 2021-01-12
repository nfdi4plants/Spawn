module SubpageTemplate
open Elmish
open Fable.React
open Fable.React.Props
open Fulma
open Fable.FontAwesome
open Shared

/// this needs to be copy pasted in "Model.fs"

type Model = {
    Loading: bool
    Debug: string option
}
    with
        static member init() = {
            Loading = false
            Debug = None
        }

/// this needs to be copy pasted in "Messages.fs"

type Msg =
| UpdateDebug of string option

let init () :  Model * Cmd<Msg> =
    let initialModel = {
        Loading = false
        Debug = None
    }
    initialModel, Cmd.none

let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
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
    div [] [
        match model.Debug with
        | Some _ -> errorField model dispatch
        | None -> div [][]

        str "Welcome HOME"
    ]

type Props = {
    Model: Model
    Dispatch: Msg -> unit
}

let view props = 
    mainElement props.Model props.Dispatch