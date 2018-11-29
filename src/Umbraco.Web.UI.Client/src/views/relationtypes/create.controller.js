function RelationTypeCreateController($scope, relationTypeResource, navigationService, formHelper, appState) {
    var vm = this;
    vm.relationType = {};
    vm.objectTypes = {};

    vm.createRelationType = createRelationType;

    init();

    function init() {
        relationTypeResource.getRelationObjectTypes().then(function (data) {
            vm.objectTypes = data;
        }, function (err) {
            alert("oh no");
        })
    }

    function createRelationType() {
        if (formHelper.submitForm({ scope: $scope, formCtrl: this.createRelationTypeForm, statusMessage: "Creating relation type..." })) {
            var node = $scope.dialogOptions.currentNode;

            relationTypeResource.create(vm.relationType).then(function (data) {
                navigationService.hideMenu();

                // Set the new item as active in the tree
                var currentPath = node.path ? node.path : "-1";
                navigationService.syncTree({ tree: "relationType", path: currentPath + "," + data, forceReload: true, activate: true });

                formHelper.resetForm({ scope: $scope });

                var currentSection = appState.getSectionState("currentSection");
                $location.path("/" + currentSection + "/relationTypes/edit/" + data);
            }, function (err) {
                if (err.data && err.data.message) {
                    notificationsService.error(err.data.message);
                    navigationService.hideMenu();
                }
            });
        }
    }
}

angular.module("umbraco").controller("Umbraco.Editors.RelationTypes.CreateController", RelationTypeCreateController);
