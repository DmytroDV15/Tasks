SELECT p.ProductID, p.ProductName
FROM Products p
WHERE p.ProductID NOT IN (
    SELECT od.ProductID
    FROM [Order Details] od
    JOIN Orders o ON od.OrderID = o.OrderID
    WHERE o.OrderDate >= DATE(
        (SELECT MAX(OrderDate) FROM Orders),
        '-90 day'
    )
);
