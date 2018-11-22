/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTabView
* @deprecated
* We plan to remove this directive in the next major version of umbraco (8.0). The directive is not recommended to use.
*
* @restrict E
**/

angular.module("umbraco.directives")
.directive('umbTabView', function($timeout, $log){
	return {
		restrict: 'E',
		replace: true,
		transclude: 'true',
		templateUrl: 'views/directives/_obsolete/umb-tab-view.html'
	};
});
