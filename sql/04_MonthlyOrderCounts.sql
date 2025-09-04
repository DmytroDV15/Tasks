SELECT STRFTIME('%m', o.OrderDate) as Month,COUNT(o.OrderID) AS Count
FROM Orders o 
WHERE o.OrderDate >= DATE(
    (SELECT MAX(OrderDate) FROM Orders),
    '-12 months'
)
GROUP BY STRFTIME('%m', o.OrderDate)