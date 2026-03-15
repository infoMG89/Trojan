using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LineProperties.Helpers;

namespace LineProperties.UI;

public partial class ConnectionCodeDialog : Window
{
    public record CodeItem(string Code, string? Description)
    {
        public string Display => string.IsNullOrEmpty(Description) ? Code : $"{Code} – {Description}";
    }

    public string? SelectedCode { get; private set; }

    public ConnectionCodeDialog(string connectionId, IReadOnlyList<CodeItem> codes)
    {
        InitializeComponent();
        Loaded += (_, _) => {
            if (LogoHelper.GetTrojanLogoSource() is { } t) LogoImage.Source = t;
            if (LogoHelper.GetHmrLogoSource() is { } h) LogoImage2.Source = h;
        };
        IdText.Text = connectionId;
        CodeCombo.ItemsSource = codes;
        CodeCombo.SelectedIndex = 0;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        SelectedCode = (CodeCombo.SelectedItem as CodeItem)?.Code;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
