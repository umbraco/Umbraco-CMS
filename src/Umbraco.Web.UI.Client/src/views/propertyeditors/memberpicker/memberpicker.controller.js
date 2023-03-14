//this controller simply tells the dialogs service to open a memberPicker window
//with a specified callback, this callback will receive an object with a selection on it
function memberPickerController($scope, entityResource, iconHelper, editorService){

    function trim(str, chr) {
        var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^' + chr + '+|' + chr + '+$', 'g');
        return str.replace(rgxtrim, '');
    }

    $scope.renderModel = [];
    $scope.allowRemove = !$scope.readonly;
    $scope.allowAdd = !$scope.readonly;

    var dialogOptions = {
        multiPicker: false,
        entityType: "Member",
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
        }
    };

    function setDirty() {
        if ($scope.modelValueForm) {
            $scope.modelValueForm.modelValue.$setDirty();
        }
    }

    //since most of the pre-value config's are used in the dialog options (i.e. maxNumber, minNumber, etc...) we'll merge the
    // pre-value config on to the dialog options
    if ($scope.model.config) {
        Utilities.extend(dialogOptions, $scope.model.config);
    }

    $scope.openMemberPicker = function () {
        if(!$scope.allowAdd) return;

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
        if (!$scope.allowRemove) return;

        $scope.renderModel.splice(index, 1);
        setDirty();
    };

    $scope.add = function (item) {
        if (!$scope.allowAdd) return; 

        var currIds = _.map($scope.renderModel, function (i) {
            if ($scope.model.config.idType === "udi") {
                return i.udi;
            }
            else {
                return i.id;
            }            
        });

        var itemId = $scope.model.config.idType === "udi" ? item.udi : item.id;

        if (currIds.indexOf(itemId) < 0) {
            item.icon = iconHelper.convertFromLegacyIcon(item.icon);
            $scope.renderModel.push({ name: item.name, id: item.id, udi: item.udi, icon: item.icon });
            setDirty();
        }
    };

    $scope.clear = function() {
        if (!$scope.allowRemove) return;

        $scope.renderModel = [];
    };

    var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
        var currIds = _.map($scope.renderModel, function (i) {
            if ($scope.model.config.idType === "udi") {
                return i.udi;
            }
            else {
                return i.id;
            }   
        });
        $scope.model.value = trim(currIds.join(), ",");
    });

    //when the scope is destroyed we need to unsubscribe
    $scope.$on('$destroy', function () {
        unsubscribe();
    });

    //load member data
    var modelIds = $scope.model.value ? $scope.model.value.split(',') : [];
    entityResource.getByIds(modelIds, "Member").then(function (data) {
        _.each(data, function (item, i) {
            // set default icon if it's missing
            item.icon = (item.icon) ? iconHelper.convertFromLegacyIcon(item.icon) : "icon-user";
            $scope.renderModel.push({ name: item.name, id: item.id, udi: item.udi, icon: item.icon });
        });
    });
}


angular.module('umbraco').controller("Umbraco.PropertyEditors.MemberPickerController", memberPickerController);
