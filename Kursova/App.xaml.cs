using System.Windows;
using Kursova.Data;

namespace Kursova;

public partial class App : Application
{
  protected override void OnStartup(StartupEventArgs e)
  {
    base.OnStartup(e);
    DatabaseInitializer.Initialize();
  }
}
