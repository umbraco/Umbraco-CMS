<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ClientDependencyTest.aspx.cs" Inherits="umbraco.presentation.umbraco.ClientDependencyTest" Trace="true" %>
<%@ Register TagPrefix="umb" Namespace="umbraco.presentation.ClientDependency" Assembly="umbraco.presentation.ClientDependency" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>   
</head>
<body>
	<umb:ClientDependencyLoader runat="server" id="ClientLoader" EmbedType="ClientSideRegistration" IsDebugMode="true" >
		<Paths>
			<umb:ClientDependencyPath Name="UmbracoClient" Path="~/umbraco_client" />
			<umb:ClientDependencyPath Name="UmbracoRoot" Path='<%#umbraco.GlobalSettings.Path%>' />
		</Paths>		
	</umb:ClientDependencyLoader>

	<umb:ClientDependencyInclude runat="server" id="Dependency1" Type="Css" File='<%#umbraco.GlobalSettings.Path + "/css/permissionsEditor.css"%>' />

    <form id="form1" runat="server">
    <div>
    <asp:ScriptManager runat="server"></asp:ScriptManager>
    </div>
    </form>
</body>
</html>
