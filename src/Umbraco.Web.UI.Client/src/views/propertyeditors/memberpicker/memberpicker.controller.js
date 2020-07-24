//this controller simply tells the dialogs service to open a memberPicker window
//with a specified callback, this callback will receive an object with a selection on it
function memberPickerController($scope, $q, entityResource, iconHelper, angularHelper, editorService, localizationService){

    var vm = {
        labels: {
            general_add: ""
        }
    };

    function trim(str, chr) {
        var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^' + chr + '+|' + chr + '+$', 'g');
        return str.replace(rgxtrim, '');
    }

    var unsubscribe;

    function subscribe() {
        unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
            var currIds = _.map($scope.renderModel, function (i) {
                return $scope.model.config.idType === "udi" ? i.udi : i.id;
            });
            $scope.model.value = trim(currIds.join(), ",");
        });
    }

    /** Performs validation based on the renderModel data */
    function validate() {
        if ($scope.memberPickerForm) {
            //Validate!
            if ($scope.model.config && $scope.model.config.minNumber && parseInt($scope.model.config.minNumber) > $scope.renderModel.length) {
                $scope.memberPickerForm.minCount.$setValidity("minCount", false);
            }
            else {
                $scope.memberPickerForm.minCount.$setValidity("minCount", true);
            }

            if ($scope.model.config && $scope.model.config.maxNumber && parseInt($scope.model.config.maxNumber) < $scope.renderModel.length) {
                $scope.memberPickerForm.maxCount.$setValidity("maxCount", false);
            }
            else {
                $scope.memberPickerForm.maxCount.$setValidity("maxCount", true);
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
                syncRenderModel(true);
            }
        });
    }

    //the default pre-values
    var defaultConfig = {
        multiPicker: false,
        maxNumber: 1,
        minNumber: 0
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

    $scope.renderModel = [];
    $scope.sortableModel = [];
    $scope.allowRemove = true;

    $scope.labels = vm.labels;

    var entityType = "Member";

    var dialogOptions = {
        multiPicker: $scope.model.config.multiPicker,
        entityType: entityType,
        section: "member",
        treeAlias: "member",
        filter: function(i) {
            return i.metaData.isContainer == true;
        },
        filterCssClass: "not-allowed",
        callback: function(data) {
            if (Utilities.isArray(data)) {
                _.each(data, function (item, i) {
                    $scope.add(item);
                });
            } else {
                $scope.clear();
                $scope.add(data);
            }
            angularHelper.getCurrentForm($scope).$setDirty();
        }
    };

    //since most of the pre-value config's are used in the dialog options (i.e. maxNumber, minNumber, etc...) we'll merge the
    // pre-value config on to the dialog options
    if ($scope.model.config) {
        
        //merge the server config on top of the default config, then set the server config to use the result
        $scope.model.config = Utilities.extend(defaultConfig, $scope.model.config);

        // if the property is mandatory, set the minCount config to 1 (unless of course it is set to something already),
        // that way the minCount/maxCount validation handles the mandatory as well
        if ($scope.model.validation && $scope.model.validation.mandatory && !$scope.model.config.minNumber) {
            $scope.model.config.minNumber = 1;
        }

        // if we can't pick more than one item, explicitly disable multiPicker in the dialog options
        if ($scope.model.config.maxNumber && parseInt($scope.model.config.maxNumber) === 1) {
            dialogOptions.multiPicker = false;
        }
    }

    $scope.model.config.multiPicker = Object.toBoolean($scope.model.config.multiPicker);

    //since most of the pre-value config's are used in the dialog options (i.e. maxNumber, minNumber, etc...) we'll merge the
    // pre-value config on to the dialog options
    Utilities.extend(dialogOptions, $scope.model.config);

    // if we can't pick more than one item, explicitly disable multiPicker in the dialog options
    if ($scope.model.config.maxNumber && parseInt($scope.model.config.maxNumber) === 1) {
        dialogOptions.multiPicker = false;
    }

    $scope.openMemberPicker = function () {

        var memberPicker = dialogOptions;

        memberPicker.submit = function (model) {
            if (model.selection) {
                _.each(model.selection, function (item, i) {
                    $scope.add(item);
                });
            }
            editorService.close();
        };

        memberPicker.close = function () {
            editorService.close();
        };

        editorService.treePicker(memberPicker);
    };

    $scope.remove = function (index) {
        var currIds = $scope.model.value ? $scope.model.value.split(',') : [];
        if (currIds.length > 0) {
            currIds.splice(index, 1);
            angularHelper.getCurrentForm($scope).$setDirty();
            $scope.model.value = currIds.join();
        }
    };

    $scope.add = function (item) {
        var currIds = $scope.model.value ? $scope.model.value.split(',') : [];

        var itemId = ($scope.model.config.idType === "udi" ? item.udi : item.id).toString();

        if (currIds.indexOf(itemId) < 0) {
            currIds.push(itemId);
            $scope.model.value = currIds.join();
        }
    };

    $scope.clear = function() {
        $scope.renderModel = [];
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
            if (doValidation) {
                validate();
            }
            setSortingState($scope.renderModel);
            return $q.when(true);
        }

    }

    function addSelectedItem(item) {

        // set icon
        if (item.icon) {
            item.icon = iconHelper.convertFromLegacyIcon(item.icon);
        }

        // set default icon
        if (!item.icon) {
            item.icon = "icon-user";
        }

        $scope.renderModel.push({
            "name": item.name,
            "id": item.id,
            "udi": item.udi,
            "icon": item.icon,
            "path": item.path,
            "url": item.url
        });
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

        //load member data
        //var modelIds = $scope.model.value ? $scope.model.value.split(',') : [];
        //entityResource.getByIds(modelIds, "Member").then(function (data) {
        //    _.each(data, function (item, i) {
        //        // set default icon if it's missing
        //        item.icon = (item.icon) ? iconHelper.convertFromLegacyIcon(item.icon) : "icon-user";
        //        $scope.renderModel.push({ name: item.name, id: item.id, udi: item.udi, icon: item.icon });
        //    });
        //});

        localizationService.localizeMany(["general_add"])
            .then(function (data) {
                vm.labels.general_add = data[0];

                syncRenderModel(false).then(function () {
                    //everything is loaded, start the watch on the model
                    startWatch();
                    subscribe();
                    validate();
                });
            });
    }

    init();
}


angular.module('umbraco').controller("Umbraco.PropertyEditors.MemberPickerController", memberPickerController);
