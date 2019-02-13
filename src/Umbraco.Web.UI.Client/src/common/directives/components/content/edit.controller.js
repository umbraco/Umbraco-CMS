(function () {
    'use strict';

    function ContentEditController($rootScope, $scope, $routeParams, $q, $window,
        appState, contentResource, entityResource, navigationService, notificationsService,
        serverValidationManager, contentEditingHelper, treeService, formHelper, umbRequestHelper,
        editorState, $http, eventsService, relationResource, overlayService, $location) {

        var evts = [];
        var infiniteMode = $scope.infiniteModel && $scope.infiniteModel.infiniteMode;

        //setup scope vars
        $scope.defaultButton = null;
        $scope.subButtons = [];

        $scope.page = {};
        $scope.page.loading = false;
        $scope.page.menu = {};
        $scope.page.menu.currentNode = null;
        $scope.page.menu.currentSection = appState.getSectionState("currentSection");
        $scope.page.listViewPath = null;
        $scope.page.isNew = $scope.isNew ? true : false;
        $scope.page.buttonGroupState = "init";
        $scope.page.hideActionsMenu = infiniteMode ? true : false;
        $scope.page.hideChangeVariant = false;
        $scope.allowOpen = true;
        $scope.app = null;

        function init() {
            
            var content = $scope.content;
            
            // we need to check wether an app is present in the current data, if not we will present the default app.
            var isAppPresent = false;
            
            // on first init, we dont have any apps. but if we are re-initializing, we do, but ...
            if ($scope.app) {
                
                // lets check if it still exists as part of our apps array. (if not we have made a change to our docType, even just a re-save of the docType it will turn into new Apps.)
                _.forEach(content.apps, function(app) {
                    if (app === $scope.app) {
                        isAppPresent = true;
                    }
                });
                
                // if we did reload our DocType, but still have the same app we will try to find it by the alias.
                if (isAppPresent === false) {
                    _.forEach(content.apps, function(app) {
                        if (app.alias === $scope.app.alias) {
                            isAppPresent = true;
                            app.active = true;
                            $scope.appChanged(app);
                        }
                    });
                }
                
            }
            
            // if we still dont have a app, lets show the first one:
            if (isAppPresent === false) {
                content.apps[0].active = true;
                $scope.appChanged(content.apps[0]);
            }
            
            editorState.set(content);

            //We fetch all ancestors of the node to generate the footer breadcrumb navigation
            if (!$scope.page.isNew) {
                if (content.parentId && content.parentId !== -1) {
                    entityResource.getAncestors(content.id, "document", $scope.culture)
                        .then(function (anc) {
                            $scope.ancestors = anc;
                        });
                    $scope.$watch('culture',
                        function (value, oldValue) {
                            entityResource.getAncestors(content.id, "document", value)
                                .then(function (anc) {
                                    $scope.ancestors = anc;
                                });
                        });
                }
            }

            bindEvents();

            resetVariantFlags();
        }


        /**
         * This will reset isDirty flags if save is true.
         * When working with multiple variants, this will set the save/publish flags of each one to false.
         * When working with a single variant, this will set the publish flag to false and the save flag to true.
         */
        function resetVariantFlags() {
            if ($scope.content.variants.length > 1) {
                for (var i = 0; i < $scope.content.variants.length; i++) {
                    var v = $scope.content.variants[i];
                    if (v.save) {
                        v.isDirty = false;
                    }
                    v.save = false;
                    v.publish = false;
                }
            }
            else {
                if ($scope.content.variants[0].save) {
                    $scope.content.variants[0].isDirty = false;
                }
                $scope.content.variants[0].save = true;
                $scope.content.variants[0].publish = false;
            }
        }

        /** Returns true if the content item varies by culture */
        function isContentCultureVariant() {
            return $scope.content.variants.length > 1;
        }

        function bindEvents() {
            //bindEvents can be called more than once and we don't want to have multiple bound events
            for (var e in evts) {
                eventsService.unsubscribe(evts[e]);
            }

            evts.push(eventsService.on("editors.content.reload", function (name, args) {
                // if this content item uses the updated doc type we need to reload the content item
                if(args && args.node && args.node.key === $scope.content.key) {
                    $scope.page.loading = true;
                    loadContent().then(function() {
                        $scope.page.loading = false;
                    });
                }
            }));

        }

        /**
         *  This does the content loading and initializes everything, called on first load
         */
        function loadContent() {

            //we are editing so get the content item from the server
            return $scope.getMethod()($scope.contentId)
                .then(function (data) {

                    $scope.content = data;

                    if (data.isChildOfListView && data.trashed === false) {
                        $scope.page.listViewPath = ($routeParams.page) ?
                            "/content/content/edit/" + data.parentId + "?page=" + $routeParams.page :
                            "/content/content/edit/" + data.parentId;
                    }

                    init();

                    syncTreeNode($scope.content, $scope.content.path, true);

                    resetLastListPageNumber($scope.content);

                    eventsService.emit("content.loaded", { content: $scope.content });

                    return $q.resolve($scope.content);


                });

        }

        /**
         * Create the save/publish/preview buttons for the view
         * @param {any} content the content node
         * @param {any} app the active content app
         */
        function createButtons(content) {

            // for trashed items, the save button is the primary action - otherwise it's a secondary action
            $scope.page.saveButtonStyle = content.trashed ? "primary" : "info";

            // only create the save/publish/preview buttons if the
            // content app is "Conent"
            if ($scope.app && $scope.app.alias !== "umbContent" && $scope.app.alias !== "umbInfo") {
                $scope.defaultButton = null;
                $scope.subButtons = null;
                $scope.page.showSaveButton = false;
                $scope.page.showPreviewButton = false;
                return;
            }

            // create the save button
            if (_.contains($scope.content.allowedActions, "A")) {
                $scope.page.showSaveButton = true;
                // add ellipsis to the save button if it opens the variant overlay
                $scope.page.saveButtonEllipsis = content.variants && content.variants.length > 1 ? "true" : "false";
            }

            // create the pubish combo button
            $scope.page.buttonGroupState = "init";
            var buttons = contentEditingHelper.configureContentEditorButtons({
                create: $scope.page.isNew,
                content: content,
                methods: {
                    saveAndPublish: $scope.saveAndPublish,
                    sendToPublish: $scope.sendToPublish,
                    unpublish: $scope.unpublish,
                    schedulePublish: $scope.schedule,
                    publishDescendants: $scope.publishDescendants
                }
            });

            $scope.defaultButton = buttons.defaultButton;
            $scope.subButtons = buttons.subButtons;
            $scope.page.showPreviewButton = true;

        }
        
        /** Syncs the content item to it's tree node - this occurs on first load and after saving */
        function syncTreeNode(content, path, initialLoad) {

            if (infiniteMode || !path) {
                return;
            }

            if (!$scope.content.isChildOfListView) {
                navigationService.syncTree({ tree: $scope.treeAlias, path: path.split(","), forceReload: initialLoad !== true }).then(function (syncArgs) {
                    $scope.page.menu.currentNode = syncArgs.node;
                });
            }
            else if (initialLoad === true) {

                //it's a child item, just sync the ui node to the parent
                navigationService.syncTree({ tree: $scope.treeAlias, path: path.substring(0, path.lastIndexOf(",")).split(","), forceReload: initialLoad !== true });

                //if this is a child of a list view and it's the initial load of the editor, we need to get the tree node
                // from the server so that we can load in the actions menu.
                umbRequestHelper.resourcePromise(
                    $http.get(content.treeNodeUrl),
                    'Failed to retrieve data for child node ' + content.id).then(function (node) {
                        $scope.page.menu.currentNode = node;
                    });
            }
        }

        function checkValidility() {
            //Get all controls from the 'contentForm'
            var allControls = $scope.contentForm.$getControls();

            //An array to store items in when we find child form fields (no matter how many deep nested forms)
            var childFieldsToMarkAsValid = [];

            //Exclude known formControls 'contentHeaderForm' and 'tabbedContentForm'
            //Check property - $name === "contentHeaderForm"
            allControls = _.filter(allControls, function (obj) {
                return obj.$name !== 'contentHeaderForm' && obj.$name !== 'tabbedContentForm' && obj.hasOwnProperty('$submitted');
            });

            for (var i = 0; i < allControls.length; i++) {
                var nestedForm = allControls[i];

                //Get Nested Controls of this form in the loop
                var nestedFormControls = nestedForm.$getControls();

                //Need to recurse through controls (could be more nested forms)
                childFieldsToMarkAsValid = recurseFormControls(nestedFormControls, childFieldsToMarkAsValid);
            }

            return childFieldsToMarkAsValid;
        }

        //Controls is the
        function recurseFormControls(controls, array) {

            //Loop over the controls
            for (var i = 0; i < controls.length; i++) {
                var controlItem = controls[i];

                //Check if the controlItem has a property ''
                if (controlItem.hasOwnProperty('$submitted')) {
                    //This item is a form - so lets get the child controls of it & recurse again
                    var childFormControls = controlItem.$getControls();
                    recurseFormControls(childFormControls, array);
                }
                else {
                    //We can assume its a field on a form
                    if (controlItem.hasOwnProperty('$error')) {
                        //Set the validlity of the error/s to be valid
                        //String of keys of error invalid messages
                        var errorKeys = [];

                        for (var key in controlItem.$error) {
                            errorKeys.push(key);
                            controlItem.$setValidity(key, true);
                        }

                        //Create a basic obj - storing the control item & the error keys
                        var obj = { 'control': controlItem, 'errorKeys': errorKeys };

                        //Push the updated control into the array - so we can set them back
                        array.push(obj);
                    }
                }
            }
            return array;
        }

        function resetNestedFieldValiation(array) {
            for (var i = 0; i < array.length; i++) {
                var item = array[i];
                //Item is an object containing two props
                //'control' (obj) & 'errorKeys' (string array)
                var fieldControl = item.control;
                var fieldErrorKeys = item.errorKeys;

                for (var j = 0; j < fieldErrorKeys.length; j++) {
                    fieldControl.$setValidity(fieldErrorKeys[j], false);
                }
            }
        }

        function ensureDirtyIsSetIfAnyVariantIsDirty() {

            $scope.contentForm.$dirty = false;

            for (var i = 0; i < $scope.content.variants.length; i++) {
                if($scope.content.variants[i].isDirty){
                    $scope.contentForm.$dirty = true;
                    return;
                }
            }
        }

        // This is a helper method to reduce the amount of code repitition for actions: Save, Publish, SendToPublish
        function performSave(args) {


            //Used to check validility of nested form - coming from Content Apps mostly
            //Set them all to be invalid
            var fieldsToRollback = checkValidility();
            eventsService.emit("content.saving", { content: $scope.content, action: args.action });

            return contentEditingHelper.contentEditorPerformSave({
                saveMethod: args.saveMethod,
                scope: $scope,
                content: $scope.content,
                action: args.action,
                showNotifications: args.showNotifications
            }).then(function (data) {
                //success
                init();
                syncTreeNode($scope.content, data.path);

                eventsService.emit("content.saved", { content: $scope.content, action: args.action });

                resetNestedFieldValiation(fieldsToRollback);
                ensureDirtyIsSetIfAnyVariantIsDirty();

                return $q.when(data);
            },
                function (err) {
                    syncTreeNode($scope.content, $scope.content.path);

                    //error
                    if (err) {
                        editorState.set($scope.content);
                    }

                    resetNestedFieldValiation(fieldsToRollback);

                    return $q.reject(err);
                });
        }

        function clearNotifications(content) {
            if (content.notifications) {
                content.notifications = [];
            }
            if (content.variants) {
                for (var i = 0; i < content.variants.length; i++) {
                    if (content.variants[i].notifications) {
                        content.variants[i].notifications = [];
                    }
                }
            }
        }

        function resetLastListPageNumber(content) {
            // We're using rootScope to store the page number for list views, so if returning to the list
            // we can restore the page.  If we've moved on to edit a piece of content that's not the list or it's children
            // we should remove this so as not to confuse if navigating to a different list
            if (!content.isChildOfListView && !content.isContainer) {
                $rootScope.lastListViewPageViewed = null;
            }
        }

        /**
         * Used to clear the dirty state for successfully saved variants when not all variant saving was successful
         * @param {any} variants
         */
        function clearDirtyState(variants) {
            for (var i = 0; i < variants.length; i++) {
                var v = variants[i];
                if (v.notifications) {
                    var isSuccess = _.find(v.notifications, function (n) {
                        return n.type === 3; //this is a success notification
                    });
                    if (isSuccess) {
                        v.isDirty = false;
                    }
                }
            }
        }

        if ($scope.page.isNew) {

            $scope.page.loading = true;

            //we are creating so get an empty content item
            $scope.getScaffoldMethod()()
                .then(function (data) {

                    $scope.content = data;

                    init();

                    resetLastListPageNumber($scope.content);

                    eventsService.emit("content.newReady", { content: $scope.content });

                    $scope.page.loading = false;

                });
        }
        else {

            $scope.page.loading = true;

            loadContent().then(function () {
                $scope.page.loading = false;
            });
        }

        $scope.unpublish = function () {
            clearNotifications($scope.content);
            if (formHelper.submitForm({ scope: $scope, action: "unpublish", skipValidation: true })) {
                var dialog = {
                    parentScope: $scope,
                    view: "views/content/overlays/unpublish.html",
                    variants: $scope.content.variants, //set a model property for the dialog
                    skipFormValidation: true, //when submitting the overlay form, skip any client side validation
                    submitButtonLabelKey: "content_unpublish",
                    submitButtonStyle: "warning",
                    submit: function (model) {

                        model.submitButtonState = "busy";

                        var selectedVariants = _.filter(model.variants, v => v.save && v.language); //ignore invariant
                        var culturesForUnpublishing = _.map(selectedVariants, v => v.language.culture);

                        contentResource.unpublish($scope.content.id, culturesForUnpublishing)
                            .then(function (data) {
                                formHelper.resetForm({ scope: $scope });
                                contentEditingHelper.reBindChangedProperties($scope.content, data);
                                init();
                                syncTreeNode($scope.content, data.path);
                                $scope.page.buttonGroupState = "success";
                                eventsService.emit("content.unpublished", { content: $scope.content });
                                overlayService.close();
                            }, function (err) {
                                $scope.page.buttonGroupState = 'error';
                            });


                    },
                    close: function () {
                        overlayService.close();
                    }
                };
                overlayService.open(dialog);
            }
        };

        $scope.sendToPublish = function () {
            clearNotifications($scope.content);
            if (isContentCultureVariant()) {
                //before we launch the dialog we want to execute all client side validations first
                if (formHelper.submitForm({ scope: $scope, action: "publish" })) {

                    var dialog = {
                        parentScope: $scope,
                        view: "views/content/overlays/sendtopublish.html",
                        variants: $scope.content.variants, //set a model property for the dialog
                        skipFormValidation: true, //when submitting the overlay form, skip any client side validation
                        submitButtonLabelKey: "buttons_saveToPublish",
                        submit: function (model) {
                            model.submitButtonState = "busy";
                            clearNotifications($scope.content);
                            //we need to return this promise so that the dialog can handle the result and wire up the validation response
                            return performSave({
                                saveMethod: contentResource.sendToPublish,
                                action: "sendToPublish",
                                showNotifications: false
                            }).then(function (data) {
                                //show all notifications manually here since we disabled showing them automatically in the save method
                                formHelper.showNotifications(data);
                                clearNotifications($scope.content);
                                overlayService.close();
                                return $q.when(data);
                            }, function (err) {
                                clearDirtyState($scope.content.variants);
                                model.submitButtonState = "error";
                                //re-map the dialog model since we've re-bound the properties
                                dialog.variants = $scope.content.variants;
                                //don't reject, we've handled the error
                                return $q.when(err);
                            });
                        },
                        close: function () {
                            overlayService.close();
                        }
                    };

                    overlayService.open(dialog);
                }
            }
            else {
                $scope.page.buttonGroupState = "busy";
                return performSave({
                    saveMethod: contentResource.sendToPublish,
                    action: "sendToPublish"
                }).then(function () {
                    $scope.page.buttonGroupState = "success";
                }, function () {
                    $scope.page.buttonGroupState = "error";
                });;
            }
        };

        $scope.saveAndPublish = function () {
            clearNotifications($scope.content);
            if (isContentCultureVariant()) {
                //before we launch the dialog we want to execute all client side validations first
                if (formHelper.submitForm({ scope: $scope, action: "publish" })) {

                    var dialog = {
                        parentScope: $scope,
                        view: "views/content/overlays/publish.html",
                        variants: $scope.content.variants, //set a model property for the dialog
                        skipFormValidation: true, //when submitting the overlay form, skip any client side validation
                        submitButtonLabelKey: "buttons_saveAndPublish",
                        submit: function (model) {
                            model.submitButtonState = "busy";
                            clearNotifications($scope.content);
                            //we need to return this promise so that the dialog can handle the result and wire up the validation response
                            return performSave({
                                saveMethod: contentResource.publish,
                                action: "publish",
                                showNotifications: false
                            }).then(function (data) {
                                //show all notifications manually here since we disabled showing them automatically in the save method
                                formHelper.showNotifications(data);
                                clearNotifications($scope.content);
                                overlayService.close();
                                return $q.when(data);
                            },
                                function (err) {
                                    clearDirtyState($scope.content.variants);
                                    model.submitButtonState = "error";
                                    //re-map the dialog model since we've re-bound the properties
                                    dialog.variants = $scope.content.variants;
                                    //don't reject, we've handled the error
                                    return $q.when(err);
                                });
                        },
                        close: function () {
                            overlayService.close();
                        }
                    };

                    overlayService.open(dialog);
                }
            }
            else {
                //ensure the flags are set
                $scope.content.variants[0].save = true;
                $scope.content.variants[0].publish = true;
                $scope.page.buttonGroupState = "busy";
                return performSave({
                    saveMethod: contentResource.publish,
                    action: "publish"
                }).then(function () {
                    $scope.page.buttonGroupState = "success";
                }, function () {
                    $scope.page.buttonGroupState = "error";
                });;
            }
        };

        $scope.save = function () {
            clearNotifications($scope.content);
            // TODO: Add "..." to save button label if there are more than one variant to publish - currently it just adds the elipses if there's more than 1 variant
            if (isContentCultureVariant()) {
                //before we launch the dialog we want to execute all client side validations first
                if (formHelper.submitForm({ scope: $scope, action: "openSaveDialog" })) {

                    var dialog = {
                        parentScope: $scope,
                        view: "views/content/overlays/save.html",
                        variants: $scope.content.variants, //set a model property for the dialog
                        skipFormValidation: true, //when submitting the overlay form, skip any client side validation
                        submitButtonLabelKey: "buttons_save",
                        submit: function (model) {
                            model.submitButtonState = "busy";
                            clearNotifications($scope.content);
                            //we need to return this promise so that the dialog can handle the result and wire up the validation response
                            return performSave({
                                saveMethod: $scope.saveMethod(),
                                action: "save",
                                showNotifications: false
                            }).then(function (data) {
                                //show all notifications manually here since we disabled showing them automatically in the save method
                                formHelper.showNotifications(data);
                                clearNotifications($scope.content);
                                overlayService.close();
                                return $q.when(data);
                            }, function (err) {
                                clearDirtyState($scope.content.variants);
                                model.submitButtonState = "error";
                                //re-map the dialog model since we've re-bound the properties
                                dialog.variants = $scope.content.variants;
                                //don't reject, we've handled the error
                                return $q.when(err);
                            });
                        },
                        close: function (oldModel) {
                            overlayService.close();
                        }
                    };

                    overlayService.open(dialog);
                }
            }
            else {
                //ensure the flags are set
                $scope.content.variants[0].save = true;
                $scope.page.saveButtonState = "busy";
                return performSave({
                    saveMethod: $scope.saveMethod(),
                    action: "save"
                }).then(function () {
                    $scope.page.saveButtonState = "success";
                }, function () {
                    $scope.page.saveButtonState = "error";
                });
            }

        };

        $scope.schedule = function() {
            clearNotifications($scope.content);
            //before we launch the dialog we want to execute all client side validations first
            if (formHelper.submitForm({ scope: $scope, action: "schedule" })) {

                //used to track the original values so if the user doesn't save the schedule and they close the dialog we reset the dates back to what they were.
                let origDates = [];
                for (let i = 0; i < $scope.content.variants.length; i++) {
                    origDates.push({
                        releaseDate: $scope.content.variants[i].releaseDate,
                        expireDate: $scope.content.variants[i].expireDate
                    });
                }

                if (!isContentCultureVariant()) {
                    //ensure the flags are set
                    $scope.content.variants[0].save = true;
                }

                var dialog = {
                    parentScope: $scope,
                    view: "views/content/overlays/schedule.html",
                    variants: $scope.content.variants, //set a model property for the dialog
                    skipFormValidation: true, //when submitting the overlay form, skip any client side validation
                    submitButtonLabelKey: "buttons_schedulePublish",
                    submit: function (model) {
                        model.submitButtonState = "busy";
                        clearNotifications($scope.content);

                        //we need to return this promise so that the dialog can handle the result and wire up the validation response
                        return performSave({
                            saveMethod: contentResource.saveSchedule,
                            action: "schedule",
                            showNotifications: false
                        }).then(function (data) {
                            //show all notifications manually here since we disabled showing them automatically in the save method
                            formHelper.showNotifications(data);
                            clearNotifications($scope.content);
                            overlayService.close();
                            return $q.when(data);
                        }, function (err) {
                            clearDirtyState($scope.content.variants);
                            //if this is invariant, show the notification errors, else they'll be shown inline with the variant
                            if (!isContentCultureVariant()) {
                                formHelper.showNotifications(err.data);
                            }
                            model.submitButtonState = "error";
                            //re-map the dialog model since we've re-bound the properties
                            dialog.variants = $scope.content.variants;
                            //don't reject, we've handled the error
                            return $q.when(err);
                        });

                    },
                    close: function () {
                        overlayService.close();
                        //restore the dates
                        for (let i = 0; i < $scope.content.variants.length; i++) {
                            $scope.content.variants[i].releaseDate = origDates[i].releaseDate;
                            $scope.content.variants[i].expireDate = origDates[i].expireDate;
                        }
                    }
                };
                overlayService.open(dialog);
            }
        };

        $scope.publishDescendants = function() {
            clearNotifications($scope.content);
            //before we launch the dialog we want to execute all client side validations first
            if (formHelper.submitForm({ scope: $scope, action: "publishDescendants" })) {

                if (!isContentCultureVariant()) {
                    //ensure the flags are set
                    $scope.content.variants[0].save = true;
                    $scope.content.variants[0].publish = true;
                }

                var dialog = {
                    parentScope: $scope,
                    view: "views/content/overlays/publishdescendants.html",
                    variants: $scope.content.variants, //set a model property for the dialog
                    skipFormValidation: true, //when submitting the overlay form, skip any client side validation
                    submitButtonLabelKey: "buttons_publishDescendants",
                    submit: function (model) {
                        model.submitButtonState = "busy";
                        clearNotifications($scope.content);

                        //we need to return this promise so that the dialog can handle the result and wire up the validation response
                        return performSave({
                            saveMethod: function (content, create, files, showNotifications) {
                                return contentResource.publishWithDescendants(content, create, model.includeUnpublished, files, showNotifications);
                            },
                            action: "publishDescendants",
                            showNotifications: false
                        }).then(function (data) {
                            //show all notifications manually here since we disabled showing them automatically in the save method
                            formHelper.showNotifications(data);
                            clearNotifications($scope.content);
                            overlayService.close();
                            return $q.when(data);
                        }, function (err) {
                            clearDirtyState($scope.content.variants);
                            //if this is invariant, show the notification errors, else they'll be shown inline with the variant
                            if (!isContentCultureVariant()) {
                                formHelper.showNotifications(err.data);
                            }
                            model.submitButtonState = "error";
                            //re-map the dialog model since we've re-bound the properties
                            dialog.variants = $scope.content.variants;
                            //don't reject, we've handled the error
                            return $q.when(err);
                        });

                    },
                    close: function () {
                        overlayService.close();
                    }
                };
                overlayService.open(dialog);
            }
        };

        $scope.preview = function (content) {


            if (!$scope.busy) {

                // Chromes popup blocker will kick in if a window is opened
                // without the initial scoped request. This trick will fix that.
                //
                var previewWindow = $window.open('preview/?init=true', 'umbpreview');

                // Build the correct path so both /#/ and #/ work.
                var query = 'id=' + content.id;
                if ($scope.culture) {
                    query += "#?culture=" + $scope.culture;
                }
                var redirect = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/preview/?' + query;

                //The user cannot save if they don't have access to do that, in which case we just want to preview
                //and that's it otherwise they'll get an unauthorized access message
                if (!_.contains(content.allowedActions, "A")) {
                    previewWindow.location.href = redirect;
                }
                else {
                    var selectedVariant;
                    if (!$scope.culture) {
                        selectedVariant = $scope.content.variants[0];
                    }
                    else {
                        selectedVariant = _.find($scope.content.variants, function (v) {
                            return v.language.culture === $scope.culture;
                        });
                    }

                    //ensure the save flag is set
                    selectedVariant.save = true;
                    performSave({ saveMethod: $scope.saveMethod(), action: "save" }).then(function (data) {
                        previewWindow.location.href = redirect;
                    }, function (err) {
                        //validation issues ....
                    });
                }
            }
        };

        /* publish method used in infinite editing */
        $scope.publishAndClose = function (content) {
            $scope.publishAndCloseButtonState = "busy";
            performSave({ saveMethod: contentResource.publish, action: "publish" }).then(function () {
                if ($scope.infiniteModel.submit) {
                    $scope.infiniteModel.contentNode = content;
                    $scope.infiniteModel.submit($scope.infiniteModel);
                }
                $scope.publishAndCloseButtonState = "success";
            });
        };

        /* save method used in infinite editing */
        $scope.saveAndClose = function (content) {
            $scope.saveAndCloseButtonState = "busy";
            performSave({ saveMethod: $scope.saveMethod(), action: "save" }).then(function () {
                if ($scope.infiniteModel.submit) {
                    $scope.infiniteModel.contentNode = content;
                    $scope.infiniteModel.submit($scope.infiniteModel);
                }
                $scope.saveAndCloseButtonState = "success";
            });
        };

        /**
         * Call back when a content app changes
         * @param {any} app
         */
        $scope.appChanged = function (app) {
            
            $scope.app = app;
            
            $scope.$broadcast("editors.apps.appChanged", { app: app });
            
            createButtons($scope.content);
            
        };

        /**
         * Call back when a content app changes
         * @param {any} app
         */
        $scope.appAnchorChanged = function (app, anchor) {
            //send an event downwards
            $scope.$broadcast("editors.apps.appAnchorChanged", { app: app, anchor: anchor });
        };

        // methods for infinite editing
        $scope.close = function () {
            if ($scope.infiniteModel.close) {
                $scope.infiniteModel.close($scope.infiniteModel);
            }
        };
        
        /**
         * Call back when user click the back-icon
         */
        $scope.onBack = function() {
            if ($scope.infiniteModel && $scope.infiniteModel.close) {
                $scope.infiniteModel.close($scope.infiniteModel);
            } else {
                // navigate backwards if content has a parent.
                $location.path('/' + $routeParams.section + '/' + $routeParams.tree + '/' + $routeParams.method + '/' + $scope.content.parentId);
            }
        };

        //ensure to unregister from all events!
        $scope.$on('$destroy', function () {
            for (var e in evts) {
                eventsService.unsubscribe(evts[e]);
            }
            //since we are not notifying and clearing server validation messages when they are received due to how the variant
            //switching works, we need to ensure they are cleared when this editor is destroyed
            if (!$scope.page.isNew) {
                serverValidationManager.clear();
            }
        });

    }

    function createDirective() {

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/content/edit.html',
            controller: 'Umbraco.Editors.Content.EditorDirectiveController',
            scope: {
                contentId: "=",
                isNew: "=?",
                treeAlias: "@",
                page: "=?",
                saveMethod: "&",
                getMethod: "&",
                getScaffoldMethod: "&?",
                culture: "=?",
                infiniteModel: "=?"
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').controller('Umbraco.Editors.Content.EditorDirectiveController', ContentEditController);
    angular.module('umbraco.directives').directive('contentEditor', createDirective);

})();
