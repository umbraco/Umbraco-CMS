//this controller simply tells the dialogs service to open a memberPicker window
//with a specified callback, this callback will receive an object with a selection on it
function memberGroupPicker($scope, editorService, memberGroupResource, localizationService, overlayService){

    function trim(str, chr) {
        var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^' + chr + '+|' + chr + '+$', 'g');
        return str.replace(rgxtrim, '');
    }

    var removeAllEntriesAction = {
        labelKey: 'clipboard_labelForRemoveAllEntries',
        labelTokens: [],
        icon: 'trash',
        method: removeAllEntries,
        isDisabled: true
    };

    $scope.renderModel = [];
    $scope.allowRemove = true;
    $scope.groupIds = [];

    if ($scope.model.config && $scope.umbProperty) {

        var propertyActions = [
            removeAllEntriesAction
        ];

        $scope.umbProperty.setPropertyActions(propertyActions);
    }

    if ($scope.model.value) {
        var groupIds = $scope.model.value.split(',');

        memberGroupResource.getByIds(groupIds).then(function(groups) {
            $scope.renderModel = groups;
        });

        removeAllEntriesAction.isDisabled = groupIds.length === 0;
    }

    $scope.openMemberGroupPicker = function() {
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

                removeAllEntriesAction.isDisabled = currIds.length === 0 && newGroupIds.length === 0;

                if (newGroupIds && newGroupIds.length) {
                    memberGroupResource.getByIds(newGroupIds).then(function (groups) {
                        $scope.renderModel = _.union($scope.renderModel, groups);
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
    };

    $scope.remove =function(index){
        $scope.renderModel.splice(index, 1);
    };

    $scope.add = function (item) {

        var currIds = renderModelIds();

        if (currIds.indexOf(item) < 0) {
            $scope.renderModel.push({ name: item, id: item, icon: 'icon-users' });
        }

        removeAllEntriesAction.isDisabled = groupIds.length === 0;
    };

    $scope.clear = function() {
        $scope.renderModel = [];
        removeAllEntriesAction.isDisabled = true;
    };

    function removeAllEntries() {
        localizationService.localizeMany(["content_nestedContentDeleteAllItems", "general_delete"]).then(data => {
            overlayService.confirmDelete({
                title: data[1],
                content: data[0],
                close: () => {
                    overlayService.close();
                },
                submit: () => {
                    $scope.clear();
                    overlayService.close();
                }
            });
        });
    }

    function renderModelIds() {

        var currIds = _.map($scope.renderModel, function (i) {
            return i.id;
        });

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
