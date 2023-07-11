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

let createApiEndpointsHandler endpoints =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder apiRPCRouteBuilder
    |> Remoting.fromValue endpoints
    |> Remoting.buildHttpHandler

let hello (message: string) =
    task {
        printfn "got to hello"
        printfn "%A" message
    }

let apiEndpoints: ApiEndpoints = { hello = hello }

let logsApiRPC = createApiEndpointsHandler apiEndpoints

let webApp = choose [ logsApiRPC; RequestErrors.notFound (text "404 - Not Found") ]

type internal LifetimeEventsHostedServices(appLifetime: IHostApplicationLifetime) =

    let _appLifetime = appLifetime

    let onStarted () =
        callHello "Hello World" |> ignore
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
