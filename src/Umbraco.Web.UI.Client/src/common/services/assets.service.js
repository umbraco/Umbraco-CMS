/**
 * @ngdoc service
 * @name umbraco.services.assetsService
 *
 * @requires $q
 * @requires angularHelper
 *
 * @description
 * Promise-based utillity service to lazy-load client-side dependencies inside angular controllers.
 *
 * ##usage
 * To use, simply inject the assetsService into any controller that needs it, and make
 * sure the umbraco.services module is accesible - which it should be by default.
 *
 * <pre>
 *      angular.module("umbraco").controller("my.controller". function(assetsService){
 *          assetsService.load(["script.js", "styles.css"], $scope).then(function(){
 *                 //this code executes when the dependencies are done loading
 *          });
 *      });
 * </pre>
 *
 * You can also load individual files, which gives you greater control over what attibutes are passed to the file, as well as timeout
 *
 * <pre>
 *      angular.module("umbraco").controller("my.controller". function(assetsService){
 *          assetsService.loadJs("script.js", $scope, {charset: 'utf-8'}, 10000 }).then(function(){
 *                 //this code executes when the script is done loading
 *          });
 *      });
 * </pre>
 *
 * For these cases, there are 2 individual methods, one for javascript, and one for stylesheets:
 *
 * <pre>
 *      angular.module("umbraco").controller("my.controller". function(assetsService){
 *          assetsService.loadCss("stye.css", $scope, {media: 'print'}, 10000 }).then(function(){
 *                 //loadcss cannot determine when the css is done loading, so this will trigger instantly
 *          });
 *      });
 * </pre>
 */
angular.module('umbraco.services')
    .factory('assetsService', function ($q, $log, angularHelper, umbRequestHelper, $rootScope, $http, userService, javascriptLibraryResource) {

        var initAssetsLoaded = false;

        function appendRnd(url) {
            //if we don't have a global umbraco obj yet, the app is bootstrapping
            if (!Umbraco.Sys.ServerVariables.application) {
                return url;
            }

            var rnd = Umbraco.Sys.ServerVariables.application.cacheBuster;
            var _op = (url.indexOf("?") > 0) ? "&" : "?";
            url = url + _op + "umb__rnd=" + rnd;
            return url;
        };

        function convertVirtualPath(path) {
            //make this work for virtual paths
            if (path.startsWith("~/")) {
                path = umbRequestHelper.convertVirtualToAbsolutePath(path);
            }
            return path;
        }

        function getMomentLocales(locales, supportedLocales) {
            return getLocales(locales, supportedLocales, 'lib/moment/');
        }

        function getFlatpickrLocales(locales, supportedLocales) {
            return getLocales(locales, supportedLocales, 'lib/flatpickr/l10n/');
        }
        
        function getLocales(locales, supportedLocales, path) {
            var localeUrls = [];
            var locales = locales.split(',');
            for (var i = 0; i < locales.length; i++) {
                var locale = locales[i].toString().toLowerCase();
                if (locale !== 'en-us') {
                    if (supportedLocales.indexOf(locale + '.js') > -1) {
                        localeUrls.push(path + locale + '.js');
                    }
                    if (locale.indexOf('-') > -1) {
                        var majorLocale = locale.split('-')[0] + '.js';
                        if (supportedLocales.indexOf(majorLocale) > -1) {
                            localeUrls.push(path + majorLocale);
                        }
                    }
                }
            }

            return localeUrls;
        }

        /**
         * Loads specific Moment.js and Flatpickr Locales.
         * @param {any} locales
         * @param {any} supportedLocales
         */
        function loadLocales(locales, supportedLocales) {
            var localeUrls = getMomentLocales(locales, supportedLocales.moment);
            localeUrls = localeUrls.concat(getFlatpickrLocales(locales, supportedLocales.flatpickr));
            if (localeUrls.length >= 1) {
                return service.load(localeUrls, $rootScope);
            }
            else {
                $q.when(true);
            }
        }

        /**
         * Loads in locale requirements during the _loadInitAssets call
         */
        function loadLocaleForCurrentUser() {

            userService.getCurrentUser().then(function (currentUser) {
                return javascriptLibraryResource.getSupportedLocales().then(function (supportedLocales) {
                    return loadLocales(currentUser.locale, supportedLocales);
                });
            });

        }

        var service = {
            loadedAssets: {},

            _getAssetPromise: function (path) {

                if (this.loadedAssets[path]) {
                    return this.loadedAssets[path];
                } else {
                    var deferred = $q.defer();
                    this.loadedAssets[path] = { deferred: deferred, state: "new", path: path };
                    return this.loadedAssets[path];
                }
            },

            /**
                Internal method. This is called when the application is loading and the user is already authenticated, or once the user is authenticated.
                There's a few assets the need to be loaded for the application to function but these assets require authentication to load.
            */
            _loadInitAssets: function () {

                //here we need to ensure the required application assets are loaded
                if (initAssetsLoaded === false) {
                    var self = this;
                    return self.loadJs(umbRequestHelper.getApiUrl("serverVarsJs", "", ""), $rootScope).then(function () {
                        initAssetsLoaded = true;
                        return loadLocaleForCurrentUser();
                    });
                }
                else {
                    return $q.when(true);
                }
            },

            loadLocales: loadLocales,

            /**
             * @ngdoc method
             * @name umbraco.services.assetsService#loadCss
             * @methodOf umbraco.services.assetsService
             *
             * @description
             * Injects a file as a stylesheet into the document head
             *
             * @param {String} path path to the css file to load
             * @param {Scope} scope optional scope to pass into the loader
             * @param {Object} keyvalue collection of attributes to pass to the stylesheet element
             * @param {Number} timeout in milliseconds
             * @returns {Promise} Promise object which resolves when the file has loaded
             */
            loadCss: function (path, scope, attributes, timeout) {

                path = convertVirtualPath(path);

                var asset = this._getAssetPromise(path); // $q.defer();
                var t = timeout || 5000;
                var a = attributes || undefined;

                if (asset.state === "new") {
                    asset.state = "loading";
                    LazyLoad.css(appendRnd(path), function () {
                        if (!scope) {
                            scope = $rootScope;
                        }
                        asset.state = "loaded";
                        angularHelper.safeApply(scope, function () {
                            asset.deferred.resolve(true);
                        });
                    });
                } else if (asset.state === "loaded") {
                    asset.deferred.resolve(true);
                }
                return asset.deferred.promise;
            },

            /**
             * @ngdoc method
             * @name umbraco.services.assetsService#loadJs
             * @methodOf umbraco.services.assetsService
             *
             * @description
             * Injects a file as a javascript into the document
             *
             * @param {String} path path to the js file to load
             * @param {Scope} scope optional scope to pass into the loader
             * @param {Object} keyvalue collection of attributes to pass to the script element
             * @param {Number} timeout in milliseconds
             * @returns {Promise} Promise object which resolves when the file has loaded
             */
            loadJs: function (path, scope, attributes, timeout) {

                path = convertVirtualPath(path);

                var asset = this._getAssetPromise(path); // $q.defer();
                var t = timeout || 5000;
                var a = attributes || undefined;

                if (asset.state === "new") {
                    asset.state = "loading";

                    LazyLoad.js(appendRnd(path), function () {
                        if (!scope) {
                            scope = $rootScope;
                        }
                        asset.state = "loaded";
                        angularHelper.safeApply(scope, function () {
                            asset.deferred.resolve(true);
                        });
                    });

                } else if (asset.state === "loaded") {
                    asset.deferred.resolve(true);
                }

                return asset.deferred.promise;
            },

            /**
             * @ngdoc method
             * @name umbraco.services.assetsService#load
             * @methodOf umbraco.services.assetsService
             *
             * @description
             * Injects a collection of css and js files
             *
             *
             * @param {Array} pathArray string array of paths to the files to load
             * @param {Scope} scope optional scope to pass into the loader
             * @param {string} defaultAssetType optional default asset type used to load assets with no extension 
             * @returns {Promise} Promise object which resolves when all the files has loaded
             */
            load: function (pathArray, scope, defaultAssetType) {
                var promise;

                if (!Utilities.isArray(pathArray)) {
                    throw "pathArray must be an array";
                }

                // Check to see if there's anything to load, resolve promise if not
                var nonEmpty = _.reject(pathArray, function (item) {
                    return item === undefined || item === "";
                });

                if (nonEmpty.length === 0) {
                    var deferred = $q.defer();
                    promise = deferred.promise;
                    deferred.resolve(true);
                    return promise;
                }

                //compile a list of promises
                //blocking
                var promises = [];
                var assets = [];
                nonEmpty.forEach(path => {
                    path = convertVirtualPath(path);
                    var asset = service._getAssetPromise(path);
                    //if not previously loaded, add to list of promises
                    if (asset.state !== "loaded") {
                        if (asset.state === "new") {
                            asset.state = "loading";
                            assets.push(asset);
                        }

                        //we need to always push to the promises collection to monitor correct execution
                        promises.push(asset.deferred.promise);
                    }
                });

                //gives a central monitoring of all assets to load
                promise = $q.all(promises);

                // Split into css and js asset arrays, and use LazyLoad on each array
                var cssAssets = [];
                var jsAssets = [];

                for (var i = 0; i < assets.length; i++) {
                    var asset = assets[i];
                    if (asset.path.match(/(\.css$|\.css\?)/ig)) {
                        cssAssets.push(asset);
                    } else if (asset.path.match(/(\.js$|\.js\?)/ig)) {
                        jsAssets.push(asset);
                    } else {
                        // Handle unknown assets
                        switch (defaultAssetType) {
                            case "css":
                                cssAssets.push(asset);
                                break;
                            case "js":
                                jsAssets.push(asset);
                                break;
                            default:
                                throw "Found unknown asset without a valid defaultAssetType specified";
                        }
                    }
                }

                function assetLoaded(asset) {
                    asset.state = "loaded";
                    if (!scope) {
                        scope = $rootScope;
                    }
                    angularHelper.safeApply(scope,
                        () => asset.deferred.resolve(true));
                }

                if (cssAssets.length > 0) {
                    var cssPaths = cssAssets.map(css => appendRnd(css.path));
                    LazyLoad.css(cssPaths, () => cssAssets.forEach(assetLoaded));
                }

                if (jsAssets.length > 0) {
                    var jsPaths = jsAssets.map(js => appendRnd(js.path));
                    LazyLoad.js(jsPaths, () => jsAssets.forEach(assetLoaded));
                }

                return promise;
            }
        };

        return service;
    });
