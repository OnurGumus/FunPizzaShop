module DB

open FluentMigrator
open System
open Microsoft.Extensions.DependencyInjection
open FluentMigrator.Runner
open Microsoft.Extensions.Configuration
open System.Collections.Generic

[<MigrationAttribute(2023_06_18_1829L)>]
type AddOffsetsTable =
    new: unit -> AddOffsetsTable
    inherit Migration
    override Up: unit -> unit
    override Down: unit -> unit

[<MigrationAttribute(2023_06_18_1900L)>]
type AddSpecialsTable =
    new: unit -> AddSpecialsTable
    inherit Migration
    override Up: unit -> unit
    override Down: unit -> unit

[<MigrationAttribute(2023_06_18_1901L)>]
type AddToppingsTable =
    new: unit -> AddToppingsTable
    inherit Migration
    override Up: unit -> unit
    override Down: unit -> unit

[<MigrationAttribute(2023_06_18_1956L)>]
type AddSeedData =
    new: unit -> AddSeedData
    inherit Migration
    override Up: unit -> unit
    override Down: unit -> unit

[<MigrationAttribute(2023_06_29_0301L)>]
type AddOrdersTable =
    new: unit -> AddOrdersTable
    inherit Migration
    override Up: unit -> unit
    override Down: unit -> unit

val updateDatabase: serviceProvider: IServiceProvider -> unit
val createServices: config: IConfiguration -> ServiceProvider
val init: config: IConfiguration -> unit
