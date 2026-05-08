namespace Content.Medical.Common._Inky.Events;

[ByRefEvent]
public record struct BrainOxygenRestoreEvent(float GasVolume, float OxygenModifier);
