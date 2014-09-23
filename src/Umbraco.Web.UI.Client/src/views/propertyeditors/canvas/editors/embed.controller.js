angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Canvas.EmbedController",
    function ($scope, $rootScope, $timeout, dialogService) {

    	$scope.setEmbed = function(){
    		dialogService.embedDialog({
                    callback: function (data) {
                        $scope.control.value = data;
                    }
                });
    	};

    	$timeout(function(){
    		if($scope.control.value === null){
    			$scope.setEmbed();
    		}
    	}, 200);
});
