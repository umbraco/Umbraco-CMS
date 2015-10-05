/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTabView
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
