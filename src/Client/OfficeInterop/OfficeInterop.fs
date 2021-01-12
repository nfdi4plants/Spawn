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
        let paragraphs = context.document.getSelection().paragraphs
        let _ = paragraphs.load(propertyNames=U2.Case2(ResizeArray[|"items"|]))

        promise {

            let! paras =
                context.sync().``then``(fun e ->
                    paragraphs.items.[0].insertText("This is some new Text", InsertLocation.End)
                ) 

            return sprintf "Added custom text successfully." 
        }
    )