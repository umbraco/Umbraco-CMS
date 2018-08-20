(function () {
    "use strict";

    function TemplateSectionsController($scope, formHelper) {

        var vm = this;

        vm.select = select;
        vm.submit = submit;
        vm.close = close;

        $scope.model.mandatoryRenderSection = false;

        if(!$scope.model.title) {
            $scope.model.title = "Sections";
        }

        function onInit() {
            if($scope.model.hasMaster) {
                $scope.model.insertType = 'addSection';
            } else {
                $scope.model.insertType = 'renderBody';
            }
        }

        function select(type) {
            $scope.model.insertType = type;
        }

        function submit(model) {
            if (formHelper.submitForm({scope: $scope})) {
                $scope.model.submit($scope.model);
            }
        }

        function close() {
            if($scope.model.close) {
                $scope.model.close();
            }
        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Editors.TemplateSectionsController", TemplateSectionsController);
})();
