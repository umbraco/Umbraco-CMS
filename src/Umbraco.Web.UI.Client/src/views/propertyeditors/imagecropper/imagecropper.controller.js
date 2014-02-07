//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it
angular.module('umbraco').controller("Umbraco.PropertyEditors.ImageCropperController",
    function($rootScope, $scope, mediaHelper, $timeout, editorState) {

        //check the pre-values for multi-picker
        var multiPicker = $scope.model.config.multiPicker !== '0' ? true : false;
        
        //used to reference the part of the data we will either crop or "point"
        $scope.currentCrop = undefined;
        $scope.currentPoint = undefined;

        var imgPath = mediaHelper.getImagePropertyValue({imageModel: editorState.current});

        $scope.crop = function(crop){
            $scope.currentCrop = crop;
            $scope.currentPoint = undefined;
        };

        $scope.done = function(){
            $scope.currentCrop = undefined;
            $scope.currentPoint = undefined;
        };


        //Data sample
        if(!$scope.model.value){
            $scope.model.value = {
                //image to crops
                src: imgPath,

                //global intrestpoint, used if no crop is specified
                center: {left: 0.5, top: 0.4},
                crops:{
                    thumbnail: 
                    {   
                        //crop dimensions
                        width: 100,
                        height: 100,

                        //crops in percentages
                        crop:{ 
                            "x1": 0.31731772342645215,
                            "y1": 0.17420325244997603,
                            "x2": -0.36246473116627076,
                            "y2": -0.30226197981593617
                        }
                    },
                    highrise:
                    {
                        width: 90,
                        height: 340
                    }     
                }
            };
        }        
});