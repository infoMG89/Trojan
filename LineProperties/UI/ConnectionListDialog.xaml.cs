using System.Collections.Generic;
using System.Windows;
using LineProperties.Services;

namespace LineProperties.UI;

public partial class ConnectionListDialog : Window
{
    public ConnectionListDialog(IReadOnlyList<ConnectionListDisplayItem> items)
    {
        InitializeComponent();
        DgConnections.ItemsSource = items;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
