using System.Collections.ObjectModel;
using System.Windows;
using Kursova.Data;
using Kursova.Models;
using Kursova.Views;

namespace Kursova.ViewModels;

public class CategoriesViewModel : ViewModelBase
{
  private readonly WarehouseRepository _repository;
  private Category? _selectedCategory;

  public CategoriesViewModel(WarehouseRepository repository)
  {
    _repository = repository;
    Categories = new ObservableCollection<Category>();

    AddCommand = new RelayCommand(AddCategory);
    EditCommand = new RelayCommand(EditCategory, () => SelectedCategory != null);
    DeleteCommand = new RelayCommand(DeleteCategory, () => SelectedCategory != null);
    RefreshCommand = new RelayCommand(Load);

    Load();
  }

  public ObservableCollection<Category> Categories { get; }

  public Category? SelectedCategory
  {
    get => _selectedCategory;
    set => SetProperty(ref _selectedCategory, value);
  }

  public RelayCommand AddCommand { get; }
  public RelayCommand EditCommand { get; }
  public RelayCommand DeleteCommand { get; }
  public RelayCommand RefreshCommand { get; }

  private void Load()
  {
    Categories.Clear();
    foreach (var category in _repository.GetCategories())
      Categories.Add(category);
  }

  private void AddCategory()
  {
    var dialog = new CategoryEditWindow();
    if (dialog.ShowDialog() == true && dialog.Category != null)
    {
      _repository.AddCategory(dialog.Category);
      Load();
    }
  }

  private void EditCategory()
  {
    if (SelectedCategory == null) return;
    var dialog = new CategoryEditWindow(SelectedCategory);
    if (dialog.ShowDialog() == true && dialog.Category != null)
    {
      _repository.UpdateCategory(dialog.Category);
      Load();
    }
  }

  private void DeleteCategory()
  {
    if (SelectedCategory == null) return;
    var result = MessageBox.Show(
        $"Видалити категорію «{SelectedCategory.Name}»?",
        "Підтвердження",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question);
    if (result != MessageBoxResult.Yes) return;

    try
    {
      _repository.DeleteCategory(SelectedCategory.Id);
      Load();
    }
    catch (Exception ex)
    {
      MessageBox.Show($"Помилка видалення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }
}
