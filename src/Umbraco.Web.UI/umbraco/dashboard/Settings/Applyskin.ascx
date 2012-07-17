<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Applyskin.ascx.cs" Inherits="umbraco.presentation.umbraco.dashboard.Settings.Applyskin" %>


<asp:DropDownList ID="skinpicker" runat="server" /> 

<asp:Button ID="bt_apply" runat="server" Text="Apply" OnClick="apply" />


<asp:Button ID="bt_rollback" runat="server" Text="Rollback skin" OnClick="rollback" />