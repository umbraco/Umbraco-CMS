(function () {
    'use strict';

    function EditorNavigationDirective(eventsService) {

        function link(scope, el, attr, ctrl) {

            scope.showNavigation = true;
            scope.showMoreButton = false;
            scope.showTray = false;

            scope.moreButton = {
                alias: "more",
                active: false,
                name: "More",
                icon: "icon-thumbnails-small",
                view: "views/content/apps/info/info.html"
            };

            scope.clickNavigationItem = function (selectedItem) {
                runItemAction(selectedItem);
                eventsService.emit("app.tabChange", selectedItem);
                setItemToActive(selectedItem);
            };

            scope.trayClick = function () {
                if (scope.showTray === false) {
                    scope.showTray = true;
                } else {
                    scope.showTray = false;
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

                        // set 'more' button active if there is an active app in the app tray
                        var selectedItemIndex = scope.navigation.indexOf(selectedItem);
                        if (selectedItemIndex + 1 > scope.contentAppsLimit) {
                            scope.moreButton.active = true;
                        } else {
                            scope.moreButton.active = false;
                        }

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
            }

            function showMoreButton() {
                if (scope.showNavigation === false) {
                    return;
                }


                if (scope.navigation.length > scope.contentAppsLimit) {
                    scope.showMoreButton = true;

                    calculateTrayLimit();
                }
            }

            function calculateTrayLimit() {
                var navLength = scope.navigation.length;
                var maxApps = scope.contentAppsLimit;

                scope.contentAppsTrayLimit = maxApps - navLength;
            }

            activate();
            showMoreButton();
        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor-navigation.html',
            scope: {
                navigation: "=",
                contentAppsLimit: "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives.html').directive('umbEditorNavigation', EditorNavigationDirective);

})();
