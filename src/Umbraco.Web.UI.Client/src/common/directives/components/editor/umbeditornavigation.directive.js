(function () {
    'use strict';

    function EditorNavigationDirective(eventsService, $timeout) {

        function link(scope, el, attr, ctrl) {


            var maxApps = 3;
            var contentAppsWidth = [];

            scope.showNavigation = true;
            scope.showMoreButton = false;
            scope.showTray = false;

            scope.contentAppsLimit = maxApps;


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

                $timeout(function () {

                    $("#contentApps li").each(function (index) {
                        contentAppsWidth.push($(this).outerWidth());
                    });
                });


                // hide navigation if there is only 1 item
                if (scope.navigation.length <= 1) {
                    scope.showNavigation = false;
                }
            }

            function showMoreButton() {
                if (scope.showNavigation === false) {
                    return;
                }

                if (scope.contentAppsLimit < maxApps) {
                    scope.showMoreButton = true;

                } else {
                    scope.showMoreButton = false;
                }
            }

            window.onresize = calculateWidth;

            function calculateWidth() {
                $timeout(function () {
                    //total width minus room for avatar, search and help icon
                    var nameHeaderWidth = $(window).width() - 200;
                    var appsWidth = 0;
                    var totalApps = scope.navigation.length;

                    // detect how many apps we can show on the screen and tray
                    for (var i = 0; i < contentAppsWidth.length; i++) {
                        var appItemWidth = contentAppsWidth[i];
                        appsWidth += appItemWidth;

                        // substract current content apps width to get a more accurate measurement of the name header width
                        nameHeaderWidth -= appsWidth;

                        if (appsWidth > nameHeaderWidth) {
                            scope.contentAppsLimit = i - 1;
                            scope.contentAppsTrayLimit = scope.contentAppsLimit - totalApps;
                            break;
                        } else {
                            scope.contentAppsLimit = maxApps;
                            scope.contentAppsTrayLimit = scope.contentAppsLimit - totalApps;
                        }

                    }
                });

                showMoreButton();
            }


            activate();
            showMoreButton();
        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor-navigation.html',
            scope: {
                navigation: "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives.html').directive('umbEditorNavigation', EditorNavigationDirective);

})();
