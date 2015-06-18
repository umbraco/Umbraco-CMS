/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */
function PermissionsController($scope, contentTypeResource, $log, iconHelper) {

    /* ----------- SCOPE VARIABLES ----------- */

    $scope.contentTypes = [];

    /* ---------- INIT ---------- */

    init();

    function init() {

        contentTypeResource.getAll().then(function(contentTypes){

            $scope.contentTypes = contentTypes;

            angular.forEach($scope.contentTypes, function(contentType){

                // convert legacy icons
                iconHelper.formatContentTypeIcons($scope.contentTypes);

                var exists = false;

                angular.forEach($scope.contentType.allowedContentTypes, function(allowedContentType){

                    if( contentType.alias === allowedContentType.alias ) {
                        exists = true;
                    }

                });

                if(exists) {
                    contentType.show = false;
                } else {
                    contentType.show = true;
                }

            });

        });

    }

    $scope.removeAllowedChildNode = function(selectedContentType) {

        // splice from array
        var selectedContentTypeIndex = $scope.contentType.allowedContentTypes.indexOf(selectedContentType);
        $scope.contentType.allowedContentTypes.splice(selectedContentTypeIndex, 1);

        // show content type in content types array
        for (var contentTypeIndex = 0; contentTypeIndex < $scope.contentTypes.length; contentTypeIndex++) {

            var contentType = $scope.contentTypes[contentTypeIndex];

            if( selectedContentType.alias === contentType.alias ) {
                contentType.show = true;
            }
        }

    };

    $scope.addItemOverlay = function ($event) {

        $scope.showDialog = false;
        $scope.dialogModel = {};
        $scope.dialogModel.title = "Choose content type";
        $scope.dialogModel.contentTypes = $scope.contentTypes;
        $scope.dialogModel.event = $event;
        $scope.dialogModel.view = "views/documentType/dialogs/contenttypes/contenttypes.html";
        $scope.showDialog = true;

        $scope.dialogModel.chooseContentType = function(selectedContentType) {

            // format content type to match service
            var reformatedContentType = {
                "name": selectedContentType.name,
                "id": {
                    "m_boxed": {
                        "m_value": selectedContentType.id
                    }
                },
                "icon": selectedContentType.icon,
                "key": selectedContentType.key,
                "alias": selectedContentType.alias
            };

            // push to content type model
            $scope.contentType.allowedContentTypes.push(reformatedContentType);

            // hide selected content type from content types array
            for (var contentTypeIndex = 0; contentTypeIndex < $scope.contentTypes.length; contentTypeIndex++) {

                var contentType = $scope.contentTypes[contentTypeIndex];

                if( selectedContentType.alias === contentType.alias ) {
                    contentType.show = false;
                }
            }

            $scope.showDialog = false;
            $scope.dialogModel = null;
        };

        $scope.dialogModel.close = function(){
            $scope.showDialog = false;
            $scope.dialogModel = null;
        };
    };

}

angular.module("umbraco").controller("Umbraco.Editors.DocumentType.PermissionsController", PermissionsController);
