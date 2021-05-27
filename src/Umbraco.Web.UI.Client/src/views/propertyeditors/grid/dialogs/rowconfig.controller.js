function RowConfigController($scope, localizationService) {

    var vm = this;

    vm.configureCell = configureCell;
    vm.closeArea = closeArea;
    vm.deleteArea = deleteArea;
    vm.selectEditor = selectEditor;
    vm.toggleAllowed = toggleAllowed;
    vm.percentage = percentage;
    vm.scaleUp = scaleUp;
    vm.scaleDown = scaleDown;
    vm.close = close;
    vm.submit = submit;

    vm.labels = {};
    
    function init() {

        $scope.currentRow = $scope.model.currentRow;
        $scope.columns = $scope.model.columns;
        $scope.editors = $scope.model.editors;
        $scope.nameChanged = false;

        var labelKeys = [
            "grid_addRowConfiguration",
            "grid_allowAllEditors"
        ];

        localizationService.localizeMany(labelKeys).then(function (data) {

            vm.labels.title = data[0];
            vm.labels.allowAllEditors = data[1];

            setTitle(vm.labels.title);
        });
    }

    function setTitle(value) {
        if (!$scope.model.title) {
            $scope.model.title = value;
        }
    }

    function scaleUp(section, max, overflow) {
        var add = 1;
        if (overflow !== true) {
            add = (max > 1) ? 1 : max;
        }
        //var add = (max > 1) ? 1 : max;
        section.grid = section.grid + add;
    };

    function scaleDown(section) {
        var remove = (section.grid > 1) ? 1 : 0;
        section.grid = section.grid - remove;
    }

    function percentage(spans) {
        return ((spans / $scope.columns) * 100).toFixed(8);
    }

    /****************
        area
    *****************/
    function configureCell(cell, row) {
        if ($scope.currentCell && $scope.currentCell === cell) {
            delete $scope.currentCell;
        }
        else {
            if (cell === null) {
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

            cell.allowed = cell.allowed || [];

            $scope.editors.forEach(function (e) { e.allowed = cell.allowed.indexOf(e.alias) !== -1 });

            cell.allowAll = cell.allowAll || !cell.allowed || !cell.allowed.length;

            $scope.currentCell = cell;
        }
    }

    function toggleAllowed(cell) {
        cell.allowAll = !cell.allowAll;

        if (cell.allowed) {
            delete cell.allowed;
        }
        else {
            cell.allowed = [];
        }
    }

    function deleteArea(cell, row) {
    	if ($scope.currentCell === cell) {
    		$scope.currentCell = null;
    	}
    	var index = row.areas.indexOf(cell)
    	row.areas.splice(index, 1);
    }

    // This doesn't seem to be used?
    function closeArea() {
        $scope.currentCell = null;
    }

    function selectEditor(cell, editor) {
        cell.allowed = cell.allowed || [];

        var index = cell.allowed.indexOf(editor.alias);
        if (editor.allowed === true) {
            if (index === -1) {
                cell.allowed.push(editor.alias);
            }
        }
        else {
            cell.allowed.splice(index, 1);
        }
    }
    
    function close () {
        if ($scope.model.close) {
            $scope.model.close();
        }
    }

    function submit() {
        if ($scope.model.submit) {
            $scope.model.submit($scope.currentRow);
        }
    }

    $scope.$watch("currentRow", function(row) {
        if (row) {

            var total = 0;
            _.forEach(row.areas, function(area) {
                total = (total + area.grid);
            });

            $scope.availableRowSpace = $scope.columns - total;

            var originalName = $scope.currentRow.name;
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
