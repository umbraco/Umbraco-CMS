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
.factory('assetsService', function ($q, $log, angularHelper, umbRequestHelper, $rootScope, $http) {

    var initAssetsLoaded = false;
    var appendRnd = function(url){
        //if we don't have a global umbraco obj yet, the app is bootstrapping
        if(!Umbraco.Sys.ServerVariables.application){
            return url;
        }

        var rnd = Umbraco.Sys.ServerVariables.application.version +"."+Umbraco.Sys.ServerVariables.application.cdf;
        var _op = (url.indexOf("?")>0) ? "&" : "?";
        url = url + _op + "umb__rnd=" + rnd;
        return url;
    };

    var service = {
        loadedAssets:{},
        
        _getAssetPromise : function(path){
            if(this.loadedAssets[path]){
                return this.loadedAssets[path];
            }else{
                var deferred = $q.defer();
                this.loadedAssets[path] = {deferred: deferred, state: "new", path: path};
                return this.loadedAssets[path];
            }
        },
        /** 
            Internal method. This is called when the application is loading and the user is already authenticated, or once the user is authenticated.
            There's a few assets the need to be loaded for the application to function but these assets require authentication to load.
        */
        _loadInitAssets: function () {
            var deferred = $q.defer();
            //here we need to ensure the required application assets are loaded
            if (initAssetsLoaded === false) {
                var self = this;
                self.loadJs(umbRequestHelper.getApiUrl("serverVarsJs", "", ""), $rootScope).then(function() {
                    initAssetsLoaded = true;

                    //now we need to go get the legacyTreeJs - but this can be done async without waiting.
                    self.loadJs(umbRequestHelper.getApiUrl("legacyTreeJs", "", ""), $rootScope);

                    deferred.resolve();
                });
            }
            else {
                deferred.resolve();
            }
            return deferred.promise;
        },

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
         loadCss : function(path, scope, attributes, timeout){
             var asset = this._getAssetPromise(path); // $q.defer();
             var t = timeout || 5000;
             var a = attributes || undefined;
             
             if(asset.state === "new"){
                 asset.state = "loading";
                 LazyLoad.css(appendRnd(path), function () {
                   if (!scope) {
                       asset.state = "loaded";  
                       asset.deferred.resolve(true);
                   }else{
                       asset.state = "loaded";    
                       angularHelper.safeApply(scope, function () {
                           asset.deferred.resolve(true);
                       });
                   }
                 });    
             }else if(asset.state === "loaded"){
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
        loadJs : function(path, scope, attributes, timeout){
            
            var asset = this._getAssetPromise(path); // $q.defer();
            var t = timeout || 5000;
            var a = attributes || undefined;
            
            if(asset.state === "new"){
                asset.state = "loading";

                LazyLoad.js(appendRnd(path), function () {
                  if (!scope) {
                      asset.state = "loaded";  
                      asset.deferred.resolve(true);
                  }else{
                      asset.state = "loaded";  
                      angularHelper.safeApply(scope, function () {
                          asset.deferred.resolve(true);
                      });
                  }
                });

            }else if(asset.state === "loaded"){
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
         * Injects a collection of files, this can be ONLY js files
         * 
         *
         * @param {Array} pathArray string array of paths to the files to load
         * @param {Scope} scope optional scope to pass into the loader
         * @returns {Promise} Promise object which resolves when all the files has loaded
         */
        load: function (pathArray, scope) {
            var promise;

            if (!angular.isArray(pathArray)) {
                throw "pathArray must be an array";
            }

            var nonEmpty = _.reject(pathArray, function(item) {
                return item === undefined || item === "";
            });


            //don't load anything if there's nothing to load
            if (nonEmpty.length > 0) {
                var promises = [];
                var assets = [];

                //compile a list of promises
                //blocking
                _.each(nonEmpty, function(path){
                    var asset = service._getAssetPromise(path);
                    //if not previously loaded, add to list of promises
                    if(asset.state !== "loaded")
                    {
                        if(asset.state === "new"){
                            asset.state = "loading";
                            assets.push(asset);
                        }

                        //we need to always push to the promises collection to monitor correct 
                        //execution                        
                        promises.push(asset.deferred.promise);
                    }
                });


                //gives a central monitoring of all assets to load
                promise = $q.all(promises);
                
                _.each(assets, function(asset){
                    LazyLoad.js(appendRnd(asset.path), function () {
                      if (!scope) {
                          asset.state = "loaded";  
                          asset.deferred.resolve(true);
                      }else{
                          asset.state = "loaded";    
                          angularHelper.safeApply(scope, function () {
                              asset.deferred.resolve(true);
                          });
                      }
                    });    
                });
            }else{
                //return and resolve
                var deferred = $q.defer();
                promise = deferred.promise;
                deferred.resolve(true);
            }


            return promise;
        }
    };

    return service;
});