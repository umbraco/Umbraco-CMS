//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it

function contentPickerController($scope, entityResource, editorState, iconHelper, $routeParams, angularHelper, navigationService, $location, miniEditorHelper, localizationService) {

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

    function startWatch() {
        //We need to watch our renderModel so that we can update the underlying $scope.model.value properly, this is required
        // because the ui-sortable doesn't dispatch an event after the digest of the sort operation. Any of the events for UI sortable
        // occur after the DOM has updated but BEFORE the digest has occured so the model has NOT changed yet - it even states so in the docs.
        // In their source code there is no event so we need to just subscribe to our model changes here.
        //This also makes it easier to manage models, we update one and the rest will just work.
        $scope.$watch(function () {
            //return the joined Ids as a string to watch
            return _.map($scope.renderModel, function (i) {
                return $scope.model.config.idType === "udi" ? i.udi : i.id;                 
            }).join();
        }, function (newVal) {
            var currIds = _.map($scope.renderModel, function (i) {
                return $scope.model.config.idType === "udi" ? i.udi : i.id;                
            });
            $scope.model.value = trim(currIds.join(), ",");

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

            setSortingState($scope.renderModel);

        });
    }

    $scope.renderModel = [];
	    
    $scope.dialogEditor = editorState && editorState.current && editorState.current.isDialogEditor === true;

    //the default pre-values
    var defaultConfig = {
        multiPicker: false,
        showOpenButton: false,
        showEditButton: false,
        showPathOnHover: false,
        startNode: {
            query: "",
            type: "content",
	        id: $scope.model.config.startNodeId ? $scope.model.config.startNodeId : -1 // get start node for simple Content Picker
        }
    };

    // sortable options
    $scope.sortableOptions = {
        distance: 10,
        tolerance: "pointer",
        scroll: true,
        zIndex: 6000
    };

    if ($scope.model.config) {
        //merge the server config on top of the default config, then set the server config to use the result
        $scope.model.config = angular.extend(defaultConfig, $scope.model.config);
    }

    //Umbraco persists boolean for prevalues as "0" or "1" so we need to convert that!
    $scope.model.config.multiPicker = ($scope.model.config.multiPicker === "1" ? true : false);
    $scope.model.config.showOpenButton = ($scope.model.config.showOpenButton === "1" ? true : false);
    $scope.model.config.showEditButton = ($scope.model.config.showEditButton === "1" ? true : false);
    $scope.model.config.showPathOnHover = ($scope.model.config.showPathOnHover === "1" ? true : false);
 
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
        dialogOptions.filter = function(i) {
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
    } else if ($scope.model.config.startNode.query) {
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
    $scope.openContentPicker = function() {
      $scope.contentPickerOverlay = dialogOptions;
      $scope.contentPickerOverlay.view = "treepicker";
      $scope.contentPickerOverlay.show = true;

      $scope.contentPickerOverlay.submit = function(model) {

          if (angular.isArray(model.selection)) {
             _.each(model.selection, function (item, i) {
                  $scope.add(item);
             });
          }

          $scope.contentPickerOverlay.show = false;
          $scope.contentPickerOverlay = null;
      }

      $scope.contentPickerOverlay.close = function(oldModel) {
          $scope.contentPickerOverlay.show = false;
          $scope.contentPickerOverlay = null;
      }

    };

    $scope.remove = function (index) {
        $scope.renderModel.splice(index, 1);
        angularHelper.getCurrentForm($scope).$setDirty();
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
        var currIds = _.map($scope.renderModel, function (i) {
            return $scope.model.config.idType === "udi" ? i.udi : i.id;
        });

        var itemId = $scope.model.config.idType === "udi" ? item.udi : item.id;

        if (currIds.indexOf(itemId) < 0) {
            setEntityUrl(item);
        }
    };

    $scope.clear = function () {
        $scope.renderModel = [];
    };

    $scope.openMiniEditor = function(node) {
        miniEditorHelper.launchMiniEditor(node).then(function(updatedNode){
            // update the node
            node.name = updatedNode.name;
            node.published = updatedNode.hasPublishedVersion;
            if(entityType !== "Member") {
                entityResource.getUrl(updatedNode.id, entityType).then(function(data){
                    node.url = data;
                });
            }
        });
    };

    //when the scope is destroyed we need to unsubscribe
    $scope.$on('$destroy', function () {
        if(unsubscribe) {
            unsubscribe();
        }
    });
    
    var modelIds = $scope.model.value ? $scope.model.value.split(',') : [];

    //load current data if anything selected
    if (modelIds.length > 0) {
        entityResource.getByIds(modelIds, entityType).then(function(data) {

            _.each(modelIds,
                function(id, i) {
                    var entity = _.find(data,
                        function(d) {
                            return $scope.model.config.idType === "udi" ? (d.udi == id) : (d.id == id);
                        });

                    if (entity) {
                        setEntityUrl(entity);
                    }

                });

            //everything is loaded, start the watch on the model
            startWatch();
            subscribe();
        });
    }
    else {
        //everything is loaded, start the watch on the model
        startWatch();
        subscribe();
    }

    function setEntityUrl(entity) {

        // get url for content and media items
        if(entityType !== "Member") {
            entityResource.getUrl(entity.id, entityType).then(function(data){
                // update url                
                angular.forEach($scope.renderModel, function(item){
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

        // add the selected item to the renderModel
        // if it needs to show a url the item will get 
        // updated when the url comes back from server
        addSelectedItem(entity);

    }

    function addSelectedItem(item) {

        // set icon
        if(item.icon) {
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

    }

    function setSortingState(items) {
        // disable sorting if the list only consist of one item
        if(items.length > 1) {
            $scope.sortableOptions.disabled = false;
        } else {
            $scope.sortableOptions.disabled = true;
        }
    }

}

angular.module('umbraco').controller("Umbraco.PropertyEditors.ContentPickerController", contentPickerController);
