using System.Globalization;
using System.Windows;
using Kursova.Data;
using Kursova.Models;

namespace Kursova.Views;

public partial class ProductEditWindow : Window
{
  private readonly WarehouseRepository _repository;
  private readonly Product? _existing;

  public ProductEditWindow(WarehouseRepository repository, Product? existing = null)
  {
    InitializeComponent();
    _repository = repository;
    _existing = existing;

    Title = existing == null ? "Новий продукт" : "Редагування продукту";

    var categories = _repository.GetCategories();
    CategoryBox.ItemsSource = categories;

    var suppliers = _repository.GetSuppliers();
    SupplierBox.ItemsSource = suppliers;

    UnitBox.ItemsSource = new[] { "шт", "кг", "л", "уп" };
    UnitBox.Text = "шт";

    if (existing != null)
    {
      NameBox.Text = existing.Name;
      CategoryBox.SelectedItem = categories.FirstOrDefault(c => c.Id == existing.CategoryId);
      SupplierBox.SelectedItem = suppliers.FirstOrDefault(s => s.Id == existing.SupplierId);
      BarcodeBox.Text = existing.Barcode ?? string.Empty;
      UnitBox.Text = existing.Unit;
      PurchaseBox.Text = existing.PurchasePrice.ToString(CultureInfo.InvariantCulture);
      SaleBox.Text = existing.SalePrice.ToString(CultureInfo.InvariantCulture);
      QuantityBox.Text = existing.Quantity.ToString(CultureInfo.InvariantCulture);
      MinStockBox.Text = existing.MinStockLevel.ToString(CultureInfo.InvariantCulture);
      ExpiryPicker.SelectedDate = existing.ExpiryDate;
    }
    else if (categories.Count > 0)
    {
      CategoryBox.SelectedIndex = 0;
    }
  }

  private void Save_Click(object sender, RoutedEventArgs e)
  {
    if (string.IsNullOrWhiteSpace(NameBox.Text))
    {
      MessageBox.Show("Введіть назву продукту.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
      return;
    }

    if (CategoryBox.SelectedItem is not Category category)
    {
      MessageBox.Show("Оберіть категорію.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
      return;
    }

    if (!decimal.TryParse(PurchaseBox.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var purchase))
      purchase = 0;
    if (!decimal.TryParse(SaleBox.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var sale))
      sale = 0;
    if (!decimal.TryParse(QuantityBox.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var qty))
      qty = 0;
    if (!decimal.TryParse(MinStockBox.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var minStock))
      minStock = 0;

    var product = new Product
    {
      Id = _existing?.Id ?? 0,
      Name = NameBox.Text.Trim(),
      CategoryId = category.Id,
      SupplierId = (SupplierBox.SelectedItem as Supplier)?.Id,
      Barcode = string.IsNullOrWhiteSpace(BarcodeBox.Text) ? null : BarcodeBox.Text.Trim(),
      Unit = string.IsNullOrWhiteSpace(UnitBox.Text) ? "шт" : UnitBox.Text.Trim(),
      PurchasePrice = purchase,
      SalePrice = sale,
      Quantity = qty,
      MinStockLevel = minStock,
      ExpiryDate = ExpiryPicker.SelectedDate,
      CreatedAt = _existing?.CreatedAt ?? DateTime.Now
    };

    try
    {
      if (_existing == null)
        _repository.AddProduct(product);
      else
        _repository.UpdateProduct(product);

      DialogResult = true;
    }
    catch (Exception ex)
    {
      MessageBox.Show($"Помилка збереження: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }

  private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
