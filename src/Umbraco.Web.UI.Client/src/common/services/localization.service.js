angular.module('umbraco.services')
.factory('localizationService', function ($http, $q, $rootScope, $window, $filter, userService) {
        var localize = {
            // use the $window service to get the language of the user's browser
            language: userService.getCurrentUser().locale,
            // array to hold the localized resource string entries
            dictionary:[],
            // location of the resource file
            url: "js/language.aspx",
            // flag to indicate if the service hs loaded the resource file
            resourceFileLoaded:false,

            // success handler for all server communication
            successCallback:function (data) {
                // store the returned array in the dictionary
                localize.dictionary = data;
                // set the flag that the resource are loaded
                localize.resourceFileLoaded = true;
                // broadcast that the file has been loaded
                $rootScope.$broadcast('localizeResourcesUpdates');
            },

            // allows setting of language on the fly
            setLanguage: function(value) {
                localize.language = value;
                localize.initLocalizedResources();
            },

            // allows setting of resource url on the fly
            setUrl: function(value) {
                localize.url = value;
                localize.initLocalizedResources();
            },

            // builds the url for locating the resource file
            buildUrl: function() {
                return '/i18n/resources-locale_' + localize.language + '.js';
            },

            // loads the language resource file from the server
            initLocalizedResources:function () {
                var deferred = $q.defer();
                // build the url to retrieve the localized resource file
                $http({ method:"GET", url:localize.url, cache:false })
                    .then(function(response){
                        localize.resourceFileLoaded = true;
                        localize.dictionary = response.data;

                        $rootScope.$broadcast('localizeResourcesUpdates');

                        return deferred.resolve(localize.dictionary);
                    }, function(err){
                        return deferred.reject("Something broke");
                    });
                return deferred.promise;
            },

            // checks the dictionary for a localized resource string
            getLocalizedString: function(value) {
                if(localize.resourceFileLoaded){
                    return _lookup(value);
                }else{
                    localize.initLocalizedResources().then(function(dic){
                        return _lookup(value);
                    });
                }
            },
            _lookup: function(value){
                var entry = localize.dictionary[value];
                if(entry){
                    return entry;
                }
                return "[" + value + "]";
            }


        };

        // force the load of the resource file
        localize.initLocalizedResources();

        // return the local instance when called
        return localize;
    });