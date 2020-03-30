(function () {
    'use strict';

    /**
     * @ngdoc controller
     * @name Umbraco.Editors.FileUploadController
     * @function
     *
     * @description
     * The controller for the file upload property editor.
     *
    */
    function fileUploadController($scope, fileManager) {
        
        $scope.fileChanged = onFileChanged;
        //declare a special method which will be called whenever the value has changed from the server
        $scope.model.onValueChanged = onValueChanged;

        /**
         * Called when the file selection value changes
         * @param {any} value
         */
        function onFileChanged(value) {
            $scope.model.value = value;
        }

        /**
         * called whenever the value has changed from the server
         * @param {any} newVal
         * @param {any} oldVal
         */
        function onValueChanged(newVal, oldVal) {
            //clear current uploaded files
            fileManager.setFiles({
                propertyAlias: $scope.model.alias,
                culture: $scope.model.culture,
                files: []
            });
        }
        
    };

    angular.module("umbraco")
        .controller('Umbraco.PropertyEditors.FileUploadController', fileUploadController);
})();
