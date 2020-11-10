angular.module("umbraco.directives").directive('validWhen', function () {
    return {
        require: 'ngModel',
        restrict: 'A',
        link: function (scope, element, attr, ngModel) {

            attr.$observe("validWhen", function (newValue) {
                ngModel.$setValidity("validWhen", newValue === "true");
            });
        }
    };
});
