using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace umbraco.cms.businesslogic.skinning.dependencies
{
    public class Text : DependencyType
    {
        public System.Web.UI.WebControls.TextBox tb;
        public List<Object> _value;

        public Text()
        {
            this.Name = "Text";
            this.Description = "Will render a text input";
           

            tb = new TextBox();
            _value = new List<object>();
        }
        
        public override WebControl Editor
        {
            get
            {
                tb.TextMode = System.Web.UI.WebControls.TextBoxMode.SingleLine;
                tb.CssClass = "text";

                if (_value.Count > 0 && !string.IsNullOrEmpty(_value[0].ToString()))
                    tb.Text = _value[0].ToString();
               
                return tb;
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
                if (tb.Text != "")
                {
                    _value.Clear();
                    _value.Add(tb.Text);
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
