module CallHello

open EndpointTypes
open Fable.Remoting.DotnetClient

let private routeBuilder (typeName: string) (methodName: string) =
    sprintf "/api/%s/%s" typeName methodName

let private hello =
    Remoting.createApi ("http://localhost:8080")
    |> Remoting.withRouteBuilder routeBuilder
    |> Remoting.buildProxy<ApiEndpoints>

let callHello message = hello.hello message
