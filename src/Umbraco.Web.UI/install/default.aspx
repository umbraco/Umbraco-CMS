<%@ Page Language="c#" CodeBehind="default.aspx.cs" AutoEventWireup="True" Inherits="umbraco.presentation.install._default" EnableViewState="False" %>
<%@ Register TagPrefix="umb" Namespace="ClientDependency.Core.Controls" Assembly="ClientDependency.Core" %>

<%@ Register Src="~/install/Title.ascx" TagPrefix="umb1" TagName="PageTitle" %>

<!DOCTYPE html>
<html>
<head runat="server">

	<meta charset="utf-8">

	<umb1:PageTitle runat="server" />

	<link media="all" rel="stylesheet" href="../umbraco_client/installer/css/jquery-ui-1.8.6.custom.css">
	<link media="all" type="text/css" rel="stylesheet" href="../umbraco_client/installer/css/reset.css" />
	<link media="all" rel="stylesheet" href="../umbraco_client/installer/css/all.css">
	<link media="all" type="text/css" rel="stylesheet" href="../umbraco_client/installer/css/form.css" />

	<script src="../umbraco_client/installer/js/jquery.1.4.4.js" type="text/javascript"></script>
	<script src="../umbraco_client/installer/js/jquery.ui.selectmenu.js" type="text/javascript"></script>
	<script src="../umbraco_client/installer/js/jquery.main.js" type="text/javascript"></script>

    <script src="../umbraco_client/passwordStrength/passwordstrength.js" type="text/javascript"></script>

	<!--[if lt IE 9]>
		<link media="all" rel="stylesheet" href="../umbraco_client/installer/css/lt7.css">
		<script src="http://html5shim.googlecode.com/svn/trunk/html5.js"></script>
	<![endif]-->

	<!--[if lt IE 7]><script type="text/javascript" src="../umbraco_client/installer/js/ie-png.js"></script><![endif]-->
</head>

<body class="<%= currentStepClass %>">

   
	<form runat="server">
	<asp:ScriptManager ID="ScriptManager1"  EnablePageMethods="true" runat="server"/>
    <!-- all page -->

	<section id="wrapper">

		<div class="wholder">

			<!-- header -->

			<header id="header">

				<div class="holder">

					<strong class="logo"><a href="#">Umbraco</a></strong>

				</div>

			</header>

			<!-- all content -->

			<section id="main">

				<!-- tabset -->

				<nav class="tabset">

					<asp:Repeater ID="rp_steps" runat="server" OnItemDataBound="bindStep">
					<HeaderTemplate><ul></HeaderTemplate>
					<FooterTemplate></ul></FooterTemplate>
					<ItemTemplate>
						<li class="<asp:literal runat='server' ID='lt_class' />"><asp:Literal ID="lt_name" runat="server" /><em>&nbsp;</em></li>
					</ItemTemplate>
					</asp:Repeater>
					
					<div class="b">&nbsp;</div>

				</nav>

				<!-- content -->

				<section class="content">
					<asp:PlaceHolder ID="PlaceHolderStep" runat="server"></asp:PlaceHolder>
				</section>
			</section>
		</div>
	</section>

	

	<!-- bg page -->
	<div class="bg-main">
		<div class="color2">
			
			<div class="bg-c"></div>
		</div>

		<div class="color3">
			
			<div class="bg-c"></div>
		</div>

		<div class="color1">
			
			<div class="bg-c"></div>
		</div>

		<div class="color4">
			
			<div class="bg-c"></div>
		</div>

		<div class="color5">
			
			<div class="bg-c"></div>
		</div>

	</div>
	

    <!-- lightbox -->
<div class="lightbox" id="lightbox">
	<a href="#" class="btn-close btn-close-box">close</a>
	<div class="t">&nbsp;</div>
	<div class="c">
		<div class="heading">
			<strong class="title">Name of skin</strong>
			<span class="create">Created by: <a href="#">Cogworks</a></span>
		</div>
		<div class="carusel">
			<ul>
				<li><img src="../umbraco_client/installer/images/img09.jpg" alt="image description"></li>
				<li><img src="../umbraco_client/installer/images/img10.jpg" alt="image description"></li>
				<li><img src="../umbraco_client/installer/images/img11.jpg" alt="image description"></li>
			</ul>
		</div>

		<footer class="btn-box">
			<a href="#single-tab4" class="single-tab btn-install btn-close-box">Install</a>
		</footer>
	</div>
	<div class="b">&nbsp;</div>
</div>

	<input type="hidden" runat="server" value="welcome" id="step">
	
	</form>
</body>

</html>

