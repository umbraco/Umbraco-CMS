angular.module("umbraco").controller("Umbraco.PrevalueEditors.CropSizesController",
	function ($scope) {

	    if (!$scope.model.value) {
	        $scope.model.value = [];
	    }

        $scope.editMode = false;
        $scope.setFocus = false;

	    $scope.remove = function (item, evt) {
	        evt.preventDefault();
	        $scope.model.value = _.reject($scope.model.value, function (x) {
	            return x.alias === item.alias;
	        });
	    };

	    $scope.edit = function (item, evt) {
            evt.preventDefault();
            $scope.editMode = true;
            $scope.setFocus = false;

	        $scope.newItem = item;
	    };

	    $scope.cancel = function (evt) {
            evt.preventDefault();
            $scope.editMode = false;
            $scope.setFocus = true;

	        $scope.newItem = null;
	    };

        $scope.change = function () {
            // Listen to the change event and set focus 2 false
            if ($scope.setFocus) {
                $scope.setFocus = false;
                return;
            }
        };

	    $scope.add = function (evt) {
            evt.preventDefault();

            $scope.editMode = false;
            $scope.setFocus = true;

	        if ($scope.newItem && $scope.newItem.alias &&
                Utilities.isNumber($scope.newItem.width) && Utilities.isNumber($scope.newItem.height) &&
                $scope.newItem.width > 0 && $scope.newItem.height > 0) {

                var exists = _.find($scope.model.value, function (item) { return $scope.newItem.alias === item.alias; });

	            if (!exists) {
	                $scope.model.value.push($scope.newItem);
	                $scope.newItem = {};
                    $scope.hasError = false;
                    $scope.cropAdded = false;
	                return;
                }
                else{
                    $scope.newItem = null;
                    $scope.hasError = false;
                    return;
                }
	        }

	        //there was an error, do the highlight (will be set back by the directive)
	        $scope.hasError = true;
        };

        $scope.createNew = function (event) {
            if (event.keyCode == 13) {
                $scope.add(event);
            }
        };

        $scope.sortableOptions = {
            axis: 'y',
            containment: 'parent',
            cursor: 'move',
            tolerance: 'pointer'
        };

	});
