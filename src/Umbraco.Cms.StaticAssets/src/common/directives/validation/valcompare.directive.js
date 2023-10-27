angular.module('umbraco.directives.validation')
	.directive('valCompare',function () {
	return {
	        require: ["ngModel", "^^form"], 
	        link: function (scope, elem, attrs, ctrls) {

                var ctrl = ctrls[0];
                var formCtrl = ctrls[1];
          
                var otherInput = formCtrl[attrs.valCompare];

                //normal validator on the original source
                ctrl.$validators.valCompare = function(modelValue, viewValue) {
                    return viewValue === otherInput.$viewValue;
                };

                //custom parser on the destination source with custom validation applied to the original source
	            otherInput.$parsers.push(function(value) {
	                ctrl.$setValidity("valCompare", value === ctrl.$viewValue);
	                return value;
	            });
	        }
	};
});
