angular.module("umbraco.directives")
    .directive('umbAutoResize', function ($timeout) {
        return {
            require: ["^?umbTabs", "ngModel"],
            link: function (scope, element, attr, controllersArr) {

                var domEl = element[0];
                var domElType = domEl.type;
                var umbTabsController = controllersArr[0];
                var ngModelController = controllersArr[1];

                function resizeInput() {

                    if (domEl.scrollWidth !== domEl.clientWidth) {
                        if (ngModelController.$modelValue) {
                            element.width(domEl.scrollWidth);
                        }
                    }

                    // if the element is hidden the width will be 0 even though it has a value. 
                    // This could happen if the element is hidden in a tab.
                    if (ngModelController.$modelValue && domEl.clientWidth === 0) {
                        element.width('auto');
                    }

                    if (!ngModelController.$modelValue && attr.placeholder) {
                        attr.$set('size', attr.placeholder.length);
                        element.width('auto');
                    }

                }

                function resizeTextarea() {
                    if (domEl.scrollHeight !== domEl.clientHeight) {
                        element.height(domEl.scrollHeight);
                    }
                }

                var update = function (force) {
                    if (force === true) {
                        if (domElType === "textarea") {
                            element.height(0);
                        } else if (domElType === "text") {
                            element.width(0);
                        }
                    }

                    if (domElType === "textarea") {
                        resizeTextarea();
                    } else if (domElType === "text") {
                        resizeInput();
                    }
                };

                //listen for tab changes
                if (umbTabsController != null) {
                    umbTabsController.onTabShown(function (args) {
                        update();
                    });
                }

                // listen for ng-model changes
                var unbindModelWatcher = scope.$watch(function () {
                    return ngModelController.$modelValue;
                }, function (newValue) {
                    $timeout(
                        function () {
                            update(true);
                        }
                    );
                });

                scope.$on('$destroy', function () {
                    element.off('keyup keydown keypress change', update);
                    element.off('blur', update(true));
                    unbindModelWatcher();
                });
            }
        };
    });
