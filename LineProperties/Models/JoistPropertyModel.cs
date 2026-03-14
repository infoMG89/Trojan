using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LineProperties.Models;

/// <summary>
/// View model for Edit Joist Smart Data dialog. Binds to MORAVIO_SMART XData fields.
/// </summary>
public class JoistPropertyModel : INotifyPropertyChanged
{
    private string _mark = "";
    private string _designation = "10K1";
    private string _extensionLeft = "";
    private string _extensionRight = "";
    private string _cantileverLeft = "";
    private string _cantileverRight = "";
    private string _shoeLeft = "STD";
    private string _shoeRight = "STD";
    private string _bridgingLeft = "";
    private string _bridgingRight = "";
    private bool _isTieJoist;
    private string _activeLoads = "";
    private string _sequenceZone = "";
    private double _depth;
    private double _weight;

    public string Mark
    {
        get => _mark;
        set => SetField(ref _mark, value);
    }

    public string Designation
    {
        get => _designation;
        set => SetField(ref _designation, value);
    }

    public string ExtensionLeft
    {
        get => _extensionLeft;
        set => SetField(ref _extensionLeft, value);
    }

    public string ExtensionRight
    {
        get => _extensionRight;
        set => SetField(ref _extensionRight, value);
    }

    public string CantileverLeft
    {
        get => _cantileverLeft;
        set => SetField(ref _cantileverLeft, value);
    }

    public string CantileverRight
    {
        get => _cantileverRight;
        set => SetField(ref _cantileverRight, value);
    }

    public string ShoeLeft
    {
        get => _shoeLeft;
        set => SetField(ref _shoeLeft, value);
    }

    public string ShoeRight
    {
        get => _shoeRight;
        set => SetField(ref _shoeRight, value);
    }

    public string BridgingLeft
    {
        get => _bridgingLeft;
        set => SetField(ref _bridgingLeft, value);
    }

    public string BridgingRight
    {
        get => _bridgingRight;
        set => SetField(ref _bridgingRight, value);
    }

    public bool IsTieJoist
    {
        get => _isTieJoist;
        set => SetField(ref _isTieJoist, value);
    }

    public string ActiveLoads
    {
        get => _activeLoads;
        set => SetField(ref _activeLoads, value);
    }

    public string SequenceZone
    {
        get => _sequenceZone;
        set => SetField(ref _sequenceZone, value);
    }

    /// <summary>Depth in inches (from section library, read-only for display).</summary>
    public double Depth
    {
        get => _depth;
        set => SetField(ref _depth, value);
    }

    /// <summary>Weight in plf (from section library, read-only for display).</summary>
    public double Weight
    {
        get => _weight;
        set => SetField(ref _weight, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}
