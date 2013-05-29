'use strict';

//requires namespaceMgr
define(['namespaceMgr'], function () {
    
    Umbraco.Sys.registerNamespace("MyPackage.PropertyEditors");

    MyPackage.PropertyEditors.FileUploadEditor = function ($scope) {        
        $scope.$on("fileSelected", function(event, args) {            
            $scope.$apply(function() {
                //assign the file name to the model property
                $//scope.model.value = args.file.name;
                //save the file object to the scope's files collection
                $scope.files.push({ id: $scope.model.id, file: args.file });
            });
        });        

    };
    
});