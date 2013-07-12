angular.module("umbraco.directives")
	.directive('preventDefault', function () {
		return function (scope, element, attrs) {
		    $(element).click(function (event) {
		       event.preventDefault();
			});
		};
	});