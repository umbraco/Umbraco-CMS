/**
@ngdoc directive
@name umbraco.directives.directive:umbDrawer
@restrict E
@scope

@description
The drawer component is a global component and is already added to the umbraco markup. It is registered in globalState and can be opened and configured by raising events.

<h3>Markup example - how to open the drawer</h3>
<pre>
    <div ng-controller="My.DrawerController as vm">

        <umb-button
            type="button"
            label="Toggle drawer"
            action="vm.toggleDrawer()">
        </umb-button>

    </div>
</pre>

<h3>Controller example - how to open the drawer</h3>
<pre>
    (function () {
        "use strict";

        function DrawerController(appState) {

            var vm = this;

            vm.toggleDrawer = toggleDrawer;

            function toggleDrawer() {

                var showDrawer = appState.getDrawerState("showDrawer");            

                var model = {
                    firstName: "Super",
                    lastName: "Man"
                };

                appState.setDrawerState("view", "/App_Plugins/path/to/drawer.html");
                appState.setDrawerState("model", model);
                appState.setDrawerState("showDrawer", !showDrawer);
                
            }

        }

        angular.module("umbraco").controller("My.DrawerController", DrawerController);

    })();
</pre>

<h3>Use the following components in the custom drawer to render the content</h3>
<ul>
    <li>{@link umbraco.directives.directive:umbDrawerView umbDrawerView}</li>
    <li>{@link umbraco.directives.directive:umbDrawerHeader umbDrawerHeader}</li>
    <li>{@link umbraco.directives.directive:umbDrawerContent umbDrawerContent}</li>
    <li>{@link umbraco.directives.directive:umbDrawerFooter umbDrawerFooter}</li>
</ul>

@param {string} view (<code>binding</code>): Set the drawer view
@param {string} model (<code>binding</code>): Pass in custom data to the drawer

**/

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
