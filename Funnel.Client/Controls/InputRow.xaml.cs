using System.Windows;
using System.Windows.Controls;

namespace Funnel.Client.Controls;

public partial class InputRow : UserControl
{
    public required string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public required string TextContent
    {
        get => (string)GetValue(TextContentProperty);
        set => SetValue(TextContentProperty, value);
    }

    public InputRow()
    {
        InitializeComponent();
        // SetValue(LabelProperty, "Label");
        RootUiElement.DataContext = this;
    }

    public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(
        nameof(LabelText),
        typeof(string),
        typeof(InputRow),
        new FrameworkPropertyMetadata("Label"));

    public static readonly DependencyProperty TextContentProperty = DependencyProperty.Register(
        nameof(TextContent),
        typeof(string),
        typeof(InputRow),
        new FrameworkPropertyMetadata("TextContent"));
}