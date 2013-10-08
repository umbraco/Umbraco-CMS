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
            
                <h3>Examine Management</h3>
                
                <div data-bind="visible: loading()">
                    <cc1:ProgressBar runat="server" ID="ProgBar1" Text="Loading..." />
                </div>

                <h4>Indexers</h4>

                <div data-bind="foreach: indexerDetails, visible: !loading()">
                    <div class="provider">
                        <a href="#" data-bind="text: Name, click: toggleProperties"></a>

                        <div data-bind="visible: showProperties">
                            
                            <div class="propertyPane index-tools">
                                <a href="#" data-bind="click: toggleTools, css: {expanded:showTools() }">Index info & tools</a>                                
                                <div data-bind="visible: showTools() && IsLuceneIndex">
                                    <div class="index-actions">
                                        <div data-bind="visible: processingAttempts() < 100">
                                            <button data-bind="click: rebuildIndex, disable: isProcessing">Rebuild index</button>
                                            <button data-bind="click: optimizeIndex, disable: isProcessing, visible: DocumentCount() > 0">Optimize index</button>
                                        </div>
                                        <div data-bind="visible: isProcessing()">
                                            <cc1:ProgressBar runat="server" ID="ProgressBar1" Text="Loading..." />
                                        </div>
                                        <div class="error" data-bind="visible: processingAttempts() >= 100">
                                            The process is taking longer than expected, check the umbraco log to see if there have been any errors during this operation
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

                <h4>Searchers</h4>

                <div data-bind="foreach: searcherDetails, visible: !loading()">
                    <div class="provider">
                        <a href="#" data-bind="text: Name, click: toggleProperties"></a>
                        
                        <div data-bind="visible: showProperties">
                            
                            <div class="propertyPane search-tools">
                                <a href="#" data-bind="click: toggleTools, css: {expanded:showTools() }">Search tools</a>                       
                                <div data-bind="visible: showTools()">                                    
                                    <a class="hide" href="#" data-bind="click: closeSearch, visible: isSearching">Hide search results</a>                                    
                                    <input type="text" data-bind="value: searchText, event: {keyup: handleEnter}"/>         
                                    <button data-bind="click: search, disable: isProcessing">Search</button>
                                    <input type="radio" name="searchType" id="textSearch" value="text" data-bind="checked: searchType" />
                                    <label for="textSearch">Text Search</label>
                                    <input type="radio" name="searchType" id="luceneSearch" value="lucene" data-bind="checked: searchType" />
                                    <label for="luceneSearch">Lucene Search</label>                                                           
                                    <div class="search-results" data-bind="visible: isSearching">
                                        <div data-bind="visible: isProcessing()">
                                            <cc1:ProgressBar runat="server" ID="ProgressBar2" Text="Loading..." />
                                        </div>  
                                        <table data-bind="visible: !isProcessing()">
                                            <thead>
                                                <tr>
                                                    <th class="score">Score</th>
                                                    <th class="id">Id</th>
                                                    <th>Values</th>
                                                </tr>
                                            </thead>
                                            <tbody data-bind="foreach: searchResults">
                                                <tr>
                                                    <td data-bind="text: Score"></td>
                                                    <td data-bind="text: Id"></td>
                                                    <td data-bind="foreach: Fields">                                                        
                                                        <span class="key" data-bind="text: key"></span>
                                                        <span class="value" data-bind="text: value"></span>                                                        
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                </div>
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

            </div>
    </div>

</div>
