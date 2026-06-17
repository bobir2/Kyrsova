using System.Globalization;
using Kursova.Models;
using Microsoft.Data.Sqlite;

namespace Kursova.Data;

public class WarehouseRepository
{
  private static string? FormatDate(DateTime? date) =>
      date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

  private static DateTime? ParseDate(string? value) =>
      DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;

  private static DateTime ParseDateTime(string value) =>
      DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

  public List<Category> GetCategories()
  {
    using var connection = Open();
    using var command = connection.CreateCommand();
    command.CommandText = "SELECT Id, Name, Description FROM Categories ORDER BY Name";
    return ReadCategories(command);
  }

  public void AddCategory(Category category)
  {
    using var connection = Open();
    using var command = connection.CreateCommand();
    command.CommandText = "INSERT INTO Categories (Name, Description) VALUES ($name, $desc); SELECT last_insert_rowid();";
    command.Parameters.AddWithValue("$name", category.Name);
    command.Parameters.AddWithValue("$desc", category.Description ?? (object)DBNull.Value);
    category.Id = Convert.ToInt32(command.ExecuteScalar());
  }

  public void UpdateCategory(Category category)
  {
    using var connection = Open();
    using var command = connection.CreateCommand();
    command.CommandText = "UPDATE Categories SET Name = $name, Description = $desc WHERE Id = $id";
    command.Parameters.AddWithValue("$id", category.Id);
    command.Parameters.AddWithValue("$name", category.Name);
    command.Parameters.AddWithValue("$desc", category.Description ?? (object)DBNull.Value);
    command.ExecuteNonQuery();
  }

  public void DeleteCategory(int id)
  {
    using var connection = Open();
    using var command = connection.CreateCommand();
    command.CommandText = "DELETE FROM Categories WHERE Id = $id";
    command.Parameters.AddWithValue("$id", id);
    command.ExecuteNonQuery();
  }

  public List<Supplier> GetSuppliers()
  {
    using var connection = Open();
    using var command = connection.CreateCommand();
    command.CommandText = "SELECT Id, Name, Phone, Email, Address FROM Suppliers ORDER BY Name";
    return ReadSuppliers(command);
  }

  public void AddSupplier(Supplier supplier)
  {
    using var connection = Open();
    using var command = connection.CreateCommand();
    command.CommandText = """
        INSERT INTO Suppliers (Name, Phone, Email, Address)
        VALUES ($name, $phone, $email, $address);
        SELECT last_insert_rowid();
        """;
    command.Parameters.AddWithValue("$name", supplier.Name);
    command.Parameters.AddWithValue("$phone", supplier.Phone ?? (object)DBNull.Value);
    command.Parameters.AddWithValue("$email", supplier.Email ?? (object)DBNull.Value);
    command.Parameters.AddWithValue("$address", supplier.Address ?? (object)DBNull.Value);
    supplier.Id = Convert.ToInt32(command.ExecuteScalar());
  }

  public void UpdateSupplier(Supplier supplier)
  {
    using var connection = Open();
    using var command = connection.CreateCommand();
    command.CommandText = """
        UPDATE Suppliers SET Name = $name, Phone = $phone, Email = $email, Address = $address
        WHERE Id = $id
        """;
    command.Parameters.AddWithValue("$id", supplier.Id);
    command.Parameters.AddWithValue("$name", supplier.Name);
    command.Parameters.AddWithValue("$phone", supplier.Phone ?? (object)DBNull.Value);
    command.Parameters.AddWithValue("$email", supplier.Email ?? (object)DBNull.Value);
    command.Parameters.AddWithValue("$address", supplier.Address ?? (object)DBNull.Value);
    command.ExecuteNonQuery();
  }

  public void DeleteSupplier(int id)
  {
    using var connection = Open();
    using var command = connection.CreateCommand();
    command.CommandText = "DELETE FROM Suppliers WHERE Id = $id";
    command.Parameters.AddWithValue("$id", id);
    command.ExecuteNonQuery();
  }

  public List<Product> GetProducts(string? search = null, int? categoryId = null)
  {
    using var connection = Open();
    using var command = connection.CreateCommand();
    command.CommandText = """
        SELECT p.Id, p.Name, p.CategoryId, p.SupplierId, p.Barcode, p.Unit,
               p.PurchasePrice, p.SalePrice, p.Quantity, p.MinStockLevel,
               p.ExpiryDate, p.CreatedAt, c.Name, s.Name
        FROM Products p
        JOIN Categories c ON c.Id = p.CategoryId
        LEFT JOIN Suppliers s ON s.Id = p.SupplierId
        WHERE ($search IS NULL OR p.Name LIKE '%' || $search || '%' OR p.Barcode LIKE '%' || $search || '%')
          AND ($categoryId IS NULL OR p.CategoryId = $categoryId)
        ORDER BY p.Name
        """;
    command.Parameters.AddWithValue("$search", string.IsNullOrWhiteSpace(search) ? DBNull.Value : search.Trim());
    command.Parameters.AddWithValue("$categoryId", categoryId.HasValue ? categoryId.Value : DBNull.Value);
    return ReadProducts(command);
  }

  public void AddProduct(Product product)
  {
    using var connection = Open();
    using var command = connection.CreateCommand();
    command.CommandText = """
        INSERT INTO Products (Name, CategoryId, SupplierId, Barcode, Unit, PurchasePrice, SalePrice,
                              Quantity, MinStockLevel, ExpiryDate, CreatedAt)
        VALUES ($name, $categoryId, $supplierId, $barcode, $unit, $purchase, $sale,
                $qty, $min, $expiry, $created);
        SELECT last_insert_rowid();
        """;
    BindProduct(command, product);
    product.Id = Convert.ToInt32(command.ExecuteScalar());
  }

  public void UpdateProduct(Product product)
  {
    using var connection = Open();
    using var command = connection.CreateCommand();
    command.CommandText = """
        UPDATE Products SET Name = $name, CategoryId = $categoryId, SupplierId = $supplierId,
            Barcode = $barcode, Unit = $unit, PurchasePrice = $purchase, SalePrice = $sale,
            Quantity = $qty, MinStockLevel = $min, ExpiryDate = $expiry
        WHERE Id = $id
        """;
    command.Parameters.AddWithValue("$id", product.Id);
    BindProduct(command, product, includeCreatedAt: false);
    command.ExecuteNonQuery();
  }

  public void DeleteProduct(int id)
  {
    using var connection = Open();
    using var transaction = connection.BeginTransaction();
    try
    {
      using (var deleteMovements = connection.CreateCommand())
      {
        deleteMovements.Transaction = transaction;
        deleteMovements.CommandText = "DELETE FROM StockMovements WHERE ProductId = $id";
        deleteMovements.Parameters.AddWithValue("$id", id);
        deleteMovements.ExecuteNonQuery();
      }

      using (var deleteProduct = connection.CreateCommand())
      {
        deleteProduct.Transaction = transaction;
        deleteProduct.CommandText = "DELETE FROM Products WHERE Id = $id";
        deleteProduct.Parameters.AddWithValue("$id", id);
        deleteProduct.ExecuteNonQuery();
      }

      transaction.Commit();
    }
    catch
    {
      transaction.Rollback();
      throw;
    }
  }

  public List<StockMovement> GetStockMovements(int? productId = null)
  {
    using var connection = Open();
    using var command = connection.CreateCommand();
    command.CommandText = """
        SELECT sm.Id, sm.ProductId, sm.MovementType, sm.Quantity, sm.UnitPrice, sm.Note, sm.CreatedAt, p.Name
        FROM StockMovements sm
        JOIN Products p ON p.Id = sm.ProductId
        WHERE ($productId IS NULL OR sm.ProductId = $productId)
        ORDER BY sm.CreatedAt DESC
        """;
    command.Parameters.AddWithValue("$productId", productId.HasValue ? productId.Value : DBNull.Value);
    return ReadMovements(command);
  }

  public void AddStockMovement(StockMovement movement)
  {
    using var connection = Open();
    using var transaction = connection.BeginTransaction();
    try
    {
      using (var insert = connection.CreateCommand())
      {
        insert.Transaction = transaction;
        insert.CommandText = """
            INSERT INTO StockMovements (ProductId, MovementType, Quantity, UnitPrice, Note, CreatedAt)
            VALUES ($productId, $type, $qty, $price, $note, $created);
            SELECT last_insert_rowid();
            """;
        insert.Parameters.AddWithValue("$productId", movement.ProductId);
        insert.Parameters.AddWithValue("$type", movement.MovementType.ToString());
        insert.Parameters.AddWithValue("$qty", movement.Quantity);
        insert.Parameters.AddWithValue("$price", movement.UnitPrice);
        insert.Parameters.AddWithValue("$note", movement.Note ?? (object)DBNull.Value);
        insert.Parameters.AddWithValue("$created", movement.CreatedAt.ToString("o", CultureInfo.InvariantCulture));
        movement.Id = Convert.ToInt32(insert.ExecuteScalar());
      }

      var delta = movement.MovementType == MovementType.Incoming ? movement.Quantity : -movement.Quantity;
      using (var update = connection.CreateCommand())
      {
        update.Transaction = transaction;
        update.CommandText = "UPDATE Products SET Quantity = Quantity + $delta WHERE Id = $id";
        update.Parameters.AddWithValue("$delta", delta);
        update.Parameters.AddWithValue("$id", movement.ProductId);
        update.ExecuteNonQuery();
      }

      transaction.Commit();
    }
    catch
    {
      transaction.Rollback();
      throw;
    }
  }

  public DashboardStats GetDashboardStats()
  {
    using var connection = Open();
    using var command = connection.CreateCommand();
    command.CommandText = """
        SELECT
            (SELECT COUNT(*) FROM Products),
            (SELECT COUNT(*) FROM Categories),
            (SELECT COUNT(*) FROM Suppliers),
            (SELECT COALESCE(SUM(Quantity * PurchasePrice), 0) FROM Products),
            (SELECT COUNT(*) FROM Products WHERE Quantity <= MinStockLevel),
            (SELECT COUNT(*) FROM Products WHERE ExpiryDate IS NOT NULL AND ExpiryDate <= date('now', '+7 days') AND ExpiryDate >= date('now')),
            (SELECT COUNT(*) FROM Products WHERE ExpiryDate IS NOT NULL AND ExpiryDate < date('now'))
        """;
    using var reader = command.ExecuteReader();
    reader.Read();
    return new DashboardStats
    {
      TotalProducts = reader.GetInt32(0),
      TotalCategories = reader.GetInt32(1),
      TotalSuppliers = reader.GetInt32(2),
      TotalStockValue = reader.GetDecimal(3),
      LowStockCount = reader.GetInt32(4),
      ExpiringSoonCount = reader.GetInt32(5),
      ExpiredCount = reader.GetInt32(6)
    };
  }

  public List<Product> GetLowStockProducts() => GetProducts().Where(p => p.IsLowStock).ToList();

  public List<Product> GetExpiringProducts() =>
      GetProducts().Where(p => p.IsExpiringSoon || p.IsExpired).OrderBy(p => p.ExpiryDate).ToList();

  private static SqliteConnection Open()
  {
    var connection = new SqliteConnection(DatabaseInitializer.ConnectionString);
    connection.Open();
    return connection;
  }

  private static void BindProduct(SqliteCommand command, Product product, bool includeCreatedAt = true)
  {
    if (!includeCreatedAt)
    {
      command.Parameters.AddWithValue("$name", product.Name);
      command.Parameters.AddWithValue("$categoryId", product.CategoryId);
      command.Parameters.AddWithValue("$supplierId", product.SupplierId.HasValue ? product.SupplierId.Value : DBNull.Value);
      command.Parameters.AddWithValue("$barcode", product.Barcode ?? (object)DBNull.Value);
      command.Parameters.AddWithValue("$unit", product.Unit);
      command.Parameters.AddWithValue("$purchase", product.PurchasePrice);
      command.Parameters.AddWithValue("$sale", product.SalePrice);
      command.Parameters.AddWithValue("$qty", product.Quantity);
      command.Parameters.AddWithValue("$min", product.MinStockLevel);
      command.Parameters.AddWithValue("$expiry", FormatDate(product.ExpiryDate) ?? (object)DBNull.Value);
      return;
    }

    product.CreatedAt = product.CreatedAt == default ? DateTime.Now : product.CreatedAt;
    command.Parameters.AddWithValue("$name", product.Name);
    command.Parameters.AddWithValue("$categoryId", product.CategoryId);
    command.Parameters.AddWithValue("$supplierId", product.SupplierId.HasValue ? product.SupplierId.Value : DBNull.Value);
    command.Parameters.AddWithValue("$barcode", product.Barcode ?? (object)DBNull.Value);
    command.Parameters.AddWithValue("$unit", product.Unit);
    command.Parameters.AddWithValue("$purchase", product.PurchasePrice);
    command.Parameters.AddWithValue("$sale", product.SalePrice);
    command.Parameters.AddWithValue("$qty", product.Quantity);
    command.Parameters.AddWithValue("$min", product.MinStockLevel);
    command.Parameters.AddWithValue("$expiry", FormatDate(product.ExpiryDate) ?? (object)DBNull.Value);
    command.Parameters.AddWithValue("$created", product.CreatedAt.ToString("o", CultureInfo.InvariantCulture));
  }

  private static List<Category> ReadCategories(SqliteCommand command)
  {
    var list = new List<Category>();
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
      list.Add(new Category
      {
        Id = reader.GetInt32(0),
        Name = reader.GetString(1),
        Description = reader.IsDBNull(2) ? null : reader.GetString(2)
      });
    }
    return list;
  }

  private static List<Supplier> ReadSuppliers(SqliteCommand command)
  {
    var list = new List<Supplier>();
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
      list.Add(new Supplier
      {
        Id = reader.GetInt32(0),
        Name = reader.GetString(1),
        Phone = reader.IsDBNull(2) ? null : reader.GetString(2),
        Email = reader.IsDBNull(3) ? null : reader.GetString(3),
        Address = reader.IsDBNull(4) ? null : reader.GetString(4)
      });
    }
    return list;
  }

  private static List<Product> ReadProducts(SqliteCommand command)
  {
    var list = new List<Product>();
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
      list.Add(new Product
      {
        Id = reader.GetInt32(0),
        Name = reader.GetString(1),
        CategoryId = reader.GetInt32(2),
        SupplierId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
        Barcode = reader.IsDBNull(4) ? null : reader.GetString(4),
        Unit = reader.GetString(5),
        PurchasePrice = reader.GetDecimal(6),
        SalePrice = reader.GetDecimal(7),
        Quantity = reader.GetDecimal(8),
        MinStockLevel = reader.GetDecimal(9),
        ExpiryDate = ParseDate(reader.IsDBNull(10) ? null : reader.GetString(10)),
        CreatedAt = ParseDateTime(reader.GetString(11)),
        CategoryName = reader.GetString(12),
        SupplierName = reader.IsDBNull(13) ? null : reader.GetString(13)
      });
    }
    return list;
  }

  private static List<StockMovement> ReadMovements(SqliteCommand command)
  {
    var list = new List<StockMovement>();
    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
      list.Add(new StockMovement
      {
        Id = reader.GetInt32(0),
        ProductId = reader.GetInt32(1),
        MovementType = Enum.Parse<MovementType>(reader.GetString(2)),
        Quantity = reader.GetDecimal(3),
        UnitPrice = reader.GetDecimal(4),
        Note = reader.IsDBNull(5) ? null : reader.GetString(5),
        CreatedAt = ParseDateTime(reader.GetString(6)),
        ProductName = reader.GetString(7)
      });
    }
    return list;
  }
}
