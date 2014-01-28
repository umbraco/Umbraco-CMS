//used for the icon picker dialog
angular.module("umbraco")
    .controller("Umbraco.Dialogs.IconPickerController",
        function ($scope, iconHelper) {
            
            iconHelper.getIcons().then(function(icons){
            	$scope.icons = icons;
            });

			$scope.submitClass = function(icon){
				if($scope.color)
				{
					$scope.submit(icon + " " + $scope.color);
				}else{
					$scope.submit(icon);	
				}
			};
		}
	);