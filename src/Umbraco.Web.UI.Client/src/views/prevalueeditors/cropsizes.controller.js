angular.module("umbraco").controller("Umbraco.PrevalueEditors.CropSizesController",
	function ($scope, $timeout) {

		if(!$scope.model.value){
			$scope.model.value = [];
		}

		$scope.remove = function(item, evt) {
			evt.preventDefault();
			$scope.model.value = _.reject($scope.model.value, function (x) {
				return x.alias === item.alias;
			});
		};

		$scope.edit = function(item, evt) {
			evt.preventDefault();
			$scope.newItem = item;
		};

		$scope.cancel = function(evt) {
			evt.preventDefault();
			$scope.newItem = null;
		};

		$scope.add = function (evt) {
			evt.preventDefault();

			if ($scope.newItem) {
				var exists = _.find($scope.model.value, function(item){return $scope.newItem.alias ===  item.alias;});
				if (!exists){                
					$scope.model.value.push($scope.newItem);
					$scope.newItem = {};
					$scope.hasError = false;
					return;
				}
			
			$scope.newItem = null;
			//there was an error, do the highlight (will be set back by the directive)
			$scope.hasError = true;
			}
		};
    });