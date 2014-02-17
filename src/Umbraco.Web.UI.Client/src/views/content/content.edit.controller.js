/**
 * @ngdoc controller
 * @name Umbraco.Editors.Content.EditController
 * @function
 * 
 * @description
 * The controller for the content editor
 */
function ContentEditController($scope, $routeParams, $q, $timeout, $window, appState, contentResource, entityResource, navigationService, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, treeService, fileManager, formHelper, umbRequestHelper, keyboardService, umbModelMapper, editorState, $http) {

    //setup scope vars
    $scope.defaultButton = null;
    $scope.subButtons = [];
    $scope.nav = navigationService;
    $scope.currentSection = appState.getSectionState("currentSection");
    $scope.currentNode = null; //the editors affiliated node
    

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

        //We fetch all ancestors of the node to generate the footer breadcrump navigation
        if (!$routeParams.create) {
        entityResource.getAncestors(content.id, "document")
            .then(function(anc) {
                anc.pop();
                $scope.ancestors = anc; 
            });
        }
    }

    function createButtonDefinition(ch) {
        switch (ch) {
            case "U":
                //publish action
                keyboardService.bind("ctrl+p", $scope.saveAndPublish);

                return {
                    letter: ch,
                    labelKey: "buttons_saveAndPublish",
                    handler: $scope.saveAndPublish,
                    hotKey: "ctrl+p"
                };
            case "H":
                //send to publish
                keyboardService.bind("ctrl+p", $scope.sendToPublish);

                return {
                    letter: ch,
                    labelKey: "buttons_saveToPublish",
                    handler: $scope.sendToPublish,
                    hotKey: "ctrl+p"
                };
            case "A":
                //save
                keyboardService.bind("ctrl+s", $scope.save);
                return {
                    letter: ch,
                    labelKey: "buttons_save",
                    handler: $scope.save,
                    hotKey: "ctrl+s"
                };
            case "Z":
                //unpublish
                keyboardService.bind("ctrl+u", $scope.unPublish);

                return {
                    letter: ch,
                    labelKey: "content_unPublish",
                    handler: $scope.unPublish
                };
            default:
                return null;
        }
    }
    
    /** Syncs the content item to it's tree node - this occurs on first load and after saving */
    function syncTreeNode(content, path, initialLoad) {        

        //If this is a child of a list view then we can't actually sync the real tree
        if (!$scope.content.isChildOfListView) {
            navigationService.syncTree({ tree: "content", path: path.split(","), forceReload: initialLoad !== true }).then(function (syncArgs) {
                $scope.currentNode = syncArgs.node;
            });
        }
        else if (initialLoad === true) {
            //if this is a child of a list view and it's the initial load of the editor, we need to get the tree node 
            // from the server so that we can load in the actions menu.
            umbRequestHelper.resourcePromise(
                $http.get(content.treeNodeUrl),
                'Failed to retreive data for child node ' + content.id).then(function(node) {
                    $scope.currentNode = node;
                });
        }
    }

    /** This is a helper method to reduce the amount of code repitition for actions: Save, Publish, SendToPublish */
    function performSave(args) {
        var deferred = $q.defer();

        $scope.busy = true;

        if (formHelper.submitForm({ scope: $scope, statusMessage: args.statusMessage })) {

            args.saveMethod($scope.content, $routeParams.create, fileManager.getFiles())
                .then(function (data) {

                    formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        savedContent: data,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                    });

                    editorState.set($scope.content);
                    $scope.busy = false;

                    configureButtons(data);

                    syncTreeNode($scope.content, data.path);

                    deferred.resolve(data);
                    
                }, function (err) {
                    
                    contentEditingHelper.handleSaveError({
                        redirectOnFailure: true,
                        err: err,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, err.data)
                    });

                    editorState.set($scope.content);
                    $scope.busy = false;

                    deferred.reject(err);
                });
        }
        else {
            $scope.busy = false;
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
                
                editorState.set($scope.content);

                configureButtons($scope.content);
            });
    }
    else {
        //we are editing so get the content item from the server
        contentResource.getById($routeParams.id)
            .then(function(data) {
                $scope.loaded = true;
                $scope.content = data;
                
                editorState.set($scope.content);
                
                configureButtons($scope.content);
                
                //in one particular special case, after we've created a new item we redirect back to the edit
                // route but there might be server validation errors in the collection which we need to display
                // after the redirect, so we will bind all subscriptions which will show the server validation errors
                // if there are any and then clear them so the collection no longer persists them.
                serverValidationManager.executeAndClearAllSubscriptions();

                syncTreeNode($scope.content, data.path, true);

            });
    }


    $scope.unPublish = function () {
        
        if (formHelper.submitForm({ scope: $scope, statusMessage: "Unpublishing...", skipValidation: true })) {

            contentResource.unPublish($scope.content.id)
                .then(function (data) {
                    
                    formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        savedContent: data,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                    });

                    editorState.set($scope.content);

                    configureButtons(data);

                    syncTreeNode($scope.content, data.path);

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
            }
            else {
                $window.open('dialogs/preview.aspx?id='+content.id,'umbpreview');
            }    
    };
    
    /** this method is called for all action buttons and then we proxy based on the btn definition */
    $scope.performAction = function(btn) {

        if (!btn || !angular.isFunction(btn.handler)) {
            throw "btn.handler must be a function reference";
        }
        
        if(!$scope.busy){
            btn.handler.apply(this);    
        }
    };

}

angular.module("umbraco").controller("Umbraco.Editors.Content.EditController", ContentEditController);
