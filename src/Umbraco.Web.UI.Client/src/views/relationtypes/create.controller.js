/**
 * @ngdoc controller
 * @name Umbraco.Editors.RelationTypes.CreateController
 * @function
 *
 * @description
 * The controller for creating relation types.
 */
function RelationTypeCreateController($scope, $location, relationTypeResource, navigationService, formHelper, appState, notificationsService) {
    var vm = this;
    vm.relationType = {};
    vm.objectTypes = {};

    vm.createRelationType = createRelationType;

    init();

    function init() {
        relationTypeResource.getRelationObjectTypes().then(function(data) {
                vm.objectTypes = data;
            },
            function(err) {
                notificationsService.error("Could not load form.");
            });
    }

    function createRelationType() {
        if (formHelper.submitForm({ scope: $scope, formCtrl: this.createRelationTypeForm, statusMessage: "Creating relation type..." })) {
            var node = $scope.currentNode;

            relationTypeResource.create(vm.relationType).then(function (data) {
                navigationService.hideMenu();

                // Set the new item as active in the tree
                var currentPath = node.path ? node.path : "-1";
                navigationService.syncTree({ tree: "relationTypes", path: currentPath + "," + data, forceReload: true, activate: true });

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
