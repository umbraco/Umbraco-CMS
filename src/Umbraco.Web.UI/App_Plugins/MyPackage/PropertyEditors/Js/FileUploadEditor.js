'use strict';

(function() {
    function fileUploadEditorController($scope, $element, $compile, umbImageHelper) {

        /** Clears the file collections when content is saving (if we need to clear) or after saved */
        function clearFiles() {
            //TODO: There should be a better way! We don't want to have to know about the parent scope
            //clear the parent files collection (we don't want to upload any!)
            $scope.$parent.addFiles($scope.model.id, []);
            //clear the current files
            $scope.files = [];
        }

        //clear the current files
        $scope.files = [];
        //create the property to show the list of files currently saved
        if ($scope.model.value != "") {

            //for legacy data, this will not be an array, just a string so convert to an array
            if (!$scope.model.value.startsWith('[')) {

                //check if it ends with a common image extensions
                var isImage = umbImageHelper.detectIfImageByExtension($scope.model.value);
                $scope.model.value = "[{\"file\": \"" + $scope.model.value + "\",\"isImage\":" + isImage + "}]";
            }

            $scope.persistedFiles = angular.fromJson($scope.model.value);
        }
        else {
            $scope.persistedFiles = [];
        }

        _.each($scope.persistedFiles, function (file) {
            file.thumbnail = umbImageHelper.getThumbnailFromPath(file.file);
        });


        $scope.clearFiles = false;

        //listen for clear files changes to set our model to be sent up to the server
        $scope.$watch("clearFiles", function (isCleared) {
            if (isCleared == true) {
                $scope.model.value = "{clearFiles: true}";
                clearFiles();
            }
            else {
                $scope.model.value = "";
            }
        });

        //listen for when a file is selected
        $scope.$on("filesSelected", function (event, args) {
            $scope.$apply(function () {
                //set the parent files collection
                $scope.$parent.addFiles($scope.model.id, args.files);
                //clear the current files
                $scope.files = [];
                for (var i = 0; i < args.files.length; i++) {
                    //save the file object to the scope's files collection
                    $scope.files.push({ id: $scope.model.id, file: args.files[i] });
                }
                //set clear files to false, this will reset the model too
                $scope.clearFiles = false;
            });
        });

    };
    
    angular.module("myPackage.controllers").controller('MyPackage.PropertyEditors.FileUploadEditor', fileUploadEditorController);
})();