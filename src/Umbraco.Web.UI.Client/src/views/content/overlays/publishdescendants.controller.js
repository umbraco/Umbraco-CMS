(function () {
    "use strict";

    function PublishDescendantsController($scope, localizationService) {

        var vm = this;

        vm.changeSelection = changeSelection;

        function onInit() {

            vm.includeUnpublished = false;
            vm.variants = $scope.model.variants;
            vm.labels = {};

            if (!$scope.model.title) {
                localizationService.localize("buttons_publishDescendants").then(function (value) {
                    $scope.model.title = value;
                });
            }

            _.each(vm.variants,
                function (variant) {
                    variant.compositeId = (variant.language ? variant.language.culture : "inv") + "_" + (variant.segment ? variant.segment : "");
                    variant.htmlId = "_content_variant_" + variant.compositeId;
                });

            if (vm.variants.length > 1) {

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
                // localize help text for invariant content
                vm.labels.help = {
                    "key": "content_publishDescendantsHelp",
                    "tokens": []
                };
                // add the node name as a token so it will show up in the translated text
                vm.labels.help.tokens.push(vm.variants[0].name);
            }
            
        }

        /** Returns true if publishing is possible based on if there are un-published mandatory languages */
        function canPublish() {
            var selected = [];
            for (var i = 0; i < vm.variants.length; i++) {
                var variant = vm.variants[i];

                var published = !(variant.state === "NotCreated" || variant.state === "Draft");

                if (variant.language.isMandatory && !published && !variant.publish) {
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
