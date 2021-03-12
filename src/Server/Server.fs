module Server

open Fable.Remoting.DotnetClient

open Shared
open Giraffe

let accessSwateAPIv1 =
    Remoting.createApi @"https://swate.denbi.uni-tuebingen.de/api/IAnnotatorAPIv1/"
    |> Remoting.withRouteBuilder Route.builderToSwate
    |> Remoting.buildProxy<ISwateAPI>

let serviceApi = {
    getAppVersion = fun () -> async {return "0.0.1"(*System.AssemblyVersionInformation.AssemblyVersion*)}
}

let swateApi = {
    // Development
    getTestNumber               = fun ()    -> accessSwateAPIv1.getTestNumber()
    getTestString               = fun str   -> accessSwateAPIv1.getTestString str

    // Ontology related requests
    /// (name,version,definition,created,user)
    testOntologyInsert          = fun vals  -> accessSwateAPIv1.testOntologyInsert vals
    getAllOntologies            = fun ()    -> accessSwateAPIv1.getAllOntologies()
    
    // Term related requests
    getTermSuggestions                  = fun vals  -> accessSwateAPIv1.getTermSuggestions vals
    /// (nOfReturnedResults*queryString*parentOntology). If parentOntology = "" then isNull -> Error.
    getTermSuggestionsByParentTerm      = fun vals  -> accessSwateAPIv1.getTermSuggestionsByParentTerm vals
    ///
    getAllTermsByParentTerm             = fun vals  -> accessSwateAPIv1.getAllTermsByParentTerm vals
    /// (ontOpt,searchName,mustContainName,searchDefinition,mustContainDefinition,keepObsolete)
    getTermsForAdvancedSearch           = fun vals  -> accessSwateAPIv1.getTermsForAdvancedSearch vals
    
    getUnitTermSuggestions              = fun vals  -> accessSwateAPIv1.getUnitTermSuggestions vals
     
    getTermsByNames                     = fun vals  -> accessSwateAPIv1.getTermsByNames vals
    
    // Protocol apis
    getAllProtocolsWithoutXml       = fun ()  -> accessSwateAPIv1.getAllProtocolsWithoutXml()
    getProtocolsByName              = fun vals  -> accessSwateAPIv1.getProtocolsByName vals
    getProtocolXmlForProtocol       = fun vals  -> accessSwateAPIv1.getProtocolXmlForProtocol vals
    increaseTimesUsed               = fun vals  -> accessSwateAPIv1.increaseTimesUsed vals
}



open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn


let createISwateAPIv1 = 
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue swateApi
    |> Remoting.buildHttpHandler


let createIServiceAPIv1 =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue serviceApi
    |> Remoting.buildHttpHandler

let webApp =
    router {
        get "/test/test1" (htmlString "<h1>Hi this is test response 1</h1>")
        forward @"" (fun next ctx ->
            createIServiceAPIv1 next ctx
        )

        forward @"" (fun next ctx ->
            createISwateAPIv1 next ctx
        )
    }

let app =
    application {
        //service_config configureServices
        url "http://0.0.0.0:5000"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
