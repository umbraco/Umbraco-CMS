(function () {
    'use strict';

    function ContentEditController($rootScope, $scope, $routeParams, $q, $timeout, $window, $location,
        appState, contentResource, entityResource, navigationService, notificationsService, angularHelper,
        serverValidationManager, contentEditingHelper, treeService, fileManager, formHelper, umbRequestHelper,
        keyboardService, umbModelMapper, editorState, $http, eventsService, relationResource, overlayService, localizationService) {

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
        $scope.page.hideChangeVariant = infiniteMode ? true : false;
        $scope.allowOpen = true;

        // add all editors to an editors array to support split view 
        $scope.editors = [];
        $scope.initVariant = initVariant;
        $scope.splitViewChanged = splitViewChanged;

        function init(content) {

            if (infiniteMode) {
                createInfiniteModeButtons(content);
            } else {
                createButtons(content);
            }

            editorState.set($scope.content);

            //We fetch all ancestors of the node to generate the footer breadcrumb navigation
            if (!$scope.page.isNew) {
                if (content.parentId && content.parentId !== -1) {
                    entityResource.getAncestors(content.id, "document")
                        .then(function (anc) {
                            $scope.ancestors = anc;
                        });
                }
            }

            bindEvents();

            // set first app to active
            // We need to track active
            $scope.content.apps[0].active = true;

            setActiveCulture();

            resetVariantFlags();            
        }

        /** This is called when the split view changes based on the umb-variant-content */
        function splitViewChanged() {
            //send an event downwards
            $scope.$broadcast("editors.content.splitViewChanged", { editors: $scope.editors });
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

        function countDirtyVariants() {
            var count = 0;
            for (var i = 0; i < $scope.content.variants.length; i++) {
                var v = $scope.content.variants[i];
                if (v.isDirty) {
                    count++;
                }
            }
            return count;
        }

        /** Returns true if the save/publish dialog should be shown when pressing the button */
        function showSaveOrPublishDialog() {
            return $scope.content.variants.length > 1;
        }

        /**
         * The content item(s) are loaded into an array and this will set the active content item based on the current culture (query string).
         * If the content item is invariant, then only one item exists in the array.
         */
        function setActiveCulture() {
            // set the active variant
            var activeVariant = null;
            _.each($scope.content.variants, function (v) {
                if (v.language && v.language.culture === $scope.culture) {
                    v.active = true;
                    activeVariant = v;
                }
                else {
                    v.active = false;
                }
            });
            if (!activeVariant) {
                // set the first variant to active
                $scope.content.variants[0].active = true;
                activeVariant = $scope.content.variants[0];
            }

            initVariant(activeVariant);

            //If there are no editors yet then create one with the current content.
            //if there's already a main editor then update it with the current content.
            if ($scope.editors.length === 0) {
                var editor = {
                    content: activeVariant
                };
                $scope.editors.push(editor);
            }
            else {
                //this will mean there is only one
                $scope.editors[0].content = activeVariant;

                if ($scope.editors.length > 1) {
                    //now re-sync any other editor content (i.e. if split view is open)
                    for (var s = 1; s < $scope.editors.length; s++) {
                        //get the variant from the scope model
                        var variant = _.find($scope.content.variants, function (v) {
                            return v.language.culture === $scope.editors[s].content.language.culture;
                        });
                        $scope.editors[s].content = initVariant(variant);
                    }
                }

            }
        }

        function initVariant(variant) {
            //The model that is assigned to the editor contains the current content variant along
            //with a copy of the contentApps. This is required because each editor renders it's own
            //header and content apps section and the content apps contains the view for editing content itself
            //and we need to assign a view model to the subView so that it is scoped to the current
            //editor so that split views work. 

            //copy the apps from the main model if not assigned yet to the variant
            if (!variant.apps) {
                variant.apps = angular.copy($scope.content.apps);
            }

            //if this is a variant has a culture/language than we need to assign the language drop down info 
            if (variant.language) {
                //if the variant list that defines the header drop down isn't assigned to the variant then assign it now
                if (!variant.variants) {
                    variant.variants = _.map($scope.content.variants,
                        function (v) {
                            return _.pick(v, "active", "language", "state");
                        });
                }
                else {
                    //merge the scope variants on top of the header variants collection (handy when needing to refresh)
                    angular.extend(variant.variants,
                        _.map($scope.content.variants,
                            function (v) {
                                return _.pick(v, "active", "language", "state");
                            }));
                }

                //ensure the current culture is set as the active one
                for (var i = 0; i < variant.variants.length; i++) {
                    if (variant.variants[i].language.culture === variant.language.culture) {
                        variant.variants[i].active = true;
                    }
                    else {
                        variant.variants[i].active = false;
                    }
                }
            }

            //then assign the variant to a view model to the content app
            var contentApp = _.find(variant.apps, function (a) {
                return a.alias === "content";
            });
            contentApp.viewModel = variant;

            return variant;
        }

        function bindEvents() {
            //bindEvents can be called more than once and we don't want to have multiple bound events
            for (var e in evts) {
                eventsService.unsubscribe(evts[e]);
            }

            evts.push(eventsService.on("editors.content.changePublishDate", function (event, args) {
                createButtons(args.node);
            }));

            evts.push(eventsService.on("editors.content.changeUnpublishDate", function (event, args) {
                createButtons(args.node);
            }));

            evts.push(eventsService.on("editors.documentType.saved", function (name, args) {
                // if this content item uses the updated doc type we need to reload the content item
                if (args && args.documentType && args.documentType.key === content.documentType.key) {
                    loadContent();
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

                    init($scope.content);

                    if (!infiniteMode) {
                        syncTreeNode($scope.content, true);
                    }

                    resetLastListPageNumber($scope.content);

                    eventsService.emit("content.loaded", { content: $scope.content });

                    return $q.resolve($scope.content);


                });

        }

        function createButtons(content) {
            $scope.page.buttonGroupState = "init";
            var buttons = contentEditingHelper.configureContentEditorButtons({
                create: $scope.page.isNew,
                content: content,
                methods: {
                    saveAndPublish: $scope.saveAndPublish,
                    sendToPublish: $scope.sendToPublish,
                    save: $scope.save,
                    unPublish: $scope.unPublish
                }
            });

            $scope.defaultButton = buttons.defaultButton;
            $scope.subButtons = buttons.subButtons;

        }

        // create infinite editing buttons
        function createInfiniteModeButtons(content) {

            $scope.page.allowInfinitePublishAndClose = false;
            $scope.page.allowInfiniteSaveAndClose = false;

            // check for publish rights
            if (_.contains(content.allowedActions, "U")) {
                $scope.page.allowInfinitePublishAndClose = true;

                // check for save rights
            } else if (_.contains(content.allowedActions, "A")) {
                $scope.page.allowInfiniteSaveAndClose = true;
            }

        }

        /** Syncs the content item to it's tree node - this occurs on first load and after saving */
        function syncTreeNode(content, initialLoad) {

            var path = content.path;

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

        // This is a helper method to reduce the amount of code repitition for actions: Save, Publish, SendToPublish
        function performSave(args) {

            $scope.page.buttonGroupState = "busy";

            eventsService.emit("content.saving", { content: $scope.content, action: args.action });

            return contentEditingHelper.contentEditorPerformSave({
                saveMethod: args.saveMethod,
                scope: $scope,
                content: $scope.content,
                action: args.action,
                showNotifications: args.showNotifications
            }).then(function (data) {
                //success            
                init($scope.content);

                if (!infiniteMode) {
                    syncTreeNode($scope.content);
                }

                $scope.page.buttonGroupState = "success";

                eventsService.emit("content.saved", { content: $scope.content, action: args.action });

                return $q.when(data);
            },
                function (err) {

                    setActiveCulture();
                    syncTreeNode($scope.content);

                    //error
                    if (err) {
                        editorState.set($scope.content);
                    }

                    $scope.page.buttonGroupState = "error";

                    return $q.reject(err);
                });
        }

        function resetLastListPageNumber(content) {
            // We're using rootScope to store the page number for list views, so if returning to the list
            // we can restore the page.  If we've moved on to edit a piece of content that's not the list or it's children
            // we should remove this so as not to confuse if navigating to a different list
            if (!content.isChildOfListView && !content.isContainer) {
                $rootScope.lastListViewPageViewed = null;
            }
        }

        if ($scope.page.isNew) {

            $scope.page.loading = true;

            //we are creating so get an empty content item
            $scope.getScaffoldMethod()()
                .then(function (data) {

                    $scope.content = data;

                    init($scope.content);

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

        $scope.unPublish = function () {

            //if there's any variants than we need to set the language and include the variants to publish
            var culture = null;
            if ($scope.content.variants.length > 0) {
                _.each($scope.content.variants,
                    function (d) {
                        //set the culture if this is active
                        if (d.active === true) {
                            culture = d.language.culture;
                        }
                    });
            }

            if (formHelper.submitForm({ scope: $scope, skipValidation: true })) {

                $scope.page.buttonGroupState = "busy";

                eventsService.emit("content.unpublishing", { content: $scope.content });

                contentResource.unPublish($scope.content.id, culture)
                    .then(function (data) {

                        formHelper.resetForm({ scope: $scope });

                        contentEditingHelper.handleSuccessfulSave({
                            scope: $scope,
                            savedContent: data,
                            rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                        });

                        init($scope.content);

                        if (!infiniteMode) {
                            syncTreeNode($scope.content);
                        }

                        $scope.page.buttonGroupState = "success";

                        eventsService.emit("content.unpublished", { content: $scope.content });

                    }, function (err) {
                        $scope.page.buttonGroupState = 'error';
                    });
            }

        };

        $scope.sendToPublish = function () {
            return performSave({ saveMethod: contentResource.sendToPublish, action: "sendToPublish" });
        };

        $scope.saveAndPublish = function () {

            // TODO: Add "..." to publish button label if there are more than one variant to publish - currently it just adds the elipses if there's more than 1 variant
            if (showSaveOrPublishDialog()) {
                //before we launch the dialog we want to execute all client side validations first
                if (formHelper.submitForm({ scope: $scope, action: "publish" })) {

                    var dialog = {
                        view: "views/content/overlays/publish.html",
                        variants: $scope.content.variants, //set a model property for the dialog
                        skipFormValidation: true, //when submitting the overlay form, skip any client side validation
                        submitButtonLabel: "Publish",
                        submit: function (model) {
                            model.submitButtonState = "busy";

                            //we need to return this promise so that the dialog can handle the result and wire up the validation response
                            return performSave({
                                saveMethod: contentResource.publish,
                                action: "publish",
                                showNotifications: false
                            }).then(function (data) {
                                overlayService.close();
                                return $q.when(data);
                            },
                                function (err) {
                                    model.submitButtonState = "error";
                                    //re-map the dialog model since we've re-bound the properties
                                    dialog.variants = $scope.content.variants;

                                    return $q.reject(err);
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
                //ensure the publish flag is set
                $scope.content.variants[0].publish = true;
                return performSave({ saveMethod: contentResource.publish, action: "publish" });
            }
        };

        $scope.save = function () {

            // TODO: Add "..." to save button label if there are more than one variant to publish - currently it just adds the elipses if there's more than 1 variant
            if (showSaveOrPublishDialog()) {
                //before we launch the dialog we want to execute all client side validations first
                if (formHelper.submitForm({ scope: $scope, action: "save" })) {

                    var dialog = {
                        view: "views/content/overlays/save.html",
                        variants: $scope.content.variants, //set a model property for the dialog
                        skipFormValidation: true, //when submitting the overlay form, skip any client side validation
                        submitButtonLabel: "Save",
                        submit: function (model) {
                            model.submitButtonState = "busy";

                            //we need to return this promise so that the dialog can handle the result and wire up the validation response
                            return performSave({
                                saveMethod: $scope.saveMethod(),
                                action: "save",
                                showNotifications: false
                            }).then(function (data) {
                                overlayService.close();
                                return $q.when(data);
                            },
                                function (err) {
                                    model.submitButtonState = "error";
                                    //re-map the dialog model since we've re-bound the properties
                                    dialog.variants = $scope.content.variants;

                                    return $q.reject(err);
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
                return performSave({ saveMethod: $scope.saveMethod(), action: "save" });
            }

        };

        $scope.preview = function (content) {


            if (!$scope.busy) {

                // Chromes popup blocker will kick in if a window is opened 
                // without the initial scoped request. This trick will fix that.
                //  
                var previewWindow = $window.open('preview/?init=true&id=' + content.id, 'umbpreview');

                // Build the correct path so both /#/ and #/ work.
                var redirect = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/preview/?id=' + content.id;

                //The user cannot save if they don't have access to do that, in which case we just want to preview
                //and that's it otherwise they'll get an unauthorized access message
                if (!_.contains(content.allowedActions, "A")) {
                    previewWindow.location.href = redirect;
                }
                else {
                    $scope.save().then(function (data) {
                        previewWindow.location.href = redirect;
                    });
                }
            }
        };

        $scope.backToListView = function () {
            $location.path($scope.page.listViewPath);
        };

        $scope.restore = function (content) {

            $scope.page.buttonRestore = "busy";

            relationResource.getByChildId(content.id, "relateParentDocumentOnDelete").then(function (data) {

                var relation = null;
                var target = null;
                var error = { headline: "Cannot automatically restore this item", content: "Use the Move menu item to move it manually" };

                if (data.length === 0) {
                    notificationsService.error(error.headline, "There is no 'restore' relation found for this node. Use the Move menu item to move it manually.");
                    $scope.page.buttonRestore = "error";
                    return;
                }

                relation = data[0];

                if (relation.parentId === -1) {
                    target = { id: -1, name: "Root" };
                    moveNode(content, target);
                } else {
                    contentResource.getById(relation.parentId).then(function (data) {
                        target = data;

                        // make sure the target item isn't in the recycle bin
                        if (target.path.indexOf("-20") !== -1) {
                            notificationsService.error(error.headline, "The item you want to restore it under (" + target.name + ") is in the recycle bin. Use the Move menu item to move the item manually.");
                            $scope.page.buttonRestore = "error";
                            return;
                        }

                        moveNode(content, target);

                    }, function (err) {
                        $scope.page.buttonRestore = "error";
                        notificationsService.error(error.headline, error.content);
                    });
                }

            }, function (err) {
                $scope.page.buttonRestore = "error";
                notificationsService.error(error.headline, error.content);
            });
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

        function moveNode(node, target) {

            contentResource.move({ "parentId": target.id, "id": node.id })
                .then(function (path) {

                    // remove the node that we're working on
                    if ($scope.page.menu.currentNode) {
                        treeService.removeNode($scope.page.menu.currentNode);
                    }

                    // sync the destination node
                    if (!infiniteMode) {
                        navigationService.syncTree({ tree: "content", path: path, forceReload: true, activate: false });
                    }

                    $scope.page.buttonRestore = "success";
                    notificationsService.success("Successfully restored " + node.name + " to " + target.name);

                    // reload the node
                    loadContent();

                }, function (err) {
                    $scope.page.buttonRestore = "error";
                    notificationsService.error("Cannot automatically restore this item", err);
                });

        }

        // methods for infinite editing
        $scope.close = function () {
            if ($scope.infiniteModel.close) {
                $scope.infiniteModel.close($scope.infiniteModel);
            }
        };

        $scope.$watch('culture', function (newVal, oldVal) {
            if (newVal !== oldVal) {
                setActiveCulture();
            }
        });

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
