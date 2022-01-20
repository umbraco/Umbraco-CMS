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
        $scope.$emit("$changeTitle", "");
        relationTypeResource.getRelationObjectTypes().then(function(data) {
                vm.objectTypes = data;
            },
            function(err) {
                notificationsService.error("Could not load form.");
            });
    }

    function createRelationType() {
        if (formHelper.submitForm({ scope: $scope, formCtrl: $scope.createRelationTypeForm, statusMessage: "Creating relation type..." })) {
            var node = $scope.currentNode;

            relationTypeResource.create(vm.relationType).then(function (data) {
                navigationService.hideMenu();

                // Set the new item as active in the tree
                var currentPath = node.path ? node.path : "-1";
                navigationService.syncTree({ tree: "relationTypes", path: currentPath + "," + data, forceReload: true, activate: true });

                formHelper.resetForm({ scope: $scope, formCtrl: $scope.createRelationTypeForm });

                var currentSection = appState.getSectionState("currentSection");
                $location.path("/" + currentSection + "/relationTypes/edit/" + data);
            }, function (err) {
                formHelper.resetForm({ scope: $scope, formCtrl: $scope.createRelationTypeForm, hasErrors: true });
                if (err.data && err.data.message) {
                    notificationsService.error(err.data.message);
                    navigationService.hideMenu();
                }
            });
        }
    }

    $scope.close = function () {
        navigationService.hideDialog(true);
    };
}

angular.module("umbraco").controller("Umbraco.Editors.RelationTypes.CreateController", RelationTypeCreateController);
