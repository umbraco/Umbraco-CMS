angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.MediaController",
    function ($scope, $rootScope, $timeout, dialogService) {

    	$scope.setImage = function(){
    	    dialogService.mediaPicker({
    	        startNodeId: $scope.control.editor.config && $scope.control.editor.config.startNodeId ? $scope.control.editor.config.startNodeId : undefined,
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

