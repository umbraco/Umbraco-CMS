using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.IO;
using Umbraco.Web.UI;
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
            if (IsPostBack == false)
            {
                sbmt.Text = ui.Text("create");
                var nodeId = int.Parse(Request["nodeID"]);

                var allowedIds = new int[0];
                if (nodeId > 0)
                {
                    var c = new Document(nodeId);
                    allowedIds = c.ContentType.AllowedChildContentTypeIDs;
                }

                nodeType.Attributes.Add("onChange", "document.getElementById('typeDescription').innerHTML = typeInfo[this.selectedIndex];");

                var counter = 0;
                var typeInited = false;
                var js = new StringBuilder();
                var documentTypeList = DocumentType.GetAllAsList().ToList();
                foreach (var dt in documentTypeList)
                {
                    string docDescription = "<em>No description available...</em>";
                    if (string.IsNullOrEmpty(dt.Description) == false)
                        docDescription = System.Web.HttpUtility.HtmlEncode(dt.Description);

                    docDescription = "<strong>" + dt.Text + "</strong><br/>" + docDescription.Replace(Environment.NewLine, "<br />");
                    docDescription = docDescription.Replace("'", "\\'");

                    var docImage = (dt.Thumbnail != "") ? dt.Thumbnail : "../nada.gif";
                    docImage = IOHelper.ResolveUrl( SystemDirectories.Umbraco ) + "/images/thumbnails/" + docImage;

                    var li = new ListItem();
                    li.Text = dt.Text;
                    li.Value = dt.Id.ToString();

                    if (nodeId > 0)
                    {
                        foreach (var i in allowedIds) if (i == dt.Id)
                        {
                            nodeType.Items.Add(li);
                            js.Append("typeInfo[" + counter + "] = '<img src=\"" + docImage + "\"><p>" +
                                      docDescription + "</p>'\n");

                            if (typeInited == false)
                            {
                                descr.Text = "<img src=\"" + docImage + "\"><p>" +
                                             docDescription + "</p>";
                                typeInited = true;
                                
                            }
                            counter++;
                        }
                    }
                    // The Any check is here for backwards compatibility, if none are allowed at root, then all are allowed
                    else if (documentTypeList.Any(d => d.AllowAtRoot) == false || dt.AllowAtRoot)
                    {
                        nodeType.Items.Add(li);
                        js.Append("typeInfo[" + counter + "] = '<img src=\"" + docImage + "\"><p>" +
                                  docDescription + "</p>'\n");
                        if (typeInited == false)
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

        
        protected void sbmt_Click(object sender, EventArgs e)
        {
            DoCreation();
        }

        private void DoCreation()
        {
            if (Page.IsValid)
            {
                var returnUrl = LegacyDialogHandler.Create(
                    new HttpContextWrapper(Context),
                    BasePage.Current.getUser(),
                    helper.Request("nodeType"),
                    int.Parse(Request["nodeID"]),
                    rename.Text,
                    int.Parse(nodeType.SelectedValue));

				BasePage.Current.ClientTools
					.ChangeContentFrameUrl(returnUrl)
					.CloseModalWindow();

            }
        }
    }
}