using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.create;
using Content=umbraco.cms.businesslogic.Content;
using umbraco.cms.helpers;
using umbraco.BasePages;

namespace umbraco.cms.presentation.create.controls
{
    /// <summary>
    ///		Summary description for content.
    /// </summary>
    public partial class content : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                sbmt.Text = ui.Text("create");
                int NodeId = int.Parse(Request["nodeID"]);

                int[] allowedIds = new int[0];
                if (NodeId > 0)
                {
                    Content c = new Document(NodeId);
                    allowedIds = c.ContentType.AllowedChildContentTypeIDs;
                }

                nodeType.Attributes.Add("onChange", "document.getElementById('typeDescription').innerHTML = typeInfo[this.selectedIndex];");

                int counter = 0;
                bool typeInited = false;
                StringBuilder js = new StringBuilder();
                foreach (DocumentType dt in DocumentType.GetAllAsList())
                {
                    string docDescription = "<em>No description available...</em>";
                    if (dt.Description != null && dt.Description != "")
                        docDescription = dt.Description;
                    docDescription = "<strong>" + dt.Text + "</strong><br/>" + docDescription.Replace(Environment.NewLine, "<br />");
                    docDescription = docDescription.Replace("'", "\'");

                    string docImage = (dt.Thumbnail != "") ? dt.Thumbnail : "../nada.gif";
                    docImage = GlobalSettings.Path + "/images/thumbnails/" + docImage;

                    ListItem li = new ListItem();
                    li.Text = dt.Text;
                    li.Value = dt.Id.ToString();

                    if (NodeId > 0)
                    {
                        foreach (int i in allowedIds) if (i == dt.Id)
                        {
                            nodeType.Items.Add(li);
                            js.Append("typeInfo[" + counter + "] = '<img src=\"" + docImage + "\"><p>" +
                                      docDescription + "</p>'\n");

                            if (!typeInited)
                            {
                                descr.Text = "<img src=\"" + docImage + "\"><p>" +
                                             docDescription + "</p>";
                                typeInited = true;
                                
                            }
                            counter++;
                        }
                    }
                    else {
                        nodeType.Items.Add(li);
                        js.Append("typeInfo[" + counter + "] = '<img src=\"" + docImage + "\"><p>" +
                                  docDescription + "</p>'\n");
                        if (!typeInited)
                        {
                            descr.Text = "<img src=\"" + docImage + "\"><p>" +
                                         docDescription + "</p>'";
                            typeInited = true;
                        }
                        counter++;
                    }


                }
                if (nodeType.Items.Count == 0) {
                    sbmt.Enabled = false;
                }
                typeJs.Text = "<script type=\"text/javascript\">\nvar typeInfo = new Array(" + counter.ToString() + ");\n " + js.ToString() + "\n</script>\n    ";
            }
        }

        #region Web Form Designer generated code

        protected override void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        ///		Required method for Designer support - do not modify
        ///		the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        #endregion

        protected void sbmt_Click(object sender, EventArgs e)
        {
            doCreation();
        }


        private void doCreation()
        {
            if (Page.IsValid)
            {
                string returnUrl = dialogHandler_temp.Create(
                    helper.Request("nodeType"),
                    int.Parse(nodeType.SelectedValue),
                    int.Parse(Request["nodeID"]),
                    rename.Text);

				BasePage.Current.ClientTools
					.ChangeContentFrameUrl(returnUrl)
					.CloseModalWindow();

            }
        }
    }
}