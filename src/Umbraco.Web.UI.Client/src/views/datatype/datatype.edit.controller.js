/**
 * @ngdoc controller
 * @name Umbraco.Editors.DataType.EditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function DataTypeEditController($scope, $routeParams, $location, dataTypeResource, notificationsService, angularHelper, serverValidationManager, contentEditingHelper) {

    //set up the standard data type props
    function createDisplayProps() {
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
    }
    
    //setup the pre-values as props
    function createPreValueProps(preVals) {
        $scope.preValues = [];
        for (var i = 0; i < preVals.length; i++) {
            $scope.preValues.push({
                hideLabel: preVals[i].hideLabel,
                alias: preVals[i].key,
                description: preVals[i].description,
                label: preVals[i].label,
                view: preVals[i].view,
            });
        }
    }

    if ($routeParams.create) {
        //we are creating so get an empty content item
        dataTypeResource.getScaffold($routeParams.id, $routeParams.doctype)
            .then(function(data) {
                $scope.loaded = true;
                $scope.content = data;
                createDisplayProps();
            });
    }
    else {
        //we are editing so get the content item from the server
        dataTypeResource.getById($routeParams.id)
            .then(function(data) {
                $scope.loaded = true;
                $scope.content = data;
                createDisplayProps();
                createPreValueProps($scope.content.preValues);
                
                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                serverValidationManager.executeAndClearAllSubscriptions();
            });
    }
    
    //ensure there is a form object assigned.
    var currentForm = angularHelper.getRequiredCurrentForm($scope);

    //TODO: We need to handle the dynamic loading of the pre-value editor view whenever the drop down changes!
    
    $scope.save = function (cnt) {
        $scope.$broadcast("saving", { scope: $scope });
            
        //don't continue if the form is invalid
        if (currentForm.$invalid) return;

        serverValidationManager.reset();

        dataTypeResource.save(cnt, $routeParams.create)
            .then(function (data) {
                
                //TODO: SD: I need to finish this on monday!
                alert("Woot!");

            }, function (err) {
                contentEditingHelper.handleSaveError(err, $scope);
        });
    };

}

angular.module("umbraco").controller("Umbraco.Editors.DataType.EditController", DataTypeEditController);