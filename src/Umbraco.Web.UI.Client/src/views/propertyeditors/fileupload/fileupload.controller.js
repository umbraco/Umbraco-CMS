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
    function fileUploadController($scope) {
        
        $scope.valueChanged = valueChanged;

        /**
         * Called when the file selection value changes
         * @param {any} value
         */
        function valueChanged(value) {
            $scope.model.value = value;
        }
    };

    angular.module("umbraco")
        .controller('Umbraco.PropertyEditors.FileUploadController', fileUploadController)
        .run(function (mediaHelper, umbRequestHelper, assetsService) {
            if (mediaHelper && mediaHelper.registerFileResolver) {

                //NOTE: The 'entity' can be either a normal media entity or an "entity" returned from the entityResource
                // they contain different data structures so if we need to query against it we need to be aware of this.
                mediaHelper.registerFileResolver("Umbraco.UploadField", function (property, entity, thumbnail) {
                    if (thumbnail) {
                        if (mediaHelper.detectIfImageByExtension(property.value)) {
                            //get default big thumbnail from image processor
                            var thumbnailUrl = property.value + "?rnd=" + moment(entity.updateDate).format("YYYYMMDDHHmmss") + "&width=500&animationprocessmode=first";
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
