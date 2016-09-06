 (function() {
	"use strict";

	function CompositionsOverlay($scope) {

        var vm = this;

        vm.isSelected = isSelected;

        function isSelected(alias) {
            if($scope.model.contentType.compositeContentTypes.indexOf(alias) !== -1) {
                return true;
            }
        }
	}

	angular.module("umbraco").controller("Umbraco.Overlays.CompositionsOverlay", CompositionsOverlay);

})();
