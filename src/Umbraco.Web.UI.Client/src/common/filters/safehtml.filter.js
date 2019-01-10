angular.module('umbraco.filters')
    .filter('safe_html', ['$sce', function($sce){
        return function(text) {
            return $sce.trustAsHtml(text);
        };
    }]);