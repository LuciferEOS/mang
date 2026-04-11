using Content.Shared.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Inky.Shared.Concussion;

public abstract class SharedConcussionSystem : EntitySystem
{
    /// <summary>
    /// updates <see cref="ConcussionThresholdComponent.CurrentState"/> based on
    /// <see cref="ConcussionThresholdComponent.StoredDamage"/> and raises
    /// <see cref="ConcussionStateChangedEvent"/> if the state changed
    /// </summary>
    protected void UpdateConcussionState(EntityUid uid, ConcussionThresholdComponent comp)
    {
        var newState = GetStateForDamage(comp);
        if (newState == comp.CurrentState)
            return;

        var oldState = comp.CurrentState;
        comp.CurrentState = newState;

        var ev = new ConcussionStateChangedEvent(uid, comp, oldState, newState);
        RaiseLocalEvent(uid, ref ev);
    }

    private ConcussionState GetStateForDamage(ConcussionThresholdComponent comp)
    {
        var result = ConcussionState.Sane;

        foreach (var (threshold, state) in comp.Thresholds)
        {
            if (comp.StoredDamage < threshold)
                break;

            if (comp.AllowedStates.Contains(state))
                result = state;
        }

        return result;
    }
}
