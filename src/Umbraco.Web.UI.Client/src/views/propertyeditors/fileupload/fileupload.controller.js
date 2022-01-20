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


        $scope.fileExtensionsString = $scope.model.config.fileExtensions ? $scope.model.config.fileExtensions.map(x => "."+x.value).join(",") : "";

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
                segment: $scope.model.segment,
                files: []
            });
        }

    };

    angular.module("umbraco")
        .controller('Umbraco.PropertyEditors.FileUploadController', fileUploadController)
        .run(function (mediaHelper) {
            if (mediaHelper && mediaHelper.registerFileResolver) {

                //NOTE: The 'entity' can be either a normal media entity or an "entity" returned from the entityResource
                // they contain different data structures so if we need to query against it we need to be aware of this.
                mediaHelper.registerFileResolver("Umbraco.UploadField", function (property, entity, thumbnail) {
                    if (thumbnail) {
                        if (mediaHelper.detectIfImageByExtension(property.value)) {
                            //get default big thumbnail from image processor
                            var thumbnailUrl = property.value + "?width=500&rnd=" + moment(entity.updateDate).format("YYYYMMDDHHmmss");
                            return thumbnailUrl;
                        }
                        else {
                            return null;
                        }
                    }
                    else {
                        return property.value;
                    }
                });

            }
        });


})();
