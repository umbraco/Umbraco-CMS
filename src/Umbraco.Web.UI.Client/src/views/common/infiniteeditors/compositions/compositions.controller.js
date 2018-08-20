 (function() {
	"use strict";

	function CompositionsOverlay($scope,$location) {

        var vm = this;
        var oldModel = null;

        vm.isSelected = isSelected;
        vm.openContentType = openContentType;
        vm.submit = submit;
        vm.close = close;

        function onInit() {
            console.log("on init");
            oldModel = angular.copy($scope.model);
        }

        function isSelected(alias) {
            if($scope.model.contentType.compositeContentTypes.indexOf(alias) !== -1) {
                return true;
            }
        }
        
        function openContentType(contentType, section) {
            var url = (section === "documentType" ? "/settings/documenttypes/edit/" : "/settings/mediaTypes/edit/") + contentType.id;
            $location.path(url);
        }

        function submit() {
            if ($scope.model && $scope.model.submit) {
                $scope.model.submit($scope.model, oldModel);
            }
        }

        function close() {
            if ($scope.model && $scope.model.close) {
                $scope.model.close(oldModel);
            }
        }

        onInit();
	}

	angular.module("umbraco").controller("Umbraco.Overlays.CompositionsOverlay", CompositionsOverlay);

})();
