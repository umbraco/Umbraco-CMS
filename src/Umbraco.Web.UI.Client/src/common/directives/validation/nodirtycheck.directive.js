/**
* @ngdoc directive
* @name umbraco.directives.directive:noDirtyCheck
* @restrict A
* @description Can be attached to form inputs to prevent them from setting the form as dirty (http://stackoverflow.com/questions/17089090/prevent-input-from-setting-form-dirty-angularjs)
**/
function noDirtyCheck() {
    return {
        restrict: 'A',
        require: 'ngModel',
        link: function (scope, elm, attrs, ctrl) {
            elm.focus(function () {
                ctrl.$pristine = false;
            });
        }
    };
}
angular.module('umbraco.directives.validation').directive("noDirtyCheck", noDirtyCheck);