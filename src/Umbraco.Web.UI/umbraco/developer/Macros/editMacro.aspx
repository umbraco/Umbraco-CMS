<%@ Page Language="c#" MasterPageFile="../../masterpages/umbracoPage.Master" Title="Edit macro"
    CodeBehind="EditMacro.aspx.cs" AutoEventWireup="True" Inherits="Umbraco.Web.UI.Umbraco.Developer.Macros.EditMacro" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="CD" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
    
    <CD:CssInclude ID="CssInclude1" runat="server" FilePath="Editors/EditMacro.css" PathNameAlias="UmbracoClient" />

    <script type="text/javascript">
        
        //handles the change selection of the drop downs to populate the text box
        (function($) {
            $(document).ready(function () {

                //on drop down change, update the text box and clear other text boxes
                $(".fileChooser select").change(function () {
                    //update the txt box
                    var txt = $(this).prev("input[type='text']");
                    txt.val($(this).val());
                    //clear other text boxes
                    $(".fileChooser input[type='text']").not(txt).val("");
                    //reset other drop downs
                    $(".fileChooser select").not($(this)).val("");
                });           
                
                UmbClientMgr.appActions().bindSaveShortCut();

                // U4-667: Make the "Render content in editor" checkbox dependent on the "Use in editor checkbox"
                var useInEditorCheckBox = $("#<%= macroEditor.ClientID %>");
                var renderInEditorCheckBox = $("#<%= macroRenderContent.ClientID %>");

                function toggle() {
                    var disabled = useInEditorCheckBox.is(":checked") == false;
                    renderInEditorCheckBox.prop("disabled", disabled);
                }

                toggle();

                useInEditorCheckBox.on("change", function () {
                    toggle();
                });

                
            });
        })(jQuery);
        
    </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">
    <cc1:TabView ID="TabView1" runat="server"></cc1:TabView>

    <cc1:Pane ID="Pane1" runat="server">
        <cc1:PropertyPanel runat="server" Text="Name">
            <asp:TextBox ID="macroName" runat="server" CssClass="guiInputText"></asp:TextBox>
        </cc1:PropertyPanel>
        <cc1:PropertyPanel runat="server" Text="Alias">
            <asp:TextBox ID="macroAlias" runat="server" CssClass="guiInputText"></asp:TextBox>
        </cc1:PropertyPanel>    
    </cc1:Pane>
    
    
    <cc1:Pane ID="Pane1_2" runat="server" title="Choose a file to render" CssClass="fileChooser">

        <cc1:PropertyPanel runat="server" Text="MVC Partial view">
             <asp:TextBox ID="SelectedPartialView" runat="server" CssClass="guiInputText"></asp:TextBox>
                    <asp:DropDownList ID="PartialViewList" runat="server" >
                    </asp:DropDownList>
        </cc1:PropertyPanel>

        <cc1:PropertyPanel runat="server" Text="XSLT">
            <asp:TextBox ID="macroXslt" runat="server" CssClass="guiInputText"></asp:TextBox>
                    <asp:DropDownList ID="xsltFiles" runat="server">
                    </asp:DropDownList>
        </cc1:PropertyPanel>

        <cc1:PropertyPanel runat="server" Text="usercontrol">
             <asp:TextBox ID="macroUserControl" runat="server" CssClass="guiInputText"></asp:TextBox>
                    <asp:DropDownList ID="userControlList" runat="server">
                    </asp:DropDownList>
                    <asp:PlaceHolder ID="assemblyBrowserUserControl" runat="server"></asp:PlaceHolder>
        </cc1:PropertyPanel>

        <cc1:PropertyPanel runat="server" Text="Razor script">
             <asp:TextBox ID="macroPython" runat="server" CssClass="guiInputText"></asp:TextBox>
                    <asp:DropDownList ID="pythonFiles" runat="server">
                    </asp:DropDownList>
        </cc1:PropertyPanel>

        <asp:PlaceHolder runat="server" Visible="false">
              <asp:TextBox ID="macroAssembly" runat="server" CssClass="guiInputText"></asp:TextBox>
                    (Assembly)<br />
                    <asp:TextBox ID="macroType" runat="server" CssClass="guiInputText"></asp:TextBox>
                    (Type)
                    <asp:PlaceHolder ID="assemblyBrowser" runat="server"></asp:PlaceHolder>  
        </asp:PlaceHolder>
    </cc1:Pane>
    
    <cc1:Pane ID="Pane1_3" runat="server" Title="Editor settings">

        <cc1:PropertyPanel runat="server" Text="Use in rich text editor and the grid">
            <asp:CheckBox ID="macroEditor" runat="server" Text="Yes"></asp:CheckBox>
        </cc1:PropertyPanel>
        
        <cc1:PropertyPanel runat="server" Text="Render in rich text editor and the grid">
             <asp:CheckBox ID="macroRenderContent" runat="server" Text="Yes"></asp:CheckBox>
        </cc1:PropertyPanel>
    </cc1:Pane>
    <cc1:Pane ID="Pane1_4" runat="server" Title="Cache settings">

        <cc1:PropertyPanel runat="server" Text="Cache period">
            <asp:TextBox ID="cachePeriod" runat="server" CssClass="guiInputText input-small"></asp:TextBox>&nbsp;Seconds
        </cc1:PropertyPanel>

        <cc1:PropertyPanel runat="server" Text="Cache by page">
            <asp:CheckBox ID="cacheByPage" runat="server" Text="Yes"></asp:CheckBox>
        </cc1:PropertyPanel>

        <cc1:PropertyPanel runat="server" Text="Cache personalized">
             <asp:CheckBox ID="cachePersonalized" runat="server" Text="Yes"></asp:CheckBox>
        </cc1:PropertyPanel>
    </cc1:Pane>

    <cc1:Pane ID="Panel2" runat="server">
        <asp:Repeater ID="macroProperties" runat="server" OnItemDataBound="MacroPropertiesOnItemDataBound">
            <HeaderTemplate>
                <table class="table">
                    <thead>
                        <tr>
                            <th>
                                <%=umbraco.ui.Text("general", "alias",UmbracoUser)%>
                            </th>
                            <th>
                                <%=umbraco.ui.Text("general", "name",UmbracoUser)%>
                            </th>
                            <th>
                                <%=umbraco.ui.Text("general", "type",UmbracoUser)%>
                            </th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td>                        
                        <input type="hidden" id="macroPropertyID" runat="server" value='<%#Eval("Id")%>'
                            name="macroPropertyID" />
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="macroPropertyAlias" Display="Dynamic"  ForeColor="#b94a48">Required<br/></asp:RequiredFieldValidator>
                        <asp:TextBox runat="server" ID="macroPropertyAlias" Text='<%#Eval("Alias")%>' />
                    </td>
                    <td>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="macroPropertyName" Display="Dynamic" ForeColor="#b94a48">Required<br/></asp:RequiredFieldValidator>
                        <asp:TextBox runat="server" ID="macroPropertyName" Text='<%#Eval("Name")%>' />
                    </td>
                    <td>
                        
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="macroPropertyType" Display="Dynamic" ForeColor="#b94a48">Required<br/></asp:RequiredFieldValidator>
                        <asp:DropDownList OnPreRender="AddChooseList" runat="server" ID="macroPropertyType"
                            DataTextFormatString="" DataTextField='Name' DataValueField="Alias">
                        </asp:DropDownList>
                    </td>
                    <td>
                        <asp:Button OnClick="deleteMacroProperty" ID="delete" Text="Delete" runat="server" CssClass="btn btn-default delete-button" />
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                        <tr>
                            <td>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" EnableViewState="false" Enabled="false" EnableClientScript="false" runat="server" ControlToValidate="macroPropertyAliasNew" Display="Dynamic" ForeColor="#b94a48">Required<br/></asp:RequiredFieldValidator>
                                <asp:TextBox runat="server" ID="macroPropertyAliasNew" PlaceHolder='New Alias'  />
                            </td>
                            <td>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator4" EnableViewState="false" Enabled="false" EnableClientScript="false" runat="server" ControlToValidate="macroPropertyNameNew" Display="Dynamic" ForeColor="#b94a48">Required<br/></asp:RequiredFieldValidator>
                                <asp:TextBox runat="server" ID="macroPropertyNameNew" PlaceHolder='New Name' />
                            </td>
                            <td>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator5" EnableViewState="false" Enabled="false" EnableClientScript="false" runat="server" ControlToValidate="macroPropertyTypeNew" Display="Dynamic" ForeColor="#b94a48">Required<br/></asp:RequiredFieldValidator>
                                <asp:DropDownList OnPreRender="AddChooseList" runat="server" ID="macroPropertyTypeNew"
                                    DataTextField="Name" 
                                    DataValueField="Alias" 
                                    DataSource='<%# GetMacroParameterEditors()%>'>
                                </asp:DropDownList>
                            </td>
                            <td>
                                <asp:Button ID="createNew" Text="Add" runat="server" CssClass="btn btn-default add-button" OnClick="macroPropertyCreate" />
                            </td>
                        </tr>
                </tbody>
                </table>
            </FooterTemplate>
        </asp:Repeater>
    </cc1:Pane>

</asp:Content>
