angular.module("umbraco.directives").directive('focusWhen', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, elm, attrs, ctrl) {
            attrs.$observe("focusWhen", function (newValue) {
                if (newValue === "true") {
                    $timeout(function() {
                        elm.focus();
                    });
                }
            });
        }
    };
});
