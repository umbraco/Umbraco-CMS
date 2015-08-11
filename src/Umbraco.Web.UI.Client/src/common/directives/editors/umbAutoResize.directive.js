angular.module("umbraco.directives")
    .directive('umbAutoResize', function($timeout) {
        return {
            require: ["^?umbTabs", "ngModel"],
            link: function(scope, element, attr, controllersArr) {

                var domEl = element[0];
                var domElType = domEl.type;
                var umbTabsController = controllersArr[0];
                var ngModelController = controllersArr[1];

                var update = function(force) {


                  if (force === true) {

                    if (domElType === "textarea") {
                      element.height(0);
                    } else if (domElType === "text") {
                      element.width(0);
                    }
                  }

                  if (domEl.scrollHeight !== domEl.clientHeight && domElType === "textarea") {
                    element.height(domEl.scrollHeight);
                  } else if (domEl.scrollWidth !== domEl.clientWidth && domElType === "text") {

                    if (ngModelController.$modelValue === undefined || ngModelController.$modelValue === "" || ngModelController.$modelValue === null) {

                      if (attr.placeholder) {
                        attr.$set('size', attr.placeholder.length);
                        element.width('auto');
                      }

                    } else {
                      element.width(domEl.scrollWidth);
                    }

                  }


                };

                var blur = function() {
                    update(true);
                };

                element.bind('keyup keydown keypress change', update);
                element.bind('blur', blur);

                $timeout(function() {
                    update(true);
                }, 200);

                //listen for tab changes
                if (umbTabsController != null) {
                    umbTabsController.onTabShown(function(args) {
                        update();
                    });
                }

                // listen for ng-model changes
                var unbindModelWatcher = scope.$watch(function () {
                  return ngModelController.$modelValue;
                }, function(newValue) {
                  update(true);
                });

                scope.$on('$destroy', function () {
                    element.unbind('keyup keydown keypress change', update);
                    element.unbind('blur', blur);
                    unbindModelWatcher();
                });
            }
        };
    });
