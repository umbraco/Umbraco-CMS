(function () {
    "use strict";

    function TemplateSectionsController($scope, formHelper, localizationService) {

        var vm = this;

        vm.labels = {};

        vm.select = select;
        vm.submit = submit;
        vm.close = close;

        $scope.model.mandatoryRenderSection = false;

        if (!$scope.model.title) {
            $scope.model.title = "Sections";
        }

        function onInit() {
            if ($scope.model.hasMaster) {
                $scope.model.insertType = 'addSection';
            } else {
                $scope.model.insertType = 'renderBody';
            }

            var labelKeys = [
                "template_sectionMandatory"
            ];

            localizationService.localizeMany(labelKeys).then(function (data) {
                vm.labels.sectionMandotory = data[0];
            });
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
            if ($scope.model.close) {
                $scope.model.close();
            }
        }

        onInit();
    }

    angular.module("umbraco").controller("Umbraco.Editors.TemplateSectionsController", TemplateSectionsController);
})();
