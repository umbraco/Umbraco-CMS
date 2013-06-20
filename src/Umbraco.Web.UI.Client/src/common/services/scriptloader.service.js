//script loader wrapping around 3rd party loader
angular.module('umbraco.services')
.factory('scriptLoader', function ($q) {

    return {
        load: function (pathArray, scope) {
            var deferred = $q.defer();

            yepnope({
                load: pathArray,
                complete: function () {

                    //if a scope is supplied then we need to make a digest here because
                    // deferred only executes in a digest. This might be required if we 
                    // are doing a load script after an http request or some other async call.
                    if (!scope) {
                        deferred.resolve(true);
                    }
                    else {
                        scope.$apply(function () {
                            deferred.resolve(true);
                        });
                    }
                }
            });

            return deferred.promise;
        }
    };
});