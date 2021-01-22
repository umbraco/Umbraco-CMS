(function () {
    "use strict";

    function RollbackController($scope, contentResource, localizationService, assetsService, dateHelper, userService) {
        
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
            vm.labels = {};

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

            localizationService.localizeMany(["actions_rollback", "general_choose"]).then(function (data) {
                // set default title
                if (!$scope.model.title) {
                    $scope.model.title = data[0];
                }
                vm.labels.choose = data[1];
            });

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

                vm.loading = true;

                const culture = $scope.model.node.variants.length > 1 ? vm.currentVersion.language.culture : null;

                contentResource.getRollbackVersion(version.versionId, culture)
                    .then(function(data) {
                        vm.previousVersion = data;
                        vm.previousVersion.versionId = version.versionId;
                        createDiff(vm.currentVersion, vm.previousVersion);

                        vm.loading = false;
                        vm.rollbackButtonDisabled = false;
                    }, function () {
                        vm.loading = false;
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
                .then(function (data) {
                    // get current backoffice user and format dates
                    userService.getCurrentUser().then(function (currentUser) {
                        vm.previousVersions = data.map(version => {
                            var timestampFormatted = dateHelper.getLocalDate(version.versionDate, currentUser.locale, 'LLL');
                            version.displayValue = timestampFormatted + ' - ' + version.versionAuthorName;
                            return version;
                        }); 
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

                    // copy existing properties, so it doesn't manipulate existing properties on page
                    oldProperty = Utilities.copy(oldProperty);
                    property = Utilities.copy(property);

                    // we have to make properties storing values as object into strings (Grid, nested content, etc.)
                    if(property.value instanceof Object) {
                        property.value = JSON.stringify(property.value, null, 1);
                        property.isObject = true;
                    }

                    if(oldProperty.value instanceof Object) {
                        oldProperty.value = JSON.stringify(oldProperty.value, null, 1);
                        oldProperty.isObject = true;
                    }

                    // diff requires a string
                    property.value = property.value ? property.value + "" : "";
                    oldProperty.value = oldProperty.value ? oldProperty.value + "" : "";

                    var diffProperty = {
                        "alias": property.alias,
                        "label": property.label,
                        "diff": JsDiff.diffWords(property.value, oldProperty.value),
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
