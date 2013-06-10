angular.module("umbraco.directives")
	.directive('umbPanel', function(){
		return {
			restrict: 'E',
			replace: true,
			transclude: 'true',
			templateUrl: 'views/directives/umb-panel.html'
		};
	});