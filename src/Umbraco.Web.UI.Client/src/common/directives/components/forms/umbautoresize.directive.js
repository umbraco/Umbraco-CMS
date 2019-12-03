angular.module("umbraco.directives")
    .directive('umbAutoResize', function($timeout) {
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

                const update = force => {
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
                    umbTabsController.onTabShown(() => update());
                }

                // listen for ng-model changes
                // timeout ensure change has been bound before attempting to calculate size
                var unbindModelWatcher =
                    scope.$watch(() => ngModelController.$modelValue,
                        () => $timeout(() => update(true)));

                scope.$on('$destroy', function () {
                    element.off('keyup keydown keypress change', update);
                    element.off('blur', update(true));

                    unbindModelWatcher();
                });
            }
        };
    });
