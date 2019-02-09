
/**
 * The controller that is used for a couple different Property Editors: Multi Node Tree Picker, Content Picker,
 * since this is used by MNTP and it supports content, media and members, there is code to deal with all 3 of those types
 * @param {any} $scope
 * @param {any} entityResource
 * @param {any} editorState
 * @param {any} iconHelper
 * @param {any} $routeParams
 * @param {any} angularHelper
 * @param {any} navigationService
 * @param {any} $location
 * @param {any} localizationService
 */
function contentPickerController($scope, entityResource, editorState, iconHelper, $routeParams, angularHelper, navigationService, $location, localizationService, editorService, $q) {

    var unsubscribe;

    function subscribe() {
        unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
            var currIds = _.map($scope.renderModel, function (i) {
                return $scope.model.config.idType === "udi" ? i.udi : i.id;
            });
            $scope.model.value = trim(currIds.join(), ",");
        });
    }

    function trim(str, chr) {
        var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^' + chr + '+|' + chr + '+$', 'g');
        return str.replace(rgxtrim, '');
    }

    /** Performs validation based on the renderModel data */
    function validate() {
        if ($scope.contentPickerForm) {
            angularHelper.getCurrentForm($scope).$setDirty();
            //Validate!
            if ($scope.model.config && $scope.model.config.minNumber && parseInt($scope.model.config.minNumber) > $scope.renderModel.length) {
                $scope.contentPickerForm.minCount.$setValidity("minCount", false);
            }
            else {
                $scope.contentPickerForm.minCount.$setValidity("minCount", true);
            }

            if ($scope.model.config && $scope.model.config.maxNumber && parseInt($scope.model.config.maxNumber) < $scope.renderModel.length) {
                $scope.contentPickerForm.maxCount.$setValidity("maxCount", false);
            }
            else {
                $scope.contentPickerForm.maxCount.$setValidity("maxCount", true);
            }
        }
    }

    function startWatch() {

        //due to the way angular-sortable works, it needs to update a model, we don't want it to update renderModel since renderModel
        //is updated based on changes to model.value so if we bound angular-sortable to that and put a watch on it we'd end up in a
        //infinite loop. Instead we have a custom array model for angular-sortable and we'll watch that which we'll use to sync the model.value
        //which in turn will sync the renderModel.
        $scope.$watchCollection("sortableModel", function (newVal, oldVal) {
            $scope.model.value = newVal.join();
        });

        //if the underlying model changes, update the view model, this ensures that the view is always consistent with the underlying
        //model if it changes (i.e. based on server updates, or if used in split view, etc...)
        $scope.$watch("model.value", function (newVal, oldVal) {
            if (newVal !== oldVal) {
                syncRenderModel();
            }
        });
    }

    $scope.renderModel = [];
    $scope.sortableModel = [];

    $scope.dialogEditor = editorState && editorState.current && editorState.current.isDialogEditor === true;

    //the default pre-values
    var defaultConfig = {
        multiPicker: false,
        showOpenButton: false,
        showEditButton: false,
        showPathOnHover: false,
        maxNumber: 1,
        minNumber: 0,
        startNode: {
            query: "",
            type: "content",
            id: $scope.model.config.startNodeId ? $scope.model.config.startNodeId : -1 // get start node for simple Content Picker
        }
    };

    // sortable options
    $scope.sortableOptions = {
        axis: "y",
        containment: "parent",
        distance: 10,
        opacity: 0.7,
        tolerance: "pointer",
        scroll: true,
        zIndex: 6000,
        update: function (e, ui) {
            angularHelper.getCurrentForm($scope).$setDirty();
        }
    };

    if ($scope.model.config) {
        //merge the server config on top of the default config, then set the server config to use the result
        $scope.model.config = angular.extend(defaultConfig, $scope.model.config);
    }

    //Umbraco persists boolean for prevalues as "0" or "1" so we need to convert that!
    $scope.model.config.multiPicker = Object.toBoolean($scope.model.config.multiPicker);
    $scope.model.config.showOpenButton = Object.toBoolean($scope.model.config.showOpenButton);
    $scope.model.config.showEditButton = Object.toBoolean($scope.model.config.showEditButton);
    $scope.model.config.showPathOnHover = Object.toBoolean($scope.model.config.showPathOnHover);

    var entityType = $scope.model.config.startNode.type === "member"
        ? "Member"
        : $scope.model.config.startNode.type === "media"
            ? "Media"
            : "Document";
    $scope.allowOpenButton = entityType === "Document";
    $scope.allowEditButton = entityType === "Document";
    $scope.allowRemoveButton = true;

    //the dialog options for the picker
    var dialogOptions = {
        multiPicker: $scope.model.config.multiPicker,
        entityType: entityType,
        filterCssClass: "not-allowed not-published",
        startNodeId: null,
        currentNode: editorState ? editorState.current : null,
        callback: function (data) {
            if (angular.isArray(data)) {
                _.each(data, function (item, i) {
                    $scope.add(item);
                });
            } else {
                $scope.clear();
                $scope.add(data);
            }
            angularHelper.getCurrentForm($scope).$setDirty();
        },
        treeAlias: $scope.model.config.startNode.type,
        section: $scope.model.config.startNode.type,
        idType: "int"
    };

    //since most of the pre-value config's are used in the dialog options (i.e. maxNumber, minNumber, etc...) we'll merge the 
    // pre-value config on to the dialog options
    angular.extend(dialogOptions, $scope.model.config);

    //We need to manually handle the filter for members here since the tree displayed is different and only contains
    // searchable list views
    if (entityType === "Member") {
        //first change the not allowed filter css class
        dialogOptions.filterCssClass = "not-allowed";
        var currFilter = dialogOptions.filter;
        //now change the filter to be a method
        dialogOptions.filter = function (i) {
            //filter out the list view nodes
            if (i.metaData.isContainer) {
                return true;
            }
            if (!currFilter) {
                return false;
            }
            //now we need to filter based on what is stored in the pre-vals, this logic duplicates what is in the treepicker.controller, 
            // but not much we can do about that since members require special filtering.
            var filterItem = currFilter.toLowerCase().split(',');
            var found = filterItem.indexOf(i.metaData.contentType.toLowerCase()) >= 0;
            if (!currFilter.startsWith("!") && !found || currFilter.startsWith("!") && found) {
                return true;
            }

            return false;
        }
    }
    if ($routeParams.section === "settings" && $routeParams.tree === "documentTypes") {
        //if the content-picker is being rendered inside the document-type editor, we don't need to process the startnode query
        dialogOptions.startNodeId = -1;
    }
    else if ($scope.model.config.startNode.query) {
        //if we have a query for the startnode, we will use that.
        var rootId = $routeParams.id;
        entityResource.getByQuery($scope.model.config.startNode.query, rootId, "Document").then(function (ent) {
            dialogOptions.startNodeId = $scope.model.config.idType === "udi" ? ent.udi : ent.id;
        });
    }
    else {
        dialogOptions.startNodeId = $scope.model.config.startNode.id;
    }

    //dialog
    $scope.openCurrentPicker = function () {
        $scope.currentPicker = dialogOptions;

        $scope.currentPicker.submit = function (model) {
            if (angular.isArray(model.selection)) {
                _.each(model.selection, function (item, i) {
                    $scope.add(item);
                });
                angularHelper.getCurrentForm($scope).$setDirty();
            }
            angularHelper.getCurrentForm($scope).$setDirty();
            editorService.close();
        }

        $scope.currentPicker.close = function () {
            editorService.close();
        }

        //open the correct editor based on the entity type
        switch (entityType) {
            case "Document":
                editorService.contentPicker($scope.currentPicker);
                break;
            case "Media":
                editorService.mediaPicker($scope.currentPicker);
                break;
            case "Member":
                editorService.memberPicker($scope.currentPicker);
                break;

            default:
        }

    };

    $scope.remove = function (index) {
        var currIds = $scope.model.value ? $scope.model.value.split(',') : [];
        if (currIds.length > 0) {
            currIds.splice(index, 1);
            angularHelper.getCurrentForm($scope).$setDirty();
            $scope.model.value = currIds.join();
        }
    };

    $scope.showNode = function (index) {
        var item = $scope.renderModel[index];
        var id = item.id;
        var section = $scope.model.config.startNode.type.toLowerCase();

        entityResource.getPath(id, entityType).then(function (path) {
            navigationService.changeSection(section);
            navigationService.showTree(section, {
                tree: section, path: path, forceReload: false, activate: true
            });
            var routePath = section + "/" + section + "/edit/" + id.toString();
            $location.path(routePath).search("");
        });
    }

    $scope.add = function (item) {
        var currIds = $scope.model.value ? $scope.model.value.split(',') : [];

        var itemId = $scope.model.config.idType === "udi" ? item.udi : item.id;

        if (currIds.indexOf(itemId) < 0) {
            currIds.push(itemId);
            $scope.model.value = currIds.join();
        }
    };

    $scope.clear = function () {
        $scope.model.value = null;
    };

    $scope.openContentEditor = function (node) {
        var contentEditor = {
            id: node.id,
            submit: function (model) {
                // update the node
                node.name = model.contentNode.name;
                node.published = model.contentNode.hasPublishedVersion;
                if (entityType !== "Member") {
                    entityResource.getUrl(model.contentNode.id, entityType).then(function (data) {
                        node.url = data;
                    });
                }
                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        };
        editorService.contentEditor(contentEditor);
    };

    //when the scope is destroyed we need to unsubscribe
    $scope.$on('$destroy', function () {
        if (unsubscribe) {
            unsubscribe();
        }
    });

    /** Syncs the renderModel based on the actual model.value and returns a promise */
    function syncRenderModel(doValidation) {

        var valueIds = $scope.model.value ? $scope.model.value.split(',') : [];

        //sync the sortable model
        $scope.sortableModel = valueIds;

        //load current data if anything selected
        if (valueIds.length > 0) {

            //need to determine which items we already have loaded
            var renderModelIds = _.map($scope.renderModel, function (d) {
                return ($scope.model.config.idType === "udi" ? d.udi : d.id).toString();
            });

            //get the ids that no longer exist
            var toRemove = _.difference(renderModelIds, valueIds);


            //remove the ones that no longer exist
            for (var j = 0; j < toRemove.length; j++) {
                var index = renderModelIds.indexOf(toRemove[j]);
                $scope.renderModel.splice(index, 1);
            }

            //get the ids that we need to lookup entities for
            var missingIds = _.difference(valueIds, renderModelIds);

            if (missingIds.length > 0) {
                return entityResource.getByIds(missingIds, entityType).then(function (data) {

                    _.each(valueIds,
                        function (id, i) {
                            var entity = _.find(data, function (d) {
                                return $scope.model.config.idType === "udi" ? (d.udi == id) : (d.id == id);
                            });

                            if (entity) {
                                addSelectedItem(entity);
                            }

                        });

                    if (doValidation) {
                        validate();
                    }

                    setSortingState($scope.renderModel);
                    return $q.when(true);
                });
            }
            else {
                //if there's nothing missing, make sure it's sorted correctly

                var current = $scope.renderModel;
                $scope.renderModel = [];
                for (var k = 0; k < valueIds.length; k++) {
                    var id = valueIds[k];
                    var found = _.find(current, function (d) {
                        return $scope.model.config.idType === "udi" ? (d.udi == id) : (d.id == id);
                    });
                    if (found) {
                        $scope.renderModel.push(found);
                    }
                }

                if (doValidation) {
                    validate();
                }

                setSortingState($scope.renderModel);
                return $q.when(true);
            }
        }
        else {
            $scope.renderModel = [];
            if (validate) {
                validate();
            }
            setSortingState($scope.renderModel);
            return $q.when(true);
        }

    }

    function setEntityUrl(entity) {

        // get url for content and media items
        if (entityType !== "Member") {
            entityResource.getUrl(entity.id, entityType).then(function (data) {
                // update url                
                angular.forEach($scope.renderModel, function (item) {
                    if (item.id === entity.id) {
                        if (entity.trashed) {
                            item.url = localizationService.dictionary.general_recycleBin;
                        } else {
                            item.url = data;
                        }
                    }
                });
            });
        }

    }

    function addSelectedItem(item) {

        // set icon
        if (item.icon) {
            item.icon = iconHelper.convertFromLegacyIcon(item.icon);
        }

        // set default icon
        if (!item.icon) {
            switch (entityType) {
                case "Document":
                    item.icon = "icon-document";
                    break;
                case "Media":
                    item.icon = "icon-picture";
                    break;
                case "Member":
                    item.icon = "icon-user";
                    break;
            }
        }

        $scope.renderModel.push({
            "name": item.name,
            "id": item.id,
            "udi": item.udi,
            "icon": item.icon,
            "path": item.path,
            "url": item.url,
            "trashed": item.trashed,
            "published": (item.metaData && item.metaData.IsPublished === false && entityType === "Document") ? false : true
            // only content supports published/unpublished content so we set everything else to published so the UI looks correct 
        });

        setEntityUrl(item);
    }

    function setSortingState(items) {
        // disable sorting if the list only consist of one item
        if (items.length > 1) {
            $scope.sortableOptions.disabled = false;
        } else {
            $scope.sortableOptions.disabled = true;
        }
    }

    function init() {
        syncRenderModel(false).then(function () {
            //everything is loaded, start the watch on the model
            startWatch();
            subscribe();
        });
    }

    init();

}

angular.module('umbraco').controller("Umbraco.PropertyEditors.ContentPickerController", contentPickerController);
