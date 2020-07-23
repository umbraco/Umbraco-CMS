function booleanEditorController($scope, angularHelper) {

    // Setup the default config
    // This allow to overwrite the configuration when property editor is re-used 
    // in e.g. third party packages, dashboard or content app. For example when using umb-property-editor.
    // At the moment this use "1/0" as default for "truevalue" and "falsevalue", but allow "True/False" as well.
    // Maybe sometime later we can make it support "Yes/No" or "On/Off" as well similar to ng-true-value and ng-false-value in Angular.
    var config = {
        truevalue: "1",
        falsevalue: "0"
    };

    // Map the user config
    Utilities.extend(config, $scope.model.config);

    // Map back to the model
    $scope.model.config = config;

    function setupViewModel() {

        $scope.renderModel = {
            value: false
        };

        if ($scope.model.config && $scope.model.config.default && Object.toBoolean($scope.model.config.default) && $scope.model && !$scope.model.value) {
            $scope.renderModel.value = true;
        }

        if ($scope.model && $scope.model.value && Object.toBoolean($scope.model.value)) {
            $scope.renderModel.value = true;
        }
    }

    setupViewModel();

    if ($scope.model && !$scope.model.value) {
        $scope.model.value = ($scope.renderModel.value === true) ? $scope.model.config.truevalue : $scope.model.config.falsevalue;
    }

    // Here we declare a special method which will be called whenever the value has changed from the server
    // this is instead of doing a watch on the model.value = faster
    $scope.model.onValueChanged = function (newVal, oldVal) {
        //update the display val again if it has changed from the server
        setupViewModel();
    };

    // Update the value when the toggle is clicked
    $scope.toggle = function(){
        angularHelper.getCurrentForm($scope).$setDirty();
        if ($scope.renderModel.value){
            $scope.model.value = $scope.model.config.falsevalue;
            setupViewModel();
            return;
        }

        $scope.model.value = $scope.model.config.truevalue;
        setupViewModel();
    };

}
angular.module("umbraco").controller("Umbraco.PropertyEditors.BooleanController", booleanEditorController);
