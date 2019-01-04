function memberGroupController($scope, editorService, memberGroupResource) {
    //set the selected to the keys of the dictionary who's value is true
    $scope.getSelected = function () {
        var selected = [];
        for (var n in $scope.model.value) {
            if ($scope.model.value[n] === true) {
                selected.push(n);
            }
        }
        return selected;
    };

    $scope.pickGroup = function() {
        editorService.memberGroupPicker({
            multiPicker: true,
            submit: function (model) {
                var selectedGroupIds = _.map(model.selectedMemberGroups
                    ? model.selectedMemberGroups
                    : [model.selectedMemberGroup],
                    function(id) { return parseInt(id) }
                );
                memberGroupResource.getByIds(selectedGroupIds).then(function (selectedGroups) {
                    _.each(selectedGroups, function(group) {
                        $scope.model.value[group.name] = true;
                    });
                });
                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        });
    }

    $scope.removeGroup = function (group) {
        $scope.model.value[group] = false;
    }
}
angular.module('umbraco').controller("Umbraco.PropertyEditors.MemberGroupController", memberGroupController);
