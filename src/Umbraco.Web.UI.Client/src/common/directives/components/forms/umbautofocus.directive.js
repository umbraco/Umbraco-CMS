angular.module("umbraco.directives")
    .directive('umbAutoFocus', function($timeout) {

        return function (scope, element, attrs) {
            
            var update = function() {
                //if it uses its default naming
                if (element.val() === "" || attrs.focusOnFilled) {
                    element.trigger("focus");
                }
            };

            attrs.$observe("umbAutoFocus", function (newVal) {
                var enabled = (newVal === "false" || newVal === 0 || newVal === false) ? false : true;
                if (enabled) {
                    $timeout(function() {
                        update();
                    });
                }
            });

        };
});
