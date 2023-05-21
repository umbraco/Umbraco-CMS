(function () {
    "use strict";

    function InsertFieldController($scope, contentTypeResource, localizationService) {

        var vm = this;

        vm.field = null;
        vm.defaultValue = null;
        vm.recursive = false;
        vm.showDefaultValue = false;

        vm.generateOutputSample = generateOutputSample;
        vm.submit = submit;
        vm.close = close;

        function onInit() {

            var labelKeys = [
                "template_insertPageField"
            ];

            // set default title
            if(!$scope.model.title) {
                localizationService.localizeMany(labelKeys).then(function (data) {
                    $scope.model.title = data[0];
                });
            }

            // Load all fields
            contentTypeResource.getAllPropertyTypeAliases().then(function (array) {
                vm.properties = array;
            });

            // Load all standard fields
            contentTypeResource.getAllStandardFields().then(function (array) {
                vm.standardFields = array;
            });

        }

        function generateOutputSample() {

            var fallback = null;

            if (vm.recursive !== false && vm.defaultValue !== null) {
                fallback = "Fallback.To(Fallback.Ancestors, Fallback.DefaultValue)";
            } else if (vm.recursive !== false) {
                fallback = "Fallback.ToAncestors";
            } else if (vm.defaultValue !== null) {
                fallback = "Fallback.ToDefaultValue";
            }

            var pageField = (vm.field !== null ? '@Model.Value("' + vm.field + '"' : "")
                + (fallback  !== null? ', fallback: ' + fallback : "")
                + (vm.defaultValue !== null ? ', defaultValue: new HtmlString("' + vm.defaultValue + '")' : "")

                + (vm.field ? ')' : "");

            $scope.model.umbracoField = pageField;

            return pageField;
        }

        function submit(model) {
            if ($scope.model.submit) {
                $scope.model.submit(model);
            }
        }

        function close() {
            if ($scope.model.close) {
                $scope.model.close();
            }
        }

        onInit();
    }

    angular.module("umbraco").controller("Umbraco.Editors.InsertFieldController", InsertFieldController);
})();
