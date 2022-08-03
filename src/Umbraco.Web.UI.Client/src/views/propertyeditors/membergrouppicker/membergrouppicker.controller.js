//this controller tells the dialogs service to open a memberPicker window
//with a specified callback, this callback will receive an object with a selection on it
function memberGroupPicker($scope, editorService, memberGroupResource, localizationService, overlayService){

    const vm = this;

    vm.openMemberGroupPicker = openMemberGroupPicker;
    vm.remove = remove;
    vm.clear = clear;

    function trim(str, chr) {
        var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^' + chr + '+|' + chr + '+$', 'g');
        return str.replace(rgxtrim, '');
    }

    $scope.renderModel = [];
    $scope.allowRemove = !$scope.readonly;
    $scope.allowAdd = !$scope.readonly;
    $scope.groupIds = [];

    let removeAllEntriesAction = {
        labelKey: "clipboard_labelForRemoveAllEntries",
        labelTokens: [],
        icon: "icon-trash",
        method: removeAllEntries,
        isDisabled: !$scope.allowRemove,
        useLegacyIcon: false
    };

    if ($scope.model.config && $scope.umbProperty) {
        $scope.umbProperty.setPropertyActions([
            removeAllEntriesAction
        ]);
    }

    if ($scope.model.value) {
        var groupIds = $scope.model.value.split(',');

        memberGroupResource.getByIds(groupIds).then(function(groups) {
            $scope.renderModel = groups;
        });

        removeAllEntriesAction.isDisabled = groupIds.length === 0 || !$scope.allowRemove;
    }

    function setDirty() {
        if ($scope.modelValueForm) {
            $scope.modelValueForm.modelValue.$setDirty();
        }
    }

    function openMemberGroupPicker($event) {
        if (!$scope.allowAdd) {
            $event.preventDefault();
            $event.stopPropagation();
            return;
        }

        var memberGroupPicker = {
            multiPicker: true,
            submit: function (model) {
                var selectedGroupIds = _.map(model.selectedMemberGroups
                    ? model.selectedMemberGroups
                    : [model.selectedMemberGroup],
                    function (id) { return parseInt(id); }
                );

                var currIds = renderModelIds();

                // figure out which groups are new and fetch them
                var newGroupIds = _.difference(selectedGroupIds, currIds);

                removeAllEntriesAction.isDisabled = (currIds.length === 0 && newGroupIds.length === 0) || !$scope.allowRemove;

                if (newGroupIds && newGroupIds.length) {
                    memberGroupResource.getByIds(newGroupIds).then(function (groups) {
                        $scope.renderModel = _.union($scope.renderModel, groups);
                        setDirty();
                        editorService.close();
                    });
                }
                else {
                    // no new groups selected
                    editorService.close();
                }
            },
            close: function() {
                editorService.close();
            }
        };
        editorService.memberGroupPicker(memberGroupPicker);
    }

    function remove(index) {
        if (!$scope.allowRemove) return;

        $scope.renderModel.splice(index, 1);

        var currIds = renderModelIds();
        removeAllEntriesAction.isDisabled = currIds.length === 0 || !$scope.allowRemove;

        setDirty();
    }

    function clear() {
        if (!$scope.allowRemove) return;

        $scope.renderModel = [];
        removeAllEntriesAction.isDisabled = true;

        setDirty();
    }

    function removeAllEntries() {
        if (!$scope.allowRemove) return;

        localizationService.localizeMany(["content_nestedContentDeleteAllItems", "general_delete"]).then(data => {
            overlayService.confirmDelete({
                title: data[1],
                content: data[0],
                close: () => {
                    overlayService.close();
                },
                submit: () => {
                    vm.clear();
                    overlayService.close();
                }
            });
        });
    }

    function renderModelIds() {
        var currIds = $scope.renderModel.map(i => i.id);
        return currIds;
    }

    var unsubscribe = $scope.$on("formSubmitting", function (ev, args) {
        $scope.model.value = trim(renderModelIds().join(), ",");
    });

    //when the scope is destroyed we need to unsubscribe
    $scope.$on('$destroy', function () {
        unsubscribe();
    });

}

angular.module('umbraco').controller("Umbraco.PropertyEditors.MemberGroupPickerController", memberGroupPicker);
