/**
 * @ngdoc controller
 * @name Umbraco.Editors.DataType.EditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function DataTypeEditController($scope, $routeParams, $location, dataTypeResource, notificationsService, angularHelper, serverValidationManager, contentEditingHelper) {

    //method used to configure the pre-values when we retreive them from the server
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

    //set up the standard data type props
    $scope.properties = {
        selectedEditor: {
            alias: "selectedEditor",
            description: "Select a property editor",
            label: "Property editor"
        },
        selectedEditorId: {
            alias: "selectedEditorId",
            label: "Property editor GUID"
        }
    };
    
    //setup the pre-values as props
    $scope.preValues = [];

    if ($routeParams.create) {
        //we are creating so get an empty content item
        dataTypeResource.getScaffold($routeParams.id, $routeParams.doctype)
            .then(function(data) {
                $scope.loaded = true;
                $scope.preValuesLoaded = true;
                $scope.content = data;
            });
    }
    else {
        //we are editing so get the content item from the server
        dataTypeResource.getById($routeParams.id)
            .then(function(data) {
                $scope.loaded = true;
                $scope.preValuesLoaded = true;
                $scope.content = data;
                createPreValueProps($scope.content.preValues);
                
                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                serverValidationManager.executeAndClearAllSubscriptions();
            });
    }
    
    $scope.$watch("content.selectedEditor", function (newVal, oldVal) {
        //when the value changes, we need to dynamically load in the new editor
        if (newVal !== null && newVal !== undefined && newVal != oldVal) {
            //we are editing so get the content item from the server
            var currDataTypeId = $routeParams.create ? undefined : $routeParams.id;
            dataTypeResource.getPreValues(newVal, currDataTypeId)
                .then(function (data) {
                    $scope.preValuesLoaded = true;
                    $scope.content.preValues = data;
                    createPreValueProps($scope.content.preValues);
                });
        }
    });

    $scope.save = function () {
        $scope.$broadcast("saving", { scope: $scope });
    
        //ensure there is a form object assigned.
        var currentForm = angularHelper.getRequiredCurrentForm($scope);

        //don't continue if the form is invalid
        if (currentForm.$invalid) return;

        serverValidationManager.reset();
        
        dataTypeResource.save($scope.content, $scope.preValues, $routeParams.create)
            .then(function (data) {
                
                contentEditingHelper.handleSuccessfulSave({
                    scope: $scope,
                    newContent: data,
                    rebindCallback: function() {
                        createPreValueProps(data.preValues);
                    }
                });

            }, function (err) {
                
                //NOTE: in the case of data type values we are setting the orig/new props 
                // to be the same thing since that only really matters for content/media.
                contentEditingHelper.handleSaveError({
                    err: err,
                    allNewProps: $scope.preValues,
                    allOrigProps: $scope.preValues
                });
        });
    };

}

angular.module("umbraco").controller("Umbraco.Editors.DataType.EditController", DataTypeEditController);