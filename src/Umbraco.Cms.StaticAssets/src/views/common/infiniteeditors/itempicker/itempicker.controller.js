/**
 * @ngdoc controller
 * @name Umbraco.Editors.ItemPickerController
 * @function
 *
 * @description
 * The controller for a reusable editor to pick items
 */

function ItemPickerController($scope, localizationService) {

    var vm = this;

    vm.selectItem = selectItem;
    vm.submit = submit;
    vm.close = close;

    function onInit() {
        if (!$scope.model.title) {
            localizationService.localize("defaultdialogs_selectItem").then(function(value){
                $scope.model.title = value;
            });
        }
    }

    function selectItem(item) {
        $scope.model.selectedItem = item;
        submit($scope.model);
    };

    function submit(model) {
        if($scope.model.submit) {
            $scope.model.submit(model);
        }
    }

    function close() {
        if($scope.model.close) {
            $scope.model.close();
        }
    }

    onInit();

}

angular.module("umbraco").controller("Umbraco.Editors.ItemPicker", ItemPickerController);
