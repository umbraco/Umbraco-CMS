angular.module('umbraco.directives.validation')
	.directive('valCompare',function () {
	return {
	        require: "ngModel", 
	        link: function (scope, elem, attrs, ctrl) {
	            
	            //TODO: Pretty sure this should be done using a requires ^form in the directive declaration	            
	            var otherInput = elem.inheritedData("$formController")[attrs.valCompare];

	            ctrl.$parsers.push(function(value) {
	                if(value === otherInput.$viewValue) {
	                    ctrl.$setValidity("valCompare", true);
	                    return value;
	                }
	                ctrl.$setValidity("valCompare", false);
	            });

	            otherInput.$parsers.push(function(value) {
	                ctrl.$setValidity("valCompare", value === ctrl.$viewValue);
	                return value;
	            });
	        }
	};
});