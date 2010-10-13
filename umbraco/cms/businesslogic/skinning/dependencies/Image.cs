using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.skinning.controls;

namespace umbraco.cms.businesslogic.skinning.dependencies
{
    public class Image: DependencyType
    {

        public string Height { get; set; }
        public string Width { get; set; }
        //currently just returning a textbox, need to replace this with a custom image control

        public ImageUploader iu;
        public List<Object> _value;

        public Image()
        {
            this.Name = "Text";
            this.Description = "Will render a text input";


            iu = new ImageUploader();
            _value = new List<object>();
        }
        
        public override WebControl Editor
        {
            get
            {
                iu.TextMode = System.Web.UI.WebControls.TextBoxMode.SingleLine;
                iu.CssClass = "image";
                iu.ID = "imageupload";

                int w;

                if(int.TryParse(Width, out w))
                    iu.ImageWidth = w;

                int h;

                if(int.TryParse(Height, out h))
                    iu.ImageHeight = h;


                if (_value.Count > 0 && !string.IsNullOrEmpty(_value[0].ToString()))
                    iu.Text = _value[0].ToString();

                return iu;
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
                if (iu.Text != "")
                {
                    _value.Clear();
                    _value.Add(iu.Text);
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
