using Kursova.Data;
using Kursova.Models;

namespace Kursova.ViewModels;

public class MainViewModel : ViewModelBase
{
  private readonly WarehouseRepository _repository = new();
  private ViewModelBase? _currentViewModel;
  private string _title = "Панель керування";

  public MainViewModel()
  {
    NavigateDashboardCommand = new RelayCommand(() => NavigateTo(new DashboardViewModel(_repository)));
    NavigateProductsCommand = new RelayCommand(() => NavigateTo(new ProductsViewModel(_repository)));
    NavigateCategoriesCommand = new RelayCommand(() => NavigateTo(new CategoriesViewModel(_repository)));
    NavigateSuppliersCommand = new RelayCommand(() => NavigateTo(new SuppliersViewModel(_repository)));
    NavigateMovementsCommand = new RelayCommand(() => NavigateTo(new StockMovementsViewModel(_repository)));

    CurrentViewModel = new DashboardViewModel(_repository);
  }

  public ViewModelBase? CurrentViewModel
  {
    get => _currentViewModel;
    set => SetProperty(ref _currentViewModel, value);
  }

  public string Title
  {
    get => _title;
    set => SetProperty(ref _title, value);
  }

  public RelayCommand NavigateDashboardCommand { get; }
  public RelayCommand NavigateProductsCommand { get; }
  public RelayCommand NavigateCategoriesCommand { get; }
  public RelayCommand NavigateSuppliersCommand { get; }
  public RelayCommand NavigateMovementsCommand { get; }

  private void NavigateTo(ViewModelBase viewModel)
  {
    CurrentViewModel = viewModel;
    Title = viewModel switch
    {
      DashboardViewModel => "Панель керування",
      ProductsViewModel => "Продукти",
      CategoriesViewModel => "Категорії",
      SuppliersViewModel => "Постачальники",
      StockMovementsViewModel => "Рух товарів",
      _ => "Склад продуктів"
    };
  }
}
