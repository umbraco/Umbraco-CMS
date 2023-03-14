/**
 * @ngdoc service
 * @name umbraco.services.localizationService
 *
 * @requires $http
 * @requires $q
 * @requires $window
 * @requires $filter
 *
 * @description
 * Application-wide service for handling localization
 *
 * ##usage
 * To use, simply inject the localizationService into any controller that needs it, and make
 * sure the umbraco.services module is accesible - which it should be by default.
 *
 * <pre>
 *    localizationService.localize("area_key").then(function(value){
 *        element.html(value);
 *    });
 * </pre>
 */

angular.module('umbraco.services')
.factory('localizationService', function ($http, $q, eventsService) {

    // TODO: This should be injected as server vars
    var url = "LocalizedText";
    var resourceFileLoadStatus = "none";
    var resourceLoadingPromise = [];

    // array to hold the localized resource string entries
    var innerDictionary = [];

    function _lookup(alias, tokens, dictionary, fallbackValue) {

        //strip the key identifier if its there
        if (alias && alias[0] === "@") {
            alias = alias.substring(1);
        }

        var underscoreIndex = alias.indexOf("_");
        //if no area specified, add general_
        if (alias && underscoreIndex < 0) {
            alias = "general_" + alias;
            underscoreIndex = alias.indexOf("_");
        }

        var areaAlias = alias.substring(0, underscoreIndex);
        var valueAlias = alias.substring(underscoreIndex + 1);

        var areaEntry = dictionary[areaAlias];
        if (areaEntry) {
            var valueEntry = areaEntry[valueAlias];
            if (valueEntry) {
                return service.tokenReplace(valueEntry, tokens);
            }
        }

        if (fallbackValue) return fallbackValue;

        return "[" + alias + "]";
    }

    var service = {


        // loads the language resource file from the server
        initLocalizedResources: function () {

            // TODO: This promise handling is super ugly, we should just be returnning the promise from $http and returning inner values.

            var deferred = $q.defer();

            if (resourceFileLoadStatus === "loaded") {
                deferred.resolve(innerDictionary);
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
                    innerDictionary = response.data;

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

        /**
         * @ngdoc method
         * @name umbraco.services.localizationService#tokenize
         * @methodOf umbraco.services.localizationService
         *
         * @description
         * Helper to tokenize and compile a localization string
         * @param {String} value the value to tokenize
         * @param {Object} scope the $scope object
         * @returns {String} tokenized resource string
         */
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


        /**
         * @ngdoc method
         * @name umbraco.services.localizationService#tokenReplace
         * @methodOf umbraco.services.localizationService
         *
         * @description
         * Helper to replace tokens
         * @param {String} value the text-string to manipulate
         * @param {Array} tekens An array of tokens values
         * @returns {String} Replaced test-string
         */
        tokenReplace: function (text, tokens) {
            if (tokens) {
                for (var i = 0; i < tokens.length; i++) {
                    text = text.replace("%" + i + "%", _.escape(tokens[i]));
                }
            }
            return text;
        },


        /**
         * @ngdoc method
         * @name umbraco.services.localizationService#localize
         * @methodOf umbraco.services.localizationService
         *
         * @description
         * Checks the dictionary for a localized resource string
         * @param {String} value the area/key to localize in the format of 'section_key'
         * alternatively if no section is set such as 'key' then we assume the key is to be looked in
         * the 'general' section
         *
         * @param {Array} tokens if specified this array will be sent as parameter values
         * This replaces %0% and %1% etc in the dictionary key value with the passed in strings
         *
         * @param {String} fallbackValue if specified this string will be returned if no matching
         * entry was found in the dictionary
         *
         * @returns {String} localized resource string
         */
        localize: function (value, tokens, fallbackValue) {
            return service.initLocalizedResources().then(function (dic) {
                    return _lookup(value, tokens, dic, fallbackValue);
            });
        },

        /**
         * @ngdoc method
         * @name umbraco.services.localizationService#localizeMany
         * @methodOf umbraco.services.localizationService
         *
         * @description
         * Checks the dictionary for multipe localized resource strings at once, preventing the need for nested promises
         * with localizationService.localize
         *
         * ##Usage
         * <pre>
         * localizationService.localizeMany(["speechBubbles_templateErrorHeader", "speechBubbles_templateErrorText"]).then(function(data){
         *      var header = data[0];
         *      var message = data[1];
         *
         *
         *
         *
         *      notificationService.error(header, message);
         * });
         * </pre>
         *
         * @param {Array} keys is an array of strings of the area/key to localize in the format of 'section_key'
         * alternatively if no section is set such as 'key' then we assume the key is to be looked in
         * the 'general' section
         *
         * @returns {Array} An array of localized resource string in the same order
         */
        localizeMany: function(keys) {
            if(keys){

                //The LocalizationService.localize promises we want to resolve
                var promises = [];

                for(var i = 0; i < keys.length; i++){
                    promises.push(service.localize(keys[i], undefined));
                }

                return $q.all(promises).then(function(localizedValues){
                    return localizedValues;
                });
            }
        },

        /**
         * @ngdoc method
         * @name umbraco.services.localizationService#concat
         * @methodOf umbraco.services.localizationService
         *
         * @description
         * Checks the dictionary for multipe localized resource strings at once & concats them to a single string
         * Which was not possible with localizationSerivce.localize() due to returning a promise
         *
         * ##Usage
         * <pre>
         * localizationService.concat(["speechBubbles_templateErrorHeader", "speechBubbles_templateErrorText"]).then(function(data){
         *      var combinedText = data;
         * });
         * </pre>
         *
         * @param {Array} keys is an array of strings of the area/key to localize in the format of 'section_key'
         * alternatively if no section is set such as 'key' then we assume the key is to be looked in
         * the 'general' section
         *
         * @returns {String} An concatenated string of localized resource string passed into the function in the same order
         */
        concat: function(keys) {
            if(keys){

                //The LocalizationService.localize promises we want to resolve
                var promises = [];

                for(var i = 0; i < keys.length; i++){
                    promises.push(service.localize(keys[i], undefined));
                }

                return $q.all(promises).then(function(localizedValues){

                    //Build a concat string by looping over the array of resolved promises/translations
                    var returnValue = "";

                    for(var j = 0; j < localizedValues.length; j++){
                        returnValue += localizedValues[j];
                    }

                    return returnValue;
                });
            }
        },

        /**
         * @ngdoc method
         * @name umbraco.services.localizationService#format
         * @methodOf umbraco.services.localizationService
         *
         * @description
         * Checks the dictionary for multipe localized resource strings at once & formats a tokenized message
         * Which was not possible with localizationSerivce.localize() due to returning a promise
         *
         * ##Usage
         * <pre>
         * localizationService.format(["template_insert", "template_insertSections"], "%0% %1%").then(function(data){
         *      //Will return 'Insert Sections'
         *      var formattedResult = data;
         * });
         * </pre>
         *
         * @param {Array} keys is an array of strings of the area/key to localize in the format of 'section_key'
         * alternatively if no section is set such as 'key' then we assume the key is to be looked in
         * the 'general' section
         *
         * @param {String} message is the string you wish to replace containing tokens in the format of %0% and %1%
         * with the localized resource strings
         *
         * @returns {String} An concatenated string of localized resource string passed into the function in the same order
         */
        format: function(keys, message){
            if(keys){

                //The LocalizationService.localize promises we want to resolve
                var promises = [];

                for(var i = 0; i < keys.length; i++){
                    promises.push(service.localize(keys[i], undefined));
                }

                return $q.all(promises).then(function(localizedValues){

                    //Replace {0} and {1} etc in message with the localized values
                    for(var j = 0; j < localizedValues.length; j++){
                        var token = "%" + j + "%";
                        var regex = new RegExp(token, "g");

                        message = message.replace(regex, localizedValues[j]);
                    }

                    return message;
                });
            }
        }

    };

    //This happens after login / auth and assets loading
    eventsService.on("app.authenticated", function () {
        resourceFileLoadStatus = "none";
        resourceLoadingPromise = [];
    });


    // return the local instance when called
    return service;
});
