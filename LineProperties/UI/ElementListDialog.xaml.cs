using System.Collections.Generic;
using System.Windows;
using LineProperties.Services;

namespace LineProperties.UI;

public partial class ElementListDialog : Window
{
    public ElementListDialog(IReadOnlyList<SmartElement> elements)
    {
        InitializeComponent();
        Loaded += (_, _) => {
            if (Helpers.LogoHelper.GetTrojanLogoSource() is { } t) LogoImage.Source = t;
            if (Helpers.LogoHelper.GetHmrLogoSource() is { } h) LogoImage2.Source = h;
        };
        DgElements.ItemsSource = elements;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
