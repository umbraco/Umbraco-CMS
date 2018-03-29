angular.module('umbraco.directives.validation')
	.directive('valCompare',function () {
	return {
	        require: ["ngModel", "^form"], 
	        link: function (scope, elem, attrs, ctrls) {

                var ctrl = ctrls[0];
                var formCtrl = ctrls[1];
          
                var otherInput = formCtrl[attrs.valCompare];

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