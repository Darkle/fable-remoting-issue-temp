module API

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open System.Threading
open System.Threading.Tasks
open EndpointTypes
open CallHello

let apiRPCRouteBuilder (typeName: string) (methodName: string) =
    sprintf "/api/%s/%s" typeName methodName

let hello (message: string) =
    task {
        printfn "got to hello"
        printfn "%A" message
    }

let apiEndpoints: ApiEndpoints = { hello = hello }

let apiRPC =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder apiRPCRouteBuilder
    |> Remoting.fromValue apiEndpoints
    |> Remoting.buildHttpHandler

let webApp = choose [ apiRPC; RequestErrors.notFound (text "404 - Not Found") ]

type internal LifetimeEventsHostedServices(appLifetime: IHostApplicationLifetime) =
    let _appLifetime = appLifetime

    let onStarted () =
        try
            callHello "Hello World" |> ignore
        with e ->
            printfn "Error: %A" e

        ()

    interface IHostedService with

        member this.StartAsync(cancellationtoken: CancellationToken) =
            _appLifetime.ApplicationStarted.Register(Action onStarted) |> ignore
            Task.CompletedTask

        member this.StopAsync(cancellationtoken: CancellationToken) = Task.CompletedTask

let configureApp (app: IApplicationBuilder) = app.UseGiraffe webApp

let configureServices (services: IServiceCollection) =
    services.AddHostedService<LifetimeEventsHostedServices>() |> ignore
    services.AddGiraffe() |> ignore

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .UseUrls("http://localhost:8080")
                .Configure(configureApp)
                .ConfigureServices(configureServices)
            |> ignore)
        .Build()
        .Run()

    0
