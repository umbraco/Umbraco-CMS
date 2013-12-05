yepnope({

  load: [
    'lib/jquery/jquery-2.0.3.min.js',

    /* the jquery ui elements we need */
    /* NOTE: I've opted not to use the full lib, just the parts we need to save on DL*/
    'lib/jquery/jquery.ui.core.min.js',
    'lib/jquery/jquery.ui.widget.min.js',

    'lib/jquery/jquery.ui.mouse.min.js',       
    'lib/jquery/jquery.ui.sortable.min.js',

    /* 1.1.5 */
    'lib/angular/1.1.5/angular.min.js',
    'lib/angular/1.1.5/angular-cookies.min.js',
    'lib/angular/1.1.5/angular-mobile.min.js',
    'lib/angular/1.1.5/angular-mocks.js',
    'lib/angular/1.1.5/angular-sanitize.min.js',

    'lib/angular/angular-ui-sortable.js',

    /* App-wide file-upload helper */
    'lib/jquery/jquery.upload/js/jquery.fileupload.js',
    'lib/jquery/jquery.upload/js/jquery.fileupload-process.js',
    'lib/jquery/jquery.upload/js/jquery.fileupload-angular.js',
    
    'lib/bootstrap/js/bootstrap.2.3.2.min.js',
    'lib/underscore/underscore.js',
    'lib/umbraco/Extensions.js',
    'lib/umbraco/NamespaceManager.js',

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

  complete: function () {
    jQuery(document).ready(function () {
        angular.bootstrap(document, ['umbraco']);
      });
  }
});