angular.module('umbraco.services')
	.factory('gridService', function ($http, $q){

	    var configPath = Umbraco.Sys.ServerVariables.umbracoUrls.gridConfig;
        var service = {
			getGridEditors: function () {
				return $http.get(configPath);
			}
		};

		return service;

	});
