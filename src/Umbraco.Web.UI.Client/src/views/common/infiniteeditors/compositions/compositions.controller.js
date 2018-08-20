 (function() {
	"use strict";

	function CompositionsOverlay($scope,$location) {

        var vm = this;

        vm.isSelected = isSelected;
        vm.openContentType = openContentType;

        function isSelected(alias) {
            if($scope.model.contentType.compositeContentTypes.indexOf(alias) !== -1) {
                return true;
            }
        }
        function openContentType(contentType, section) {
            
            var url = (section === "documentType" ? "/settings/documenttypes/edit/" : "/settings/mediaTypes/edit/") + contentType.id;
            $location.path(url);
        }
	}

	angular.module("umbraco").controller("Umbraco.Overlays.CompositionsOverlay", CompositionsOverlay);

})();
