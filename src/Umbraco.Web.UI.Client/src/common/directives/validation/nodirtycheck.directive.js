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

            // If value if "no-dirty-check" attribute truthy, then skip and use default behaviour.
            var dirtyCheck = Object.toBoolean(attrs.noDirtyCheck) === true;
            if (dirtyCheck)
                return;

            var alwaysFalse = {
                get: function () { return false; },
                set: function () { }
            };

            Object.defineProperty(ctrl, '$pristine', alwaysFalse);
            Object.defineProperty(ctrl, '$dirty', alwaysFalse);
        }
    };
}
angular.module('umbraco.directives.validation').directive("noDirtyCheck", noDirtyCheck);
