/**
 * @ngdoc controller
 * @name Umbraco.Editors.Macros.CreateController
 * @function
 * 
 * @description
 * The controller for creating macro items
 */
function MacrosCreateController($scope, $location, macroResource, navigationService, notificationsService, formHelper, appState) {
    var vm = this;

    vm.itemKey = "";

    function createItem() {

        if (formHelper.submitForm({ scope: $scope, formCtrl: this.createMacroForm })) {

            var node = $scope.currentNode;

            macroResource.createMacro(vm.itemKey).then(function (data) {
                navigationService.hideMenu();

                // set new item as active in tree
                var currPath = node.path ? node.path : "-1";
                navigationService.syncTree({ tree: "macros", path: currPath + "," + data, forceReload: true, activate: true });

                // reset form state
                formHelper.resetForm({ scope: $scope });

                // navigate to edit view
                var currentSection = appState.getSectionState("currentSection");
                $location.path("/" + currentSection + "/macros/edit/" + data);


            }, function (err) {
                if (err.data && err.data.message) {
                    notificationsService.error(err.data.message);
                    navigationService.hideMenu();
                }
            });
        }
    }

    vm.createItem = createItem;
}

angular.module("umbraco").controller("Umbraco.Editors.Macros.CreateController", MacrosCreateController);
