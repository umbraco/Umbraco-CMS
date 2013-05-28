'use strict';

//requires namespaceMgr
define(['namespaceMgr'], function () {
    
    Umbraco.Sys.registerNamespace("MyPackage.PropertyEditors");

    MyPackage.PropertyEditors.FileUploadEditor = function ($scope) {        
        $scope.$on("fileSelected", function(event, args) {            
            $scope.$apply(function() {
                //assign the file name to the model property
                $scope.model.value = args.file.name;
                //save the file object to the scope's files collection
                $scope.files.push({ id: $scope.model.id, file: args.file });
            });
        });        

    };
    
});

function Ctrl($scope, $http) {

    //a simple model to bind to and send to the server
    $scope.model = {
        name: "",
        comments: ""
    };

    //an array of files selected
    $scope.files = [];

    //listen for the file selected event
    $scope.$on("fileSelected", function (event, args) {
        $scope.$apply(function () {            
            //add the file object to the scope's files collection
            $scope.files.push(args.file);
        });
    });
    
    //the save method
    $scope.save = function() {
        $http({
            method: 'POST',
            url: "/Api/SaveStuff",
            //IMPORTANT!!! You might think this should be set to 'multipart/form-data' 
            // but this is not true because when we are sending up files the request 
            // needs to include a 'boundary' parameter which identifies the boundary 
            // name between parts in this multi-part request and setting the Content-type 
            // manually will not set this boundary parameter. For whatever reason, 
            // setting the Content-type to 'false' will force the request to automatically
            // populate the headers properly including the boundary parameter.
            headers: { 'Content-Type': false },
            //This method will allow us to change how the data is sent up to the server
            // for which we'll need to encapsulate the model data in 'FormData'
            transformRequest: function (data) {
                var formData = new FormData();
                //need to convert our json object to a string version of json otherwise
                // the browser will do a 'toString()' on the object which will result 
                // in the value '[Object object]' on the server.
                formData.append("model", angular.toJson(data.model));
                //now add all of the assigned files
                for (var i = 0; i < data.files; i++) {
                    //add each file to the form data and iteratively name them
                    formData.append("file" + i, data.files[i]);
                }
                return formData;
            },
            //Create an object that contains the model and files which will be transformed
            // in the above transformRequest method
            data: { model: $scope.model, files: $scope.files }
        }).
        success(function (data, status, headers, config) {
            alert("success!");
        }).
        error(function (data, status, headers, config) {
            alert("failed!");
        });
    };
};