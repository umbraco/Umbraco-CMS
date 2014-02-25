<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="Renaming.ascx.cs" Inherits="Umbraco.Web.UI.Install.Steps.Renaming" %>
<h1>Step 3/5: Updating old conventions</h1>
<asp:Panel ID="init" Runat="server" Visible="True">
<p>
    This version of Umbraco introduces new conventions for naming XSLT and REST extensions as well as a renaming of the Public Access storage file.<br />
    You no longer need to prefix your extension references with /bin and the public access storage file is now called access.config instead of access.xml.<br />
    This step of the installer will try to update your old references. If it fails due to permission settings, you'll need to make these changes manually!
</p>

<asp:Panel ID="noChangedNeeded" runat="server" Visible="false">
<p><strong>Everything looks good. No changes needed for the upgrade, just press next.</strong></p>
</asp:Panel>
<asp:Panel ID="changesNeeded" runat="server" Visible="true">
<p>
<strong>The following changes will need to be made. Press to update changes button to proceed:</strong>
</p>
<asp:Literal id="identifyResult" Runat="server"></asp:Literal>
<asp:Button ID="updateChanges" runat="server" Text="Update Changes" 
        onclick="UpdateChangesClick" />
</asp:Panel>
</asp:Panel>

<asp:Panel ID="result" Runat="server" Visible="False">
<asp:Literal ID="resultText" runat=server></asp:Literal>
</asp:Panel>