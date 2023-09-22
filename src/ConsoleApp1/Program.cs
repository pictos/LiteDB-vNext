﻿// SETUP //////////////////
const string VER = "v6-pointer";
var INSERT_1 = new Range(1, 100_000);
var DELETE_1 = new Range(1, 40_000);
var INSERT_2 = new Range(1, 30_000);
////////////////////////

// DATASETS
var insert1 = GetData(INSERT_1, 100, 300).ToArray();
var insert2 = GetData(INSERT_2, 5, 10).ToArray();

var delete1 = Enumerable.Range(DELETE_1.Start.Value, DELETE_1.End.Value).Select(x => new BsonInt32(x)).ToArray();
//var query1 = new Query { Where = "name like 'fernand%'" };
var query1 = new Query 
{
    Where = "age between 20 and 30 AND name like 'r%'",
    Includes = new BsonExpression[] { "country" },
    Limit = 15,
    Select = BsonExpression.Root(),
    OrderBy = new OrderBy("name", 1),
    

};
//var query1 = new AggregateQuery("age")
//{
//    Functions = new[]
//    {
//        new CountFunc("contador", "1"),
//        new CountFunc("contador_name", "name")
//    }
//};

// INITIALIZE
var filename = @$"C:\LiteDB\temp\{VER}\test-{DateTime.Now.Ticks}.db";
var settings = new EngineSettings { Filename = filename };
var db = new LiteEngine(settings);

// RUN 

//await db.OpenAsync();
//await db.InsertAsync("col1", insert1);
//await db.InsertAsync("col2", GetCountries());
//await db.EnsureIndexAsync("col1", "idx_name", "name", false);
//await db.ConsumeAsync("col1", query1, 1_000, 20);
//await db.ShutdownAsync();

await Run("Create new database", () => db.OpenAsync());

//await Run($"Insert {INSERT_1}", () => db.InsertAsync("col1", insert1));

//await Run($"Insert {INSERT_1}", async () =>
//{
//    var lotes = new List<(string, BsonDocument[])>();
//    lotes.Add(("col1", insert1[0..125_000]));
//    lotes.Add(("col2", insert1[125_001..250_000]));
//    lotes.Add(("col3", insert1[250_001..375_000]));
//    lotes.Add(("col4", insert1[375_001..500_000]));
//    //await Parallel.ForEachAsync(lotes, async (lote, ct) =>
//    foreach (var lote in lotes)
//    {
//        await db.InsertAsync(lote.Item1, lote.Item2);
//    }
//});

var list = BsonArray.FromArray(insert1);

await Run($"Create Collection 'col1'", () => db.CreateCollectionAsync("col1"));


await db.ExecuteScalarAsync("insert into col1 values @0", new BsonDocument() { ["0"] = list });




await Run($"EnsureIndex (age)", () => db.EnsureIndexAsync("col1", "idx_AGE", "age", false));
await Run($"EnsureIndex (name)", () => db.EnsureIndexAsync("col1", "idx_NAME", "name", false));

await db.ConsumeAsync("col1", query1, 1_000, 100);

//await db.ConsumeAsync("col1", query1, 1_000, 10);
//await db.ConsumeAsync("col2", query1, 1_000, 10);
//await db.ConsumeAsync("col3", query1, 1_000, 10);
//await db.ConsumeAsync("col4", query1, 1_000, 10);


//await Run($"EnsureIndex (name)", () => db.EnsureIndexAsync("col1", "idx_name", "name", false));
//
//await Run($"Query like name", () => db.ConsumeAsync(db.Query("col1", query1), 1_000, 5));
//
//
//await Run("Checkpoint", () => db.CheckpointAsync());
await Run("Shutdown", () => db.ShutdownAsync());

//await Run($"Create Collection 'col1'", () => db.CreateCollectionAsync("col1"));
//await Run($"Insert {INSERT_1}", () => db.InsertAsync("col1", insert1, BsonAutoId.Int32));
//await Run($"Query full 'col1'", () => db.ConsumeAsync(db.Query("col1", query1), 1_000));
//await Run($"EnsureIndex (age)", () => db.EnsureIndexAsync("col1", "idx_age", "age", false));
//await Run($"Delete ({DELETE_1})", () => db.DeleteAsync("col1", delete1));
//await Run($"Insert {INSERT_2}", () => db.InsertAsync("col1", insert2, BsonAutoId.Int32));
//await Run("Checkpoint", () => db.CheckpointAsync());
//await Run("Shutdown", () => db.ShutdownAsync());

db.Dispose();

Console.WriteLine();
//Profiler.PrintResults(filename);

#if DEBUG
Console.WriteLine($"# DEBUG - {VER}");
#else
Console.WriteLine($"# RELEASE - {VER}");
#endif


//Console.ReadKey();

//unsafe
//{
//    int colID = 1;

//    var buffer = new byte[10];

//    var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
//    var ptr = handle.AddrOfPinnedObject();
//    var myStruct = (MyStruct*)ptr;

//    myStruct->ExtendValue = (int)(colID << 24);

//    Console.WriteLine(myStruct->ExtendValue);

//    for (var i = 0; i < 10; i++)
//    {
//        var v = *(byte*)(ptr);
//        var bin = Convert.ToString(v, 2).PadLeft(8, '0');
//        Console.WriteLine(i + " - " + bin);
//        ptr++;
//    }

//}

//return;

