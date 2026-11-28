using Content.Medical.Shared.Inkymed.PartStatus;

namespace Content.Medical.Client.Targeting;

public sealed partial class TargetingSystem
{
    /// <inheritdoc/>
    private void InitialiseInky()
    {
        SubscribeNetworkEvent<OpenPartStatusEvent>(OnOtherPartStatusOpened);
    }
    public event Action<OpenPartStatusEvent>? OtherPartStatusOpened;
    private void OnOtherPartStatusOpened(OpenPartStatusEvent ev) => OtherPartStatusOpened?.Invoke(ev);

}
