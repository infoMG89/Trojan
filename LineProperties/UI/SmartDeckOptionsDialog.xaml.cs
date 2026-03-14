using System.Windows;
using LineProperties.Data;

namespace LineProperties.UI;

public partial class SmartDeckOptionsDialog : Window
{
    public string SelectedPlateType { get; private set; } = PlateLibrary.DefaultPlateType;
    public string Mark { get; private set; } = "P1";

    public SmartDeckOptionsDialog()
    {
        InitializeComponent();
        CmbPlateType.ItemsSource = PlateLibrary.GetPlateTypes();
        CmbPlateType.SelectedItem = PlateLibrary.DefaultPlateType;
        TxtMark.Text = "P1";
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
        if (CmbPlateType.SelectedItem is string pt)
            SelectedPlateType = pt;
        else if (!string.IsNullOrWhiteSpace(CmbPlateType.Text))
            SelectedPlateType = CmbPlateType.Text.Trim();
        Mark = TxtMark.Text?.Trim() ?? "P1";
        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
