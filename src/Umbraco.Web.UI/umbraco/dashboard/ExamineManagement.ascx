<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ExamineManagement.ascx.cs" Inherits="Umbraco.Web.UI.Umbraco.Dashboard.ExamineManagement" %>
<%@ Import Namespace="Umbraco.Core" %>
<%@ Import Namespace="Umbraco.Web" %>
<%@ Import Namespace="umbraco" %>
<%@ Register TagPrefix="cc1" Namespace="Umbraco.Web.UI.Controls" Assembly="umbraco" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>


<umb:JsInclude ID="JsInclude1" runat="server" FilePath="Dashboards/ExamineManagement.js" PathNameAlias="UmbracoClient" />
<umb:CssInclude ID="CssInclude1" runat="server" FilePath="Dashboards/ExamineManagement.css" PathNameAlias="UmbracoClient" />

<script type="text/javascript">

    (function ($) {
        $(document).ready(function () {
            var mgmt = new Umbraco.Dashboards.ExamineManagement({
                container: $("#examineManagement"),
                restServiceLocation: "<%=Url.GetExamineManagementServicePath() %>"
            });
            mgmt.init();
        });
    })(jQuery);

</script>


<div id="examineManagement">

    <div class="propertypane">
        <div class="propertyItem">
            <div class="dashboardWrapper">
                <h2>Examine Management</h2>
                <img src="<%= GlobalSettings.ClientPath %>/Dashboards/ExamineManagementIco.png" alt="Examine Management" class="dashboardIcon">

                <div data-bind="visible: loading()">
                    <cc1:ProgressBar runat="server" ID="ProgBar1" Text="Loading..." />
                </div>

                <h3>Indexers</h3>

                <div data-bind="foreach: indexerDetails">
                    <div class="indexer">
                        <a href="#" data-bind="text: Name, click: toggleProperties"></a>
                        
                        <table class="propertyPane" data-bind="visible: showProperties">
                            <tr>
                                <th>Include node types</th>
                                <td data-bind="text: IndexCriteria.IncludeNodeTypes"></td>
                            </tr>
                            <tr>
                                <th>Exclude node types</th>
                                <td data-bind="text: IndexCriteria.ExcludeNodeTypes"></td>
                            </tr>
                            <tr data-bind="visible: IndexCriteria.ParentNodeId != null">
                                <th>Parent node id</th>
                                <td data-bind="text: IndexCriteria.ParentNodeId"></td>
                            </tr>
                        </table>
                        
                        <div class="propertyPane" data-bind="visible: showProperties">                                                   
                            <strong>System fields</strong>
                            <table>
                                <thead>
                                    <tr>
                                        <th>Name</th>
                                        <th>Enable sorting</th>
                                        <th>Type</th>
                                    </tr>    
                                </thead>
                                <tbody data-bind="foreach: IndexCriteria.StandardFields">
                                    <tr>
                                        <th data-bind="text: Name"></th>
                                        <td data-bind="text: EnableSorting"></td>
                                        <td data-bind="text: Type"></td>
                                    </tr>
                                </tbody>                            
                            </table>
                        </div>
                        
                        <div class="propertyPane" data-bind="visible: showProperties">                                                   
                            <strong>User fields</strong>
                            <table>
                                <thead>
                                    <tr>
                                        <th>Name</th>
                                        <th>Enable sorting</th>
                                        <th>Type</th>
                                    </tr>    
                                </thead>
                                <tbody data-bind="foreach: IndexCriteria.UserFields">
                                    <tr>
                                        <th data-bind="text: Name"></th>
                                        <td data-bind="text: EnableSorting"></td>
                                        <td data-bind="text: Type"></td>
                                    </tr>
                                </tbody>                            
                            </table>
                        </div>

                        <table class="propertyPane" data-bind="foreach: ProviderProperties, visible: showProperties">
                            <tr>
                                <th data-bind="text: key"></th>
                                <td data-bind="text: value"></td>
                            </tr>
                        </table>

                    </div>
                </div>
                
                <h3>Searchers</h3>
                


            </div>
        </div>
    </div>

</div>
