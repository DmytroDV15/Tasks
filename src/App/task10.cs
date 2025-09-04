using Db;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

static class Task10
{
    public static async Task Run()
    {
        await SetupRowVersion();
        await ConcurrencyTest();
        await TransactionTest();
        await TransactionTest1();
    }
    static async Task SetupRowVersion()
    {
        using var context = new NorthwindDbContext();
        foreach (var p in context.Products)
        {
            if (p.RowVersion == null || p.RowVersion.Length == 0)
                p.RowVersion = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
        }
        await context.SaveChangesAsync();
    }

    static async Task ConcurrencyTest()
    {
        using var contex1 = new NorthwindDbContext();
        using var context2 = new NorthwindDbContext();

        var product1 = await contex1.Products.FirstAsync();
        var product2 = await context2.Products.FirstAsync(p => p.ProductId == product1.ProductId);

        product1.UnitPrice = (product1.UnitPrice ?? 0) + 1;
        await contex1.SaveChangesAsync();
        Console.WriteLine("Context1 saved");

        product2.UnitPrice = (product2.UnitPrice ?? 0) + 2;
        //coment out the next line to see successful save
        context2.Entry(product2).Property(p => p.RowVersion).OriginalValue =
            product2.RowVersion.Select(b => (byte)(b ^ 0xFF)).ToArray();

        try
        {
            await context2.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine("Concurrency conflict detected!");
        }
    }

    static async Task TransactionTest1()
    {
        int maxRetries = 3;
        int attempt = 0;
        bool success = false;

        while (!success && attempt < maxRetries)
        {
            attempt++;
            try
            {
                using var context = new NorthwindDbContext();
                using var transaction = await context.Database.BeginTransactionAsync();

                var products = await context.Products.Take(2).ToListAsync();
                products[0].UnitPrice = (products[0].UnitPrice ?? 0) + 5;
                products[1].UnitPrice = (products[1].UnitPrice ?? 0) + 10;

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                success = true;
                Console.WriteLine($"Transaction succeeded on attempt {attempt}");
            }
            catch (DbUpdateConcurrencyException)
            {
                Console.WriteLine($"Concurrency conflict on attempt {attempt}, retrying...");
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 5)
            {
                Console.WriteLine($"Database busy on attempt {attempt}, retrying...");
                await Task.Delay(100);
            }
        }
    }
    static async Task TransactionTest()
    {
        int maxRetries = 3;
        int attempt = 0;
        bool success = false;

        using var blockingContext = new NorthwindDbContext();
        using var blockingTransaction = await blockingContext.Database.BeginTransactionAsync();
        var blockedProduct = await blockingContext.Products.FirstAsync();
        blockedProduct.UnitPrice += 1;
        await blockingContext.SaveChangesAsync();

        while (!success && attempt < maxRetries)
        {
            attempt++;
            try
            {
                using var context = new NorthwindDbContext();
                using var transaction = await context.Database.BeginTransactionAsync();

                var products = await context.Products.Take(2).ToListAsync();
                products[0].UnitPrice += 5;
                products[1].UnitPrice += 10;

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                success = true;
                Console.WriteLine($"Transaction succeeded on attempt {attempt}");
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 5) 
            {
                Console.WriteLine($"Database busy on attempt {attempt}, retrying...");
                await Task.Delay(100);
            }
        }

        await blockingTransaction.CommitAsync();
    }
}
