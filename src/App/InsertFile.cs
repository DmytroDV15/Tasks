using System.Globalization;
using System.Text;

namespace task11;
public class InsertFile
{
    public void CreateFile()
    {
        var random = new Random();
        var sb = new StringBuilder();
        var totalRows = 100000;

        sb.AppendLine("ProductID,ProductName,SupplierID,CategoryID,QuantityPerUnit,UnitPrice,UnitsInStock,UnitsOnOrder,ReorderLevel,Discontinued");

        for (int i = 1; i <= totalRows; i++)
        {
            var productID = i;
            var productName = $"Product {i}";
            var supplierID = random.Next(1, 30);
            var categoryID = random.Next(1, 9);
            var quantityPerUnit = $"{random.Next(1, 100)} pcs";
            var unitPrice = (random.NextDouble() * 100).ToString("F2", CultureInfo.InvariantCulture);
            var unitsInStock = random.Next(0, 500);
            var unitsOnOrder = random.Next(0, 100);
            var reorderLevel = random.Next(0, 50);
            var discontinued = random.Next(0, 2);

            sb.AppendLine($"{productID},{productName},{supplierID},{categoryID},{quantityPerUnit},{unitPrice},{unitsInStock},{unitsOnOrder},{reorderLevel},{discontinued}");
        }

        File.WriteAllText("products.csv", sb.ToString());
    }
}
