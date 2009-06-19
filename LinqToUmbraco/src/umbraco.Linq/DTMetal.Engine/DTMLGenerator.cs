using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace umbraco.Linq.DTMetal.Engine
{
    public sealed class DTMLGenerator
    {
        private string fileExtension = ".dtml";

        public string SavePath { get; private set; }
        public string umbracoConnectionString { get; private set; }
        public string DataContextName { get; private set; }
        public bool DisablePluralization { get; set; }

        public DTMLGenerator(string savePath, string umbracoConnectionString, string dataContextName, bool disablePluralization)
        {
            if (string.IsNullOrEmpty(savePath))
            {
                throw new ArgumentNullException("savePath");
            }
            if (string.IsNullOrEmpty(umbracoConnectionString))
            {
                throw new ArgumentNullException("umbracoConnectionString");
            }
            
            this.SavePath = savePath;
            this.umbracoConnectionString = umbracoConnectionString;
            this.DataContextName = dataContextName;
        }

        public void GenerateDTMLFile()
        {
            using (var objBuilder = new DocTypeObjectBuilder(this.umbracoConnectionString))
            {
                objBuilder.LoadDocTypes();

                var dtmlGen = new DocTypeMarkupLanguageBuilder(objBuilder.DocumentTypes, this.DataContextName, this.DisablePluralization);

                dtmlGen.BuildXml();

                dtmlGen.Save(Path.Combine(this.SavePath, (string.IsNullOrEmpty(this.DataContextName) ? "umbraco" : this.DataContextName) + fileExtension));
            }
        }
    }
}
