function RowConfigController($scope) {
    
    $scope.currentRow = angular.copy($scope.dialogOptions.currentRow);
    $scope.editors = $scope.dialogOptions.editors;
    $scope.columns = $scope.dialogOptions.columns;

    $scope.scaleUp = function(section, max, overflow) {
        var add = 1;
        if (overflow !== true) {
            add = (max > 1) ? 1 : max;
        }
        //var add = (max > 1) ? 1 : max;
        section.grid = section.grid + add;
    };

    $scope.scaleDown = function(section) {
        var remove = (section.grid > 1) ? 1 : section.grid;
        section.grid = section.grid - remove;
    };

    $scope.percentage = function(spans) {
        return ((spans / $scope.columns) * 100).toFixed(1);
    };

    $scope.toggleCollection = function(collection, toggle) {
        if (toggle) {
            collection = [];
        }
        else {
            delete collection;
        }
    };


    /****************
        area
    *****************/
    $scope.configureCell = function(cell, row) {
        if ($scope.currentCell && $scope.currentCell === cell) {
            delete $scope.currentCell;
        }
        else {
            if (cell === undefined) {
                var available = $scope.availableRowSpace;
                var space = 4;

                if (available < 4 && available > 0) {
                    space = available;
                }

                cell = {
                    grid: space
                };

                row.areas.push(cell);
            }
            $scope.currentCell = cell;
        }
    };

    $scope.deleteArea = function(index) {
        $scope.currentRow.areas.splice(index, 1);
    };
    $scope.closeArea = function() {
        $scope.currentCell = undefined;
    };

    $scope.nameChanged = false;
    var originalName = $scope.currentRow.name;
    $scope.$watch("currentRow", function(row) {
        if (row) {

            var total = 0;
            _.forEach(row.areas, function(area) {
                total = (total + area.grid);
            });

            $scope.availableRowSpace = $scope.columns - total;

            if (originalName) {
                if (originalName != row.name) {
                    $scope.nameChanged = true;
                }
                else {
                    $scope.nameChanged = false;
                }
            }
        }
    }, true);

    $scope.complete = function () {
        angular.extend($scope.dialogOptions.currentRow, $scope.currentRow);
        $scope.close();
    }
}

angular.module("umbraco").controller("Umbraco.PropertyEditors.GridPrevalueEditor.RowConfigController", RowConfigController);