//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco').controller("Umbraco.PropertyEditors.ImageCropperController",
    function($rootScope, $routeParams, $scope, mediaHelper, cropperHelper, $timeout, editorState, umbRequestHelper, fileManager) {
        
    cropperHelper.configuration("Image").then(function(config){
        
        //move previously saved value to the editor
        if($scope.model.value){
            //backwards compat with the old file upload (incase some-one swaps them..)
            if(angular.isString($scope.model.value)){
                config.src = $scope.model.value;
                $scope.model.value = config;
            }
            $scope.imageSrc = $scope.model.value.src;
        }

        //crop a specific crop
        $scope.crop = function(crop){
            $scope.currentCrop = crop;
            $scope.currentPoint = undefined;
        };

        //done cropping
        $scope.done = function(){
            $scope.currentCrop = undefined;
            $scope.currentPoint = undefined;
        };

        //crop a specific crop
        $scope.clear = function(crop){
            //clear current uploaded files
            fileManager.setFiles($scope.model.alias, []);
            
            //clear the ui
            $scope.imageSrc = undefined;
            if($scope.model.value.src){
                delete $scope.model.value.src;
            }
        };

        //show previews
        $scope.togglePreviews = function(){
            if($scope.showPreviews){
                $scope.showPreviews = false;
                $scope.tempShowPreviews = false;
            }else{
                $scope.showPreviews = true;    
            }
        };


        $scope.$on("imageFocalPointStart", function(){
            $scope.tempShowPreviews = true;
        });

        $scope.$on("imageFocalPointStop", function(){
            $scope.tempShowPreviews = false;
        });

        //on image selected, update the cropper
        $scope.$on("filesSelected", function (ev, args) {
            $scope.model.value = config;
            
            if(args.files && args.files[0]){

                fileManager.setFiles($scope.model.alias, args.files);

                var reader = new FileReader();
                reader.onload = function (e) {
                    $scope.$apply(function(){
                        $scope.imageSrc = e.target.result;
                    });
                };

                reader.readAsDataURL(args.files[0]);
            }
        });

    });   
});