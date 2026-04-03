using Robust.Shared.Configuration;

namespace Content.Mango.Common.CCVar;

[CVarDefs]
public sealed partial class MangoCVars
{
    /// <summary>
    /// Whether something funny would roll on the start of the shift
    /// </summary>
    public static readonly CVarDef<int> FunProb =
        CVarDef.Create("mango.fun_value", 100, CVar.SERVER); // 100%
}
