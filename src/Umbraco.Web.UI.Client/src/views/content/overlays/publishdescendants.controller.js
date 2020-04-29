(function () {
    "use strict";

    function PublishDescendantsController($scope, localizationService) {

        var vm = this;

        vm.includeUnpublished = false;

        vm.changeSelection = changeSelection;
        vm.toggleIncludeUnpublished = toggleIncludeUnpublished;


        function onInit() {

            vm.variants = $scope.model.variants;
            vm.displayVariants = vm.variants.slice(0);// shallow copy, we dont want to share the array-object(because we will be performing a sort method) but each entry should be shared (because we need validation and notifications).
            vm.labels = {};

            if (!$scope.model.title) {
                localizationService.localize("buttons_publishDescendants").then(function (value) {
                    $scope.model.title = value;
                });
            }

            _.each(vm.variants, function (variant) {
                variant.isMandatory = isMandatoryFilter(variant);
            });

            if (vm.variants.length > 1) {

                vm.displayVariants.sort(function (a, b) {
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
                // localize help text for invariant content
                vm.labels.help = {
                    "key": "content_publishDescendantsHelp",
                    "tokens": [vm.variants[0].name]
                };
            }
            
        }

        function toggleIncludeUnpublished() {
            console.log("toggleIncludeUnpublished")
            vm.includeUnpublished = !vm.includeUnpublished;
        }

        /** Returns true if publishing is possible based on if there are un-published mandatory languages */
        function canPublish() {
            var selected = [];
            for (var i = 0; i < vm.variants.length; i++) {
                var variant = vm.variants[i];

                var published = !(variant.state === "NotCreated" || variant.state === "Draft");

                if (variant.segment == null &&  variant.language && variant.language.isMandatory && !published && !variant.publish) {
                    //if a mandatory variant isn't published 
                    //and not flagged for saving
                    //then we cannot continue

                    // TODO: Show a message when this occurs
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

        
        function isMandatoryFilter(variant) {
            //determine a variant is 'dirty' (meaning it will show up as publish-able) if it's
            // * has a mandatory language
            // * without having a segment, segments cant be mandatory at current state of code.
            return (variant.language && variant.language.isMandatory === true && variant.segment == null);
        }

        //when this dialog is closed, reset all 'publish' flags
        $scope.$on('$destroy', function () {
            for (var i = 0; i < vm.variants.length; i++) {
                vm.variants[i].publish = false;
                vm.variants[i].save = false;
            }
        });

        onInit();

    }

    angular.module("umbraco").controller("Umbraco.Overlays.PublishDescendantsController", PublishDescendantsController);

})();
