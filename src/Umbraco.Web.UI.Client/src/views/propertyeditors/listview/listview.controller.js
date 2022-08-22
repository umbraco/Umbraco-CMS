function listViewController($scope, $interpolate, $routeParams, $injector, $timeout, currentUserResource, notificationsService, iconHelper, editorState, localizationService, appState, $location, contentEditingHelper, listViewHelper, navigationService, editorService, overlayService, languageResource, mediaHelper, eventsService) {

    //this is a quick check to see if we're in create mode, if so just exit - we cannot show children for content
    // that isn't created yet, if we continue this will use the parent id in the route params which isn't what
    // we want. NOTE: This is just a safety check since when we scaffold an empty model on the server we remove
    // the list view tab entirely when it's new.
    if ($routeParams.create) {
        $scope.isNew = true;
        return;
    }

    //Now we need to check if this is for media, members or content because that will depend on the resources we use
    var contentResource, getContentTypesCallback, getListResultsCallback, deleteItemCallback, getIdCallback, createEditUrlCallback;

    //check the config for the entity type, or the current section name (since the config is only set in c#, not in pre-vals)
    if (($scope.model.config.entityType && $scope.model.config.entityType === "member") || (appState.getSectionState("currentSection") === "member")) {
        $scope.entityType = "member";
        contentResource = $injector.get('memberResource');
        getContentTypesCallback = $injector.get('memberTypeResource').getTypes;
        getListResultsCallback = contentResource.getPagedResults;
        deleteItemCallback = contentResource.deleteByKey;
        getIdCallback = function (selected) {
            return selected.key;
        };
        createEditUrlCallback = function (item) {
            return "/" + $scope.entityType + "/" + $scope.entityType + "/edit/" + item.key + "?page=" + $scope.options.pageNumber + "&listName=" + $scope.contentId;
        };
    }
    else {
        //check the config for the entity type, or the current section name (since the config is only set in c#, not in pre-vals)
        if (($scope.model.config.entityType && $scope.model.config.entityType === "media") || (appState.getSectionState("currentSection") === "media")) {
            $scope.entityType = "media";
            contentResource = $injector.get('mediaResource');
            getContentTypesCallback = $injector.get('mediaTypeResource').getAllowedTypes;
        }
        else {
            $scope.entityType = "content";
            contentResource = $injector.get('contentResource');
            getContentTypesCallback = $injector.get('contentTypeResource').getAllowedTypes;
        }
        getListResultsCallback = contentResource.getChildren;
        deleteItemCallback = contentResource.deleteById;
        getIdCallback = function (selected) {
            return selected.id;
        };
        createEditUrlCallback = function (item) {
            return "/" + $scope.entityType + "/" + $scope.entityType + "/edit/" + item.id 
                + "?list=" + $routeParams.id + "&page=" + $scope.options.pageNumber + "&filter=" + $scope.options.filter 
                + "&orderBy=" + $scope.options.orderBy + "&orderDirection=" + $scope.options.orderDirection;
        };
    }

    $scope.pagination = [];
    $scope.isNew = false;
    $scope.actionInProgress = false;
    $scope.selection = [];
    $scope.folders = [];
    $scope.page = {
        createDropdownOpen: false
    };
    $scope.listViewResultSet = {
        totalPages: 0,
        items: []
    };

    $scope.createAllowedButtonSingle = false;
    $scope.createAllowedButtonSingleWithBlueprints = false;
    $scope.createAllowedButtonMultiWithBlueprints = false;

    //when this is null, we don't check permissions
    $scope.currentNodePermissions = $scope.entityType === "content" ? contentEditingHelper.getPermissionsForContent() : null;

    //when this is null, we don't check permissions
    $scope.buttonPermissions = null;

    //When we are dealing with 'content', we need to deal with permissions on child nodes.
    // Currently there is no real good way to
    if ($scope.entityType === "content") {

        var idsWithPermissions = null;

        $scope.buttonPermissions = {
            canCopy: true,
            canCreate: true,
            canDelete: true,
            canMove: true,
            canPublish: true,
            canUnpublish: true
        };

        $scope.$watch("selection.length", function (newVal, oldVal) {

            if ((idsWithPermissions == null && newVal > 0) || (idsWithPermissions != null)) {

                //get all of the selected ids
                var ids = _.map($scope.selection, function (i) {
                    return i.id.toString();
                });

                //remove the dictionary items that don't have matching ids
                var filtered = {};
                _.each(idsWithPermissions, function (value, key, list) {
                    if (_.contains(ids, key)) {
                        filtered[key] = value;
                    }
                });
                idsWithPermissions = filtered;

                //find all ids that we haven't looked up permissions for
                var existingIds = _.keys(idsWithPermissions);
                var missingLookup = _.map(_.difference(ids, existingIds), function (i) {
                    return Number(i);
                });

                if (missingLookup.length > 0) {
                    currentUserResource.getPermissions(missingLookup).then(function (p) {
                        $scope.buttonPermissions = listViewHelper.getButtonPermissions(p, idsWithPermissions);
                    });
                }
                else {
                    $scope.buttonPermissions = listViewHelper.getButtonPermissions({}, idsWithPermissions);
                }
            }
        });

    }

    var listParamsForCurrent = $routeParams.id == $routeParams.list;
    
    $scope.options = {
        useInfiniteEditor: $scope.model.config.useInfiniteEditor === true,
        pageSize: $scope.model.config.pageSize ? $scope.model.config.pageSize : 10,
        pageNumber: (listParamsForCurrent && $routeParams.page && Number($routeParams.page) != NaN && Number($routeParams.page) > 0) ? $routeParams.page : 1,
        filter: (listParamsForCurrent && $routeParams.filter ? $routeParams.filter : '').trim(),
        orderBy: (listParamsForCurrent && $routeParams.orderBy ? $routeParams.orderBy : $scope.model.config.orderBy ? $scope.model.config.orderBy : 'VersionDate').trim(),
        orderDirection: (listParamsForCurrent && $routeParams.orderDirection ? $routeParams.orderDirection : $scope.model.config.orderDirection ? $scope.model.config.orderDirection : "desc").trim(),
        orderBySystemField: true,
        includeProperties: $scope.model.config.includeProperties ? $scope.model.config.includeProperties : [
            { alias: 'updateDate', header: 'Last edited', isSystem: 1 },
            { alias: 'updater', header: 'Last edited by', isSystem: 1 }
        ],
        layout: {
            layouts: $scope.model.config.layouts,
            activeLayout: listViewHelper.getLayout($routeParams.id, $scope.model.config.layouts)
        },
        allowBulkPublish: $scope.entityType === 'content' && $scope.model.config.bulkActionPermissions.allowBulkPublish && !$scope.readonly,
        allowBulkUnpublish: $scope.entityType === 'content' && $scope.model.config.bulkActionPermissions.allowBulkUnpublish && !$scope.readonly,
        allowBulkCopy: $scope.entityType === 'content' && $scope.model.config.bulkActionPermissions.allowBulkCopy && !$scope.readonly,
        allowBulkMove: $scope.entityType !== 'member' && $scope.model.config.bulkActionPermissions.allowBulkMove && !$scope.readonly,
        allowBulkDelete: $scope.model.config.bulkActionPermissions.allowBulkDelete && !$scope.readonly,
        allowCreate: !$scope.readonly,
        cultureName: $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture,
        readonly: $scope.readonly
    };
    
    _.each($scope.options.includeProperties, function (property) {
        property.nameExp = !!property.nameTemplate
            ? $interpolate(property.nameTemplate)
            : undefined;
    });

    //watch for culture changes in the query strings and update accordingly
    $scope.$watch(function () {
        return $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture;
    }, function (newVal, oldVal) {
        if (newVal && newVal !== oldVal) {
            //update the options
            $scope.options.cultureName = newVal;
            $scope.reloadView($scope.contentId);
        }
    });

    // Check if selected order by field is actually custom field
    for (var j = 0; j < $scope.options.includeProperties.length; j++) {
        var includedProperty = $scope.options.includeProperties[j];
        if (includedProperty.alias.toLowerCase() === $scope.options.orderBy.toLowerCase()) {
            $scope.options.orderBySystemField = includedProperty.isSystem === 1;
            break;
        }
    }

    //update all of the system includeProperties to enable sorting
    _.each($scope.options.includeProperties, function (e, i) {
        e.allowSorting = true;

        // Special case for members, only the configured system fields should be enabled sorting
        // (see MemberRepository.ApplySystemOrdering)
        if (e.isSystem && $scope.entityType === "member") {
            e.allowSorting = e.alias === "username" ||
                e.alias === "email" ||
                e.alias === "updateDate" ||
                e.alias === "createDate" ||
                e.alias === "contentTypeAlias";
        }

        if (e.isSystem) {
            //localize the header
            var key = getLocalizedKey(e.alias);
            localizationService.localize(key).then(function (v) {
                e.header = v;
            });
        }
    });

    $scope.selectLayout = function (layout) {
        $scope.options.layout.activeLayout = listViewHelper.setLayout($routeParams.id, layout, $scope.model.config.layouts);
    };

    function showNotificationsAndReset(err, reload, successMsgPromise) {

        //check if response is ysod
        if (err.status && err.status >= 500) {

            // Open ysod overlay
            overlayService.ysod(err);
        }

        $timeout(function () {
            $scope.bulkStatus = "";
            $scope.actionInProgress = false;
        }, 500);

        if (successMsgPromise)
        {
            localizationService.localize("bulk_done").then(function (v) {
                successMsgPromise.then(function (successMsg) {
                    notificationsService.success(v, successMsg);
                })
            });
        }
    }

    $scope.next = function (pageNumber) {
        $scope.options.pageNumber = pageNumber;
        $scope.reloadView($scope.contentId);
    };

    $scope.goToPage = function (pageNumber) {
        $scope.options.pageNumber = pageNumber;
        $scope.reloadView($scope.contentId);
    };

    $scope.prev = function (pageNumber) {
        $scope.options.pageNumber = pageNumber;
        $scope.reloadView($scope.contentId);
    };


    /*Loads the search results, based on parameters set in prev,next,sort and so on*/
    /*Pagination is done by an array of objects, due angularJS's funky way of monitoring state
    with simple values */

    $scope.getContent = function (contentId) {
        $scope.reloadView($scope.contentId, true);
    };

    $scope.reloadView = function (id, reloadActiveNode) {
        if (!id) {
            return;
        }
        
        $scope.viewLoaded = false;
        $scope.folders = [];

        listViewHelper.clearSelection($scope.listViewResultSet.items, $scope.folders, $scope.selection);

        getListResultsCallback(id, $scope.options).then(function (data) {

            $scope.actionInProgress = false;
            $scope.listViewResultSet = data;

            //update all values for display
            var section = appState.getSectionState("currentSection");
            if ($scope.listViewResultSet.items) {
                _.each($scope.listViewResultSet.items, function (e, index) {
                    setPropertyValues(e);
               // create the folders collection (only for media list views)
               if (section === "media" && !mediaHelper.hasFilePropertyType(e)) {
                        $scope.folders.push(e);
                    }
                });
            }

            $scope.viewLoaded = true;

            //NOTE: This might occur if we are requesting a higher page number than what is actually available, for example
            // if you have more than one page and you delete all items on the last page. In this case, we need to reset to the last
            // available page and then re-load again
            if ($scope.options.pageNumber > $scope.listViewResultSet.totalPages) {
                $scope.options.pageNumber = $scope.listViewResultSet.totalPages;

                //reload!
                $scope.reloadView(id, reloadActiveNode);
            }
            // in the media section, the list view items are by default also shown in the tree, so we need 
            // to refresh the current tree node when changing the folder contents (adding and removing)
            else if (reloadActiveNode && section === "media") {
                var activeNode = appState.getTreeState("selectedNode");
                if (activeNode) {
                    if (activeNode.expanded) {
                        navigationService.reloadNode(activeNode);
                    }
                } else {
                    navigationService.reloadSection(section);
                }
            }
        }).catch(function(error){
          // if someone attempts to add mix listviews across sections (i.e. use a members list view on content types),
          // a not-supported exception will be most likely be thrown, at least for the default list views - lets be
          // helpful and show a meaningful error message directly in content/content type UI
          if(error.data && error.data.ExceptionType && error.data.ExceptionType.indexOf("System.NotSupportedException") > -1) {
            $scope.viewLoadedError = error.errorMsg + ": " + error.data.ExceptionMessage;
          }
          $scope.viewLoaded = true;
        });
    };

    $scope.makeSearch = function() {
        if ($scope.options.filter !== null && $scope.options.filter !== undefined) {
            $scope.options.pageNumber = 1;
            $scope.reloadView($scope.contentId);
        }
    };

    $scope.onSearchStartTyping = function() {
        $scope.viewLoaded = false;
    }

    $scope.selectedItemsCount = function () {
        return $scope.selection.length;
    };

    $scope.clearSelection = function () {
        listViewHelper.clearSelection($scope.listViewResultSet.items, $scope.folders, $scope.selection);
    };

    $scope.getIcon = function (entry) {
        return iconHelper.convertFromLegacyIcon(entry.icon);
    };

    function serial(selected, fn, getStatusMsg, index) {
        return fn(selected, index).then(function (content) {
            index++;
            getStatusMsg(index, selected.length).then(function (value) {
                $scope.bulkStatus = value;
            });
            return index < selected.length ? serial(selected, fn, getStatusMsg, index) : content;
        }, function (err) {
            var reload = index > 0;
            showNotificationsAndReset(err, reload);
            return err;
        });
    }

    function applySelected(fn, getStatusMsg, getSuccessMsg, confirmMsg) {
        var selected = $scope.selection;
        if (selected.length === 0)
            return;
        if (confirmMsg && !confirm(confirmMsg))
            return;

        $scope.actionInProgress = true;

        getStatusMsg(0, selected.length).then(function (value) {
            $scope.bulkStatus = value;
        });

        return serial(selected, fn, getStatusMsg, 0).then(function (result) {
            // executes once the whole selection has been processed
            // in case of an error (caught by serial), result will be the error
            if (!(result.data && Utilities.isArray(result.data.notifications)))
                showNotificationsAndReset(result, true, getSuccessMsg(selected.length));
        });
    }

    $scope.delete = function (numberOfItems, totalItems) {

        const dialog = {
            view: "views/propertyeditors/listview/overlays/delete.html",
            deletesVariants: selectionHasVariants(),
            isTrashed: $scope.isTrashed,
            selection: $scope.selection,
            submitButtonLabelKey: "contentTypeEditor_yesDelete",
            submitButtonStyle: "danger",
            submit: function (model) {
                performDelete();
                overlayService.close();
            },
            close: function () {
                overlayService.close();
            },
            numberOfItems: numberOfItems,
            totalItems: totalItems
        };

        localizationService.localize("general_delete").then(value => {
            dialog.title = value;
            overlayService.open(dialog);
        });
    };

    function performDelete() {
        applySelected(
            function (selected, index) { return deleteItemCallback(getIdCallback(selected[index])); },
            function (count, total) {
                var key = (total === 1 ? "bulk_deletedItemOfItem" : "bulk_deletedItemOfItems");
                return localizationService.localize(key, [count, total]);
            },
            function (total) {
                var key = (total === 1 ? "bulk_deletedItem" : "bulk_deletedItems");
                return localizationService.localize(key, [total]);
            }).then(function () {
                $scope.reloadView($scope.contentId, true);
            });
    }

    function selectionHasVariants() {
        let variesByCulture = false;

        // check if any of the selected nodes has variants
        $scope.selection.forEach(selectedItem => {
            $scope.listViewResultSet.items.forEach(resultItem => {
                if ((selectedItem.id === resultItem.id || selectedItem.key === resultItem.key) && resultItem.variesByCulture) {
                    variesByCulture = true;
                }
            })
        });

        return variesByCulture;
    }

    $scope.publish = function () {

        const dialog = {
            view: "views/propertyeditors/listview/overlays/listviewpublish.html",
            submitButtonLabelKey: "actions_publish",
            submit: function (model) {
                // create a comma separated array of selected cultures
                let selectedCultures = [];
                if (model.languages && model.languages.length > 0) {
                    model.languages.forEach(language => {
                        if (language.publish) {
                            selectedCultures.push(language.culture);
                        }
                    });
                }
                performPublish(selectedCultures);
                overlayService.close();
            },
            close: function () {
                overlayService.close();
            }
        };

        // if any of the selected nodes has variants we want to 
        // show a dialog where the languages can be chosen
        if (selectionHasVariants()) {
            languageResource.getAll()
                .then(languages => {
                    dialog.languages = languages;
                    overlayService.open(dialog);
                }, error => {
                    notificationsService.error(error);
                });
        } else {
            overlayService.open(dialog);
        }

    };

    function performPublish(cultures) {
        applySelected(
            function (selected, index) { return contentResource.publishById(getIdCallback(selected[index]), cultures); },
            function (count, total) {
                var key = (total === 1 ? "bulk_publishedItemOfItem" : "bulk_publishedItemOfItems");
                return localizationService.localize(key, [count, total]);
            },
            function (total) {
                var key = (total === 1 ? "bulk_publishedItem" : "bulk_publishedItems");
                return localizationService.localize(key, [total]);
            }).then(function () {
                $scope.reloadView($scope.contentId);
            });
    }

    $scope.unpublish = function () {

        const dialog = {
            view: "views/propertyeditors/listview/overlays/listviewunpublish.html",
            submitButtonLabelKey: "actions_unpublish",
            submitButtonStyle: "warning",
            selection: $scope.selection,
            submit: function (model) {
                // create a comma separated array of selected cultures
                let selectedCultures = [];
                if (model.languages && model.languages.length > 0) {
                    model.languages.forEach(language => {
                        if (language.unpublish) {
                            selectedCultures.push(language.culture);
                        }
                    });
                }
                performUnpublish(selectedCultures);
                overlayService.close();
            },
            close: function () {
                overlayService.close();
            }
        };

        // if any of the selected nodes has variants we want to 
        // show a dialog where the languages can be chosen
        if (selectionHasVariants()) {
            languageResource.getAll()
                .then(languages => {
                    dialog.languages = languages;
                    overlayService.open(dialog);
                }, error => {
                    notificationsService.error(error);
                });
        } else {
            overlayService.open(dialog);
        }

    };

    function performUnpublish(cultures) {
        applySelected(
            function (selected, index) { return contentResource.unpublish(getIdCallback(selected[index]), cultures); },
            function (count, total) {
                var key = (total === 1 ? "bulk_unpublishedItemOfItem" : "bulk_unpublishedItemOfItems");
                return localizationService.localize(key, [count, total]);
            },
            function (total) {
                var key = (total === 1 ? "bulk_unpublishedItem" : "bulk_unpublishedItems");
                return localizationService.localize(key, [total]);
            }).then(function () {
                $scope.reloadView($scope.contentId, true);
            });
    }

    $scope.move = function () {
        var move = {
            section: $scope.entityType,
            currentNode: $scope.contentId,
            submit: function (model) {
                if (model.target) {
                    performMove(model.target);
                }
                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        }
        editorService.move(move);
    };


    function performMove(target) {

        //NOTE: With the way this applySelected/serial works, I'm not sure there's a better way currently to return
        // a specific value from one of the methods, so we'll have to try this way. Even though the first method
        // will fire once per every node moved, the destination path will be the same and we need to use that to sync.
        var newPath = null;
        applySelected(
            function (selected, index) {
                return contentResource.move({ parentId: target.id, id: getIdCallback(selected[index]) })
                    .then(function (path) {
                        newPath = path;
                        return path;
                    });
            },
            function (count, total) {
                var key = (total === 1 ? "bulk_movedItemOfItem" : "bulk_movedItemOfItems");
                return localizationService.localize(key, [count, total]);
            },
            function (total) {
                var key = (total === 1 ? "bulk_movedItem" : "bulk_movedItems");
                return localizationService.localize(key, [total]);
            })
            .then(function () {
                //executes if all is successful, let's sync the tree
                if (newPath) {
                    // reload the current view so the moved items are no longer shown
                    $scope.reloadView($scope.contentId);

                    //we need to do a double sync here: first refresh the node where the content was moved,
                    // then refresh the node where the content was moved from
                    navigationService.syncTree({
                        tree: target.nodeType ? target.nodeType : (target.metaData.treeAlias),
                        path: newPath,
                        forceReload: true,
                        activate: false
                    })
                        .then(function (args) {
                            //get the currently edited node (if any)
                            var activeNode = appState.getTreeState("selectedNode");
                            if (activeNode) {
                                navigationService.reloadNode(activeNode);
                            }
                        });
                }
            });
    }

    $scope.copy = function () {
        var copyEditor = {
            section: $scope.entityType,
            currentNode: $scope.contentId,
            submit: function (model) {
                if (model.target) {
                    performCopy(model.target, model.relateToOriginal, model.includeDescendants);
                }
                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        };
        editorService.copy(copyEditor);
    };

    function performCopy(target, relateToOriginal, includeDescendants) {
        applySelected(
            function (selected, index) { return contentResource.copy({ parentId: target.id, id: getIdCallback(selected[index]), relateToOriginal: relateToOriginal, recursive: includeDescendants }); },
            function (count, total) {
                var key = (total === 1 ? "bulk_copiedItemOfItem" : "bulk_copiedItemOfItems");
                return localizationService.localize(key, [count, total]);
            },
            function (total) {
                var key = (total === 1 ? "bulk_copiedItem" : "bulk_copiedItems");
                return localizationService.localize(key, [total]);
            });
    }

    function getCustomPropertyValue(alias, properties) {
        var value = '';
        var index = 0;
        var foundAlias = false;
        for (var i = 0; i < properties.length; i++) {
            if (properties[i].alias == alias) {
                foundAlias = true;
                break;
            }
            index++;
        }

        if (foundAlias) {
            value = properties[index].value;
        }

        return value;
    }

    /** This ensures that the correct value is set for each item in a row, we don't want to call a function during interpolation or ng-bind as performance is really bad that way */
    function setPropertyValues(result) {

        //set the edit url
        result.editPath = createEditUrlCallback(result);

        _.each($scope.options.includeProperties, function (e, i) {

            var alias = e.alias;

            // First try to pull the value directly from the alias (e.g. updatedBy)
            var value = result[alias];

            // If this returns an object, look for the name property of that (e.g. owner.name)
            if (value === Object(value)) {
                value = value['name'];
            }

            // If we've got nothing yet, look at a user defined property
            if (typeof value === 'undefined') {
                value = getCustomPropertyValue(alias, result.properties);
            }

            // If we have a date, format it
            if (isDate(value)) {
                value = value.substring(0, value.length - 3);
            }

            if (e.nameExp) {
                var newValue = e.nameExp({ value });
                if (newValue && (newValue = newValue.trim())) {
                    value = newValue;
                }
            }

            // set what we've got on the result
            result[alias] = value;
        });
    }

    function isDate(val) {
        if (Utilities.isString(val)) {
            return val.match(/^(\d{4})\-(\d{2})\-(\d{2})\ (\d{2})\:(\d{2})\:(\d{2})$/);
        }
        return false;
    }

    function initView() {
        
        var id = $routeParams.id;
        if (id === undefined) {
            // no ID found in route params - don't list anything as we don't know for sure where we are
            return;
        }
        
        // Get current id for node to load it's children
        $scope.contentId = editorState.current ? editorState.current.id : id;
        $scope.isTrashed = editorState.current ? editorState.current.trashed : id === "-20" || id === "-21";

        $scope.options.allowBulkPublish = $scope.options.allowBulkPublish && !$scope.isTrashed;
        $scope.options.allowBulkUnpublish = $scope.options.allowBulkUnpublish && !$scope.isTrashed;
        $scope.options.allowBulkCopy = $scope.options.allowBulkCopy && !$scope.isTrashed;

        $scope.options.bulkActionsAllowed = $scope.options.allowBulkPublish ||
            $scope.options.allowBulkUnpublish ||
            $scope.options.allowBulkCopy ||
            $scope.options.allowBulkMove ||
            $scope.options.allowBulkDelete;

        if ($scope.isTrashed === false) {
            getContentTypesCallback(id).then(function (listViewAllowedTypes) {
                $scope.listViewAllowedTypes = listViewAllowedTypes;

                var blueprints = false;
                _.each(listViewAllowedTypes, function (allowedType) {
                    if (_.isEmpty(allowedType.blueprints)) {
                        // this helps the view understand that there are no blueprints available
                        allowedType.blueprints = null;
                    }
                    else {
                        blueprints = true;
                        // turn the content type blueprints object into an array of sortable objects for the view
                        allowedType.blueprints = _.map(_.pairs(allowedType.blueprints || {}), function (pair) {
                            return {
                                id: pair[0],
                                name: pair[1]
                            };
                        });
                    }
                });

                if (listViewAllowedTypes.length === 1 && blueprints === false) {
                    $scope.createAllowedButtonSingle = true;
                }
                if (listViewAllowedTypes.length === 1 && blueprints === true) {
                    $scope.createAllowedButtonSingleWithBlueprints = true;
                }
                if (listViewAllowedTypes.length > 1) {
                    $scope.createAllowedButtonMultiWithBlueprints = true;
                }
            });
        }

        $scope.reloadView($scope.contentId);
    }

    function getLocalizedKey(alias) {

        switch (alias) {
            case "sortOrder":
                return "general_sort";
            case "updateDate":
                return "content_updateDate";
            case "updater":
                return "content_updatedBy";
            case "createDate":
                return "content_createDate";
            case "owner":
                return "content_createBy";
            case "published":
                return "content_isPublished";
            case "contentTypeAlias":
                return $scope.entityType === "content"
                    ? "content_documentType"
                    : $scope.entityType === "media"
                        ? "content_mediatype"
                        : "content_membertype";
            case "email":
                return "general_email";
            case "username":
                return "general_username";
        }
        return alias;
    }

    function getItemKey(itemId) {
        for (var i = 0; i < $scope.listViewResultSet.items.length; i++) {
            var item = $scope.listViewResultSet.items[i];
            if (item.id === itemId) {
                return item.key;
            }
        }
    }

    function createBlank(entityType, docTypeAlias) {
        if ($scope.options.useInfiniteEditor) {
            
            var editorModel = {
                create: true,
                submit: function(model) {
                    editorService.close();
                    $scope.reloadView($scope.contentId);
                },
                close: function() {
                    editorService.close();
                    $scope.reloadView($scope.contentId);
                }
            };

            if (entityType == "content")
            {
                editorModel.parentId = $scope.contentId;
                editorModel.documentTypeAlias = docTypeAlias;
                editorService.contentEditor(editorModel);
                return;
            }

            if (entityType == "media")
            {
                editorService.mediaEditor(editorModel);
                return;
            }

            if (entityType == "member")
            {
                editorModel.doctype = docTypeAlias;
                editorService.memberEditor(editorModel);
                return;
            }
        }

        $location
            .path("/" + entityType + "/" + entityType + "/edit/" + $scope.contentId)
            .search("doctype", docTypeAlias)
            .search("create", "true");
    }

    function createFromBlueprint(entityType, docTypeAlias, blueprintId) {
        $location
            .path("/" + entityType + "/" + entityType + "/edit/" + $scope.contentId)
            .search("doctype", docTypeAlias)
            .search("create", "true")
            .search("blueprintId", blueprintId);
    }

    function toggleDropdown () {
        $scope.page.createDropdownOpen = !$scope.page.createDropdownOpen;
    }

    function leaveDropdown () {
        $scope.page.createDropdownOpen = false;
    }

    $scope.createBlank = createBlank;
    $scope.createFromBlueprint = createFromBlueprint;
    $scope.toggleDropdown = toggleDropdown;
    $scope.leaveDropdown = leaveDropdown;

    // if this listview has sort order in it, make sure it is updated when sorting is performed on the current content
    if (_.find($scope.options.includeProperties, property => property.alias === "sortOrder")) {
        var eventSubscription = eventsService.on("sortCompleted", function (e, args) {
            if (parseInt(args.id) === parseInt($scope.contentId)) {
                $scope.reloadView($scope.contentId);
            }
        });

        $scope.$on('$destroy', function () {
            eventsService.unsubscribe(eventSubscription);
        });
    }

    //GO!
    initView();
}


angular.module("umbraco").controller("Umbraco.PropertyEditors.ListViewController", listViewController);
