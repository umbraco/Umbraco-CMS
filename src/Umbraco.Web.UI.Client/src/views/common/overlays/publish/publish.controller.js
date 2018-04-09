(function () {
    "use strict";

    function PublishController($scope, $timeout) {

        var vm = this;
        vm.variants = [];
        vm.loading = true;

        function onInit() {

            vm.loading = true;

            $timeout(function(){

                vm.variants = [
                    {
                        "cultureDisplayName": "English (United States)",
                        "culture": "en-US",
                        "state": "Published (pending changes)",
                        "selected": false,
                        "validationError": false,
                        "validationErrorMessage": ""
                    },
                    {
                        "cultureDisplayName": "Spanish (Spain)",
                        "culture": "es-ES",
                        "state": "Draft",
                        "selected": false,
                        "validationError": false,
                        "validationErrorMessage": ""
                    },
                    {
                        "cultureDisplayName": "French (France)",
                        "culture": "fr-FR",
                        "state": "Published (pending changes)",
                        "selected": false,
                        "validationError": true,
                        "validationErrorMessage": "Lorem ipsum dolor sit amet..."
                    },
                    {
                        "cultureDisplayName": "German (Germany)",
                        "culture": "de-DE",
                        "state": "Draft",
                        "selected": false,
                        "validationError": false,
                        "validationErrorMessage": ""
                    }
                ];

                vm.loading = false;

            }, 1000);

        }

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.PublishController", PublishController);

})();
