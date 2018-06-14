(function () {
    'use strict';

    function ContentEditController($rootScope, $scope, $routeParams, $q, $timeout, $window, $location,
        appState, contentResource, entityResource, navigationService, notificationsService, angularHelper,
        serverValidationManager, contentEditingHelper, treeService, fileManager, formHelper, umbRequestHelper,
        keyboardService, umbModelMapper, editorState, $http, eventsService, relationResource, overlayService, localizationService) {

        var evts = [];

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
        $scope.page.culture = $scope.culture;
        $scope.allowOpen = true;

        // add all editors to an editors array to support split view 
        $scope.editors = [];
        $scope.splitView = {
            "leftIsOpen": true,
            "rightIsOpen": false
        };

        function init(content) {

            createButtons(content);

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

            //init can be called more than once and we don't want to have multiple bound events
            for (var e in evts) {
                eventsService.unsubscribe(evts[e]);
            }

            evts.push(eventsService.on("editors.content.changePublishDate", function (event, args) {
                createButtons(args.node);
            }));

            evts.push(eventsService.on("editors.content.changeUnpublishDate", function (event, args) {
                createButtons(args.node);
            }));

            // We don't get the info tab from the server from version 7.8 so we need to manually add it
            //contentEditingHelper.addInfoTab($scope.content.tabs);

            // prototype content and info apps
            var contentApp = {
                "name": "Content",
                "alias": "content",
                "icon": "icon-document",
                "view": "views/content/apps/content/content.html"
            };

            var infoApp = {
                "name": "Info",
                "alias": "info",
                "icon": "icon-info",
                "view": "views/content/apps/info/info.html"
            };

            var listview = {
                "name": "Child items",
                "alias": "childItems",
                "icon": "icon-list",
                "view": "views/content/apps/listview/listview.html"
            };

            $scope.content.apps = [];

            if ($scope.content.isContainer) {
                // add list view app
                $scope.content.apps.push(listview);

                // remove the list view tab
                angular.forEach($scope.content.tabs, function (tab, index) {
                    if (tab.alias === "umbContainerView") {
                        tab.hide = true;
                    }
                });
            }

            $scope.content.apps.push(contentApp);
            $scope.content.apps.push(infoApp);

            // set first app to active
            $scope.content.apps[0].active = true;

            // create new editor for split view
            if ($scope.editors.length === 0) {
                var editor = {
                    content: $scope.content
                };
                $scope.editors.push(editor);
            }
            else if ($scope.editors.length === 1) {
                $scope.editors[0].content = $scope.content;
            }
            else {
                //fixme - need to fix something here if we are re-loading a content item that is in a split view
            }
        }

        /**
         *  This does the content loading and initializes everything, called on load and changing variants
         * @param {any} culture
         */
        function loadContent(culture) {

            $scope.page.loading = true;

            //we are editing so get the content item from the server
            $scope.getMethod()($scope.contentId, culture)
                .then(function (data) {

                    $scope.content = data;

                    if (data.isChildOfListView && data.trashed === false) {
                        $scope.page.listViewPath = ($routeParams.page) ?
                            "/content/content/edit/" + data.parentId + "?page=" + $routeParams.page :
                            "/content/content/edit/" + data.parentId;
                    }

                    init($scope.content);

                    //in one particular special case, after we've created a new item we redirect back to the edit
                    // route but there might be server validation errors in the collection which we need to display
                    // after the redirect, so we will bind all subscriptions which will show the server validation errors
                    // if there are any and then clear them so the collection no longer persists them.
                    serverValidationManager.executeAndClearAllSubscriptions();

                    syncTreeNode($scope.content, data.path, true);

                    resetLastListPageNumber($scope.content);

                    $scope.page.loading = false;

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

        /** Syncs the content item to it's tree node - this occurs on first load and after saving */
        function syncTreeNode(content, path, initialLoad) {

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

            return contentEditingHelper.contentEditorPerformSave({
                saveMethod: args.saveMethod,
                scope: $scope,
                content: $scope.content,
                action: args.action
            }).then(function (data) {
                //success            
                init($scope.content);
                syncTreeNode($scope.content, data.path);

                $scope.page.buttonGroupState = "success";
                return $q.when(data);
            },
                function (err) {
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

                    $scope.page.loading = false;

                });
        }
        else {

            //Browse content nodes based on the selected tree language variant
            if ($scope.page.culture) {
                loadContent($scope.page.culture);
            }
            else {
                loadContent();
            }
        }

        $scope.unPublish = function () {

            //if there's any variants than we need to set the language and include the variants to publish
            var culture = null;
            if ($scope.content.variants.length > 0) {
                _.each($scope.content.variants,
                    function (d) {
                        //set the culture if this is current
                        if (d.current === true) {
                            culture = d.language.culture;
                        }
                    });
            }

            if (formHelper.submitForm({ scope: $scope, skipValidation: true })) {

                $scope.page.buttonGroupState = "busy";

                contentResource.unPublish($scope.content.id, culture)
                    .then(function (data) {

                        formHelper.resetForm({ scope: $scope, notifications: data.notifications });

                        contentEditingHelper.handleSuccessfulSave({
                            scope: $scope,
                            savedContent: data,
                            rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                        });

                        init($scope.content);

                        syncTreeNode($scope.content, data.path);

                        $scope.page.buttonGroupState = "success";

                    }, function (err) {
            formHelper.showNotifications(err.data);
                        $scope.page.buttonGroupState = 'error';
                    });
            }

        };

        $scope.sendToPublish = function () {
            return performSave({ saveMethod: contentResource.sendToPublish, action: "sendToPublish" });
        };

        $scope.saveAndPublish = function () {

            // TODO: Add "..." to publish button label if there are more than one variant to publish - currently it just adds the elipses if there's more than 1 variant
            if ($scope.content.variants.length > 1) {
                //before we launch the dialog we want to execute all client side validations first
                if (formHelper.submitForm({ scope: $scope, action: "publish"})) {

                    var dialog = {
                        view: "publish",
                        variants: $scope.content.variants, //set a model property for the dialog
                        skipFormValidation: true, //when submitting the overlay form, skip any client side validation
                        submitButtonLabel: "Publish",
                        submit: function (model) {
                            model.submitButtonState = "busy";

                            //we need to return this promise so that the dialog can handle the result and wire up the validation response
                            return performSave({
                                saveMethod: contentResource.publish,
                                action: "publish"
                            }).then(function (data) {
                                overlayService.close();
                                return $q.when(data);
                            },
                                function (err) {
                                    model.submitButtonState = "error";
                                    //re-map the dialog model since we've re-bound the properties
                                    dialog.variants = $scope.content.variants;

                                    //check the error list for specific variant errors, if none exist that means that only server side validation
                                    //for the current variant's properties failed, in this case we want to close the publish dialog since the user
                                    //will need to fix validation errors on the properties
                                    if (err.data && err.data.ModelState) {
                                        var keys = _.keys(err.data.ModelState);
                                        var foundVariantError = _.find(keys,
                                            function (k) {
                                                return k.startsWith("publish_variant_");
                                            });
                                        if (!foundVariantError) {
                                            //no variant errors, close the dialog
                                            overlayService.close();
                                        }
                                    }

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
                return performSave({ saveMethod: contentResource.publish, action: "publish" }).catch(angular.noop);;
            }
        };

        $scope.save = function () {
            return performSave({ saveMethod: $scope.saveMethod(), action: "save" }).catch(angular.noop);
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
        
        $scope.closeSplitView = function (index, editor) {
            // hacky animation stuff - it will be much better when angular is upgraded
            editor.loading = true;
            editor.collapsed = true;
            $timeout(function () {
                $scope.editors.splice(index, 1);
            }, 400);
        };

        $scope.openInSplitView = function (selectedVariant) {

            console.log(selectedVariant);

            var editor = {};
            // hacking animation states - these should hopefully be easier to do when we upgrade angular
            editor.collapsed = true;
            editor.loading = true;
            $scope.editors.push(editor);
            var editorIndex = $scope.editors.length - 1;
            $timeout(function () {
                $scope.editors[editorIndex].collapsed = false;
            }, 100);

            // fake loading of content
            // TODO: Make this real, but how do we deal with saving since currently we only save one variant at a time?!
            $timeout(function () {
                $scope.editors[editorIndex].content = angular.copy($scope.content);
                $scope.editors[editorIndex].content.name = "What a variant";
                // set selected variant on split view content
                angular.forEach($scope.editors[editorIndex].content.variants, function (variant) {
                    if (variant.culture === selectedVariant.culture) {
                        variant.current = true;
                    } else {
                        variant.current = false;
                    }
                });
                $scope.editors[editorIndex].loading = false;
            }, 500);
        };

        function moveNode(node, target) {

            contentResource.move({ "parentId": target.id, "id": node.id })
                .then(function (path) {

                    // remove the node that we're working on
                    if ($scope.page.menu.currentNode) {
                        treeService.removeNode($scope.page.menu.currentNode);
                    }

                    // sync the destination node
                    navigationService.syncTree({ tree: "content", path: path, forceReload: true, activate: false });

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
        $scope.close = function() {
            if($scope.model.close) {
                $scope.model.close($scope.model);
            }
        };

        //ensure to unregister from all events!
        $scope.$on('$destroy', function () {
            for (var e in evts) {
                eventsService.unsubscribe(evts[e]);
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
                model: "=?"
            }
        };

        return directive;

    }

    angular.module('umbraco.directives').controller('Umbraco.Editors.Content.EditorDirectiveController', ContentEditController);
    angular.module('umbraco.directives').directive('contentEditor', createDirective);

})();
