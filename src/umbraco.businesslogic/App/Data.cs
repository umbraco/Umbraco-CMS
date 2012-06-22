
namespace umbraco.businesslogic.app
{
    public class Data
    {
        public Data()
        {
            
        }

        public Data(bool HasChildren, string App, string Id, string[] Actions, string Name, string IconOpen, string IconClosed, string Description, string Url, DataProperty[] Properties)
        {
            _hasChildren = HasChildren;
            _app = App;
            _id = Id;
            _actions = Actions;
            _name = Name;
            _iconOpen = IconOpen;
            _iconClosed = IconClosed;
            _description = Description;
            _url = Url;
            _properties = Properties;
        }

        private string  _app;

        public string  App
        {
            get { return _app; }
            set { _app = value; }
        }

        private string  _id;

        public string  Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string[] _actions;

        public string[] Actions
        {
            get { return _actions; }
            set { _actions = value; }
        }

        private string  _name;

        public string  Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private bool  _hasChildren;

        public bool  HasChildren
        {
            get { return _hasChildren; }
            set { _hasChildren = value; }
        }

        private string  _iconOpen;

        public string  IconOpen
        {
            get { return _iconOpen; }
            set { _iconOpen = value; }
        }

        private string  _iconClosed;

        public string  IconClosed
        {
            get { return _iconClosed; }
            set { _iconClosed = value; }
        }

        private string  _description;

        public string  Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private string  _url;

        public string  Url
        {
            get { return _url; }
            set { _url = value; }
        }

        private DataProperty[] _properties;

        public DataProperty[] Properties
        {
            get { return _properties; }
            set { _properties = value; }
        }
	
    }

    public class DataProperty
    {
        public DataProperty() {}

        public DataProperty(string Key, string Value)
        {
            _key = Key;
            _value = Value;
        }

        private string  _key;

        public string  Key
        {
            get { return _key; }
            set { _key = value; }
        }

        private string  _value;

        public string  Value
        {
            get { return _value; }
            set { _value = value; }
        }
	
	
    }
}
