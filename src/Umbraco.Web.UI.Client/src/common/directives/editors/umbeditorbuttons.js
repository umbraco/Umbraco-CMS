/**
* @ngdoc directive
* @name umbraco.directives.directive:umbEditorButtons 
* @restrict E
* @function
* @description 
* The button holder for editors (i.e. save, update, etc...)
**/
angular.module("umbraco.directives")
	.directive('umbEditorButtons', function () {
	    return {
	        require: "^umbTabs",
			restrict: 'E',
            transclude: true,
            template: '<div class="umb-tab-buttons" detect-fold ng-class="{\'umb-dimmed\': busy}" ng-transclude></div>',
			link: function(scope, element, attrs, ngModel) {

				
			}
	    };
	});