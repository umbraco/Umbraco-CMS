
LazyLoad.js([
    '../lib/jquery/jquery.min.js',
    '../lib/angular/angular.js',
    '../lib/underscore/underscore-min.js',
    '../lib/umbraco/Extensions.js',
    '../js/app.js',
    '../js/umbraco.resources.js',
    '../js/umbraco.services.js',
    '../js/umbraco.interceptors.js',
    '../ServerVariables',
    '../lib/signalr/jquery.signalR.js',
    '../BackOffice/signalr/hubs',
    '../js/umbraco.canvasdesigner.js'
], function () {
    jQuery(document).ready(function () {
        angular.bootstrap(document, ['Umbraco.canvasdesigner']);
    });
});
