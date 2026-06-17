using System.Windows;
using Kursova.ViewModels;

namespace Kursova;

public partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();
    DataContext = new MainViewModel();
  }
}
