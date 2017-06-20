(function () {
    'use strict';

    function ToggleDirective() {

        function link(scope, el, attr, ctrl) {

            function onInit() {
                setLabelText();
            }

            function setLabelText() {

                if (!scope.labelOn) {
                    scope.labelOn = "On";
                }

                if (!scope.labelOff) {
                    scope.labelOff = "Off";
                }

            }

            scope.click = function() {
                if(scope.onClick) {
                    scope.onClick();
                }
            };

            onInit();

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/buttons/umb-toggle.html',
            link: link,
            scope: {
                checked: "=",
                labelOn: "@?",
                labelOff: "@?",
                onClick: "&"
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbToggle', ToggleDirective);

})();



