<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProgressBar.ascx.cs" Inherits="umbraco.presentation.umbraco.controls.ProgressBar" %>
<img src="<%#umbraco.GlobalSettings.ClientPath%>/images/progressBar.gif" runat="server"
	id="ImgBar" alt="<%#umbraco.ui.Text("publish", "inProgress", null)%>" /><br />