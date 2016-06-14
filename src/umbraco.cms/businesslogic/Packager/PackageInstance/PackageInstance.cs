using System;

using System.Xml;
using System.Xml.XPath;
using System.Collections.Generic;

namespace umbraco.cms.businesslogic.packager
{
   public class PackageInstance
    {
       public int Id { get; set; }

       public string RepositoryGuid { get; set; }

       public string PackageGuid { get; set; }

       public bool HasUpdate { get; set; }

       public bool EnableSkins { get; set; }

       public Guid SkinRepoGuid { get; set; }


       public string Name { get; set; }

       public string Url { get; set; }

       public string Folder { get; set; }

       public string PackagePath { get; set; }

       public string Version { get; set; }

       public string Author { get; set; }

       public string AuthorUrl { get; set; }


       public string License { get; set; }

       public string LicenseUrl { get; set; }

       public string Readme { get; set; }

       public bool ContentLoadChildNodes { get; set; }

       public string ContentNodeId { get; set; }

       public List<string> Macros { get; set; }

       public List<string> Languages { get; set; }

       public List<string> DictionaryItems { get; set; }

       public List<string> Templates { get; set; }

       public List<string> Documenttypes { get; set; }

       public List<string> Stylesheets { get; set; }

       public List<string> Files { get; set; }

       public string LoadControl { get; set; }

       public string Actions { get; set; }

       public List<string> DataTypes { get; set; }

       public PackageInstance()
        {
           SkinRepoGuid = Guid.Empty;
           Name = "";
           Url = "";
           Folder = "";
           PackagePath = "";
           Version = "";
           Author = "";
           AuthorUrl = "";
           License = "";
           LicenseUrl = "";
           Readme = "";
           ContentNodeId = "";
           Macros = new List<string>();
           Languages = new List<string>();
           DictionaryItems = new List<string>();
           Templates = new List<string>();
           Documenttypes = new List<string>();
           Stylesheets = new List<string>();
           Files = new List<string>();
           LoadControl = "";
           DataTypes = new List<string>();
            EnableSkins = false;
            ContentLoadChildNodes = false;
        }
    }
}
