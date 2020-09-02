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

    vm.multiPicker = $scope.model.multiPicker || false;
    vm.listType = $scope.model.listType || 'grid';

    vm.submit = submit;
    vm.close = close;
    vm.isSelected = isSelected;

    function onInit() {
        if (!$scope.model.title) {
            localizationService.localize("defaultdialogs_selectItem").then(function(value){
                $scope.model.title = value;
            });
        }

        if (vm.multiPicker) {
            vm.selectedItems = [];

            //if (Utilities.isArray($scope.model.selectedItems)) {
            //    vm.selectedItems = $scope.model.selectedItems;
            //}
            //TODO: Push selected items from model
        }


    }

    function isSelected(item) {
        return vm.selectedItems.indexOf(item) > -1;
    }

    function selectItem(item) {

        if (vm.multiPicker) {
            
            if (vm.selectedItems.indexOf(item) > -1) {
                vm.selectedItems.splice(vm.selectedItems.indexOf(item), 1);
            } else {
                vm.selectedItems.push(item);
            }

        } else {
            $scope.model.selectedItem = item;
            submit($scope.model);
        }

    };

    function submit(model) {
        if($scope.model.submit) {
            $scope.model.selectedItems = vm.selectedItems;
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
