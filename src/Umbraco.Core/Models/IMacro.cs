using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models
{
    public interface IMacro : IUmbracoEntity
    {
        /// <summary>
        /// If set to true, the macro can be inserted on documents using the richtexteditor.
        /// </summary>
        bool UseInEditor { get; set; }

        /// <summary>
        /// The cache refreshrate - the maximum amount of time the macro should remain cached in the umbraco
        /// runtime layer.
        /// 
        /// The macro caches are refreshed whenever a document is changed
        /// </summary>
        int RefreshRate { get; set; }

        /// <summary>
        /// The alias of the macro - are used for retrieving the macro when parsing the {?UMBRACO_MACRO}{/?UMBRACO_MACRO} element,
        /// by using the alias instead of the Id, it's possible to distribute macroes from one installation to another - since the id
        /// is given by an autoincrementation in the database table, and might be used by another macro in the foreing umbraco
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// If the macro is a wrapper for a custom control, this is the assemly name from which to load the macro
        /// 
        /// specified like: /bin/mydll (without the .dll extension)
        /// </summary>
        string Assembly { get; set; }

        /// <summary>
        /// The relative path to the usercontrol or the assembly type of the macro when using .Net custom controls
        /// </summary>
        /// <remarks>
        /// When using a user control the value is specified like: /usercontrols/myusercontrol.ascx (with the .ascx postfix)
        /// </remarks>
        string Type { get; set; }

        /// <summary>
        /// The xsl file used to transform content
        /// 
        /// Umbraco assumes that the xslfile is present in the "/xslt" folder
        /// </summary>
        string Xslt { get; set; }

        /// <summary>
        /// This field is used to store the file value for any scripting macro such as python, ruby, razor macros or Partial View Macros        
        /// </summary>
        /// <remarks>
        /// Depending on how the file is stored depends on what type of macro it is. For example if the file path is a full virtual path
        /// starting with the ~/Views/MacroPartials then it is deemed to be a Partial View Macro, otherwise the file extension of the file
        /// saved will determine which macro engine will be used to execute the file.
        /// </remarks>
        string ScriptingFile { get; set; }

        /// <summary>
        /// The python file used to be executed
        /// 
        /// Umbraco assumes that the python file is present in the "/python" folder
        /// </summary>
        bool RenderContent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [cache personalized].
        /// </summary>
        /// <value><c>true</c> if [cache personalized]; otherwise, <c>false</c>.</value>
        bool CachePersonalized { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the macro is cached for each individual page.
        /// </summary>
        /// <value><c>true</c> if [cache by page]; otherwise, <c>false</c>.</value>
        bool CacheByPage { get; set; }

        /// <summary>
        /// Properties which are used to send parameters to the xsl/usercontrol/customcontrol of the macro
        /// </summary>
        IMacroProperty[] Properties { get; }

    }
}