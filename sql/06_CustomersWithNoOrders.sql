SELECT c.CustomerID, c.CompanyName 
FROM Customers c 
LEFT JOIN Orders o ON c.CustomerID = o.CustomerID 
where o.CustomerID IS NULL