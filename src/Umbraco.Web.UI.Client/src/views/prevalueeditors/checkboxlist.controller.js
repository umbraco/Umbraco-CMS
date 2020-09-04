angular.module("umbraco").controller("Umbraco.PrevalueEditors.CheckboxListController",
    function ($scope) {
        
        var vm = this;

        vm.change = change;

        function init() {

        }

        function change(model, value) {

            console.log("checkboxlist prevalues", model, value);

            var index = $scope.model.value.indexOf(value);

            if (model === true) {
                //if it doesn't exist in the model, then add it
                if (index < 0) {
                    $scope.model.value.push(value);
                }
            } else {
                //if it exists in the model, then remove it
                if (index >= 0) {
                    $scope.model.value.splice(index, 1);
                }
            }

        }

        init();

    });
