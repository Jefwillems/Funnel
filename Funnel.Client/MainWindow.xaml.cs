using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Funnel.Client;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public required string ServerUrl
    {
        get => (string)GetValue(ServerUrlProperty);
        set => SetValue(ServerUrlProperty, value);
    }

    public required string LocalUrl
    {
        get => (string)GetValue(LocalUrlProperty);
        set => SetValue(LocalUrlProperty, value);
    }

    public MainWindow()
    {
        InitializeComponent();
        RootUiElement.DataContext = this;
    }

    public static readonly DependencyProperty ServerUrlProperty = DependencyProperty.Register(
        nameof(ServerUrl),
        typeof(string),
        typeof(MainWindow),
        new FrameworkPropertyMetadata("TextContent"));

    public static readonly DependencyProperty LocalUrlProperty = DependencyProperty.Register(
        nameof(LocalUrl),
        typeof(string),
        typeof(MainWindow),
        new FrameworkPropertyMetadata("TextContent"));
}