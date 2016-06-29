
LazyLoad.js([
      '../lib/jquery/jquery.min.js',
      //'../lib/jquery-ui/jquery-ui.min.js',
      '/Scripts/jquery-1.6.4.min.js',
      '../lib/angular/1.1.5/angular.min.js',
      '../lib/underscore/underscore-min.js',
      '../lib/umbraco/Extensions.js',
      '../js/app.js',
      '../js/umbraco.resources.js',
      '../js/umbraco.services.js',
      '../js/umbraco.security.js',
      '../ServerVariables',
      '../lib/spectrum/spectrum.js',
      '/Scripts/jquery.signalR-2.2.0.min.js',
      '/signalr/hubs',

      '../js/canvasdesigner.panel.js',
], function () {
    jQuery(document).ready(function () {
        angular.bootstrap(document, ['Umbraco.canvasdesigner']);
    });
});
