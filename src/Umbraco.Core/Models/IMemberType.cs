namespace Umbraco.Core.Models
{
    /// <summary>
    /// Defines a MemberType, which Member is based on
    /// </summary>
    public interface IMemberType : IContentTypeComposition
    {
        /// <summary>
        /// Gets a boolean indicating whether a Property is editable by the Member.
        /// </summary>
        /// <param name="propertyTypeAlias">PropertyType Alias of the Property to check</param>
        /// <returns></returns>
        bool MemberCanEditProperty(string propertyTypeAlias);

        /// <summary>
        /// Gets a boolean indicating whether a Property is visible on the Members profile.
        /// </summary>
        /// <param name="propertyTypeAlias">PropertyType Alias of the Property to check</param>
        /// <returns></returns>
        bool MemberCanViewProperty(string propertyTypeAlias);

        /// <summary>
        /// Sets a boolean indicating whether a Property is editable by the Member.
        /// </summary>
        /// <param name="propertyTypeAlias">PropertyType Alias of the Property to set</param>
        /// <param name="value">Boolean value, true or false</param>
        void SetMemberCanEditProperty(string propertyTypeAlias, bool value);

        /// <summary>
        /// Sets a boolean indicating whether a Property is visible on the Members profile.
        /// </summary>
        /// <param name="propertyTypeAlias">PropertyType Alias of the Property to set</param>
        /// <param name="value">Boolean value, true or false</param>
        void SetMemberCanViewProperty(string propertyTypeAlias, bool value);
    }
}