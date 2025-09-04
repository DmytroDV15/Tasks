using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "CREATE INDEX IX_Orders_CustomerId_OrderDate ON Orders (CustomerID, OrderDate DESC)"
            );

            migrationBuilder.Sql(
                "CREATE INDEX IX_OrderDetails_OrderId ON [Order Details] (OrderID)"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IX_Orders_CustomerId_OrderDate ");
            migrationBuilder.Sql("DROP INDEX IX_OrderDetails_OrderId ");
        }
    }
}
