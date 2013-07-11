/**
 * @ngdoc service
 * @name umbraco.services.scriptLoader
 *
 * @requires $q 
 * @requires angularHelper
 *  
 * @description
 * Promise-based utillity service to lazy-load client-side dependencies inside angular controllers.
 * 
 * ##usage
 * To use, simply inject the scriptLoader into any controller that needs it, and make
 * sure the umbraco.services module is accesible - which it should be by default.
 *
 * <pre>
 *      angular.module("umbraco").controller("my.controller". function(scriptLoader){
 *          scriptLoader.load(["script.js", "styles.css"], $scope).then(function(){
 *                 //this code executes when the dependencies are done loading
 *          });
 *      });
 *     
 * </pre> 
 */
angular.module('umbraco.services')
.factory('scriptLoader', function ($q, angularHelper) {

    return {
        load: function (pathArray, scope) {
            var deferred = $q.defer();

            var nonEmpty = _.reject(pathArray, function(item) {
                return item === undefined || item === "";
            });

            //don't load anything if there's nothing to load
            if (nonEmpty.length > 0) {
                yepnope({
                    load: pathArray,
                    complete: function() {

                        //if a scope is supplied then we need to make a digest here because
                        // deferred only executes in a digest. This might be required if we 
                        // are doing a load script after an http request or some other async call.
                        if (!scope) {
                            deferred.resolve(true);
                        }
                        else {
                            angularHelper.safeApply(scope, function () {
                                deferred.resolve(true);
                            });
                        }
                    }
                });
            }
            else {
                if (!scope) {
                    deferred.resolve(true);
                }
                else {
                    angularHelper.safeApply(scope, function () {
                        deferred.resolve(true);
                    });
                }
            }
            return deferred.promise;
        }
    };
});