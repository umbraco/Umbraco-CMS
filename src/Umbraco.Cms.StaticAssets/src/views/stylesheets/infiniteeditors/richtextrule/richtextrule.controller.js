(function() {
	"use strict";

	function RichTextRuleController($scope, formHelper) {

        const vm = this;

        vm.submit = submit;
        vm.close = close;

        function submit() {
            if ($scope.model && $scope.model.submit && formHelper.submitForm({scope: $scope})) {
                $scope.model.submit($scope.model);
            }
        }

        function close() {
            if ($scope.model && $scope.model.close) {
                $scope.model.close();
            }
        }

	}

	angular.module("umbraco").controller("Umbraco.Editors.RichTextRuleController", RichTextRuleController);

})();
