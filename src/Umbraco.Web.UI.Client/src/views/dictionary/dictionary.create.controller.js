/**
 * @ngdoc controller
 * @name Umbraco.Editors.Dictionary.CreateController
 * @function
 * 
 * @description
 * The controller for creating dictionary items
 */
function DictionaryCreateController($scope, $location, dictionaryResource, navigationService, notificationsService, formHelper, appState) {

    var vm = this;

    vm.itemKey = "";
    vm.createItem = createItem;
    vm.close = close;
    $scope.$emit("$changeTitle", "");

    function close() {
        navigationService.hideDialog();
    }

    function createItem() {

        if (formHelper.submitForm({ scope: $scope, formCtrl: $scope.createDictionaryForm })) {

            var node = $scope.currentNode;

            dictionaryResource.create(node.id, vm.itemKey).then(function (data) {
                navigationService.hideMenu();

                // set new item as active in tree
                var currPath = node.path ? node.path : "-1";
                navigationService.syncTree({ tree: "dictionary", path: currPath + "," + data, forceReload: true, activate: true });

                // reset form state
                formHelper.resetForm({ scope: $scope, formCtrl: $scope.createDictionaryForm });

                // navigate to edit view
                var currentSection = appState.getSectionState("currentSection");
                $location.path("/" + currentSection + "/dictionary/edit/" + data);

            }, function (err) {
                formHelper.resetForm({ scope: $scope, formCtrl: $scope.createDictionaryForm, hasErrors: true });
                if (err.data && err.data.message) {
                    notificationsService.error(err.data.message);
                    navigationService.hideMenu();
                }
            });
        }
    }
}

angular.module("umbraco").controller("Umbraco.Editors.Dictionary.CreateController", DictionaryCreateController);
