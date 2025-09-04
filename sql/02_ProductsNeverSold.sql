SELECT p.ProductID, p.ProductName
FROM Products p 
LEFT JOIN [Order Details] od ON p.ProductID = od.ProductID
WHERE od.ProductID IS NULL
