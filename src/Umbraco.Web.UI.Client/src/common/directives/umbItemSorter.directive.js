/**
* @ngdoc directive
* @name umbraco.directives.directive:umbItemSorter
* @function
* @element ANY
* @restrict E
* @description A re-usable directive for sorting items
**/
function umbItemSorter(angularHelper) {
    return {
        scope: {
            model: "="
        },
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: 'views/directives/umb-item-sorter.html',
        link: function(scope, element, attrs, ctrl) {
            var defaultModel = {
                okButton: "Ok",
                successMsg: "Sorting successful",
                complete: false
            };
            //assign user vals to default
            angular.extend(defaultModel, scope.model);
            //re-assign merged to user
            scope.model = defaultModel;

            scope.performSort = function() {
                scope.$emit("umbItemSorter.sorting", {
                    sortedItems: scope.model.itemsToSort
                });
            };

            scope.handleCancel = function () {
                scope.$emit("umbItemSorter.cancel");
            };

            scope.handleOk = function() {
                scope.$emit("umbItemSorter.ok");
            };
            
            //defines the options for the jquery sortable
            scope.sortableOptions = {
                axis: 'y',
                cursor: "move",
                placeholder: "ui-sortable-placeholder",
                update: function (ev, ui) {
                    //highlight the item when the position is changed
                    $(ui.item).effect("highlight", { color: "#049cdb" }, 500);
                },
                stop: function (ev, ui) {
                    //the ui-sortable directive already ensures that our list is re-sorted, so now we just
                    // need to update the sortOrder to the index of each item
                    angularHelper.safeApply(scope, function () {
                        angular.forEach(scope.itemsToSort, function (val, index) {
                            val.sortOrder = index + 1;
                        });

                    });
                }
            };
        }
    };
}

angular.module('umbraco.directives').directive("umbItemSorter", umbItemSorter);
