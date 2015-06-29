<%@ Control Language="c#" AutoEventWireup="True" Codebehind="ContentTypeControlNew.ascx.cs"
  Inherits="Umbraco.Web.UI.Umbraco.Controls.ContentTypeControlNew" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Import Namespace="umbraco" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="cc2" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="cdf" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<cdf:JsInclude ID="JsInclude" runat="server" FilePath="Editors/EditContentType.js" PathNameAlias="UmbracoClient" />

<cc1:TabView ID="TabView1" Height="392px" Width="552px" runat="server"></cc1:TabView>

<asp:Panel ID="pnlGeneral" runat="server"></asp:Panel>

<asp:Panel ID="pnlTab" Style="text-align: left" runat="server">

  <cc2:Pane ID="PaneTabsInherited" runat="server" Visible="false">
  <p><strong><%=ui.GetText("settings", "contentTypeEnabled")%></strong><br /><%=umbraco.ui.GetText("settings", "contentTypeUses")%> <em><asp:Literal ID="tabsMasterContentTypeName" runat="server"></asp:Literal></em> <%=umbraco.ui.GetText("settings", "asAContentMasterType")%></p>
  </cc2:Pane>
  
  <cc2:Pane ID="Pane2" runat="server">
    <cc2:PropertyPanel runat="server" id="pp_newTab" Text="New tab">
      <asp:TextBox ID="txtNewTab" runat="server"/> &nbsp; <asp:Button ID="btnNewTab" CssClass="btn" runat="server" Text="New tab" OnClick="btnNewTab_Click"/>
    </cc2:PropertyPanel>
  </cc2:Pane>
    
  <cc2:Pane ID="Pane1" runat="server" Width="216" Height="80">
    <asp:DataGrid ID="dgTabs" Width="100%" runat="server" CellPadding="2" HeaderStyle-CssClass="propertyHeader"
      ItemStyle-CssClass="propertyContent" GridLines="None" OnItemCommand="dgTabs_ItemCommand" OnItemDataBound="dgTabs_ItemDataBound" OnPreRender="dgTabs_PreRender"
      HeaderStyle-Font-Bold="True" AutoGenerateColumns="False" CssClass="tabs-table">
      <Columns>
        <asp:BoundColumn DataField="id" Visible="False"></asp:BoundColumn>
        <asp:TemplateColumn HeaderText="Name & sort order">
          <ItemTemplate>
            <i class="icon-navigation handle" style="display:inline-block; margin-top:5px;"></i>
            <asp:TextBox ID="txtTab" runat="server" Value='<%#DataBinder.Eval(Container.DataItem,"name")%>'></asp:TextBox>
            <asp:TextBox ID="txtSortOrder" runat="server" CssClass="sort-order" style="width:40px;background-color:#f2f2f2;" Value='<%#DataBinder.Eval(Container.DataItem,"order") %>'></asp:TextBox>
            <asp:Button ID="btnDelete" runat="server" Text="Delete" CommandName="Delete" CssClass="btn btn-small btn-danger" />
          </ItemTemplate>
        </asp:TemplateColumn>
      </Columns>
    </asp:DataGrid>
    <p style="text-align: center;">
      <asp:Literal ID="lttNoTabs" runat="server"></asp:Literal></p>
  </cc2:Pane>
</asp:Panel>

<asp:Panel ID="pnlInfo" runat="server">

  <cc2:Pane ID="Pane3" runat="server">
    <cc2:PropertyPanel ID="pp_name" runat="server" Text="Name">
        <asp:TextBox ID="txtName" CssClass="guiInputText guiInputStandardSize" runat="server"></asp:TextBox>
        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="txtName" runat="server" ErrorMessage="Name cannot be empty!"></asp:RequiredFieldValidator>
    </cc2:PropertyPanel>
    
    <cc2:PropertyPanel ID="pp_alias" runat="server" Text="Alias">
         <asp:TextBox ID="txtAlias" CssClass="guiInputText guiInputStandardSize" runat="server"></asp:TextBox>
         <asp:RequiredFieldValidator Display="Dynamic" ControlToValidate="txtAlias" runat="server" ErrorMessage="Alias cannot be empty!"></asp:RequiredFieldValidator>
        <asp:CustomValidator Display="Dynamic" runat="server" ID="DuplicateAliasValidator" ErrorMessage="A type with this alias already exists"></asp:CustomValidator>
    </cc2:PropertyPanel>
  </cc2:Pane>
  
  <cc2:Pane runat="server">  
    <cc2:PropertyPanel ID="pp_icon" runat="server" Text="Icon">
        <div class="umbIconDropdownList">
          <a href="#" class="btn btn-link picker-icons"> <i class="<asp:Literal runat="server" ID="lt_icon" />"></i> Choose...</a>
          <asp:HiddenField ID="tb_icon" runat="server" />
        </div>
    </cc2:PropertyPanel>
    <cc2:PropertyPanel ID="pp_description" runat="server" Text="Description">
          <asp:TextBox ID="description" runat="server" CssClass="guiInputText guiInputStandardSize" TextMode="MultiLine" Rows="3"/>
    </cc2:PropertyPanel>
  </cc2:Pane>
</asp:Panel>


<asp:Panel ID="pnlStructure" runat="server">
    <cc2:Pane ID="Pane6" runat="server">
        <cc2:PropertyPanel ID="pp_Root" runat="server">
            <asp:CheckBox runat="server" ID="allowAtRoot" Text="Yes" /><br />            
        </cc2:PropertyPanel>
        
        <cc2:PropertyPanel ID="pp_isContainer" runat="server">            
            <asp:CheckBox runat="server" ID="cb_isContainer" Text="Yes" />  
            
             <%if (cb_isContainer.Checked) { %>
                
                <div id="containerConfigPanel">
                    
                    <div class="umb-pane">
	                    <div class="umb-property">
                            <div class="control-group umb-control-group">
                                <div class="umb-el-wrap">
                                    <label class="control-label">
                                        <%=ui.Text("editcontenttype", "currentListView", Security.CurrentUser) %>
                                        <small><%=ui.Text("editcontenttype", "currentListViewDesc", Security.CurrentUser) %></small>
                                    </label>

                                    <div class="controls controls-row">
                                        <strong></strong>
                                        
                                        <em style="display:none;">&nbsp;(<%=ui.Text("general", "default", Security.CurrentUser) %>)</em>
                                        
                                        <br/>
                                        <a href="#" class="text-info">
                                            <%=ui.Text("general", "edit", Security.CurrentUser) %>
                                        </a>
                                    </div>                                    
                                    
                                    <div class="controls controls-row">
                                        <button id="createListView" class="btn" type="button" style="display: none;">
                                            <%=ui.Text("editcontenttype", "createListView", Security.CurrentUser) %>
                                        </button>
                                        <button id="removeListView" class="btn btn-warning" type="button" style="display: none;">
                                            <%=ui.Text("editcontenttype", "removeListView", Security.CurrentUser) %>
                                        </button>
                                    </div>

                                </div>
                            </div>
                        </div>
                        
                    </div>
                </div>        
                
                <%--Scripting to for configuring a list view--%>
                <script type="text/javascript">            
                    $(document).ready(function () {

                        var editor = new Umbraco.Editors.EditContentType({
                            configPanel: $("#containerConfigPanel"),
                            createListViewButton: $("#createListView"),
                            removeListViewButton: $("#removeListView"),
                            isContainerChk: $("#<%= cb_isContainer.ClientID %>"),
                            contentTypeAlias: "<%=ContentTypeAlias%>",
                            contentTypeId: <%=ContentTypeId%>,                            
                            dataTypeServiceBaseUrl: "<%=DataTypeControllerUrl %>",
                            contentTypeServiceBaseUrl: "<%=ContentTypeControllerUrl %>",
                            });

                        editor.init();

                    });

                </script>

            <%} %>       

        </cc2:PropertyPanel>   

    </cc2:Pane>

    <cc2:Pane ID="Pane5" runat="server">
        <cc2:PropertyPanel ID="pp_allowedChildren" runat="server" Text="Allowed Child nodetypes">
            <asp:CheckBoxList ID="lstAllowedContentTypes" runat="server" EnableViewState="True"/>
            <asp:PlaceHolder ID="PlaceHolderAllowedContentTypes" runat="server"/>
        </cc2:PropertyPanel>
    </cc2:Pane>
    
    <cc2:Pane ID="Pane9" runat="server">
        <cc2:PropertyPanel ID="pp_compositions" runat="server" Text="ContentType Compositions">
            <asp:CheckBoxList ID="lstContentTypeCompositions" runat="server" EnableViewState="True"/>
            <asp:PlaceHolder ID="PlaceHolderContentTypeCompositions" runat="server"/>
        </cc2:PropertyPanel>
    </cc2:Pane>
</asp:Panel>


<asp:Panel ID="pnlProperties" runat="server">
  <cc2:Pane ID="PanePropertiesInherited" runat="server" Visible="false">
  <p><strong>Master Content Type enabled</strong><br />This Content Type uses <em><asp:Literal ID="propertiesMasterContentTypeName" runat="server"></asp:Literal></em> as a Master Content Type. Properties from Master Content Types are not shown and can only be edited on the Master Content Type itself</p>
  </cc2:Pane>
  
  <cc2:Pane ID="Pane4" runat="server" Width="216" Height="80">
      <div class="genericPropertyForm">
            <asp:PlaceHolder ID="PropertyTypeNew" runat="server"></asp:PlaceHolder>
            <asp:PlaceHolder ID="PropertyTypes" runat="server"></asp:PlaceHolder>
      </div>
    </cc2:Pane>
</asp:Panel>
<%-- cannot put a <%= block here 'cos it prevents the Controls collection from being modified = use a literal --%>
<script type="text/javascript">
    $(function () {
        <asp:Literal runat="server" ID="checkTxtAliasJs" />
    });


    jQuery(document).ready(function () {

        checkAlias('.prop-alias');
   
        duplicatePropertyNameAsSafeAlias('ul.addNewProperty');
   
        jQuery(".picker-icons").click(function(){
            var that = this;
            UmbClientMgr.openAngularModalWindow({
                template: 'views/common/dialogs/iconpicker.html', 
                callback: function(data){
                    jQuery(that).next().val(data);
                    jQuery(that).find("i").attr("class", data);
                }}); 
            
            return false;
        });
        
        $("table.tabs-table tr.propertyContent input.sort-order").keydown(function(e) {
            // Allow: backspace, delete, tab, escape, enter and .
            if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 190]) !== -1 ||
                // Allow: Ctrl+A
                (e.keyCode == 65 && e.ctrlKey === true) || 
                // Allow: home, end, left, right
                (e.keyCode >= 35 && e.keyCode <= 39)) {
                // let it happen, don't do anything
                return;
            }
            // Ensure that it is a number and stop the keypress
            if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                e.preventDefault();
            }
        });


        // Make each tr of the tabs table sortable (prevent dragging of header row, and set up a callback for when row dragged)
        $("table.tabs-table tbody").sortable({
            containment: 'parent',
            cancel: '.propertyHeader, input',
            tolerance: 'pointer',
            update: function (event, ui) {
                saveOrder();
            }
        });

        // Fired after row dragged; go through each tr and save position to the hidden sort order field
        function saveOrder() {
            $("table.tabs-table tbody tr.propertyContent").each(function (index) {
                $("input.sort-order", this).val(index + 1);
            });
        }

    });
</script>