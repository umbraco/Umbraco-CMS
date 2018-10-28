function RelationTypeEditController($scope, $routeParams, relationTypeResource, editorState, navigationService) {

    var vm = this;

    vm.page = {};
    vm.page.loading = false;
    vm.page.saveButtonState = "init";
    vm.page.menu = {}

    if($routeParams.create) {
        alert("create");
    } else {
        vm.page.loading = true;

        relationTypeResource.getById($routeParams.id)
            .then(function(data) {
                vm.relation = data;

                editorState.set(vm.relation);

                navigationService.syncTree({ tree: "relationTypes", path: data.path, forceReload: true }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node;
                });

                vm.page.loading = false;
            });
    }
}

angular.module("umbraco").controller("Umbraco.Editors.RelationTypes.EditController", RelationTypeEditController);
