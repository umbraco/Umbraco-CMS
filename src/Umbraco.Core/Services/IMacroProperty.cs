using System.Xml;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IMacroProperty
    {
        /// <summary>
        /// The sortorder
        /// </summary>
        int SortOrder { get; set; }

        /// <summary>
        /// The alias if of the macroproperty, this is used in the special macro element
        /// <?UMBRACO_MACRO macroAlias="value"></?UMBRACO_MACRO>
        /// 
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// The userfriendly name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        int Id { get; }

        /// <summary>
        /// Gets or sets the macro.
        /// </summary>
        /// <value>The macro.</value>
        IMacro Macro { get; set; }

        /// <summary>
        /// The basetype which defines which component is used in the UI for editing content
        /// </summary>
        IMacroPropertyType Type { get; set; }

        /// <summary>
        /// Deletes the current macroproperty
        /// </summary>
        void Delete();

        void Save();

        /// <summary>
        /// Retrieve a Xmlrepresentation of the MacroProperty used for exporting the Macro to the package
        /// </summary>
        /// <param name="xd">XmlDocument context</param>
        /// <returns>A xmlrepresentation of the object</returns>
        XmlElement ToXml(XmlDocument xd);
    }
}