angular.module('umbraco.mocks.resources')
.factory('tagsResource', function () {
	return {
		getTags: function (group) {
			var g = [
				{"id":1, "label":"Jordbærkage"},
				{"id":2, "label":"Banankage"},
				{"id":3, "label":"Kiwikage"},
				{"id":4, "label":"Rabarbertærte"}
			];
			return g;
		}
	};
});