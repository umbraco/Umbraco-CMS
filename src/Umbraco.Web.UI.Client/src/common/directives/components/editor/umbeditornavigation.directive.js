(function () {
    'use strict';

    function EditorNavigationDirective(eventsService) {

        function link(scope, el, attr, ctrl) {



            scope.showNavigation = true;
            scope.showMore = false;
            scope.showTray = false;
            scope.overflow = [];
            scope.currentApps = 0;

            

            scope.clickNavigationItem = function (selectedItem) {
                console.log(scope.something);

                if (selectedItem.alias !== "more") {
                    runItemAction(selectedItem);
                    eventsService.emit("app.tabChange", selectedItem);
                    setItemToActive(selectedItem);
                } else {
                    if (scope.showTray === false) {
                        scope.showTray = true;
                    } else {
                        scope.showTray = false;
                    }
                }
            };

            function runItemAction(selectedItem) {
                if (selectedItem.action) {
                    selectedItem.action(selectedItem);
                }
            }

            function setItemToActive(selectedItem) {
                // set all other views to inactive
                if (selectedItem.view) {

                    for (var index = 0; index < scope.navigation.length; index++) {
                        var item = scope.navigation[index];
                        item.active = false;
                    }

                    // set view to active
                    selectedItem.active = true;
                }
            }

            function activate() {

                // hide navigation if there is only 1 item
                if (scope.navigation.length <= 1) {
                    scope.showNavigation = false;
                }
                scope.currentApps = scope.navigation.length;

                var maxApps =4;
                var navLenght = scope.navigation.length - 1;


                var appsToPop = navLenght - maxApps;

                if (navLenght > maxApps) {

                    for (var i = 0; i < appsToPop +1; i++) {
                        var v = scope.navigation.pop();
                        scope.overflow.push(v);
                    }
                    scope.showMore = true;

                }

            }

            activate();
            
        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor-navigation.html',
            scope: {
                navigation: "=",
                something: "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives.html').directive('umbEditorNavigation', EditorNavigationDirective);

})();
