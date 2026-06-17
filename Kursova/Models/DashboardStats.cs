namespace Kursova.Models;

public class DashboardStats
{
    public int TotalProducts { get; set; }
    public int TotalCategories { get; set; }
    public int TotalSuppliers { get; set; }
    public decimal TotalStockValue { get; set; }
    public int LowStockCount { get; set; }
    public int ExpiringSoonCount { get; set; }
    public int ExpiredCount { get; set; }
}
