using System;
using System.Collections.Generic;

namespace Db;
public partial class OrderSubtotal
{
    public int? OrderId { get; set; }

    public double? Subtotal { get; set; }
}
