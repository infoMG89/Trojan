using System.Windows;
using LineProperties.Data;

namespace LineProperties.UI;

public partial class SmartDeckOptionsDialog : Window
{
    public string SelectedDeckType { get; private set; } = DeckLibrary.DefaultDeckType;

    public SmartDeckOptionsDialog()
    {
        InitializeComponent();
        CmbDeckType.ItemsSource = DeckLibrary.GetDeckTypes();
        CmbDeckType.SelectedItem = DeckLibrary.DefaultDeckType;
        CmbGauge.ItemsSource = new[] { 18, 20, 22 };
        CmbGauge.SelectedItem = 18;
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
        if (CmbDeckType.SelectedItem is string dt)
            SelectedDeckType = dt;
        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
