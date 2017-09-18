function Drawer($location, $routeParams, helpService, userService, localizationService, dashboardResource) {
    
        return {
    
            restrict: "E",    // restrict to an element
            replace: true,   // replace the html element with the template
            templateUrl: 'views/components/application/umbdrawer/umb-drawer.html',
            transclude: true,
            scope: {
                view: "=?",
                model: "=?"
            },
    
            link: function (scope, element, attr, ctrl) {
    
                function onInit() {
                    setView();
                }
    
                function setView() {
                    if (scope.view) {
                        //we do this to avoid a hidden dialog to start loading unconfigured views before the first activation
                        var configuredView = scope.view;
                        if (scope.view.indexOf(".html") === -1) {
                            var viewAlias = scope.view.toLowerCase();
                            configuredView = "views/common/drawers/" + viewAlias + "/" + viewAlias + ".html";
                        }
                        if (configuredView !== scope.configuredView) {
                            scope.configuredView = configuredView;
                        }
                    }
                }
    
                onInit();
               
            }
    
        };
    }
    
    angular.module('umbraco.directives').directive("umbDrawer", Drawer);