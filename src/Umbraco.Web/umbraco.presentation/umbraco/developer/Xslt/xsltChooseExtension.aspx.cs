using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core;

namespace umbraco.developer
{
    /// <summary>
    /// Summary description for xsltChooseExtension.
    /// </summary>
    [WebformsPageTreeAuthorize(Constants.Trees.Xslt)]
	public partial class xsltChooseExtension : BasePages.UmbracoEnsuredPage
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
            SortedList<string, List<string>> ht = GetXsltAssembliesAndMethods();
			if (!IsPostBack) 
			{
				assemblies.Attributes.Add("onChange", "document.forms[0].submit()");
				foreach(string s in ht.Keys)
					assemblies.Items.Add(new ListItem(s));

				// Select the umbraco extensions as default
				assemblies.Items[0].Selected = true;
			}

			string selectedMethod = "";
			if (methods.SelectedValue != "") 
			{
				selectedMethod = methods.SelectedValue;
				PlaceHolderParamters.Controls.Clear();
				PlaceHolderParamters.Controls.Add(new LiteralControl("<div class='code'>" + assemblies.SelectedItem + ":" + methods.SelectedValue.Substring(0, methods.SelectedValue.IndexOf("(")) + "("));
                PlaceHolderParamters.Controls.Add(new LiteralControl("<input type=\"hidden\" name=\"selectedMethod\" id=\"selectedMethod\" value=\"" + methods.SelectedValue.Substring(0, methods.SelectedValue.IndexOf("(")) + "\"/>"));

				int counter = 0;
                string[] props = methods.SelectedValue.Substring(methods.SelectedValue.IndexOf("(") + 1, methods.SelectedValue.IndexOf(")") - methods.SelectedValue.IndexOf("(") - 1).Split(',');               
                
                foreach (string s in props) 
				{
					if (s.Trim() != "") 
					{
						counter++;
						TextBox t = new TextBox();
						t.ID = "param" + Guid.NewGuid().ToString();
						t.Text = s.Trim();
						t.TabIndex = (short) counter;
						t.Attributes.Add("onFocus", "if (this.value == '" + s.Trim() + "') this.value = '';");
						t.Attributes.Add("onBlur", "if (this.value == '') this.value = '" + s.Trim() + "'");
						t.Attributes.Add("style", "width:80px;");
						PlaceHolderParamters.Controls.Add(t);

                        if(counter < props.Length)
                            PlaceHolderParamters.Controls.Add(new LiteralControl(","));
					}
				}

				counter++;

				//PlaceHolderParamters.Controls.Add(new LiteralControl(")</div> <br/><input type=\"button\" style=\"font-size: XX-Small\" tabIndex=\"" + counter.ToString() + "\" value=\"Insert\" onClick=\"returnResult();\"/>"));
                PlaceHolderParamters.Controls.Add(new LiteralControl(")</div>"));
                bt_insert.Enabled = true;
			} 
			else
				PlaceHolderParamters.Controls.Clear();


			if (assemblies.SelectedValue != "") 
			{
				methods.Items.Clear();
				methods.Items.Add(new ListItem("Choose method", ""));
				methods.Attributes.Add("onChange", "document.forms[0].submit()");
				List<string> methodList = ht[assemblies.SelectedValue];
                foreach (string method in methodList)
				{
					ListItem li = new ListItem(method);
                    if (method == selectedMethod)
						li.Selected = true;
					methods.Items.Add(li);
				}
			}
		}

        /// <summary>
        /// Gets the XSLT assemblies and their methods.
        /// </summary>
        /// <returns>A list of assembly names linked to a list of method signatures.</returns>
        private SortedList<string, List<string>> GetXsltAssembliesAndMethods() 
		{
            SortedList<string, List<string>> _tempAssemblies = new SortedList<string, List<string>>();

            // add all extensions definied by macro
            foreach(KeyValuePair<string, object> extension in macro.GetXsltExtensions())
                _tempAssemblies.Add(extension.Key, GetStaticMethods(extension.Value.GetType()));

            // add the Umbraco library (not included in macro extensions)
            _tempAssemblies.Add("umbraco.library", GetStaticMethods(typeof(umbraco.library)));

			return _tempAssemblies;

		}

        /// <summary>
        /// Gets the static methods of the specified type, alphabetically sorted.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A sortd list with method signatures.</returns>
        private List<string> GetStaticMethods(Type type)
        {
            List<string> methods = new List<string>();
            foreach (MethodInfo method in type.GetMethods())
            {
                if (method.IsStatic)
                {
                    // add method name to signature
                    StringBuilder methodSignature = new StringBuilder(method.Name);

                    // add parameters to signature
                    methodSignature.Append('(');
                    ParameterInfo[] parameters = method.GetParameters();
                    for(int i=0; i<parameters.Length; i++)
                    {
                        ParameterInfo parameter = parameters[i];
                        methodSignature.Append(i == 0 ? String.Empty : ", ")
                                       .Append(parameter.ParameterType.Name)
                                       .Append(' ')
                                       .Append(parameter.Name);
                    }
                    methodSignature.Append(')');

                    // add method signature to list
                    methods.Add(methodSignature.ToString());
                }
            }
            methods.Sort();
            return methods;
        }
	

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
		}
		#endregion
	}
}
