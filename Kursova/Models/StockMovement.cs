namespace Kursova.Models;

public enum MovementType
{
    Incoming,
    Outgoing
}

public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public MovementType MovementType { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? ProductName { get; set; }
    public string MovementTypeDisplay => MovementType == MovementType.Incoming ? "Надходження" : "Відвантаження";
}
