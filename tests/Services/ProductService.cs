using System.Collections.Generic;
using System.Linq;

namespace SampleApp;

public class ProductService
{
    private readonly List<Product> _products;

    public ProductService(List<Product> products)
    {
        _products = products;
    }

    public int CountAvailableProducts()
    {
        // Should be refactored: Where + Count -> Count
        return _products.Where(p => p.IsAvailable).Count();
    }

    public int CountExpensiveProducts(decimal minPrice)
    {
        // Should be refactored: Where + Count -> Count
        return _products
            .Where(p => p.Price > minPrice)
            .Count();
    }

    public int GetTotalProducts()
    {
        // No predicate — should NOT be changed
        return _products.Count();
    }
}

public class Product
{
    public bool IsAvailable { get; set; }
    public decimal Price { get; set; }
}
