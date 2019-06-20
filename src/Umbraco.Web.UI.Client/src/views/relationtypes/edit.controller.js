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
        if (relationType.relations) {
            // can we grab app entity types in one go?
            if (relationType.parentObjectType === relationType.childObjectType) {
                // yep, grab the distinct list of parent and child entities
                var entityIds = _.uniq(_.union(_.pluck(relationType.relations, "parentId"), _.pluck(relationType.relations, "childId")));
                entityResource.getByIds(entityIds, relationType.parentObjectTypeName).then(function (entities) {
                    updateRelationNames(relationType, entities);
                });
            } else {
                // nope, grab the parent and child entities individually
                var parentEntityIds = _.uniq(_.pluck(relationType.relations, "parentId"));
                var childEntityIds = _.uniq(_.pluck(relationType.relations, "childId"));
                entityResource.getByIds(parentEntityIds, relationType.parentObjectTypeName).then(function (entities) {
                    updateRelationNames(relationType, entities);
                });
                entityResource.getByIds(childEntityIds, relationType.childObjectTypeName).then(function (entities) {
                    updateRelationNames(relationType, entities);
                });
            }
        }
    }

    function updateRelationNames(relationType, entities) {
        var entitiesById = _.indexBy(entities, "id");
        _.each(relationType.relations, function(relation) {
            if (entitiesById[relation.parentId]) {
                relation.parentName = entitiesById[relation.parentId].name;
            }
            if (entitiesById[relation.childId]) {
                relation.childName = entitiesById[relation.childId].name;
            }
        });
    }

    function saveRelationType() {

        if (formHelper.submitForm({ scope: $scope, statusMessage: "Saving..." })) {

            vm.page.saveButtonState = "busy";

            relationTypeResource.save(vm.relationType).then(function (data) {
                formHelper.resetForm({ scope: $scope, notifications: data.notifications });
                bindRelationType(data);

                vm.page.saveButtonState = "success";

            }, function (error) {
                contentEditingHelper.handleSaveError({
                    err: error
                });

                notificationsService.error(error.data.message);

                vm.page.saveButtonState = "error";
            });
        }
    }
}

angular.module("umbraco").controller("Umbraco.Editors.RelationTypes.EditController", RelationTypeEditController);
