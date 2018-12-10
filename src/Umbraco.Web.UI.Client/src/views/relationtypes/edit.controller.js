/**
 * @ngdoc controller
 * @name Umbraco.Editors.RelationTypes.EditController
 * @function
 *
 * @description
 * The controller for editing relation types.
 */
function RelationTypeEditController($scope, $routeParams, relationTypeResource, editorState, navigationService, dateHelper, userService, entityResource, formHelper, contentEditingHelper, localizationService) {

    var vm = this;

    vm.page = {};
    vm.page.loading = false;
    vm.page.saveButtonState = "init";
    vm.page.menu = {}

    vm.save = saveRelationType;

    init();

    function init() {
        vm.page.loading = true;

        localizationService.localizeMany(["relationType_tabRelationType", "relationType_tabRelations"]).then(function (data) {
            vm.page.navigation = [
                {
                    "name": data[0],
                    "alias": "relationType",
                    "icon": "icon-info",
                    "view": "views/relationTypes/views/relationType.html",
                    "active": true
                },
                {
                    "name": data[1],
                    "alias": "relations",
                    "icon": "icon-trafic",
                    "view": "views/relationTypes/views/relations.html"
                }
            ];
        });

        relationTypeResource.getById($routeParams.id)
            .then(function(data) {
                bindRelationType(data);
                vm.page.loading = false;
            });
    }

    function bindRelationType(relationType) {
        formatDates(relationType.relations);
        getRelationNames(relationType);

        vm.relationType = relationType;

        editorState.set(vm.relationType);

        navigationService.syncTree({ tree: "relationTypes", path: relationType.path, forceReload: true }).then(function (syncArgs) {
            vm.page.menu.currentNode = syncArgs.node;
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

    function saveRelationType() {
        vm.page.saveButtonState = "busy";

        if (formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {
            relationTypeResource.save(vm.relationType).then(function (data) {
                formHelper.resetForm({ scope: $scope, notifications: data.notifications });
                bindRelationType(data);
                vm.page.saveButtonState = "success";
            }, function (error) {
                contentEditingHelper.handleSaveError({
                    redirectOnFailure: false,
                    err: error
                });

                notificationsService.error(error.data.message);
                vm.page.saveButtonState = "error";
            });
        }
    }
}

angular.module("umbraco").controller("Umbraco.Editors.RelationTypes.EditController", RelationTypeEditController);
