using System.Diagnostics;
using System.Globalization;
using Db;

public static class Task11
{
    public static async Task Run()
    {
        var insertFile = new task11.InsertFile();

        insertFile.CreateFile();

        var filePath = "products.csv";

        var stopwatch = Stopwatch.StartNew();

        var lines = File.ReadAllLines(filePath).Skip(1);
        var products = lines.Select(line =>
        {
            var columns = line.Split(',');
            return new Product
            {
                ProductName = columns[1],
                SupplierId = int.Parse(columns[2]),
                CategoryId = int.Parse(columns[3]),
                QuantityPerUnit = columns[4],
                UnitPrice = double.Parse(columns[5], CultureInfo.InvariantCulture),
                UnitsInStock = int.Parse(columns[6]),
                UnitsOnOrder = int.Parse(columns[7]),
                ReorderLevel = int.Parse(columns[8]),
                Discontinued = columns[9] == "1"
            };
        }).ToList();

        using (var context = new NorthwindDbContext())
        using (var transaction = context.Database.BeginTransaction())
        {
            context.Products.AddRange(products);
            context.SaveChanges();
            transaction.Rollback();
        }
        stopwatch.Stop();
        Console.WriteLine($"Inserted {products.Count} products in {stopwatch.Elapsed.TotalSeconds:F2} sec");

        var stopwatch2 = Stopwatch.StartNew();

        var batchSize = 5000;

        using (var context = new NorthwindDbContext())
        using (var transaction = context.Database.BeginTransaction())
        {
            for (int i = 0; i < products.Count; i += batchSize)
            {
                var batch = products.Skip(i).Take(batchSize).ToList();
                context.Products.AddRange(batch);
                context.SaveChanges();
                context.ChangeTracker.Clear();
            }

            transaction.Rollback();
        }


        stopwatch2.Stop();
        Console.WriteLine($"Inserted {products.Count} products in batches of {batchSize} in {stopwatch2.Elapsed.TotalSeconds:F2} sec");
    }
}
