using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Umbraco.Core.IO;

namespace Umbraco.Core.Models
{
    //internal class PartialViewMacro : PartialView
    //{
    //    public PartialViewMacro()
    //        : base(string.Empty)
    //    {
    //    }

    //    public PartialViewMacro(string path) : base(path)
    //    {
    //    }

    //    public IMacro AssociatedMacro { get; set; }
    //}

    /// <summary>
    /// Represents a Partial View file
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class PartialView : File
    {
        //public PartialView(): base(string.Empty)
        //{
        //}

        public PartialView(string path)
            : base(path)
        {
            base.Path = path;
        }

        /// <summary>
        /// Boolean indicating whether the file could be validated
        /// </summary>
        /// <returns>True if file is valid, otherwise false</returns>
        public override bool IsValid()
        {            
            //TODO: Why is this here? Needs to go on the FileService

            var validatePath = IOHelper.ValidateEditPath(Path, new[] { SystemDirectories.MvcViews + "/Partials/", SystemDirectories.MvcViews + "/MacroPartials/" });
            var verifyFileExtension = IOHelper.VerifyFileExtension(Path, new List<string> { "cshtml" });

            return validatePath && verifyFileExtension;
        }
        
    }
}