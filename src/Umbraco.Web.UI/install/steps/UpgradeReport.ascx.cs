using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.UI.Install.Steps
{
    public partial class UpgradeReport : StepUserControl
    {
        protected Version CurrentVersion { get; private set; }
        protected Version NewVersion { get; private set; }
        protected IEnumerable<Tuple<bool, string>> Report { get; private set; }
        
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var result = ApplicationContext.Current.DatabaseContext.ValidateDatabaseSchema();
            var determinedVersion = result.DetermineInstalledVersion();

            CurrentVersion = determinedVersion;
            NewVersion = UmbracoVersion.Current;
            Report = new List<Tuple<bool, string>>();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack)
            {
                DataBind();    
            }
        }

        protected void NextButtonClick(object sender, EventArgs e)
        {
            if (ToggleView.ActiveViewIndex == 1)
            {
                GotoNextStep(sender, e);
            }
            else
            {
                CreateReport();
                ToggleView.ActiveViewIndex = 1;
                DataBind();
            }
        }

        private void CreateReport()
        {
            var errorReport = new List<Tuple<bool, string>>();

            var sql = new Sql();
            sql
                .Select(
                    SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumn("cmsDataType", "controlId"),
                    SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumn("umbracoNode", "text"))
                .From(SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("cmsDataType"))
                .InnerJoin(SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName("umbracoNode"))
                .On(
                    SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumn("cmsDataType", "nodeId") + " = " + 
                    SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumn("umbracoNode", "id"));
            
            var list = ApplicationContext.Current.DatabaseContext.Database.Fetch<dynamic>(sql);
            foreach (var item in list)
            {
                Guid legacyId = item.controlId;
                //check for a map entry
                var alias = LegacyPropertyEditorIdToAliasConverter.GetAliasFromLegacyId(legacyId);
                if (alias != null)
                {
                    //check that the new property editor exists with that alias
                    var editor = PropertyEditorResolver.Current.GetByAlias(alias);
                    if (editor == null)
                    {
                        errorReport.Add(new Tuple<bool, string>(false, string.Format("Property Editor with ID '{0}' (assigned to Data Type '{1}') has a valid GUID -> Alias map but no property editor was found. It will be replaced with a Readonly/Label property editor.", item.controlId, item.text)));
                    }
                }
                else
                {
                    errorReport.Add(new Tuple<bool, string>(false, string.Format("Property Editor with ID '{0}' (assigned to Data Type '{1}') does not have a valid GUID -> Alias map. It will be replaced with a Readonly/Label property editor.", item.controlId, item.text)));
                }
            }

            Report = errorReport;
        }
    }
}