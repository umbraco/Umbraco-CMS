using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Template file
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Template : File
    {
        public Template(string path)
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
            var exts = new List<string>();
            if (UmbracoSettings.EnableMvcSupport)
            {
                exts.Add("cshtml");
                exts.Add("vbhtml");
            }
            else
            {
                exts.Add(UmbracoSettings.UseAspNetMasterPages ? "masterpage" : "aspx");
            }

            var dirs = SystemDirectories.Masterpages;
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