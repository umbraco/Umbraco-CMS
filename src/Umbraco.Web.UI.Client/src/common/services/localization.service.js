angular.module('umbraco.services')
.factory('localizationService', function ($http, $q, eventsService, $window, $filter, userService) {

    //TODO: This should be injected as server vars
    var url = "LocalizedText";
    var resourceFileLoadStatus = "none";
    var resourceLoadingPromise = [];

    function _lookup(value, tokens, dictionary) {

        //strip the key identifier if its there
        if (value && value[0] === "@") {
            value = value.substring(1);
        }

        //if no area specified, add general_
        if (value && value.indexOf("_") < 0) {
            value = "general_" + value;
        }

        var entry = dictionary[value];
        if (entry) {
            if (tokens) {
                for (var i = 0; i < tokens.length; i++) {
                    entry = entry.replace("%" + i + "%", tokens[i]);
                }
            }
            return entry;
        }
        return "[" + value + "]";
    }

    var service = {
        // array to hold the localized resource string entries
        dictionary: [],

        // loads the language resource file from the server
        initLocalizedResources: function () {
            var deferred = $q.defer();

            if (resourceFileLoadStatus === "loaded") {
                deferred.resolve(service.dictionary);
                return deferred.promise;
            }

            //if the resource is already loading, we don't want to force it to load another one in tandem, we'd rather
            // wait for that initial http promise to finish and then return this one with the dictionary loaded
            if (resourceFileLoadStatus === "loading") {
                //add to the list of promises waiting
                resourceLoadingPromise.push(deferred);

                //exit now it's already loading
                return deferred.promise;
            }

            resourceFileLoadStatus = "loading";

            // build the url to retrieve the localized resource file
            $http({ method: "GET", url: url, cache: false })
                .then(function (response) {
                    resourceFileLoadStatus = "loaded";
                    service.dictionary = response.data;

                    eventsService.emit("localizationService.updated", response.data);

                    deferred.resolve(response.data);
                    //ensure all other queued promises are resolved
                    for (var p in resourceLoadingPromise) {
                        resourceLoadingPromise[p].resolve(response.data);
                    }
                }, function (err) {
                    deferred.reject("Something broke");
                    //ensure all other queued promises are resolved
                    for (var p in resourceLoadingPromise) {
                        resourceLoadingPromise[p].reject("Something broke");
                    }
                });
            return deferred.promise;
        },

        //helper to tokenize and compile a localization string
        tokenize: function (value, scope) {
            if (value) {
                var localizer = value.split(':');
                var retval = { tokens: undefined, key: localizer[0].substring(0) };
                if (localizer.length > 1) {
                    retval.tokens = localizer[1].split(',');
                    for (var x = 0; x < retval.tokens.length; x++) {
                        retval.tokens[x] = scope.$eval(retval.tokens[x]);
                    }
                }

                return retval;
            }
            return value;
        },

        // checks the dictionary for a localized resource string
        localize: function (value, tokens) {
            return service.initLocalizedResources().then(function (dic) {
                var val = _lookup(value, tokens, dic);
                return val;
            });
        },

    };

    //This happens after login / auth and assets loading
    eventsService.on("app.authenticated", function () {
        resourceFileLoadStatus = "none";
        resourceLoadingPromise = [];
    });

    // return the local instance when called
    return service;
});
