(function () {
    "use strict";

    function SendToPublishController($scope, localizationService, contentEditingHelper) {

        var vm = this;
        vm.loading = true;

        vm.changeSelection = changeSelection;

        function onInit() {

            vm.variants = $scope.model.variants;

            // set dialog title
            if (!$scope.model.title) {
                localizationService.localize("content_sendForApproval").then(function (value) {
                    $scope.model.title = value;
                });
            }

            _.each(vm.variants, function (variant) {
                variant.isMandatory = isMandatoryFilter(variant);
            });
            
            vm.availableVariants = vm.variants.filter(publishableVariantFilter);
            
            if (vm.availableVariants.length !== 0) {

                vm.availableVariants = vm.availableVariants.sort(function (a, b) {
                    if (a.language && b.language) {
                        if (a.language.name > b.language.name) {
                            return -1;
                        }
                        if (a.language.name < b.language.name) {
                            return 1;
                        }
                    }
                    if (a.segment && b.segment) {
                        if (a.segment > b.segment) {
                            return -1;
                        }
                        if (a.segment < b.segment) {
                            return 1;
                        }
                    }
                    return 0;
                });

                _.each(vm.availableVariants, function (v) {
                    if(v.active) {
                        v.save = true;
                    }
                });

            } else {
                //disable save button if we have nothing to save
                $scope.model.disableSubmitButton = true;
            }

            vm.loading = false;
            
        }

        function changeSelection() {
            var firstSelected = _.find(vm.variants, function (v) {
                return v.save;
            });
            $scope.model.disableSubmitButton = !firstSelected; //disable submit button if there is none selected
        }

        function isMandatoryFilter(variant) {
            //determine a variant is 'dirty' (meaning it will show up as publish-able) if it's
            // * has a mandatory language
            // * without having a segment, segments cant be mandatory at current state of code.
            return (variant.language && variant.language.isMandatory === true && variant.segment == null);
        }

        function publishableVariantFilter(variant) {
            //determine a variant is 'dirty' (meaning it will show up as publish-able) if it's
            // * variant is active
            // * it's editor is in a $dirty state
            // * it has pending saves
            // * it is unpublished
            return (variant.active || variant.isDirty || variant.state === "Draft" || variant.state === "PublishedPendingChanges");
        }

        //when this dialog is closed, reset all 'save' flags
        $scope.$on('$destroy', function () {
            for (var i = 0; i < vm.variants.length; i++) {
                vm.variants[i].save = false;
            }
        });

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.SendToPublishController", SendToPublishController);

})();
