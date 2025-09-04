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
