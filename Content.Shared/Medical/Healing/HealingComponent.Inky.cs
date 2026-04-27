namespace Content.Shared.Medical.Healing;

public sealed partial class HealingComponent
{
    /// <summary>
    /// if true, this item can only be used via the body dol and not via UseInHand
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool DollOnly = true;
}
