angular.module('umbraco.services')
.factory('localizationService', function ($http, $q, eventsService, $window, $filter, userService) {
        var service = {
            // array to hold the localized resource string entries
            dictionary:[],
            // location of the resource file
            url: "js/language.aspx",
            // flag to indicate if the service hs loaded the resource file
            resourceFileLoaded:false,

            // success handler for all server communication
            successCallback:function (data) {
                // store the returned array in the dictionary
                service.dictionary = data;
                // set the flag that the resource are loaded
                service.resourceFileLoaded = true;
                // broadcast that the file has been loaded
                eventsService.emit("localizationService.updated", data);
            },

            // allows setting of language on the fly
            setLanguage: function(value) {
                service.initLocalizedResources();
            },

            // allows setting of resource url on the fly
            setUrl: function(value) {
                service.url = value;
                service.initLocalizedResources();
            },

            // loads the language resource file from the server
            initLocalizedResources:function () {
                var deferred = $q.defer();
                // build the url to retrieve the localized resource file
                $http({ method:"GET", url:service.url, cache:false })
                    .then(function(response){
                        service.resourceFileLoaded = true;
                        service.dictionary = response.data;

                        eventsService.emit("localizationService.updated", service.dictionary);

                        return deferred.resolve(service.dictionary);
                    }, function(err){
                        return deferred.reject("Something broke");
                    });
                return deferred.promise;
            },

            //helper to tokenize and compile a localization string
            tokenize: function(value,scope) {
                    if(value){
                        var localizer = value.split(':');
                        var retval = {tokens: undefined, key: localizer[0].substring(0)};
                        if(localizer.length > 1){
                            retval.tokens = localizer[1].split(',');
                            for (var x = 0; x < retval.tokens.length; x++) {
                                retval.tokens[x] = scope.$eval(retval.tokens[x]);
                            }
                        }

                        return retval;
                    }
            },

            // checks the dictionary for a localized resource string
            localize: function(value,tokens) {
                var deferred = $q.defer();

                if(service.resourceFileLoaded){
                    var val = service._lookup(value,tokens);
                    deferred.resolve(val);
                }else{
                    service.initLocalizedResources().then(function(dic){
                           var val = service._lookup(value,tokens);
                           deferred.resolve(val); 
                    });
                }

                return deferred.promise;
            },
            _lookup: function(value,tokens){

                //strip the key identifier if its there
                if(value && value[0] === "@"){
                    value = value.substring(1);
                }

                //if no area specified, add general_
                if(value && value.indexOf("_") < 0){
                    value = "general_" + value;
                }

                var entry = service.dictionary[value];
                if(entry){
                    if(tokens){
                        for (var i = 0; i < tokens.length; i++) {
                            entry = entry.replace("%"+i+"%", tokens[i]);
                        }    
                    }
                    return entry;
                }
                return "[" + value + "]";
            }
        };

        // force the load of the resource file
        service.initLocalizedResources();

        //This happens after login / auth and assets loading
        eventsService.on("app.authenticated", function(){
            service.resourceFileLoaded = false;
        });

        // return the local instance when called
        return service;
    });