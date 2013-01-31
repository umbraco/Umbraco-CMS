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
                $("#Table2 td.propertyContent select").change(function() {
                    //update the txt box
                    var txt = $(this).prev("input[type='text']");
                    txt.val($(this).val());
                    //clear other text boxes
                    $("#Table2 td.propertyContent input[type='text']").not(txt).val("");
                    //reset other drop downs
                    $("#Table2 td.propertyContent select").not($(this)).val("");
                });                
            });
        })(jQuery);
        
    </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="body" runat="server">
    <cc1:TabView ID="TabView1" runat="server" Width="552px" Height="392px"></cc1:TabView>
    <cc1:Pane ID="Pane1" runat="server">
        <table id="macroPane" class="macro-props" runat="server">
            <tr>
                <td class="propertyHeader">
                    Name
                </td>
                <td class="propertyContent">
                    <asp:TextBox ID="macroName" runat="server" CssClass="guiInputText"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="propertyHeader">
                    Alias
                </td>
                <td class="propertyContent">
                    <asp:TextBox ID="macroAlias" runat="server" CssClass="guiInputText"></asp:TextBox>
                </td>
            </tr>
        </table>
    </cc1:Pane>
    <cc1:Pane ID="Pane1_2" BackColor="mediumaquamarine" runat="server">
        <table id="Table2" class="macro-props" >
            <tr>
                <td class="propertyHeader">
                    <img alt="python Icon" src="../../images/umbraco/developerScript.gif" align="absMiddle" />
                    Use MVC Partial View
                </td>
                <td class="propertyContent">
                    <asp:TextBox ID="SelectedPartialView" runat="server" CssClass="guiInputText"></asp:TextBox>
                    <asp:DropDownList ID="PartialViewList" runat="server" >
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td class="propertyHeader">
                    <img alt="Xslt Icon" src="../../images/umbraco/developerXslt.gif" align="absMiddle"/>
                    or XSLT file
                </td>
                <td class="propertyContent">
                    <asp:TextBox ID="macroXslt" runat="server" CssClass="guiInputText"></asp:TextBox>
                    <asp:DropDownList ID="xsltFiles" runat="server">
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td class="propertyHeader">
                    <img alt="User control Icon" src="../../images/developer/userControlIcon.png" align="absMiddle"/>
                    or .NET User Control
                </td>
                <td class="propertyContent">
                    <asp:TextBox ID="macroUserControl" runat="server" CssClass="guiInputText"></asp:TextBox>
                    <asp:DropDownList ID="userControlList" runat="server">
                    </asp:DropDownList>
                    <asp:PlaceHolder ID="assemblyBrowserUserControl" runat="server"></asp:PlaceHolder>
                </td>
            </tr>
            <tr>
                <td class="propertyHeader">
                    <img alt="Custom Control Icon" src="../../images/developer/customControlIcon.png" align="absMiddle"/>
                    or .NET Custom Control
                </td>
                <td class="propertyContent">
                    <asp:TextBox ID="macroAssembly" runat="server" CssClass="guiInputText"></asp:TextBox>
                    (Assembly)<br />
                    <asp:TextBox ID="macroType" runat="server" CssClass="guiInputText"></asp:TextBox>
                    (Type)
                    <asp:PlaceHolder ID="assemblyBrowser" runat="server"></asp:PlaceHolder>
                </td>
            </tr>
            <tr>
                <td class="propertyHeader">
                    <img alt="python Icon" src="../../images/umbraco/developerScript.gif" align="absMiddle"/>
                    or script file
                </td>
                <td class="propertyContent">
                    <asp:TextBox ID="macroPython" runat="server" CssClass="guiInputText"></asp:TextBox>
                    <asp:DropDownList ID="pythonFiles" runat="server">
                    </asp:DropDownList>
                </td>
            </tr>
        </table>
    </cc1:Pane>
    <cc1:Pane ID="Pane1_3" runat="server">
        <table id="Table1"  class="macro-props" runat="server">
            <tr>
                <td class="propertyHeader">
                    Use in editor
                </td>
                <td class="propertyContent">
                    <asp:CheckBox ID="macroEditor" runat="server" Text="Yes"></asp:CheckBox>
                </td>
            </tr>
            <tr>
                <td class="propertyHeader">
                    Render content in editor
                </td>
                <td class="propertyContent">
                    <asp:CheckBox ID="macroRenderContent" runat="server" Text="Yes"></asp:CheckBox>
                </td>
            </tr>
        </table>
    </cc1:Pane>
    <cc1:Pane ID="Pane1_4" runat="server">
        <table id="Table3"  class="macro-props" runat="server">
            <tr>
                <td class="propertyHeader">
                    Cache Period
                </td>
                <td class="propertyContent">
                    <asp:TextBox ID="cachePeriod" runat="server" CssClass="guiInputText small"></asp:TextBox>&nbsp;Seconds
                </td>
            </tr>
            <tr>
                <td class="propertyHeader">
                    Cache By Page
                </td>
                <td class="propertyContent">
                    <asp:CheckBox ID="cacheByPage" runat="server" Text="Yes"></asp:CheckBox>
                </td>
            </tr>
            <tr>
                <td class="propertyHeader">
                    Cache Personalized
                </td>
                <td class="propertyContent">
                    <asp:CheckBox ID="cachePersonalized" runat="server" Text="Yes"></asp:CheckBox>
                </td>
            </tr>
        </table>
    </cc1:Pane>
    <cc1:Pane ID="Panel2" runat="server">
        <asp:Repeater ID="macroProperties" runat="server">
            <HeaderTemplate>
                <table class="macro-props params">
                    <tr>
                        <td class="propertyHeader">
                            <%=umbraco.ui.Text("general", "alias",this.getUser())%>
                        </td>
                        <td class="propertyHeader">
                            <%=umbraco.ui.Text("general", "name",this.getUser())%>
                        </td>
                        <td class="propertyHeader">
                            <%=umbraco.ui.Text("general", "type",this.getUser())%>
                        </td>
                        <td class="propertyHeader" />
                    </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td class="propertyContent">
                        <input type="hidden" id="macroPropertyID" runat="server" value='<%#DataBinder.Eval(Container.DataItem, "id")%>'
                            name="macroPropertyID" />
                        <asp:TextBox runat="server" ID="macroPropertyAlias" Text='<%#DataBinder.Eval(Container.DataItem, "Alias")%>' />
                    </td>
                    <td class="propertyContent">
                        <asp:TextBox runat="server" ID="macroPropertyName" Text='<%#DataBinder.Eval(Container.DataItem, "Name")%>' />
                    </td>
                    <td class="propertyContent">
                        <asp:DropDownList OnPreRender="AddChooseList" runat="server" ID="macroPropertyType"
                            DataTextFormatString="" DataTextField='macroPropertyTypeAlias' DataValueField="id"
                            DataSource='<%# GetMacroPropertyTypes()%>' SelectedValue='<%# ((umbraco.cms.businesslogic.macro.MacroPropertyType) DataBinder.Eval(Container.DataItem,"Type")).Id %>'>
                        </asp:DropDownList>
                    </td>
                    <td class="propertyContent">
                        <asp:Button OnClick="deleteMacroProperty" ID="delete" Text="Delete" runat="server"
                            CssClass="guiInputButton" />
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                <tr>
                    <td class="propertyContent">
                        <asp:TextBox runat="server" ID="macroPropertyAliasNew" Text='New Alias' OnTextChanged="macroPropertyCreate" />
                    </td>
                    <td class="propertyContent">
                        <asp:TextBox runat="server" ID="macroPropertyNameNew" Text='New Name' />
                    </td>
                    <td class="propertyContent">
                        <asp:DropDownList OnPreRender="AddChooseList" runat="server" ID="macroPropertyTypeNew"
                            DataTextField="macroPropertyTypeAlias" DataValueField="id" DataSource='<%# GetMacroPropertyTypes()%>'>
                        </asp:DropDownList>
                    </td>
                    <td class="propertyContent">
                        <asp:Button ID="createNew" Text="Add" runat="server" CssClass="guiInputButton" />
                    </td>
                </tr>
                </table>
            </FooterTemplate>
        </asp:Repeater>
    </cc1:Pane>
    <script type="text/javascript">
        jQuery(document).ready(function () {
            UmbClientMgr.appActions().bindSaveShortCut();
        });
    </script>
</asp:Content>
