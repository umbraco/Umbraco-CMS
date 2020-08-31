(function () {
    'use strict';

    function umbTextarea($document) {

        function autogrow(scope, element, attributes) {
            if (!element.hasClass("autogrow")) {
                // no autogrow for you today
                return;
            }

            // get possible minimum height style
            var minHeight = parseInt(window.getComputedStyle(element[0]).getPropertyValue("min-height")) || 0;

            // prevent newlines in textbox
            element.on("keydown", function (evt) {
                if (evt.which === 13) {
                    //evt.preventDefault();
                }
            });

            element.on("input", function (evt) {
                element.css({
                    height: 'auto',
                    minHeight: 0
                });

                var contentHeight = this.scrollHeight;
                var borderHeight = 1;
                var paddingHeight = 4;

                element.css({
                    minHeight: null, // remove property
                    height: contentHeight + borderHeight + paddingHeight + "px" // because we're using border-box
                });
            });

            // watch model changes from the outside to adjust height
            scope.$watch(attributes.ngModel, trigger);

            // set initial size
            trigger();

            function trigger() {
                setTimeout(element.triggerHandler.bind(element, "input"), 1);
            }
        }

        var directive = {
            restrict: 'E',
            link: autogrow
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('textarea', umbTextarea);

})();
