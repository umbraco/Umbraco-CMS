angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.Grid.MediaController",
    function ($scope, $rootScope, $timeout, dialogService) {

        $scope.setImage = function(){

            dialogService.mediaPicker({
                startNodeId: $scope.control.editor.config && $scope.control.editor.config.startNodeId ? $scope.control.editor.config.startNodeId : undefined,
                multiPicker: false,
                cropSize:  $scope.control.editor.config && $scope.control.editor.config.size ? $scope.control.editor.config.size : undefined,
                showDetails: true,
                callback: function (data) {

                    $scope.control.value = {
                        focalPoint: data.focalPoint,
                        id: data.id,
                        image: data.image
                    };

                    $scope.setUrl();
                }
            });
        };

        $scope.setUrl = function(){

            if($scope.control.value.image){
                var url = $scope.control.value.image;

                if($scope.control.editor.config && $scope.control.editor.config.size){
                    url += "?width=" + $scope.control.editor.config.size.width;
                    url += "&height=" + $scope.control.editor.config.size.height;

                    if($scope.control.value.focalPoint){
                        url += "&center=" + $scope.control.value.focalPoint.top +"," + $scope.control.value.focalPoint.left;
                        url += "&mode=crop";
                    }
                }

                $scope.url = url;
            }
        };

        $timeout(function(){
            if($scope.control.$initializing){
                $scope.setImage();
            }else{
                $scope.setUrl();
            }
        }, 200);
});
