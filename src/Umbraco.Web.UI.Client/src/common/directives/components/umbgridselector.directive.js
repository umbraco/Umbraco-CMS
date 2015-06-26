(function() {
    'use strict';

    function GridSelector() {

        function link(scope, el, attr, ctrl) {

            scope.dialogModel = {};
            scope.showDialog = false;
            scope.name = "";

            // set default item name
            if(!scope.itemName){
                scope.name = "item";
            } else {
                scope.name = scope.itemName;
            }

            scope.removeItem = function(selectedItem) {
                var selectedItemIndex = scope.selectedItems.indexOf(selectedItem);
                scope.selectedItems.splice(selectedItemIndex, 1);
            };

            scope.removeDefaultItem = function() {

                // it will be the last item so we can clear the array
                scope.selectedItems = [];

                // remove as default item
                scope.defaultItem = null;

            };

            scope.openItemPicker = function($event){
                scope.dialogModel = {};
                scope.dialogModel.title = "Choose " + scope.name;
                scope.dialogModel.availableItems = scope.availableItems;
                scope.dialogModel.selectedItems = scope.selectedItems;
                scope.dialogModel.event = $event;
                scope.dialogModel.view = "views/common/dialogs/itempicker/itempicker.html";
                scope.showDialog = true;

                scope.dialogModel.chooseItem = function(selectedItem) {

                    scope.selectedItems.push(selectedItem);

                    // if no default item - set item as default
                    if(scope.defaultItem === null) {
                        scope.setAsDefaultItem(selectedItem);
                    }

                    scope.showDialog = false;
                    scope.dialogModel = null;
                };

                scope.dialogModel.close = function(){
                    scope.showDialog = false;
                    scope.dialogModel = null;
                };

            };

            scope.setAsDefaultItem = function(selectedItem) {
                scope.defaultItem = {};
                scope.defaultItem = selectedItem;
            };

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-grid-selector.html',
            scope: {
                selectedItems: '=',
                availableItems: "=",
                defaultItem: "=",
                itemName: "@"
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbGridSelector', GridSelector);

})();