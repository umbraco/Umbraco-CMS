function userPickerController($scope, iconHelper, editorService, overlayService, entityResource) {

    var vm = this;

    function trim(str, chr) {
        var rgxtrim = (!chr) ? new RegExp('^\\s+|\\s+$', 'g') : new RegExp('^' + chr + '+|' + chr + '+$', 'g');
        return str.replace(rgxtrim, '');
    }

    $scope.renderModel = [];
    $scope.allowRemove = !$scope.readonly;
    $scope.allowAdd = !$scope.readonly;

    var multiPicker = $scope.model.config.multiPicker && $scope.model.config.multiPicker !== '0' ? true : false;

    function setDirty() {
        if ($scope.modelValueForm) {
            $scope.modelValueForm.modelValue.$setDirty();
        }
    }

    $scope.openUserPicker = function () {
        if (!$scope.allowAdd) return;

        var currentSelection = [];
        var userPicker = {
            multiPicker: multiPicker,
            selection: currentSelection,
            submit: function (model) {
                if (model.selection) {
                    _.each(model.selection, function (item, i) {
                        $scope.add(item);
                    });
                }
                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        };

        editorService.userPicker(userPicker);
    };

    $scope.remove = function (index) {
        if (!$scope.allowRemove) return;

        const dialog = {
            view: "views/propertyeditors/userpicker/overlays/remove.html",
            username: $scope.renderModel[index].name,
            submitButtonLabelKey: "defaultdialogs_yesRemove",
            submitButtonStyle: "danger",

            submit: function () {
                $scope.renderModel.splice(index, 1);
                $scope.userName = '';
                setDirty();
                overlayService.close();
            },
            close: function () {
                overlayService.close();
            }
        };

        overlayService.open(dialog);
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
            item.icon = item.icon ? iconHelper.convertFromLegacyIcon(item.icon) : "icon-user";
            $scope.renderModel.push({ name: item.name, id: item.id, udi: item.udi, icon: item.icon, avatars: item.avatars });
            setDirty();
        }
    };

    $scope.clear = function() {
        if (!$scope.allowRemove) return;
        
        $scope.renderModel = [];
        setDirty();
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

    //load user data - split to an array of ints (map)
    const modelIds = $scope.model.value ? $scope.model.value.split(',').map(x => +x) : [];
    if(modelIds.length !== 0) {
        entityResource.getAll("User").then(function (users) {
            const filteredUsers = users.filter(user => modelIds.indexOf(user.id) !== -1);
            filteredUsers.forEach(item => {
                    $scope.renderModel.push({
                        name: item.name,
                        id: item.id,
                        udi: item.udi,
                        icon: item.icon = item.icon ? iconHelper.convertFromLegacyIcon(item.icon) : "icon-user",
                        avatars: item.avatars
                    });
                });
            });
    }
}


angular.module('umbraco').controller("Umbraco.PropertyEditors.UserPickerController", userPickerController);
