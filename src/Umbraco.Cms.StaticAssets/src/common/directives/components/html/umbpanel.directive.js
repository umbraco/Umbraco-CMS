/**
* @ngdoc directive
* @name umbraco.directives.directive:umbPanel
* @restrict E
**/
angular.module("umbraco.directives.html")
	.directive('umbPanel', function($timeout, $log){
		return {
			restrict: 'E',
			replace: true,
			transclude: 'true',
			templateUrl: 'views/components/html/umb-panel.html'
		};
	});
