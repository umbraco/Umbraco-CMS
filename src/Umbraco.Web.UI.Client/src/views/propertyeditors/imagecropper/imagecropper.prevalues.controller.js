angular.module("umbraco").controller("Umbraco.PrevalueEditors.CropSizesController",
	function ($scope, $timeout, localizationService) {

	    if (!$scope.model.value) {
	        $scope.model.value = [];
		}
		$scope.setDisplayName = function (crop) {
		  if (crop.name.startsWith("@")) {
		    localizationService.localize(crop.name.substring(1)).then(function (value) {
		      if (value !== "") {
		        crop.displayName = value;
		      } else {
		        crop.displayName = crop.name;
		      }
		    });
		  } else {
		    crop.displayName = crop.name;
		  }
	  };
	    _.each($scope.model.value, function (crop) {
	      $scope.setDisplayName(crop);
	    });
	  $scope.remove = function (item, evt) {
	        evt.preventDefault();
	        $scope.model.value = _.reject($scope.model.value, function (x) {
	            return x.alias === item.alias;
	        });
	    };

	    $scope.edit = function (item, evt) {
	        evt.preventDefault();
	        $scope.newItem = item;
	    };

	    $scope.cancel = function (evt) {
	        evt.preventDefault();
	        $scope.newItem = null;
	    };

	    $scope.add = function (evt) {
	        evt.preventDefault();

	        if ($scope.newItem && $scope.newItem.name && $scope.newItem.alias && 
             angular.isNumber($scope.newItem.width) && angular.isNumber($scope.newItem.height) &&
	           $scope.newItem.width > 0 && $scope.newItem.height > 0) {
	              $scope.setDisplayName($scope.newItem);
	            var exists = _.find($scope.model.value, function (item) { return $scope.newItem.alias === item.alias; });
	            if (!exists) {
	                $scope.model.value.push($scope.newItem);
	                $scope.newItem = {};
	                $scope.hasError = false;
	                return;
	            }
	        }

	        //there was an error, do the highlight (will be set back by the directive)
	        $scope.hasError = true;
	    };
	});