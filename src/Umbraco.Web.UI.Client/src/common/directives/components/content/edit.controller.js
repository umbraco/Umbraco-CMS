(function () {
    'use strict';

    function ContentEditController($rootScope, $scope, $routeParams, $q, $window,
        appState, contentResource, entityResource, navigationService, notificationsService, contentAppHelper,
        serverValidationManager, contentEditingHelper, localizationService, formHelper, umbRequestHelper,
        editorState, $http, eventsService, overlayService, $location, localStorageService, treeService,
        $exceptionHandler) {

        var evts = [];
        var infiniteMode = $scope.infiniteModel && $scope.infiniteModel.infiniteMode;
        var watchingCulture = false;

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

        if (infiniteMode) {
            $scope.page.allowInfinitePublishAndClose = $scope.infiniteModel.allowPublishAndClose;
            $scope.page.allowInfiniteSaveAndClose = $scope.infiniteModel.allowSaveAndClose;
        }

        $scope.page.buttonGroupState = "init";
        $scope.page.hideActionsMenu = infiniteMode ? true : false;
        $scope.page.hideChangeVariant = false;
        $scope.allowOpen = true;
        $scope.activeApp = null;

        //initializes any watches
        function startWatches(content) {

            //watch for changes to isNew, set the page.isNew accordingly and load the breadcrumb if we can
            $scope.$watch('isNew', function (newVal, oldVal) {

                $scope.page.isNew = Object.toBoolean(newVal);

                //We fetch all ancestors of the node to generate the footer breadcrumb navigation
                if (content.parentId && content.parentId !== -1 && content.parentId !== -20) {
                    loadBreadcrumb();
                    if (!watchingCulture) {
                        $scope.$watch('culture',
                            function (value, oldValue) {
                                if (value !== oldValue) {
                                    loadBreadcrumb();
                                }
                            });
                    }
                }
            });

        }

        //this initializes the editor with the data which will be called more than once if the data is re-loaded
        function init() {

            var content = $scope.content;
            if (content.id && content.isChildOfListView && content.trashed === false) {
                $scope.page.listViewPath = "/content/content/edit/" + content.parentId
                    + "?list=" + $routeParams.list
                    + "&page=" + $routeParams.page
                    + "&filter=" + $routeParams.filter
                    + "&orderBy=" + $routeParams.orderBy
                    + "&orderDirection=" + $routeParams.orderDirection;
            }

            // we need to check wether an app is present in the current data, if not we will present the default app.
            var isAppPresent = false;

            // on first init, we dont have any apps. but if we are re-initializing, we do, but ...
            if ($scope.activeApp) {

                _.forEach(content.apps, function (app) {
                    if (app.alias === $scope.activeApp.alias) {
                        isAppPresent = true;
                        $scope.appChanged(app);
                    }
                });

                if (isAppPresent === false) {
                    // active app does not exist anymore.
                    $scope.activeApp = null;
                }
            }

            // if we still dont have a app, lets show the first one:
            if ($scope.activeApp === null && content.apps.length) {
                $scope.appChanged(content.apps[0]);
            }
            // otherwise make sure the save options are up to date with the current content state
            else {
                createButtons($scope.content);
            }

            editorState.set(content);

            bindEvents();

            resetVariantFlags();
        }

        function loadBreadcrumb() {
            // load the parent breadcrumb when creating new content
            var id = $scope.page.isNew ? $scope.content.parentId : $scope.content.id;
            if (!id) {
                return;
            }
            entityResource.getAncestors(id, "document", $scope.culture)
                .then(function (anc) {
                    $scope.ancestors = anc;
                });
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
        function hasVariants(content) {
            return content.variants.length > 1;
        }

        function reload() {
            $scope.page.loading = true;

            if ($scope.page.isNew) {
                loadScaffold().then(function () {
                    $scope.page.loading = false;
                });
            } else {
                loadContent().then(function () {
                    $scope.page.loading = false;
                });
            }
        }

        function bindEvents() {
            //bindEvents can be called more than once and we don't want to have multiple bound events
            for (var e in evts) {
                eventsService.unsubscribe(evts[e]);
            }

            evts.push(eventsService.on("editors.documentType.saved", function (name, args) {
                // if this content item uses the updated doc type we need to reload the content item
                if (args && args.documentType && $scope.content.documentType.id === args.documentType.id) {
                    reload();
                }
            }));

            evts.push(eventsService.on("editors.content.reload", function (name, args) {
                if (args && args.node && $scope.content.id === args.node.id) {
                    reload();
                    loadBreadcrumb();
                    syncTreeNode($scope.content, $scope.content.path);
                }
            }));

            evts.push(eventsService.on("rte.file.uploading", function () {
                $scope.page.saveButtonState = "busy";
                $scope.page.buttonGroupState = "busy";

            }));

            evts.push(eventsService.on("rte.file.uploaded", function () {
                $scope.page.saveButtonState = "success";
                $scope.page.buttonGroupState = "success";
            }));

            evts.push(eventsService.on("rte.shortcut.save", function () {
                if ($scope.page.showSaveButton) {
                    $scope.save();
                }
            }));

            evts.push(eventsService.on("rte.shortcut.saveAndPublish", function () {
                $scope.saveAndPublish();
            }));

            evts.push(eventsService.on("content.saved", function () {
                // Clear out localstorage keys that start with tinymce__
                // When we save/perist a content node
                // NOTE: clearAll supports a RegEx pattern of items to remove
                localStorageService.clearAll(/^tinymce__/);
            }));
        }

        function appendRuntimeData() {
            $scope.content.variants.forEach((variant) => {
                variant.compositeId = contentEditingHelper.buildCompositeVariantId(variant);
                variant.htmlId = "_content_variant_" + variant.compositeId + "_";
            });
        }

        /**
         *  This does the content loading and initializes everything, called on first load
         */
        function loadContent() {

            //we are editing so get the content item from the server
            return $scope.getMethod()($scope.contentId)
                .then(function (data) {

                    $scope.content = data;

                    appendRuntimeData();
                    init();

                    syncTreeNode($scope.content, $scope.content.path, true);

                    resetLastListPageNumber($scope.content);

                    eventsService.emit("content.loaded", { content: $scope.content });

                    return $q.resolve($scope.content);
                });
        }

        /**
        *  This loads the content scaffold for when creating new content
        */
        function loadScaffold() {
            //we are creating so get an empty content item
            return $scope.getScaffoldMethod()()
                .then(function (data) {

                    $scope.content = data;

                    appendRuntimeData();
                    init();
                    startWatches($scope.content);

                    resetLastListPageNumber($scope.content);

                    eventsService.emit("content.newReady", { content: $scope.content });

                    return $q.resolve($scope.content);

                });
        }

        /**
         * Create the save/publish/preview buttons for the view
         * @param {any} content the content node
         * @param {any} app the active content app
         */
        function createButtons(content) {

            var isBlueprint = content.isBlueprint;

            if ($scope.page.isNew && $location.path().search(/contentBlueprints/i) !== -1) {
               isBlueprint = true;
            }

            // for trashed and element type items, the save button is the primary action - otherwise it's a secondary action
            $scope.page.saveButtonStyle = content.trashed || content.isElement || isBlueprint ? "primary" : "info";
            // only create the save/publish/preview buttons if the
            // content app is "Conent"

            if ($scope.activeApp && !contentAppHelper.isContentBasedApp($scope.activeApp)) {
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
            } else {
                $scope.page.showSaveButton = false;
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
        function syncTreeNode(content, path, initialLoad, reloadChildren) {

            if (infiniteMode || !path) {
                return;
            }

            if (!$scope.content.isChildOfListView) {
                navigationService.syncTree({ tree: $scope.treeAlias, path: path.split(","), forceReload: initialLoad !== true })
                    .then(function (syncArgs) {
                        $scope.page.menu.currentNode = syncArgs.node;
                        if (reloadChildren && syncArgs.node.expanded) {
                            treeService.loadNodeChildren({ node: syncArgs.node });
                        }
                    }, function () {
                        //handle the rejection
                        console.log("A problem occurred syncing the tree! A path is probably incorrect.")
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
                if ($scope.content.variants[i].isDirty) {
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
                create: $scope.page.isNew,
                action: args.action,
                showNotifications: args.showNotifications,
                softRedirect: true,
                skipValidation: args.skipValidation
            }).then(function (data) {
                //success
                init();

                //needs to be manually set for infinite editing mode
                $scope.page.isNew = false;

                syncTreeNode($scope.content, data.path, false, args.reloadChildren);

                eventsService.emit("content.saved", { content: $scope.content, action: args.action, valid: true });

                if($scope.contentForm.$invalid !== true) {
                    resetNestedFieldValiation(fieldsToRollback);
                }
                ensureDirtyIsSetIfAnyVariantIsDirty();

                return $q.when(data);
            },
                function (err) {
                    syncTreeNode($scope.content, $scope.content.path);

                    if($scope.contentForm.$invalid !== true) {
                        resetNestedFieldValiation(fieldsToRollback);
                    }
                    if (err && err.status === 400 && err.data) {
                        // content was saved but is invalid.
                        eventsService.emit("content.saved", { content: $scope.content, action: args.action, valid: false });
                    }

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

        function handleHttpException(err) {
            if (err && !err.status) {
                $exceptionHandler(err);
            }
        }

        /** Just shows a simple notification that there are client side validation issues to be fixed */
        function showValidationNotification() {
            //TODO: We need to make the validation UI much better, there's a lot of inconsistencies in v8 including colors, issues with the property groups and validation errors between variants

            //need to show a notification else it's not clear there was an error.
            localizationService.localizeMany([
                "speechBubbles_validationFailedHeader",
                "speechBubbles_validationFailedMessage"
            ]
            ).then(function (data) {
                notificationsService.error(data[0], data[1]);
            });
        }

        if ($scope.page.isNew) {

            $scope.page.loading = true;

            loadScaffold().then(function () {
                $scope.page.loading = false;
            });
        }
        else {

            $scope.page.loading = true;

            loadContent().then(function () {
                startWatches($scope.content);
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
                    includeUnpublished: false,
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
                                formHelper.resetForm({ scope: $scope, hasErrors: true });
                                $scope.page.buttonGroupState = 'error';
                                handleHttpException(err);
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
            if (hasVariants($scope.content)) {
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
                            },
                            function (err) {
                                clearDirtyState($scope.content.variants);
                                model.submitButtonState = "error";
                                //re-map the dialog model since we've re-bound the properties
                                dialog.variants = $scope.content.variants;

                                handleHttpException(err);
                            });
                        },
                        close: function () {
                            overlayService.close();
                        }
                    };

                    overlayService.open(dialog);
                }
                else {
                    showValidationNotification();
                }
            }
            else {
                $scope.page.buttonGroupState = "busy";
                return performSave({
                    saveMethod: contentResource.sendToPublish,
                    action: "sendToPublish"
                }).then(function () {
                    $scope.page.buttonGroupState = "success";
                }, function (err) {
                    $scope.page.buttonGroupState = "error";
                    handleHttpException(err);
                });;
            }
        };

        $scope.saveAndPublish = function () {
            clearNotifications($scope.content);
            if (hasVariants($scope.content)) {
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
                            }, function (err) {
                                clearDirtyState($scope.content.variants);
                                model.submitButtonState = "error";
                                //re-map the dialog model since we've re-bound the properties
                                dialog.variants = $scope.content.variants;

                                //ensure error messages are displayed
                                formHelper.showNotifications(err.data);
                                clearNotifications($scope.content);
                                
                                handleHttpException(err);
                            });
                        },
                        close: function () {
                            overlayService.close();
                        }
                    };
                    overlayService.open(dialog);
                }
                else {
                    showValidationNotification();
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
                }, function (err) {
                    $scope.page.buttonGroupState = "error";
                    handleHttpException(err);
                });
            }
        };

        $scope.save = function () {
            clearNotifications($scope.content);
            // TODO: Add "..." to save button label if there are more than one variant to publish - currently it just adds the elipses if there's more than 1 variant
            if (hasVariants($scope.content)) {
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
                            showNotifications: false,
                            skipValidation: true
                        }).then(function (data) {
                            //show all notifications manually here since we disabled showing them automatically in the save method
                            formHelper.showNotifications(data);
                            clearNotifications($scope.content);
                            overlayService.close();
                            return $q.when(data);
                        }, function (err) {
                            clearDirtyState($scope.content.variants);
                            //model.submitButtonState = "error";
                            // Because this is the "save"-action, then we actually save though there was a validation error, therefor we will show success and display the validation errors politely.
                            if(err && err.data && err.data.ModelState && Object.keys(err.data.ModelState).length > 0) {
                                model.submitButtonState = "success";
                            } else {
                                model.submitButtonState = "error";
                                //re-map the dialog model since we've re-bound the properties
                                dialog.variants = $scope.content.variants;

                                //ensure error messages are displayed
                                formHelper.showNotifications(err.data);
                                clearNotifications($scope.content);

                                handleHttpException(err);
                            }
                        })
                    },
                    close: function (oldModel) {
                        overlayService.close();
                    }
                };

                overlayService.open(dialog);
            }
            else {
                //ensure the flags are set
                $scope.content.variants[0].save = true;
                $scope.page.saveButtonState = "busy";
                return performSave({
                    saveMethod: $scope.saveMethod(),
                    action: "save",
                    skipValidation: true
                }).then(function () {
                    $scope.page.saveButtonState = "success";
                }, function (err) {
                    // Because this is the "save"-action, then we actually save though there was a validation error, therefor we will show success and display the validation errors politely.
                    if(err && err.data && err.data.ModelState && Object.keys(err.data.ModelState).length > 0) {
                        $scope.page.saveButtonState = "success";
                    } else {
                        $scope.page.saveButtonState = "error";
                    }
                    handleHttpException(err);
                });
            }

        };

        $scope.schedule = function () {
            clearNotifications($scope.content);
            //before we launch the dialog we want to execute all client side validations first
            if (formHelper.submitForm({ scope: $scope, action: "schedule" })) {
                if (!hasVariants($scope.content)) {
                    //ensure the flags are set
                    $scope.content.variants[0].save = true;
                }

                var dialog = {
                    parentScope: $scope,
                    view: "views/content/overlays/schedule.html",
                    variants: Utilities.copy($scope.content.variants), //set a model property for the dialog
                    skipFormValidation: true, //when submitting the overlay form, skip any client side validation
                    submitButtonLabelKey: "buttons_schedulePublish",
                    submit: function (model) {
                        for (let i = 0; i < $scope.content.variants.length; i++) {
                            $scope.content.variants[i].releaseDate = model.variants[i].releaseDate;
                            $scope.content.variants[i].expireDate = model.variants[i].expireDate;
                            $scope.content.variants[i].releaseDateFormatted = model.variants[i].releaseDateFormatted;
                            $scope.content.variants[i].expireDateFormatted = model.variants[i].expireDateFormatted;
                            $scope.content.variants[i].save = model.variants[i].save;
                        }

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
                            if (!hasVariants($scope.content)) {
                                formHelper.showNotifications(err.data);
                            }
                            model.submitButtonState = "error";
                            //re-map the dialog model since we've re-bound the properties
                            dialog.variants = Utilities.copy($scope.content.variants);
                            handleHttpException(err);
                        });

                    },
                    close: function () {
                        overlayService.close();
                    }
                };
                overlayService.open(dialog);
            }
            else {
                showValidationNotification();
            }
        };

        $scope.publishDescendants = function () {
            clearNotifications($scope.content);
            //before we launch the dialog we want to execute all client side validations first
            if (formHelper.submitForm({ scope: $scope, action: "publishDescendants" })) {

                if (!hasVariants($scope.content)) {
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
                            showNotifications: false,
                            reloadChildren: model.includeUnpublished
                        }).then(function (data) {
                            //show all notifications manually here since we disabled showing them automatically in the save method
                            formHelper.showNotifications(data);
                            clearNotifications($scope.content);
                            overlayService.close();
                            return $q.when(data);
                        }, function (err) {
                            clearDirtyState($scope.content.variants);
                            //if this is invariant, show the notification errors, else they'll be shown inline with the variant
                            if (!hasVariants($scope.content)) {
                                formHelper.showNotifications(err.data);
                            }
                            model.submitButtonState = "error";
                            //re-map the dialog model since we've re-bound the properties
                            dialog.variants = $scope.content.variants;
                            handleHttpException(err);
                        });

                    },
                    close: function () {
                        overlayService.close();
                    }
                };
                overlayService.open(dialog);
            }
            else {
                showValidationNotification();
            }
        };

        $scope.preview = function (content) {
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
                var selectedVariant = $scope.content.variants[0];
                if ($scope.culture) {
                    var found = _.find($scope.content.variants, function (v) {
                        return (v.language && v.language.culture === $scope.culture);
                    });

                    if (found) {
                        selectedVariant = found;
                    }
                }

                //ensure the save flag is set
                selectedVariant.save = true;
                performSave({ saveMethod: $scope.saveMethod(), action: "save" }).then(function (data) {
                    previewWindow.location.href = redirect;
                }, function (err) {
                    //validation issues ....
                });
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
        $scope.appChanged = function (activeApp) {

            $scope.activeApp = activeApp;

            _.forEach($scope.content.apps, function (app) {
                app.active = false;
                if (app.alias === $scope.activeApp.alias) {
                    app.active = true;
                }
            });

            $scope.$broadcast("editors.apps.appChanged", { app: activeApp });

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
        $scope.onBack = function () {
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
            serverValidationManager.clear();
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
                segment: "=?",
                infiniteModel: "=?"
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').controller('Umbraco.Editors.Content.EditorDirectiveController', ContentEditController);
    angular.module('umbraco.directives').directive('contentEditor', createDirective);

})();
