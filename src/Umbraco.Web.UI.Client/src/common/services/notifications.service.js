angular.module('umbraco.services')
.factory('notificationsService', function ($rootScope, $timeout, angularHelper) {

	var nArray = [];

	function add(item) {
		var index = nArray.length;
		nArray.push(item);


		$timeout(function () {
		    angularHelper.safeApply($rootScope, function () {
				nArray.splice(index, 1);
			});
			
		}, 5000);

		return nArray[index];
	}

	return {
	    success: function (headline, message) {
	        angularHelper.safeApply($rootScope, function () {
	            return add({ headline: headline, message: message, type: 'success', time: new Date() });
	        });
		},
	    error: function (headline, message) {
	        angularHelper.safeApply($rootScope, function() {
	            return add({ headline: headline, message: message, type: 'error', time: new Date() });
	        });			
		},
	    warning: function (headline, message) {
	        angularHelper.safeApply($rootScope, function() {
	            return add({ headline: headline, message: message, type: 'warning', time: new Date() });
	        });
		},
	    remove: function (index) {
	        angularHelper.safeApply($rootScope, function() {
	            nArray.splice(index, 1);
	        });
		},
	    removeAll: function () {
	        angularHelper.safeApply($rootScope, function() {
	            nArray = [];
	        });
		},

		current: nArray,

		getCurrent: function(){
			return nArray;
		}
	};
});