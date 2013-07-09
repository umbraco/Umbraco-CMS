angular.module('umbraco.services')
.factory('historyService', function ($rootScope, $timeout, angularHelper) {

	var nArray = [];

	function add(item) {
		nArray.splice(0,0,item);
		return nArray[0];
	}

	return {
		add: function (item) {
			var icon = item.icon || "icon-file";
			angularHelper.safeApply($rootScope, function () {
				return add({name: item.name, icon: icon, link: item.link, time: new Date() });
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