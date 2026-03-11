var builder = DistributedApplication.CreateBuilder(args);

/* Docker setup */

var sqlServer = builder.AddSqlServer("sqlserver")
                       .WithDataVolume()
                       .AddDatabase("StockSyncDB");

var mongo = builder.AddMongoDB("mongodb")
                   .WithDataVolume();

/* Add Distributed caching (optional - not implemented */

//var redis = builder.AddRedis("redis")
//                   .WithRedisCommander();

/* Local setup (without docker) */

//var sqlServer = builder.AddConnectionString("sqlserver");
//var mongo = builder.AddConnectionString("mongodb");

builder.AddProject<Projects.StockSync_SupplierService>("stocksync-supplierservice")
                             .WithReference(sqlServer)
                             //.WithReference(redis)
                             .WaitFor(sqlServer)
                             .WaitForCompletion(sqlServer);

builder.AddProject<Projects.StockSync_ItemService>("stocksync-itemservice")
                         .WithReference(mongo)
                         //.WithReference(redis)
                         .WaitFor(mongo)
                         .WaitForCompletion(mongo);

await builder.Build().RunAsync();
