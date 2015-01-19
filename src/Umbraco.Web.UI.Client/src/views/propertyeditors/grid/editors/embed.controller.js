angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.EmbedController",
    function ($scope, $rootScope, $timeout, dialogService) {

    	$scope.setEmbed = function(){
    		dialogService.embedDialog({
                    callback: function (data) {
                        $scope.control.value = data;
                    }
                });
    	};

    	$timeout(function(){
    		if($scope.control.$initializing){
    			$scope.setEmbed();
    		}
    	}, 200);
});

