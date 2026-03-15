using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LineProperties.Data;
using LineProperties.Models;
using LineProperties.Services;

namespace LineProperties.UI;

public partial class EditJoistDialog : Window
{
    public JoistPropertyModel Model { get; } = new();

    /// <summary>SJI = joist, W/C = AISC beam. Used by SmartJoistCommand for layer and MemberType.</summary>
    public string SelectedLibrary { get; private set; } = "SJI";

    private static readonly (string Key, string Label)[] LibraryOptions =
    {
        ("SJI", "SJI Joists"),
        ("W", "AISC W-Shapes"),
        ("C", "AISC C-Channels")
    };

    public EditJoistDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => {
            if (Helpers.LogoHelper.GetTrojanLogoSource() is { } t) LogoImage.Source = t;
            if (Helpers.LogoHelper.GetHmrLogoSource() is { } h) LogoImage2.Source = h;
        };
        DataContext = Model;
        CmbLibrary.ItemsSource = LibraryOptions.Select(x => x.Label).ToList();
        CmbLibrary.SelectedIndex = 0;
        LoadDesignationsForLibrary("SJI");
        CmbDesignation.SelectedItem = Model.Designation;
        UpdateDepthWeightFromDesignation(Model.Designation);
    }

    public EditJoistDialog(JoistPropertyModel model) : this()
    {
        Model.Mark = model.Mark;
        Model.Designation = model.Designation;
        Model.ExtensionLeft = model.ExtensionLeft;
        Model.ExtensionRight = model.ExtensionRight;
        Model.CantileverLeft = model.CantileverLeft;
        Model.CantileverRight = model.CantileverRight;
        Model.ShoeLeft = model.ShoeLeft;
        Model.ShoeRight = model.ShoeRight;
        Model.BridgingLeft = model.BridgingLeft;
        Model.BridgingRight = model.BridgingRight;
        Model.IsTieJoist = model.IsTieJoist;
        Model.ActiveLoads = model.ActiveLoads;
        Model.SequenceZone = model.SequenceZone;
        Model.Depth = model.Depth;
        Model.Weight = model.Weight;
        var aisc = AiscShapeLibrary.GetByDesignation(model.Designation);
        if (aisc != null)
        {
            SelectedLibrary = aisc.ShapeType;
            CmbLibrary.SelectedIndex = Array.FindIndex(LibraryOptions, x => x.Key == SelectedLibrary);
            LoadDesignationsForLibrary(SelectedLibrary);
        }
        SelectDesignationInCombo(model.Designation);
    }

    private void CmbLibrary_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbLibrary.SelectedIndex >= 0 && CmbLibrary.SelectedIndex < LibraryOptions.Length)
        {
            SelectedLibrary = LibraryOptions[CmbLibrary.SelectedIndex].Key;
            LoadDesignationsForLibrary(SelectedLibrary);
        }
    }

    private void LoadDesignationsForLibrary(string libraryKey)
    {
        var designations = libraryKey == "SJI"
            ? SjiJoistLibrary.GetDesignations().ToList()
            : AiscShapeLibrary.GetDesignations(libraryKey).ToList();
        CmbDesignation.ItemsSource = designations;
        CmbDesignation.SelectedItem = libraryKey == "SJI"
            ? SjiJoistLibrary.DefaultDesignation
            : AiscShapeLibrary.GetDefaultDesignation(libraryKey);
        Model.Designation = CmbDesignation.SelectedItem as string ?? "";
    }

    private void SelectDesignationInCombo(string designation)
    {
        if (CmbDesignation.ItemsSource is IEnumerable<string> items)
        {
            var match = items.FirstOrDefault(d =>
                string.Equals(d, designation, StringComparison.OrdinalIgnoreCase));
            CmbDesignation.SelectedItem = match;
        }
        CmbDesignation.Text = designation;
    }

    private void CmbDesignation_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbDesignation.SelectedItem is string s)
            UpdateDepthWeightFromDesignation(s);
    }

    private void UpdateDepthWeightFromDesignation(string designation)
    {
        var sji = SjiJoistLibrary.GetByDesignation(designation);
        if (sji != null)
        {
            Model.Depth = sji.DepthInches;
            Model.Weight = sji.WeightPlf;
            return;
        }
        var aisc = AiscShapeLibrary.GetByDesignation(designation);
        if (aisc != null)
        {
            Model.Depth = aisc.DepthInches;
            Model.Weight = aisc.WeightPlf;
        }
    }

    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
        UpdateDepthWeightFromDesignation(Model.Designation);
        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    /// <summary>Converts model to MoravioSmartData for XData write.</summary>
    public static Services.MoravioSmartData ToMoravioSmartData(JoistPropertyModel m, string memberType, double spanLength)
    {
        return new Services.MoravioSmartData
        {
            MemberType = memberType,
            Mark = m.Mark ?? "",
            Designation = m.Designation ?? "",
            ExtensionLeft = FractionalInchParser.ParseOr(m.ExtensionLeft, 0),
            ExtensionRight = FractionalInchParser.ParseOr(m.ExtensionRight, 0),
            CantileverLeft = FractionalInchParser.ParseOr(m.CantileverLeft, 0),
            CantileverRight = FractionalInchParser.ParseOr(m.CantileverRight, 0),
            ShoeLeft = m.ShoeLeft ?? "STD",
            ShoeRight = m.ShoeRight ?? "STD",
            BridgingLeft = m.BridgingLeft ?? "",
            BridgingRight = m.BridgingRight ?? "",
            IsTieJoist = m.IsTieJoist,
            ActiveLoads = FractionalInchParser.ParseOr(m.ActiveLoads, 0),
            SequenceZone = m.SequenceZone ?? "",
            SpanLength = spanLength,
            Depth = m.Depth,
            Weight = m.Weight
        };
    }

    /// <summary>Creates JoistPropertyModel from MoravioSmartData.</summary>
    public static JoistPropertyModel FromMoravioSmartData(Services.MoravioSmartData d)
    {
        return new JoistPropertyModel
        {
            Mark = d.Mark ?? "",
            Designation = d.Designation ?? "",
            ExtensionLeft = FractionalInchParser.Format(d.ExtensionLeft),
            ExtensionRight = FractionalInchParser.Format(d.ExtensionRight),
            CantileverLeft = FractionalInchParser.Format(d.CantileverLeft),
            CantileverRight = FractionalInchParser.Format(d.CantileverRight),
            ShoeLeft = d.ShoeLeft ?? "STD",
            ShoeRight = d.ShoeRight ?? "STD",
            BridgingLeft = d.BridgingLeft ?? "",
            BridgingRight = d.BridgingRight ?? "",
            IsTieJoist = d.IsTieJoist,
            ActiveLoads = d.ActiveLoads > 0 ? d.ActiveLoads.ToString("F") : "",
            SequenceZone = d.SequenceZone ?? "",
            Depth = d.Depth,
            Weight = d.Weight
        };
    }
}
