using System.Collections.ObjectModel;
using Kursova.Data;
using Kursova.Models;

namespace Kursova.ViewModels;

public class DashboardViewModel : ViewModelBase
{
  private readonly WarehouseRepository _repository;

  public DashboardViewModel(WarehouseRepository repository)
  {
    _repository = repository;
    RefreshCommand = new RelayCommand(Load);
    Load();
  }

  public DashboardStats Stats { get; private set; } = new();
  public ObservableCollection<Product> LowStockProducts { get; } = new();
  public ObservableCollection<Product> ExpiringProducts { get; } = new();
  public RelayCommand RefreshCommand { get; }

  private void Load()
  {
    Stats = _repository.GetDashboardStats();
    OnPropertyChanged(nameof(Stats));

    LowStockProducts.Clear();
    foreach (var product in _repository.GetLowStockProducts())
      LowStockProducts.Add(product);

    ExpiringProducts.Clear();
    foreach (var product in _repository.GetExpiringProducts())
      ExpiringProducts.Add(product);
  }
}
