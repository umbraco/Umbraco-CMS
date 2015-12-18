(function() {
    'use strict';

    function ChildSelectorDirective() {

        function link(scope, el, attr, ctrl) {

            var eventBindings = [];
            scope.dialogModel = {};
            scope.showDialog = false;

            scope.removeChild = function(selectedChild, $index) {
               if(scope.onRemove) {
                  scope.onRemove(selectedChild, $index);
               }
            };

            scope.addChild = function($event) {
               if(scope.onAdd) {
                  scope.onAdd($event);
               }
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
                parentId: "=",
                onRemove: "=",
                onAdd: "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbChildSelector', ChildSelectorDirective);

})();
