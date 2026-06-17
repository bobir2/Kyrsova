using System.IO;
using Microsoft.Data.Sqlite;

namespace Kursova.Data;

public static class DatabaseInitializer
{
  private static readonly string DbPath = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
      "Kursova",
      "warehouse.db");

  public static string ConnectionString => $"Data Source={DbPath}";

  public static void Initialize()
  {
    var directory = Path.GetDirectoryName(DbPath)!;
    Directory.CreateDirectory(directory);

    using var connection = new SqliteConnection(ConnectionString);
    connection.Open();

    using var command = connection.CreateCommand();
    command.CommandText = """
        CREATE TABLE IF NOT EXISTS Categories (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL UNIQUE,
            Description TEXT
        );

        CREATE TABLE IF NOT EXISTS Suppliers (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Phone TEXT,
            Email TEXT,
            Address TEXT
        );

        CREATE TABLE IF NOT EXISTS Products (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            CategoryId INTEGER NOT NULL,
            SupplierId INTEGER,
            Barcode TEXT,
            Unit TEXT NOT NULL DEFAULT 'шт',
            PurchasePrice REAL NOT NULL DEFAULT 0,
            SalePrice REAL NOT NULL DEFAULT 0,
            Quantity REAL NOT NULL DEFAULT 0,
            MinStockLevel REAL NOT NULL DEFAULT 0,
            ExpiryDate TEXT,
            CreatedAt TEXT NOT NULL,
            FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
            FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
        );

        CREATE TABLE IF NOT EXISTS StockMovements (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            ProductId INTEGER NOT NULL,
            MovementType TEXT NOT NULL,
            Quantity REAL NOT NULL,
            UnitPrice REAL NOT NULL DEFAULT 0,
            Note TEXT,
            CreatedAt TEXT NOT NULL,
            FOREIGN KEY (ProductId) REFERENCES Products(Id)
        );
        """;
    command.ExecuteNonQuery();

    SeedIfEmpty(connection);
  }

  private static void SeedIfEmpty(SqliteConnection connection)
  {
    using var check = connection.CreateCommand();
    check.CommandText = "SELECT COUNT(*) FROM Categories";
    var count = Convert.ToInt32(check.ExecuteScalar());
    if (count > 0) return;

    using var seed = connection.CreateCommand();
    seed.CommandText = """
        INSERT INTO Categories (Name, Description) VALUES
            ('Молочні продукти', 'Молоко, сир, йогурт'),
            ('М''ясо та риба', 'Свіже та заморожене м''ясо'),
            ('Овочі та фрукти', 'Свіжі овочі та фрукти'),
            ('Бакалія', 'Крупи, макарони, консерви'),
            ('Напої', 'Вода, соки, безалкогольні напої');

        INSERT INTO Suppliers (Name, Phone, Email, Address) VALUES
            ('ТОВ АгроПродукт', '+380441234567', 'info@agro.ua', 'м. Київ, вул. Складська 1'),
            ('Фермерське господарство', '+380501112233', 'farm@mail.ua', 'Київська обл.'),
            ('ОптТорг', '+380672223344', 'opt@trade.ua', 'м. Львів');

        INSERT INTO Products (Name, CategoryId, SupplierId, Barcode, Unit, PurchasePrice, SalePrice, Quantity, MinStockLevel, ExpiryDate, CreatedAt) VALUES
            ('Молоко 2.5% 1л', 1, 1, '4820000001001', 'шт', 28.50, 35.00, 120, 30, date('now', '+14 days'), datetime('now')),
            ('Сир Гауда 200г', 1, 1, '4820000001002', 'шт', 65.00, 82.00, 45, 15, date('now', '+30 days'), datetime('now')),
            ('Куряче філе 1кг', 2, 2, '4820000002001', 'кг', 120.00, 155.00, 25, 10, date('now', '+5 days'), datetime('now')),
            ('Картопля', 3, 2, '4820000003001', 'кг', 12.00, 18.00, 200, 50, date('now', '+60 days'), datetime('now')),
            ('Гречка 1кг', 4, 3, '4820000004001', 'шт', 35.00, 48.00, 80, 20, date('now', '+365 days'), datetime('now')),
            ('Мінеральна вода 1.5л', 5, 3, '4820000005001', 'шт', 8.00, 14.00, 5, 20, date('now', '+180 days'), datetime('now'));
        """;
    seed.ExecuteNonQuery();
  }
}
