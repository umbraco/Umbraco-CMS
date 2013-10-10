/**
 * @ngdoc controller
 * @name Umbraco.Editors.Member.EditController
 * @function
 * 
 * @description
 * The controller for the member editor
 */
function MemberEditController($scope, $routeParams, $q, $timeout, $window, memberResource, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, fileManager, editorContextService) {
       
    //initialize the file manager
    fileManager.clearFiles();

    if ($routeParams.create) {
        //we are creating so get an empty member item
        memberResource.getScaffold($routeParams.doctype)
            .then(function(data) {
                $scope.loaded = true;
                $scope.content = data;
                editorContextService.setContext($scope.content);
            });
    }
    else {
        //we are editing so get the content item from the server
        memberResource.getByKey($routeParams.id)
            .then(function(data) {
                $scope.loaded = true;
                $scope.content = data;
                editorContextService.setContext($scope.content);

                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                serverValidationManager.executeAndClearAllSubscriptions();
            });
    }

    //TODO: Need to figure out a way to share the saving and event broadcasting with all editors!
    
    $scope.setStatus = function(status){
        //add localization
        $scope.status = status;
        $timeout(function(){
            $scope.status = undefined;
        }, 2500);
    };

    $scope.save = function () {
        var deferred = $q.defer();

        $scope.setStatus("Saving...");
        $scope.$broadcast("saving", { scope: $scope });
            
        var currentForm = angularHelper.getRequiredCurrentForm($scope);

        //don't continue if the form is invalid
        if (currentForm.$invalid) return;

        serverValidationManager.reset();

        memberResource.save($scope.content, $routeParams.create, fileManager.getFiles())
            .then(function (data) {
                
                contentEditingHelper.handleSuccessfulSave({
                    scope: $scope,
                    newContent: data,
                    //specify a custom id to redirect to since we want to use the GUID
                    redirectId: data.key,
                    rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                });

                deferred.resolve(data);
                
            }, function (err) {
                contentEditingHelper.handleSaveError({
                    err: err,
                    allNewProps: contentEditingHelper.getAllProps(err.data),
                    allOrigProps: contentEditingHelper.getAllProps($scope.content)
                });

                deferred.reject(err);
        });

        return deferred.promise;
    };

}

angular.module("umbraco").controller("Umbraco.Editors.Member.EditController", MemberEditController);
