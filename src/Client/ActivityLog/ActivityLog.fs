module ActivityLog

open Fulma
open Fable
open Fable.React
open Fable.React.Props
open Fable.FontAwesome
open Fable.Core.JS
open Fable.Core.JsInterop

open Model
open Messages
open Browser.Types

//TO-DO: Save log as tab seperated file

let debugBox model dispatch =
    Box.box' [][
    ]

let activityLogComponent (model:Model) dispatch =
    div [][

        Label.label [Label.Size IsLarge; Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]] [str "Activity Log"]

        //debugBox model dispatch

        Label.label [Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]] [str "Display all recorded activities of this session."]
        Customcomponents.subModuleBox [
            Table.table [
                Table.IsFullWidth
                Table.Props [WordColors.colorBackground model.SiteStyleState.ColorMode]
            ] [
                tbody [] [
                    for log in model.DevState.Logs do
                        yield
                            log.toTableRow
                ]
            ]
        ]
    ]

