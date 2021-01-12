module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared
open Giraffe

type Storage () =
    let todos = ResizeArray<_>()

    member __.GetTodos () =
        List.ofSeq todos

    member __.AddTodo (todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok ()
        else Error "Invalid todo"

let storage = Storage()

storage.AddTodo(Todo.create "Create new SAFE project") |> ignore
storage.AddTodo(Todo.create "Write your app") |> ignore
storage.AddTodo(Todo.create "Ship it !!!") |> ignore

let serviceApi = {
    getAppVersion = fun () -> async {return "0.0.1"(*System.AssemblyVersionInformation.AssemblyVersion*)}
}

let createIServiceAPIv1 =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue serviceApi
    |> Remoting.buildHttpHandler

let router = router {
    get "/test/test1" (htmlString "<h1>Hi this is test response 1</h1>")
    forward @"" (fun next ctx ->
        createIServiceAPIv1 next ctx
    )
}

let app =
    application {
        url "http://0.0.0.0:5000"
        use_router router
        memory_cache
        use_static "public"
        use_gzip
    }

run app
