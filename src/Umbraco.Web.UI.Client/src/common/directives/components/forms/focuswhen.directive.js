angular.module("umbraco.directives").directive('focusWhen', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, elm, attrs, ctrl) {

            var delayTimer;

            attrs.$observe("focusWhen", function (newValue) {
                if (newValue === "true" && document.activeelement !== elm[0]) {
                    delayTimer = $timeout(function () {
                        elm[0].focus();
                    });
                }
            });

            scope.$on('$destroy', function() {
                $timeout.cancel(delayTimer);
            });
        }
    };
});
