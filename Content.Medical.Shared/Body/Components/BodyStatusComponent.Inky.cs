namespace Content.Medical.Shared.Body;

public sealed partial class BodyStatusComponent
{
    [ViewVariables]
    public string[] Limbs = new[] // add new limbs/body parts here if youre stupid enough to deal with them
    {
        "Head",
        "Torso",

        "ArmLeft",
        "ArmRight",

        "LegLeft",
        "LegRight" // todo inkymed wigns n tails ??
    };

    [DataField, AutoNetworkedField]
    public Dictionary<string, HashSet<string>> LimbAlerts { get; set; } = new();

}
