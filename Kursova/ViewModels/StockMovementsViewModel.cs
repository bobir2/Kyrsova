using System.Collections.ObjectModel;
using Kursova.Data;
using Kursova.Models;
using Kursova.Views;

namespace Kursova.ViewModels;

public class StockMovementsViewModel : ViewModelBase
{
  private readonly WarehouseRepository _repository;
  private StockMovement? _selectedMovement;

  public StockMovementsViewModel(WarehouseRepository repository)
  {
    _repository = repository;
    Movements = new ObservableCollection<StockMovement>();

    AddIncomingCommand = new RelayCommand(() => AddMovement(MovementType.Incoming));
    AddOutgoingCommand = new RelayCommand(() => AddMovement(MovementType.Outgoing));
    RefreshCommand = new RelayCommand(Load);

    Load();
  }

  public ObservableCollection<StockMovement> Movements { get; }

  public StockMovement? SelectedMovement
  {
    get => _selectedMovement;
    set => SetProperty(ref _selectedMovement, value);
  }

  public RelayCommand AddIncomingCommand { get; }
  public RelayCommand AddOutgoingCommand { get; }
  public RelayCommand RefreshCommand { get; }

  private void Load()
  {
    Movements.Clear();
    foreach (var movement in _repository.GetStockMovements())
      Movements.Add(movement);
  }

  private void AddMovement(MovementType type)
  {
    var dialog = new StockMovementWindow(_repository, type);
    if (dialog.ShowDialog() == true)
      Load();
  }
}
