var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sqlserver")
                       .WithDataVolume()
                       .AddDatabase("StockSyncDB");

var mongo = builder.AddMongoDB("mongodb")
                   .WithDataVolume();

var redis = builder.AddRedis("redis")
                   .WithRedisCommander();

builder.AddProject<Projects.StockSync_SupplierService>("stocksync-supplierservice")
                             .WithReference(sqlServer)
                             .WithReference(redis)
                             .WaitFor(sqlServer)
                             .WaitForCompletion(sqlServer);

builder.AddProject<Projects.StockSync_ItemService>("stocksync-itemservice")
                         .WithReference(mongo)
                         .WithReference(redis)
                         .WaitFor(mongo)
                         .WaitForCompletion(mongo);

await builder.Build().RunAsync();
