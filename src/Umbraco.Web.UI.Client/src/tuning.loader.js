LazyLoad.js([
      '/Umbraco/lib/jquery/jquery-2.0.3.min.js',
      '/Umbraco/lib/jquery/jquery-ui-1.10.4.custom.min.js',
      '/Umbraco/lib/angular/1.1.5/angular.min.js',
      '/Umbraco/lib/angular-bootstrap/ui-bootstrap-tpls-0.10.0.min.js',
      '/Umbraco/lib/spectrum/spectrum.js',
      'http://ajax.googleapis.com/ajax/libs/webfont/1/webfont.js',
      '/Umbraco/js/tuning.config.js',
      '/Umbraco/js/tuning.palettes.js',
      '/umbraco/js/tuning.panel.js'
], function () {
    jQuery(document).ready(function () {
        angular.bootstrap(document, ['umbraco.tuning']);
    });
}
);