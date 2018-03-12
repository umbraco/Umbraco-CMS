(function () {
    "use strict";

    function InsertFieldController($scope, $http, contentTypeResource) {
        
        var vm = this;

        vm.field;
        vm.altField;
        vm.altText;
        vm.insertBefore;
        vm.insertAfter;
        vm.recursive = false;
        vm.properties = [];
        vm.standardFields = [];
        vm.date = false;
        vm.dateTime = false;
        vm.dateTimeSeparator = "";
        vm.casingUpper = false;
        vm.casingLower = false;
        vm.encodeHtml = false;
        vm.encodeUrl = false;
        vm.convertLinebreaks = false;

        vm.showAltField = false;
        vm.showAltText = false;

        vm.setDateOption = setDateOption;
        vm.setCasingOption = setCasingOption;
        vm.setEncodingOption = setEncodingOption;
        vm.generateOutputSample = generateOutputSample;

        function onInit() {

            // set default title
            if(!$scope.model.title) {
                $scope.model.title = "Insert value";
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

        // date formatting
        function setDateOption(option) {
            
            if (option === 'date') {
                if(vm.date) {
                    vm.date = false;
                } else {
                    vm.date = true;
                    vm.dateTime = false;
                }
            }

            if (option === 'dateWithTime') {
                if(vm.dateTime) {
                    vm.dateTime = false;
                } else {
                    vm.date = false;
                    vm.dateTime = true;
                }
            }

        }

        // casing formatting
        function setCasingOption(option) {
            if (option === 'uppercase') {
                if(vm.casingUpper) {
                    vm.casingUpper = false;
                } else {
                    vm.casingUpper = true;
                    vm.casingLower = false;
                }
            }

            if (option === 'lowercase') {
                if(vm.casingLower) {
                    vm.casingLower = false;
                } else {
                    vm.casingUpper = false;
                    vm.casingLower = true;
                }
            }
        }

        // encoding formatting
        function setEncodingOption(option) {
            if (option === 'html') {
                if(vm.encodeHtml) {
                    vm.encodeHtml = false;
                } else {
                    vm.encodeHtml = true;
                    vm.encodeUrl = false;
                }
            }
            
            if (option === 'url') {
                if (vm.encodeUrl) {
                    vm.encodeUrl = false;
                } else {
                    vm.encodeHtml = false;
                    vm.encodeUrl = true;
                }
            }
        }

        function generateOutputSample() {

            var pageField = (vm.field !== undefined ? '@Umbraco.Field("' + vm.field + '"' : "")
                + (vm.altField !== undefined ? ', altFieldAlias:"' + vm.altField + '"' : "")
                + (vm.altText !== undefined ? ', altText:"' + vm.altText + '"' : "")
                + (vm.insertBefore !== undefined ? ', insertBefore:"' + vm.insertBefore + '"' : "")
                + (vm.insertAfter !== undefined ? ', insertAfter:"' + vm.insertAfter + '"' : "")
                + (vm.recursive !== false ? ', recursive: ' + vm.recursive : "")
                + (vm.date !== false ? ', formatAsDate: ' + vm.date : "")
                + (vm.dateTime !== false ? ', formatAsDateWithTimeSeparator:"' + vm.dateTimeSeparator + '"' : "")
                + (vm.casingUpper !== false ? ', casing: ' + "RenderFieldCaseType.Upper" : "")
                + (vm.casingLower !== false ? ', casing: ' + "RenderFieldCaseType.Lower" : "")
                + (vm.encodeHtml !== false ? ', encoding: ' + "RenderFieldEncodingType.Html" : "")
                + (vm.encodeUrl !== false ? ', encoding: ' + "RenderFieldEncodingType.Url" : "")
                + (vm.convertLinebreaks !== false ? ', convertLineBreaks: ' + "true" : "")
                + (vm.field ? ')' : "");

            $scope.model.umbracoField = pageField;
            
            return pageField;

        }

        onInit();


    }

    angular.module("umbraco").controller("Umbraco.Overlays.InsertFieldController", InsertFieldController);
})();
