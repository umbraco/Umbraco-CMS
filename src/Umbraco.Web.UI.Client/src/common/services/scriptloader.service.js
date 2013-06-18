//script loader wrapping around 3rd party loader
angular.module('umbraco.services')
.factory('scriptLoader', function ($q) {

	return {
		load: function (pathArray) {
			var deferred = $q.defer();

			yepnope({
				load: pathArray,
				complete: function () {
						deferred.resolve(true);
				}
			});
					
			return deferred.promise;
		}
	};
});