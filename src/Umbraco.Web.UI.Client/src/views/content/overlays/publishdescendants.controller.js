(function () {
    "use strict";

    function PublishDescendantsController($scope, localizationService, contentEditingHelper) {

        var vm = this;
        vm.includeUnpublished = $scope.model.includeUnpublished || false;

        vm.changeSelection = changeSelection;
        vm.toggleIncludeUnpublished = toggleIncludeUnpublished;

        function onInit() {

            vm.variants = $scope.model.variants;
            vm.displayVariants = vm.variants.slice(0); // shallow copy, we don't want to share the array-object (because we will be performing a sort method) but each entry should be shared (because we need validation and notifications).
            vm.labels = {};

            // get localized texts for use in directives
            if (!$scope.model.title) {
                localizationService.localize("buttons_publishDescendants").then(value => {
                    $scope.model.title = value;
                });
            }
            if (!vm.labels.includeUnpublished) {
                localizationService.localize("content_includeUnpublished").then(value => {
                    vm.labels.includeUnpublished = value;
                });
            }
            if (!vm.labels.includeUnpublished) {
                localizationService.localize("content_includeUnpublished").then(value => {
                    vm.labels.includeUnpublished = value;
                });
            }

            vm.variants.forEach(variant => {
                variant.isMandatory = isMandatoryFilter(variant);
            });

            if (vm.variants.length > 1) {

                vm.displayVariants = vm.displayVariants.filter(variant => allowPublish(variant));
                vm.displayVariants = contentEditingHelper.getSortedVariantsAndSegments(vm.displayVariants);

                var active = vm.variants.find(v => v.active);

                if (active) {
                    //ensure that the current one is selected
                    active.publish = active.save = true;
                }

                $scope.model.disableSubmitButton = !canPublish();
                
            } else {
                // localize help text for invariant content
                vm.labels.help = {
                    "key": "content_publishDescendantsHelp",
                    "tokens": [vm.variants[0].name]
                };
            }            
        }

        function allowPublish (variant) {
            return variant.allowedActions.includes("U");
        }

        function toggleIncludeUnpublished() {
            vm.includeUnpublished = !vm.includeUnpublished;
            // make sure this value is pushed back to the scope
            $scope.model.includeUnpublished = vm.includeUnpublished;
        }

        /** Returns true if publishing is possible based on if there are un-published mandatory languages */
        function canPublish() {
            var selected = [];
            vm.variants.forEach(variant => {

                var published = !(variant.state === "NotCreated" || variant.state === "Draft");

                if (variant.segment == null && variant.language && variant.language.isMandatory && !published && !variant.publish) {
                    //if a mandatory variant isn't published 
                    //and not flagged for saving
                    //then we cannot continue

                    // TODO: Show a message when this occurs
                    return false;
                }

                if (variant.publish) {
                    selected.push(variant.publish);
                }
            });

            return selected.length > 0;
        }

        function changeSelection(variant) {
            $scope.model.disableSubmitButton = !canPublish();
            //need to set the Save state to true if publish is true
            variant.save = variant.publish;
        }

        
        function isMandatoryFilter(variant) {
            //determine a variant is 'dirty' (meaning it will show up as publish-able) if it's
            // * has a mandatory language
            // * without having a segment, segments cant be mandatory at current state of code.
            return (variant.language && variant.language.isMandatory === true && variant.segment == null);
        }

        //when this dialog is closed, reset all 'publish' flags
        $scope.$on('$destroy', () => {
            vm.variants.forEach(variant => {
                variant.publish = variant.save = false;
            });
        });

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.PublishDescendantsController", PublishDescendantsController);

})();
