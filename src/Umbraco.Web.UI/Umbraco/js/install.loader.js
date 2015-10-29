try {
    LazyLoad.js([
      'lib/jquery/jquery.min.js',
      /* 1.1.5 */
      'lib/angular/1.1.5/angular.min.js',
      'lib/angular/1.1.5/angular-cookies.min.js',
      'lib/angular/1.1.5/angular-mobile.min.js',
      'lib/angular/1.1.5/angular-mocks.js',
      'lib/angular/1.1.5/angular-sanitize.min.js',
      'lib/underscore/underscore-min.js',
      'js/umbraco.installer.js',
      'js/umbraco.directives.js'
    ], function () {
        jQuery(document).ready(function () {
            angular.bootstrap(document, ['ngSanitize', 'umbraco.install', 'umbraco.directives.validation']);
        });
    });
}
catch (err) {
    if (err.message == 'LazyLoad is not defined') {
        document.getElementById("feedback").style.display = "none";
        document.getElementById("missinglazyload").style.display = "block";
    }
}