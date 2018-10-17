angular.module("umbraco").controller("Umbraco.PrevalueEditors.CropSizesController",
	function ($scope) {

	    if (!$scope.model.value) {
	        $scope.model.value = [];
	    }

        $scope.editMode = false;

	    $scope.remove = function (item, evt) {
	        evt.preventDefault();
	        $scope.model.value = _.reject($scope.model.value, function (x) {
	            return x.alias === item.alias;
	        });
	    };

	    $scope.edit = function (item, evt) {
            evt.preventDefault();
            $scope.editMode = true;

	        $scope.newItem = item;
	    };

	    $scope.cancel = function (evt) {
            evt.preventDefault();
            $scope.editMode = false;

	        $scope.newItem = null;
	    };

	    $scope.add = function (evt) {
            evt.preventDefault();

            $scope.editMode = false;

	        if ($scope.newItem && $scope.newItem.alias &&
                angular.isNumber($scope.newItem.width) && angular.isNumber($scope.newItem.height) &&
                $scope.newItem.width > 0 && $scope.newItem.height > 0) {

                var exists = _.find($scope.model.value, function (item) { return $scope.newItem.alias === item.alias; });

	            if (!exists) {
	                $scope.model.value.push($scope.newItem);
	                $scope.newItem = {};
	                $scope.hasError = false;
	                return;
                }
                else{
                    $scope.newItem = null;
                }
	        }

	        //there was an error, do the highlight (will be set back by the directive)
	        $scope.hasError = true;
	    };
	});
