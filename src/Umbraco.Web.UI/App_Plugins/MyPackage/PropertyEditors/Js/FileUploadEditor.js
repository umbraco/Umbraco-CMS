'use strict';

//requires namespaceMgr
define(['namespaceMgr'], function () {
    
    Umbraco.Sys.registerNamespace("MyPackage.PropertyEditors");

    MyPackage.PropertyEditors.FileUploadEditor = function ($scope, $element, $compile) {
        
        //create the property to show the list of files currently saved
        $scope.persistedFiles = _.map(angular.fromJson($scope.model.value), function(item) {
            return item.file;
        });

        $scope.clearFiles = false;

        //listen for clear files changes to set our model to be sent up to the server
        $scope.$watch("clearFiles", function(isCleared) {
            if (isCleared == true) {
                $scope.model.value = "{clearFiles: true}";
            }
            else {
                $scope.model.value = "";
            }
        });

        //listen for the saving event
        $scope.$on("contentSaving", function() {
            //if clear files is selected then we'll clear all the files that are about
            // to be uploaded
            if ($scope.clearFiles) {
                //clear the parent files collection (we don't want to upload any!)
                $scope.$parent.addFiles($scope.model.id, []);
                //clear the current files
                $scope.files = [];
            }
        });
        
        //listen for when a file is selected
        $scope.$on("filesSelected", function (event, args) {
            $scope.$apply(function() {
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
    
});