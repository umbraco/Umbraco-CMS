LazyLoad.js([
    'lib/jquery/jquery.min.js',

    'lib/angular/angular.js',
    'lib/angular-cookies/angular-cookies.js',
    'lib/angular-touch/angular-touch.js',
    'lib/angular-sanitize/angular-sanitize.js',
    'lib/angular-messages/angular-messages.js',
    'lib/angular-aria/angular-aria.min.js',
    'lib/underscore/underscore-min.js',
    'lib/angular-ui-sortable/sortable.js',

    'js/utilities.js',

    'js/installer.app.js',
    'js/umbraco.directives.js',
    'js/umbraco.installer.js'

], function () {
    jQuery(document).ready(function () {
        angular.bootstrap(document, ['umbraco']);
    });
}
);
