//this controller simply tells the dialogs service to open a mediaPicker window
//with a specified callback, this callback will receive an object with a selection on it

function contentPickerController($scope, dialogService, entityResource, editorState, $log, iconHelper, $routeParams, fileManager, contentEditingHelper) {

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
                return i.id;
            }).join();
        }, function (newVal) {
            var currIds = _.map($scope.renderModel, function (i) {
                return i.id;
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
        });
    }

    $scope.renderModel = [];
	    
    $scope.dialogEditor = editorState && editorState.current && editorState.current.isDialogEditor === true;

    //the default pre-values
    var defaultConfig = {
        multiPicker: false,
        showEditButton: false,        
        startNode: {
            query: "",
            type: "content",
            id: -1
        }
    };

    if ($scope.model.config) {
        //merge the server config on top of the default config, then set the server config to use the result
        $scope.model.config = angular.extend(defaultConfig, $scope.model.config);
    }

    //Umbraco persists boolean for prevalues as "0" or "1" so we need to convert that!
    $scope.model.config.multiPicker = ($scope.model.config.multiPicker === "1" ? true : false);
    $scope.model.config.showEditButton = ($scope.model.config.showEditButton === "1" ? true : false);

    var entityType = $scope.model.config.startNode.type === "member"
        ? "Member"
        : $scope.model.config.startNode.type === "media"
        ? "Media"
        : "Document";

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
        },
        treeAlias: $scope.model.config.startNode.type,
        section: $scope.model.config.startNode.type
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


    //if we have a query for the startnode, we will use that. 
    if ($scope.model.config.startNode.query) {
        var rootId = $routeParams.id;
        entityResource.getByQuery($scope.model.config.startNode.query, rootId, "Document").then(function (ent) {
            dialogOptions.startNodeId = ent.id;
        });
    } else {
        dialogOptions.startNodeId = $scope.model.config.startNode.id;
    }        
    
    //dialog
    $scope.openContentPicker = function () {                
        var d = dialogService.treePicker(dialogOptions);
    };

    $scope.remove = function (index) {
        $scope.renderModel.splice(index, 1);
    };
        
    $scope.add = function (item) {
        var currIds = _.map($scope.renderModel, function (i) {
            return i.id;
        });

        if (currIds.indexOf(item.id) < 0) {
            item.icon = iconHelper.convertFromLegacyIcon(item.icon);
            $scope.renderModel.push({ name: item.name, id: item.id, icon: item.icon });
        }
    };

    $scope.clear = function () {
        $scope.renderModel = [];
    };
        
    $scope.$on("formSubmitting", function (ev, args) {
        var currIds = _.map($scope.renderModel, function (i) {
            return i.id;
        });
        $scope.model.value = trim(currIds.join(), ",");
    });

    //load current data
    var modelIds = $scope.model.value ? $scope.model.value.split(',') : [];
    entityResource.getByIds(modelIds, entityType).then(function (data) {

        //Ensure we populate the render model in the same order that the ids were stored!
        _.each(modelIds, function (id, i) {
            var entity = _.find(data, function (d) {                
                return d.id == id;
            });
           
            if (entity) {
                entity.icon = iconHelper.convertFromLegacyIcon(entity.icon);
                $scope.renderModel.push({ name: entity.name, id: entity.id, icon: entity.icon });
            }
            
           
        });

        //everything is loaded, start the watch on the model
        startWatch();

    });
}

angular.module('umbraco').controller("Umbraco.PropertyEditors.ContentPickerController", contentPickerController);
