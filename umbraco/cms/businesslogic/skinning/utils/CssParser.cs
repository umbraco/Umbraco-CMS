using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO; 

namespace umbraco.cms.businesslogic.skinning.utils
{
    public class CssParser
    {
        private List<string> _styleSheets;
        private SortedList<string, StyleClass> _scc;
        public SortedList<string, StyleClass> Styles
        {
            get { return this._scc; }
            set { this._scc = value; }
        }

        public CssParser()
        {
            this._styleSheets = new List<string>();
            this._scc = new SortedList<string, StyleClass>();
        }

        public void AddStyleSheet(string path)
        {
            this._styleSheets.Add(path);
            ProcessStyleSheet(path);
        }

        public string GetStyleSheet(int index)
        {
            return this._styleSheets[index];
        }

        private void ProcessStyleSheet(string path)
        {
            string content = CleanUp(File.ReadAllText(path));
            string[] parts = content.Split('}');
            foreach (string s in parts)
            {
                if (CleanUp(s).IndexOf('{') > -1)
                {
                    FillStyleClass(s);
                }
            }
        }

        private void FillStyleClass(string s)
        {
            StyleClass sc = null;
            string[] parts = s.Split('{');
            string styleName = CleanUp(parts[0]).Trim().ToLower();

            if (this._scc.ContainsKey(styleName))
            {
                sc = this._scc[styleName];
                this._scc.Remove(styleName);
            }
            else
            {
                sc = new StyleClass();
            }

            sc.Name = styleName;

            string[] atrs = CleanUp(parts[1]).Replace("}", "").Split(';');
            foreach (string a in atrs)
            {
                if (a.Contains(":"))
                {
                    string _key = a.Split(':')[0].Trim().ToLower();
                    if (sc.Attributes.ContainsKey(_key))
                    {
                        sc.Attributes.Remove(_key);
                    }
                    sc.Attributes.Add(_key, a.Split(':')[1].Trim().ToLower());
                }
            }
            this._scc.Add(sc.Name, sc);
        }

        private string CleanUp(string s)
        {
            string temp = s;
            string reg = "(/\\*(.|[\r\n])*?\\*/)|(//.*)";
            Regex r = new Regex(reg);
            temp = r.Replace(temp, "");
            temp = temp.Replace("\r", "").Replace("\n", "");
            return temp;
        }

        public class StyleClass
        {
            private string _name = string.Empty;
            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }

            private SortedList<string, string> _attributes = new SortedList<string, string>();
            public SortedList<string, string> Attributes
            {
                get { return _attributes; }
                set { _attributes = value; }
            }
        }
    }
}
