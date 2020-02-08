function userPickerController($scope, entityResource, iconHelper, angularHelper, editorService){

    function trim(str, chr) {
        var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^' + chr + '+|' + chr + '+$', 'g');
        return str.replace(rgxtrim, '');
    }

    $scope.renderModel = [];
    $scope.allowRemove = true;

    var multiPicker = $scope.model.config.multiPicker && $scope.model.config.multiPicker !== '0' ? true : false;

    $scope.openUserPicker = function () {

        var currentSelection = [];
        //angular.copy(vm.userGroup.users, currentSelection);
        var userPicker = {
            multiPicker: multiPicker,
            selection: currentSelection,
            submit: function (model) {

                if (model.selection) {
                    _.each(model.selection, function (item, i) {
                        $scope.add(item);
                    });
                }
                //vm.userGroup.users = model.selection;
                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        };

        editorService.userPicker(userPicker);
    };

    $scope.remove =function(index){
        $scope.renderModel.splice(index, 1);
    };

    $scope.add = function (item) {
        var currIds = _.map($scope.renderModel, function (i) {
            if ($scope.model.config.idType === "udi") {
                return i.udi;
            }
            else {
                return i.id;
            }            
        });

        var itemId = $scope.model.config.idType === "udi" ? item.udi : item.id;

        console.log("itemId", itemId);
        console.log("currIds", currIds);

        if (currIds.indexOf(itemId) < 0) {
            item.icon = item.icon ? iconHelper.convertFromLegacyIcon(item.icon) : "icon-user";
            $scope.renderModel.push({ name: item.name, id: item.id, udi: item.udi, icon: item.icon, avatars: item.avatars });

            console.log("$scope.renderModel", $scope.renderModel);
        }
    };

    $scope.clear = function() {
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

    //load user data
    var modelIds = $scope.model.value ? $scope.model.value.split(',') : [];
    entityResource.getByIds(modelIds, "User").then(function (data) {
        _.each(data, function (item, i) {
            // set default icon if it's missing
            item.icon = (item.icon) ? iconHelper.convertFromLegacyIcon(item.icon) : "icon-user";
            $scope.renderModel.push({ name: item.name, id: item.id, udi: item.udi, icon: item.icon });
        });
    });
}


angular.module('umbraco').controller("Umbraco.PropertyEditors.UserPickerController", userPickerController);
