SELECT c.CategoryID, c.CategoryName,
	SUM(od.UnitPrice * od.Quantity * (1 - od.Discount)) AS TotalRevenue
FROM Categories c 
JOIN Products p ON c.CategoryID = p.CategoryID
JOIN [Order Details] od ON p.ProductID = od.ProductID 
GROUP BY c.CategoryName 
ORDER BY TotalRevenue DESC