'use strict';

define(['namespaceMgr'], function () {
    
    Umbraco.Sys.registerNamespace("MyPackage.PropertyEditors");

    MyPackage.PropertyEditors.PostcodeEditor = function ($scope, $http, $filter) {
        //change the config json model into something usable
        $scope.model.config = { country : $scope.model.config[0] };
    };
});