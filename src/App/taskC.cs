using System.Diagnostics;
using Db;
using Microsoft.EntityFrameworkCore;

class taskC
{
    private readonly NorthwindDbContext _context;

    public taskC(NorthwindDbContext context)
    {
        _context = context;
    }
    public async Task Run()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var lastDayOfSales = await _context.Orders
            .AsNoTracking()
            .MaxAsync(o => o.OrderDate);

        var cutoffDate = lastDayOfSales.Value.AddMonths(-12);

        var topCustomers = await _context.Customers
            .AsNoTracking()
            .Select(c => new
            {
                c.CustomerId,
                c.CompanyName,
                RevenueLast12Months = c.Orders
                    .Where(o => o.OrderDate >= cutoffDate)
                    .SelectMany(o => o.OrderDetails)
                    .Sum(od => od.UnitPrice * od.Quantity * (1 - od.Discount)),

                LastOrderDate = c.Orders
                    .Where(o => o.OrderDate >= cutoffDate)
                    .Max(o => (DateTime?)o.OrderDate)
            })
            .OrderByDescending(c => c.RevenueLast12Months)
            .Take(10)
            .ToListAsync();

        stopwatch.Stop();

        foreach (var c in topCustomers)
        {
            Console.WriteLine(
                $"CustomerID: {c.CustomerId}, " +
                $"Company: {c.CompanyName}, " +
                $"Revenue: {c.RevenueLast12Months:F2}, " +
                $"LastOrder: {c.LastOrderDate}");
        }

        Console.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");
    }
}
