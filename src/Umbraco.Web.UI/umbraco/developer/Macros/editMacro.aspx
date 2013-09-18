<%@ Page Language="c#" MasterPageFile="../../masterpages/umbracoPage.Master" Title="Edit macro"
    CodeBehind="EditMacro.aspx.cs" AutoEventWireup="True" Inherits="Umbraco.Web.UI.Umbraco.Developer.Macros.EditMacro" %>

<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="CD" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
    
    <CD:CssInclude ID="CssInclude1" runat="server" FilePath="Editors/EditMacro.css" PathNameAlias="UmbracoClient" />

    <script type="text/javascript">
        function doSubmit() {
            document.forms.aspnetForm.submit();
        }

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

        <cc1:PropertyPanel runat="server" Text="Use in rich text editor">
            <asp:CheckBox ID="macroEditor" runat="server" Text="Yes"></asp:CheckBox>
        </cc1:PropertyPanel>
        
        <cc1:PropertyPanel runat="server" Text="Render in rich text editor">
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
        <asp:Repeater ID="macroProperties" runat="server">
            <HeaderTemplate>
                <table class="table">
                    <thead>
                        <tr>
                            <th>
                                <%=umbraco.ui.Text("general", "alias",this.getUser())%>
                            </th>
                            <th>
                                <%=umbraco.ui.Text("general", "name",this.getUser())%>
                            </th>
                            <th>
                                <%=umbraco.ui.Text("general", "type",this.getUser())%>
                            </th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td>
                        <input type="hidden" id="macroPropertyID" runat="server" value='<%#DataBinder.Eval(Container.DataItem, "id")%>'
                            name="macroPropertyID" />
                        <asp:TextBox runat="server" ID="macroPropertyAlias" Text='<%#DataBinder.Eval(Container.DataItem, "Alias")%>' />
                    </td>
                    <td>
                        <asp:TextBox runat="server" ID="macroPropertyName" Text='<%#DataBinder.Eval(Container.DataItem, "Name")%>' />
                    </td>
                    <td>
                        <asp:DropDownList OnPreRender="AddChooseList" runat="server" ID="macroPropertyType"
                            DataTextFormatString="" DataTextField='macroPropertyTypeAlias' DataValueField="id"
                            DataSource='<%# GetMacroPropertyTypes()%>' SelectedValue='<%# ((umbraco.cms.businesslogic.macro.MacroPropertyType) DataBinder.Eval(Container.DataItem,"Type")).Id %>'>
                        </asp:DropDownList>
                    </td>
                    <td>
                        <asp:Button OnClick="deleteMacroProperty" ID="delete" Text="Delete" runat="server" CssClass="btn btn-default" />
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                        <tr>
                            <td>
                                <asp:TextBox runat="server" ID="macroPropertyAliasNew" Text='New Alias' OnTextChanged="macroPropertyCreate" />
                            </td>
                            <td>
                                <asp:TextBox runat="server" ID="macroPropertyNameNew" Text='New Name' />
                            </td>
                            <td>
                                <asp:DropDownList OnPreRender="AddChooseList" runat="server" ID="macroPropertyTypeNew"
                                    DataTextField="macroPropertyTypeAlias" DataValueField="id" DataSource='<%# GetMacroPropertyTypes()%>'>
                                </asp:DropDownList>
                            </td>
                            <td>
                                <asp:Button ID="createNew" Text="Add" runat="server" CssClass="btn btn-default" />
                            </td>
                        </tr>
                </tbody>
                </table>
            </FooterTemplate>
        </asp:Repeater>
    </cc1:Pane>

    <asp:PlaceHolder runat="server">
        <script type="text/javascript">
            jQuery(document).ready(function () {
                UmbClientMgr.appActions().bindSaveShortCut();

                (function ($) {
                    // U4-667: Make the "Render content in editor" checkbox dependent on the "Use in editor checkbox"
                    var useInEditorCheckBox = $("#<%= macroEditor.ClientID %>");
                    var renderInEditorCheckBox = $("#<%= macroRenderContent.ClientID %>");

                    toggle();

                    useInEditorCheckBox.on("change", function() {
                        toggle();
                    });
                    
                    function toggle() {
                        var disabled = useInEditorCheckBox.is(":checked") == false;
                        renderInEditorCheckBox.prop("disabled", disabled);
                    }
                })(jQuery);
            });
        </script>
    </asp:PlaceHolder>
</asp:Content>
