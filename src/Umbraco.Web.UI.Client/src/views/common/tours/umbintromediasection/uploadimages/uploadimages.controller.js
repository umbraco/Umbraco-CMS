(function () {
    "use strict";

    function UploadImagesController($scope, editorState, mediaResource) {

        var vm = this;

        vm.error = false;

        vm.initNextStep = initNextStep;

        function initNextStep() {

            vm.error = false;
            vm.buttonState = "busy";

            var currentNode = editorState.getCurrent();

            // make sure we have uploaded at least one image
            mediaResource.getChildren(currentNode.id)
                .then(function (data) {

                    var children = data;

                    if (children.items && children.items.length > 0) {
                        $scope.model.nextStep();
                    } else {
                        vm.error = true;
                    }

                    vm.buttonState = "init";

                });

        }

    }

    angular.module("umbraco").controller("Umbraco.Tours.UmbIntroMediaSection.UploadImagesController", UploadImagesController);
})();
