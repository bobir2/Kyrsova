using System.Collections.ObjectModel;
using System.Windows;
using Kursova.Data;
using Kursova.Models;
using Kursova.Views;

namespace Kursova.ViewModels;

public class ProductsViewModel : ViewModelBase
{
  private readonly WarehouseRepository _repository;
  private string _searchText = string.Empty;
  private Category? _selectedCategoryFilter;
  private Product? _selectedProduct;

  public ProductsViewModel(WarehouseRepository repository)
  {
    _repository = repository;
    Categories = new ObservableCollection<Category>();
    Products = new ObservableCollection<Product>();
    LoadCategories();

    AddCommand = new RelayCommand(AddProduct);
    EditCommand = new RelayCommand(EditProduct, () => SelectedProduct != null);
    DeleteCommand = new RelayCommand(DeleteProduct, () => SelectedProduct != null);
    RefreshCommand = new RelayCommand(Load);
    SearchCommand = new RelayCommand(Load);

    Load();
  }

  public ObservableCollection<Product> Products { get; }
  public ObservableCollection<Category> Categories { get; }

  public string SearchText
  {
    get => _searchText;
    set
    {
      if (SetProperty(ref _searchText, value))
        Load();
    }
  }

  public Category? SelectedCategoryFilter
  {
    get => _selectedCategoryFilter;
    set
    {
      if (SetProperty(ref _selectedCategoryFilter, value))
        Load();
    }
  }

  public Product? SelectedProduct
  {
    get => _selectedProduct;
    set => SetProperty(ref _selectedProduct, value);
  }

  public RelayCommand AddCommand { get; }
  public RelayCommand EditCommand { get; }
  public RelayCommand DeleteCommand { get; }
  public RelayCommand RefreshCommand { get; }
  public RelayCommand SearchCommand { get; }

  private void LoadCategories()
  {
    Categories.Clear();
    Categories.Add(new Category { Id = 0, Name = "— Всі категорії —" });
    foreach (var category in _repository.GetCategories())
      Categories.Add(category);
    SelectedCategoryFilter = Categories[0];
  }

  private void Load()
  {
    Products.Clear();
    int? categoryId = SelectedCategoryFilter is { Id: > 0 } c ? c.Id : null;
    foreach (var product in _repository.GetProducts(
        string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
        categoryId))
    {
      Products.Add(product);
    }
  }

  private void AddProduct()
  {
    var dialog = new ProductEditWindow(_repository);
    if (dialog.ShowDialog() == true)
      Load();
  }

  private void EditProduct()
  {
    if (SelectedProduct == null) return;
    var dialog = new ProductEditWindow(_repository, SelectedProduct);
    if (dialog.ShowDialog() == true)
      Load();
  }

  private void DeleteProduct()
  {
    if (SelectedProduct == null) return;
    var result = MessageBox.Show(
        $"Видалити продукт «{SelectedProduct.Name}»?",
        "Підтвердження",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question);
    if (result != MessageBoxResult.Yes) return;

    try
    {
      _repository.DeleteProduct(SelectedProduct.Id);
      Load();
    }
    catch (Exception ex)
    {
      MessageBox.Show($"Помилка видалення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }
}
