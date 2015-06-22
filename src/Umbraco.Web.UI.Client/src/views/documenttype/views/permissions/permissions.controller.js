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
        });

    }

    $scope.removeAllowedChildNode = function(selectedContentType) {

        // splice from array
        var selectedContentTypeIndex = $scope.contentType.allowedContentTypes.indexOf(selectedContentType);
        $scope.contentType.allowedContentTypes.splice(selectedContentTypeIndex, 1);

    };

    $scope.addItemOverlay = function ($event) {

        $scope.showDialog = false;
        $scope.dialogModel = {};
        $scope.dialogModel.title = "Choose content type";
        $scope.dialogModel.contentTypes = $scope.contentTypes;
        $scope.dialogModel.allowedContentTypes = $scope.contentType.allowedContentTypes;
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
