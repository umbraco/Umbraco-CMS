function RelationTypeEditController($scope, $routeParams, relationTypeResource, editorState, navigationService, dateHelper, userService, entityResource) {

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
                vm.relationType = data;

                editorState.set(vm.relationType);

                navigationService.syncTree({ tree: "relationTypes", path: data.path, forceReload: true }).then(function (syncArgs) {
                    vm.page.menu.currentNode = syncArgs.node;
                });

                formatDates(vm.relationType.relations);
                getRelationNames(vm.relationType);

                vm.page.loading = false;
            });
    }

    function formatDates(relations) {
        if(relations) {
            userService.getCurrentUser().then(function (currentUser) {
                angular.forEach(relations, function (relation) {
                    relation.timestampFormatted = dateHelper.getLocalDate(relation.createDate, currentUser.locale, 'LLL');
                });
            });
        }
    }

    function getRelationNames(relationType) {
        if(relationType.relations) {
            angular.forEach(relationType.relations, function(relation){
                entityResource.getById(relation.parentId, relationType.parentObjectTypeName).then(function(entity) {
                    relation.parentName = entity.name;
                });
                entityResource.getById(relation.childId, relationType.childObjectTypeName).then(function(entity) {
                    relation.childName = entity.name;
                });
            });
        }
    }
}

angular.module("umbraco").controller("Umbraco.Editors.RelationTypes.EditController", RelationTypeEditController);
