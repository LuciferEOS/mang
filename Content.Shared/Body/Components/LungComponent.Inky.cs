using Content.Shared.Atmos;

namespace Content.Shared.Body.Components;

public sealed partial class LungComponent
{
    [DataField] public float AsphyxationThreshold = 15f;
    [DataField] public float LungFailThreshold = 65; // todo 100?

    [DataField] public string HypoxiaAlertId = "Hypoxia"; // holy slop tho
    [DataField] public string AsphyxationAlertId = "Asphyxia";
    [DataField] public string LungFailAlertId = "RespiratoryArrest";

    /// <summary>
    /// Are lungs active
    /// </summary>
    [DataField]
    public bool IsActive = true;

    /// <summary>
    ///     Volume of our breath in liters
    /// </summary>
    [DataField]
    public float BreathVolume = Atmospherics.BreathVolume;

    /// <summary>
    ///     How much of the gas we inhale is metabolized? Value range is (0, 1]
    /// </summary>
    [DataField]
    public float Ratio = 1.0f; // also apparently this shit is never used literally anywhere?????

    /// <summary>
    /// The modifier slapped onto the lungs that tells it how much o2 needs to be supplies
    /// to the <see cref="InkyBrainComponent.OxygenLevel"/>
    /// </summary>
    /// // TODO INKYMED: enum these bitches
    [DataField]
    public Dictionary<string, float> ModifierThresholds = new()
    {
        { "Normal", 1.0f },
        { "Hypoxia", 0.8f },
        { "Asphyxia", 0.5f },
        { "ResArrest", 0.0f }
    };

    /// <summary>
    /// How much oxygen is restored to the brain per inhaling
    /// </summary>
    [DataField]
    public float OxygenRestoreRate = 15f;

}
