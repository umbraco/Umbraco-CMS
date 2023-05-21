angular.module('umbraco.mocks.services')
.factory('assetsService', function ($q) {

    return {
        loadCss : function(path, scope, attributes, timeout){
            var deferred = $q.defer();
            deferred.resolve();
            return deferred.promise;
        },
        loadJs : function(path, scope, attributes, timeout){
            var deferred = $q.defer();
            
            if(path[0] !== "/"){
                path = "/" + path;
            }   

            $.getScript( "base" + path, function( data, textStatus, jqxhr ) {
                deferred.resolve();
            });

            return deferred.promise;
        }
    };
});