angular.module("umbraco").controller("Umbraco.Overlays.MediaTypePickerController",
	function ($scope) {

		$scope.select = function(mediatype){
			$scope.model.selectedType = mediatype;
			$scope.model.submit($scope.model);
			$scope.model.show = false;
		}
	    
	});
