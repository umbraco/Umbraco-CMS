/**
@ngdoc directive
@name umbraco.directives.directive:umbChildSelector
@restrict E
@scope

@description
Use this directive to render a ui component for selecting child items to a parent node.

<h3>Markup example</h3>
<pre>
	<div ng-controller="My.Controller as vm">

        <umb-child-selector
                selected-children="vm.selectedChildren"
                available-children="vm.availableChildren"
                parent-name="vm.name"
                parent-icon="vm.icon"
                parent-id="vm.id"
                on-add="vm.addChild"
                on-remove="vm.removeChild">
        </umb-child-selector>

	</div>
</pre>

<h3>Controller example</h3>
<pre>
	(function () {
		"use strict";

		function Controller(overlayService) {

            var vm = this;

            vm.id = 1;
            vm.name = "My Parent element";
            vm.icon = "icon-document";
            vm.selectedChildren = [];
            vm.availableChildren = [
                {
                    id: 1,
                    alias: "item1",
                    name: "Item 1",
                    icon: "icon-document"
                },
                {
                    id: 2,
                    alias: "item2",
                    name: "Item 2",
                    icon: "icon-document"
                }
            ];

            vm.addChild = addChild;
            vm.removeChild = removeChild;

            function addChild($event) {
                
                const dialog = {
                    view: "itempicker",
                    title: "Choose child",
                    availableItems: vm.availableChildren,
                    selectedItems: vm.selectedChildren,
                    event: $event,
                    submit: function(model) {
                        
                        if (model.selectedItem) {
                            // add selected child
                            vm.selectedChildren.push(model.selectedItem);
                        }

                        // close overlay
                        overlayService.close();
                    },
                    close: function() {
                        overlayService.close();
                    }
                };

                overlayService.open(dialog);
            }

            function removeChild($index) {
                vm.selectedChildren.splice($index, 1);
            }

        }

		angular.module("umbraco").controller("My.Controller", Controller);

	})();
</pre>

@param {array} selectedChildren (<code>binding</code>): Array of selected children.
@param {array} availableChildren (<code>binding</code>: Array of items available for selection.
@param {string} parentName (<code>binding</code>): The parent name.
@param {string} parentIcon (<code>binding</code>): The parent icon.
@param {number} parentId (<code>binding</code>): The parent id.
@param {callback} onRemove (<code>binding</code>): Callback when removing an item.
    <h3>The callback returns:</h3>
    <ul>
        <li><code>child</code>: The selected item.</li>
        <li><code>$index</code>: The selected item index.</li>
    </ul>
@param {callback} onAdd (<code>binding</code>): Callback when adding an item.
    <h3>The callback returns:</h3>
    <ul>
        <li><code>$event</code>: The select event.</li>
    </ul>
@param {callback} onSort (<code>binding</code>): Callback when sorting an item.
    <h3>The callback returns:</h3>
    <ul>
        <li><code>$event</code>: The select event.</li>
    </ul>
**/

(function() {
    'use strict';

    function ChildSelectorDirective(overlayService, localizationService) {

        function link(scope, el, attr, ctrl) {

            var eventBindings = [];
            scope.dialogModel = {};
            scope.showDialog = false;

            scope.removeChild = (selectedChild, $index, event) => {
               const dialog = {
                    view: "views/components/overlays/umb-template-remove-confirm.html",
                    layout: selectedChild,
                    submitButtonLabelKey: "defaultdialogs_yesRemove",
                    submitButtonStyle: "danger",
                    submit: function () {
                        if(scope.onRemove) {
                            scope.onRemove(selectedChild, $index);
                            overlayService.close();
                        }
                    },
                    close: function () {
                        overlayService.close();
                    }
                };

                localizationService.localize("general_delete").then(value => {
                    dialog.title = value;
                    overlayService.open(dialog);
                });

                event.preventDefault();
                event.stopPropagation();
            };

            scope.addChild = $event => {
               if (scope.onAdd) {
                  scope.onAdd($event);
               }
            };

            function syncParentName() {

              // update name on available item
              Utilities.forEach(scope.availableChildren, availableChild => {
                if (availableChild.id === scope.parentId) {
                   availableChild.name = scope.parentName;
                }
              });

              // update name on selected child
              Utilities.forEach(scope.selectedChildren, selectedChild => {
                if (selectedChild.id === scope.parentId) {
                   selectedChild.name = scope.parentName;
                }
              });

            }

            function syncParentIcon() {

              // update icon on available item
              Utilities.forEach(scope.availableChildren, availableChild => {
                if (availableChild.id === scope.parentId) {
                   availableChild.icon = scope.parentIcon;
                }
              });

              // update icon on selected child
              Utilities.forEach(scope.selectedChildren, selectedChild => {
                if (selectedChild.id === scope.parentId) {
                   selectedChild.icon = scope.parentIcon;
                }
              });

            }

            eventBindings.push(scope.$watch('parentName', function(newValue, oldValue){

              if (newValue === oldValue) { return; }
              if (oldValue === undefined || newValue === undefined) { return; }

              syncParentName();
            }));

            eventBindings.push(scope.$watch('parentIcon', function(newValue, oldValue){

              if (newValue === oldValue) { return; }
              if (oldValue === undefined || newValue === undefined) { return; }

              syncParentIcon();
            }));

            // sortable options for allowed child content types
            scope.sortableOptions = {
                axis: "y",
                cancel: ".unsortable",
                containment: "parent",
                distance: 10,
                opacity: 0.7,
                tolerance: "pointer",
                scroll: true,
                zIndex: 6000,
                update: function (e, ui) {
                    if(scope.onSort) {
                       scope.onSort();
                    }
                }
            };

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
                onAdd: "=",
                onSort: "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbChildSelector', ChildSelectorDirective);

})();
