<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="customizeSkin.ascx.cs" Inherits="umbraco.presentation.install.steps.Skinning.customizeSkin" %>

<p>
  Skin installed,click <a href="<%= umbraco.GlobalSettings.Path %>/canvas.aspx?redir=<%= HttpRuntime.AppDomainAppVirtualPath %>&skinning=true" target="_blank">here</a> to customize the skin.

</p>