module Api

open Fable.Remoting.Client

open Shared

let serviceApiv1 =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IServiceAPIv1>