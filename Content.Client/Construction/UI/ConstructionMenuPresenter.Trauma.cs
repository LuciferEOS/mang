// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.CCVar;
using Content.Shared.Construction.Prototypes;
using Content.Trauma.Common.Knowledge.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Client.Construction.UI;

internal sealed partial class ConstructionMenuPresenter
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    // private CommonKnowledgeSystem _knowledge = default!; // mango edit - kill skills

    private bool _autoFocusSearch;
    // private bool _useSkills; // mango edit - kill skills, linter dies cuz of this
    private Dictionary<EntProtoId, int> _skills = new();

    private void InitializeTrauma()
    {
        // _knowledge = _entManager.System<CommonKnowledgeSystem>(); // mango edit - kill skills

        _cfg.OnValueChanged(GoobCVars.AutoFocusSearchOnBuildMenu, x => _autoFocusSearch = x, true);
    }

    bool CanUnderstand(ConstructionPrototype recipe)
    {
        // mango edit - kill skills, let whoever build whatever
        // if (!_useSkills)
        //     return true; // for mobs that dont use the knowledge system, let them build anything
        //
        // foreach (var (id, needed) in recipe.Theory)
        // {
        //     if (!_skills.TryGetValue(id, out var mastery) || mastery < needed)
        //         return false;
        // }

        return true;
    }
}
