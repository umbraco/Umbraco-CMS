using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace umbraco.cms.businesslogic.skinning.dependencies
{
    public class Option : DependencyType
    {
        public DropDownList ddl;
        public List<Object> _value;

        public string Options { get; set; }

        public Option()
        {
            this.Name = "Option";
            this.Description = "Will render a dropdown";


            ddl = new DropDownList();
            _value = new List<object>();
        }

        public override WebControl Editor
        {
            get
            {
                ddl.Items.Clear();


                ddl.Items.Add("");

                foreach (string option in Options.Split('|'))
                {
                    string text = option.Split(';')[0];
                    string value = option.Split(';').Length == 2 ? option.Split(';')[1] : text;

                    ddl.Items.Add(new ListItem(text,value));
                }

                if (_value.Count > 0)
                    ddl.SelectedValue = _value[0].ToString();

                return ddl;
            }
            set
            {
                base.Editor = value;
            }
        }

        public override List<Object> Values
        {
            get
            {
                if (ddl.SelectedValue != "")
                {
                    _value.Clear();
                    _value.Add(ddl.SelectedValue);
                }
                return _value;
            }
            set
            {
                _value = value;
            }
        }
    }
}
