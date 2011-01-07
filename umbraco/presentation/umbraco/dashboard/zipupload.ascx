<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="zipupload.ascx.cs" Inherits="Nibble.Umb.ZipUpload.zipupload" %>
<div class="dashboardWrapper">
    <h2>
        Zip Upload</h2>
    <img src="/umbraco/dashboard/images/zipfile.png" alt="Umbraco" class="dashboardIcon" />
    <p>
        Quickly upload an archive of media assets to Umbraco.</p>
    <div class="dashboardColWrapper">
        <div class="dashboardCols">
            <div class="dashboardCol">
                <asp:Panel ID="Panel1" runat="server">
                </asp:Panel>
                <asp:Panel ID="Panel2" runat="server">
                    <asp:Literal ID="Literal1" runat="server"></asp:Literal>
                </asp:Panel>
            </div>
        </div>
    </div>
</div>
