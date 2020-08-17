/**
* @ngdoc directive
* @name umbraco.directives.directive:noDirtyCheck
* @restrict A
* @description Can be attached to form inputs to prevent them from setting the form as dirty (https://stackoverflow.com/questions/17089090/prevent-input-from-setting-form-dirty-angularjs)
**/
function noDirtyCheck() {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, elm, attrs, ctrl) {

            // If no attribute value is "false", then skip and use default behaviour.
            var dirtyCheck = scope.$eval(attrs.noDirtyCheck) === false;
            if (dirtyCheck)
                return;

            ctrl.$setDirty = Utilities.noop;
        }
    };
}
angular.module('umbraco.directives.validation').directive("noDirtyCheck", noDirtyCheck);
