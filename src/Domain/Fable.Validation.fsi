module Fable.Validation

open System
open System.Text.RegularExpressions

module ValidateRegexes =
    open System
    open System.Text.RegularExpressions

    val mail: Regex
    val url: Regex

type ValidateResult<'T> =
    | Valid of 'T
    | Invalid

    member IsValid_: bool
    member IsInvalid_: bool

type FieldInfo<'T, 'T0, 'E> =
    { key: string
      original: 'T
      result: ValidateResult<'T0>
      validator: Validator<'E> }

    member Replace: result: ValidateResult<'T1> -> FieldInfo<'T, 'T1, 'E>

and Validator<'E> =
    new: all: bool -> Validator<'E>
    member HasError: bool
    member Errors: Map<string, 'E list>
    member PushError: name: string -> error: 'E -> unit
    member Test: name: string -> value: 'T -> FieldInfo<'T, 'T, 'E>
    member inline TestAsync: name: string -> value: 'T -> Async<FieldInfo<'T, 'T, 'E>>
    member inline TestOne: value: 'T -> FieldInfo<'T, 'T, 'E>

    member inline TestOneOnlySome:
        value: 'T option -> ((FieldInfo<'T, 'T, 'E> -> FieldInfo<'T, 'T, 'E>) list -> 'T option)

    member inline TestOneOnlySomeAsync:
        value: 'T option -> ((Async<FieldInfo<'T, 'T, 'E>> -> Async<FieldInfo<'T, 'T, 'E>>) list -> Async<'T option>)

    member inline TestOneOnlyOk:
        value: Result<'T, 'TError> -> ((FieldInfo<'T, 'T, 'E> -> FieldInfo<'T, 'T, 'E>) list -> Result<'T, 'TError>)

    member inline TestOneOnlyOkAsync:
        value: Result<'T, 'TError> ->
            ((Async<FieldInfo<'T, 'T, 'E>> -> Async<FieldInfo<'T, 'T, 'E>>) list -> Async<Result<'T, 'TError>>)

    member End: input: FieldInfo<'T, 'T0, 'E> -> 'T0
    member EndAsync: input: Async<FieldInfo<'T, 'T0, 'E>> -> Async<'T0>
    member inline private ExecRules: rules: ('a -> 'a) list -> info: 'a -> map: ('a -> 'b) -> 'b

    member inline private ExecRulesAsync:
        rules: (Async<'a> -> Async<'a>) list -> info: Async<'a> -> map: ('a -> 'b) -> Async<'b>

    /// Test rules only if value is Some,
    /// it won't collect error if value is None
    member TestOnlySome:
        name: string -> value: 'T option -> rules: (FieldInfo<'T, 'T, 'E> -> FieldInfo<'T, 'T, 'E>) list -> 'T option

    /// Test rules only if value is Ok,
    /// it won't collect error if value is Error
    member TestOnlyOk:
        name: string ->
        value: Result<'T, 'TError> ->
        rules: (FieldInfo<'T, 'T, 'E> -> FieldInfo<'T, 'T, 'E>) list ->
            Result<'T, 'TError>

    /// Test rules only if value is Some,
    /// it won't collect error if value is None
    member TestOnlySomeAsync:
        name: string ->
        value: 'T option ->
        rules: (Async<FieldInfo<'T, 'T, 'E>> -> Async<FieldInfo<'T, 'T, 'E>>) list ->
            Async<'T option>

    /// Test rules only if value is Ok,
    /// it won't collect error if value is Error
    member TestOnlyOkAsync:
        name: string ->
        value: Result<'T, 'TError> ->
        rules: (Async<FieldInfo<'T, 'T, 'E>> -> Async<FieldInfo<'T, 'T, 'E>>) list ->
            Async<Result<'T, 'TError>>

    /// Validate with a custom tester, return ValidateResult DU to modify input value
    member IsValidOpt:
        tester: ('T0 -> ValidateResult<'T1>) -> error: 'E -> input: FieldInfo<'T, 'T0, 'E> -> FieldInfo<'T, 'T1, 'E>

    /// Validate with a custom tester, return bool
    member IsValid: tester: ('T0 -> bool) -> ('E -> FieldInfo<'T, 'T0, 'E> -> FieldInfo<'T, 'T0, 'E>)

    member IsValidOptAsync:
        tester: ('T0 -> Async<ValidateResult<'T1>>) ->
        error: 'E ->
        input: Async<FieldInfo<'T, 'T0, 'E>> ->
            Async<FieldInfo<'T, 'T1, 'E>>

    member IsValidAsync:
        tester: ('T0 -> Async<bool>) -> ('E -> Async<FieldInfo<'T, 'T0, 'E>> -> Async<FieldInfo<'T, 'T0, 'E>>)

    member Trim: input: FieldInfo<'T, string, string> -> FieldInfo<'T, string, string>
    /// Validate with `String.IsNullOrWhiteSpace`
    member NotBlank: err: 'E -> (FieldInfo<'T, string, 'E> -> FieldInfo<'T, string, 'E>)
    /// Test an option value is some and unwrap it
    /// it will collect error
    member IsSome: error: 'E -> (FieldInfo<'T, 'T0 option, 'E> -> FieldInfo<'T, 'T0, 'E>)
    /// Defaults of None value, it won't collect error
    member DefaultOfNone: defaults: 'T0 -> input: FieldInfo<'T, 'T0 option, 'E> -> FieldInfo<'T, 'T0, 'E>
    /// Test a Result value is Ok and unwrap it
    /// it will collect error
    member IsOk: error: 'E -> (FieldInfo<'T, Result<'T0, 'TError>, 'E> -> FieldInfo<'T, 'T0, 'E>)
    /// Defaults of Error value, it won't collect error
    member DefaultOfError: defaults: 'T0 -> input: FieldInfo<'T, Result<'T0, 'TError>, 'E> -> FieldInfo<'T, 'T0, 'E>
    /// Map a function or constructor to the value, aka lift
    /// fn shouldn't throw error, if it would, please using `t.To fn error`
    member Map: fn: ('T0 -> 'T1) -> (FieldInfo<'T, 'T0, 'E> -> FieldInfo<'T, 'T1, 'E>)
    /// Convert the input value by fn
    /// if fn throws error then it will collect error
    member To: fn: ('T0 -> 'T1) -> ('E -> FieldInfo<'T, 'T0, 'E> -> FieldInfo<'T, 'T1, 'E>)
    /// Convert a synchronize validate pipe to asynchronize
    member ToAsync: input: FieldInfo<'T, 'T0, 'E> -> Async<FieldInfo<'T, 'T0, 'E>>
    /// Greater then a value, if err is a string, it can contains `{min}` to reuse first param
    member Gt: min: 'a -> err: 'E -> (FieldInfo<'a0, 'a, 'E> -> FieldInfo<'a0, 'a, 'E>) when 'a: comparison
    /// Greater and equal then a value, if err is a string, it can contains `{min}` to reuse first param
    member Gte: min: 'a -> err: 'E -> (FieldInfo<'a0, 'a, 'E> -> FieldInfo<'a0, 'a, 'E>) when 'a: comparison
    /// Less then a value, if err is a string, it can contains `{max}` to reuse first param
    member Lt: max: 'a -> err: 'E -> (FieldInfo<'a0, 'a, 'E> -> FieldInfo<'a0, 'a, 'E>) when 'a: comparison
    /// Less and equal then a value, if err is a string, it can contains `{max}` to reuse first param
    member Lte: max: 'a -> err: 'E -> (FieldInfo<'a0, 'a, 'E> -> FieldInfo<'a0, 'a, 'E>) when 'a: comparison
    /// Max length, if err is a string, it can contains `{len}` to reuse first param
    member MaxLen: len: int -> err: 'E -> input: FieldInfo<'T, 'T0, 'E> -> FieldInfo<'T, 'T0, 'E> when 'T0 :> seq<'a>
    /// Min length, if err is a string, it can contains `{len}` to reuse first param
    member MinLen: len: int -> err: 'E -> input: FieldInfo<'T, 'T0, 'E> -> FieldInfo<'T, 'T0, 'E> when 'T0 :> seq<'a>
    member Enum: enums: 'T0 list -> ('E -> FieldInfo<'T, 'T0, 'E> -> FieldInfo<'T, 'T0, 'E>) when 'T0: equality
    member IsMail: error: 'E -> input: FieldInfo<'T, string, 'E> -> FieldInfo<'T, string, 'E>
    member IsUrl: error: 'E -> input: FieldInfo<'T, string, 'E> -> FieldInfo<'T, string, 'E>
    member Match: regex: Regex -> error: 'E -> input: FieldInfo<'T, string, 'E> -> FieldInfo<'T, string, 'E>
    member IsDegist: error: 'E -> input: FieldInfo<'T, string, 'E> -> FieldInfo<'T, string, 'E>

/// IsValid helper from Validator method for custom rule functions, you can also extend Validator class directly.
val isValid: (('T0 -> bool) -> 'E -> FieldInfo<'T, 'T0, 'E> -> FieldInfo<'T, 'T0, 'E>)
/// IsValidOpt helper from Validator method for custom rule functions, you can also extend Validator class directly.
val isValidOpt: (('T0 -> ValidateResult<'T1>) -> 'E -> FieldInfo<'T, 'T0, 'E> -> FieldInfo<'T, 'T1, 'E>)
/// IsValidAsync helper from Validator method for custom rule functions, you can also extend Validator class directly.
val isValidAsync: (('T -> Async<bool>) -> 'E -> Async<FieldInfo<'T0, 'T, 'E>> -> Async<FieldInfo<'T0, 'T, 'E>>)
/// IsValidOptAsync helper from Validator method for custom rule functions, you can also extend Validator class directly.
val isValidOptAsync:
val validateSync: all: bool -> tester: (Validator<'E> -> 'T) -> Result<'T, Map<string, 'E list>>
val validateAsync: all: bool -> tester: (Validator<'E> -> Async<'T>) -> Async<Result<'T, Map<string, 'E list>>>
/// validate all fields and return a custom type,
val inline all: tester: (Validator<'E> -> 'T) -> Result<'T, Map<string, 'E list>>
/// Exit after first error occurred and return a custom type
val inline fast: tester: (Validator<'E> -> 'T) -> Result<'T, Map<string, 'E list>>
val inline allAsync: tester: (Validator<'E> -> Async<'T>) -> Async<Result<'T, Map<string, 'E list>>>
val inline fastAsync: tester: (Validator<'E> -> Async<'T>) -> Async<Result<'T, Map<string, 'E list>>>
/// Validate single value
val single: tester: (Validator<'E> -> 'T) -> Result<'T, 'E list>
/// Validate single value asynchronize
val singleAsync: tester: (Validator<'E> -> Async<'T>) -> Async<Result<'T, 'E list>>
