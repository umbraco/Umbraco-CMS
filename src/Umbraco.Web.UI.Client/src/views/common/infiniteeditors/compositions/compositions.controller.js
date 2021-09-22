(function () {
    "use strict";

    function CompositionsController($scope, $location, $filter, $timeout, overlayService, localizationService) {

        var vm = this;
        var oldModel = null;

        vm.showConfirmSubmit = false;
        vm.loadingAlias = null;

        vm.isSelected = isSelected;
        vm.openContentType = openContentType;
        vm.selectCompositeContentType = selectCompositeContentType;
        vm.submit = submit;
        vm.close = close;

        function onInit() {

            /* make a copy of the init model so it is possible to roll
            back the changes on cancel */
            oldModel = Utilities.copy($scope.model);

            if (!$scope.model.title) {
                $scope.model.title = "Compositions";
            }

            // Group the content types by their container paths
            vm.availableGroups = $filter("orderBy")(
                _.map(
                    _.groupBy($scope.model.availableCompositeContentTypes, function (compositeContentType) {

                        compositeContentType.selected = isSelected(compositeContentType.contentType.alias);

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
            return false;
        }

        function openContentType(contentType, section) {
            var url = (section === "documentType" ? "/settings/documentTypes/edit/" : "/settings/mediaTypes/edit/") + contentType.id;
            $location.path(url);
        }

        function selectCompositeContentType(compositeContentType) {
            vm.loadingAlias = compositeContentType.contentType.alias

            var contentType = compositeContentType.contentType;

            $scope.model.selectCompositeContentType(contentType).then(function (response) {
                vm.loadingAlias = null;
            });

            // Check if the template is already selected.
            var index = $scope.model.contentType.compositeContentTypes.indexOf(contentType.alias);

            if (index === -1) {
                $scope.model.contentType.compositeContentTypes.push(contentType.alias);
            } else {
                $scope.model.contentType.compositeContentTypes.splice(index, 1);
            }
        }

        function submit() {
            if ($scope.model && $scope.model.submit) {

                // check if any compositions has been removed
                var compositionRemoved = false;
                for (var i = 0; oldModel.compositeContentTypes.length > i; i++) {
                    var oldComposition = oldModel.compositeContentTypes[i];
                    if (_.contains($scope.model.compositeContentTypes, oldComposition) === false) {
                        compositionRemoved = true;
                    }
                }

                /* submit the form if there havne't been removed any composition
                or the confirm checkbox has been checked */
                if (compositionRemoved) {
                    vm.allowSubmit = false;
                    localizationService.localize("general_remove").then(function (value) {
                        const dialog = {
                            view: "views/common/infiniteeditors/compositions/overlays/confirmremove.html",
                            title: value,
                            submitButtonLabelKey: "general_ok",
                            submitButtonStyle: "danger",
                            closeButtonLabelKey: "general_cancel",
                            submit: function (model) {
                                $scope.model.submit($scope.model);
                                overlayService.close();
                            },
                            close: function () {
                                overlayService.close();
                            }
                        };
                        overlayService.open(dialog);
                    });
                    return;
                }

                $scope.model.submit($scope.model);
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
