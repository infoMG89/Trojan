using System.Windows;
using System.Windows.Controls;
using LineProperties.Data;

namespace LineProperties.UI;

public partial class AiscShapeDialog : Window
{
    public string SelectedDesignation { get; private set; } = AiscShapeLibrary.DefaultWShape;
    public string Mark { get; private set; } = "B1";

    public AiscShapeDialog()
    {
        InitializeComponent();
        CmbShapeType.ItemsSource = AiscShapeLibrary.GetShapeTypes();
        CmbShapeType.SelectedItem = "W";
        LoadDesignations("W");
        CmbDesignation.SelectedItem = AiscShapeLibrary.DefaultWShape;
        TxtMark.Text = "B1";
    }

    private void CmbShapeType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbShapeType.SelectedItem is string st)
            LoadDesignations(st);
    }

    private void LoadDesignations(string shapeType)
    {
        CmbDesignation.ItemsSource = AiscShapeLibrary.GetDesignations(shapeType);
        CmbDesignation.SelectedItem = AiscShapeLibrary.GetDefaultDesignation(shapeType);
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
        if (CmbDesignation.SelectedItem is string d)
            SelectedDesignation = d;
        else if (!string.IsNullOrWhiteSpace(CmbDesignation.Text))
            SelectedDesignation = CmbDesignation.Text.Trim();
        Mark = TxtMark.Text?.Trim() ?? "B1";
        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
