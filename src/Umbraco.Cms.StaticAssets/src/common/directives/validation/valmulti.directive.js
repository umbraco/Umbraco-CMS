(function () {
    /**
    * @ngdoc directive
    * @name umbraco.directives.directive:multi
    * @restrict A
    * @description Used on input fields when you want to validate multiple fields at once.
    **/
    function multi($parse, $rootScope) {
        return {
            restrict: 'A',
            require: 'ngModel',
            link: function (scope, elem, attrs, ngModelCtrl) {
                var validate = $parse(attrs.multi)(scope);
                ngModelCtrl.$viewChangeListeners.push(function () {
                    // ngModelCtrl.$setValidity('multi', validate());
                    $rootScope.$broadcast('multi:valueChanged');
                });

                var deregisterListener = scope.$on('multi:valueChanged', function (event) {
                    ngModelCtrl.$setValidity('multi', validate());
                });
                scope.$on('$destroy', deregisterListener); // optional, only required for $rootScope.$on
            }
        };
    }
    angular.module('umbraco.directives.validation').directive('multi', ['$parse', '$rootScope', multi]);
})();
