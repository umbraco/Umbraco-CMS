function booleanEditorController($scope) {

    // Setup the default config
    // This allow to overwrite the configuration when property editor is re-used
    // in e.g. third party packages, dashboard or content app. For example when using umb-property-editor.
    // At the moment this use "1/0" as default for "truevalue" and "falsevalue", but allow "True/False" as well.
    // Maybe sometime later we can make it support "Yes/No" or "On/Off" as well similar to ng-true-value and ng-false-value in Angular.
    var config = {
        truevalue: "1",
        falsevalue: "0",
        showLabels: false
    };

    if ($scope.model.config) {
        $scope.model.config.showLabels = $scope.model.config.showLabels ? Object.toBoolean($scope.model.config.showLabels) : config.showLabels;
    }

    // Map the user config
    Utilities.extend(config, $scope.model.config);

    // Map back to the model
    $scope.model.config = config;

    function setupViewModel() {

        $scope.renderModel = {
            value: false
        };
        
        // if no model, the subsequent conditions will both fail
        // so instead let's exit early
        if (!$scope.model) {
            return;
        }

        if ($scope.model.config && $scope.model.config.default && Object.toBoolean($scope.model.config.default) && !$scope.model.value) {
            $scope.renderModel.value = true;
            // no need to check the final condition as
            // the value is already set to true
            return;
        }

        if ($scope.model.value && Object.toBoolean($scope.model.value)) {
            $scope.renderModel.value = true;
        }
    }

    function setDirty() {
        if ($scope.modelValueForm) {
            $scope.modelValueForm.modelValue.$setDirty();
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

    // If another property editor changes the model.value we want to pick that up and reflect the value in this one.
    var unsubscribe = $scope.$watch("model.value", function(newVal, oldVal) {
        if(newVal !== oldVal) {
            setupViewModel();
        }
    });

    // Update the value when the toggle is clicked
    $scope.toggle = function(){
        setDirty();
        if ($scope.renderModel.value){
            $scope.model.value = $scope.model.config.falsevalue;
            setupViewModel();
            return;
        }

        $scope.model.value = $scope.model.config.truevalue;
        setupViewModel();
    };

    $scope.$on('$destroy', function () {
        unsubscribe();
    });

}
angular.module("umbraco").controller("Umbraco.PropertyEditors.BooleanController", booleanEditorController);
