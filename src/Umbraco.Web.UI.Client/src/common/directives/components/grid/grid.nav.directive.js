(function (angular) {
    "use strict";

    angular.module("umbraco").directive("umbGridNav", [
        function () {
            return {
                template:
                    '<div class="umb-grid-nav">' +
                    '<div class="container-fluid">' +
                    '<div class="row">' +
                    '<div ng-class="getClass(section)" ng-repeat="section in model.value.sections">' +
                    '<div class="row" ng-repeat="row in section.rows">' +
                    '<div ng-class="getClass(area)" class="area" ng-repeat="area in row.areas">' +
                    '<div class="area-gutter" ng-class="{active:area.active}" ng-click="goTo(area)"></div>' +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>',
                replace: true,
                restrict: "E",
                controller: ["$scope", function (scope) {
                    scope.getClass = function (column) {
                        return "col-xs-" + column.grid;
                    }
                }],
                link: function (scope, element, attrs) {
                    var grid = element.closest(".umb-grid");

                    scope.goTo = function(area) {
                        var areaCell = $.grep($(".umb-cell", grid), function(cell) {
                            return angular.element(cell).scope().area === area;
                        })[0];
                        if (areaCell) {
                            areaCell.scrollIntoView();
                        }
                    }

                }
            }
        }
    ]);

}(angular));