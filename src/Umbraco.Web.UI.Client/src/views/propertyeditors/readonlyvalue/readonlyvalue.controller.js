/**
 * @ngdoc controller
 * @name Umbraco.Editors.ReadOnlyValueController
 * @function
 * 
 * @description
 * The controller for the readonlyvalue property editor. 
 *  This controller offer more functionality than just a simple label as it will be able to apply formatting to the 
 *  value to be displayed. This means that we also have to apply more complex logic of watching the model value when 
 *  it changes because we are creating a new scope value called displayvalue which will never change based on the server data.
 *  In some cases after a form submission, the server will modify the data that has been persisted, especially in the cases of 
 *  readonlyvalues so we need to ensure that after the form is submitted that the new data is reflected here.
*/
function ReadOnlyValueController($rootScope, $scope, $filter) {

    function formatDisplayValue() {
        if ($scope.model.config && $scope.model.config.filter) {
            if ($scope.model.config.format) {
                $scope.displayvalue = $filter($scope.model.config.filter)($scope.model.value, $scope.model.config.format);
            } else {
                $scope.displayvalue = $filter($scope.model.config.filter)($scope.model.value);
            }
        } else {
            $scope.displayvalue = $scope.model.value;
        }
    }

    //format the display value on init:
    formatDisplayValue();

    $scope.$watch("model.value", function (newVal, oldVal) {
        //cannot just check for !newVal because it might be an empty string which we 
        //want to look for.
        if (newVal !== null && newVal !== undefined && newVal !== oldVal) {
            //update the display val again
            formatDisplayValue();
        }
    });
}

angular.module('umbraco').controller("Umbraco.PropertyEditors.ReadOnlyValueController", ReadOnlyValueController);
