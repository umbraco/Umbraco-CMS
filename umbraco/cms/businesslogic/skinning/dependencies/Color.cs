using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.cms.businesslogic.skinning;
using umbraco.cms.businesslogic.skinning.controls;
using System.Web.UI.WebControls;

namespace umbraco.cms.businesslogic.skinning.dependencies
{
    public class Color : DependencyType
    {
        public ColorPicker cp;
        public List<Object> _value;

        public Color()
        {
            this.Name = "Color";
            this.Description = "Will render a color picker";


            cp = new ColorPicker();
            _value = new List<object>();
        }

        public override WebControl Editor
        {
            get
            {
                cp.TextMode = System.Web.UI.WebControls.TextBoxMode.SingleLine;
                cp.CssClass = "color";

                if (_value.Count > 0)
                    cp.Text = _value[0].ToString();

                return cp;
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
                if (cp.Text != "")
                {
                    _value.Clear();
                    _value.Add(cp.Text);
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
