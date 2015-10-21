(function() {
    'use strict';

    function ChildSelectorDirective() {

        function link(scope, el, attr, ctrl) {

            var eventBindings = [];
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
                scope.dialogModel.view = "itemPicker";
                scope.showDialog = true;

                scope.dialogModel.chooseItem = function(selectedChild) {

                    var child = {
                      "id": selectedChild.id,
                      "name": selectedChild.name,
                      "icon": selectedChild.icon,
                      "alias": selectedChild.alias
                    };

                    scope.selectedChildren.push(child);

                    scope.showDialog = false;
                    scope.dialogModel = null;
                };

                scope.dialogModel.close = function(){
                    scope.showDialog = false;
                    scope.dialogModel = null;
                };
            };

            function syncParentName() {

              // update name on available item
              angular.forEach(scope.availableChildren, function(availableChild){
                if(availableChild.id === scope.parentId) {
                  availableChild.name = scope.parentName;
                }
              });

              // update name on selected child
              angular.forEach(scope.selectedChildren, function(selectedChild){
                if(selectedChild.id === scope.parentId) {
                  selectedChild.name = scope.parentName;
                }
              });

            }

            function syncParentIcon() {

              // update icon on available item
              angular.forEach(scope.availableChildren, function(availableChild){
                if(availableChild.id === scope.parentId) {
                  availableChild.icon = scope.parentIcon;
                }
              });

              // update icon on selected child
              angular.forEach(scope.selectedChildren, function(selectedChild){
                if(selectedChild.id === scope.parentId) {
                  selectedChild.icon = scope.parentIcon;
                }
              });

            }

            function insertParentPlaceholder() {

              var placeholderExists = false;
              var placeholder = {
                "name": scope.parentName,
                "icon": scope.parentIcon,
                "id": scope.parentId,
                "alias": "umbChildNodePlaceholder"
              };

              if(scope.availableChildren.length === 0) {

                scope.availableChildren.push(placeholder);

              } else {

                angular.forEach(scope.availableChildren, function(availableChild){
                  if(availableChild.id !== placeholder.id && !placeholderExists) {
                    scope.availableChildren.push(placeholder);
                    placeholderExists = true;
                  }
                });

              }

            }

            function createSelectedChildrenObjectArray(selectedChildren, availableChildren) {

              if( selectedChildren.length > 0 && availableChildren.length > 0 && !angular.isObject(selectedChildren[0])) {

                var newArray = [];

                angular.forEach(selectedChildren, function(selectedChild){

                    var selectedChildId = selectedChild;

                    angular.forEach(availableChildren, function(availableChild){

                      if(selectedChildId === availableChild.id) {

                        var selectedChild = {};

                        selectedChild.id = availableChild.id;
                        selectedChild.name = availableChild.name;
                        selectedChild.icon = availableChild.icon;
                        selectedChild.alias = availableChild.alias;

                        newArray.push(selectedChild);

                      }

                    });

                });
                return newArray;

              } else {

                return selectedChildren;

              }

            }

            eventBindings.push(scope.$watch('parentName', function(newValue, oldValue){

              if (newValue === oldValue) { return; }
              if ( oldValue === undefined || newValue === undefined) { return; }

              syncParentName();

            }));

            eventBindings.push(scope.$watch('parentIcon', function(newValue, oldValue){

              if (newValue === oldValue) { return; }
              if ( oldValue === undefined || newValue === undefined) { return; }

              syncParentIcon();
            }));

            eventBindings.push(scope.$watch('availableChildren', function(newValue, oldValue){

              if (newValue === oldValue) { return; }
              if ( oldValue === undefined || newValue === undefined) { return; }

              scope.selectedChildren = createSelectedChildrenObjectArray(scope.selectedChildren, scope.availableChildren);

              if(scope.parentId === 0) {
                insertParentPlaceholder();
              }

            }));

            eventBindings.push(scope.$watch('selectedChildren', function(newValue, oldValue){

              if (newValue === oldValue) { return; }
              if ( oldValue === undefined || newValue === undefined) { return; }

              scope.selectedChildren = createSelectedChildrenObjectArray(scope.selectedChildren, scope.availableChildren);
            }));

            // clean up
            scope.$on('$destroy', function(){
              // unbind watchers
              for(var e in eventBindings) {
                eventBindings[e]();
               }
            });

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-child-selector.html',
            scope: {
                selectedChildren: '=',
                availableChildren: "=",
                parentName: "=",
                parentIcon: "=",
                parentId: "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbChildSelector', ChildSelectorDirective);

})();
