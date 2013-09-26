/**
* @ngdoc directive
* @name umbraco.directives.directive:umbContentName 
* @restrict E
* @function
* @description 
* Used by editors that require naming an entity. Shows a textbox/headline with a required validator within it's own form.
**/
angular.module("umbraco.directives")
	.directive('umbContentName', function ($timeout) {
	    return {
	        require: "ngModel",
			restrict: 'E',
			replace: true,
			templateUrl: 'views/directives/umb-content-name.html',
			scope: {
			    placeholder: '@placeholder',
			    model: '=ngModel'
			},
			link: function(scope, element, attrs, ngModel) {

				ngModel.$render = function(){
					$timeout(function(){
						if(scope.model === ""){
							scope.goEdit();
						}
					}, 100);
				};

				scope.goEdit = function(){
					scope.editMode = true;
					$timeout(function(){
						element.find("input").focus();
					}, 100);
				};

				scope.exitEdit = function(){
					scope.editMode = false;

					if(scope.model === ""){
						scope.model = "Empty...";
					}
				};
			}
	    };
	});