(function () {
    "use strict";

    function PublishController($scope, localizationService, contentEditingHelper) {

        var vm = this;
        vm.loading = true;
        vm.isNew = true;

        vm.changeSelection = changeSelection;

        /** Returns true if publish meets the requirements of mandatory languages */
        function canPublish() {
            
            var hasSomethingToPublish = false;

            for (var i = 0; i < vm.variants.length; i++) {
                var variant = vm.variants[i];

                // if varaint is mandatory and not already published:
                if (variant.publish === false && notPublishedMandatoryFilter(variant)) {
                    return false;
                }
                if (variant.publish === true) {
                    hasSomethingToPublish = true;
                }

            }
            return hasSomethingToPublish;
        }

        function changeSelection(variant) {
            // update submit button state:
            $scope.model.disableSubmitButton = !canPublish();
            //need to set the Save state to same as publish.
            variant.save = variant.publish;
        }


        function hasAnyDataFilter(variant) {

            if (variant.name == null || variant.name.length === 0) {
                return false;
            }

            if(variant.isDirty === true) {
                return true;
            }

            for (var t=0; t < variant.tabs.length; t++){
                for (var p=0; p < variant.tabs[t].properties.length; p++){
                    var property = variant.tabs[t].properties[p];
                    if (property.value != null && property.value.length > 0) {
                        return true;
                    }
                }
            }

            return false;
        }

        function dirtyVariantFilter(variant) {
            //determine a variant is 'dirty' (meaning it will show up as publish-able) if it's
            // * it's editor is in a $dirty state
            // * it has pending saves
            // * it is unpublished
            return (variant.isDirty || variant.state === "Draft" || variant.state === "PublishedPendingChanges");
        }

        function publishableVariantFilter(variant) {
            //determine a variant is 'dirty' (meaning it will show up as publish-able) if it's
            // * variant is active
            // * it's editor is in a $dirty state
            // * it has pending saves
            // * it is unpublished
            return (variant.active || variant.isDirty || variant.state === "Draft" || variant.state === "PublishedPendingChanges");
        }

        function notPublishedMandatoryFilter(variant) {
            return variant.state !== "Published" && isMandatoryFilter(variant);
        }
        function isMandatoryFilter(variant) {
            //determine a variant is 'dirty' (meaning it will show up as publish-able) if it's
            // * has a mandatory language
            // * without having a segment, segments cant be mandatory at current state of code.
            return (variant.language && variant.language.isMandatory === true && variant.segment == null);
        }
        function notPublishableButMandatoryFilter(variant) {
            //determine a variant is needed, but not already a choice.
            // * publishable — aka. displayed as a publish option.
            // * published — its already published and everything is then fine.
            // * mandatory — this is needed, and thats why we highlight it.
            return !publishableVariantFilter(variant) && variant.state !== "Published" && variant.isMandatory === true;
        }

        function onInit() {

            vm.variants = $scope.model.variants;

            _.each(vm.variants, (variant) => {
                
                // reset to not be published
                variant.publish = false;
                variant.save = false;
                
                variant.isMandatory = isMandatoryFilter(variant);

                // If we have a variant thats not in the state of NotCreated, then we know we have adata and its not a new content node.
                if(variant.state !== "NotCreated") {
                    vm.isNew = false;
                }
            });

            _.each(vm.variants, (variant) => {
                
                // if this is a new node and we have data on this variant.
                if(vm.isNew === true && hasAnyDataFilter(variant)) {
                    variant.save = true;
                }
                
            });

            vm.availableVariants = vm.variants.filter(publishableVariantFilter);
            vm.missingMandatoryVariants = vm.variants.filter(notPublishableButMandatoryFilter);

            // if any active varaiant that is available for publish, we set it to be published:
            _.each(vm.availableVariants, (v) => {
                if(v.active) {
                    v.save = v.publish = true;
                }
            });

            if (vm.availableVariants.length !== 0) {
                vm.availableVariants.sort(function (a, b) {
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
            }


            $scope.model.disableSubmitButton = !canPublish();

            if (vm.missingMandatoryVariants.length > 0) {
                localizationService.localize("content_notReadyToPublish").then(function (value) {
                    $scope.model.title = value;
                    vm.loading = false;
                });
            } else {
                if (!$scope.model.title) {
                    localizationService.localize("content_readyToPublish").then(function (value) {
                        $scope.model.title = value;
                        vm.loading = false;
                    });
                } else {
                    vm.loading = false;
                }
            }

        }

        onInit();

        //when this dialog is closed, reset all 'publish' flags
        $scope.$on('$destroy', function () {
            for (var i = 0; i < vm.variants.length; i++) {
                vm.variants[i].publish = false;
                vm.variants[i].save = false;
            }
        });
    }

    angular.module("umbraco").controller("Umbraco.Overlays.PublishController", PublishController);

})();
