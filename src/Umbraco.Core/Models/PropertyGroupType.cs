namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents the type of a property group.
    /// </summary>
    public enum PropertyGroupType : short
    {
        /// <summary>
        /// Display as a group (using a header).
        /// </summary>
        Group = 0,
        /// <summary>
        /// Display as an app (using a name and icon).
        /// </summary>
        App = 1,
        //Tab = 2,
        //Fieldset = 3
    }
}
