(function () {
    "use strict";

    function InsertFieldController($scope, contentTypeResource, localizationService) {

        var vm = this;

        vm.field;
        vm.defaultValue;
        vm.recursive = false;
        vm.showDefaultValue = false;

        vm.generateOutputSample = generateOutputSample;
        vm.submit = submit;
        vm.close = close;

        function onInit() {

            // set default title
            if(!$scope.model.title) {
                localizationService.localize("template_insertPageField").then(function(value){
                    $scope.model.title = value;
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

            var fallback;

            if(vm.recursive !== false && vm.defaultValue !== undefined){
                fallback = "Fallback.To(Fallback.Ancestors, Fallback.DefaultValue)";
            }else if(vm.recursive !== false){
                fallback = "Fallback.ToAncestors";
            }else if(vm.defaultValue !== undefined){
                fallback = "Fallback.ToDefaultValue";
            }

            var pageField = (vm.field !== undefined ? '@Model.Value("' + vm.field + '"' : "")
                + (fallback  !== undefined? ', fallback: ' + fallback : "")
                + (vm.defaultValue !== undefined ? ', defaultValue: new HtmlString("' + vm.defaultValue + '")' : "")

                + (vm.field ? ')' : "");

            $scope.model.umbracoField = pageField;

            return pageField;

        }

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

    angular.module("umbraco").controller("Umbraco.Editors.InsertFieldController", InsertFieldController);
})();
