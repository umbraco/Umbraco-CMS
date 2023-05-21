angular.module('umbraco').controller("Umbraco.PropertyEditors.UrlListController",
	function($rootScope, $scope, $filter) {

	    function formatDisplayValue() {            
	        if (Utilities.isArray($scope.model.value)) {
	            //it's the json value
	            $scope.renderModel = _.map($scope.model.value, function (item) {
	                return {
	                    url: item.url,
	                    linkText: item.linkText,
	                    urlTarget: (item.target) ? item.target : "_blank",
	                    icon: (item.icon) ? item.icon : "icon-out"
	                };
	            });
	        }
	        else {
                //it's the default csv value
	            $scope.renderModel = _.map($scope.model.value.split(","), function (item) {
	                return {
	                    url: item,
	                    linkText: "",
	                    urlTarget: ($scope.config && $scope.config.target) ? $scope.config.target : "_blank",
	                    icon: ($scope.config && $scope.config.icon) ? $scope.config.icon : "icon-out"
	                };
	            });
	        }
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
