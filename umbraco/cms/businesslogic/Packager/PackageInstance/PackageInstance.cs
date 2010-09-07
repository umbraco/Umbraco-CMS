using System;

using System.Xml;
using System.Xml.XPath;
using System.Collections.Generic;

namespace umbraco.cms.businesslogic.packager
{
   public class PackageInstance
    {
        private int _id;

        private string _name = "";
        private string _url = "";
        private string _folder = "";
        private string _packagePath = "";
        private string _version = "";

        private string _author = "";
        private string _authorUrl = "";

        private string _license = "";
        private string _licenseUrl = "";

        private string _readMe = "";
            
        private bool _contentLoadChildNodes = false;
        private string _contentNodeId = "";

        private List<string> _macros = new List<string>();
        private List<string> _templates = new List<string>();
        private List<string> _documentTypes = new List<string>();
        private List<string> _stylesheets = new List<string>();
        private List<string> _files = new List<string>();
        private List<string> _languages = new List<string>();
        private List<string> _dictionaryItems = new List<string>();
        private List<string> _dataTypes = new List<string>();

        private string _loadControl = "";
        private string _repoGuid;
        private string _packageGuid;
        private bool _hasUpdate;
        private bool _enableSkins = false; 
        private string _actions;

        public int Id
        {
            get { return _id; }
            set {_id = value; }
        }

        public String RepositoryGuid {
            get { return _repoGuid; }
            set { _repoGuid = value; }
        }

        public String PackageGuid {
            get { return _packageGuid; }
            set { _packageGuid = value; }
        }

        public bool HasUpdate {
            get { return _hasUpdate; }
            set { _hasUpdate = value; }
        }

        public bool EnableSkins
        {
            get { return _enableSkins; }
            set { _enableSkins = value; }
        }

        public String Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        public String Url
        {
            get { return _url; }
            set
            {
                _url = value;
            }
        }

        public String Folder
        {
            get { return _folder; }
            set
            {
                _folder = value;
            }
        }

        public String PackagePath
        {
            get { return _packagePath; }
            set
            {
                _packagePath = value;
            }
        }

        public String Version
        {
            get { return _version; }
            set
            {
                _version = value;
            }
        }

        public String Author
        {
            get { return _author; }
            set
            {
                _author = value;
            }
        }

        public String AuthorUrl
        {
            get { return _authorUrl; }
            set
            {
                _authorUrl = value;
            }
        }


        public String License
        {
            get { return _license; }
            set
            {
                _license = value;
            }
        }

        public String LicenseUrl
        {
            get { return _licenseUrl; }
            set
            {
                _licenseUrl = value;
            }
        }

        public String Readme
        {
            get { return _readMe ; }
            set
            {
                _readMe = value;
            }
        }

        public bool ContentLoadChildNodes
        {
            get { return _contentLoadChildNodes; }
            set
            {
                _contentLoadChildNodes = value;
            }
        }
        public string ContentNodeId
        {
            get { return _contentNodeId; }
            set
            {
                _contentNodeId = value;
            }
        }

        public List<string> Macros
        {
            get { return _macros; }
            set
            {
                _macros = value;
            }
        }

       public List<string> Languages {
           get { return _languages; }
           set {
               _languages = value;
           }
       }

       public List<string> DictionaryItems {
           get { return _dictionaryItems; }
           set {
               _dictionaryItems = value;
           }
       }

        public List<string> Templates
        {
            get { return _templates; }
            set
            {
                _templates = value;
            }
        }

        public List<string> Documenttypes
        {
            get { return _documentTypes; }
            set
            {
                _documentTypes = value;
            }
        }

        public List<string> Stylesheets
        {
            get { return _stylesheets; }
            set
            {
               _stylesheets = value;
            }
        }

        public List<string> Files
        {
            get { return _files; }
            set
            {
                _files = value;
            }
        }

        public String LoadControl
        {
            get { return _loadControl; }
            set
            {
                _loadControl = value;
            }
        }

        public String Actions
        {
            get { return _actions; }
            set
            {
                _actions = value;
            }
        }

        public List<string> DataTypes {
            get { return _dataTypes; }
            set {
                _dataTypes = value;
            }
        }

        public PackageInstance() {}
    }
}
