/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.EditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function ContentEditController($scope, $routeParams, $q, $timeout, $window, contentResource, navigationService, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, fileManager, formHelper) {

    $scope.defaultButton = null;
    $scope.subButtons = [];

    //This sets up the action buttons based on what permissions the user has.
    //The allowedActions parameter contains a list of chars, each represents a button by permission so 
    //here we'll build the buttons according to the chars of the user.
    function configureButtons(content) {
        //reset
        $scope.subButtons = [];

        //This is the ideal button order but depends on circumstance, we'll use this array to create the button list
        // Publish, SendToPublish, Save
        var buttonOrder = ["U", "H", "A"];

        //Create the first button (primary button)
        //We cannot have the Save or SaveAndPublish buttons if they don't have create permissions when we are creating a new item.
        if (!$routeParams.create || _.contains(content.allowedActions, "C")) {
            for (var b in buttonOrder) {
                if (_.contains(content.allowedActions, buttonOrder[b])) {
                    $scope.defaultButton = createButtonDefinition(buttonOrder[b]);
                    break;
                }
            }
        }
        
        //Now we need to make the drop down button list, this is also slightly tricky because:
        //We cannot have any buttons if there's no default button above.
        //We cannot have the unpublish button (Z) when there's no publish permission.    
        //We cannot have the unpublish button (Z) when the item is not published.           
        if ($scope.defaultButton) {

            //get the last index of the button order
            var lastIndex = _.indexOf(buttonOrder, $scope.defaultButton.letter);
            //add the remaining
            for (var i = lastIndex + 1; i < buttonOrder.length; i++) {
                if (_.contains(content.allowedActions, buttonOrder[i])) {
                    $scope.subButtons.push(createButtonDefinition(buttonOrder[i]));
                }
            }

            //if we are not creating, then we should add unpublish too, 
            // so long as it's already published and if the user has access to publish
            if (!$routeParams.create) {
                if (content.publishDate && _.contains(content.allowedActions,"U")) {
                    $scope.subButtons.push(createButtonDefinition("Z"));
                }
            }
        }
    }

    function createButtonDefinition(ch) {
        switch (ch) {
            case "U":
                //publish action
                return {
                    letter: ch,
                    labelKey: "buttons_saveAndPublish",
                    handler: $scope.saveAndPublish,
                    hotKey: "ctrl+p"
                };
            case "H":
                //send to publish
                return {
                    letter: ch,
                    labelKey: "buttons_saveToPublish",
                    handler: $scope.sendToPublish,
                    hotKey: "ctrl+t"
                };
            case "A":
                //save
                return {
                    letter: ch,
                    labelKey: "buttons_save",
                    handler: $scope.save,
                    hotKey: "ctrl+s"
                };
            case "Z":
                //unpublish
                return {
                    letter: ch,
                    labelKey: "content_unPublish",
                    handler: $scope.unPublish
                };
            default:
                return null;
        }
    }
    
    /** This is a helper method to reduce the amount of code repitition for actions: Save, Publish, SendToPublish */
    function performSave(args) {
        var deferred = $q.defer();

        if (formHelper.submitForm({ scope: $scope, statusMessage: args.statusMessage })) {

            args.saveMethod($scope.content, $routeParams.create, fileManager.getFiles())
                .then(function (data) {

                    formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        newContent: data,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                    });

                    configureButtons(data);

                    navigationService.syncPath(data.path.split(","), true);

                    deferred.resolve(data);
                    
                }, function (err) {
                    
                    contentEditingHelper.handleSaveError({
                        redirectOnFailure: true,
                        err: err,
                        allNewProps: contentEditingHelper.getAllProps(err.data),
                        allOrigProps: contentEditingHelper.getAllProps($scope.content)
                    });

                    deferred.reject(err);
                });
        }
        else {
            deferred.reject();
        }

        return deferred.promise;
    }

    if ($routeParams.create) {
        //we are creating so get an empty content item
        contentResource.getScaffold($routeParams.id, $routeParams.doctype)
            .then(function(data) {
                $scope.loaded = true;
                $scope.content = data;
                configureButtons($scope.content);
            });
    }
    else {
        //we are editing so get the content item from the server
        contentResource.getById($routeParams.id)
            .then(function(data) {
                $scope.loaded = true;
                $scope.content = data;
                configureButtons($scope.content);

                //just get the cached version, no need to force a reload
                navigationService.syncPath(data.path.split(","), false);
                
                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                serverValidationManager.executeAndClearAllSubscriptions();


            });
    }


    $scope.unPublish = function () {
        
        if (formHelper.submitForm({ scope: $scope, statusMessage: "Unpublishing...", skipValidation: true })) {

            contentResource.unPublish($scope.content.id)
                .then(function (data) {
                    
                    formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        newContent: data,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                    });

                    configureButtons(data);

                    navigationService.syncPath(data.path.split(","), true);
                });
        }
        
    };

    $scope.sendToPublish = function() {
        return performSave({ saveMethod: contentResource.sendToPublish, statusMessage: "Sending..." });
    };

    $scope.saveAndPublish = function() {
        return performSave({ saveMethod: contentResource.publish, statusMessage: "Publishing..." });        
    };

    $scope.save = function () {
        return performSave({ saveMethod: contentResource.save, statusMessage: "Saving..." });
    };

    $scope.preview = function(content){
            if(!content.id){
                $scope.save().then(function(data){
                      $window.open('dialogs/preview.aspx?id='+data.id,'umbpreview');  
                });
            }else{
                $window.open('dialogs/preview.aspx?id='+content.id,'umbpreview');
            }    
    };
    
    /** this method is called for all action buttons and then we proxy based on the btn definition */
    $scope.performAction = function(btn) {
        if (!btn || !angular.isFunction(btn.handler)) {
            throw "btn.handler must be a function reference";
        }
        btn.handler.apply(this);
    };

}

angular.module("umbraco").controller("Umbraco.Editors.Content.EditController", ContentEditController);
