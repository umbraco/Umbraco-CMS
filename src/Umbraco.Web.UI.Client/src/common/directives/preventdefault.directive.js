/**
* @ngdoc directive
* @name umbraco.directives.directive:preventDefault
**/
angular.module("umbraco.directives")
	.directive('preventDefault', function () {
		return function (scope, element, attrs) {
		    $(element).click(function (event) {
				if(event.metaKey || event.ctrlKey){
					return;
				}else{
					event.preventDefault();
				}		
			});
		};
	});