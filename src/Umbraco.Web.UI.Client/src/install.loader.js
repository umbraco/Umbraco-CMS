LazyLoad.js([
  'lib/jquery/jquery.min.js',

  'lib/angular/angular.min.js',
  'lib/angular-cookies/angular-cookies.min.js',
  'lib/angular-touch/angular-touch.min.js',
  'lib/angular-sanitize/angular-sanitize.min.js',
  'lib/angular-messages/angular-messages.min.js',
  'lib/angular-aria/angular-aria.min.js',
  'lib/underscore/underscore-min.js',
  'lib/angular-ui-sortable/sortable.min.js',
  'lib/nouislider/nouislider.min.js',

  'js/utilities.min.js',

  'js/installer.app.min.js',
  'js/umbraco.directives.min.js',
  'js/umbraco.installer.min.js'

], function () {
  jQuery(document).ready(function () {
    angular.bootstrap(document, ['umbraco']);
  });
}
);
