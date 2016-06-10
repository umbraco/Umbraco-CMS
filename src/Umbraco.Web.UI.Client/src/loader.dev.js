LazyLoad.js(
    [
        'lib/jquery/jquery.min.js',
        'lib/jquery-ui/jquery-ui.min.js',

        
        /* 1.1.5 */
        'lib/angular/1.1.5/angular.min.js',
        'lib/angular/1.1.5/angular-cookies.min.js',

        'lib/angular/1.1.5/angular-sanitize.min.js',

        'lib/angular/angular-ui-sortable.js',

        'lib/angular/1.1.5/angular-mocks.js',
        'lib/angular/1.1.5/angular-mobile.min.js',
        'lib/underscore/underscore-min.js',

        'lib/angular-dynamic-locale/tmhDynamicLocale.min.js',
        
        'lib/bootstrap/js/bootstrap.2.3.2.min.js',
        'lib/bootstrap-tabdrop/bootstrap-tabdrop.min.js',
        'lib/umbraco/Extensions.js',

        'lib/umbraco/NamespaceManager.js',
        'lib/umbraco/LegacyUmbClientMgr.js',
        'lib/umbraco/LegacySpeechBubble.js',

        'js/umbraco.servervariables.js',
        'js/app.dev.js',
        'js/umbraco.httpbackend.js',
        'js/umbraco.testing.js',

        'js/umbraco.directives.js',
        'js/umbraco.filters.js',
        'js/umbraco.resources.js',
        'js/umbraco.services.js',
        'js/umbraco.security.js',
        'js/umbraco.controllers.js',
        'js/routes.js',
        'js/init.js'
  ],

  function () {
    jQuery(document).ready(function () {
        angular.bootstrap(document, ['umbraco']);
    });
  }
);