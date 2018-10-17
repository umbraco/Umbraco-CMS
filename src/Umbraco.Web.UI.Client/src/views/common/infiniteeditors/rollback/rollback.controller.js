(function () {
    "use strict";

    function RollbackController($scope, contentResource, localizationService, assetsService) {
        
        var vm = this;

        vm.rollback = rollback;
        vm.loadVersion = loadVersion;
        vm.submit = submit;
        vm.close = close;

        //////////

        function onInit() {

            vm.loading = true;
            vm.variantVersions = [];
            vm.diff = null;

            // preselect the active language
            if($scope.model.node.variants.length > 1) {
                var active = _.find($scope.model.node.variants, function (v) {
                    return v.active;
                });

                if(active) {
                    vm.selectedVariant = active;
                }
            }

            // set default title
            if(!$scope.model.title) {
                localizationService.localize("actions_rollback").then(function(value){
                    $scope.model.title = value;
                });
            }

            // Load in diff library
            assetsService.loadJs('lib/jsdiff/diff.min.js', $scope).then(function () {

                vm.currentVersion = {
                    "id": 1,
                    "createDate": "22/08/2018 13.32",
                    "name": "Forside",
                    "properties": [
                        {
                            "alias": "headline",
                            "label": "Headline",
                            "value": "Velkommen"
                        },
                        {
                            "alias": "text",
                            "label": "Text",
                            "value": "This is my danish Content"
                        }
                    ]
                };

                const nodeId = $scope.model.node.id;
                const culture = vm.selectedVariant ? vm.selectedVariant.language.culture : null;

                contentResource.getRollbackVersions(nodeId, culture)
                    .then(function(data){
                        console.log(data);
                        vm.previousVersions = data;
                    });

                vm.loading = false;

            });
            
        }

        function rollback() {
            console.log("rollback");
        }

        /**
         * This will load in a new version
         */
        function loadVersion(id) {

            // fake load version

            vm.diff = {};
            vm.diff.properties = [];

            var oldVersion = {
                "id": 2,
                "name": "Foride",
                "properties": [
                    {
                        "alias": "headline",
                        "label": "Headline",
                        "value": ""
                    },
                    {
                        "alias": "text",
                        "label": "Text",
                        "value": "This is my danish Content Test"
                    }
                ]
            };

            // find diff in name
            vm.diff.name = JsDiff.diffWords(vm.currentVersion.name, oldVersion.name);

            // find diff in properties
            angular.forEach(vm.currentVersion.properties, 
                function(newProperty, index){
                    var oldProperty = oldVersion.properties[index];
                    var diffProperty = {
                        "alias": newProperty.alias,
                        "label": newProperty.label,
                        "diff": JsDiff.diffWords(newProperty.value, oldProperty.value)
                    };
                    vm.diff.properties.push(diffProperty);
                });

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

    angular.module("umbraco").controller("Umbraco.Editors.RollbackController", RollbackController);

})();