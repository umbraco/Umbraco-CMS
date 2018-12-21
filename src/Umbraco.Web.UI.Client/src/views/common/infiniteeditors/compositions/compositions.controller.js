(function () {
    "use strict";

    function CompositionsController($scope, $location, $filter) {

        var vm = this;
        var oldModel = null;

        vm.showConfirmSubmit = false;

        vm.isSelected = isSelected;
        vm.openContentType = openContentType;
        vm.submit = submit;
        vm.close = close;

        function onInit() {

            /* make a copy of the init model so it is possible to roll 
            back the changes on cancel */
            oldModel = angular.copy($scope.model);

            if (!$scope.model.title) {
                $scope.model.title = "Compositions";
            }

            // group the content types by their container paths
            vm.availableGroups = $filter("orderBy")(
                _.map(
                    _.groupBy($scope.model.availableCompositeContentTypes, function (compositeContentType) {
                        return compositeContentType.contentType.metaData.containerPath;
                    }), function (group) {
                        return {
                            containerPath: group[0].contentType.metaData.containerPath,
                            compositeContentTypes: group
                        };
                    }
                ), function (group) {
                    return group.containerPath.replace(/\//g, " ");
                });
        }

        

        function isSelected(alias) {
            if ($scope.model.contentType.compositeContentTypes.indexOf(alias) !== -1) {
                return true;
            }
        }

        function openContentType(contentType, section) {
            var url = (section === "documentType" ? "/settings/documenttypes/edit/" : "/settings/mediaTypes/edit/") + contentType.id;
            $location.path(url);
        }

        function submit() {
            if ($scope.model && $scope.model.submit) {

                // check if any compositions has been removed
                vm.compositionRemoved = false;
                for (var i = 0; oldModel.compositeContentTypes.length > i; i++) {
                    var oldComposition = oldModel.compositeContentTypes[i];
                    if (_.contains($scope.model.compositeContentTypes, oldComposition) === false) {
                        vm.compositionRemoved = true;
                    }
                }

                /* submit the form if there havne't been removed any composition
                or the confirm checkbox has been checked */
                if (!vm.compositionRemoved || vm.allowSubmit) {
                    $scope.model.submit($scope.model);
                }
            }
        }

        function close() {
            if ($scope.model && $scope.model.close) {
                $scope.model.close(oldModel);
            }
        }

        onInit();
    }

    angular.module("umbraco").controller("Umbraco.Editors.CompositionsController", CompositionsController);

})();
