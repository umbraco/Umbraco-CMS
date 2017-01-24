using System;

namespace Umbraco.Core.Scoping
{
    [Flags]
    public enum ActionTime
    {
        None = 0,
        BeforeCommit = 1,
        BeforeEvent = 2,
        BeforeDispose = 4
    }
}