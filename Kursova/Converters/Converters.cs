using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Kursova.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      value is true ? Visibility.Visible : Visibility.Collapsed;

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      value is Visibility.Visible;
}

public class NullToVisibilityConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      value == null ? Visibility.Collapsed : Visibility.Visible;

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}

public class DecimalToCurrencyConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      value is decimal d ? $"{d:N2} ₴" : string.Empty;

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}

public class DateToStringConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
      value is DateTime dt ? dt.ToString("dd.MM.yyyy") : "—";

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();
}

public class ViewModelTemplateSelector : DataTemplateSelector
{
  public DataTemplate? DashboardTemplate { get; set; }
  public DataTemplate? ProductsTemplate { get; set; }
  public DataTemplate? CategoriesTemplate { get; set; }
  public DataTemplate? SuppliersTemplate { get; set; }
  public DataTemplate? MovementsTemplate { get; set; }

  public override DataTemplate? SelectTemplate(object item, DependencyObject container) => item switch
  {
    ViewModels.DashboardViewModel => DashboardTemplate,
    ViewModels.ProductsViewModel => ProductsTemplate,
    ViewModels.CategoriesViewModel => CategoriesTemplate,
    ViewModels.SuppliersViewModel => SuppliersTemplate,
    ViewModels.StockMovementsViewModel => MovementsTemplate,
    _ => base.SelectTemplate(item, container)
  };
}
