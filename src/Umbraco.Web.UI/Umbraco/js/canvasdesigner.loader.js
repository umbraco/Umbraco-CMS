LazyLoad.js([
      '/Umbraco/lib/jquery/jquery-2.0.3.min.js',
      '/Umbraco/lib/jquery/jquery-ui-1.10.4.custom.min.js',
      '/Umbraco/lib/angular/1.1.5/angular.min.js',
      '/Umbraco/lib/underscore/underscore.js',
      '/Umbraco/js/app.js',
      '/Umbraco/js/umbraco.resources.js',
      '/Umbraco/js/umbraco.services.js',
      '/Umbraco/js/umbraco.security.js',
      '/Umbraco/ServerVariables',
      '/Umbraco/lib/spectrum/spectrum.js',
      'http://ajax.googleapis.com/ajax/libs/webfont/1/webfont.js',
      '/umbraco/js/canvasdesigner.panel.js',
], function () {
    jQuery(document).ready(function () {
        angular.bootstrap(document, ['Umbraco.canvasdesigner']);
    });
}
);
