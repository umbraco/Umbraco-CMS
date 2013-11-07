/**
* @ngdoc directive
* @name umbraco.directives.directive:umbContentName 
* @restrict E
* @function
* @description 
* Used by editors that require naming an entity. Shows a textbox/headline with a required validator within it's own form.
**/
angular.module("umbraco.directives")
	.directive('umbContentName', function ($timeout, localizationService) {
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

				var inputElement = element.find("input");
				if(scope.placeholder && scope.placeholder[0] === "@"){
					localizationService.localize(scope.placeholder.substring(1))
						.then(function(value){
							scope.placeholder = value;	
						});
				}

				$timeout(function(){
					if(!scope.model){
						scope.goEdit();
					}
				}, 100, false);
			
				scope.goEdit = function(){
					scope.editMode = true;

					$timeout(function () {					    
					    inputElement.focus();					    
					}, 100, false);
				};

				scope.exitEdit = function(){
					if(scope.model && scope.model !== ""){
						scope.editMode = false;	
					}
				};
			}
	    };
	});