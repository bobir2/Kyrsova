using System.Windows;
using Kursova.Models;

namespace Kursova.Views;

public partial class CategoryEditWindow : Window
{
  public Category? Category { get; private set; }

  public CategoryEditWindow(Category? existing = null)
  {
    InitializeComponent();
    Title = existing == null ? "Нова категорія" : "Редагування категорії";

    if (existing != null)
    {
      NameBox.Text = existing.Name;
      DescriptionBox.Text = existing.Description ?? string.Empty;
      Category = new Category { Id = existing.Id };
    }
  }

  private void Save_Click(object sender, RoutedEventArgs e)
  {
    if (string.IsNullOrWhiteSpace(NameBox.Text))
    {
      MessageBox.Show("Введіть назву категорії.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
      return;
    }

    Category ??= new Category();
    Category.Name = NameBox.Text.Trim();
    Category.Description = string.IsNullOrWhiteSpace(DescriptionBox.Text) ? null : DescriptionBox.Text.Trim();
    DialogResult = true;
  }

  private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
