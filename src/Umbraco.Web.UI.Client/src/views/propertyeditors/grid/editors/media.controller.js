angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.MediaController",
    function ($scope, $rootScope, $timeout, dialogService) {
    	
    	$scope.setImage = function(){
    		dialogService.mediaPicker({
    		    multiPicker: false,
    		    callback: function (data) {
    		    	$scope.control.value = {
    		                    id: data.id,
    		                    image: data.image,
    		                    thumbnail: data.thumbnail
    		                };
    		        }
    		    });
    	};

    	$timeout(function(){
    		if($scope.control.value === null){
    			$scope.setImage();
    		}
    	}, 200);
});