(function() {
    'use strict';

    function ChildSelectorDirective() {

        function link(scope, el, attr, ctrl) {

            scope.dialogModel = {};
            scope.showDialog = false;

            scope.removeSelectedChild = function(selectedChild) {
                var selectedChildIndex = scope.selectedChildren.indexOf(selectedChild);
                scope.selectedChildren.splice(selectedChildIndex, 1);
            };

            scope.openItemPicker = function($event) {
                scope.showDialog = false;
                scope.dialogModel = {};
                scope.dialogModel.title = "Choose child";
                scope.dialogModel.availableItems = scope.availableChildren;
                scope.dialogModel.selectedItems = scope.selectedChildren;
                scope.dialogModel.event = $event;
                scope.dialogModel.view = "views/common/dialogs/itempicker/itempicker.html";
                scope.showDialog = true;

                scope.dialogModel.chooseItem = function(selectedChild) {
                    scope.selectedChildren.push(selectedChild);
                    scope.showDialog = false;
                    scope.dialogModel = null;
                };

                scope.dialogModel.close = function(){
                    scope.showDialog = false;
                    scope.dialogModel = null;
                };
            };

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-child-selector.html',
            scope: {
                selectedChildren: '=',
                availableChildren: "=",
                parent: "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbChildSelector', ChildSelectorDirective);

})();