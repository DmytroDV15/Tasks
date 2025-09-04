SELECT c.CustomerID, c.CompanyName,
	SUM(od.UnitPrice * od.Quantity * (1 - od.Discount)) AS LifetimeValue
FROM Customers c 
JOIN Orders o ON c.CustomerID = o.CustomerID
JOIN [Order Details] od ON o.OrderID = od.OrderID 
GROUP BY c.CustomerID, c.CompanyName
ORDER BY LifetimeValue DESC
LIMIT 5
