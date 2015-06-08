/**
* @ngdoc directive
* @name umbraco.directives.directive:umbContentName 
* @restrict E
* @function
* @description 
* Used by editors that require naming an entity. Shows a textbox/headline with a required validator within it's own form.
**/

angular.module("umbraco.directives")
	.directive('umbLockedField', function ($timeout, localizationService) {
	    return {

			require: "ngModel",
			restrict: 'E',
			replace: true,

			templateUrl: 'views/components/umb-locked-field.html',
			
			scope: {
				model: '=ngModel'
			},

			link: function(scope, element, attrs, ngModel) {

				scope.locked = true;
				scope.toggleLock = function(){
					if(scope.locked){
						scope.locked = false;
					}else{
						scope.locked =true;
					}
				};

			}

		};
		});