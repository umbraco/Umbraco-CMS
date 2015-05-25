/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.PropertyController
 * @function
 *
 * @description
 * The controller for the content type editor property dialog
 */
function EditDataTypeController($scope, dataTypeResource) {


    editDataType($scope.model.property.dataType.id);

    function editDataType(dataTypeId) {

        dataTypeResource.getById(dataTypeId)
            .then(function(data) {

                console.log(data);

                //$scope.loaded = true;
                //$scope.preValuesLoaded = true;
                //$scope.content = data;

                $scope.model.property.dataType.preValues = data.preValues;

                createPreValueProps($scope.model.property.dataType.preValues);

                //share state
                //editorState.set($scope.content);

                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                //serverValidationManager.executeAndClearAllSubscriptions();

                /*
                navigationService.syncTree({ tree: "datatype", path: [String(data.id)] }).then(function (syncArgs) {
                    $scope.currentNode = syncArgs.node;
                });
                */
            });

    }

    function createPreValueProps(preVals) {
        $scope.preValues = [];
        for (var i = 0; i < preVals.length; i++) {
            $scope.preValues.push({
                hideLabel: preVals[i].hideLabel,
                alias: preVals[i].key,
                description: preVals[i].description,
                label: preVals[i].label,
                view: preVals[i].view,
                value: preVals[i].value
            });
        }
    }


}

angular.module("umbraco").controller("Umbraco.Editors.DocumentType.EditDataTypeController", EditDataTypeController);
