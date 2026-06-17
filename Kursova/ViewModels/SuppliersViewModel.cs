using System.Collections.ObjectModel;
using System.Windows;
using Kursova.Data;
using Kursova.Models;
using Kursova.Views;

namespace Kursova.ViewModels;

public class SuppliersViewModel : ViewModelBase
{
  private readonly WarehouseRepository _repository;
  private Supplier? _selectedSupplier;

  public SuppliersViewModel(WarehouseRepository repository)
  {
    _repository = repository;
    Suppliers = new ObservableCollection<Supplier>();

    AddCommand = new RelayCommand(AddSupplier);
    EditCommand = new RelayCommand(EditSupplier, () => SelectedSupplier != null);
    DeleteCommand = new RelayCommand(DeleteSupplier, () => SelectedSupplier != null);
    RefreshCommand = new RelayCommand(Load);

    Load();
  }

  public ObservableCollection<Supplier> Suppliers { get; }

  public Supplier? SelectedSupplier
  {
    get => _selectedSupplier;
    set => SetProperty(ref _selectedSupplier, value);
  }

  public RelayCommand AddCommand { get; }
  public RelayCommand EditCommand { get; }
  public RelayCommand DeleteCommand { get; }
  public RelayCommand RefreshCommand { get; }

  private void Load()
  {
    Suppliers.Clear();
    foreach (var supplier in _repository.GetSuppliers())
      Suppliers.Add(supplier);
  }

  private void AddSupplier()
  {
    var dialog = new SupplierEditWindow();
    if (dialog.ShowDialog() == true && dialog.Supplier != null)
    {
      _repository.AddSupplier(dialog.Supplier);
      Load();
    }
  }

  private void EditSupplier()
  {
    if (SelectedSupplier == null) return;
    var dialog = new SupplierEditWindow(SelectedSupplier);
    if (dialog.ShowDialog() == true && dialog.Supplier != null)
    {
      _repository.UpdateSupplier(dialog.Supplier);
      Load();
    }
  }

  private void DeleteSupplier()
  {
    if (SelectedSupplier == null) return;
    var result = MessageBox.Show(
        $"Видалити постачальника «{SelectedSupplier.Name}»?",
        "Підтвердження",
        MessageBoxButton.YesNo,
        MessageBoxImage.Question);
    if (result != MessageBoxResult.Yes) return;

    try
    {
      _repository.DeleteSupplier(SelectedSupplier.Id);
      Load();
    }
    catch (Exception ex)
    {
      MessageBox.Show($"Помилка видалення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
    }
  }
}
