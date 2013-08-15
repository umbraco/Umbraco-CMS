/**
 * @ngdoc controller
 * @name Umbraco.Editors.DataType.EditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function DataTypeEditController($scope, $routeParams, $location, dataTypeResource, notificationsService, angularHelper, serverValidationManager, contentEditingHelper) {
       
    if ($routeParams.create) {
        //we are creating so get an empty content item
        dataTypeResource.getScaffold($routeParams.id, $routeParams.doctype)
            .then(function(data) {
                $scope.loaded = true;
                $scope.content = data;
            });
    }
    else {
        //we are editing so get the content item from the server
        dataTypeResource.getById($routeParams.id)
            .then(function(data) {
                $scope.loaded = true;
                $scope.content = data;
                
                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                serverValidationManager.executeAndClearAllSubscriptions();
            });
    }
    
    //ensure there is a form object assigned.
    var currentForm = angularHelper.getRequiredCurrentForm($scope);

    $scope.save = function (cnt) {
        $scope.$broadcast("saving", { scope: $scope });
            
        //don't continue if the form is invalid
        if (currentForm.$invalid) return;

        serverValidationManager.reset();

        dataTypeResource.save(cnt, $routeParams.create, $scope.files)
            .then(function (data) {
                contentEditingHelper.handleSuccessfulSave({
                    scope: $scope,
                    newContent: data
                });
            }, function (err) {
                contentEditingHelper.handleSaveError(err, $scope);
        });
    };

}

angular.module("umbraco").controller("Umbraco.Editors.DataType.EditController", DataTypeEditController);
