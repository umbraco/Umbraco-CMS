using System;

namespace Umbraco.ModelsBuilder
{
    /// <summary>
    /// Indicates that an Assembly is a PureLive models assembly.
    /// </summary>
    /// <remarks>Though technically not required, ie models will work without it, the attribute
    /// can be used by Umbraco view models binder to figure out whether the model type comes
    /// from a PureLive Assembly.</remarks>
    [Obsolete("Should use ModelsBuilderAssemblyAttribute but that requires a change in Umbraco Core.")]
    [AttributeUsage(AttributeTargets.Assembly /*, AllowMultiple = false, Inherited = false*/)]
    public sealed class PureLiveAssemblyAttribute : Attribute
    { }
}
