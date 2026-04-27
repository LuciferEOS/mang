namespace Content.Medical.Shared.Body;

public sealed partial class BodyStatusComponent
{
    [DataField, AutoNetworkedField]
    public Dictionary<string, HashSet<string>> LimbAlerts { get; set; } = new();

    [DataField, AutoNetworkedField]
    public Dictionary<string, short> LimbBleedSeverity { get; set; } = new(); // TODO INKYMED: nuke this and just do LimbAlerts
}
