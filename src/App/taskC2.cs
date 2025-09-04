using Db;
using Microsoft.EntityFrameworkCore;


class taskC2
{
    private readonly NorthwindDbContext _context;
    public taskC2(NorthwindDbContext context)
    {
        _context = context;
    }
    public async Task Run()
    {
        var newOrder = new Order
        {
            CustomerId = "ALFKI",
            EmployeeId = 1,
            OrderDate = DateTime.UtcNow
        };

        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync();

        Console.WriteLine($"CreatedAt: {newOrder.CreatedAt}, ModifiedAt: {newOrder.ModifiedAt}");

        newOrder.Freight = 123;
        await _context.SaveChangesAsync();

        Console.WriteLine($"After modification, ModifiedAt: {newOrder.ModifiedAt}");

        newOrder.IsDeleted = true;
        await _context.SaveChangesAsync();

        var orderFromDb = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderId == newOrder.OrderId);

        Console.WriteLine(orderFromDb == null
            ? "Soft-deleted order is filtered out"
            : "Order is still visible (ERROR)");
    }
}
