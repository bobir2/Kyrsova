using System.Globalization;
using System.Windows;
using Kursova.Data;
using Kursova.Models;

namespace Kursova.Views;

public partial class StockMovementWindow : Window
{
  private readonly WarehouseRepository _repository;
  private readonly MovementType _type;

  public StockMovementWindow(WarehouseRepository repository, MovementType type)
  {
    InitializeComponent();
    _repository = repository;
    _type = type;

    Title = type == MovementType.Incoming ? "Надходження товару" : "Відвантаження товару";
    TypeLabel.Text = type == MovementType.Incoming ? "📥 Надходження на склад" : "📤 Відвантаження зі складу";

    ProductBox.ItemsSource = _repository.GetProducts();
    if (ProductBox.Items.Count > 0)
      ProductBox.SelectedIndex = 0;
  }

  private void Save_Click(object sender, RoutedEventArgs e)
  {
    if (ProductBox.SelectedItem is not Product product)
    {
      MessageBox.Show("Оберіть продукт.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
      return;
    }

    if (!decimal.TryParse(QuantityBox.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var qty) || qty <= 0)
    {
      MessageBox.Show("Введіть коректну кількість.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
      return;
    }

    if (_type == MovementType.Outgoing && qty > product.Quantity)
    {
      MessageBox.Show($"Недостатньо товару на складі. Доступно: {product.Quantity} {product.Unit}",
          "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
      return;
    }

    if (!decimal.TryParse(PriceBox.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
      price = product.PurchasePrice;

    var movement = new StockMovement
    {
      ProductId = product.Id,
      MovementType = _type,
      Quantity = qty,
      UnitPrice = price,
      Note = string.IsNullOrWhiteSpace(NoteBox.Text) ? null : NoteBox.Text.Trim(),
      CreatedAt = DateTime.Now
    };

    try
    {
      _repository.AddStockMovement(movement);
      DialogResult = true;
    }
    catch (Exception ex)
    {
      MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }

  private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
