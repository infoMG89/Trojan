using System.Collections.Generic;
using System.Windows;
using LineProperties.Services;

namespace LineProperties.UI;

public partial class ConnectionListDialog : Window
{
    public ConnectionListDialog(IReadOnlyList<ConnectionListDisplayItem> items)
    {
        InitializeComponent();
        Loaded += (_, _) => {
            if (Helpers.LogoHelper.GetTrojanLogoSource() is { } t) LogoImage.Source = t;
            if (Helpers.LogoHelper.GetHmrLogoSource() is { } h) LogoImage2.Source = h;
        };
        DgConnections.ItemsSource = items;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
