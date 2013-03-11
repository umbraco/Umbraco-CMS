using System;

namespace Umbraco.Core.PropertyEditors.Attributes
{
    /// <summary>
    /// Attribute determining whether or not to hide/show the label of a property
    /// </summary>
    /// <remarks>
    /// This directly affects the meta data property: HideSurroundingHtml
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    internal class ShowLabelAttribute : Attribute
    {
        public ShowLabelAttribute(bool show)
        {
            ShowLabel = show;
        }

        public bool ShowLabel { get; private set; }

    }
}