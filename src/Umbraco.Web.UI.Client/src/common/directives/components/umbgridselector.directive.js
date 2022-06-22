(function () {
    'use strict';

    function GridSelector(overlayService, editorService) {

        function link(scope) {

            var eventBindings = [];
            scope.dialogModel = {};
            scope.showDialog = false;
            scope.itemLabel = "";

            // set default item name
            if (!scope.itemName) {
                scope.itemLabel = "item";
            } else {
                scope.itemLabel = scope.itemName;
            }

            scope.removeItem = function (selectedItem) {
                var selectedItemIndex = scope.selectedItems.indexOf(selectedItem);
                scope.selectedItems.splice(selectedItemIndex, 1);
            };

            scope.removeDefaultItem = function () {

                // it will be the last item so we can clear the array
                scope.selectedItems = [];

                // remove as default item
                scope.defaultItem = null;

            };

            scope.openItemPicker = function ($event) {
                if (scope.itemPicker) {
                    scope.itemPicker();
                } else {
                    var dialogModel = {
                        view: "itempicker",
                        title: "Choose " + scope.itemLabel,
                        availableItems: scope.availableItems,
                        selectedItems: scope.selectedItems,
                        position: "target",
                        event: $event,
                        submit: function (model) {
                            scope.selectedItems.push(model.selectedItem);
                            // if no default item - set item as default
                            if (scope.defaultItem === null) {
                                scope.setAsDefaultItem(model.selectedItem);
                            }
                            overlayService.close();
                        },
                        close: function () {
                            overlayService.close();
                        }
                    };
                    overlayService.open(dialogModel);
                }
            };

            scope.openTemplate = function (selectedItem) {
                const editor = {
                    id: selectedItem.id,
                    submit: function () {
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.templateEditor(editor);
            }

            scope.setAsDefaultItem = function (selectedItem) {

                // clear default item
                scope.defaultItem = {};

                // set as default item
                scope.defaultItem = selectedItem;
            };

            function updatePlaceholders() {

                // update default item
                if (scope.defaultItem !== null && scope.defaultItem.placeholder) {

                    scope.defaultItem.name = scope.name;

                    if (scope.alias !== null && scope.alias !== undefined) {
                        scope.defaultItem.alias = scope.alias;
                    }

                }

                // update selected items
                Utilities.forEach(scope.selectedItems, selectedItem => {
                    if (selectedItem.placeholder) {

                        selectedItem.name = scope.name;

                        if (scope.alias !== null && scope.alias !== undefined) {
                            selectedItem.alias = scope.alias;
                        }
                    }
                });

                // update availableItems
                Utilities.forEach(scope.availableItems, availableItem => {
                    if (availableItem.placeholder) {

                        availableItem.name = scope.name;

                        if (scope.alias !== null && scope.alias !== undefined) {
                            availableItem.alias = scope.alias;
                        }
                    }
                });

            }

            function activate() {

                // add watchers for updating placeholde name and alias
                if (scope.updatePlaceholder) {
                    eventBindings.push(scope.$watch('name', function (newValue, oldValue) {
                        updatePlaceholders();
                    }));

                    eventBindings.push(scope.$watch('alias', function (newValue, oldValue) {
                        updatePlaceholders();
                    }));
                }

            }

            activate();

            // clean up
            scope.$on('$destroy', function () {

                // clear watchers
                for (var e in eventBindings) {
                    eventBindings[e]();
                }

            });

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-grid-selector.html',
            scope: {
                name: "=",
                alias: "=",
                selectedItems: '=',
                availableItems: "=",
                defaultItem: "=",
                itemName: "@",
                updatePlaceholder: "=",
                itemPicker: "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbGridSelector', GridSelector);

})();
