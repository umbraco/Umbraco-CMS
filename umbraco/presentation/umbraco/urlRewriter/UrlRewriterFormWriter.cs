using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace umbraco.presentation.urlRewriter
{
    public class FormRewriterControlAdapter : System.Web.UI.Adapters.ControlAdapter
    {
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(new UrlRewriterFormWriter(writer));
        }
    }

    public class UrlRewriterFormWriter : HtmlTextWriter
    {

        public UrlRewriterFormWriter(HtmlTextWriter writer) : base(writer)
        {
            base.InnerWriter = writer.InnerWriter;

        }

        public UrlRewriterFormWriter(System.IO.TextWriter writer)
            : base(writer)
        {

            base.InnerWriter = writer;

        }
        public override void WriteAttribute(string name, string value, bool fEncode)
        {
            if (name == "action")
            {
                HttpContext Context;
                Context = HttpContext.Current;
                if (Context.Items["ActionAlreadyWritten"] == null)
                {
                    string formAction = "";
                    if (Context.Items["VirtualUrl"] != null && !String.IsNullOrEmpty(Context.Items["VirtualUrl"].ToString()))
                    {
                        formAction = Context.Items["VirtualUrl"].ToString();
                    }
                    else
                    {
                        formAction = Context.Items[requestModule.ORIGINAL_URL_CXT_KEY].ToString();
                        if (!String.IsNullOrEmpty(Context.Request.Url.Query))
                        {
                            formAction += Context.Request.Url.Query;
                        }
                    }
                    value = formAction;
                    Context.Items["ActionAlreadyWritten"] = true;
                }
            }
            base.WriteAttribute(name, value, fEncode);
        }
    }
}
