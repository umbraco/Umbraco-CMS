<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BlogInstaller.ascx.cs" Inherits="Runway.Blog.usercontrols.BlogInstaller" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<asp:Panel runat="server" ID="done" Visible="false">
<p>
    <strong>Installation complete!</strong> you can now go to the content section and start blogging directly from the dashboard.
</p>

<p>
    Or you can view your blog <asp:HyperLink Target="_blank" Text="here" runat="server" ID="blogLink" />
</p>
</asp:Panel>

<asp:Panel runat="server" ID="install">

<umb:PropertyPanel runat="server">
    <p>
        <strong>Runway Blog has been installed</strong>
    </p>
    <ul>
        <li>Document types and templates have been added</li>
        <li>Macros and xslt-files have been setup</li>
        <li>A test blog has been created</li>
    </ul>
    <p>
        All you need to do now is give it a name and a description, and you can start blogging right away.
    </p>
</umb:PropertyPanel>

<umb:PropertyPanel runat="server" Text="Blog name">
    <asp:TextBox ID="tb_name" runat="server" CssClass="guiInputText" style="width: 230px" /> <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ControlToValidate="tb_name" runat="server" ErrorMessage="*"/>      
</umb:PropertyPanel>

<umb:PropertyPanel  runat="server" Text="Blog description">
    <asp:TextBox ID="tb_description" TextMode="MultiLine" CssClass="guiInputText" style="width: 230px" runat="server" /> <asp:RequiredFieldValidator ControlToValidate="tb_description" runat="server" ErrorMessage="*"/>    
</umb:PropertyPanel>

<umb:PropertyPanel runat="server" Text=" ">
    <asp:Button ID="bt_create" runat="server" OnClick="saveAndPublish" Text="Save" />
</umb:PropertyPanel>

</asp:Panel>