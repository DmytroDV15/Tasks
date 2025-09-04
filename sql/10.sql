SELECT OrderID, (JULIANDAY(ShippedDate) - JULIANDAY(RequiredDate)) AS DaysLate
FROM Orders
WHERE ShippedDate > RequiredDate
ORDER BY DaysLate DESC 