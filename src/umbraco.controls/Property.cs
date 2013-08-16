using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

namespace umbraco.uicontrols {
    public class PropertyPanel  : System.Web.UI.WebControls.Panel {
        public string Text { get; set; }

        protected override void Render(System.Web.UI.HtmlTextWriter writer) {
            this.CreateChildControls();
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "propertyItem" );
            foreach ( string key in Style.Keys )
            {
                writer.AddStyleAttribute( key, Style[ key ] );
            }
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( !String.IsNullOrEmpty( Text ) )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "propertyItemheader" );

                // write the alias into the title (tooltip) if one has been supplied
            if ( !String.IsNullOrEmpty( ToolTip ) )
                writer.AddAttribute( HtmlTextWriterAttribute.Title, ToolTip );

                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                if ( !String.IsNullOrEmpty(Text))
                    writer.Write( Text );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "propertyItemContent" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
            }

            try
            {
                this.RenderChildren( writer );
            }
            catch ( Exception ex )
            {
                writer.WriteLine( "Error creating control <br />" );
                writer.WriteEncodedText( ex.ToString() );
            }

            if ( !String.IsNullOrEmpty( Text ) )
                writer.RenderEndTag();

            writer.RenderEndTag();
        }

    }

}
