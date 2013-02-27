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

                <div data-bind="foreach: indexerDetails, visible: !loading()">
                    <div class="provider">
                        <a href="#" data-bind="text: Name, click: toggleProperties"></a>

                        <div data-bind="visible: showProperties">
                            
                            <div class="propertyPane index-tools">
                                <a href="#" data-bind="click: toggleIndexTools, css: {expanded:showIndexTools() }">Index info & tools</a>                                
                                <div data-bind="visible: showIndexTools() && IsLuceneIndex">
                                    <div class="index-actions">
                                        <button data-bind="click: rebuildIndex, disable: isProcessing">Rebuild index</button>
                                        <button data-bind="click: optimizeIndex, disable: isProcessing">Optimize index</button>
                                        <br/>
                                        <div data-bind="visible: isProcessing()">
                                            <cc1:ProgressBar runat="server" ID="ProgressBar1" Text="Loading..." />
                                        </div>
                                    </div>
                                    <table>
                                        <tr>
                                            <th>Documents in index</th>
                                            <td data-bind="text: DocumentCount"></td>
                                        </tr>
                                        <tr>
                                            <th>Fields in index</th>
                                            <td data-bind="text: FieldCount"></td>
                                        </tr>         
                                        <tr>
                                            <th>Has deletions? / Optimized?</th>
                                            <td>
                                                <span data-bind="text: hasDeletions"></span>
                                                (<span data-bind="text: DeletionCount"></span>)/
                                                <span data-bind="text: IsOptimized"></span>
                                            </td>
                                        </tr>
                                    </table>   
                                </div>     
                            </div>

                            <div class="propertyPane">
                                <a href="#" data-bind="click: toggleNodeTypes, css: {expanded:showNodeTypes() }">Node types</a>
                                <table data-bind="visible: showNodeTypes">
                                    <tr>
                                        <th>Include node types</th>
                                        <td data-bind="text: IndexCriteria.IncludeNodeTypes"></td>
                                    </tr>
                                    <tr>
                                        <th>Exclude node types</th>
                                        <td data-bind="text: IndexCriteria.ExcludeNodeTypes"></td>
                                    </tr>
                                    <tr data-bind="visible: IndexCriteria.ParentNodeId() != null">
                                        <th>Parent node id</th>
                                        <td data-bind="text: IndexCriteria.ParentNodeId"></td>
                                    </tr>
                                </table>
                            </div>

                            <div class="propertyPane">
                                <a href="#" data-bind="click: toggleSystemFields, css: {expanded:showSystemFields() }">System fields</a>
                                <table data-bind="visible: showSystemFields">
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

                            <div class="propertyPane">
                                <a href="#" data-bind="click: toggleUserFields, css: {expanded:showUserFields() }">User fields</a>
                                <table data-bind="visible: showUserFields">
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
                            
                            <div class="propertyPane">
                                <a href="#" data-bind="click: toggleProviderProperties, css: {expanded:showProviderProperties() }">Provider properties</a>
                                <table data-bind="visible: showProviderProperties, foreach: ProviderProperties">                                    
                                    <tr>
                                        <th data-bind="text: key"></th>
                                        <td data-bind="text: value"></td>
                                    </tr>
                                </table>
                            </div>                            
                        </div>
                    </div>
                </div>

                <h3>Searchers</h3>

                <div data-bind="foreach: searcherDetails, visible: !loading()">
                    <div class="provider">
                        <a href="#" data-bind="text: Name, click: toggleProperties"></a>

                        <table class="propertyPane" data-bind="foreach: ProviderProperties, visible: showProperties">
                            <tr>
                                <th data-bind="text: key"></th>
                                <td data-bind="text: value"></td>
                            </tr>
                        </table>

                    </div>
                </div>

            </div>
        </div>
    </div>

</div>
