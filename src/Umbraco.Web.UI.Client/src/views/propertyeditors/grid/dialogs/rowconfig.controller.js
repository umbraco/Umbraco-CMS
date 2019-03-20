function RowConfigController($scope, localizationService) {

    function init() {
        setTitle();
    }

    function setTitle() {
        if (!$scope.model.title) {
            localizationService.localize("grid_addRowConfiguration")
                .then(function(data){
                    $scope.model.title = data;
                });
        }
    }
    
    
    $scope.currentRow = $scope.model.currentRow;
    $scope.editors = $scope.model.editors;
    $scope.columns = $scope.model.columns;

    $scope.scaleUp = function(section, max, overflow) {
        var add = 1;
        if (overflow !== true) {
            add = (max > 1) ? 1 : max;
        }
        //var add = (max > 1) ? 1 : max;
        section.grid = section.grid + add;
    };

    $scope.scaleDown = function(section) {
        var remove = (section.grid > 1) ? 1 : 0;
        section.grid = section.grid - remove;
    };

    $scope.percentage = function(spans) {
        return ((spans / $scope.columns) * 100).toFixed(8);
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
            $scope.currentCell.allowAll = cell.allowAll || !cell.allowed || !cell.allowed.length;
        }
    };

    $scope.toggleAllowed = function (cell) {
        if (cell.allowed) {
            delete cell.allowed;
        }
        else {
            cell.allowed = [];
        }
    }

    $scope.deleteArea = function (cell, row) {
    	if ($scope.currentCell === cell) {
    		$scope.currentCell = undefined;
    	}
    	var index = row.areas.indexOf(cell)
    	row.areas.splice(index, 1);
    };

    $scope.closeArea = function() {
        $scope.currentCell = undefined;
    };
    
    $scope.close = function() {
        if($scope.model.close) {
            $scope.model.close();
        }
    }

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

    
    init();
    

}

angular.module("umbraco").controller("Umbraco.PropertyEditors.GridPrevalueEditor.RowConfigController", RowConfigController);
