using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.cms.businesslogic.skinning.controls;
using System.Web.UI.WebControls;

namespace umbraco.cms.businesslogic.skinning.dependencies
{
    public class Slider : DependencyType
    {
        public  SliderControl sc;
        public List<Object> _value;

        public string Minimum { get; set; }
        public string Maximum { get; set; }
        public string Initial { get; set; }

        public Slider()
        {
            this.Name = "Slider";
            this.Description = "Will render a slider";


            sc = new SliderControl();
            _value = new List<object>();
        }

        public override WebControl Editor
        {
            get
            {

                int min;

                if (int.TryParse(Minimum, out min))
                    sc.MinimumValue = min;

                int max;

                if (int.TryParse(Maximum, out max))
                    sc.MaximumValue = max;

                int init;

                if (int.TryParse(Initial, out init))
                {
                    sc.InitialValue = init;
                    sc.Text = init.ToString();
                }


                if (_value.Count > 0 && !string.IsNullOrEmpty(_value[0].ToString()))
                    sc.Text = _value[0].ToString();

                return sc;
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
                if (sc.Text != "")
                {
                    _value.Clear();
                    _value.Add(sc.Text);
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
