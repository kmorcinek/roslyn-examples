using System.Collections.Generic;
using System.Linq;

namespace SampleApp;

public class OrderService
{
    private readonly List<Order> _orders;

    public OrderService(List<Order> orders)
    {
        _orders = orders;
    }

    public int GetPendingOrderCount()
    {
        // Should be refactored: Where + Count -> Count
        return _orders.Count(o => o.Status == "Pending");
    }

    public int GetExpensiveOrderCount(decimal threshold)
    {
        // Should be refactored: Where + Count -> Count
        return _orders.Count(o => o.Total > threshold);
    }

    public int GetTotalOrders()
    {
        // No predicate — should NOT be changed
        return _orders.Count();
    }
}

public class Order
{
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
