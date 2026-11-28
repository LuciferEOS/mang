using Content.Medical.Common.Wounds;
using Content.Shared.Body;

namespace Content.Medical.Shared.Inkymed.PartStatus
{
    [Serializable, NetSerializable]
    public sealed class OpenPartStatusEvent(NetEntity target, Dictionary<ProtoId<OrganCategoryPrototype>, WoundableSeverity> status) : EntityEventArgs
    {
        public NetEntity Target = target;
        public Dictionary<ProtoId<OrganCategoryPrototype>, WoundableSeverity> Status = status;
    }

}
