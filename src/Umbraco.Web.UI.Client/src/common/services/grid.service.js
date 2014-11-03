angular.module('umbraco.services')
	.factory('gridService', function ($http, $q){

	    var configPath = "../config/grid.editors.config.js";
	    var configSettingsPath = "../config/grid.settings.config.js";

		var service = {
			getGridEditors: function () {
				return $http.get(configPath);
			},
			getGridSettings: function () {
			    return $http.get(configSettingsPath);
			}
		};

		return service;

	});
