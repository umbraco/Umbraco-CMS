(function () {
    'use strict';

    function EditorNavigationDirective($window, $timeout, eventsService, windowResizeListener) {

        function link(scope) {

            const unsubscribe = [];

            scope.showNavigation = true;
            scope.showMoreButton = false;
            scope.showDropdown = false;
            scope.overflowingItems = 0;
            scope.itemsLimit = Number.isInteger(scope.limit) ? scope.limit : 6;

            scope.moreButton = {
                alias: "more",
                active: false,
                name: "More"
            };

            scope.openNavigationItem = item => {
                
                scope.showDropdown = false;
                runItemAction(item);
                setItemToActive(item);
                if (scope.onSelect) {
                    scope.onSelect({"item": item});
                }
                eventsService.emit("app.tabChange", item);
            };

            scope.openAnchorItem = (item, anchor) => {
                if (scope.onAnchorSelect) {
                    scope.onAnchorSelect({"item": item, "anchor": anchor});
                }
                if (item.active !== true) {
                    scope.openNavigationItem(item);
                }
            };

            scope.toggleDropdown = () => {
                scope.showDropdown = !scope.showDropdown;
            };

            scope.hideDropdown = () => {
                scope.showDropdown = false;
            };

            function onInit() {
                var firstRun = true;
                calculateVisibleItems($window.innerWidth);
;
                unsubscribe.push(scope.$watch("navigation", (newVal, oldVal) => {
                    const newLength = newVal.length;
                    const oldLength = oldVal.length;

                    if (firstRun || newLength !== undefined && newLength !== oldLength) {
                        firstRun = false;
                        scope.showNavigation = newLength > 1;
                        calculateVisibleItems($window.innerWidth);
                    }

                    setMoreButtonErrorState();
                }, true));
            }

            function calculateVisibleItems(windowWidth) {
                // if we don't get a windowWidth stick with the default item limit
                if (!windowWidth) {
                    return;
                }

                // if we haven't set a specific limit prop we base the amount of visible items on the window width
                if (scope.limit === undefined) {
                    scope.itemsLimit = 0;

                    // set visible items based on browser width
                    if (windowWidth > 1500) {
                        scope.itemsLimit = 6;
                    } 
                    else if (windowWidth > 700) {
                        scope.itemsLimit = 4;
                    }
                }

                // toggle more button
                if(scope.navigation.length > scope.itemsLimit) {
                    scope.showMoreButton = true;
                    scope.overflowingItems = scope.itemsLimit - scope.navigation.length;
                } else {
                    scope.showMoreButton = false;
                    scope.overflowingItems = 0;
                }

                scope.moreButton.name = scope.itemsLimit === 0 ? "Menu" : "More";
                setMoreButtonActiveState();
                setMoreButtonErrorState();
            }

            function runItemAction(selectedItem) {
                if (selectedItem.action) {
                    selectedItem.action(selectedItem);
                }
            }

            function setItemToActive(selectedItem) {
                if (selectedItem.view) {
                    
                    // deselect all items
                    Utilities.forEach(scope.navigation, item => {
                        item.active = false;
                    });
                    
                    // set clicked item to active
                    selectedItem.active = true;
                    setMoreButtonActiveState();
                    setMoreButtonErrorState();
                }
            }

            function setMoreButtonActiveState() {
                // set active state on more button if any of the overflown items is active
                scope.moreButton.active = scope.navigation.findIndex(item => item.active) + 1 > scope.itemsLimit;
            };

            function setMoreButtonErrorState() {
                if (scope.overflowingItems === 0) {
                    return;
                }

                const overflow = scope.navigation.slice(scope.itemsLimit, scope.navigation.length);
                const active = scope.navigation.find(item => item.active)
                // set error state on more button if any of the overflown items has an error. We use it show the error badge and color the item
                scope.moreButton.hasError = overflow.filter(item => item.hasError).length > 0;
                // set special active/error state on button if the current selected item is has an error
                // we don't want to show the error badge in this case so we need a special state for that
                scope.moreButton.activeHasError = active.hasError;
            };

            var resizeCallback = size => {
                if (size && size.width) {
                    calculateVisibleItems(size.width);
                }
            };

            windowResizeListener.register(resizeCallback);

            unsubscribe.push(scope.$watch('limit', (newVal) => {
                scope.itemsLimit = newVal;
                calculateVisibleItems($window.innerWidth);
            }));

            scope.$on('$destroy', function () {
                windowResizeListener.unregister(resizeCallback);

                for (var u in unsubscribe) {
                    unsubscribe[u]();
                }
            });

            onInit();
        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor-navigation.html',
            scope: {
                navigation: "=",
                onSelect: "&",
                onAnchorSelect: "&",
                limit: "<"
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives.html').directive('umbEditorNavigation', EditorNavigationDirective);

})();
