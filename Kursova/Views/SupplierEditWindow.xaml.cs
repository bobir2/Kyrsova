using System.Windows;
using Kursova.Models;

namespace Kursova.Views;

public partial class SupplierEditWindow : Window
{
  public Supplier? Supplier { get; private set; }

  public SupplierEditWindow(Supplier? existing = null)
  {
    InitializeComponent();
    Title = existing == null ? "Новий постачальник" : "Редагування постачальника";

    if (existing != null)
    {
      NameBox.Text = existing.Name;
      PhoneBox.Text = existing.Phone ?? string.Empty;
      EmailBox.Text = existing.Email ?? string.Empty;
      AddressBox.Text = existing.Address ?? string.Empty;
      Supplier = new Supplier { Id = existing.Id };
    }
  }

  private void Save_Click(object sender, RoutedEventArgs e)
  {
    if (string.IsNullOrWhiteSpace(NameBox.Text))
    {
      MessageBox.Show("Введіть назву постачальника.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
      return;
    }

    Supplier ??= new Supplier();
    Supplier.Name = NameBox.Text.Trim();
    Supplier.Phone = string.IsNullOrWhiteSpace(PhoneBox.Text) ? null : PhoneBox.Text.Trim();
    Supplier.Email = string.IsNullOrWhiteSpace(EmailBox.Text) ? null : EmailBox.Text.Trim();
    Supplier.Address = string.IsNullOrWhiteSpace(AddressBox.Text) ? null : AddressBox.Text.Trim();
    DialogResult = true;
  }

  private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
