LazyLoad.js([
    'lib/jquery/jquery.min.js',
    /* 1.1.5 */
    'lib/angular/1.1.5/angular.min.js',
    'lib/angular/1.1.5/angular-cookies.min.js',
    'lib/angular/1.1.5/angular-mobile.min.js',
    'lib/angular/1.1.5/angular-mocks.js',
    'lib/angular/1.1.5/angular-sanitize.min.js',
    'lib/underscore/underscore-min.js',
    'lib/angular/angular-ui-sortable.js',
    'js/installer.app.js',
    'js/umbraco.directives.js',
    'js/umbraco.installer.js'
], function () {
    jQuery(document).ready(function () {
        angular.bootstrap(document, ['umbraco']);
    });
}
);