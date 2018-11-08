(function () {
    "use strict";

    function RollbackController($scope, contentResource, localizationService, assetsService) {
        
        var vm = this;

        vm.rollback = rollback;
        vm.changeLanguage = changeLanguage;
        vm.changeVersion = changeVersion;
        vm.submit = submit;
        vm.close = close;

        //////////

        function onInit() {

            vm.loading = true;
            vm.variantVersions = [];
            vm.diff = null;
            vm.currentVersion = null;
            vm.rollbackButtonDisabled = true;

            // find the current version for invariant nodes
            if($scope.model.node.variants.length === 1) {
                vm.currentVersion = $scope.model.node.variants[0];
            }

            // find the current version for nodes with variants
            if($scope.model.node.variants.length > 1) {
                var active = _.find($scope.model.node.variants, function (v) {
                    return v.active;
                });

                // preselect the language in the dropdown
                if(active) {
                    vm.selectedLanguage = active;
                    vm.currentVersion = active;
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

                getVersions().then(function(){
                    vm.loading = false;
                });

            });
            
        }

        function changeLanguage(language) {
            vm.currentVersion = language;
            getVersions();
        }

        function changeVersion(version) {

            if(version && version.versionId) {

                const culture = $scope.model.node.variants.length > 1 ? vm.currentVersion.language.culture : null;

                contentResource.getRollbackVersion(version.versionId, culture)
                    .then(function(data){
                        vm.previousVersion = data;
                        vm.previousVersion.versionId = version.versionId;
                        createDiff(vm.currentVersion, vm.previousVersion);
                        vm.rollbackButtonDisabled = false;
                    });

            } else {
                vm.diff = null;
                vm.rollbackButtonDisabled = true;
            }
        }

        function getVersions() {

            const nodeId = $scope.model.node.id;
            const culture = $scope.model.node.variants.length > 1 ? vm.currentVersion.language.culture : null;

            return contentResource.getRollbackVersions(nodeId, culture)
                .then(function(data){
                    vm.previousVersions = data.map(version => {
                        version.displayValue = version.versionDate + " - " + version.versionAuthorName;
                        return version;
                    }); 
                });
        }

        /**
         * This will load in a new version
         */
        function createDiff(currentVersion, previousVersion) {

            vm.diff = {};
            vm.diff.properties = [];

            // find diff in name
            vm.diff.name = JsDiff.diffWords(currentVersion.name, previousVersion.name);

            // extract all properties from the tabs and create new object for the diff
            currentVersion.tabs.forEach((tab, tabIndex) => {
                tab.properties.forEach((property, propertyIndex) => {
                    var oldProperty = previousVersion.tabs[tabIndex].properties[propertyIndex];

                    // we have to make properties storing values as object into strings (Grid, nested content, etc.)
                    if(property.value instanceof Object) {
                        property.value = JSON.stringify(property.value, null, 1);
                        property.isObject = true;
                    }

                    if(oldProperty.value instanceof Object) {
                        oldProperty.value = JSON.stringify(oldProperty.value, null, 1);
                        oldProperty.isObject = true;
                    }

                    // create new property object used in the diff table
                    var diffProperty = {
                        "alias": property.alias,
                        "label": property.label,
                        "diff": (property.value || oldProperty.value) ? JsDiff.diffWords(property.value, oldProperty.value) : "",
                        "isObject": (property.isObject || oldProperty.isObject) ? true : false
                    };

                    vm.diff.properties.push(diffProperty);

                });
            });

        }

        function rollback() {

            vm.rollbackButtonState = "busy";

            const nodeId = $scope.model.node.id;
            const versionId = vm.previousVersion.versionId;
            const culture = $scope.model.node.variants.length > 1 ? vm.currentVersion.language.culture : null;            

            return contentResource.rollback(nodeId, versionId, culture)
                .then(data => {
                    vm.rollbackButtonState = "success";
                    submit();
                }, error => {
                    vm.rollbackButtonState = "error";
                });

        }

        function submit() {
            if($scope.model.submit) {
                $scope.model.submit($scope.model.submit);
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