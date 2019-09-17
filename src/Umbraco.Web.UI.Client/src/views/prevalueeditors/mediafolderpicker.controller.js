function mediaFolderPickerController($scope, editorService, entityResource) {
    
    
    $scope.folderName = "";
    
    
    function retriveFolderData() {
        
        var id = $scope.model.value;
        
        if (id == null) {
            $scope.folderName = "";
            return;
        }
        
        entityResource.getById(id, "Media").then(
            function (media) {
                $scope.media = media;
            }
        );
    }
    
    
    retriveFolderData();
    
    
    $scope.add = function() {
        var mediaPickerOptions = {
            view: "mediapicker",
            multiPicker: true,
            disableFolderSelect: false,
            onlyImages: false,
            onlyFolders: true,
            submit: function (model) {
                
                $scope.model.value = model.selection[0].udi;
                
                retriveFolderData();
                
                editorService.close();
            },
            close: function () {
                editorService.close();
            } 
        };
        editorService.mediaPicker(mediaPickerOptions);
    };

    $scope.remove = function () {
        $scope.model.value = null;
        retriveFolderData();
    };

}

angular.module('umbraco').controller("Umbraco.PrevalueEditors.MediaFolderPickerController", mediaFolderPickerController);
