﻿module FunPizzaShop.Query.API

open Microsoft.Extensions.Configuration
open Akka.Streams.Dsl
open Akka.Persistence.Query
open Akka.Streams
open Akkling.Streams
open FunPizzaShop.Shared.Model
open Thoth.Json.Net
open Projection
open FunPizzaShop.ServerInterfaces.Query

[<Interface>]
type IAPI =
    abstract Query<'t> :
        ?filter: Predicate *
        ?orderby: string *
        ?orderbydesc: string *
        ?thenby: string *
        ?thenbydesc: string *
        ?take: int *
        ?skip: int ->
            list<'t> Async

    abstract Subscribe: (DataEvent -> unit) -> IKillSwitch

let subscribeToStream source mat (sink:Sink<DataEvent,_>) =
    source
    |> Source.viaMat KillSwitch.single Keep.right
    |> Source.toMat (sink) Keep.both
    |> Graph.run mat

open FSharp.Data.Sql.Common
open Serilog
open System.Linq
open FunPizzaShop.Shared
open FunPizzaShop.Shared.Model.Pizza
open FunPizzaShop.Shared.Model.Authentication
open Command.Serialization

let api (config: IConfiguration) actorApi =
    let connString = config.GetSection(Constants.ConnectionString).Value

    let source = Projection.init connString actorApi

    subscribeToStream
        source
        actorApi.Materializer
        (Sink.ForEach(fun x -> Serilog.Log.Verbose("data event : {@dataevent}", x)))
    |> ignore

    let subscribeCmd =
        (fun (cb:DataEvent->unit) ->
            let sink = Sink.forEach (fun event -> cb (event))
            let ks, _ = subscribeToStream source actorApi.Materializer sink
            ks :> IKillSwitch)


    { new IAPI with
        override this.Subscribe(cb) = subscribeCmd (cb)

        override this.Query(?filter, ?orderby, ?orderbydesc, ?thenby, ?thenbydesc, ?take, ?skip) : Async<'t list> =
            let ctx = Sql.GetDataContext(connString)


            let rec eval2 (t) =
                match t with
                | Equal(s, n) -> <@@ fun (x: SqlEntity) -> x.GetColumn(s) = n @@>
                | NotEqual(s, n) -> <@@ fun (x: SqlEntity) -> x.GetColumn(s) <> n @@>
                | Greater(s, n) -> <@@ fun (x: SqlEntity) -> x.GetColumn(s) > n @@>
                | GreaterOrEqual(s, n) -> <@@ fun (x: SqlEntity) -> x.GetColumn(s) >= n @@>
                | Smaller(s, n) -> <@@ fun (x: SqlEntity) -> x.GetColumn(s) < n @@>
                | SmallerOrEqual(s, n) -> <@@ fun (x: SqlEntity) -> x.GetColumn(s) <= n @@>
                | And(t1, t2) -> <@@ fun (x: SqlEntity) -> (%%eval2 t1) x && (%%eval2 t2) x @@>
                | Or(t1, t2) -> <@@ fun (x: SqlEntity) -> (%%eval2 t1) x || (%%eval2 t2) x @@>
                | Not(t0) -> <@@ fun (x: SqlEntity) -> not ((%%eval2 t0) x) @@>

            let sortByEval column =
                <@@ fun (x: SqlEntity) -> x.GetColumn<System.IComparable>(column) @@>

            let augment db =
                let db =
                    match filter with
                    | Some filter ->
                        <@
                            query {
                                for c in (%db) do
                                    where ((%%eval2 filter) c)
                                    select c
                            }
                        @>
                    | None -> db

                let db =
                    match orderby with
                    | Some orderby ->
                        <@
                            query {
                                for c in (%db) do
                                    sortBy ((%%sortByEval orderby) c)
                                    select c
                            }
                        @>
                    | None ->
                        <@
                            query {
                                for c in (%db) do
                                    select c
                            }
                        @>

                let db =
                    match orderbydesc with
                    | Some orderbydesc ->
                        <@
                            query {
                                for c in (%db) do
                                    sortByDescending ((%%sortByEval orderbydesc) c)
                                    select c
                            }
                        @>
                    | None -> db

                let db =
                    match thenby with
                    | Some thenby ->
                        <@
                            query {
                                for c in (%db) do
                                    thenBy ((%%sortByEval thenby) c)
                                    select c
                            }
                        @>
                    | None -> db

                let db =
                    match thenbydesc with
                    | Some thenbydesc ->
                        <@
                            query {
                                for c in (%db) do
                                    thenByDescending ((%%sortByEval thenbydesc) c)
                                    select c
                            }
                        @>
                    | None -> db

                let db =
                    match take with
                    | Some take -> <@ (%db).Take(take) @>
                    | None -> db

                let db =
                    match skip with
                    | Some skip -> <@ (%db).Skip(skip) @>
                    | None -> db

                query {
                    for u in (%db) do
                        select u
                }

            let res = 
                if typeof<'t> = typeof<Pizza.PizzaSpecial> then
                    let items =
                        query {
                            for c in ctx.Main.Specials do
                                select c
                        }

                    augment <@ items @>
                    |> Seq.map (fun x ->
                        {
                                Name = x.Name |> ShortString.TryCreate |> forceValidate
                                Description = x.Description |> ShortString.TryCreate |> forceValidate
                                BasePrice = x.BasePrice |> Price.TryCreate |> forceValidate
                                ImageUrl = x.ImageUrl |> ShortString.TryCreate |> forceValidate
                                Id = x.Id |> SpecialId.TryCreate |> forceValidate
                        }
                        : Pizza.PizzaSpecial)
                    |> List.ofSeq
                    |> box

                elif typeof<'t> = typeof<Pizza.Topping> then
                    let items =
                        query {
                            for c in ctx.Main.Toppings do
                                select c
                        }

                    augment <@ items @>
                    |> Seq.map (fun x ->
                        {
                                Name = x.Name |> ShortString.TryCreate |> forceValidate
                                Price = x.Price |> Price.TryCreate |> forceValidate
                                Id = x.Id |> ToppingId.TryCreate |> forceValidate
                        }
                        : Pizza.Topping)
                    |> List.ofSeq
                    |> box

                elif typeof<'t> = typeof<Order> then
                    let items =
                        query {
                            for c in ctx.Main.Orders do
                                select c
                        }

                    augment <@ items @>
                    |> Seq.map (fun x ->
                        {
                                OrderId = x.OrderId |> ShortString.TryCreate |> forceValidate |> OrderId
                                UserId = x.UserId  |> UserId.TryCreate |> forceValidate
                                CreatedTime = x.CreatedTime 
                                DeliveryAddress = x.DeliveryAddress |> decode |> forceValidateWithString
                                DeliveryLocation = x.DeliveryLocation |> decode |> forceValidateWithString
                                Pizzas =  x.Pizzas |> decode |> forceValidateWithString
                                Version = x.Version |> Version
                                CurrentLocation = x.CurrentLocation |> decode |> forceValidateWithString
                                DeliveryStatus = x.DeliveryStatus |>  decode |> forceValidateWithString
                        }
                        : Order)
                    |> List.ofSeq
                    |> box
               
                else
                    failwith "not implemented"

            async { return res :?> list<'t> }
    }
