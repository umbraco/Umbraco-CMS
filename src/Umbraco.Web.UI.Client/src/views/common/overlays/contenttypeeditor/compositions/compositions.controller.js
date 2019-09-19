 (function() {
	"use strict";

	function CompositionsOverlay($scope,$location,$filter) {

        var vm = this;

        vm.isSelected = isSelected;
        vm.openContentType = openContentType;

        // group the content types by their container paths
        vm.availableGroups = $filter("orderBy")(
            _.map(
                _.groupBy($scope.model.availableCompositeContentTypes, function (compositeContentType) {
                    return compositeContentType.contentType.metaData.containerPath;
                }), function(group) {
                    return {
                        containerPath: group[0].contentType.metaData.containerPath,
                        compositeContentTypes: group
                    };
                }
            ), function (group) {
                return group.containerPath.replace(/\//g, " ");
            });

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
