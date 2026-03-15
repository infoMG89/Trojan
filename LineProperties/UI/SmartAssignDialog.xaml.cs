using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LineProperties.Data;
using LineProperties.Services;

namespace LineProperties.UI;

public partial class SmartAssignDialog : Window
{
    public string MemberType { get; private set; } = MoravioSmartXDataService.MemberTypeJoist;
    public string Designation { get; private set; } = "";
    public string Mark { get; private set; } = "";

    private readonly bool _isPolyline;

    public SmartAssignDialog(bool isPolyline)
    {
        _isPolyline = isPolyline;
        InitializeComponent();
        Loaded += (_, _) => {
            if (Helpers.LogoHelper.GetTrojanLogoSource() is { } t) LogoImage.Source = t;
            if (Helpers.LogoHelper.GetHmrLogoSource() is { } h) LogoImage2.Source = h;
        };

        if (isPolyline)
        {
            CmbType.ItemsSource = new[] { "Plech (Plate)" };
            CmbType.SelectedIndex = 0;
            CmbDesignation.ItemsSource = PlateLibrary.GetPlateTypes().ToList();
            CmbDesignation.SelectedItem = PlateLibrary.DefaultPlateType;
        }
        else
        {
            CmbType.ItemsSource = new[] { "Joist (SJI)", "Nosník (AISC W)", "Nosník (AISC C)", "Deck" };
            CmbType.SelectedIndex = 0;
            LoadDesignationsForType(0);
        }
    }

    private void CmbType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_isPolyline) return;
        if (CmbType.SelectedIndex >= 0)
            LoadDesignationsForType(CmbType.SelectedIndex);
    }

    private void LoadDesignationsForType(int index)
    {
        var designations = index switch
        {
            0 => SjiJoistLibrary.GetDesignations().ToList(),
            1 => AiscShapeLibrary.GetDesignations("W").ToList(),
            2 => AiscShapeLibrary.GetDesignations("C").ToList(),
            3 => DeckLibrary.GetDeckTypes().ToList(),
            _ => new List<string>()
        };
        CmbDesignation.ItemsSource = designations;
        CmbDesignation.SelectedIndex = designations.Count > 0 ? 0 : -1;
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
        MemberType = _isPolyline ? MoravioSmartXDataService.MemberTypePlate
            : CmbType.SelectedIndex switch
            {
                0 => MoravioSmartXDataService.MemberTypeJoist,
                1 => MoravioSmartXDataService.MemberTypeBeam,
                2 => MoravioSmartXDataService.MemberTypeBeam,
                3 => MoravioSmartXDataService.MemberTypeDeck,
                _ => MoravioSmartXDataService.MemberTypeJoist
            };

        Designation = CmbDesignation.SelectedItem as string ?? CmbDesignation.Text?.Trim() ?? "";
        Mark = TxtMark.Text?.Trim() ?? "";
        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
