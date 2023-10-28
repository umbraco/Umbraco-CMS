(function () {
    "use strict";

    function PublishController($scope, localizationService, contentEditingHelper) {

        var vm = this;
        vm.loading = true;
        vm.isNew = true;

        vm.changeSelection = changeSelection;

        function allowPublish (variant) {
            return variant.allowedActions.includes("U");
        }

        /** 
         * Returns true if publish meets the requirements of mandatory languages 
         * */
        function canPublish() {

            var hasSomethingToPublish = false;

            vm.variants.forEach(variant => {
                // if varaint is mandatory and not already published:
                if (variant.publish === false && notPublishedMandatoryFilter(variant)) {
                    return false;
                }
                if (variant.publish === true) {
                    hasSomethingToPublish = true;
                }
            });

            return hasSomethingToPublish;
        }

        function changeSelection(variant) {
            // update submit button state:
            $scope.model.disableSubmitButton = !canPublish();
            //need to set the Save state to same as publish.
            variant.save = variant.publish;
        }

        function hasAnyDataFilter(variant) {

            // if we have a name, then we have data.
            if (variant.name != null && variant.name.length > 0) {
                return true;
            }

            if(variant.isDirty === true) {
                return true;
            }

            variant.tabs.forEach(tab => {
                tab.properties.forEach(property => {
                    if (property.value != null && property.value.length > 0) {
                        return true;
                    }
                });
            });

            return false;
        }

        /**
         * determine a variant is 'dirty' (meaning it will show up as publish-able) if it's
         *  * it's editor is in a $dirty state
         *  * it has pending saves
         *  * it is unpublished
         * @param {*} variant 
         */
        function dirtyVariantFilter(variant) {
            return (variant.isDirty || variant.state === "Draft" || variant.state === "PublishedPendingChanges");
        }

        /**
         * determine a variant is 'dirty' (meaning it will show up as publish-able) if it's
         *  * variant is active
         *  * it's editor is in a $dirty state
         *  * it has pending saves
         *  * it is unpublished
         * @param {*} variant 
         */
        function publishableVariantFilter(variant) {
            variant.notAllowed = allowPublish(variant) === false && variant.active;
            return (variant.active || variant.isDirty || variant.state === "Draft" || variant.state === "PublishedPendingChanges") && (allowPublish(variant) || variant.active);
        }

        function notPublishedMandatoryFilter(variant) {
            return variant.state !== "Published" && variant.state !== "PublishedPendingChanges" && variant.isMandatory === true;
        }

        /**
         * determine a variant is 'dirty' (meaning it will show up as publish-able) if it's
         *  * has a mandatory language
         *  * without having a segment, segments cant be mandatory at current state of code.
         * @param {*} variant 
         */
        function isMandatoryFilter(variant) {            
            return (variant.language && variant.language.isMandatory === true && variant.segment == null);
        }

        /**
         * determine a variant is needed, but not already a choice.
         *  * publishable — aka. displayed as a publish option.
         *  * published — its already published and everything is then fine.
         *  * mandatory — this is needed, and thats why we highlight it.
         * @param {*} variant 
         */
        function notPublishableButMandatoryFilter(variant) {

            return !publishableVariantFilter(variant) && variant.state !== "Published" && variant.isMandatory === true;
        }

        function onInit() {

            vm.variants = $scope.model.variants;

            // If we have a variant that's not in the state of NotCreated, 
            // then we know we have data and it's not a new content node.
            vm.isNew = vm.variants.some(variant => variant.state === 'NotCreated');

            vm.variants.forEach(variant => {

                // reset to not be published
                variant.publish = variant.save = false;

                
                variant.isMandatory = isMandatoryFilter(variant);

                
                // if this is a new node and we have data on this variant.
                if (vm.isNew === true && hasAnyDataFilter(variant)) {
                    variant.save = true;
                }
                
            });

            vm.availableVariants = vm.variants.filter(publishableVariantFilter);
            vm.missingMandatoryVariants = vm.variants.filter(notPublishableButMandatoryFilter);

            // if any active varaiant that is available for publish, we set it to be published:
            vm.availableVariants.forEach(v => {
                if(v.active && allowPublish(v)) {
                    v.save = v.publish = true;
                }
            });

            if (vm.availableVariants.length !== 0) {
                vm.availableVariants = contentEditingHelper.getSortedVariantsAndSegments(vm.availableVariants);
            }

            $scope.model.disableSubmitButton = !canPublish();

            const localizeKey = vm.missingMandatoryVariants.length > 0 ? 'content_notReadyToPublish' : 
                !$scope.model.title ? 'content_readyToPublish' : '';

            if (localizeKey) {
                localizationService.localize(localizeKey).then(value => {
                    $scope.model.title = value;
                    vm.loading = false;
                });
            } else {
                vm.loading = false;
            }
        }

        onInit();

        //when this dialog is closed, reset all 'publish' flags
        $scope.$on('$destroy', () => {
            vm.variants.forEach(variant => {
                variant.publish = variant.save = false;
                variant.notAllowed = false;
            });
        });
    }

    angular.module("umbraco").controller("Umbraco.Overlays.PublishController", PublishController);

})();
