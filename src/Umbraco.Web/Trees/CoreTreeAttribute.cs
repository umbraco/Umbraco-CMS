using System;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Indicates that a tree is a core tree and shouldn't be treated as a plugin tree
    /// </summary>
    /// <remarks>
    /// This ensures that umbraco will look in the umbraco folders for views for this tree
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class CoreTreeAttribute : Attribute
    {
        
    }
}