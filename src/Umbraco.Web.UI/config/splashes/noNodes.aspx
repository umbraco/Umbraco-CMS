<%@ Page Language="C#" AutoEventWireup="True" Inherits="Umbraco.Web.UI.Config.Splashes.NoNodes" CodeBehind="NoNodes.aspx.cs" %>
<%@ Import Namespace="Umbraco.Core.Configuration" %>
<%@ Import Namespace="Umbraco.Core.IO" %>

<!DOCTYPE html>
<html>
<head runat="server">

    <meta charset="utf-8">

    <title>Umbraco - no pages found</title>
    <link rel="icon" type="image/png" href="<%=umbraco.GlobalSettings.Path + "/Images/PinnedIcons/umb.ico" %>" />

    <link media="all" rel="stylesheet" href="../../umbraco_client/installer/css/jquery-ui-1.8.6.custom.css" />

    <link media="all" type="text/css" rel="stylesheet" href="../../umbraco_client/installer/css/reset.css" />

    <link media="all" rel="stylesheet" href="../../umbraco_client/installer/css/all.css" />

    <link media="all" type="text/css" rel="stylesheet" href="../../umbraco_client/installer/css/form.css" />

    <script src="../../umbraco_client/installer/js/jquery.1.4.4.js" type="text/javascript"></script>
    <script src="../../umbraco_client/installer/js/jquery.ui.selectmenu.js" type="text/javascript"></script>
    <script src="../../umbraco_client/installer/js/jquery.main.js" type="text/javascript"></script>

    <script src="../../umbraco_client/passwordStrength/passwordstrength.js" type="text/javascript"></script>

    <!--[if lt IE 9]>
		<link media="all" rel="stylesheet" href="../../umbraco_client/installer/css/lt7.css" />
		<script src="//html5shim.googlecode.com/svn/trunk/html5.js"></script>
	<![endif]-->

    <!--[if lt IE 7]><script type="text/javascript" src="../../umbraco_client/installer/js/ie-png.js"></script><![endif]-->
    
</head>
<body class="theend">
    <form id="Form1" method="post" runat="server">

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
                    
                    <!-- content -->

                    <section class="content">

                        <script type="text/javascript">
                            jQuery(document).ready(function () {
        $.post("../../install/InstallerRestService.aspx?feed=sitebuildervids",
                                    function(data) {
                                        jQuery("#ajax-sitebuildervids").html(data);
                                    });

        $.post("../../install/InstallerRestService.aspx?feed=developervids",
                                    function(data) {
                                        jQuery("#ajax-developervids").html(data);
                                    });
                            });

                        </script>

                        <!-- done box -->

                        <div class="tab main-tabinfo">
                            <div class="container">
                                <h1>Looks like there's still work to do</h1>
                                <p>
                                    You're seeing the wonderful page because your website doesn't contain any <strong>published</strong> content yet.
                                </p>
                                <p>
                                    So get rid of this page by starting umbraco and publishing some content. You can do this by clicking the "set up your new website" button below.
                                </p>
                                <ul class="btn-web">
                                    <li class="btn-set"><a href="<%= IOHelper.ResolveUrl(SystemDirectories.Umbraco) %>"><span>Launch umbraco</span></a></li>
                                </ul>
                            </div>
                            <div class="threcol">
                                <div class="t">
                                    &nbsp;
                                </div>
                                <div class="hold">
                                    <aside class="col1">
                                        <h2>Useful links</h2>
                                        <p>We’ve put together some useful links to help you get started with Umbraco.</p>
                                        <nav class="links">
                                            <ul>
                                                <li><a href="http://our.umbraco.org?ref=ourFromInstaller">our.umbraco.org</a></li>
                                            </ul>

                                            <ul>
                                                <li><a href="http://our.umbraco.org/wiki?ref=LatestDocsFromInstaller">New documentation</a></li>
                                                <li><a href="http://our.umbraco.org/projects?ref=LatestProjectsFromInstaller">New Projects</a></li>
                                                <li><a href="http://our.umbraco.org/forum?ref=LatesTalkFromInstaller">Forum Talk</a></li>
                                            </ul>
                                        </nav>
                                    </aside>
                                    <aside class="col2">
                                        <h2>Sitebuilder introduction</h2>
                                        <div id="ajax-sitebuildervids"><small>Loading...</small></div>
                                    </aside>
                                    <aside class="col3">
                                        <h2>Developer introduction</h2>
                                        <div id="ajax-developervids"><small>Loading...</small></div>

                                    </aside>
                                </div>
                            </div>
                        </div>
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
        
    </form>
</body>
</html>
