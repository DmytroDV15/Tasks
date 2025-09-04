SELECT s.SupplierID, s.CompanyName,
	COUNT(DISTINCT p.ProductID) AS ProductsCount,
	MIN(o.OrderDate) AS FirstOrderDate
FROM Suppliers s 
LEFT JOIN Products p ON s.SupplierID = p.SupplierID
LEFT JOIN [Order Details] od ON p.ProductID = od.ProductID 
LEFT JOIN Orders o ON od.OrderID = o.OrderID
GROUP BY s.SupplierID
ORDER BY s.SupplierID 
	