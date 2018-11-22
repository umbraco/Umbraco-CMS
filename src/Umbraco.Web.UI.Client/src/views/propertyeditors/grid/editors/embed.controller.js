angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.EmbedController",
    function ($scope, $rootScope, $timeout) {

    	$scope.setEmbed = function(){
            $scope.embedDialog = {};
            $scope.embedDialog.view = "embed";
            $scope.embedDialog.show = true;

            $scope.embedDialog.submit = function(model) {
                $scope.control.value = model.embed.preview;
                $scope.embedDialog.show = false;
                $scope.embedDialog = null;
            };

            $scope.embedDialog.close = function(oldModel) {
                $scope.embedDialog.show = false;
                $scope.embedDialog = null;
            };

    	};

    	$timeout(function(){
    		if($scope.control.$initializing){
    			$scope.setEmbed();
    		}
    	}, 200);
});
