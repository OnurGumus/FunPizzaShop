module DB

open FluentMigrator
open System
open Microsoft.Extensions.DependencyInjection
open FluentMigrator.Runner
open Microsoft.Extensions.Configuration
open System.Collections.Generic

[<MigrationAttribute(1L)>]
type Zero() =
    inherit Migration()

    override this.Up() =
       ()
       
    override this.Down() = ()

[<MigrationAttribute(2L)>]
type One() =
    inherit Migration()

    override this.Up() =
       ()
       
    override this.Down() = 
        try
            this.Execute.Sql("DELETE FROM SNAPSHOT") |> ignore
            this.Execute.Sql("DELETE FROM EVENT_JOURNAL") |> ignore
            this.Execute.Sql("DELETE FROM JOURNAL_METADATA") |> ignore
        with 
            | _ -> ()

[<MigrationAttribute(2023_06_18_1829L)>]
type AddOffsetsTable() =
    inherit Migration()

    override this.Up() =
        this.Create
            .Table("Offsets")
            .WithColumn("OffsetName").AsString().PrimaryKey()
            .WithColumn("OffsetCount").AsInt64().NotNullable().WithDefaultValue(0)
        |> ignore

        let dict:IDictionary<string,obj> = Dictionary()
        dict.Add("OffsetName","Users")
        dict.Add("OffsetCount",0L)

        this.Insert.IntoTable("Offsets").Row(dict) |> ignore
       
    override this.Down() = this.Delete.Table("Offsets") |> ignore

[<MigrationAttribute(2023_06_18_1900L)>]
type AddSpecialsTable() =
    inherit Migration()
    
    override this.Up() =
        this.Create
            .Table("Specials")
            .WithColumn("Id").AsInt64().PrimaryKey()
            .WithColumn("Name").AsString()
            .WithColumn("Description").AsString()
            .WithColumn("BasePrice").AsDecimal()
            .WithColumn("ImageUrl").AsString()
        |> ignore
           
        override this.Down() = this.Delete.Table("Specials") |> ignore

[<MigrationAttribute(2023_06_18_1901L)>]
type AddToppingsTable() =
    inherit Migration()
    
    override this.Up() =
        this.Create
            .Table("Toppings")
            .WithColumn("Id").AsInt64().PrimaryKey()
            .WithColumn("Name").AsString()
            .WithColumn("Price").AsDecimal()
        |> ignore
           
        override this.Down() = this.Delete.Table("Toppings") |> ignore

[<MigrationAttribute(2023_06_18_1956L)>]
type AddSeedData() =
    inherit Migration()
    
    override this.Up() =
            this.Insert.IntoTable("Specials").Row([
                "Id", box 1L
                "Name",  "Basic Cheese Pizza"
                "Description", "It's cheesy and delicious. Why wouldn't you want one?"
                "BasePrice", 9.99M
                "ImageUrl","img/pizzas/cheese.jpg"
            ] |> Map.ofList ) |> ignore

            this.Insert.IntoTable("Specials").Row([
                "Id", box 2L
                "Name",  "The Baconatorizor"
                "Description", "It has EVERY kind of bacon"
                "BasePrice", 11.99M
                "ImageUrl","img/pizzas/bacon.jpg"
            ] |> Map.ofList ) |> ignore

            this.Insert.IntoTable("Specials").Row([
                "Id", box 3L
                "Name",  "Classic pepperoni"
                "Description", "It's the pizza you grew up with, but Blazing hot!"
                "BasePrice", 10.50M
                "ImageUrl","img/pizzas/pepperoni.jpg"
            ] |> Map.ofList ) |> ignore

            this.Insert.IntoTable("Specials").Row([
                "Id", box 4L
                "Name",  "Buffalo chicken"
                "Description", "Spicy chicken, hot sauce and bleu cheese, guaranteed to warm you up"
                "BasePrice", 12.75M
                "ImageUrl","img/pizzas/meaty.jpg"
            ] |> Map.ofList ) |> ignore

            this.Insert.IntoTable("Specials").Row([
                "Id", box 5L
                "Name",  "Mushroom Lovers"
                "Description", "It has mushrooms. Isn't that obvious?"
                "BasePrice", 11.00M
                "ImageUrl","img/pizzas/mushroom.jpg"
            ] |> Map.ofList ) |> ignore

            this.Insert.IntoTable("Specials").Row([
                "Id", box 6L
                "Name", "The Brit"
                "Description", "When in London..."
                "BasePrice", 10.25M
                "ImageUrl","img/pizzas/brit.jpg"
            ] |> Map.ofList ) |> ignore

            this.Insert.IntoTable("Specials").Row([
                "Id", box 7L
                "Name", "Veggie Delight"
                "Description", "It's like salad, but on a pizza"
                "BasePrice", 11.50M
                "ImageUrl","img/pizzas/salad.jpg"
            ] |> Map.ofList ) |> ignore

            this.Insert.IntoTable("Specials").Row([
                "Id", box 8L
                "Name", "Margherita"
                "Description",  "Traditional Italian pizza with tomatoes and basil"
                "BasePrice", 9.99M
                "ImageUrl","img/pizzas/margherita.jpg"
            ] |> Map.ofList ) |> ignore

            this.Insert.IntoTable("Toppings").Row([
                "Id", box 1L
                "Name", "Mushrooms"
                "Price", 1.0M
            ] |> Map.ofList ) |> ignore

            this.Insert.IntoTable("Toppings").Row([
                "Id", box 2L
                "Name", "Duck sausage"
                "Price", 1.0M
            ] |> Map.ofList ) |> ignore

            this.Insert.IntoTable("Toppings").Row([
                "Id", box 3L
                "Name", "Venison meatballs"
                "Price", 2.4M
            ] |> Map.ofList ) |> ignore

            this.Insert.IntoTable("Toppings").Row([
                "Id", box 4L
                "Name", "Fresh tomatos"
                "Price", 1.4M
            ] |> Map.ofList ) |> ignore
           
           
           
        override this.Down() =  () |> ignore

[<MigrationAttribute(2023_06_29_0301L)>]
type AddOrdersTable() =
    inherit Migration()
    
    override this.Up() =
        this.Create
            .Table("Orders")
            .WithColumn("OrderId").AsString().PrimaryKey()
            .WithColumn("Version").AsInt64().NotNullable().Indexed().WithDefaultValue(0)
            .WithColumn("Offset").AsInt64().NotNullable().Unique().WithDefaultValue(0)
            .WithColumn("UserId").AsString().Indexed()
            .WithColumn("CreatedTime").AsDateTime()
            .WithColumn("DeliveryAddress").AsString()
            .WithColumn("DeliveryLocation").AsString()
            .WithColumn("CurrentLocation").AsString()
            .WithColumn("DeliveryStatus").AsString()
            .WithColumn("Pizzas").AsString()
        |> ignore

    override this.Down() =  this.Delete.Table("Orders") |> ignore

                        
let updateDatabase (serviceProvider: IServiceProvider) =
    let runner = serviceProvider.GetRequiredService<IMigrationRunner>()
    runner.MigrateUp()

let resetDatabase (serviceProvider: IServiceProvider) =
    let runner = serviceProvider.GetRequiredService<IMigrationRunner>()
    if runner.HasMigrationsToApplyRollback() then
        runner.RollbackToVersion(1L)

let createServices (config: IConfiguration) =
    let connString =
        config.GetSection(FunPizzaShop.Shared.Constants.ConnectionString).Value

    ServiceCollection()
        .AddFluentMigratorCore()
        .ConfigureRunner(fun rb ->
            rb
                .AddSQLite()
                .WithGlobalConnectionString(connString)
                .ScanIn(typeof<AddOffsetsTable>.Assembly)
                .For.Migrations()
            |> ignore)
        .AddLogging(fun lb -> lb.AddFluentMigratorConsole() |> ignore)
        .BuildServiceProvider(false)


let init (env: #_) =
    let config = env :> IConfiguration
    use serviceProvider = createServices config
    use scope = serviceProvider.CreateScope()
    updateDatabase scope.ServiceProvider
    
let reset (env: #_) =
    let config = env :> IConfiguration
    use serviceProvider = createServices config
    use scope = serviceProvider.CreateScope()
    resetDatabase scope.ServiceProvider
    init env
