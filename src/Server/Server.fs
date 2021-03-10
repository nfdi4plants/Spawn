module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared
open Giraffe

let serviceApi = {
    getAppVersion = fun () -> async {return "0.0.1"(*System.AssemblyVersionInformation.AssemblyVersion*)}
}

let createIServiceAPIv1 =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue serviceApi
    |> Remoting.buildHttpHandler

open Saturn

let endpointPipe =
    pipeline {
        plug head
        plug requestId
        set_header "x-pipeline-type" "Api"
        set_header "Access-Control-Allow-Origin" "*"
        set_header "Access-Control-Allow-Credentials" "true"
        plug (Saturn.PipelineHelpers.enableCors CORS.defaultCORSConfig)
    }

let webApp =
    router {
        pipe_through endpointPipe

        get "/test/test1" (htmlString "<h1>Hi this is test response 1</h1>")
        forward @"" (fun next ctx ->
            createIServiceAPIv1 next ctx
        )

    }

open Giraffe
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder

/////https://github.com/giraffe-fsharp/Giraffe/blob/master/samples/IdentityApp/IdentityApp/Program.fs
//let configureServices (services : IServiceCollection) =

//    // Enable CORS
//    services.AddCors() |> ignore

//    // Configure Giraffe dependencies
//    services.AddGiraffe()

open Microsoft.AspNetCore.Cors.Infrastructure

//let configure_cors (builder : CorsPolicyBuilder) =
//    builder
//        .AllowAnyMethod()
//        .AllowAnyHeader()
//        .AllowAnyOrigin
//    |> ignore

let app =
    application {
        //service_config configureServices
        url "http://0.0.0.0:5000"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
        //use_cors "localhost:3000" configure_cors
    }

run app
