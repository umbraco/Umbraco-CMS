using System;

namespace Umbraco.Core
{
    /// <summary>
    /// Used to notify the TypeFinder to ignore any class attributed with this during it's discovery
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HideFromTypeFinderAttribute : Attribute
    {
        
    }
}