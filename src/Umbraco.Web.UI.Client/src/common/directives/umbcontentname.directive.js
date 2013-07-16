/**
* @ngdoc object 
* @name umbraco.directive:umbContentName 
* @restrict E
* @description 
* Used by editors that require naming an entity. Shows a textbox/headline with a required validator within it's own form.
**/
angular.module("umbraco.directives")
	.directive('umbContentName', function () {
	    return {
	        require: "ngModel",
			restrict: 'E',
			replace: true,
			templateUrl: 'views/directives/umb-content-name.html',
			scope: {
			    placeholder: '@placeholder',
			    model: '=ngModel'
			},
			link: function(scope, element, attrs, modelCtrl) {
			}
	    };
	});