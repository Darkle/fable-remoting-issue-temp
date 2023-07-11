module EndpointTypes

open System.Threading.Tasks

type ApiEndpoints = { hello: string -> Task<unit> }
