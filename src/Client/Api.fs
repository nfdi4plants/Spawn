module Api

open Fable.Remoting.Client

open Shared

let serviceApiv1 =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IServiceAPIv1>

let swateApiv1 =
    Remoting.createApi()
    |> Remoting.withBaseUrl @"https://cors-test.appspot.com/test"//"https://cors-test.appspot.com/test"//https://swate.denbi.uni-tuebingen.de/api/IAnnotatorAPIv1/getAllOntologies
    |> Remoting.withCredentials true
    |> Remoting.withRouteBuilder (fun x y -> "")
    |> Remoting.buildProxy<ISwateAPI>