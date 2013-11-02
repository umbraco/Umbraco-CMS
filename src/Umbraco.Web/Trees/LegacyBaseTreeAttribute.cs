using System;
using Umbraco.Core;
using umbraco.cms.presentation.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// This attribute is used purely to maintain some compatibility with legacy webform tree pickers
    /// </summary>
    /// <remarks>
    /// This allows us to attribute new trees with their legacy counterparts and when a legacy tree is loaded this will indicate 
    /// on the new tree which legacy tree to load (it won't actually render using the new tree)
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class LegacyBaseTreeAttribute : Attribute
    {
        public Type BaseTreeType { get; private set; }

        public LegacyBaseTreeAttribute(Type baseTreeType)
        {
            if (!TypeHelper.IsTypeAssignableFrom<BaseTree>(baseTreeType))
            {
                throw new InvalidOperationException("The type for baseTreeType must be assignable from " + typeof(BaseTree));
            }

            BaseTreeType = baseTreeType;
        }
    }
}