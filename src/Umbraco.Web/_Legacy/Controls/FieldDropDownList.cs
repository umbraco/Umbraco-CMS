using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Umbraco.Web._Legacy.Controls
{
    public class FieldDropDownList : DropDownList
    {
        private bool _customOptionsStarted;
        private bool _standardOptionsStarted;

        public string CustomPropertiesLabel { get; set; }
        public string StandardPropertiesLabel { get; set; }
        public string ChooseText { get; set; }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (Items.Count > 0 && Items[0].Text.StartsWith("#"))
            {
                Items.Insert(0, new ListItem(ChooseText, ""));
                SelectedIndex = 0;

                base.RenderContents(writer);
                return;
            }

            writer.Write("<option value=\"\">{0}</option>", ChooseText);

            foreach (ListItem item in Items)
            {
                if (!_customOptionsStarted)
                {
                    RenderOptionGroupBeginTag(CustomPropertiesLabel, writer);
                    _customOptionsStarted = true;
                }
                else if (item.Text.StartsWith("@") && !_standardOptionsStarted)
                {
                    _standardOptionsStarted = true;
                    RenderOptionGroupEndTag(writer);
                    RenderOptionGroupBeginTag(StandardPropertiesLabel, writer);
                }

                writer.WriteBeginTag("option");
                writer.WriteAttribute("value", item.Value, true);

                foreach (string key in item.Attributes.Keys)
                    writer.WriteAttribute(key, item.Attributes[key]);

                writer.Write(HtmlTextWriter.TagRightChar);
                HttpUtility.HtmlEncode(item.Text.Replace("@", ""), writer);
                writer.WriteEndTag("option");
                writer.WriteLine();
            }

            RenderOptionGroupEndTag(writer);
        }

        private void RenderOptionGroupBeginTag(string name, HtmlTextWriter writer)
        {
            writer.WriteBeginTag("optgroup");
            writer.WriteAttribute("label", name);
            writer.Write(HtmlTextWriter.TagRightChar);
            writer.WriteLine();
        }

        private void RenderOptionGroupEndTag(HtmlTextWriter writer)
        {
            writer.WriteEndTag("optgroup");
            writer.WriteLine();
        }
    }
}
