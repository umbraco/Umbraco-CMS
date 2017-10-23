<%@ Page Language="C#" AutoEventWireup="true"%>
<%@ Import Namespace="Umbraco.Core.IO" %>
<%@ Import Namespace="Umbraco.Deploy.UI" %>
<%@ Import Namespace="Umbraco.Web" %>
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

 <link rel="stylesheet" href="<%=IOHelper.ResolveUrl(SystemDirectories.Umbraco)%>/assets/css/nonodes.style.min.css" />
 <link rel="stylesheet" href="<%=IOHelper.ResolveUrl(SystemDirectories.AppPlugins)%>/deploy/assets/css/deploy.css" />

 <!--[if lt IE 9]>
  <script src="//cdnjs.cloudflare.com/ajax/libs/modernizr/2.7.1/modernizr.min.js"></script>
 <![endif]-->

</head>
<body>

<% if(HttpContext.Current.Request.IsLocal == false){  %>
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

<% }else{ %>

<section ng-controller="Umbraco.NoNodes.Controller as vm">
    <article class="ud-nonodes" ng-cloak>

        <div>
            <div class="logo"></div>

            <div>

                <div ng-if="vm.restore.status === ''">

                    <h1>Initializing your website...</h1>

                    <div ng-if="vm.isDebug && vm.requiresInitialization">
                        <small>Press the button below to get started</small>
                        <div class="cta">
                            <button class="button" ng-click="vm.restoreSchema()">Go!</button>
                        </div>
                    </div>
                </div>

                <div ng-if="vm.restore.status === 'ready'">
                    <h1>Restore from Umbraco Cloud</h1>
                    <div class="cta">
                        <button class="button" ng-click="vm.restoreData()">Restore</button>
                        <small><span>or</span> <a ng-href="<%= IOHelper.ResolveUrl(SystemDirectories.Umbraco) %>">Skip restore and open Umbraco</a></small>
                    </div>
                </div>

                <div ng-if="vm.restore.status === 'inProgress'">
                    <h1>{{ vm.restore.restoreMessage }}</h1>
                    <p>{{ vm.restore.restoreProgress }}% restored</p>
                    <small>{{ vm.restore.currentActivity }}</small>
                    <div class="timestamp">{{ vm.restore.timestamp }}</div>
                </div>

                <div ng-if="vm.restore.status === 'completed'">
                    <h1>Ready to rock n' roll!</h1>
                    <p>Everything has been restored and is ready for use, click below to open Umbraco</p>
                    <div class="cta">
                        <a href="<%= IOHelper.ResolveUrl(SystemDirectories.Umbraco) %>" class="button">Open Umbraco</a>
                    </div>
                </div>

                <ud-error
                    ng-if="vm.restore.error.hasError"
                    comment="vm.restore.error.comment"
                    log="vm.restore.error.log"
                    exception="vm.restore.error.exception"
                    status="vm.restore.status"
                    class="ud-restore-error"
                    operation="restore"
                    no-nodes="true"
                    on-debug="vm.showDebug()">
                </ud-error>

                <div class="umb-deploy-debug" ng-if="vm.restore.showDebug">
                    <div class="umb-deploy-debug-console" ng-bind-html-unsafe="vm.restore.trace"></div>
                </div>

                <%--<div ng-if="vm.restore.error.hasError" class="json">
                    <h1 style="margin-top: 0;">An error occurred: </h1>
                    <h2 ng-if="vm.restore.error.exceptionMessage">{{ vm.restore.error.exceptionMessage }}</h2>
                    <a href="#" ng-click="vm.showLog()" ng-hide="vm.logIsvisible">Show details</a>
                    <pre ng-if="vm.logIsvisible === true">{{ vm.restore.error.log }}</pre>
                </div>--%>

            </div>

        </div>
    </article>

</section>

<%= NoNodesHelper.ServerVariables(HttpContext.Current.Request.RequestContext, UmbracoContext.Current) %>
<script type="text/javascript" src="<%= IOHelper.ResolveUrl(SystemDirectories.Umbraco) %>/lib/jquery/jquery.min.js"></script>
<script type="text/javascript" src="<%= IOHelper.ResolveUrl(SystemDirectories.Umbraco) %>/lib/moment/moment-with-locales.js"></script>
<script type="text/javascript" src="<%= IOHelper.ResolveUrl(SystemDirectories.Umbraco) %>/lib/angular/1.1.5/angular.min.js"></script>
<script type="text/javascript" src="<%= IOHelper.ResolveUrl(SystemDirectories.AppPlugins) %>/deploy/lib/signalr/jquery.signalR.min.js"></script>
<script type="text/javascript" src="<%= IOHelper.ResolveUrl(SystemDirectories.Umbraco) %>/backoffice/signalr/hubs"></script>
<script type="text/javascript" src="<%= IOHelper.ResolveUrl(SystemDirectories.AppPlugins) %>/deploy/js/nonodes.modules.js"></script>
<script type="text/javascript" src="<%= IOHelper.ResolveUrl(SystemDirectories.AppPlugins) %>/deploy/js/deploy.services.js"></script>
<script type="text/javascript" src="<%= IOHelper.ResolveUrl(SystemDirectories.AppPlugins) %>/deploy/js/deploy.components.js"></script>
<script type="text/javascript" src="<%= IOHelper.ResolveUrl(SystemDirectories.AppPlugins) %>/deploy/js/nonodes.bootstrap.js"></script>
<script type="text/javascript">
    angular.bootstrap(document, ['umbraco.nonodes']);
</script>
<% } %>
</body>
</html>