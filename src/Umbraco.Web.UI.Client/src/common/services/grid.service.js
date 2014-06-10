angular.module('umbraco.services')
	.factory('gridService', function ($http, $q){

		var configPath = "views/propertyeditors/grid/config/grid.editors.config.js";

		var service = {
			getGridEditors: function () {
				return $http.get(configPath);
			}
		};

		return service;

	});