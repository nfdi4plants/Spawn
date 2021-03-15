module OfficeInterop

open Fable.Core
open Fable.Core.JsInterop
open Elmish
open Browser
open System
open System.Collections.Generic


open OfficeJS
open Word

[<Global>]
let Office : Office.IExports = jsNative

[<Global>]
let Word : Word.IExports = jsNative

[<Global>]
let RangeLoadOptions : Interfaces.RangeLoadOptions = jsNative

[<Emit("console.log($0)")>]
let consoleLog (message: string): unit = jsNative
        //ranges.format.fill.color <- "red"
        //let ranges = context.workbook.getSelectedRanges()
        //let x = ranges.load(U2.Case1 "address")

let exampleExcelFunction () =
    Word.run(fun context ->
        let selection = context.document.getSelection()
        let _ = selection.load(propertyNames=U2.Case2(ResizeArray[|"text"|]))

        promise {

            let! paras =
                context.sync().``then``(fun e ->
                    selection.text
                ) 

            return sprintf "Selected Text: %s." paras
        }
    )

let getSelectedText() =
    Word.run(fun context ->
        let selection = context.document.getSelection()
        let _ = selection.load(propertyNames=U2.Case2(ResizeArray[|"text"|]))

        promise {

            let! selectedText =
                context.sync().``then``(fun e ->
                    selection.text
                ) 

            return selectedText
        }
    )
