var builder = DistributedApplication.CreateBuilder(args);

/* Docker setup */

var sqlServer = builder.AddSqlServer("sqlserver")
                       .WithDataVolume()
                       .AddDatabase("StockSyncDB");

var mongo = builder.AddMongoDB("mongodb")
                   .WithDataVolume();

/* Local setup (without docker DB containers) */

//var sqlServer = builder.AddConnectionString("sqlserver");
//var mongo = builder.AddConnectionString("mongodb");

var supplierService = builder.AddProject<Projects.StockSync_SupplierService>("stocksync-supplierservice")
                             .WithReference(sqlServer)
                             .WaitFor(sqlServer)
                             .WaitForCompletion(sqlServer);

var itemService = builder.AddProject<Projects.StockSync_ItemService>("stocksync-itemservice")
                         .WithReference(mongo)
                         .WaitFor(mongo)
                         .WaitForCompletion(mongo);

supplierService
    .WithReference(itemService)
    .WaitFor(itemService);

await builder.Build().RunAsync();
