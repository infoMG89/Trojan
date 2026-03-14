using System.Collections.Generic;
using System.Windows;
using LineProperties.Services;

namespace LineProperties.UI;

public partial class ElementListDialog : Window
{
    public ElementListDialog(IReadOnlyList<SmartElement> elements)
    {
        InitializeComponent();
        DgElements.ItemsSource = elements;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
