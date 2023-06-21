module FunPizzaShop.Domain.Model

open System
open Fable.Validation
open FsToolkit.ErrorHandling
open Thoth.Json

let extraEncoders = Extra.empty |> Extra.withInt64 |> Extra.withDecimal

let inline forceValidate (e) =
    match e with
    | Ok x -> x
    | Error x ->
        let errors = x |> List.map (fun x -> x.ToString()) |> String.concat ", "
        invalidOp errors

type Predicate =
    | Greater of string * int64
    | GreaterOrEqual of string * int64
    | Smaller of string * int64
    | SmallerOrEqual of string * int64
    | Equal of string * obj
    | NotEqual of string * obj
    | And of Predicate * Predicate
    | Or of Predicate * Predicate
    | Not of Predicate

type Version =
    | Version of int64

    member this.Value: int64 = let (Version lng) = this in lng
    member this.Zero: Version = Version 0L

type ShortStringError =
    | EmptyString
    | TooLongString

type ShortString =
    private
    | ShortString of string

    member this.Value = let (ShortString s) = this in s

    static member TryCreate(s: string) =
        single (fun t ->
            t.TestOne s
            |> t.MinLen 1 EmptyString
            |> t.MaxLen 255 TooLongString
            |> t.Map ShortString
            |> t.End)

    static member Validate(s: ShortString) =
        s.Value |> ShortString.TryCreate |> forceValidate

    override this.ToString() = this.Value

type LongStringError = EmptyString

type LongString =
    private
    | LongString of string

    member this.Value = let (LongString lng) = this in lng

    static member TryCreate(s: string) =
        single (fun t -> t.TestOne s |> t.MinLen 1 EmptyString |> t.Map LongString |> t.End)

    static member Validate(s: LongString) =
        s.Value |> LongString.TryCreate |> forceValidate

    override this.ToString() = this.Value

type PriceError =
    | Negative

type Price =
    private
    | Price of decimal

    member this.Value = let (Price s) = this in s

    static member TryCreate(s: decimal) =
        single (fun t ->
            t.TestOne s
            |> t.Gte 0m Negative
            |> t.Map Price
            |> t.End)

    static member Validate(s: Price) =
        s.Value |> Price.TryCreate |> forceValidate

    override this.ToString() = this.Value.ToString("0.00")

module Pizza =

    type SpecialId =
        private
        | SpecialId of int64

        member this.Value = let (SpecialId pizzaId) = this in pizzaId

        static member TryCreate(s: int64) =
            single (fun t -> t.TestOne s |> t.Map SpecialId |> t.End)

        static member Validate(s: SpecialId) =
            s.Value |> SpecialId.TryCreate |> forceValidate

        override this.ToString() = this.Value.ToString()

    /// <summary>
    /// Represents a pre-configured template for a pizza a user can order
    /// </summary>
    [<CLIMutable>]
    type PizzaSpecial = {
        Id: SpecialId
        Name: ShortString
        BasePrice: Price
        Description: ShortString
        ImageUrl: ShortString
    } with

        member this.FormattedBasePrice = this.BasePrice.ToString()

    [<CLIMutable>]
    type Topping = {
        Id: int64
        Name: ShortString
        Price: Price
    } with

        member this.FormattedBasePrice = this.Price.ToString()

    [<CLIMutable>]
    type Pizza = {
        Id: Guid
        Special: PizzaSpecial
        SpecialId: SpecialId
        Size: int64
        Toppings: Topping list
    } with

        static member DefaultSize = 12
        static member MinimumSize = 9
        static member MaximumSize = 17

        member this.BasePrice =
            (this.Size |> decimal) / (Pizza.DefaultSize |> decimal) * this.Special.BasePrice.Value
            |> Price.TryCreate |> forceValidate

        member this.TotalPrice =
            this.BasePrice.Value + (this.Toppings |> List.sumBy (fun t -> t.Price.Value))
            |> Price.TryCreate |> forceValidate

        member this.FormattedTotalPrice = this.TotalPrice.ToString()

        static member CreatePizzaFromSpecial(special: PizzaSpecial) = {
            Id = Guid.NewGuid()
            Special = special
            SpecialId = special.Id
            Size = Pizza.DefaultSize
            Toppings = []
        }
