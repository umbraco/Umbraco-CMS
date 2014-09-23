angular.module('umbraco.services')
	.factory('canvasService', function ($http, $q){

		var configPath = "../config/canvas.editors.config.js";
        var service = {
			getGridEditors: function () {
				return $http.get(configPath);
			}
		};

		return service;

	});
