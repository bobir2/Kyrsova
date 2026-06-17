namespace Kursova.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public string? Barcode { get; set; }
    public string Unit { get; set; } = "шт";
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal Quantity { get; set; }
    public decimal MinStockLevel { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? CategoryName { get; set; }
    public string? SupplierName { get; set; }

    public bool IsLowStock => Quantity <= MinStockLevel;
    public bool IsExpiringSoon => ExpiryDate.HasValue && ExpiryDate.Value <= DateTime.Today.AddDays(7);
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.Today;
}
