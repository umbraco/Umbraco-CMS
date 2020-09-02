(function () {
    'use strict';

    function InlineBlockEditor($scope) {

        const bc = this;

        bc.openBlock = function(block) {

            // if we are closing:
            if (block.active === true) {
                // boardcast the formSubmitting event to trigger syncronization or none-live property-editors
                $scope.$broadcast("formSubmitting", { scope: $scope });
                // Some property editors need to performe an action after all property editors have reacted to the formSubmitting.
                $scope.$broadcast("formSubmittingFinalPhase", { scope: $scope });

                block.active = false;
            } else {
                $scope.api.activateBlock(block);
            }
        }

    }

    angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.InlineBlockEditor", InlineBlockEditor);

})();
