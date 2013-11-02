angular.module('umbraco').controller("Umbraco.PropertyEditors.UrlListController",
	function($rootScope, $scope, $filter) {

        function formatDisplayValue() {
            $scope.renderModel = _.map($scope.model.value.split(","), function (item) {
                return {
                    url: item,
                    urlTarget: ($scope.config && $scope.config.target) ? $scope.config.target : "_blank"
                };
            });
        }

	    $scope.getUrl = function(valueUrl) {
	        if (valueUrl.indexOf("/") >= 0) {
	            return valueUrl;
	        }
	        return "#";
	    };

	    formatDisplayValue();
	    
	    //here we declare a special method which will be called whenever the value has changed from the server
	    //this is instead of doing a watch on the model.value = faster
	    $scope.model.onValueChanged = function(newVal, oldVal) {
	        //update the display val again
	        formatDisplayValue();
	    };

	});