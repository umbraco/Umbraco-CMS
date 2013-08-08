/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.EditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function ContentEditController($scope, $routeParams, $location, contentResource, notificationsService, angularHelper, serverValidationManager, contentEditingHelper) {
        
    /** adds the custom controls to the generic props tab above the user props */
    function configureGenericPropertiesTab(genericPropsTab, content) {

        //NOTE: I don't really think we need these here, we should just chuck them on the mock data, then we can make
        // sure these are just returned from the server with the correct localized strings.
        genericPropsTab.properties.splice(0, 0,
            {
                label: 'Created',
                description: 'Time this document was created',
                value: content.createDate,
                view: "readonlyvalue",
                config: { filter: 'date', format: 'medium' },
                alias: "umb_createdate" //can't overlap with user defined props!! important for validation.
            },
            {
                label: 'Updated',
                description: 'Time this document was last updated',
                value: content.updateDate,
                view: "readonlyvalue",
                config: { filter: 'date', format: 'medium' },
                alias: "umb_updatedate" //can't overlap with user defined props!! important for validation.
            });
    }

    if ($routeParams.create) {
        //we are creating so get an empty content item
        contentResource.getScaffold($routeParams.id, $routeParams.doctype)
            .then(function(data) {
                $scope.contentLoaded = true;
                $scope.content = data;
                $scope.genericPropertiesTab = $.grep($scope.content.tabs, function (e) { return e.id === 0; })[0];
                configureGenericPropertiesTab($scope.genericPropertiesTab, $scope.content);
            });
    }
    else {
        //we are editing so get the content item from the server
        contentResource.getById($routeParams.id)
            .then(function(data) {
                $scope.contentLoaded = true;
                $scope.content = data;
                $scope.genericPropertiesTab = $.grep($scope.content.tabs, function(e){ return e.id === 0; })[0];
                configureGenericPropertiesTab($scope.genericPropertiesTab, $scope.content);
                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                serverValidationManager.executeAndClearAllSubscriptions();
            });
    }

    $scope.files = [];
    $scope.addFiles = function (propertyId, files) {
        //this will clear the files for the current property and then add the new ones for the current property
        $scope.files = _.reject($scope.files, function (item) {
            return item.id == propertyId;
        });
        for (var i = 0; i < files.length; i++) {
            //save the file object to the scope's files collection
            $scope.files.push({ id: propertyId, file: files[i] });
        }
    };

    //ensure there is a form object assigned.
    var currentForm = angularHelper.getRequiredCurrentForm($scope);

    //TODO: Need to figure out a way to share the saving and event broadcasting with all editors!

    $scope.saveAndPublish = function (cnt) {
        $scope.$broadcast("saving", { scope: $scope });

        //don't continue if the form is invalid
        if (currentForm.$invalid) return;

        serverValidationManager.reset();
        
        contentResource.publishContent(cnt, $routeParams.create, $scope.files)
            .then(function (data) {
                contentEditingHelper.handleSuccessfulSave({
                    scope: $scope,
                    newContent: data
                });
            }, function (err) {                
                contentEditingHelper.handleSaveError(err, $scope);
            });	        
    };

    $scope.save = function (cnt) {
        $scope.$broadcast("saving", { scope: $scope });
            
        //don't continue if the form is invalid
        if (currentForm.$invalid) return;

        serverValidationManager.reset();

        contentResource.saveContent(cnt, $routeParams.create, $scope.files)
            .then(function (data) {
                contentEditingHelper.handleSuccessfulSave({
                    scope: $scope,
                    newContent: data
                });
            }, function (err) {
                contentEditingHelper.handleSaveError(err, $scope);
        });
    };

    
    $scope.exludeLastTab = function(item, args) {
        return item.id !== 0;
    };
}

angular.module("umbraco").controller("Umbraco.Editors.Content.EditController", ContentEditController);
