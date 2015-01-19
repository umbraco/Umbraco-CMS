<%@ Page Language="C#" AutoEventWireup="True" Inherits="Umbraco.Web.UI.Config.Splashes.NoNodes" CodeBehind="NoNodes.aspx.cs" %>
<%@ Import Namespace="Umbraco.Core.Configuration" %>
<%@ Import Namespace="Umbraco.Core.IO" %>

<!doctype html>
<!--[if lt IE 7]> <html class="no-js ie6 oldie" lang="en"> <![endif]-->
<!--[if IE 7]>    <html class="no-js ie7 oldie" lang="en"> <![endif]-->
<!--[if IE 8]>    <html class="no-js ie8 oldie" lang="en"> <![endif]-->
<!--[if gt IE 8]><!--> <html class="no-js" lang="en"> <!--<![endif]-->
<head>
 <meta charset="utf-8">
 <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1">
 <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
 	
 <title></title>
 <meta name="description" content="">
 <meta name="author" content="">

 <link href='//fonts.googleapis.com/css?family=Open+Sans:300,400,700,600' rel='stylesheet' type='text/css'>
 <link href='//fonts.googleapis.com/css?family=Asap:400,700,400italic,700italic' rel='stylesheet' type='text/css'>

 <link rel="stylesheet" href="../../Umbraco/assets/css/nonodes.style.min.css" />

 <!--[if lt IE 9]>
  <script src="//cdnjs.cloudflare.com/ajax/libs/modernizr/2.7.1/modernizr.min.js"></script>
 <![endif]-->

</head>
<body>

<section>
	<article>
		<div>
			<div class="logo"></div>

			<h1>Welcome to your Umbraco installation</h1>
			<h3>You're seeing the wonderful page because your website doesn't contain any published content yet.</h3>

			<div class="cta">
				<a href="<%= IOHelper.ResolveUrl(SystemDirectories.Umbraco) %>" class="button">Open Umbraco</a>
			</div>


			<div class="row">
				<div class="col">
					<h2>Easy start with Umbraco.tv</h2>
					<p>We have created a bunch of 'how-to' videos, to get you easily started with Umbraco. Learn how to build projects in just a couple of minutes. Easiest CMS in the world.</p>
					
					<a href="http://umbraco.tv?ref=tvFromInstaller" target="_blank">Umbraco.tv &rarr;</a>
				</div>

				<div class="col">
					<h2>Be a part of the community</h2>
					<p>The Umbraco community is the best of its kind, be sure to visit, and if you have any questions, we’re sure that you can get your answers from the community.</p>
					
					<a href="http://our.umbraco.org?ref=ourFromInstaller" target="_blank">our.Umbraco &rarr;</a>
				</div>
			</div>

		</div>
	</article>

</section>

<script src="//code.jquery.com/jquery-latest.min.js"></script>

</body>
</html>