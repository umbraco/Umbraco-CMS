namespace Umbraco.Core.Models
{
    /// <summary>
    /// Used to track the property types that are visible/editable on member profiles
    /// </summary>
    internal class MemberTypePropertyProfileAccess
    {
        public MemberTypePropertyProfileAccess(bool isVisible, bool isEditable)
        {
            IsVisible = isVisible;
            IsEditable = isEditable;
        }

        public bool IsVisible { get; set; }
        public bool IsEditable { get; set; }
    }
}