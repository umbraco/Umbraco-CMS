/**
* @ngdoc directive
* @name umbraco.directives.directive:umbTab 
* @restrict E
**/
angular.module("umbraco.directives")
.directive('umbTab', function(){
	return {
		restrict: 'E',
		replace: true,
		transclude: 'true',
		templateUrl: 'views/directives/umb-tab.html'
	};
});