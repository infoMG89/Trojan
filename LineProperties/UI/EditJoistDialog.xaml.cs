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

    private readonly List<string> _allDesignations;

    public EditJoistDialog()
    {
        InitializeComponent();
        _allDesignations = SjiJoistLibrary.GetDesignations().ToList();
        DataContext = Model;
        CmbDesignation.ItemsSource = _allDesignations;
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
        CmbDesignation.Text = model.Designation;
        CmbDesignation.SelectedItem = _allDesignations.FirstOrDefault(d =>
            string.Equals(d, model.Designation, StringComparison.OrdinalIgnoreCase));
    }

    private void CmbDesignation_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbDesignation.SelectedItem is string s)
            UpdateDepthWeightFromDesignation(s);
    }

    private void UpdateDepthWeightFromDesignation(string designation)
    {
        var rec = SjiJoistLibrary.GetByDesignation(designation);
        if (rec != null)
        {
            Model.Depth = rec.DepthInches;
            Model.Weight = rec.WeightPlf;
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
