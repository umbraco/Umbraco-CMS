(function () {
    "use strict";

    function PublishController($scope, localizationService) {

        var vm = this;
        vm.loading = true;
        vm.hasPristineVariants = false;
        vm.isNew = true;

        vm.changeSelection = changeSelection;
        vm.dirtyVariantFilter = dirtyVariantFilter;
        vm.pristineVariantFilter = pristineVariantFilter;

        /** Returns true if publishing is possible based on if there are un-published mandatory languages */
        function canPublish() {
            var selected = [];
            for (var i = 0; i < vm.variants.length; i++) {
                var variant = vm.variants[i];

                //if this variant will show up in the publish-able list
                var publishable = dirtyVariantFilter(variant);
                var published = !(variant.state === "NotCreated" || variant.state === "Draft");

                if ((variant.language.isMandatory && !published) && (!publishable || !variant.publish)) {
                    //if a mandatory variant isn't published
                    //and it's not publishable or not selected to be published
                    //then we cannot continue

                    //TODO: Show a message when this occurs
                    return false;
                }

                if (variant.publish) {
                    selected.push(variant.publish);
                }
            }
            return selected.length > 0;
        }

        function changeSelection(variant) {
            $scope.model.disableSubmitButton = !canPublish();
            //need to set the Save state to true if publish is true
            variant.save = variant.publish;
        }

        function dirtyVariantFilter(variant) {
            //determine a variant is 'dirty' (meaning it will show up as publish-able) if it's
            // * the active one
            // * it's editor is in a $dirty state
            // * it has pending saves
            // * it is unpublished
            // * it is in NotCreated state
            return (variant.active || variant.isDirty || variant.state === "Draft" || variant.state === "PublishedPendingChanges" || variant.state === "NotCreated");
        }

        function hasAnyData(variant) {
            var result = variant.isDirty != null || (variant.name != null && variant.name.length > 0);

            if(result) return true;

            for (var t=0; t < variant.tabs.length; t++){
                for (var p=0; p < variant.tabs[t].properties.length; p++){

                    var property = variant.tabs[t].properties[p];

                    if(property.culture == null) continue;

                    result = result ||  (property.value != null && property.value.length > 0);

                    if(result) return true;
                }
            }

            return result;
        }

        function pristineVariantFilter(variant) {
            return !(dirtyVariantFilter(variant));
        }

        function onInit() {



            vm.variants = $scope.model.variants;

            if (!$scope.model.title) {
                localizationService.localize("content_readyToPublish").then(function (value) {
                    $scope.model.title = value;
                });
            }

            vm.hasPristineVariants = false;

            _.each(vm.variants,
                function (variant) {
                    if(variant.state !== "NotCreated"){
                        vm.isNew = false;
                    }
                });

            _.each(vm.variants,
                function (variant) {
                    variant.compositeId = variant.language.culture + "_" + (variant.segment ? variant.segment : "");
                    variant.htmlId = "_content_variant_" + variant.compositeId;

                    //check for pristine variants
                    if (!vm.hasPristineVariants) {
                        vm.hasPristineVariants = pristineVariantFilter(variant);
                    }
                    
                    if(hasAnyData(variant)){
                        if(vm.isNew || variant.publishDate == null){
                            variant.publish = true;
                            variant.save = true;
                        }
                    }else{
                        variant.publish = false;
                        variant.save = false;
                        variant.canSave = false;
                    }
                });

            if (vm.variants.length !== 0) {
                //now sort it so that the current one is at the top
                vm.variants = _.sortBy(vm.variants, function (v) {
                    return v.active ? 0 : 1;
                });

                var active = _.find(vm.variants, function (v) {
                    return v.active;
                });

                if (active) {
                    //ensure that the current one is selected
                    active.publish = true;
                    active.save = true;
                }

                $scope.model.disableSubmitButton = !canPublish();

            } else {
                //disable Publish button if we have nothing to publish, should not happen
                $scope.model.disableSubmitButton = true;
            }

            var labelKey =  vm.isNew ? "content_languagesToPublishForFirstTime" : "content_languagesToPublish";

            localizationService.localize(labelKey).then(function (value) {
                vm.headline = value;

                vm.loading = false;
            });



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
