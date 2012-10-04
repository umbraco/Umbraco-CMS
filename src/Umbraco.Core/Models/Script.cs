using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Script file
    /// </summary>
    public class Script : File
    {
        public Script(string path) : base(path)
        {
            base.Path = path;
        }

        /// <summary>
        /// Boolean indicating whether the file could be validated
        /// </summary>
        /// <remarks>
        /// The validation logic was previsouly placed in the codebehind of editScript.aspx,
        /// but has been moved to the script file so the validation is central.
        /// </remarks>
        /// <returns>True if file is valid, otherwise false</returns>
        public override bool IsValid()
        {
            //NOTE Since a script file can be both JS, Razor Views, Razor Macros and Xslt
            //it might be an idea to create validations for all 3 and divide the validation 
            //into 4 private methods.
            //See codeEditorSave.asmx.cs for reference.

            var exts = UmbracoSettings.ScriptFileTypes.Split(',').ToList();
            if (UmbracoSettings.EnableMvcSupport)
            {
                exts.Add("cshtml");
                exts.Add("vbhtml");
            }

            var dirs = SystemDirectories.Scripts;
            if (UmbracoSettings.EnableMvcSupport)
                dirs += "," + SystemDirectories.MvcViews;

            //Validate file
            var validFile = IOHelper.ValidateEditPath(Path, dirs.Split(','));

            //Validate extension
            var validExtension = IOHelper.ValidateFileExtension(Path, exts);

            return validFile && validExtension;
        }
    }
}