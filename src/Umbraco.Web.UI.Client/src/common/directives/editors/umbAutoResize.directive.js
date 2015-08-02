angular.module("umbraco.directives")
    .directive('umbAutoResize', function($timeout) {
        return {
            require: "^?umbTabs",
            link: function(scope, element, attr, tabsCtrl) {
                var domEl = element[0];
                var update = function(force) {
                    if (force === true) {
                        element.height(0);
                    }

                    if (domEl.scrollHeight !== domEl.clientHeight) {
                        element.height(domEl.scrollHeight);
                    }
                };

                element.bind('keyup keydown keypress change', update);
                element.bind('blur', function() { update(true); });

                $timeout(function() {
                    update(true);
                }, 200);


                //listen for tab changes
                if (tabsCtrl != null) {
                    tabsCtrl.onTabShown(function(args) {
                        update();
                    });
                }
            }
        };
    });
