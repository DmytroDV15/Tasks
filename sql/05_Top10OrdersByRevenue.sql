SELECT o.OrderID, o.OrderDate, o.CustomerID, 
	SUM(od.UnitPrice * od.Quantity * (1 - od.Discount)) AS Revenue
FROM Orders o 
JOIN [Order Details] od ON o.OrderID = od.OrderID
GROUP BY o.OrderID, o.OrderDate, o.CustomerID
ORDER BY Revenue DESC
LIMIT 10
