angular.module('umbraco').controller("Umbraco.Editors.UrlListController",
	function($rootScope, $scope, $filter) {

        function formatDisplayValue() {
            $scope.renderModel = _.map($scope.model.value.split(","), function (item) {
                return {
                    url: item,
                    urlTarget: ($scope.config && $scope.config.target) ? $scope.config.target : "_blank"
                };
            });
        }

	    formatDisplayValue();
	    
	    //we need to put a watch on the real model because when it changes we have to update our renderModel
	    $scope.$watch("model.value", function (newVal, oldVal) {
	        if (newVal !== null && newVal !== undefined && newVal !== oldVal) {
	            //update the display val again
	            formatDisplayValue();
	        }
	    });

	});