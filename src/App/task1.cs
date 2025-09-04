using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Db;
using Microsoft.EntityFrameworkCore;

class Task1
{
    private readonly NorthwindDbContext _context;
    private readonly Stopwatch stopwatch = new Stopwatch();
    public Task1(NorthwindDbContext context)
    {
        _context = context;
    }
    public async Task Run()
    {
        //await task1();
        //await task2();
        //await task3();
        //await task4();
        //await task5();
        //await task6();
        //await task7();
        //await task8();
        await task8ExecuteSql();
        await task9();
        await task9ExevuteSql();
        //await task10();
    }

    public async Task task1()
    {
        Console.WriteLine("TASK1");

        var topCustomer = await _context.Customers
            .AsNoTracking()
            .Select(c => new
            {
                c.CustomerId,
                c.CompanyName,
                LifetimeValue = c.Orders
                    .SelectMany(o => o.OrderDetails)
                    .Sum(od => od.UnitPrice * od.Quantity * (1 - od.Discount))
            })
            .OrderByDescending(c => c.LifetimeValue)
            .Take(5)
            .ToListAsync();

        foreach (var c in topCustomer)
        {
            Console.WriteLine($"CustomerID: {c.CustomerId}, CompanyName: {c.CompanyName}, LifetimeValue: {c.LifetimeValue}");
        }
    }

    public async Task task2()
    {
        Console.WriteLine("TASK2");

        var neverSoldProducts = await _context.Products
            .AsNoTracking()
            .Where(p => !p.OrderDetails.Any())
            .ToListAsync();

        foreach (var p in neverSoldProducts)
        {
            Console.WriteLine($"ProductID: {p.ProductId}, ProductName: {p.ProductName}");
        }
    }

    public async Task task3()
    {
        var lastDayOfSales = await _context.Orders
            .AsNoTracking()
            .MaxAsync(o => o.OrderDate);

        var thresholdDate = lastDayOfSales.Value.AddDays(-90);
        Console.WriteLine("TASK3");

        var zeroSales = await _context.Products
            .AsNoTracking()
            .Where(p => !p.OrderDetails
                .Any(od => od.Order.OrderDate >= thresholdDate))
            .ToListAsync();

        foreach (var p in zeroSales)
        {
            Console.WriteLine($"ProductID: {p.ProductId}, ProductName: {p.ProductName}");
        }
    }

    public async Task task4()
    {
        var lastDayOfSales = await _context.Orders
            .AsNoTracking()
            .MaxAsync(o => o.OrderDate);

        var thresholdDate = lastDayOfSales.Value.AddMonths(-12);

        Console.WriteLine("TASK4");
        var monthlyOrderCount = await _context.Orders
            .AsNoTracking()
            .Where(o => o.OrderDate >= thresholdDate)
            .GroupBy(o => o.OrderDate.Value.Month)
            .Select(g => new
            {
                Month = g.Key,
                OrderCount = g.Count()
            })
            .OrderBy(r => r.Month)
            .ToListAsync();

        foreach (var entry in monthlyOrderCount)
        {
            Console.WriteLine($"Month: {entry.Month}, Count: {entry.OrderCount}");
        }
    }

    public async Task task5()
    {
        Console.WriteLine("TASK5");
        
        var top10Orders = await _context.Orders
            .AsNoTracking()
            .Select(o => new
            {
                o.OrderId,
                o.OrderDate,
                o.CustomerId,
                TotalAmount = o.OrderDetails
                    .Sum(od => od.UnitPrice * od.Quantity * (1 - od.Discount))
            })
            .OrderByDescending(o => o.TotalAmount)
            .Take(10)
            .ToListAsync();

        foreach (var order in top10Orders)
        {
            Console.WriteLine($"OrderID: {order.OrderId}, OrderDate: {order.OrderDate}, CustomerID: {order.CustomerId}, TotalAmount: {order.TotalAmount}");
        }
    }

    public async Task task6()
    {
        Console.WriteLine("TASK6");

        var customerWithNoOrders = await _context.Customers
            .AsNoTracking()
            .Where(c => !c.Orders.Any())
            .ToListAsync();

        foreach (var c in customerWithNoOrders)
        {
            Console.WriteLine($"CustomerID: {c.CustomerId}, CompanyName: {c.CompanyName}");
        }
    }

    public async Task task7()
    {
        Console.WriteLine("TASK7");
        var categorySalesRanking = await _context.Categories
            .AsNoTracking()
            .Select(c => new
            {
                c.CategoryId,
                c.CategoryName,
                TotalRevenue = c.Products
                    .SelectMany(p => p.OrderDetails)
                    .Sum(od => od.UnitPrice * od.Quantity * (1 - od.Discount))
            })
            .OrderByDescending(c => c.TotalRevenue)
            .ToListAsync();

        foreach (var c in categorySalesRanking)
        {
            Console.WriteLine($"CategoryID: {c.CategoryId}, CategoryName: {c.CategoryName}, TotalRevenue: {c.TotalRevenue}");
        }
    }

    public async Task task8()
    {
        Console.WriteLine("TASK8");

        var supplierFill = _context.Suppliers
            .AsNoTracking()
            .Select(s => new
            {
                s.SupplierId,
                s.CompanyName,
                ProductCount = s.Products.Count(),
                FirstOrderDate = s.Products
                    .SelectMany(p => p.OrderDetails)
                    .Select(od => od.Order.OrderDate)
                    .Min()
            })
            .OrderBy(s => s.SupplierId)
            .ToListAsync();

        foreach (var s in await supplierFill)
        {
            Console.WriteLine($"SupplierID: {s.SupplierId}, CompanyName: {s.CompanyName}, ProductCount: {s.ProductCount}, FirstOrderDate: {s.FirstOrderDate}");
        }
    }

    public async Task task8ExecuteSql()
    {
        Console.WriteLine("TASK8 ExecuteSql");
        
        stopwatch.Start();
        using var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT s.SupplierID, s.CompanyName,
	        COUNT(DISTINCT p.ProductID) AS ProductsCount,
	        MIN(o.OrderDate) AS FirstOrderDate
        FROM Suppliers s 
        LEFT JOIN Products p ON s.SupplierID = p.SupplierID
        LEFT JOIN [Order Details] od ON p.ProductID = od.ProductID 
        LEFT JOIN Orders o ON od.OrderID = o.OrderID
        GROUP BY s.SupplierID
        ORDER BY s.SupplierID 
	        ";

        stopwatch.Stop();
        Console.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var supplierId = reader.GetInt32(0);
            var companyName = reader.GetString(1);
            var productsCount = reader.GetInt32(2);
            var firstOrderDate = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3);
            Console.WriteLine($"SupplierID: {supplierId}, CompanyName: {companyName}, ProductsCount: {productsCount}, FirstOrderDate: {firstOrderDate}");
        }
    }
    public async Task task9()
    {
        Console.WriteLine("TASK9");

        stopwatch.Start();

        var minOrderDate = await _context.Orders.MinAsync(o => o.OrderDate);
        var cutoffDate = minOrderDate.Value.AddDays(-180);

        var firstOrderStats = await _context.Customers
            .AsNoTracking()
            .Select(c => new
            {
                c.CustomerId,
                c.CompanyName,
                FirstOrder = c.Orders
                    .OrderBy(o => o.OrderDate)
                    .Select(o => new
                    {
                        o.OrderDate,
                        Revenue = o.OrderDetails.Sum(od => od.UnitPrice * od.Quantity * (1 - od.Discount))
                    })
                    .FirstOrDefault()
            })
            .Where(x => x.FirstOrder != null
                        && x.FirstOrder.OrderDate >= cutoffDate
                        && x.FirstOrder.Revenue >= 1000)
            .OrderByDescending(x => x.FirstOrder.OrderDate)
            .ToListAsync();

                stopwatch.Stop();
                Console.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");

                foreach (var c in firstOrderStats)
                {
                    Console.WriteLine($"CustomerID: {c.CustomerId}, CompanyName: {c.CompanyName}, " +
                                      $"FirstOrderDate: {c.FirstOrder.OrderDate}, FirstOrderRevenue: {c.FirstOrder.Revenue}");
                }
    }


    public async Task task9ExevuteSql()
    {
        Console.WriteLine("TASK9 ExecuteSql ");

        stopwatch.Start();
        
        using var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            WITH FirstOrders AS (
                SELECT 
                    o.CustomerID,
                    o.OrderID,
                    o.OrderDate
                FROM Orders o
                WHERE o.OrderDate = (
                    SELECT MIN(o2.OrderDate)
                    FROM Orders o2
                    WHERE o2.CustomerID = o.CustomerID
                )
            )
            SELECT 
                c.CustomerID,
                c.CompanyName,
                f.OrderDate AS FirstOrderDate,
                SUM(od.UnitPrice * od.Quantity * (1 - od.Discount)) AS FirstOrderRevenue
            FROM FirstOrders f
            JOIN Customers c ON f.CustomerID = c.CustomerID
            JOIN [Order Details] od ON f.OrderID = od.OrderID
            WHERE f.OrderDate >= DATE((SELECT MIN(OrderDate) from Orders), '-180 day')
            GROUP BY c.CustomerID, c.CompanyName, f.OrderDate
            HAVING FirstOrderRevenue >= 1000
            ORDER BY f.OrderDate DESC;
            ";

        stopwatch.Stop();
        Console.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var customerId = reader.GetString(0);
            var companyName = reader.GetString(1);
            var firstOrderDate = reader.GetDateTime(2);
            var firstOrderRevenue = reader.GetDecimal(3);
            Console.WriteLine($"CustomerID: {customerId}, CompanyName: {companyName}, FirstOrderDate: {firstOrderDate}, FirstOrderRevenue: {firstOrderRevenue}");
        }
    }

    public async Task task10()
    {
        Console.WriteLine("TASK10");

        var lateOrders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.ShippedDate > o.RequiredDate)
            .Select(o => new
            {
                o.OrderId,
                o.ShippedDate,
                o.RequiredDate
            })
            .Take(20)
            .ToListAsync();

        var lateOrdersWithDays = lateOrders
            .Select(o => new
            {
                o.OrderId,
                DaysLate = (o.ShippedDate.Value - o.RequiredDate.Value).TotalDays
            })
            .OrderByDescending(o => o.DaysLate)
            .ToList();

        foreach (var o in lateOrdersWithDays)
        {
            Console.WriteLine($"OrderID: {o.OrderId}, DaysLate: {o.DaysLate}");
        }

    }
}

public class FirstOrderInfo
{
    public string CustomerId { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public DateTime FirstOrderDate { get; set; }
    public decimal FirstOrderRevenue { get; set; }
}
