(function () {
    'use strict';
    angular.module('umbraco.services', [
        'umbraco.interceptors',
        'umbraco.resources'
    ]);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.angularHelper
 * @function
 *
 * @description
 * Some angular helper/extension methods
 */
    function angularHelper($q) {
        var requiredFormProps = [
            '$error',
            '$name',
            '$dirty',
            '$pristine',
            '$valid',
            '$submitted',
            '$pending'
        ];
        function collectAllFormErrorsRecursively(formCtrl, allErrors) {
            // skip if the control is already added to the array
            if (allErrors.indexOf(formCtrl) !== -1) {
                return;
            }
            // loop over the error dictionary (see https://docs.angularjs.org/api/ng/type/form.FormController#$error)
            var keys = Object.keys(formCtrl.$error);
            if (keys.length === 0) {
                return;
            }
            keys.forEach(function (validationKey) {
                var ctrls = formCtrl.$error[validationKey];
                ctrls.forEach(function (ctrl) {
                    if (!ctrl) {
                        // this happens when $setValidity('err', true) is called on a form controller without specifying the 3rd parameter for the control/form
                        // which just means that this is an error on the formCtrl itself
                        allErrors.push(formCtrl);    // add the error
                    } else if (isForm(ctrl)) {
                        // sometimes the control in error is the same form so we cannot recurse else we'll cause an infinite loop
                        // and in this case it means the error is assigned directly to the form, not a control
                        if (ctrl === formCtrl) {
                            allErrors.push(ctrl);
                            // add the error
                            return;
                        }
                        // recurse with the sub form
                        collectAllFormErrorsRecursively(ctrl, allErrors);
                    } else {
                        // it's a normal control
                        allErrors.push(ctrl);    // add the error
                    }
                });
            });
        }
        function isForm(obj) {
            // a method to check that the collection of object prop names contains the property name expected
            function allPropertiesExist(objectPropNames) {
                //ensure that every required property name exists on the current object
                return _.every(requiredFormProps, function (item) {
                    return _.contains(objectPropNames, item);
                });
            }
            //get the keys of the property names for the current object
            var props = _.keys(obj);
            //if the length isn't correct, try the next prop
            if (props.length < requiredFormProps.length) {
                return false;
            }
            //ensure that every required property name exists on the current scope property
            return allPropertiesExist(props);
        }
        return {
            countAllFormErrors: function countAllFormErrors(formCtrl) {
                var allErrors = [];
                collectAllFormErrorsRecursively(formCtrl, allErrors);
                return allErrors.length;
            },
            /**
     * Will traverse up the $scope chain to all ancestors until the predicate matches for the current scope or until it's at the root.
     * @param {any} scope
     * @param {any} predicate
     */
            traverseScopeChain: function traverseScopeChain(scope, predicate) {
                var s = scope.$parent;
                while (s) {
                    var result = predicate(s);
                    if (result === true) {
                        return s;
                    }
                    s = s.$parent;
                }
                return null;
            },
            /**
     * Method used to re-run the $parsers for a given ngModel
     * @param {} scope
     * @param {} ngModel
     * @returns {}
     */
            revalidateNgModel: function revalidateNgModel(scope, ngModel) {
                this.safeApply(scope, function () {
                    ngModel.$parsers.forEach(function (parser) {
                        parser(ngModel.$viewValue);
                    });
                });
            },
            /**
     * Execute a list of promises sequentially. Unlike $q.all which executes all promises at once, this will execute them in sequence.
     * @param {} promises
     * @returns {}
     */
            executeSequentialPromises: function executeSequentialPromises(promises) {
                //this is sequential promise chaining, it's not pretty but we need to do it this way.
                //$q.all doesn't execute promises in sequence but that's what we want to do here.
                if (!Utilities.isArray(promises)) {
                    throw 'promises must be an array';
                }
                //now execute them in sequence... sorry there's no other good way to do it with angular promises
                var j = 0;
                function pExec(promise) {
                    j++;
                    return promise.then(function (data) {
                        if (j === promises.length) {
                            return $q.when(data);    //exit
                        } else {
                            return pExec(promises[j]);    //recurse
                        }
                    });
                }
                if (promises.length > 0) {
                    return pExec(promises[0]);    //start the promise chain
                } else {
                    return $q.when(true);    // just exit, no promises to execute
                }
            },
            /**
     * @ngdoc function
     * @name safeApply
     * @methodOf umbraco.services.angularHelper
     * @function
     *
     * @description
     * This checks if a digest/apply is already occuring, if not it will force an apply call
     */
            safeApply: function safeApply(scope, fn) {
                if (scope.$$phase || scope.$root && scope.$root.$$phase) {
                    if (Utilities.isFunction(fn)) {
                        fn();
                    }
                } else {
                    if (Utilities.isFunction(fn)) {
                        scope.$apply(fn);
                    } else {
                        scope.$apply();
                    }
                }
            },
            isForm: isForm,
            /**
     * @ngdoc function
     * @name getCurrentForm
     * @methodOf umbraco.services.angularHelper
     * @function
     *
     * @description
     * Returns the current form object applied to the scope or null if one is not found
     */
            getCurrentForm: function getCurrentForm(scope) {
                //NOTE: There isn't a way in angular to get a reference to the current form object since the form object
                // is just defined as a property of the scope when it is named but you'll always need to know the name which
                // isn't very convenient. If we want to watch for validation changes we need to get a form reference.
                // The way that we detect the form object is a bit hackerific in that we detect all of the required properties
                // that exist on a form object.
                //
                //The other way to do it in a directive is to require "^form", but in a controller the only other way to do it
                // is to inject the $element object and use: $element.inheritedData('$formController');
                var form = null;
                for (var p in scope) {
                    if (_.isObject(scope[p]) && p !== 'this' && p.substr(0, 1) !== '$') {
                        if (this.isForm(scope[p])) {
                            form = scope[p];
                            break;
                        }
                    }
                }
                return form;
            },
            /**
     * @ngdoc function
     * @name validateHasForm
     * @methodOf umbraco.services.angularHelper
     * @function
     *
     * @description
     * This will validate that the current scope has an assigned form object, if it doesn't an exception is thrown, if
     * it does we return the form object.
     */
            getRequiredCurrentForm: function getRequiredCurrentForm(scope) {
                var currentForm = this.getCurrentForm(scope);
                if (!currentForm || !currentForm.$name) {
                    throw 'The current scope requires a current form object (or ng-form) with a name assigned to it';
                }
                return currentForm;
            },
            /**
     * @ngdoc function
     * @name getNullForm
     * @methodOf umbraco.services.angularHelper
     * @function
     *
     * @description
     * Returns a null angular FormController, mostly for use in unit tests
     *      NOTE: This is actually the same construct as angular uses internally for creating a null form but they don't expose
     *          any of this publicly to us, so we need to create our own.
     *      NOTE: The properties has been added to the null form because we use them to get a form on a scope.
     *
     * @param {string} formName The form name to assign
     */
            getNullForm: function getNullForm(formName) {
                return {
                    $error: {},
                    $dirty: false,
                    $pristine: true,
                    $valid: true,
                    $submitted: false,
                    $pending: undefined,
                    $addControl: Utilities.noop,
                    $removeControl: Utilities.noop,
                    $setValidity: Utilities.noop,
                    $setDirty: Utilities.noop,
                    $setPristine: Utilities.noop,
                    $name: formName
                };
            }
        };
    }
    angular.module('umbraco.services').factory('angularHelper', angularHelper);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.appState
 * @function
 *
 * @description
 * Tracks the various application state variables when working in the back office, raises events when state changes.
 *
 * ##Samples
 *
 * ####Subscribe to global state changes:
 * 
 * <pre>
  *    scope.showTree = appState.getGlobalState("showNavigation");
  *
  *    eventsService.on("appState.globalState.changed", function (e, args) {
  *               if (args.key === "showNavigation") {
  *                   scope.showTree = args.value;
  *               }
  *           });  
  * </pre>
 *
 * ####Subscribe to section-state changes
 *
 * <pre>
 *    scope.currentSection = appState.getSectionState("currentSection");
 *
 *    eventsService.on("appState.sectionState.changed", function (e, args) {
 *               if (args.key === "currentSection") {
 *                   scope.currentSection = args.value;
 *               }
 *           });  
 * </pre>
 */
    function appState(eventsService) {
        //Define all variables here - we are never returning this objects so they cannot be publicly mutable
        // changed, we only expose methods to interact with the values.
        var globalState = {
            showNavigation: null,
            touchDevice: null,
            showTray: null,
            stickyNavigation: null,
            navMode: null,
            isReady: null,
            isTablet: null
        };
        var sectionState = {
            //The currently active section
            currentSection: null,
            showSearchResults: null
        };
        var treeState = {
            //The currently selected node
            selectedNode: null,
            //The currently loaded root node reference - depending on the section loaded this could be a section root or a normal root.
            //We keep this reference so we can lookup nodes to interact with in the UI via the tree service
            currentRootNode: null
        };
        var menuState = {
            //this list of menu items to display
            menuActions: null,
            //the title to display in the context menu dialog
            dialogTitle: null,
            //The tree node that the ctx menu is launched for
            currentNode: null,
            //Whether the menu's dialog is being shown or not
            showMenuDialog: null,
            //Whether the menu's dialog can be hidden or not
            allowHideMenuDialog: true,
            // The dialogs template
            dialogTemplateUrl: null,
            //Whether the context menu is being shown or not
            showMenu: null
        };
        var searchState = {
            //Whether the search is being shown or not
            show: null
        };
        var drawerState = {
            //this view to show
            view: null,
            // bind custom values to the drawer
            model: null,
            //Whether the drawer is being shown or not
            showDrawer: null
        };
        /** function to validate and set the state on a state object */
        function setState(stateObj, key, value, stateObjName) {
            if (!_.has(stateObj, key)) {
                throw 'The variable ' + key + ' does not exist in ' + stateObjName;
            }
            var changed = stateObj[key] !== value;
            stateObj[key] = value;
            if (changed) {
                eventsService.emit('appState.' + stateObjName + '.changed', {
                    key: key,
                    value: value
                });
            }
        }
        /** function to validate and set the state on a state object */
        function getState(stateObj, key, stateObjName) {
            if (!_.has(stateObj, key)) {
                throw 'The variable ' + key + ' does not exist in ' + stateObjName;
            }
            return stateObj[key];
        }
        return {
            /**
     * @ngdoc function
     * @name umbraco.services.angularHelper#getGlobalState
     * @methodOf umbraco.services.appState
     * @function
     *
     * @description
     * Returns the current global state value by key - we do not return an object reference here - we do NOT want this
     * to be publicly mutable and allow setting arbitrary values
     *
     */
            getGlobalState: function getGlobalState(key) {
                return getState(globalState, key, 'globalState');
            },
            /**
     * @ngdoc function
     * @name umbraco.services.angularHelper#setGlobalState
     * @methodOf umbraco.services.appState
     * @function
     *
     * @description
     * Sets a global state value by key
     *
     */
            setGlobalState: function setGlobalState(key, value) {
                setState(globalState, key, value, 'globalState');
            },
            /**
     * @ngdoc function
     * @name umbraco.services.angularHelper#getSectionState
     * @methodOf umbraco.services.appState
     * @function
     *
     * @description
     * Returns the current section state value by key - we do not return an object here - we do NOT want this
     * to be publicly mutable and allow setting arbitrary values
     *
     */
            getSectionState: function getSectionState(key) {
                return getState(sectionState, key, 'sectionState');
            },
            /**
     * @ngdoc function
     * @name umbraco.services.angularHelper#setSectionState
     * @methodOf umbraco.services.appState
     * @function
     *
     * @description
     * Sets a section state value by key
     *
     */
            setSectionState: function setSectionState(key, value) {
                setState(sectionState, key, value, 'sectionState');
            },
            /**
     * @ngdoc function
     * @name umbraco.services.angularHelper#getTreeState
     * @methodOf umbraco.services.appState
     * @function
     *
     * @description
     * Returns the current tree state value by key - we do not return an object here - we do NOT want this
     * to be publicly mutable and allow setting arbitrary values
     *
     */
            getTreeState: function getTreeState(key) {
                return getState(treeState, key, 'treeState');
            },
            /**
     * @ngdoc function
     * @name umbraco.services.angularHelper#setTreeState
     * @methodOf umbraco.services.appState
     * @function
     *
     * @description
     * Sets a section state value by key
     *
     */
            setTreeState: function setTreeState(key, value) {
                setState(treeState, key, value, 'treeState');
            },
            /**
     * @ngdoc function
     * @name umbraco.services.angularHelper#getMenuState
     * @methodOf umbraco.services.appState
     * @function
     *
     * @description
     * Returns the current menu state value by key - we do not return an object here - we do NOT want this
     * to be publicly mutable and allow setting arbitrary values
     *
     */
            getMenuState: function getMenuState(key) {
                return getState(menuState, key, 'menuState');
            },
            /**
     * @ngdoc function
     * @name umbraco.services.angularHelper#setMenuState
     * @methodOf umbraco.services.appState
     * @function
     *
     * @description
     * Sets a section state value by key
     *
     */
            setMenuState: function setMenuState(key, value) {
                setState(menuState, key, value, 'menuState');
            },
            /**
     * @ngdoc function
     * @name umbraco.services.angularHelper#getSearchState
     * @methodOf umbraco.services.appState
     * @function
     *
     * @description
     * Returns the current search state value by key - we do not return an object here - we do NOT want this
     * to be publicly mutable and allow setting arbitrary values
     *
     */
            getSearchState: function getSearchState(key) {
                return getState(searchState, key, 'searchState');
            },
            /**
     * @ngdoc function
     * @name umbraco.services.angularHelper#setSearchState
     * @methodOf umbraco.services.appState
     * @function
     *
     * @description
     * Sets a section state value by key
     *
     */
            setSearchState: function setSearchState(key, value) {
                setState(searchState, key, value, 'searchState');
            },
            /**
     * @ngdoc function
     * @name umbraco.services.angularHelper#getDrawerState
     * @methodOf umbraco.services.appState
     * @function
     *
     * @description
     * Returns the current drawer state value by key - we do not return an object here - we do NOT want this
     * to be publicly mutable and allow setting arbitrary values
     *
     */
            getDrawerState: function getDrawerState(key) {
                return getState(drawerState, key, 'drawerState');
            },
            /**
     * @ngdoc function
     * @name umbraco.services.angularHelper#setDrawerState
     * @methodOf umbraco.services.appState
     * @function
     *
     * @description
     * Sets a drawer state value by key
     *
     */
            setDrawerState: function setDrawerState(key, value) {
                setState(drawerState, key, value, 'drawerState');
            }
        };
    }
    angular.module('umbraco.services').factory('appState', appState);
    'use strict';
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
    angular.module('umbraco.services').factory('assetsService', function ($q, $log, angularHelper, umbRequestHelper, $rootScope, $http, userService, javascriptLibraryResource) {
        var initAssetsLoaded = false;
        function appendRnd(url) {
            //if we don't have a global umbraco obj yet, the app is bootstrapping
            if (!Umbraco.Sys.ServerVariables.application) {
                return url;
            }
            var rnd = Umbraco.Sys.ServerVariables.application.cacheBuster;
            var _op = url.indexOf('?') > 0 ? '&' : '?';
            url = url + _op + 'umb__rnd=' + rnd;
            return url;
        }
        ;
        function convertVirtualPath(path) {
            //make this work for virtual paths
            if (path.startsWith('~/')) {
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
            } else {
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
            _getAssetPromise: function _getAssetPromise(path) {
                if (this.loadedAssets[path]) {
                    return this.loadedAssets[path];
                } else {
                    var deferred = $q.defer();
                    this.loadedAssets[path] = {
                        deferred: deferred,
                        state: 'new',
                        path: path
                    };
                    return this.loadedAssets[path];
                }
            },
            /**
        Internal method. This is called when the application is loading and the user is already authenticated, or once the user is authenticated.
        There's a few assets the need to be loaded for the application to function but these assets require authentication to load.
    */
            _loadInitAssets: function _loadInitAssets() {
                //here we need to ensure the required application assets are loaded
                if (initAssetsLoaded === false) {
                    var self = this;
                    return self.loadJs(umbRequestHelper.getApiUrl('serverVarsJs', '', ''), $rootScope).then(function () {
                        initAssetsLoaded = true;
                        return loadLocaleForCurrentUser();
                    });
                } else {
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
            loadCss: function loadCss(path, scope, attributes, timeout) {
                path = convertVirtualPath(path);
                var asset = this._getAssetPromise(path);
                // $q.defer();
                var t = timeout || 5000;
                var a = attributes || undefined;
                if (asset.state === 'new') {
                    asset.state = 'loading';
                    LazyLoad.css(appendRnd(path), function () {
                        if (!scope) {
                            scope = $rootScope;
                        }
                        asset.state = 'loaded';
                        angularHelper.safeApply(scope, function () {
                            asset.deferred.resolve(true);
                        });
                    });
                } else if (asset.state === 'loaded') {
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
            loadJs: function loadJs(path, scope, attributes, timeout) {
                path = convertVirtualPath(path);
                var asset = this._getAssetPromise(path);
                // $q.defer();
                var t = timeout || 5000;
                var a = attributes || undefined;
                if (asset.state === 'new') {
                    asset.state = 'loading';
                    LazyLoad.js(appendRnd(path), function () {
                        if (!scope) {
                            scope = $rootScope;
                        }
                        asset.state = 'loaded';
                        angularHelper.safeApply(scope, function () {
                            asset.deferred.resolve(true);
                        });
                    });
                } else if (asset.state === 'loaded') {
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
            load: function load(pathArray, scope, defaultAssetType) {
                var promise;
                if (!Utilities.isArray(pathArray)) {
                    throw 'pathArray must be an array';
                }
                // Check to see if there's anything to load, resolve promise if not
                var nonEmpty = _.reject(pathArray, function (item) {
                    return item === undefined || item === '';
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
                nonEmpty.forEach(function (path) {
                    path = convertVirtualPath(path);
                    var asset = service._getAssetPromise(path);
                    //if not previously loaded, add to list of promises
                    if (asset.state !== 'loaded') {
                        if (asset.state === 'new') {
                            asset.state = 'loading';
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
                        case 'css':
                            cssAssets.push(asset);
                            break;
                        case 'js':
                            jsAssets.push(asset);
                            break;
                        default:
                            throw 'Found unknown asset without a valid defaultAssetType specified';
                        }
                    }
                }
                function assetLoaded(asset) {
                    asset.state = 'loaded';
                    if (!scope) {
                        scope = $rootScope;
                    }
                    angularHelper.safeApply(scope, function () {
                        return asset.deferred.resolve(true);
                    });
                }
                if (cssAssets.length > 0) {
                    var cssPaths = cssAssets.map(function (css) {
                        return appendRnd(css.path);
                    });
                    LazyLoad.css(cssPaths, function () {
                        return cssAssets.forEach(assetLoaded);
                    });
                }
                if (jsAssets.length > 0) {
                    var jsPaths = jsAssets.map(function (js) {
                        return appendRnd(js.path);
                    });
                    LazyLoad.js(jsPaths, function () {
                        return jsAssets.forEach(assetLoaded);
                    });
                }
                return promise;
            }
        };
        return service;
    });
    'use strict';
    /**
 @ngdoc service
 * @name umbraco.services.backdropService
 *
 * @description
 * <b>Added in Umbraco 7.8</b>. Application-wide service for handling backdrops.
 * 
 */
    (function () {
        'use strict';
        function backdropService(eventsService) {
            var args = {
                opacity: null,
                element: null,
                elementPreventClick: false,
                disableEventsOnClick: false,
                show: false
            };
            /**
     * @ngdoc method
     * @name umbraco.services.backdropService#open
     * @methodOf umbraco.services.backdropService
     *
     * @description
     * Raises an event to open a backdrop
    * @param {Object} options The backdrop options
     * @param {Number} options.opacity Sets the opacity on the backdrop (default 0.4)
     * @param {DomElement} options.element Highlights a DOM-element (HTML-selector)
     * @param {Boolean} options.elementPreventClick Adds blocking element on top of highligted area to prevent all clicks
     * @param {Boolean} options.disableEventsOnClick Disables all raised events when the backdrop is clicked
    */
            function open(options) {
                if (options && options.element) {
                    args.element = options.element;
                }
                if (options && options.disableEventsOnClick) {
                    args.disableEventsOnClick = options.disableEventsOnClick;
                }
                args.show = true;
                eventsService.emit('appState.backdrop', args);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.backdropService#close
     * @methodOf umbraco.services.backdropService
     *
     * @description
     * Raises an event to close the backdrop
     * 
    */
            function close() {
                args.opacity = null, args.element = null, args.elementPreventClick = false, args.disableEventsOnClick = false, args.show = false;
                eventsService.emit('appState.backdrop', args);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.backdropService#setOpacity
     * @methodOf umbraco.services.backdropService
     *
     * @description
     * Raises an event which updates the opacity option on the backdrop
    */
            function setOpacity(opacity) {
                args.opacity = opacity;
                eventsService.emit('appState.backdrop', args);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.backdropService#setHighlight
     * @methodOf umbraco.services.backdropService
     *
     * @description
     * Raises an event which updates the element option on the backdrop
    */
            function setHighlight(element, preventClick) {
                args.element = element;
                args.elementPreventClick = preventClick;
                eventsService.emit('appState.backdrop', args);
            }
            var service = {
                open: open,
                close: close,
                setOpacity: setOpacity,
                setHighlight: setHighlight
            };
            return service;
        }
        angular.module('umbraco.services').factory('backdropService', backdropService);
    }());
    'use strict';
    function _typeof(obj) {
        if (typeof Symbol === 'function' && typeof Symbol.iterator === 'symbol') {
            _typeof = function _typeof(obj) {
                return typeof obj;
            };
        } else {
            _typeof = function _typeof(obj) {
                return obj && typeof Symbol === 'function' && obj.constructor === Symbol && obj !== Symbol.prototype ? 'symbol' : typeof obj;
            };
        }
        return _typeof(obj);
    }
    /**
 * @ngdoc service
 * @name umbraco.services.blockEditorService
 *
 * @description
 * <b>Added in Umbraco 8.7</b>. Service for dealing with Block Editors.
 *
 * Block Editor Service provides the basic features for a block editor.
 * The main feature is the ability to create a Model Object which takes care of your data for your Block Editor.
 *
 *
 * ##Samples
 *
 * ####Instantiate a Model Object for your property editor:
 *
 * <pre>
 *     modelObject = blockEditorService.createModelObject(vm.model.value, vm.model.editor, vm.model.config.blocks, $scope);
 *     modelObject.load().then(onLoaded);
 * </pre>
 *
 *
 * See {@link umbraco.services.blockEditorModelObject BlockEditorModelObject} for more samples.
 *
 */
    (function () {
        'use strict';
        /**
   * When performing a runtime copy of Block Editors entries, we copy the ElementType Data Model and inner IDs are kept identical, to ensure new IDs are changed on paste we need to provide a resolver for the ClipboardService.
   */
        angular.module('umbraco').run([
            'clipboardService',
            'udiService',
            function (clipboardService, udiService) {
                function replaceUdi(obj, key, dataObject) {
                    var udi = obj[key];
                    var newUdi = udiService.create('element');
                    obj[key] = newUdi;
                    dataObject.forEach(function (data) {
                        if (data.udi === udi) {
                            data.udi = newUdi;
                        }
                    });
                }
                function replaceUdisOfObject(obj, propValue) {
                    for (var k in obj) {
                        if (k === 'contentUdi') {
                            replaceUdi(obj, k, propValue.contentData);
                        } else if (k === 'settingsUdi') {
                            replaceUdi(obj, k, propValue.settingsData);
                        } else {
                            // lets crawl through all properties of layout to make sure get captured all `contentUdi` and `settingsUdi` properties.
                            var propType = _typeof(obj[k]);
                            if (propType != null && (propType === 'object' || propType === 'array')) {
                                replaceUdisOfObject(obj[k], propValue);
                            }
                        }
                    }
                }
                function removeBlockReferences(obj) {
                    for (var k in obj) {
                        if (k === 'contentUdi') {
                            delete obj[k];
                        } else if (k === 'settingsUdi') {
                            delete obj[k];
                        } else {
                            // lets crawl through all properties of layout to make sure get captured all `contentUdi` and `settingsUdi` properties.
                            var propType = _typeof(obj[k]);
                            if (propType != null && (propType === 'object' || propType === 'array')) {
                                removeBlockReferences(obj[k]);
                            }
                        }
                    }
                }
                function elementTypeBlockResolver(obj, propPasteResolverMethod) {
                    // we could filter for specific Property Editor Aliases, but as the Block Editor structure can be used by many Property Editor we do not in this code know a good way to detect that this is a Block Editor and will therefor leave it to the value structure to determin this.
                    rawBlockResolver(obj.value, propPasteResolverMethod);
                }
                clipboardService.registerPastePropertyResolver(elementTypeBlockResolver, clipboardService.TYPES.ELEMENT_TYPE);
                function rawBlockResolver(value, propPasteResolverMethod) {
                    if (value != null && _typeof(value) === 'object') {
                        // we got an object, and it has these three props then we are most likely dealing with a Block Editor.
                        if (value.layout !== undefined && value.contentData !== undefined && value.settingsData !== undefined) {
                            replaceUdisOfObject(value.layout, value);
                            // run resolvers for inner properties of this Blocks content data.
                            if (value.contentData.length > 0) {
                                value.contentData.forEach(function (item) {
                                    for (var k in item) {
                                        propPasteResolverMethod(item[k], clipboardService.TYPES.RAW);
                                    }
                                });
                            }
                            // run resolvers for inner properties of this Blocks settings data.
                            if (value.settingsData.length > 0) {
                                value.settingsData.forEach(function (item) {
                                    for (var k in item) {
                                        propPasteResolverMethod(item[k], clipboardService.TYPES.RAW);
                                    }
                                });
                            }
                        }
                    }
                }
                clipboardService.registerPastePropertyResolver(rawBlockResolver, clipboardService.TYPES.RAW);
                function provideNewUdisForBlockResolver(block, propPasteResolverMethod) {
                    if (block.layout) {
                        // We do not support layout child blocks currently, these should be stripped out as we only will be copying a single entry.
                        removeBlockReferences(block.layout);
                    }
                    if (block.data) {
                        // Make new UDI for content-element
                        block.data.udi = block.layout.contentUdi = udiService.create('element');
                    }
                    if (block.settingsData) {
                        // Make new UDI for settings-element
                        block.settingsData.udi = block.layout.settingsUdi = udiService.create('element');
                    }
                }
                clipboardService.registerPastePropertyResolver(provideNewUdisForBlockResolver, clipboardService.TYPES.BLOCK);
            }
        ]);
        function blockEditorService(blockEditorModelObject) {
            /**
     * @ngdocs function
     * @name createModelObject
     * @methodOf umbraco.services.blockEditorService
     *
     * @description
     * Create a new Block Editor Model Object.
     * See {@link umbraco.services.blockEditorModelObject blockEditorModelObject}
     *
     * @see umbraco.services.blockEditorModelObject
     * @param {object} propertyModelValue data object of the property editor, usually model.value.
     * @param {string} propertyEditorAlias alias of the property.
     * @param {object} blockConfigurations block configurations.
     * @param {angular-scope} scopeOfExistance A local angularJS scope that exists as long as the data exists.
     * @param {angular-scope} propertyEditorScope A local angularJS scope that represents the property editors scope.
     * @return {blockEditorModelObject} A instance of the BlockEditorModelObject class.
     */
            function createModelObject(propertyModelValue, propertyEditorAlias, blockConfigurations, scopeOfExistance, propertyEditorScope) {
                return new blockEditorModelObject(propertyModelValue, propertyEditorAlias, blockConfigurations, scopeOfExistance, propertyEditorScope);
            }
            return { createModelObject: createModelObject };
        }
        angular.module('umbraco.services').service('blockEditorService', blockEditorService);
    }());
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.blockEditorModelObject
 *
 * @description
 * <b>Added in Umbraco 8.7</b>. Model Object for dealing with data of Block Editors.
 *
 * Block Editor Model Object provides the basic features for editing data of a block editor.<br/>
 * Use the Block Editor Service to instantiate the Model Object.<br/>
 * See {@link umbraco.services.blockEditorService blockEditorService}
 *
 */
    (function () {
        'use strict';
        function blockEditorModelObjectFactory($interpolate, $q, udiService, contentResource, localizationService, umbRequestHelper, clipboardService, notificationsService) {
            /**
     * Simple mapping from property model content entry to editing model,
     * needs to stay simple to avoid deep watching.
     */
            function mapToElementModel(elementModel, dataModel) {
                if (!elementModel || !elementModel.variants || !elementModel.variants.length) {
                    return;
                }
                var variant = elementModel.variants[0];
                for (var t = 0; t < variant.tabs.length; t++) {
                    var tab = variant.tabs[t];
                    for (var p = 0; p < tab.properties.length; p++) {
                        var prop = tab.properties[p];
                        if (dataModel[prop.alias]) {
                            prop.value = dataModel[prop.alias];
                        }
                    }
                }
            }
            /**
     * Simple mapping from elementModel to property model content entry,
     * needs to stay simple to avoid deep watching.
     */
            function mapToPropertyModel(elementModel, dataModel) {
                if (!elementModel || !elementModel.variants || !elementModel.variants.length) {
                    return;
                }
                var variant = elementModel.variants[0];
                for (var t = 0; t < variant.tabs.length; t++) {
                    var tab = variant.tabs[t];
                    for (var p = 0; p < tab.properties.length; p++) {
                        var prop = tab.properties[p];
                        if (prop.value) {
                            dataModel[prop.alias] = prop.value;
                        }
                    }
                }
            }
            /**
     * Map property values from an ElementModel to another ElementModel.
     * Used to tricker watchers for synchronization.
     * @param {Object} fromModel ElementModel to recive property values from.
     * @param {Object} toModel ElementModel to recive property values from.
     */
            function mapElementValues(fromModel, toModel) {
                if (!fromModel || !fromModel.variants) {
                    toModel.variants = null;
                    return;
                }
                if (!fromModel.variants.length) {
                    toModel.variants = [];
                    return;
                }
                var fromVariant = fromModel.variants[0];
                if (!fromVariant) {
                    toModel.variants = [null];
                    return;
                }
                var toVariant = toModel.variants[0];
                for (var t = 0; t < fromVariant.tabs.length; t++) {
                    var fromTab = fromVariant.tabs[t];
                    var toTab = toVariant.tabs[t];
                    for (var p = 0; p < fromTab.properties.length; p++) {
                        var fromProp = fromTab.properties[p];
                        var toProp = toTab.properties[p];
                        toProp.value = fromProp.value;
                    }
                }
            }
            /**
     * Generate label for Block, uses either the labelInterpolator or falls back to the contentTypeName.
     * @param {Object} blockObject BlockObject to recive data values from.
     */
            function getBlockLabel(blockObject) {
                if (blockObject.labelInterpolator !== undefined) {
                    var labelVars = Object.assign({
                        '$settings': blockObject.settingsData || {},
                        '$layout': blockObject.layout || {},
                        '$index': (blockObject.index || 0) + 1
                    }, blockObject.data);
                    return blockObject.labelInterpolator(labelVars);
                }
                return blockObject.content.contentTypeName;
            }
            /**
     * Used to add watchers on all properties in a content or settings model
     */
            function addWatchers(blockObject, isolatedScope, forSettings) {
                var model = forSettings ? blockObject.settings : blockObject.content;
                if (!model || !model.variants || !model.variants.length) {
                    return;
                }
                // Start watching each property value.
                var variant = model.variants[0];
                var field = forSettings ? 'settings' : 'content';
                var watcherCreator = forSettings ? createSettingsModelPropWatcher : createContentModelPropWatcher;
                for (var t = 0; t < variant.tabs.length; t++) {
                    var tab = variant.tabs[t];
                    for (var p = 0; p < tab.properties.length; p++) {
                        var prop = tab.properties[p];
                        // Watch value of property since this is the only value we want to keep synced.
                        // Do notice that it is not performing a deep watch, meaning that we are only watching primitive and changes directly to the object of property-value.
                        // But we like to sync non-primitive values as well! Yes, and this does happen, just not through this code, but through the nature of JavaScript.
                        // Non-primitive values act as references to the same data and are therefor synced.
                        blockObject.__watchers.push(isolatedScope.$watch('blockObjects._' + blockObject.key + '.' + field + '.variants[0].tabs[' + t + '].properties[' + p + '].value', watcherCreator(blockObject, prop)));
                        // We also like to watch our data model to be able to capture changes coming from other places.
                        if (forSettings === true) {
                            blockObject.__watchers.push(isolatedScope.$watch('blockObjects._' + blockObject.key + '.' + 'settingsData' + '.' + prop.alias, createLayoutSettingsModelWatcher(blockObject, prop)));
                        } else {
                            blockObject.__watchers.push(isolatedScope.$watch('blockObjects._' + blockObject.key + '.' + 'data' + '.' + prop.alias, createDataModelWatcher(blockObject, prop)));
                        }
                    }
                }
                if (blockObject.__watchers.length === 0) {
                    // If no watcher where created, it means we have no properties to watch. This means that nothing will activate our generate the label, since its only triggered by watchers.
                    blockObject.updateLabel();
                }
            }
            /**
     * Used to create a prop watcher for the data in the property editor data model.
     */
            function createDataModelWatcher(blockObject, prop) {
                return function () {
                    if (prop.value !== blockObject.data[prop.alias]) {
                        // sync data:
                        prop.value = blockObject.data[prop.alias];
                        blockObject.updateLabel();
                    }
                };
            }
            /**
     * Used to create a prop watcher for the settings in the property editor data model.
     */
            function createLayoutSettingsModelWatcher(blockObject, prop) {
                return function () {
                    if (prop.value !== blockObject.settingsData[prop.alias]) {
                        // sync data:
                        prop.value = blockObject.settingsData[prop.alias];
                    }
                };
            }
            /**
     * Used to create a scoped watcher for a content property on a blockObject.
     */
            function createContentModelPropWatcher(blockObject, prop) {
                return function () {
                    if (blockObject.data[prop.alias] !== prop.value) {
                        // sync data:
                        blockObject.data[prop.alias] = prop.value;
                    }
                    blockObject.updateLabel();
                };
            }
            /**
     * Used to create a scoped watcher for a settings property on a blockObject.
     */
            function createSettingsModelPropWatcher(blockObject, prop) {
                return function () {
                    if (blockObject.settingsData[prop.alias] !== prop.value) {
                        // sync data:
                        blockObject.settingsData[prop.alias] = prop.value;
                    }
                };
            }
            function createDataEntry(elementTypeKey, dataItems) {
                var data = {
                    contentTypeKey: elementTypeKey,
                    udi: udiService.create('element')
                };
                dataItems.push(data);
                return data.udi;
            }
            function getDataByUdi(udi, dataItems) {
                return dataItems.find(function (entry) {
                    return entry.udi === udi;
                }) || null;
            }
            /**
     * Set the udi and key property for the content item
     * @param {any} contentData
     * @param {any} udi
     */
            function ensureUdiAndKey(contentData, udi) {
                contentData.udi = udi;
                // Change the content.key to the GUID part of the udi, else it's just random which we don't want, it must be consistent
                contentData.key = udiService.getKey(udi);
            }
            /**
     * Used to highlight unsupported properties for the user, changes unsupported properties into a unsupported-property.
     */
            var notSupportedProperties = [
                'Umbraco.Tags',
                'Umbraco.UploadField',
                'Umbraco.ImageCropper',
                'Umbraco.NestedContent'
            ];
            /**
     * Formats the content apps and ensures unsupported property's have the notsupported view
     * @param {any} scaffold
     */
            function formatScaffoldData(scaffold) {
                // deal with not supported props
                scaffold.variants.forEach(function (variant) {
                    variant.tabs.forEach(function (tab) {
                        tab.properties.forEach(function (property) {
                            if (notSupportedProperties.indexOf(property.editor) !== -1) {
                                property.view = 'notsupported';
                            }
                        });
                    });
                });
                // could be empty in tests
                if (!scaffold.apps) {
                    console.warn('No content apps found in scaffold');
                    return scaffold;
                }
                // replace view of content app
                var contentApp = scaffold.apps.find(function (entry) {
                    return entry.alias === 'umbContent';
                });
                if (contentApp) {
                    contentApp.view = 'views/common/infiniteeditors/blockeditor/blockeditor.content.html';
                }
                // remove info app
                var infoAppIndex = scaffold.apps.findIndex(function (entry) {
                    return entry.alias === 'umbInfo';
                });
                if (infoAppIndex >= 0) {
                    scaffold.apps.splice(infoAppIndex, 1);
                }
                return scaffold;
            }
            /**
     * Creates a settings content app, we only want to do this if settings is present on the specific block.
     * @param {any} contentModel
     */
            function appendSettingsContentApp(contentModel, settingsName) {
                if (!contentModel.apps) {
                    return;
                }
                // add the settings app
                var settingsTab = {
                    'name': settingsName,
                    'alias': 'settings',
                    'icon': 'icon-settings',
                    'view': 'views/common/infiniteeditors/blockeditor/blockeditor.settings.html',
                    'hasError': false
                };
                contentModel.apps.push(settingsTab);
            }
            /**
     * @ngdoc method
     * @name constructor
     * @methodOf umbraco.services.blockEditorModelObject
     * @description Constructor of the model object used to handle Block Editor data.
     * @param {object} propertyModelValue data object of the property editor, usually model.value.
     * @param {string} propertyEditorAlias alias of the property.
     * @param {object} blockConfigurations block configurations.
     * @param {angular-scope} scopeOfExistance A local angularJS scope that exists as long as the data exists.
     * @param {angular-scope} propertyEditorScope A local angularJS scope that represents the property editors scope.
     * @returns {BlockEditorModelObject} A instance of BlockEditorModelObject.
     */
            function BlockEditorModelObject(propertyModelValue, propertyEditorAlias, blockConfigurations, scopeOfExistance, propertyEditorScope) {
                if (!propertyModelValue) {
                    throw new Error('propertyModelValue cannot be undefined, to ensure we keep the binding to the angular model we need minimum an empty object.');
                }
                this.__watchers = [];
                this.__labels = {};
                // ensure basic part of data-structure is in place:
                this.value = propertyModelValue;
                this.value.layout = this.value.layout || {};
                this.value.contentData = this.value.contentData || [];
                this.value.settingsData = this.value.settingsData || [];
                this.propertyEditorAlias = propertyEditorAlias;
                this.blockConfigurations = blockConfigurations;
                this.blockConfigurations.forEach(function (blockConfiguration) {
                    if (blockConfiguration.view != null && blockConfiguration.view !== '') {
                        blockConfiguration.view = umbRequestHelper.convertVirtualToAbsolutePath(blockConfiguration.view);
                    }
                    if (blockConfiguration.stylesheet != null && blockConfiguration.stylesheet !== '') {
                        blockConfiguration.stylesheet = umbRequestHelper.convertVirtualToAbsolutePath(blockConfiguration.stylesheet);
                    }
                    if (blockConfiguration.thumbnail != null && blockConfiguration.thumbnail !== '') {
                        blockConfiguration.thumbnail = umbRequestHelper.convertVirtualToAbsolutePath(blockConfiguration.thumbnail);
                    }
                });
                this.scaffolds = [];
                this.isolatedScope = scopeOfExistance.$new(true);
                this.isolatedScope.blockObjects = {};
                this.__watchers.push(this.isolatedScope.$on('$destroy', this.destroy.bind(this)));
                this.__watchers.push(propertyEditorScope.$on('formSubmittingFinalPhase', this.sync.bind(this)));
            }
            ;
            BlockEditorModelObject.prototype = {
                update: function update(propertyModelValue, propertyEditorScope) {
                    // clear watchers
                    this.__watchers.forEach(function (w) {
                        w();
                    });
                    delete this.__watchers;
                    // clear block objects
                    for (var key in this.isolatedScope.blockObjects) {
                        this.destroyBlockObject(this.isolatedScope.blockObjects[key]);
                    }
                    this.isolatedScope.blockObjects = {};
                    // update our values
                    this.value = propertyModelValue;
                    this.value.layout = this.value.layout || {};
                    this.value.contentData = this.value.contentData || [];
                    this.value.settingsData = this.value.settingsData || [];
                    // re-create the watchers
                    this.__watchers = [];
                    this.__watchers.push(this.isolatedScope.$on('$destroy', this.destroy.bind(this)));
                    this.__watchers.push(propertyEditorScope.$on('formSubmittingFinalPhase', this.sync.bind(this)));
                },
                /**
       * @ngdoc method
       * @name getBlockConfiguration
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Get block configuration object for a given contentElementTypeKey.
       * @param {string} key contentElementTypeKey to recive the configuration model for.
       * @returns {Object | null} Configuration model for the that specific block. Or ´null´ if the contentElementTypeKey isnt available in the current block configurations.
       */
                getBlockConfiguration: function getBlockConfiguration(key) {
                    return this.blockConfigurations.find(function (bc) {
                        return bc.contentElementTypeKey === key;
                    }) || null;
                },
                /**
       * @ngdoc method
       * @name load
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Load the scaffolding models for the given configuration, these are needed to provide useful models for each block.
       * @param {Object} blockObject BlockObject to receive data values from.
       * @returns {Promise} A Promise object which resolves when all scaffold models are loaded.
       */
                load: function load() {
                    var self = this;
                    var tasks = [];
                    tasks.push(localizationService.localize('blockEditor_tabBlockSettings').then(function (settingsName) {
                        // self.__labels might not exists anymore, this happens if this instance has been destroyed before the load is complete.
                        if (self.__labels) {
                            self.__labels.settingsName = settingsName;
                        }
                    }));
                    var scaffoldKeys = [];
                    this.blockConfigurations.forEach(function (blockConfiguration) {
                        scaffoldKeys.push(blockConfiguration.contentElementTypeKey);
                        if (blockConfiguration.settingsElementTypeKey != null) {
                            scaffoldKeys.push(blockConfiguration.settingsElementTypeKey);
                        }
                    });
                    // removing duplicates.
                    scaffoldKeys = scaffoldKeys.filter(function (value, index, self) {
                        return self.indexOf(value) === index;
                    });
                    scaffoldKeys.forEach(function (contentTypeKey) {
                        tasks.push(contentResource.getScaffoldByKey(-20, contentTypeKey).then(function (scaffold) {
                            // self.scaffolds might not exists anymore, this happens if this instance has been destroyed before the load is complete.
                            if (self.scaffolds) {
                                self.scaffolds.push(formatScaffoldData(scaffold));
                            }
                        }).catch(function () {
                        }));
                    });
                    return $q.all(tasks);
                },
                /**
       * @ngdoc method
       * @name getAvailableAliasesForBlockContent
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Retrive a list of aliases that are available for content of blocks in this property editor, does not contain aliases of block settings.
       * @return {Array} array of strings representing alias.
       */
                getAvailableAliasesForBlockContent: function getAvailableAliasesForBlockContent() {
                    var _this = this;
                    return this.blockConfigurations.map(function (blockConfiguration) {
                        var scaffold = _this.getScaffoldFromKey(blockConfiguration.contentElementTypeKey);
                        if (scaffold) {
                            return scaffold.contentTypeAlias;
                        }
                    });
                },
                /**
       * @ngdoc method
       * @name getAvailableBlocksForBlockPicker
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Retrive a list of available blocks, the list containing object with the confirugation model(blockConfigModel) and the element type model(elementTypeModel).
       * The purpose of this data is to provide it for the Block Picker.
       * @return {Array} array of objects representing available blocks, each object containing properties blockConfigModel and elementTypeModel.
       */
                getAvailableBlocksForBlockPicker: function getAvailableBlocksForBlockPicker() {
                    var _this2 = this;
                    var blocks = [];
                    this.blockConfigurations.forEach(function (blockConfiguration) {
                        var scaffold = _this2.getScaffoldFromKey(blockConfiguration.contentElementTypeKey);
                        if (scaffold) {
                            blocks.push({
                                blockConfigModel: blockConfiguration,
                                elementTypeModel: scaffold.documentType
                            });
                        }
                    });
                    return blocks;
                },
                /**
       * @ngdoc method
       * @name getScaffoldFromKey
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Get scaffold model for a given contentTypeKey.
       * @param {string} key contentTypeKey to recive the scaffold model for.
       * @returns {Object | null} Scaffold model for the that content type. Or null if the scaffolding model dosnt exist in this context.
       */
                getScaffoldFromKey: function getScaffoldFromKey(contentTypeKey) {
                    return this.scaffolds.find(function (o) {
                        return o.contentTypeKey === contentTypeKey;
                    });
                },
                /**
       * @ngdoc method
       * @name getScaffoldFromAlias
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Get scaffold model for a given contentTypeAlias, used by clipboardService.
       * @param {string} alias contentTypeAlias to recive the scaffold model for.
       * @returns {Object | null} Scaffold model for the that content type. Or null if the scaffolding model dosnt exist in this context.
       */
                getScaffoldFromAlias: function getScaffoldFromAlias(contentTypeAlias) {
                    return this.scaffolds.find(function (o) {
                        return o.contentTypeAlias === contentTypeAlias;
                    });
                },
                /**
       * @ngdoc method
       * @name getBlockObject
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Retrieve a Block Object for the given layout entry.
       * The Block Object offers the necessary data to display and edit a block.
       * The Block Object setups live syncronization of content and settings models back to the data of your Property Editor model.
       * The returned object, named ´BlockObject´, contains several usefull models to make editing of this block happen.
       * The ´BlockObject´ contains the following properties:
       * - key {string}: runtime generated key, usefull for tracking of this object
       * - content {Object}: Content model, the content data in a ElementType model.
       * - settings {Object}: Settings model, the settings data in a ElementType model.
       * - config {Object}: A local deep copy of the block configuration model.
       * - label {string}: The label for this block.
       * - updateLabel {Method}: Method to trigger an update of the label for this block.
       * - data {Object}: A reference to the content data object from your property editor model.
       * - settingsData {Object}: A reference to the settings data object from your property editor model.
       * - layout {Object}: A refernce to the layout entry from your property editor model.
       * @param {Object} layoutEntry the layout entry object to build the block model from.
       * @return {Object | null} The BlockObject for the given layout entry. Or null if data or configuration wasnt found for this block.
       */
                getBlockObject: function getBlockObject(layoutEntry) {
                    var contentUdi = layoutEntry.contentUdi;
                    var dataModel = getDataByUdi(contentUdi, this.value.contentData);
                    if (dataModel === null) {
                        console.error('Couldn\'t find content data of ' + contentUdi);
                        return null;
                    }
                    var blockConfiguration = this.getBlockConfiguration(dataModel.contentTypeKey);
                    var contentScaffold = null;
                    if (blockConfiguration === null) {
                        console.warn('The block of ' + contentUdi + ' is not being initialized because its contentTypeKey(\'' + dataModel.contentTypeKey + '\') is not allowed for this PropertyEditor');
                    } else {
                        contentScaffold = this.getScaffoldFromKey(blockConfiguration.contentElementTypeKey);
                        if (contentScaffold === null) {
                            console.error('The block of ' + contentUdi + ' is not begin initialized cause its Element Type was not loaded.');
                        }
                    }
                    if (blockConfiguration === null || contentScaffold === null) {
                        blockConfiguration = {
                            label: 'Unsupported',
                            unsupported: true
                        };
                    }
                    var blockObject = {};
                    // Set an angularJS cloneNode method, to avoid this object begin cloned.
                    blockObject.cloneNode = function () {
                        return null;    // angularJS accept this as a cloned value as long as the
                    };
                    blockObject.key = String.CreateGuid().replace(/-/g, '');
                    blockObject.config = Utilities.copy(blockConfiguration);
                    if (blockObject.config.label && blockObject.config.label !== '') {
                        blockObject.labelInterpolator = $interpolate(blockObject.config.label);
                    }
                    blockObject.__scope = this.isolatedScope;
                    blockObject.updateLabel = _.debounce(function () {
                        // Check wether scope still exists, maybe object was destoyed in these seconds.
                        if (this.__scope) {
                            this.label = getBlockLabel(this);
                            this.__scope.$evalAsync();
                        }
                    }.bind(blockObject), 10);
                    // make basics from scaffold
                    if (contentScaffold !== null) {
                        // We might not have contentScaffold
                        blockObject.content = Utilities.copy(contentScaffold);
                        ensureUdiAndKey(blockObject.content, contentUdi);
                        mapToElementModel(blockObject.content, dataModel);
                    } else {
                        blockObject.content = null;
                    }
                    blockObject.data = dataModel;
                    blockObject.layout = layoutEntry;
                    blockObject.__watchers = [];
                    if (blockConfiguration.settingsElementTypeKey) {
                        var settingsScaffold = this.getScaffoldFromKey(blockConfiguration.settingsElementTypeKey);
                        if (settingsScaffold !== null) {
                            if (!layoutEntry.settingsUdi) {
                                // if this block does not have settings data, then create it. This could happen because settings model has been added later than this content was created.
                                layoutEntry.settingsUdi = createDataEntry(blockConfiguration.settingsElementTypeKey, this.value.settingsData);
                            }
                            var settingsUdi = layoutEntry.settingsUdi;
                            var settingsData = getDataByUdi(settingsUdi, this.value.settingsData);
                            if (settingsData === null) {
                                console.error('Couldnt find settings data of ' + settingsUdi);
                                return null;
                            }
                            blockObject.settingsData = settingsData;
                            // make basics from scaffold
                            blockObject.settings = Utilities.copy(settingsScaffold);
                            ensureUdiAndKey(blockObject.settings, settingsUdi);
                            mapToElementModel(blockObject.settings, settingsData);
                            // add settings content-app
                            appendSettingsContentApp(blockObject.content, this.__labels.settingsName);
                        }
                    }
                    blockObject.retrieveValuesFrom = function (content, settings) {
                        if (this.content !== null) {
                            mapElementValues(content, this.content);
                        }
                        if (this.config.settingsElementTypeKey !== null) {
                            mapElementValues(settings, this.settings);
                        }
                    };
                    blockObject.sync = function () {
                        if (this.content !== null) {
                            mapToPropertyModel(this.content, this.data);
                        }
                        if (this.config.settingsElementTypeKey !== null) {
                            mapToPropertyModel(this.settings, this.settingsData);
                        }
                    };
                    // first time instant update of label.
                    blockObject.label = getBlockLabel(blockObject);
                    // Add blockObject to our isolated scope to enable watching its values:
                    this.isolatedScope.blockObjects['_' + blockObject.key] = blockObject;
                    addWatchers(blockObject, this.isolatedScope);
                    addWatchers(blockObject, this.isolatedScope, true);
                    blockObject.destroy = function () {
                        // remove property value watchers:
                        this.__watchers.forEach(function (w) {
                            w();
                        });
                        delete this.__watchers;
                        // help carbage collector:
                        delete this.config;
                        delete this.layout;
                        delete this.data;
                        delete this.settingsData;
                        delete this.content;
                        delete this.settings;
                        // remove model from isolatedScope.
                        delete this.__scope.blockObjects['_' + this.key];
                        // NOTE: It seems like we should call this.__scope.$destroy(); since that is the only way to remove a scope from it's parent,
                        // however that is not the case since __scope is actually this.isolatedScope which gets cleaned up when the outer scope is
                        // destroyed. If we do that here it breaks the scope chain and validation.
                        delete this.__scope;
                        // removes this method, making it impossible to destroy again.
                        delete this.destroy;
                        // lets remove the key to make things blow up if this is still referenced:
                        delete this.key;
                    };
                    return blockObject;
                },
                /**
       * @ngdoc method
       * @name removeDataAndDestroyModel
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Removes the data and destroys the Block Model.
       * Notice this method does not remove the block from your layout, this will need to be handlede by the Property Editor since this services donst know about your layout structure.
       * @param {Object} blockObject The BlockObject to be removed and destroyed.
       */
                removeDataAndDestroyModel: function removeDataAndDestroyModel(blockObject) {
                    var udi = blockObject.layout.contentUdi;
                    var settingsUdi = blockObject.layout.settingsUdi || null;
                    this.destroyBlockObject(blockObject);
                    this.removeDataByUdi(udi);
                    if (settingsUdi) {
                        this.removeSettingsByUdi(settingsUdi);
                    }
                },
                /**
       * @ngdoc method
       * @name destroyBlockObject
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Destroys the Block Model, but all data is kept.
       * @param {Object} blockObject The BlockObject to be destroyed.
       */
                destroyBlockObject: function destroyBlockObject(blockObject) {
                    blockObject.destroy();
                },
                /**
       * @ngdoc method
       * @name getLayout
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Retrieve the layout object from this specific property editor model.
       * @param {object} defaultStructure if no data exist the layout of your poerty editor will be set to this object.
       * @return {Object} Layout object, structure depends on the model of your property editor.
       */
                getLayout: function getLayout(defaultStructure) {
                    if (!this.value.layout[this.propertyEditorAlias]) {
                        this.value.layout[this.propertyEditorAlias] = defaultStructure;
                    }
                    return this.value.layout[this.propertyEditorAlias];
                },
                /**
       * @ngdoc method
       * @name create
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Create a empty layout entry, notice the layout entry is not added to the property editors model layout object, since the layout sturcture depends on the property editor.
       * @param {string} contentElementTypeKey the contentElementTypeKey of the block you wish to create, if contentElementTypeKey is not avaiable in the block configuration then ´null´ will be returned.
       * @return {Object | null} Layout entry object, to be inserted at a decired location in the layout object. Or null if contentElementTypeKey is unavaiaible.
       */
                create: function create(contentElementTypeKey) {
                    var blockConfiguration = this.getBlockConfiguration(contentElementTypeKey);
                    if (blockConfiguration === null) {
                        return null;
                    }
                    var entry = { contentUdi: createDataEntry(contentElementTypeKey, this.value.contentData) };
                    if (blockConfiguration.settingsElementTypeKey != null) {
                        entry.settingsUdi = createDataEntry(blockConfiguration.settingsElementTypeKey, this.value.settingsData);
                    }
                    return entry;
                },
                /**
       * @ngdoc method
       * @name createFromElementType
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Insert data from ElementType Model
       * @return {Object | null} Layout entry object, to be inserted at a decired location in the layout object. Or ´null´ if the given ElementType isnt supported by the block configuration.
       */
                createFromElementType: function createFromElementType(elementTypeDataModel) {
                    elementTypeDataModel = clipboardService.parseContentForPaste(elementTypeDataModel, clipboardService.TYPES.ELEMENT_TYPE);
                    var contentElementTypeKey = elementTypeDataModel.contentTypeKey;
                    var layoutEntry = this.create(contentElementTypeKey);
                    if (layoutEntry === null) {
                        return null;
                    }
                    var dataModel = getDataByUdi(layoutEntry.contentUdi, this.value.contentData);
                    if (dataModel === null) {
                        return null;
                    }
                    mapToPropertyModel(elementTypeDataModel, dataModel);
                    return layoutEntry;
                },
                /**
       * @ngdoc method
       * @name createFromBlockData
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Insert data from raw models
       * @return {Object | null} Layout entry object, to be inserted at a decired location in the layout object. Or ´null´ if the given ElementType isnt supported by the block configuration.
       */
                createFromBlockData: function createFromBlockData(blockData) {
                    blockData = clipboardService.parseContentForPaste(blockData, clipboardService.TYPES.BLOCK);
                    // As the blockData is a cloned object we can use its layout part for our layout entry.
                    var layoutEntry = blockData.layout;
                    if (layoutEntry === null) {
                        return null;
                    }
                    var blockConfiguration;
                    if (blockData.data) {
                        // Ensure that we support the alias:
                        blockConfiguration = this.getBlockConfiguration(blockData.data.contentTypeKey);
                        if (blockConfiguration === null) {
                            return null;
                        }
                        this.value.contentData.push(blockData.data);
                    } else {
                        // We do not have data, this cannot be succesful paste.
                        return null;
                    }
                    if (blockData.settingsData) {
                        // Ensure that we support the alias:
                        if (blockConfiguration.settingsElementTypeKey) {
                            // If we have settings for this Block Configuration, we need to check that they align, if we dont we do not want to fail.
                            if (blockConfiguration.settingsElementTypeKey === blockData.settingsData.contentTypeKey) {
                                this.value.settingsData.push(blockData.settingsData);
                            } else {
                                notificationsService.error('Clipboard', 'Couldn\'t paste because settings-data is not compatible.');
                                return null;
                            }
                        } else {
                            // We do not have settings currently, so lets get rid of the settings part and move on with the paste.
                            delete layoutEntry.settingUdi;
                        }
                    }
                    return layoutEntry;
                },
                /**
       * @ngdoc method
       * @name sync
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Force immidiate update of the blockobject models to the property model.
       */
                sync: function sync() {
                    for (var key in this.isolatedScope.blockObjects) {
                        this.isolatedScope.blockObjects[key].sync();
                    }
                },
                /**
       * @ngdoc method
       * @name removeDataByUdi
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Removes the content data of a given UDI.
       * Notice this method does not remove the block from your layout, this will need to be handled by the Property Editor since this services don't know about your layout structure.
       * @param {string} udi The UDI of the content data to be removed.
       */
                removeDataByUdi: function removeDataByUdi(udi) {
                    var index = this.value.contentData.findIndex(function (o) {
                        return o.udi === udi;
                    });
                    if (index !== -1) {
                        this.value.contentData.splice(index, 1);
                    }
                },
                /**
       * @ngdoc method
       * @name removeSettingsByUdi
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Removes the settings data of a given UDI.
       * Notice this method does not remove the settingsUdi from your layout, this will need to be handled by the Property Editor since this services don't know about your layout structure.
       * @param {string} udi The UDI of the settings data to be removed.
       */
                removeSettingsByUdi: function removeSettingsByUdi(udi) {
                    var index = this.value.settingsData.findIndex(function (o) {
                        return o.udi === udi;
                    });
                    if (index !== -1) {
                        this.value.settingsData.splice(index, 1);
                    }
                },
                /**
       * @ngdoc method
       * @name destroy
       * @methodOf umbraco.services.blockEditorModelObject
       * @description Notice you should not need to destroy the BlockEditorModelObject since it will automaticly be destroyed when the scope of existance gets destroyed.
       */
                destroy: function destroy() {
                    this.__watchers.forEach(function (w) {
                        w();
                    });
                    for (var key in this.isolatedScope.blockObjects) {
                        this.destroyBlockObject(this.isolatedScope.blockObjects[key]);
                    }
                    delete this.__watchers;
                    delete this.value;
                    delete this.propertyEditorAlias;
                    delete this.blockConfigurations;
                    delete this.scaffolds;
                    this.isolatedScope.$destroy();
                    delete this.isolatedScope;
                    delete this.destroy;
                }
            };
            return BlockEditorModelObject;
        }
        angular.module('umbraco.services').service('blockEditorModelObject', blockEditorModelObjectFactory);
    }());
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.clipboardService
 *
 * @requires notificationsService
 * @requires eventsService
 *
 * @description
 * Service to handle clipboard in general across the application. Responsible for handling the data both storing and retrive.
 * The service has a set way for defining a data-set by a entryType and alias, which later will be used to retrive the posible entries for a paste scenario.
 *
 */
    function clipboardService($window, notificationsService, eventsService, localStorageService, iconHelper) {
        var TYPES = {};
        TYPES.ELEMENT_TYPE = 'elementType';
        TYPES.BLOCK = 'block';
        TYPES.IMAGE = 'image';
        TYPES.RAW = 'raw';
        var clearPropertyResolvers = {};
        var pastePropertyResolvers = {};
        var clipboardTypeResolvers = {};
        clipboardTypeResolvers[TYPES.ELEMENT_TYPE] = function (element, propMethod) {
            for (var t = 0; t < element.variants[0].tabs.length; t++) {
                var tab = element.variants[0].tabs[t];
                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];
                    propMethod(prop, TYPES.ELEMENT_TYPE);
                }
            }
        };
        clipboardTypeResolvers[TYPES.BLOCK] = function (block, propMethod) {
            propMethod(block, TYPES.BLOCK);
            if (block.data) {
                Object.keys(block.data).forEach(function (key) {
                    if (key === 'udi' || key === 'contentTypeKey') {
                        return;
                    }
                    propMethod(block.data[key], TYPES.RAW);
                });
            }
            if (block.settingsData) {
                Object.keys(block.settingsData).forEach(function (key) {
                    if (key === 'udi' || key === 'contentTypeKey') {
                        return;
                    }
                    propMethod(block.settingsData[key], TYPES.RAW);
                });
            }    /*
    // Concept for supporting Block that contains other Blocks.
    // Missing clarifications:
    // How do we ensure that the inner blocks of a block is supported in the new scenario. Not that likely but still relevant, so considerations should be made.
    if(block.references) {
        // A Block clipboard entry can contain other Block Clipboard Entries, here we will make sure to resolve those identical to the main entry.
        for (var r = 0; r < block.references.length; r++) {
            clipboardTypeResolvers[TYPES.BLOCK](block.references[r], propMethod);
        }
    }
    */
        };
        clipboardTypeResolvers[TYPES.RAW] = function (data, propMethod) {
            for (var p = 0; p < data.length; p++) {
                propMethod(data[p], TYPES.RAW);
            }
        };
        var STORAGE_KEY = 'umbClipboardService';
        var retriveStorage = function retriveStorage() {
            if (localStorageService.isSupported === false) {
                return null;
            }
            var dataJSON;
            var dataString = localStorageService.get(STORAGE_KEY);
            if (dataString != null) {
                dataJSON = JSON.parse(dataString);
            }
            if (dataJSON == null) {
                dataJSON = new Object();
            }
            if (dataJSON.entries === undefined) {
                dataJSON.entries = [];
            }
            return dataJSON;
        };
        var saveStorage = function saveStorage(storage) {
            var storageString = JSON.stringify(storage);
            try {
                // Check that we can parse the JSON:
                var storageJSON = JSON.parse(storageString);
                // Store the string:
                localStorageService.set(STORAGE_KEY, storageString);
                eventsService.emit('clipboardService.storageUpdate');
                return true;
            } catch (e) {
                return false;
            }
            return false;
        };
        function resolvePropertyForStorage(prop, type) {
            type = type || 'raw';
            var resolvers = clearPropertyResolvers[type];
            if (resolvers) {
                for (var i = 0; i < resolvers.length; i++) {
                    resolvers[i](prop, resolvePropertyForStorage);
                }
            }
        }
        var prepareEntryForStorage = function prepareEntryForStorage(type, entryData, firstLevelClearupMethod) {
            var cloneData = Utilities.copy(entryData);
            if (firstLevelClearupMethod != undefined) {
                firstLevelClearupMethod(cloneData);
            }
            var typeResolver = clipboardTypeResolvers[type];
            if (typeResolver) {
                typeResolver(cloneData, resolvePropertyForStorage);
            } else {
                console.warn('Umbraco.service.clipboardService has no type resolver for \'' + type + '\'.');
            }
            return cloneData;
        };
        var isEntryCompatible = function isEntryCompatible(entry, type, allowedAliases) {
            return entry.type === type && (entry.alias && allowedAliases.filter(function (allowedAlias) {
                return allowedAlias === entry.alias;
            }).length > 0 || entry.aliases && entry.aliases.filter(function (entryAlias) {
                return allowedAliases.filter(function (allowedAlias) {
                    return allowedAlias === entryAlias;
                }).length > 0;
            }).length === entry.aliases.length);
        };
        function resolvePropertyForPaste(prop, type) {
            type = type || 'raw';
            var resolvers = pastePropertyResolvers[type];
            if (resolvers) {
                for (var i = 0; i < resolvers.length; i++) {
                    resolvers[i](prop, resolvePropertyForPaste);
                }
            }
        }
        var service = {};
        /**
   * Default types to store in clipboard.
   */
        service.TYPES = TYPES;
        /**
  * @ngdoc method
  * @name umbraco.services.clipboardService#parseContentForPaste
  * @methodOf umbraco.services.clipboardService
  *
  * @param {object} function The content entry to be cloned and resolved.
  *
  * @description
  * Executed registered property resolvers for inner properties, to be done on pasting a clipbaord content entry.
  *
  */
        service.parseContentForPaste = function (pasteEntryData, type) {
            var cloneData = Utilities.copy(pasteEntryData);
            var typeResolver = clipboardTypeResolvers[type];
            if (typeResolver) {
                typeResolver(cloneData, resolvePropertyForPaste);
            } else {
                console.warn('Umbraco.service.clipboardService has no type resolver for \'' + type + '\'.');
            }
            return cloneData;
        };
        /**
  * @ngdoc method
  * @name umbraco.services.clipboardService#registrerClearPropertyResolver
  * @methodOf umbraco.services.clipboardService
  *
  * @param {string} function A method executed for every property and inner properties copied.
  * @param {string} string A string representing the property type format for this resolver to execute for.
  *
  * @description
  * Executed for all properties of given type when performing a copy action.
  *
  * @deprecated Incorrect spelling please use 'registerClearPropertyResolver'
  */
        service.registrerClearPropertyResolver = function (resolver, type) {
            this.registerClearPropertyResolver(resolver, type);
        };
        /**
  * @ngdoc method
  * @name umbraco.services.clipboardService#registerClearPropertyResolver
  * @methodOf umbraco.services.clipboardService
  *
  * @param {method} function A method executed for every property and inner properties copied. Notice the method both needs to deal with retriving a property object {alias:..editor:..value:... ,...} and the raw property value as if the property is an inner property of a nested property.
  * @param {string} string A string representing the property type format for this resolver to execute for.
  *
  * @description
  * Executed for all properties of given type when performing a copy action.
  */
        service.registerClearPropertyResolver = function (resolver, type) {
            type = type || 'raw';
            if (!clearPropertyResolvers[type]) {
                clearPropertyResolvers[type] = [];
            }
            clearPropertyResolvers[type].push(resolver);
        };
        /**
  * @ngdoc method
  * @name umbraco.services.clipboardService#registerPastePropertyResolver
  * @methodOf umbraco.services.clipboardService
  *
  * @param {method} function A method executed for every property and inner properties copied. Notice the method both needs to deal with retriving a property object {alias:..editor:..value:... ,...} and the raw property value as if the property is an inner property of a nested property.
  * @param {string} string A string representing the property type format for this resolver to execute for.
  *
  * @description
  * Executed for all properties of given type when performing a paste action.
  */
        service.registerPastePropertyResolver = function (resolver, type) {
            type = type || 'raw';
            if (!pastePropertyResolvers[type]) {
                pastePropertyResolvers[type] = [];
            }
            pastePropertyResolvers[type].push(resolver);
        };
        /**
  * @ngdoc method
  * @name umbraco.services.clipboardService#registrerTypeResolvers
  * @methodOf umbraco.services.clipboardService
  *
  * @param {method} function A method for to execute resolvers for each properties of the specific type.
  * @param {string} string A string representing the content type format for this resolver to execute for.
  *
  * @description
  * Executed for all properties including inner properties when performing a paste action.
  */
        service.registrerTypeResolvers = function (resolver, type) {
            if (!clipboardTypeResolvers[type]) {
                clipboardTypeResolvers[type] = [];
            }
            clipboardTypeResolvers[type].push(resolver);
        };
        /**
  * @ngdoc method
  * @name umbraco.services.clipboardService#copy
  * @methodOf umbraco.services.clipboardService
  *
  * @param {string} type A string defining the type of data to storing, example: 'elementType', 'contentNode'
  * @param {string} alias A string defining the alias of the data to store, example: 'product'
  * @param {object} data A object containing the properties to be saved, this could be the object of a ElementType, ContentNode, ...
  * @param {string} displayLabel (optional) A string swetting the label to display when showing paste entries.
  * @param {string} displayIcon (optional) A string setting the icon to display when showing paste entries.
  * @param {string} uniqueKey (optional) A string prodiving an identifier for this entry, existing entries with this key will be removed to ensure that you only have the latest copy of this data.
  *
  * @description
  * Saves a single JS-object with a type and alias to the clipboard.
  */
        service.copy = function (type, alias, data, displayLabel, displayIcon, uniqueKey, firstLevelClearupMethod) {
            var storage = retriveStorage();
            displayLabel = displayLabel || data.name;
            displayIcon = displayIcon || iconHelper.convertFromLegacyIcon(data.icon);
            uniqueKey = uniqueKey || data.key || console.error('missing unique key for this content');
            // remove previous copies of this entry:
            storage.entries = storage.entries.filter(function (entry) {
                return entry.unique !== uniqueKey;
            });
            var entry = {
                unique: uniqueKey,
                type: type,
                alias: alias,
                data: prepareEntryForStorage(type, data, firstLevelClearupMethod),
                label: displayLabel,
                icon: displayIcon,
                date: Date.now()
            };
            storage.entries.push(entry);
            if (saveStorage(storage) === true) {
                notificationsService.success('Clipboard', 'Copied to clipboard.');
            } else {
                notificationsService.error('Clipboard', 'Couldnt copy this data to clipboard.');
            }
        };
        /**
  * @ngdoc method
  * @name umbraco.services.clipboardService#copy
  * @methodOf umbraco.services.clipboardService
  *
  * @param {string} type A string defining the type of data to storing, example: 'elementType', 'contentNode'
  * @param {string} alias A string defining the alias of the data to store, example: 'product'
  * @param {object[]} data An array of objects containing the properties to be saved, this could be the object of a ElementType, ContentNode, ...
  *
  * @description
  * Saves multiple JS-object to the clipboard.
  */
        service.copyMultiple = function (type, alias, data, firstLevelClearupMethod) {
            var storage = retriveStorage();
            data.forEach(function (item) {
                var displayLabel = item.displayLabel || item.name;
                var displayIcon = item.displayIcon || iconHelper.convertFromLegacyIcon(item.icon);
                var uniqueKey = item.uniqueKey || item.key || console.error('missing unique key for this content');
                // remove previous copies of this entry:
                storage.entries = storage.entries.filter(function (entry) {
                    return entry.unique !== uniqueKey;
                });
                var entry = {
                    unique: uniqueKey,
                    type: type,
                    alias: alias,
                    data: prepareEntryForStorage(type, item, firstLevelClearupMethod),
                    label: displayLabel,
                    icon: displayIcon,
                    date: Date.now()
                };
                storage.entries.push(entry);
            });
            if (saveStorage(storage) === true) {
                notificationsService.success('Clipboard', 'Copied to clipboard.');
            } else {
                notificationsService.error('Clipboard', 'Couldnt copy this data to clipboard.');
            }
        };
        /**
  * @ngdoc method
  * @name umbraco.services.clipboardService#copyArray
  * @methodOf umbraco.services.clipboardService
  *
  * @param {string} type A string defining the type of data to storing, example: 'elementTypeArray', 'contentNodeArray'
  * @param {string} aliases An array of strings defining the alias of the data to store, example: ['banana', 'apple']
  * @param {object} datas An array of objects containing the properties to be saved, example: [ElementType, ElementType, ...]
  * @param {string} displayLabel A string setting the label to display when showing paste entries.
  * @param {string} displayIcon A string setting the icon to display when showing paste entries.
  * @param {string} uniqueKey A string prodiving an identifier for this entry, existing entries with this key will be removed to ensure that you only have the latest copy of this data.
  * @param {string} firstLevelClearupMethod A string prodiving an identifier for this entry, existing entries with this key will be removed to ensure that you only have the latest copy of this data.
  *
  * @description
  * Saves a single JS-object with a type and alias to the clipboard.
  */
        service.copyArray = function (type, aliases, datas, displayLabel, displayIcon, uniqueKey, firstLevelClearupMethod) {
            if (type === 'elementTypeArray') {
                type = 'elementType';
            }
            var storage = retriveStorage();
            // Clean up each entry
            var copiedDatas = datas.map(function (data) {
                return prepareEntryForStorage(type, data, firstLevelClearupMethod);
            });
            // remove previous copies of this entry:
            storage.entries = storage.entries.filter(function (entry) {
                return entry.unique !== uniqueKey;
            });
            var entry = {
                unique: uniqueKey,
                type: type,
                aliases: aliases,
                data: copiedDatas,
                label: displayLabel,
                icon: displayIcon,
                date: Date.now()
            };
            storage.entries.push(entry);
            if (saveStorage(storage) === true) {
                notificationsService.success('Clipboard', 'Copied to clipboard.');
            } else {
                notificationsService.error('Clipboard', 'Couldnt copy this data to clipboard.');
            }
        };
        /**
  * @ngdoc method
  * @name umbraco.services.supportsCopy#supported
  * @methodOf umbraco.services.clipboardService
  *
  * @description
  * Determins wether the current browser is able to performe its actions.
  */
        service.isSupported = function () {
            return localStorageService.isSupported;
        };
        /**
  * @ngdoc method
  * @name umbraco.services.supportsCopy#hasEntriesOfType
  * @methodOf umbraco.services.clipboardService
  *
  * @param {string} type A string defining the type of data test for.
  * @param {string} aliases A array of strings providing the alias of the data you want to test for.
  *
  * @description
  * Determines whether the current clipboard has entries that match a given type and one of the aliases.
  */
        service.hasEntriesOfType = function (type, aliases) {
            if (service.retriveEntriesOfType(type, aliases).length > 0) {
                return true;
            }
            return false;
        };
        /**
  * @ngdoc method
  * @name umbraco.services.supportsCopy#retriveEntriesOfType
  * @methodOf umbraco.services.clipboardService
  *
  * @param {string} type A string defining the type of data to recive.
  * @param {string} aliases A array of strings providing the alias of the data you want to recive.
  *
  * @description
  * Returns an array of entries matching the given type and one of the provided aliases.
  */
        service.retriveEntriesOfType = function (type, allowedAliases) {
            var storage = retriveStorage();
            // Find entries that are fulfilling the criteria for this nodeType and nodeTypesAliases.
            var filteretEntries = storage.entries.filter(function (entry) {
                return isEntryCompatible(entry, type, allowedAliases);
            });
            return filteretEntries;
        };
        /**
  * @ngdoc method
  * @name umbraco.services.supportsCopy#retriveEntriesOfType
  * @methodOf umbraco.services.clipboardService
  *
  * @param {string} type A string defining the type of data to recive.
  * @param {string} aliases A array of strings providing the alias of the data you want to recive.
  *
  * @description
  * Returns an array of data of entries matching the given type and one of the provided aliases.
  */
        service.retriveDataOfType = function (type, aliases) {
            return service.retriveEntriesOfType(type, aliases).map(function (x) {
                return x.data;
            });
        };
        /**
  * @ngdoc method
  * @name umbraco.services.supportsCopy#retriveEntriesOfType
  * @methodOf umbraco.services.clipboardService
  *
  * @param {string} type A string defining the type of data to remove.
  * @param {string} aliases A array of strings providing the alias of the data you want to remove.
  *
  * @description
  * Removes entries matching the given type and one of the provided aliases.
  */
        service.clearEntriesOfType = function (type, allowedAliases) {
            var storage = retriveStorage();
            // Find entries that are NOT fulfilling the criteria for this nodeType and nodeTypesAliases.
            var filteretEntries = storage.entries.filter(function (entry) {
                return !isEntryCompatible(entry, type, allowedAliases);
            });
            storage.entries = filteretEntries;
            saveStorage(storage);
        };
        var emitClipboardStorageUpdate = _.debounce(function (e) {
            eventsService.emit('clipboardService.storageUpdate');
        }, 1000);
        // Fires if LocalStorage was changed from another tab than this one.
        $window.addEventListener('storage', emitClipboardStorageUpdate);
        return service;
    }
    angular.module('umbraco.services').factory('clipboardService', clipboardService);
    'use strict';
    /**
* @ngdoc service
* @name umbraco.services.contentAppHelper
* @description A helper service for content app related functions.
**/
    function contentAppHelper() {
        var service = {};
        /**
   * Default known content based apps.
   */
        service.CONTENT_BASED_APPS = [
            'umbContent',
            'umbInfo',
            'umbListView'
        ];
        /**
  * @ngdoc method
  * @name umbraco.services.contentAppHelper#isContentBasedApp
  * @methodOf umbraco.services.contentAppHelper
  *
  * @param {object} app A content app to check
  *
  * @description
  * Determines whether the supplied content app is a known content based app
  *
  */
        service.isContentBasedApp = function (app) {
            return service.CONTENT_BASED_APPS.indexOf(app.alias) !== -1;
        };
        return service;
    }
    angular.module('umbraco.services').factory('contentAppHelper', contentAppHelper);
    'use strict';
    function _toConsumableArray(arr) {
        return _arrayWithoutHoles(arr) || _iterableToArray(arr) || _nonIterableSpread();
    }
    function _nonIterableSpread() {
        throw new TypeError('Invalid attempt to spread non-iterable instance');
    }
    function _iterableToArray(iter) {
        if (Symbol.iterator in Object(iter) || Object.prototype.toString.call(iter) === '[object Arguments]')
            return Array.from(iter);
    }
    function _arrayWithoutHoles(arr) {
        if (Array.isArray(arr)) {
            for (var i = 0, arr2 = new Array(arr.length); i < arr.length; i++) {
                arr2[i] = arr[i];
            }
            return arr2;
        }
    }
    function _slicedToArray(arr, i) {
        return _arrayWithHoles(arr) || _iterableToArrayLimit(arr, i) || _nonIterableRest();
    }
    function _nonIterableRest() {
        throw new TypeError('Invalid attempt to destructure non-iterable instance');
    }
    function _iterableToArrayLimit(arr, i) {
        if (!(Symbol.iterator in Object(arr) || Object.prototype.toString.call(arr) === '[object Arguments]')) {
            return;
        }
        var _arr = [];
        var _n = true;
        var _d = false;
        var _e = undefined;
        try {
            for (var _i = arr[Symbol.iterator](), _s; !(_n = (_s = _i.next()).done); _n = true) {
                _arr.push(_s.value);
                if (i && _arr.length === i)
                    break;
            }
        } catch (err) {
            _d = true;
            _e = err;
        } finally {
            try {
                if (!_n && _i['return'] != null)
                    _i['return']();
            } finally {
                if (_d)
                    throw _e;
            }
        }
        return _arr;
    }
    function _arrayWithHoles(arr) {
        if (Array.isArray(arr))
            return arr;
    }
    /**
* @ngdoc service
* @name umbraco.services.contentEditingHelper
* @description A helper service for most editors, some methods are specific to content/media/member model types but most are used by
* all editors to share logic and reduce the amount of replicated code among editors.
**/
    function contentEditingHelper(fileManager, $q, $location, $routeParams, editorState, notificationsService, navigationService, localizationService, serverValidationManager, formHelper) {
        function isValidIdentifier(id) {
            //empty id <= 0
            if (Utilities.isNumber(id)) {
                if (id === 0) {
                    return false;
                }
                if (id > 0) {
                    return true;
                }
            }
            //empty guid
            if (id === '00000000-0000-0000-0000-000000000000') {
                return false;
            }
            //empty string / alias
            if (id === '') {
                return false;
            }
            return true;
        }
        function showNotificationsForModelsState(ms) {
            for (var _i = 0, _Object$entries = Object.entries(ms); _i < _Object$entries.length; _i++) {
                var _Object$entries$_i = _slicedToArray(_Object$entries[_i], 2), key = _Object$entries$_i[0], value = _Object$entries$_i[1];
                var errorMsg = value[0];
                // if the error message is json it's a complex editor validation response that we need to parse
                if (Utilities.isString(errorMsg) && errorMsg.startsWith('[') || Utilities.isArray(errorMsg)) {
                    // flatten the json structure, create validation paths for each property and add each as a property error
                    var idsToErrors = serverValidationManager.parseComplexEditorError(errorMsg, '');
                    idsToErrors.forEach(function (x) {
                        if (x.modelState) {
                            showNotificationsForModelsState(x.modelState);
                        }
                    });
                } else if (value[0]) {
                    notificationsService.error('Validation', value[0]);
                }
            }
        }
        return {
            //TODO: We need to move some of this to formHelper for saving, too many editors use this method for saving when this entire
            //service should only be used for content/media/members
            /** Used by the content editor and mini content editor to perform saving operations */
            contentEditorPerformSave: function contentEditorPerformSave(args) {
                if (!Utilities.isObject(args)) {
                    throw 'args must be an object';
                }
                if (!args.scope) {
                    throw 'args.scope is not defined';
                }
                if (!args.content) {
                    throw 'args.content is not defined';
                }
                if (!args.saveMethod) {
                    throw 'args.saveMethod is not defined';
                }
                if (args.showNotifications === undefined) {
                    args.showNotifications = true;
                }
                // needed for infinite editing to create new items
                if (args.create === undefined) {
                    if ($routeParams.create) {
                        args.create = true;
                    }
                }
                if (args.softRedirect === undefined) {
                    //when true, the url will change but it won't actually re-route
                    //this is merely here for compatibility, if only the content/media/members used this service we'd prob be ok but tons of editors
                    //use this service unfortunately and probably packages too.
                    args.softRedirect = false;
                }
                var self = this;
                //we will use the default one for content if not specified
                var _rebindCallback = args.rebindCallback === undefined ? self.reBindChangedProperties : args.rebindCallback;
                if (formHelper.submitForm({
                        scope: args.scope,
                        action: args.action
                    })) {
                    return args.saveMethod(args.content, args.create, fileManager.getFiles(), args.showNotifications).then(function (data) {
                        formHelper.resetForm({ scope: args.scope });
                        if (!args.infiniteMode) {
                            self.handleSuccessfulSave({
                                scope: args.scope,
                                savedContent: data,
                                softRedirect: args.softRedirect,
                                rebindCallback: function rebindCallback() {
                                    _rebindCallback.apply(self, [
                                        args.content,
                                        data
                                    ]);
                                }
                            });
                            //update editor state to what is current
                            editorState.set(args.content);
                        }
                        return $q.resolve(data);
                    }, function (err) {
                        formHelper.resetForm({
                            scope: args.scope,
                            hasErrors: true
                        });
                        self.handleSaveError({
                            showNotifications: args.showNotifications,
                            softRedirect: args.softRedirect,
                            err: err,
                            rebindCallback: function rebindCallback() {
                                // if the error contains data, we want to map that back as we want to continue editing this save. Especially important when the content is new as the returned data will contain ID etc.
                                if (err.data) {
                                    _rebindCallback.apply(self, [
                                        args.content,
                                        err.data
                                    ]);
                                }
                            }
                        });
                        //update editor state to what is current
                        editorState.set(args.content);
                        return $q.reject(err);
                    });
                } else {
                    return $q.reject();
                }
            },
            /** Used by the content editor and media editor to add an info tab to the tabs array (normally known as the properties tab) */
            addInfoTab: function addInfoTab(tabs) {
                var infoTab = {
                    'alias': '_umb_infoTab',
                    'id': -1,
                    'label': 'Info',
                    'properties': []
                };
                // first check if tab is already added
                var foundInfoTab = false;
                tabs.forEach(function (tab) {
                    if (tab.id === infoTab.id && tab.alias === infoTab.alias) {
                        foundInfoTab = true;
                    }
                });
                // add info tab if is is not found
                if (!foundInfoTab) {
                    localizationService.localize('general_info').then(function (value) {
                        infoTab.label = value;
                        tabs.push(infoTab);
                    });
                }
            },
            /** Returns the action button definitions based on what permissions the user has.
    The content.allowedActions parameter contains a list of chars, each represents a button by permission so
    here we'll build the buttons according to the chars of the user. */
            configureContentEditorButtons: function configureContentEditorButtons(args) {
                if (!Utilities.isObject(args)) {
                    throw 'args must be an object';
                }
                if (!args.content) {
                    throw 'args.content is not defined';
                }
                if (!args.methods) {
                    throw 'args.methods is not defined';
                }
                if (!args.methods.saveAndPublish || !args.methods.sendToPublish || !args.methods.unpublish || !args.methods.schedulePublish || !args.methods.publishDescendants) {
                    throw 'args.methods does not contain all required defined methods';
                }
                var buttons = {
                    defaultButton: null,
                    subButtons: []
                };
                function createButtonDefinition(ch) {
                    switch (ch) {
                    case 'U':
                        //publish action
                        return {
                            letter: ch,
                            labelKey: 'buttons_saveAndPublish',
                            handler: args.methods.saveAndPublish,
                            hotKey: 'ctrl+p',
                            hotKeyWhenHidden: true,
                            alias: 'saveAndPublish',
                            addEllipsis: args.content.variants && args.content.variants.length > 1 ? 'true' : 'false'
                        };
                    case 'H':
                        //send to publish
                        return {
                            letter: ch,
                            labelKey: 'buttons_saveToPublish',
                            handler: args.methods.sendToPublish,
                            hotKey: 'ctrl+p',
                            hotKeyWhenHidden: true,
                            alias: 'sendToPublish',
                            addEllipsis: args.content.variants && args.content.variants.length > 1 ? 'true' : 'false'
                        };
                    case 'Z':
                        //unpublish
                        return {
                            letter: ch,
                            labelKey: 'content_unpublish',
                            handler: args.methods.unpublish,
                            hotKey: 'ctrl+u',
                            hotKeyWhenHidden: true,
                            alias: 'unpublish',
                            addEllipsis: 'true'
                        };
                    case 'SCHEDULE':
                        //schedule publish - schedule doesn't have a permission letter so
                        // the button letter is made unique so it doesn't collide with anything else
                        return {
                            letter: ch,
                            labelKey: 'buttons_schedulePublish',
                            handler: args.methods.schedulePublish,
                            hotKey: 'alt+shift+s',
                            hotKeyWhenHidden: true,
                            alias: 'schedulePublish',
                            addEllipsis: 'true'
                        };
                    case 'PUBLISH_DESCENDANTS':
                        // Publish descendants - it doesn't have a permission letter so
                        // the button letter is made unique so it doesn't collide with anything else
                        return {
                            letter: ch,
                            labelKey: 'buttons_publishDescendants',
                            handler: args.methods.publishDescendants,
                            hotKey: 'alt+shift+p',
                            hotKeyWhenHidden: true,
                            alias: 'publishDescendant',
                            addEllipsis: 'true'
                        };
                    default:
                        return null;
                    }
                }
                //reset
                buttons.subButtons = [];
                //This is the ideal button order but depends on circumstance, we'll use this array to create the button list
                // Publish, SendToPublish
                var buttonOrder = [
                    'U',
                    'H',
                    'SCHEDULE',
                    'PUBLISH_DESCENDANTS'
                ];
                //Create the first button (primary button)
                //We cannot have the Save or SaveAndPublish buttons if they don't have create permissions when we are creating a new item.
                //Another tricky rule is if they only have Create + Browse permissions but not Save but if it's being created then they will
                // require the Save button in order to create.
                //So this code is going to create the primary button (either Publish, SendToPublish, Save) if we are not in create mode
                // or if the user has access to create.
                if (!args.create || _.contains(args.content.allowedActions, 'C')) {
                    for (var b in buttonOrder) {
                        if (_.contains(args.content.allowedActions, buttonOrder[b])) {
                            buttons.defaultButton = createButtonDefinition(buttonOrder[b]);
                            break;
                        }
                    }
                    //Here's the special check, if the button still isn't set and we are creating and they have create access
                    //we need to add the Save button
                    if (!buttons.defaultButton && args.create && _.contains(args.content.allowedActions, 'C')) {
                        buttons.defaultButton = createButtonDefinition('A');
                    }
                }
                //Now we need to make the drop down button list, this is also slightly tricky because:
                //We cannot have any buttons if there's no default button above.
                //We cannot have the unpublish button (Z) when there's no publish permission.
                //We cannot have the unpublish button (Z) when the item is not published.
                if (buttons.defaultButton) {
                    //get the last index of the button order
                    var lastIndex = _.indexOf(buttonOrder, buttons.defaultButton.letter);
                    //add the remaining
                    for (var i = lastIndex + 1; i < buttonOrder.length; i++) {
                        if (_.contains(args.content.allowedActions, buttonOrder[i])) {
                            buttons.subButtons.push(createButtonDefinition(buttonOrder[i]));
                        }
                    }
                    // if publishing is allowed also allow schedule publish
                    // we add this manually becuase it doesn't have a permission so it wont
                    // get picked up by the loop through permissions
                    if (_.contains(args.content.allowedActions, 'U')) {
                        buttons.subButtons.push(createButtonDefinition('SCHEDULE'));
                        buttons.subButtons.push(createButtonDefinition('PUBLISH_DESCENDANTS'));
                    }
                    // if we are not creating, then we should add unpublish too,
                    // so long as it's already published and if the user has access to publish
                    // and the user has access to unpublish (may have been removed via Event)
                    if (!args.create) {
                        var hasPublishedVariant = args.content.variants.filter(function (variant) {
                            return variant.state === 'Published' || variant.state === 'PublishedPendingChanges';
                        }).length > 0;
                        if (hasPublishedVariant && _.contains(args.content.allowedActions, 'U') && _.contains(args.content.allowedActions, 'Z')) {
                            buttons.subButtons.push(createButtonDefinition('Z'));
                        }
                    }
                }
                return buttons;
            },
            /**
     * @ngdoc method
     * @name umbraco.services.contentEditingHelper#getAllProps
     * @methodOf umbraco.services.contentEditingHelper
     * @function
     *
     * @description
     * Returns all propertes contained for the tabbed content item
     */
            getAllProps: function getAllProps(content) {
                var allProps = [];
                for (var i = 0; i < content.tabs.length; i++) {
                    for (var p = 0; p < content.tabs[i].properties.length; p++) {
                        allProps.push(content.tabs[i].properties[p]);
                    }
                }
                return allProps;
            },
            /**
     * @ngdoc method
     * @name umbraco.services.contentEditingHelper#buildCompositeVariantId
     * @methodOf umbraco.services.contentEditingHelper
     * @function
     *
     * @description
     * Returns a id for the variant that is unique between all variants on the content
     * Note "invariant" is used for the invariant culture,
     * "null" is used for the NULL segment
     */
            buildCompositeVariantId: function buildCompositeVariantId(variant) {
                return (variant.language ? variant.language.culture : 'invariant') + '_' + (variant.segment ? variant.segment : 'null');
            },
            /**
     * @ngdoc method
     * @name umbraco.services.contentEditingHelper#configureButtons
     * @methodOf umbraco.services.contentEditingHelper
     * @function
     *
     * @description
     * Returns a letter array for buttons, with the primary one first based on content model, permissions and editor state
     */
            getAllowedActions: function getAllowedActions(content, creating) {
                //This is the ideal button order but depends on circumstance, we'll use this array to create the button list
                // Publish, SendToPublish, Save
                var actionOrder = [
                    'U',
                    'H',
                    'A'
                ];
                var defaultActions;
                var actions = [];
                //Create the first button (primary button)
                //We cannot have the Save or SaveAndPublish buttons if they don't have create permissions when we are creating a new item.
                if (!creating || _.contains(content.allowedActions, 'C')) {
                    for (var b in actionOrder) {
                        if (_.contains(content.allowedActions, actionOrder[b])) {
                            defaultAction = actionOrder[b];
                            break;
                        }
                    }
                }
                actions.push(defaultAction);
                //Now we need to make the drop down button list, this is also slightly tricky because:
                //We cannot have any buttons if there's no default button above.
                //We cannot have the unpublish button (Z) when there's no publish permission.
                //We cannot have the unpublish button (Z) when the item is not published.
                if (defaultAction) {
                    //get the last index of the button order
                    var lastIndex = _.indexOf(actionOrder, defaultAction);
                    //add the remaining
                    for (var i = lastIndex + 1; i < actionOrder.length; i++) {
                        if (_.contains(content.allowedActions, actionOrder[i])) {
                            actions.push(actionOrder[i]);
                        }
                    }
                    //if we are not creating, then we should add unpublish too,
                    // so long as it's already published and if the user has access to publish
                    if (!creating) {
                        if (content.publishDate && _.contains(content.allowedActions, 'U')) {
                            actions.push('Z');
                        }
                    }
                }
                return actions;
            },
            /**
     * @ngdoc method
     * @name umbraco.services.contentEditingHelper#getButtonFromAction
     * @methodOf umbraco.services.contentEditingHelper
     * @function
     *
     * @description
     * Returns a button object to render a button for the tabbed editor
     * currently only returns built in system buttons for content and media actions
     * returns label, alias, action char and hot-key
     */
            getButtonFromAction: function getButtonFromAction(ch) {
                switch (ch) {
                case 'U':
                    return {
                        letter: ch,
                        labelKey: 'buttons_saveAndPublish',
                        handler: 'saveAndPublish',
                        hotKey: 'ctrl+p'
                    };
                case 'H':
                    //send to publish
                    return {
                        letter: ch,
                        labelKey: 'buttons_saveToPublish',
                        handler: 'sendToPublish',
                        hotKey: 'ctrl+p'
                    };
                case 'A':
                    return {
                        letter: ch,
                        labelKey: 'buttons_save',
                        handler: 'save',
                        hotKey: 'ctrl+s'
                    };
                case 'Z':
                    return {
                        letter: ch,
                        labelKey: 'content_unpublish',
                        handler: 'unpublish'
                    };
                default:
                    return null;
                }
            },
            /**
     * @ngdoc method
     * @name umbraco.services.contentEditingHelper#reBindChangedProperties
     * @methodOf umbraco.services.contentEditingHelper
     * @function
     *
     * @description
     * Re-binds all changed property values to the origContent object from the savedContent object and returns an array of changed properties.
     * This re-binds both normal object property values along with content property values and works for content, media and members.
     * For variant content, this detects if the object contains the 'variants' property (i.e. for content) and re-binds all variant content properties.
     * This returns the list of changed content properties (does not include standard object property changes).
     */
            reBindChangedProperties: function reBindChangedProperties(origContent, savedContent) {
                // TODO: We should probably split out this logic to deal with media/members separately to content
                //a method to ignore built-in prop changes
                var shouldIgnore = function shouldIgnore(propName) {
                    return _.some([
                        'variants',
                        'tabs',
                        'properties',
                        'apps',
                        'createDateFormatted',
                        'releaseDateFormatted',
                        'expireDateFormatted',
                        'releaseDate',
                        'expireDate'
                    ], function (i) {
                        return i === propName;
                    });
                };
                //check for changed built-in properties of the content based on the server response object
                for (var o in savedContent) {
                    //ignore the ones listed in the array
                    if (shouldIgnore(o)) {
                        continue;
                    }
                    if (!_.isEqual(origContent[o], savedContent[o])) {
                        origContent[o] = savedContent[o];
                    }
                }
                //Now re-bind content properties. Since content has variants and media/members doesn't,
                //we'll detect the variants property for content to distinguish if it's content vs media/members.
                var isContent = false;
                var origVariants = [];
                var savedVariants = [];
                if (origContent.variants) {
                    isContent = true;
                    //it's content so assign the variants as they exist
                    origVariants = origContent.variants;
                    savedVariants = savedContent.variants;
                } else {
                    //it's media/member, so just add the object as-is to the variants collection
                    origVariants.push(origContent);
                    savedVariants.push(savedContent);
                }
                var changed = [];
                function getNewProp(alias, allNewProps) {
                    return _.find(allNewProps, function (item) {
                        return item.alias === alias;
                    });
                }
                //loop through each variant (i.e. tabbed content)
                for (var j = 0; j < origVariants.length; j++) {
                    var origVariant = origVariants[j];
                    var savedVariant = savedVariants[j];
                    //special case for content, don't sync this variant if it wasn't tagged
                    //for saving in the first place
                    if (isContent && !origVariant.save) {
                        continue;
                    }
                    //if it's content (not media/members), then we need to sync the variant specific data
                    if (origContent.variants) {
                        //the variant property names we need to sync
                        var variantPropertiesSync = ['state'];
                        //loop through the properties returned on the server object
                        for (var b in savedVariant) {
                            var shouldCompare = _.some(variantPropertiesSync, function (e) {
                                return e === b;
                            });
                            //only compare the explicit ones or ones we don't ignore
                            if (shouldCompare || !shouldIgnore(b)) {
                                if (!_.isEqual(origVariant[b], savedVariant[b])) {
                                    origVariant[b] = savedVariant[b];
                                }
                            }
                        }
                    }
                    //get a list of properties since they are contained in tabs
                    var allOrigProps = this.getAllProps(origVariant);
                    var allNewProps = this.getAllProps(savedVariant);
                    //check for changed properties of the content
                    for (var k = 0; k < allOrigProps.length; k++) {
                        var origProp = allOrigProps[k];
                        var alias = origProp.alias;
                        var newProp = getNewProp(alias, allNewProps);
                        if (newProp && !_.isEqual(origProp.value, newProp.value)) {
                            //they have changed so set the origContent prop to the new one
                            var origVal = origProp.value;
                            origProp.value = newProp.value;
                            //instead of having a property editor $watch their expression to check if it has
                            // been updated, instead we'll check for the existence of a special method on their model
                            // and just call it.
                            if (Utilities.isFunction(origProp.onValueChanged)) {
                                //send the newVal + oldVal
                                origProp.onValueChanged(origProp.value, origVal);
                            }
                            changed.push(origProp);
                        }
                    }
                }
                return changed;
            },
            /**
     * @ngdoc function
     * @name umbraco.services.contentEditingHelper#handleSaveError
     * @methodOf umbraco.services.contentEditingHelper
     * @function
     *
     * @description
     * A function to handle what happens when we have validation issues from the server side
     *
     */
            //TODO: Too many editors use this method for saving when this entire service should only be used for content/media/members,
            // there is formHelper.handleError for other editors which should be used!
            handleSaveError: function handleSaveError(args) {
                if (!args.err) {
                    throw 'args.err cannot be null';
                }
                //When the status is a 400 status with a custom header: X-Status-Reason: Validation failed, we have validation errors.
                //Otherwise the error is probably due to invalid data (i.e. someone mucking around with the ids or something).
                //Or, some strange server error
                if (args.err.status === 400) {
                    //now we need to look through all the validation errors
                    if (args.err.data && args.err.data.ModelState) {
                        //wire up the server validation errs
                        formHelper.handleServerValidation(args.err.data.ModelState);
                        //add model state errors to notifications
                        if (args.showNotifications) {
                            showNotificationsForModelsState(args.err.data.ModelState);
                        }
                        if (!this.redirectToCreatedContent(args.err.data.id, args.softRedirect) || args.softRedirect) {
                            // If we are not redirecting it's because this is not newly created content, else in some cases we are
                            // soft-redirecting which means the URL will change but the route wont (i.e. creating content).
                            // In this case we need to detect what properties have changed and re-bind them with the server data.
                            if (args.rebindCallback && Utilities.isFunction(args.rebindCallback)) {
                                args.rebindCallback();
                            }
                            // In this case notify all validators (don't clear the server validations though since we need to maintain their state because of
                            // how the variant switcher works in content). server validation state is always cleared when an editor first loads
                            // and in theory when an editor is destroyed.
                            serverValidationManager.notify();
                        }
                        //indicates we've handled the server result
                        return true;
                    }
                }
                return false;
            },
            /**
     * @ngdoc function
     * @name umbraco.services.contentEditingHelper#handleSuccessfulSave
     * @methodOf umbraco.services.contentEditingHelper
     * @function
     *
     * @description
     * A function to handle when saving a content item is successful. This will rebind the values of the model that have changed
     * ensure the notifications are displayed and that the appropriate events are fired. This will also check if we need to redirect
     * when we're creating new content.
     */
            //TODO: We need to move some of this to formHelper for saving, too many editors use this method for saving when this entire
            //service should only be used for content/media/members
            handleSuccessfulSave: function handleSuccessfulSave(args) {
                if (!args) {
                    throw 'args cannot be null';
                }
                if (!args.savedContent) {
                    throw 'args.savedContent cannot be null';
                }
                if (!this.redirectToCreatedContent(args.redirectId ? args.redirectId : args.savedContent.id, args.softRedirect) || args.softRedirect) {
                    // If we are not redirecting it's because this is not newly created content, else in some cases we are
                    // soft-redirecting which means the URL will change but the route wont (i.e. creating content).
                    // In this case we need to detect what properties have changed and re-bind them with the server data.
                    if (args.rebindCallback && Utilities.isFunction(args.rebindCallback)) {
                        args.rebindCallback();
                    }
                }
            },
            /**
     * @ngdoc function
     * @name umbraco.services.contentEditingHelper#redirectToCreatedContent
     * @methodOf umbraco.services.contentEditingHelper
     * @function
     *
     * @description
     * Changes the location to be editing the newly created content after create was successful.
     * We need to decide if we need to redirect to edito mode or if we will remain in create mode.
     * We will only need to maintain create mode if we have not fulfilled the basic requirements for creating an entity which is at least having a name and ID
     */
            redirectToCreatedContent: function redirectToCreatedContent(id, softRedirect) {
                //only continue if we are currently in create mode and not in infinite mode and if the resulting ID is valid
                if ($routeParams.create && isValidIdentifier(id)) {
                    //need to change the location to not be in 'create' mode. Currently the route will be something like:
                    // /belle/#/content/edit/1234?doctype=newsArticle&create=true
                    // but we need to remove everything after the query so that it is just:
                    // /belle/#/content/edit/9876 (where 9876 is the new id)
                    //clear the query strings
                    navigationService.clearSearch([
                        'cculture',
                        'csegment'
                    ]);
                    if (softRedirect) {
                        navigationService.setSoftRedirect();
                    }
                    //change to new path
                    $location.path('/' + $routeParams.section + '/' + $routeParams.tree + '/' + $routeParams.method + '/' + id);
                    //don't add a browser history for this
                    $location.replace();
                    return true;
                }
                return false;
            },
            /**
     * @ngdoc function
     * @name umbraco.services.contentEditingHelper#redirectToRenamedContent
     * @methodOf umbraco.services.contentEditingHelper
     * @function
     *
     * @description
     * For some editors like scripts or entites that have names as ids, these names can change and we need to redirect
     * to their new paths, this is helper method to do that.
     */
            //TODO: We need to move some of this to formHelper for saving, too many editors use this method for saving when this entire
            //service should only be used for content/media/members
            redirectToRenamedContent: function redirectToRenamedContent(id) {
                //clear the query strings
                navigationService.clearSearch();
                //change to new path
                $location.path('/' + $routeParams.section + '/' + $routeParams.tree + '/' + $routeParams.method + '/' + id);
                //don't add a browser history for this
                $location.replace();
                return true;
            },
            /**
     * @ngdoc function
     * @name umbraco.services.contentEditingHelper#sortVariants
     * @methodOf umbraco.services.contentEditingHelper
     * @function
     *
     * @description
     * Sorts the variants so default language is shown first. Mandatory languages are shown next and all other underneath. Both Mandatory and non mandatory languages are
     * sorted in the following groups 'Published', 'Draft', 'Not Created'. Within each of those groups the variants are
     * sorted by the language display name.
     *
     */
            sortVariants: function sortVariants(a, b) {
                var statesOrder = {
                    'PublishedPendingChanges': 1,
                    'Published': 1,
                    'Draft': 2,
                    'NotCreated': 3
                };
                var compareDefault = function compareDefault(a, b) {
                    return (!a.language.isDefault ? 1 : -1) - (!b.language.isDefault ? 1 : -1);
                };
                // Make sure mandatory variants goes on top, unless they are published, cause then they already goes to the top and then we want to mix them with other published variants.
                var compareMandatory = function compareMandatory(a, b) {
                    return a.state === 'PublishedPendingChanges' || a.state === 'Published' ? 0 : (!a.language.isMandatory ? 1 : -1) - (!b.language.isMandatory ? 1 : -1);
                };
                var compareState = function compareState(a, b) {
                    return (statesOrder[a.state] || 99) - (statesOrder[b.state] || 99);
                };
                var compareName = function compareName(a, b) {
                    return a.displayName.localeCompare(b.displayName);
                };
                return compareDefault(a, b) || compareMandatory(a, b) || compareState(a, b) || compareName(a, b);
            },
            /**
     * @ngdoc function
     * @name umbraco.services.contentEditingHelper#getSortedVariantsAndSegments
     * @methodOf umbraco.services.contentEditingHelper
     * @function
     *
     * @description
     * Returns an array of variants and segments sorted by the rules in the sortVariants method.
     * A variant language is followed by its segments in the array. If a segment doesn't have a parent variant it is
     * added to the end of the array.
     *
     */
            getSortedVariantsAndSegments: function getSortedVariantsAndSegments(variantsAndSegments) {
                var _this = this;
                var sortedVariants = variantsAndSegments.filter(function (variant) {
                    return !variant.segment;
                }).sort(this.sortVariants);
                var segments = variantsAndSegments.filter(function (variant) {
                    return variant.segment;
                });
                var sortedAvailableVariants = [];
                sortedVariants.forEach(function (variant) {
                    var sortedMatchedSegments = segments.filter(function (segment) {
                        return segment.language.culture === variant.language.culture;
                    }).sort(_this.sortVariants);
                    segments = segments.filter(function (segment) {
                        return segment.language.culture !== variant.language.culture;
                    });
                    sortedAvailableVariants = [].concat(_toConsumableArray(sortedAvailableVariants), [variant], _toConsumableArray(sortedMatchedSegments));
                });
                // if we have segments without a parent language variant we need to add the remaining segments to the array
                sortedAvailableVariants = [].concat(_toConsumableArray(sortedAvailableVariants), _toConsumableArray(segments.sort(this.sortVariants)));
                return sortedAvailableVariants;
            }
        };
    }
    angular.module('umbraco.services').factory('contentEditingHelper', contentEditingHelper);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.contentTypeHelper
 * @description A helper service for the content type editor
 **/
    function contentTypeHelper(contentTypeResource, dataTypeResource, $filter, $injector, $q) {
        var contentTypeHelperService = {
            createIdArray: function createIdArray(array) {
                var newArray = [];
                array.forEach(function (arrayItem) {
                    if (Utilities.isObject(arrayItem)) {
                        newArray.push(arrayItem.id);
                    } else {
                        newArray.push(arrayItem);
                    }
                });
                return newArray;
            },
            generateModels: function generateModels() {
                var deferred = $q.defer();
                var modelsResource = $injector.has('modelsBuilderManagementResource') ? $injector.get('modelsBuilderManagementResource') : null;
                var modelsBuilderEnabled = Umbraco.Sys.ServerVariables.umbracoPlugins.modelsBuilder.enabled;
                if (modelsBuilderEnabled && modelsResource) {
                    modelsResource.buildModels().then(function (result) {
                        deferred.resolve(result);
                        //just calling this to get the servar back to life
                        modelsResource.getModelsOutOfDateStatus();
                    }, function (e) {
                        deferred.reject(e);
                    });
                } else {
                    deferred.resolve(false);
                }
                return deferred.promise;
            },
            checkModelsBuilderStatus: function checkModelsBuilderStatus() {
                var deferred = $q.defer();
                var modelsResource = $injector.has('modelsBuilderManagementResource') ? $injector.get('modelsBuilderManagementResource') : null;
                var modelsBuilderEnabled = Umbraco && Umbraco.Sys && Umbraco.Sys.ServerVariables && Umbraco.Sys.ServerVariables.umbracoPlugins && Umbraco.Sys.ServerVariables.umbracoPlugins.modelsBuilder && Umbraco.Sys.ServerVariables.umbracoPlugins.modelsBuilder.enabled === true;
                if (modelsBuilderEnabled && modelsResource) {
                    modelsResource.getModelsOutOfDateStatus().then(function (result) {
                        //Generate models buttons should be enabled if it is 0
                        deferred.resolve(result.status === 0);
                    });
                } else {
                    deferred.resolve(false);
                }
                return deferred.promise;
            },
            makeObjectArrayFromId: function makeObjectArrayFromId(idArray, objectArray) {
                var newArray = [];
                for (var idIndex = 0; idArray.length > idIndex; idIndex++) {
                    var id = idArray[idIndex];
                    for (var objectIndex = 0; objectArray.length > objectIndex; objectIndex++) {
                        var object = objectArray[objectIndex];
                        if (id === object.id) {
                            newArray.push(object);
                        }
                    }
                }
                return newArray;
            },
            validateAddingComposition: function validateAddingComposition(contentType, compositeContentType) {
                //Validate that by adding this group that we are not adding duplicate property type aliases
                var propertiesAdding = _.flatten(_.map(compositeContentType.groups, function (g) {
                    return _.map(g.properties, function (p) {
                        return p.alias;
                    });
                }));
                var propAliasesExisting = _.filter(_.flatten(_.map(contentType.groups, function (g) {
                    return _.map(g.properties, function (p) {
                        return p.alias;
                    });
                })), function (f) {
                    return f !== null && f !== undefined;
                });
                var intersec = _.intersection(propertiesAdding, propAliasesExisting);
                if (intersec.length > 0) {
                    //return the overlapping property aliases
                    return intersec;
                }
                //no overlapping property aliases
                return [];
            },
            mergeCompositeContentType: function mergeCompositeContentType(contentType, compositeContentType) {
                //Validate that there are no overlapping aliases
                var overlappingAliases = this.validateAddingComposition(contentType, compositeContentType);
                if (overlappingAliases.length > 0) {
                    throw new Error('Cannot add this composition, these properties already exist on the content type: ' + overlappingAliases.join());
                }
                compositeContentType.groups.forEach(function (compositionGroup) {
                    // order composition groups based on sort order
                    compositionGroup.properties = $filter('orderBy')(compositionGroup.properties, 'sortOrder');
                    // get data type details
                    compositionGroup.properties.forEach(function (property) {
                        dataTypeResource.getById(property.dataTypeId).then(function (dataType) {
                            property.dataTypeIcon = dataType.icon;
                            property.dataTypeName = dataType.name;
                        });
                    });
                    // set inherited state on tab
                    compositionGroup.inherited = true;
                    // set inherited state on properties
                    compositionGroup.properties.forEach(function (compositionProperty) {
                        compositionProperty.inherited = true;
                    });
                    // set tab state
                    compositionGroup.tabState = 'inActive';
                    // if groups are named the same - merge the groups
                    contentType.groups.forEach(function (contentTypeGroup) {
                        if (contentTypeGroup.name === compositionGroup.name) {
                            // set flag to show if properties has been merged into a tab
                            compositionGroup.groupIsMerged = true;
                            // make group inherited
                            contentTypeGroup.inherited = true;
                            // add properties to the top of the array
                            contentTypeGroup.properties = compositionGroup.properties.concat(contentTypeGroup.properties);
                            // update sort order on all properties in merged group
                            contentTypeGroup.properties = contentTypeHelperService.updatePropertiesSortOrder(contentTypeGroup.properties);
                            // make parentTabContentTypeNames to an array so we can push values
                            if (contentTypeGroup.parentTabContentTypeNames === null || contentTypeGroup.parentTabContentTypeNames === undefined) {
                                contentTypeGroup.parentTabContentTypeNames = [];
                            }
                            // push name to array of merged composite content types
                            contentTypeGroup.parentTabContentTypeNames.push(compositeContentType.name);
                            // make parentTabContentTypes to an array so we can push values
                            if (contentTypeGroup.parentTabContentTypes === null || contentTypeGroup.parentTabContentTypes === undefined) {
                                contentTypeGroup.parentTabContentTypes = [];
                            }
                            // push id to array of merged composite content types
                            contentTypeGroup.parentTabContentTypes.push(compositeContentType.id);
                            // get sort order from composition
                            contentTypeGroup.sortOrder = compositionGroup.sortOrder;
                            // splice group to the top of the array
                            var contentTypeGroupCopy = Utilities.copy(contentTypeGroup);
                            var index = contentType.groups.indexOf(contentTypeGroup);
                            contentType.groups.splice(index, 1);
                            contentType.groups.unshift(contentTypeGroupCopy);
                        }
                    });
                    // if group is not merged - push it to the end of the array - before init tab
                    if (compositionGroup.groupIsMerged === false || compositionGroup.groupIsMerged === undefined) {
                        // make parentTabContentTypeNames to an array so we can push values
                        if (compositionGroup.parentTabContentTypeNames === null || compositionGroup.parentTabContentTypeNames === undefined) {
                            compositionGroup.parentTabContentTypeNames = [];
                        }
                        // push name to array of merged composite content types
                        compositionGroup.parentTabContentTypeNames.push(compositeContentType.name);
                        // make parentTabContentTypes to an array so we can push values
                        if (compositionGroup.parentTabContentTypes === null || compositionGroup.parentTabContentTypes === undefined) {
                            compositionGroup.parentTabContentTypes = [];
                        }
                        // push id to array of merged composite content types
                        compositionGroup.parentTabContentTypes.push(compositeContentType.id);
                        // push group before placeholder tab
                        contentType.groups.unshift(compositionGroup);
                    }
                });
                // sort all groups by sortOrder property
                contentType.groups = $filter('orderBy')(contentType.groups, 'sortOrder');
                return contentType;
            },
            splitCompositeContentType: function splitCompositeContentType(contentType, compositeContentType) {
                var groups = [];
                contentType.groups.forEach(function (contentTypeGroup) {
                    if (contentTypeGroup.tabState !== 'init') {
                        var idIndex = contentTypeGroup.parentTabContentTypes.indexOf(compositeContentType.id);
                        var nameIndex = contentTypeGroup.parentTabContentTypeNames.indexOf(compositeContentType.name);
                        var groupIndex = contentType.groups.indexOf(contentTypeGroup);
                        if (idIndex !== -1) {
                            var properties = [];
                            // remove all properties from composite content type
                            contentTypeGroup.properties.forEach(function (property) {
                                if (property.contentTypeId !== compositeContentType.id) {
                                    properties.push(property);
                                }
                            });
                            // set new properties array to properties
                            contentTypeGroup.properties = properties;
                            // remove composite content type name and id from inherited arrays
                            contentTypeGroup.parentTabContentTypes.splice(idIndex, 1);
                            contentTypeGroup.parentTabContentTypeNames.splice(nameIndex, 1);
                            // remove inherited state if there are no inherited properties
                            if (contentTypeGroup.parentTabContentTypes.length === 0) {
                                contentTypeGroup.inherited = false;
                            }
                            // remove group if there are no properties left
                            if (contentTypeGroup.properties.length > 1) {
                                //contentType.groups.splice(groupIndex, 1);
                                groups.push(contentTypeGroup);
                            }
                        } else {
                            groups.push(contentTypeGroup);
                        }
                    } else {
                        groups.push(contentTypeGroup);
                    }
                    // update sort order on properties
                    contentTypeGroup.properties = contentTypeHelperService.updatePropertiesSortOrder(contentTypeGroup.properties);
                });
                contentType.groups = groups;
            },
            updatePropertiesSortOrder: function updatePropertiesSortOrder(properties) {
                var sortOrder = 0;
                properties.forEach(function (property) {
                    if (!property.inherited && property.propertyState !== 'init') {
                        property.sortOrder = sortOrder;
                    }
                    sortOrder++;
                });
                return properties;
            },
            getTemplatePlaceholder: function getTemplatePlaceholder() {
                var templatePlaceholder = {
                    'name': '',
                    'icon': 'icon-layout',
                    'alias': 'templatePlaceholder',
                    'placeholder': true
                };
                return templatePlaceholder;
            },
            insertDefaultTemplatePlaceholder: function insertDefaultTemplatePlaceholder(defaultTemplate) {
                // get template placeholder
                var templatePlaceholder = contentTypeHelperService.getTemplatePlaceholder();
                // add as default template
                defaultTemplate = templatePlaceholder;
                return defaultTemplate;
            },
            insertTemplatePlaceholder: function insertTemplatePlaceholder(array) {
                // get template placeholder
                var templatePlaceholder = contentTypeHelperService.getTemplatePlaceholder();
                // add as selected item
                array.push(templatePlaceholder);
                return array;
            },
            insertChildNodePlaceholder: function insertChildNodePlaceholder(array, name, icon, id) {
                var placeholder = {
                    'name': name,
                    'icon': icon,
                    'id': id
                };
                array.push(placeholder);
            }
        };
        return contentTypeHelperService;
    }
    angular.module('umbraco.services').factory('contentTypeHelper', contentTypeHelper);
    'use strict';
    /**
* @ngdoc service
* @name umbraco.services.cropperHelper
* @description A helper object used for dealing with image cropper data
**/
    function cropperHelper(umbRequestHelper, $http) {
        var service = {
            /**
    * @ngdoc method
    * @name umbraco.services.cropperHelper#configuration
    * @methodOf umbraco.services.cropperHelper
    *
    * @description
    * Returns a collection of plugins available to the tinyMCE editor
    *
    */
            configuration: function configuration(mediaTypeAlias) {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('imageCropperApiBaseUrl', 'GetConfiguration', [{ mediaTypeAlias: mediaTypeAlias }])), 'Failed to retrieve tinymce configuration');
            },
            //utill for getting either min/max aspect ratio to scale image after
            calculateAspectRatioFit: function calculateAspectRatioFit(srcWidth, srcHeight, maxWidth, maxHeight, maximize) {
                var ratio = [
                    maxWidth / srcWidth,
                    maxHeight / srcHeight
                ];
                if (maximize) {
                    ratio = Math.max(ratio[0], ratio[1]);
                } else {
                    ratio = Math.min(ratio[0], ratio[1]);
                }
                return {
                    width: srcWidth * ratio,
                    height: srcHeight * ratio,
                    ratio: ratio
                };
            },
            //utill for scaling width / height given a ratio
            calculateSizeToRatio: function calculateSizeToRatio(srcWidth, srcHeight, ratio) {
                return {
                    width: srcWidth * ratio,
                    height: srcHeight * ratio,
                    ratio: ratio
                };
            },
            scaleToMaxSize: function scaleToMaxSize(srcWidth, srcHeight, maxSize) {
                var retVal = {
                    height: srcHeight,
                    width: srcWidth
                };
                if (srcWidth > maxSize || srcHeight > maxSize) {
                    var ratio = [
                        maxSize / srcWidth,
                        maxSize / srcHeight
                    ];
                    ratio = Math.min(ratio[0], ratio[1]);
                    retVal.height = srcHeight * ratio;
                    retVal.width = srcWidth * ratio;
                }
                return retVal;
            },
            //returns a ng-style object with top,left,width,height pixel measurements
            //expects {left,right,top,bottom} - {width,height}, {width,height}, int
            //offset is just to push the image position a number of pixels from top,left    
            convertToStyle: function convertToStyle(coordinates, originalSize, viewPort, offset) {
                var coordinates_px = service.coordinatesToPixels(coordinates, originalSize, offset);
                var _offset = offset || 0;
                var x = 1 - (coordinates.x1 + Math.abs(coordinates.x2));
                var left_of_x = originalSize.width * x;
                var ratio = viewPort.width / left_of_x;
                var style = {
                    position: 'absolute',
                    top: -(coordinates_px.y1 * ratio) + _offset,
                    left: -(coordinates_px.x1 * ratio) + _offset,
                    width: Math.floor(originalSize.width * ratio),
                    height: Math.floor(originalSize.height * ratio),
                    originalWidth: originalSize.width,
                    originalHeight: originalSize.height,
                    ratio: ratio
                };
                return style;
            },
            coordinatesToPixels: function coordinatesToPixels(coordinates, originalSize, offset) {
                var coordinates_px = {
                    x1: Math.floor(coordinates.x1 * originalSize.width),
                    y1: Math.floor(coordinates.y1 * originalSize.height),
                    x2: Math.floor(coordinates.x2 * originalSize.width),
                    y2: Math.floor(coordinates.y2 * originalSize.height)
                };
                return coordinates_px;
            },
            pixelsToCoordinates: function pixelsToCoordinates(image, width, height, offset) {
                var x1_px = Math.abs(image.left - offset);
                var y1_px = Math.abs(image.top - offset);
                var x2_px = image.width - (x1_px + width);
                var y2_px = image.height - (y1_px + height);
                //crop coordinates in %
                var crop = {};
                crop.x1 = x1_px / image.width;
                crop.y1 = y1_px / image.height;
                crop.x2 = x2_px / image.width;
                crop.y2 = y2_px / image.height;
                for (var coord in crop) {
                    if (crop[coord] < 0) {
                        crop[coord] = 0;
                    }
                }
                return crop;
            },
            alignToCoordinates: function alignToCoordinates(image, center, viewport) {
                var min_left = image.width - viewport.width;
                var min_top = image.height - viewport.height;
                var c_top = -(center.top * image.height) + viewport.height / 2;
                var c_left = -(center.left * image.width) + viewport.width / 2;
                if (c_top < -min_top) {
                    c_top = -min_top;
                }
                if (c_top > 0) {
                    c_top = 0;
                }
                if (c_left < -min_left) {
                    c_left = -min_left;
                }
                if (c_left > 0) {
                    c_left = 0;
                }
                return {
                    left: c_left,
                    top: c_top
                };
            },
            syncElements: function syncElements(source, target) {
                target.height(source.height());
                target.width(source.width());
                target.css({
                    'top': source[0].offsetTop,
                    'left': source[0].offsetLeft
                });
            }
        };
        return service;
    }
    angular.module('umbraco.services').factory('cropperHelper', cropperHelper);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.dataTypeHelper
 * @description A helper service for data types
 **/
    function dataTypeHelper() {
        var dataTypeHelperService = {
            createPreValueProps: function createPreValueProps(preVals) {
                var preValues = [];
                for (var i = 0; i < preVals.length; i++) {
                    preValues.push({
                        hideLabel: preVals[i].hideLabel,
                        alias: preVals[i].key,
                        description: preVals[i].description,
                        label: preVals[i].label,
                        view: preVals[i].view,
                        value: preVals[i].value
                    });
                }
                return preValues;
            },
            rebindChangedProperties: function rebindChangedProperties(origContent, savedContent) {
                //a method to ignore built-in prop changes
                var shouldIgnore = function shouldIgnore(propName) {
                    return _.some([
                        'notifications',
                        'ModelState'
                    ], function (i) {
                        return i === propName;
                    });
                };
                //check for changed built-in properties of the content
                for (var o in origContent) {
                    //ignore the ones listed in the array
                    if (shouldIgnore(o)) {
                        continue;
                    }
                    if (!_.isEqual(origContent[o], savedContent[o])) {
                        origContent[o] = savedContent[o];
                    }
                }
            }
        };
        return dataTypeHelperService;
    }
    angular.module('umbraco.services').factory('dataTypeHelper', dataTypeHelper);
    'use strict';
    function _slicedToArray(arr, i) {
        return _arrayWithHoles(arr) || _iterableToArrayLimit(arr, i) || _nonIterableRest();
    }
    function _nonIterableRest() {
        throw new TypeError('Invalid attempt to destructure non-iterable instance');
    }
    function _iterableToArrayLimit(arr, i) {
        if (!(Symbol.iterator in Object(arr) || Object.prototype.toString.call(arr) === '[object Arguments]')) {
            return;
        }
        var _arr = [];
        var _n = true;
        var _d = false;
        var _e = undefined;
        try {
            for (var _i = arr[Symbol.iterator](), _s; !(_n = (_s = _i.next()).done); _n = true) {
                _arr.push(_s.value);
                if (i && _arr.length === i)
                    break;
            }
        } catch (err) {
            _d = true;
            _e = err;
        } finally {
            try {
                if (!_n && _i['return'] != null)
                    _i['return']();
            } finally {
                if (_d)
                    throw _e;
            }
        }
        return _arr;
    }
    function _arrayWithHoles(arr) {
        if (Array.isArray(arr))
            return arr;
    }
    /**
 @ngdoc service
 * @name umbraco.services.editorService
 *
 * @description
 * Added in Umbraco 8.0. Application-wide service for handling infinite editing.
 *
 *
 *
 *
<h2><strong>Open a build-in infinite editor (media picker)</strong></h2>
<h3>Markup example</h3>
<pre>
    <div ng-controller="My.MediaPickerController as vm">
        <button type="button" ng-click="vm.openMediaPicker()">Open</button>
    </div>
</pre>

<h3>Controller example</h3>
<pre>
    (function () {
        "use strict";

        function MediaPickerController(editorService) {

            var vm = this;

            vm.openMediaPicker = openMediaPicker;

            function openMediaPicker() {
                var mediaPickerOptions = {
                    multiPicker: true,
                    submit: function(model) {
                        editorService.close();
                    },
                    close: function() {
                        editorService.close();
                    }
                };
                editorService.mediaPicker(mediaPickerOptions);
            };
        }

        angular.module("umbraco").controller("My.MediaPickerController", MediaPickerController);
    })();
</pre>

<h2><strong>Building a custom infinite editor</strong></h2>
<h3>Open the custom infinite editor (Markup)</h3>
<pre>
    <div ng-controller="My.Controller as vm">
        <button type="button" ng-click="vm.open()">Open</button>
    </div>
</pre>

<h3>Open the custom infinite editor (Controller)</h3>
<pre>
    (function () {
        "use strict";

        function Controller(editorService) {

            var vm = this;

            vm.open = open;

            function open() {
                var options = {
                    title: "My custom infinite editor",
                    view: "path/to/view.html",
                    submit: function(model) {
                        editorService.close();
                    },
                    close: function() {
                        editorService.close();
                    }
                };
                editorService.open(options);
            };
        }

        angular.module("umbraco").controller("My.Controller", Controller);
    })();
</pre>

<h3><strong>The custom infinite editor view</strong></h3>
When building a custom infinite editor view you can use the same components as a normal editor ({@link umbraco.directives.directive:umbEditorView umbEditorView}).
<pre>
    <div ng-controller="My.InfiniteEditorController as vm">

        <umb-editor-view>

            <umb-editor-header
                name="model.title"
                name-locked="true"
                hide-alias="true"
                hide-icon="true"
                hide-description="true">
            </umb-editor-header>

            <umb-editor-container>
                <umb-box>
                    <umb-box-content>
                        {{model | json}}
                    </umb-box-content>
                </umb-box>
            </umb-editor-container>

            <umb-editor-footer>
                <umb-editor-footer-content-right>
                    <umb-button
                        type="button"
                        button-style="link"
                        label-key="general_close"
                        shortcut="esc"
                        action="vm.close()">
                    </umb-button>
                    <umb-button
                        type="button"
                        button-style="action"
                        label-key="general_submit"
                        action="vm.submit(model)">
                    </umb-button>
                </umb-editor-footer-content-right>
            </umb-editor-footer>

        </umb-editor-view>

    </div>
</pre>

<h3>The custom infinite editor controller</h3>
<pre>
    (function () {
        "use strict";

        function InfiniteEditorController($scope) {

            var vm = this;

            vm.submit = submit;
            vm.close = close;

            function submit() {
                if($scope.model.submit) {
                    $scope.model.submit($scope.model);
                }
            }

            function close() {
                if($scope.model.close) {
                    $scope.model.close();
                }
            }

        }

        angular.module("umbraco").controller("My.InfiniteEditorController", InfiniteEditorController);
    })();
</pre>
 */
    (function () {
        'use strict';
        function editorService(eventsService, keyboardService, $timeout) {
            var editorsKeyboardShorcuts = [];
            var editors = [];
            var isEnabled = true;
            var lastElementInFocus = null;
            // events for backdrop
            eventsService.on('appState.backdrop', function (name, args) {
                if (args.show === true) {
                    blur();
                } else {
                    focus();
                }
            });
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#getEditors
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Method to return all open editors.
     */
            function getEditors() {
                return editors;
            }
            ;
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#getNumberOfEditors
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Method to return the number of open editors.
     */
            function getNumberOfEditors() {
                return editors.length;
            }
            ;
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#blur
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Method to tell editors that they are begin blurred.
     */
            function blur() {
                /* keyboard shortcuts will be overwritten by the new infinite editor
          so we need to store the shortcuts for the current editor so they can be rebound
          when the infinite editor closes
      */
                unbindKeyboardShortcuts();
                isEnabled = false;
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#blur
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Method to tell editors that they are gaining focus again.
     */
            function focus() {
                if (isEnabled === false) {
                    /* keyboard shortcuts will be overwritten by the new infinite editor
            so we need to store the shortcuts for the current editor so they can be rebound
            when the infinite editor closes
        */
                    rebindKeyboardShortcuts();
                    isEnabled = true;
                }
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#open
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Method to open a new editor in infinite editing.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.view URL to view.
     * @param {string} editor.size Sets the size of the editor (`small` or `medium`). If nothing is set it will use full width.
     */
            function open(editor) {
                /* keyboard shortcuts will be overwritten by the new infinite editor
          so we need to store the shortcuts for the current editor so they can be rebound
          when the infinite editor closes
      */
                unbindKeyboardShortcuts();
                // if this is the first editor layer, save the currently focused element
                // so we can re-apply focus to it once all the editor layers are closed 
                if (editors.length === 0) {
                    lastElementInFocus = document.activeElement;
                }
                // set flag so we know when the editor is open in "infinite mode"
                editor.infiniteMode = true;
                editors.push(editor);
                var args = {
                    editors: editors,
                    editor: editor
                };
                eventsService.emit('appState.editors.open', args);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#close
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Method to close the latest opened editor.
     */
            function close() {
                // close last opened editor
                var closedEditor = editors[editors.length - 1];
                editors.splice(-1, 1);
                var args = {
                    editors: editors,
                    editor: closedEditor
                };
                // emit event to let components know an editor has been removed
                eventsService.emit('appState.editors.close', args);
                // delay required to map the properties to the correct editor due
                // to another delay in the closing animation of the editor
                $timeout(function () {
                    // rebind keyboard shortcuts for the new editor in focus
                    rebindKeyboardShortcuts();
                    if (editors.length === 0 && lastElementInFocus) {
                        lastElementInFocus.focus();
                    }
                }, 0);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#closeAll
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Method to close all open editors.
     */
            function closeAll() {
                editors = [];
                var args = {
                    editors: editors,
                    editor: null
                };
                eventsService.emit('appState.editors.close', args);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#contentEditor
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a content editor in infinite editing, the submit callback returns the updated content item.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.id The id of the content item.
     * @param {boolean} editor.create Create new content item.
     * @param {function} editor.submit Callback function when the publish and close button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @param {string} editor.parentId If editor.create is true, provide parentId for the creation of the content item.
     * @param {string} editor.documentTypeAlias If editor.create is true, provide document type alias for the creation of the content item.
     * @param {boolean} editor.allowSaveAndClose If editor is being used in infinite editing allows the editor to close when the save action is performed.
     * @param {boolean} editor.allowPublishAndClose If editor is being used in infinite editing allows the editor to close when the publish action is performed.
     * @returns {object} editor object
     */
            function contentEditor(editor) {
                editor.view = 'views/content/edit.html';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#contentPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a content picker in infinite editing, the submit callback returns an array of selected items.
     *
     * @param {object} editor rendering options.
     * @param {boolean} editor.multiPicker Pick one or multiple items.
     * @param {number} editor.startNodeId Set the startnode of the picker (optional).
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     *
     * @returns {object} editor object.
     */
            function contentPicker(editor) {
                editor.view = 'views/common/infiniteeditors/treepicker/treepicker.html';
                if (!editor.size)
                    editor.size = 'small';
                editor.section = 'content';
                editor.treeAlias = 'content';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#contentTypePicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a content type picker in infinite editing, the submit callback returns an array of selected items
     *
     * @param {object} editor rendering options.
     * @param {boolean} editor.multiPicker Pick one or multiple items.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object
     */
            function contentTypePicker(editor) {
                editor.view = 'views/common/infiniteeditors/treepicker/treepicker.html';
                if (!editor.size)
                    editor.size = 'small';
                editor.section = 'settings';
                editor.treeAlias = 'documentTypes';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#mediaTypePicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a media type picker in infinite editing, the submit callback returns an array of selected items.
     *
     * @param {object} editor rendering options.
     * @param {boolean} editor.multiPicker Pick one or multiple items.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function mediaTypePicker(editor) {
                editor.view = 'views/common/infiniteeditors/treepicker/treepicker.html';
                if (!editor.size)
                    editor.size = 'small';
                editor.section = 'settings';
                editor.treeAlias = 'mediaTypes';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#memberTypePicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a member type picker in infinite editing, the submit callback returns an array of selected items.
     *
     * @param {object} editor rendering options.
     * @param {boolean} editor.multiPicker Pick one or multiple items.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     *
     * @returns {object} editor object.
     */
            function memberTypePicker(editor) {
                editor.view = 'views/common/infiniteeditors/treepicker/treepicker.html';
                if (!editor.size)
                    editor.size = 'small';
                editor.section = 'settings';
                editor.treeAlias = 'memberTypes';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#copy
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a copy editor in infinite editing, the submit callback returns an array of selected items.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.section The node entity type.
     * @param {string} editor.currentNode The current node id.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function copy(editor) {
                editor.view = 'views/common/infiniteeditors/copy/copy.html';
                if (!editor.size)
                    editor.size = 'small';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#move
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a move editor in infinite editing.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.section The node entity type.
     * @param {string} editor.currentNode The current node id.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function move(editor) {
                editor.view = 'views/common/infiniteeditors/move/move.html';
                if (!editor.size)
                    editor.size = 'small';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#embed
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens an embed editor in infinite editing.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function embed(editor) {
                editor.view = 'views/common/infiniteeditors/embed/embed.html';
                if (!editor.size)
                    editor.size = 'small';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#rollback
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a rollback editor in infinite editing.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.node The node to rollback.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function rollback(editor) {
                editor.view = 'views/common/infiniteeditors/rollback/rollback.html';
                if (!editor.size)
                    editor.size = 'medium';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#linkPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens an embed editor in infinite editing.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.icon The icon class.
     * @param {string} editor.color The color class.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function linkPicker(editor) {
                editor.view = 'views/common/infiniteeditors/linkpicker/linkpicker.html';
                if (!editor.size)
                    editor.size = 'small';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#mediaEditor
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a media editor in infinite editing, the submit callback returns the updated media item.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.id The id of the media item.
     * @param {boolean} editor.create Create new media item.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function mediaEditor(editor) {
                editor.view = 'views/media/edit.html';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#mediaPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a media picker in infinite editing, the submit callback returns an array of selected media items.
     *
     * @param {object} editor rendering options.
     * @param {number} editor.startNodeId Set the startnode of the picker (optional).
     * @param {boolean} editor.multiPicker Pick one or multiple items.
     * @param {boolean} editor.onlyImages Only display files that have an image file-extension.
     * @param {boolean} editor.disableFolderSelect Disable folder selection.
     * @param {boolean} editor.disableFocalPoint Disable focal point editor for selected media.
     * @param {array} editor.updatedMediaNodes A list of ids for media items that have been updated through the media picker.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function mediaPicker(editor) {
                editor.view = 'views/common/infiniteeditors/mediapicker/mediapicker.html';
                if (!editor.size)
                    editor.size = 'medium';
                editor.updatedMediaNodes = [];
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#mediaCropDetails
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the media crop details editor in infinite editing, the submit callback returns the updated media object.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function mediaCropDetails(editor) {
                editor.view = 'views/common/infiniteeditors/mediapicker/overlays/mediacropdetails.html';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#iconPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens an icon picker in infinite editing, the submit callback returns the selected icon.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.icon The CSS class representing the icon - eg. `icon-autofill.
     * @param {string} editor.color The CSS class representing the color - eg. color-red.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function iconPicker(editor) {
                editor.view = 'views/common/infiniteeditors/iconpicker/iconpicker.html';
                if (!editor.size)
                    editor.size = 'small';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#documentTypeEditor
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the document type editor in infinite editing, the submit callback returns the alias of the saved document type.
     *
     * @param {object} editor rendering options.
     * @param {number} editor.id Indicates the ID of the document type to be edited. Alternatively the ID may be set to `-1` in combination with `create` being set to `true` to open the document type editor for creating a new document type.
     * @param {boolean} editor.create Set to `true` to open the document type editor for creating a new document type.
     * @param {boolean} editor.noTemplate If `true` and in combination with `create` being set to `true`, the document type editor will not create a corresponding template by default. This is similar to selecting the "Document Type without a template" in the Create dialog.
     * @param {boolean} editor.isElement If `true` and in combination with `create` being set to `true`, the "Is an Element type" option will be selected by default in the document type editor.
     * @param {boolean} editor.allowVaryByCulture If `true` and in combination with `create`, the "Allow varying by culture" option will be selected by default in the document type editor.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function documentTypeEditor(editor) {
                editor.view = 'views/documenttypes/edit.html';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#mediaTypeEditor
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the media type editor in infinite editing, the submit callback returns the saved media type.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function mediaTypeEditor(editor) {
                editor.view = 'views/mediatypes/edit.html';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#memberTypeEditor
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the member type editor in infinite editing, the submit callback returns the saved member type.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function memberTypeEditor(editor) {
                editor.view = 'views/membertypes/edit.html';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#queryBuilder
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the query builder in infinite editing, the submit callback returns the generated query.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function queryBuilder(editor) {
                editor.view = 'views/common/infiniteeditors/querybuilder/querybuilder.html';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#treePicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the query builder in infinite editing, the submit callback returns the generted query.
     *
     * @param {object} editor rendering options.
     * @param {string} options.section tree section to display.
     * @param {string} options.treeAlias specific tree to display.
     * @param {boolean} options.multiPicker should the tree pick one or multiple items before returning.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function treePicker(editor) {
                editor.view = 'views/common/infiniteeditors/treepicker/treepicker.html';
                if (!editor.size)
                    editor.size = 'small';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#nodePermissions
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the an editor to set node permissions.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function nodePermissions(editor) {
                editor.view = 'views/common/infiniteeditors/nodepermissions/nodepermissions.html';
                if (!editor.size)
                    editor.size = 'small';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#insertCodeSnippet
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Open an editor to insert code snippets into the code editor.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function insertCodeSnippet(editor) {
                editor.view = 'views/common/infiniteeditors/insertcodesnippet/insertcodesnippet.html';
                if (!editor.size)
                    editor.size = 'small';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#userGroupPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the user group picker in infinite editing, the submit callback returns an array of the selected user groups.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Submits the editor.
     * @param {function} editor.close Closes the editor.
     * @returns {object} editor object.
     */
            function userGroupPicker(editor) {
                editor.view = 'views/common/infiniteeditors/usergrouppicker/usergrouppicker.html';
                if (!editor.size)
                    editor.size = 'small';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#userGroupEditor
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the user group picker in infinite editing, the submit callback returns the saved user group.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function userGroupEditor(editor) {
                editor.view = 'views/users/group.html';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#templateEditor
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the template editor in infinite editing, the submit callback returns the saved template.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.id The template id.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function templateEditor(editor) {
                editor.view = 'views/templates/edit.html';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#sectionPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the section picker in infinite editing, the submit callback returns an array of the selected sections.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function sectionPicker(editor) {
                editor.view = 'views/common/infiniteeditors/sectionpicker/sectionpicker.html';
                if (!editor.size)
                    editor.size = 'small';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#insertField
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the insert field editor in infinite editing, the submit callback returns the code snippet.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function insertField(editor) {
                editor.view = 'views/common/infiniteeditors/insertfield/insertfield.html';
                if (!editor.size)
                    editor.size = 'small';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#templateSections
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the template sections editor in infinite editing, the submit callback returns the type to insert.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function templateSections(editor) {
                editor.view = 'views/common/infiniteeditors/templatesections/templatesections.html';
                if (!editor.size)
                    editor.size = 'small';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#userPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the section picker in infinite editing, the submit callback returns an array of the selected users.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function userPicker(editor) {
                editor.view = 'views/common/infiniteeditors/userpicker/userpicker.html';
                if (!editor.size)
                    editor.size = 'small';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#itemPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens the section picker in infinite editing, the submit callback returns an array of the selected items.
     *
     * @param {object} editor rendering options.
     * @param {array} editor.availableItems Array of available items.
     * @param {array} editor.selectedItems Array of selected items. When passed in the selected items will be filtered from the available items.
     * @param {boolean} editor.filter Set to false to hide the filter.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function itemPicker(editor) {
                editor.view = 'views/common/infiniteeditors/itempicker/itempicker.html';
                if (!editor.size)
                    editor.size = 'small';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#macroPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a macro picker in infinite editing, the submit callback returns an array of the selected items.
     *
     * @param {object} editor rendering options.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function macroPicker(editor) {
                editor.view = 'views/common/infiniteeditors/macropicker/macropicker.html';
                if (!editor.size)
                    editor.size = 'small';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#memberGroupPicker
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a member group picker in infinite editing.
     *
     * @param {object} editor rendering options.
     * @param {object} editor.multiPicker Pick one or multiple items.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @returns {object} editor object.
     */
            function memberGroupPicker(editor) {
                editor.view = 'views/common/infiniteeditors/membergrouppicker/membergrouppicker.html';
                if (!editor.size)
                    editor.size = 'small';
                open(editor);
            }
            /**
    * @ngdoc method
    * @name umbraco.services.editorService#memberPicker
    * @methodOf umbraco.services.editorService
    *
    * @description
    * Opens a member picker in infinite editing, the submit callback returns an array of selected items.
    * 
    * @param {object} editor rendering options.
    * @param {boolean} editor.multiPicker Pick one or multiple items.
    * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
    * @param {function} editor.close Callback function when the close button is clicked.
    * @returns {object} editor object.
    */
            function memberPicker(editor) {
                editor.view = 'views/common/infiniteeditors/treepicker/treepicker.html';
                if (!editor.size)
                    editor.size = 'small';
                editor.section = 'member';
                editor.treeAlias = 'member';
                open(editor);
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#memberEditor
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Opens a member editor in infinite editing, the submit callback returns the updated member.
     *
     * @param {object} editor rendering options.
     * @param {string} editor.id The id (GUID) of the member.
     * @param {boolean} editor.create Create new member.
     * @param {function} editor.submit Callback function when the submit button is clicked. Returns the editor model object.
     * @param {function} editor.close Callback function when the close button is clicked.
     * @param {string} editor.doctype If `editor.create` is `true`, provide member type for the creation of the member.
     * @returns {object} editor object.
     */
            function memberEditor(editor) {
                editor.view = 'views/member/edit.html';
                open(editor);
            }
            ///////////////////////
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#storeKeyboardShortcuts
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Internal method to keep track of keyboard shortcuts registered
     * to each editor so they can be rebound when an editor closes.
     */
            function unbindKeyboardShortcuts() {
                var shortcuts = Utilities.copy(keyboardService.keyboardEvent);
                editorsKeyboardShorcuts.push(shortcuts);
                // unbind the current shortcuts because we only want to
                // shortcuts from the newly opened editor working
                for (var _i = 0, _Object$entries = Object.entries(shortcuts); _i < _Object$entries.length; _i++) {
                    var _Object$entries$_i = _slicedToArray(_Object$entries[_i], 2), key = _Object$entries$_i[0], value = _Object$entries$_i[1];
                    keyboardService.unbind(key);
                }
            }
            /**
     * @ngdoc method
     * @name umbraco.services.editorService#rebindKeyboardShortcuts
     * @methodOf umbraco.services.editorService
     *
     * @description
     * Internal method to rebind keyboard shortcuts for the editor in focus.
     */
            function rebindKeyboardShortcuts() {
                // find the shortcuts from the previous editor
                var lastSetOfShortcutsIndex = editorsKeyboardShorcuts.length - 1;
                var lastSetOfShortcuts = editorsKeyboardShorcuts[lastSetOfShortcutsIndex];
                // rebind shortcuts
                for (var _i2 = 0, _Object$entries2 = Object.entries(lastSetOfShortcuts); _i2 < _Object$entries2.length; _i2++) {
                    var _Object$entries2$_i = _slicedToArray(_Object$entries2[_i2], 2), key = _Object$entries2$_i[0], value = _Object$entries2$_i[1];
                    keyboardService.bind(key, value.callback, value.opt);
                }
                // remove the shortcuts from the collection
                editorsKeyboardShorcuts.splice(lastSetOfShortcutsIndex, 1);
            }
            var service = {
                getEditors: getEditors,
                getNumberOfEditors: getNumberOfEditors,
                open: open,
                close: close,
                closeAll: closeAll,
                mediaEditor: mediaEditor,
                contentEditor: contentEditor,
                contentPicker: contentPicker,
                contentTypePicker: contentTypePicker,
                mediaTypePicker: mediaTypePicker,
                memberTypePicker: memberTypePicker,
                copy: copy,
                move: move,
                embed: embed,
                rollback: rollback,
                linkPicker: linkPicker,
                mediaPicker: mediaPicker,
                iconPicker: iconPicker,
                documentTypeEditor: documentTypeEditor,
                mediaTypeEditor: mediaTypeEditor,
                memberTypeEditor: memberTypeEditor,
                queryBuilder: queryBuilder,
                treePicker: treePicker,
                nodePermissions: nodePermissions,
                insertCodeSnippet: insertCodeSnippet,
                userGroupPicker: userGroupPicker,
                userGroupEditor: userGroupEditor,
                templateEditor: templateEditor,
                sectionPicker: sectionPicker,
                insertField: insertField,
                templateSections: templateSections,
                userPicker: userPicker,
                itemPicker: itemPicker,
                macroPicker: macroPicker,
                memberGroupPicker: memberGroupPicker,
                memberPicker: memberPicker,
                memberEditor: memberEditor,
                mediaCropDetails: mediaCropDetails
            };
            return service;
        }
        angular.module('umbraco.services').factory('editorService', editorService);
    }());
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.editorState
 * @function
 *
 * @description
 * Tracks the parent object for complex editors by exposing it as 
 * an object reference via editorState.current.getCurrent(). 
 * The state is cleared on each successful route.
 *
 * it is possible to modify this object, so should be used with care
 */
    angular.module('umbraco.services').factory('editorState', function ($rootScope, eventsService) {
        var current = null;
        var state = {
            /**
     * @ngdoc function
     * @name umbraco.services.angularHelper#set
     * @methodOf umbraco.services.editorState
     * @function
     *
     * @description
     * Sets the current entity object for the currently active editor
     * This is only used when implementing an editor with a complex model
     * like the content editor, where the model is modified by several
     * child controllers. 
     */
            set: function set(entity) {
                current = entity;
                eventsService.emit('editorState.changed', { entity: entity });
            },
            /**
     * @ngdoc function
     * @name umbraco.services.angularHelper#reset
     * @methodOf umbraco.services.editorState
     * @function
     *
     * @description
     * Since the editorstate entity is read-only, you cannot set it to null
     * only through the reset() method
     */
            reset: function reset() {
                current = null;
            },
            /**
     * @ngdoc function
     * @name umbraco.services.angularHelper#getCurrent
     * @methodOf umbraco.services.editorState
     * @function
     *
     * @description
     * Returns an object reference to the current editor entity.
     * the entity is the root object of the editor.
     * EditorState is used by property/parameter editors that need
     * access to the entire entity being edited, not just the property/parameter 
     *
     * editorState.current can not be overwritten, you should only read values from it
     * since modifying individual properties should be handled by the property editors
     */
            getCurrent: function getCurrent() {
                return current;
            }
        };
        // TODO: This shouldn't be removed! use getCurrent() method instead of a hacked readonly property which is confusing.
        //create a get/set property but don't allow setting
        Object.defineProperty(state, 'current', {
            get: function get() {
                return current;
            },
            set: function set(value) {
                throw 'Use editorState.set to set the value of the current entity';
            }
        });
        //execute on each successful route (this is only bound once per application since a service is a singleton)
        $rootScope.$on('$routeChangeSuccess', function (event, current, previous) {
            //reset the editorState on each successful route chage
            state.reset();
        });
        return state;
    });
    'use strict';
    (function () {
        'use strict';
        function entityHelper() {
            function getEntityTypeFromSection(section) {
                if (section === 'member') {
                    return 'Member';
                } else if (section === 'media') {
                    return 'Media';
                } else {
                    return 'Document';
                }
            }
            ////////////
            var service = { getEntityTypeFromSection: getEntityTypeFromSection };
            return service;
        }
        angular.module('umbraco.services').factory('entityHelper', entityHelper);
    }());
    'use strict';
    /** Used to broadcast and listen for global events and allow the ability to add async listeners to the callbacks */
    /*
    Core app events:

    app.ready
    app.authenticated
    app.notAuthenticated
    app.reInitialize
    app.userRefresh
    app.navigationReady
*/
    function eventsService($q, $rootScope) {
        return {
            /** raise an event with a given name */
            emit: function emit(name, args) {
                //there are no listeners
                if (!$rootScope.$$listeners[name]) {
                    return;
                }
                //send the event
                $rootScope.$emit(name, args);
            },
            /** subscribe to a method, or use scope.$on = same thing */
            on: function on(name, callback) {
                return $rootScope.$on(name, callback);
            },
            /** pass in the result of 'on' to this method, or just call the method returned from 'on' to unsubscribe */
            unsubscribe: function unsubscribe(handle) {
                if (Utilities.isFunction(handle)) {
                    handle();
                }
            }
        };
    }
    angular.module('umbraco.services').factory('eventsService', eventsService);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.externalLoginInfoService
 * @description A service for working with external login providers
 **/
    function externalLoginInfoService(externalLoginInfo, umbRequestHelper) {
        function getLoginProvider(provider) {
            if (provider) {
                var found = _.find(externalLoginInfo.providers, function (x) {
                    return x.authType == provider;
                });
                return found;
            }
            return null;
        }
        function getLoginProviderView(provider) {
            if (provider && provider.properties && provider.properties.UmbracoBackOfficeExternalLoginOptions && provider.properties.UmbracoBackOfficeExternalLoginOptions.CustomBackOfficeView) {
                return umbRequestHelper.convertVirtualToAbsolutePath(provider.properties.UmbracoBackOfficeExternalLoginOptions.CustomBackOfficeView);
            }
            return null;
        }
        /**
   * Returns true if any provider denies local login if `provider` is null, else whether the passed 
   * @param {any} provider
   */
        function hasDenyLocalLogin(provider) {
            if (!provider) {
                return _.some(externalLoginInfo.providers, function (x) {
                    return x.properties && x.properties.UmbracoBackOfficeExternalLoginOptions && x.properties.UmbracoBackOfficeExternalLoginOptions.DenyLocalLogin === true;
                });
            } else {
                return provider && provider.properties && provider.properties.UmbracoBackOfficeExternalLoginOptions && provider.properties.UmbracoBackOfficeExternalLoginOptions.DenyLocalLogin === true;
            }
        }
        /**
   * Returns all login providers    
   */
        function getLoginProviders() {
            return externalLoginInfo.providers;
        }
        /** Returns all logins providers that have options that the user can interact with */
        function getLoginProvidersWithOptions() {
            // only include providers that allow manual linking or ones that provide a custom view
            var providers = _.filter(externalLoginInfo.providers, function (x) {
                // transform the data and also include the custom view as a nicer property
                x.customView = getLoginProviderView(x);
                if (x.customView) {
                    return true;
                } else {
                    return x.properties.ExternalSignInAutoLinkOptions.AllowManualLinking;
                }
            });
            return providers;
        }
        return {
            hasDenyLocalLogin: hasDenyLocalLogin,
            getLoginProvider: getLoginProvider,
            getLoginProviders: getLoginProviders,
            getLoginProvidersWithOptions: getLoginProvidersWithOptions,
            getLoginProviderView: getLoginProviderView
        };
    }
    angular.module('umbraco.services').factory('externalLoginInfoService', externalLoginInfoService);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.fileManager
 * @function
 *
 * @description
 * Used by editors to manage any files that require uploading with the posted data, normally called by property editors
 * that need to attach files.
 * When a route changes successfully, we ensure that the collection is cleared.
 */
    function fileManager($rootScope) {
        var fileCollection = [];
        var mgr = {
            /**
     * @ngdoc function
     * @name umbraco.services.fileManager#setFiles
     * @methodOf umbraco.services.fileManager
     * @function
     *
     * @description
     *  Attaches files to the current manager for the current editor for a particular property, if an empty array is set
     *   for the files collection that effectively clears the files for the specified editor.
     */
            setFiles: function setFiles(args) {
                //propertyAlias, files
                if (!Utilities.isString(args.propertyAlias)) {
                    throw 'args.propertyAlias must be a non empty string';
                }
                if (!Utilities.isObject(args.files)) {
                    throw 'args.files must be an object';
                }
                //normalize to null
                if (!args.culture) {
                    args.culture = null;
                }
                if (!args.segment) {
                    args.segment = null;
                }
                var metaData = [];
                if (Utilities.isArray(args.metaData)) {
                    metaData = args.metaData;
                }
                //this will clear the files for the current property/culture/segment and then add the new ones for the current property
                fileCollection = _.reject(fileCollection, function (item) {
                    return item.alias === args.propertyAlias && (!args.culture || args.culture === item.culture) && (!args.segment || args.segment === item.segment);
                });
                for (var i = 0; i < args.files.length; i++) {
                    //save the file object to the files collection
                    fileCollection.push({
                        alias: args.propertyAlias,
                        file: args.files[i],
                        culture: args.culture,
                        segment: args.segment,
                        metaData: metaData
                    });
                }
            },
            /**
     * @ngdoc function
     * @name umbraco.services.fileManager#getFiles
     * @methodOf umbraco.services.fileManager
     * @function
     *
     * @description
     *  Returns all of the files attached to the file manager
     */
            getFiles: function getFiles() {
                return fileCollection;
            },
            /**
     * @ngdoc function
     * @name umbraco.services.fileManager#clearFiles
     * @methodOf umbraco.services.fileManager
     * @function
     *
     * @description
     *  Removes all files from the manager
     */
            clearFiles: function clearFiles() {
                fileCollection = [];
            }
        };
        //execute on each successful route (this is only bound once per application since a service is a singleton)
        $rootScope.$on('$routeChangeSuccess', function (event, current, previous) {
            //reset the file manager on each route change, the file collection is only relavent
            // when working in an editor and submitting data to the server.
            //This ensures that memory remains clear of any files and that the editors don't have to manually clear the files.
            mgr.clearFiles();
        });
        return mgr;
    }
    angular.module('umbraco.services').factory('fileManager', fileManager);
    'use strict';
    /**
 @ngdoc service
 * @name umbraco.services.focusService
 *
 * @description
 * <b>Added in Umbraco 8.1</b>. Application-wide service for focus related stuff.
 * 
 */
    (function () {
        'use strict';
        function focusService() {
            var currentFocus = null;
            var lastKnownFocus = null;
            function focusInApp(e) {
                currentFocus = e.target;
            }
            document.addEventListener('focusin', focusInApp);
            var service = {
                /**
      * @ngdoc function
      * @name umbraco.services.focusService#getLastKnownFocus
      * @methodOf umbraco.services.focusService
      * @function
       *
       * @description
       * Gives the element that was set to be remembered, the directive using this should store the value of this to make sure that its not changed white using that directive.
       * This variable is avaiable for directives that are not able to figure out the focused element on init, and there this service will help remembering it untill the directive is initialized.
       * 
       */
                getLastKnownFocus: function getLastKnownFocus() {
                    return lastKnownFocus;
                },
                /**
       * @ngdoc function
       * @name umbraco.services.focusService#rememberFocus
       * @methodOf umbraco.services.focusService
       * @function
       *
       * @description
       * Call this before a new focus is begin set, to be able to return to the focus before a given scenario.
       * 
       */
                rememberFocus: function rememberFocus() {
                    lastKnownFocus = currentFocus;
                },
                /**
       * @ngdoc function
       * @name umbraco.services.focusService#setLastKnownFocus
       * @methodOf umbraco.services.focusService
       * @function
       *
       * @description
       * Overwrite the element remembered as the last known element in focus.
       * 
       */
                setLastKnownFocus: function setLastKnownFocus(element) {
                    lastKnownFocus = element;
                }
            };
            return service;
        }
        angular.module('umbraco.services').factory('focusService', focusService);
    }());
    'use strict';
    (function () {
        'use strict';
        function focusLockService($document) {
            var elementToInert = $document[0].querySelector('#mainwrapper');
            function addInertAttribute() {
                if (elementToInert) {
                    elementToInert.setAttribute('inert', true);
                }
            }
            function removeInertAttribute() {
                if (elementToInert) {
                    elementToInert.removeAttribute('inert');
                }
            }
            var service = {
                addInertAttribute: addInertAttribute,
                removeInertAttribute: removeInertAttribute
            };
            return service;
        }
        angular.module('umbraco.services').factory('focusLockService', focusLockService);
    }());
    'use strict';
    function _toConsumableArray(arr) {
        return _arrayWithoutHoles(arr) || _iterableToArray(arr) || _nonIterableSpread();
    }
    function _nonIterableSpread() {
        throw new TypeError('Invalid attempt to spread non-iterable instance');
    }
    function _iterableToArray(iter) {
        if (Symbol.iterator in Object(iter) || Object.prototype.toString.call(iter) === '[object Arguments]')
            return Array.from(iter);
    }
    function _arrayWithoutHoles(arr) {
        if (Array.isArray(arr)) {
            for (var i = 0, arr2 = new Array(arr.length); i < arr.length; i++) {
                arr2[i] = arr[i];
            }
            return arr2;
        }
    }
    /**
 * @ngdoc service
 * @name umbraco.services.formHelper
 * @function
 *
 * @description
 * A utility class used to streamline how forms are developed, to ensure that validation is check and displayed consistently and to ensure that the correct events
 * fire when they need to.
 */
    function formHelper(angularHelper, serverValidationManager, notificationsService, overlayService) {
        return {
            /**
     * @ngdoc function
     * @name umbraco.services.formHelper#submitForm
     * @methodOf umbraco.services.formHelper
     * @function
     *
     * @description
     * Called by controllers when submitting a form - this ensures that all client validation is checked,
     * server validation is cleared, that the correct events execute and status messages are displayed.
     * This returns true if the form is valid, otherwise false if form submission cannot continue.
     *
     * @param {object} args An object containing arguments for form submission
     */
            submitForm: function submitForm(args) {
                var currentForm;
                if (!args) {
                    throw 'args cannot be null';
                }
                if (!args.scope) {
                    throw 'args.scope cannot be null';
                }
                if (!args.formCtrl) {
                    //try to get the closest form controller
                    currentForm = angularHelper.getRequiredCurrentForm(args.scope);
                } else {
                    currentForm = args.formCtrl;
                }
                //the first thing any form must do is broadcast the formSubmitting event
                args.scope.$broadcast('formSubmitting', {
                    scope: args.scope,
                    action: args.action
                });
                this.focusOnFirstError(currentForm);
                // Some property editors need to perform an action after all property editors have reacted to the formSubmitting.
                args.scope.$broadcast('formSubmittingFinalPhase', {
                    scope: args.scope,
                    action: args.action
                });
                // Set the form state to submitted
                currentForm.$setSubmitted();
                //then check if the form is valid
                if (!args.skipValidation) {
                    if (currentForm.$invalid) {
                        return false;
                    }
                }
                //reset the server validations if required (default is true), otherwise notify existing ones of changes
                if (!args.keepServerValidation) {
                    serverValidationManager.reset();
                } else {
                    serverValidationManager.notify();
                }
                return true;
            },
            /**
    * @ngdoc function
    * @name umbraco.services.formHelper#focusOnFirstError
    * @methodOf umbraco.services.formHelper
    * @function
    *
    * @description
    * Called by submitForm when a form has been submitted, it will fire a focus on the first found invalid umb-property it finds in the form..
    *
    * @param {object} form Pass in a form object.
    */
            focusOnFirstError: function focusOnFirstError(form) {
                var invalidNgForms = form.$$element.find('.umb-property ng-form.ng-invalid, .umb-property-editor ng-form.ng-invalid-required');
                var firstInvalidNgForm = invalidNgForms.first();
                if (firstInvalidNgForm.length !== 0) {
                    var focusableFields = _toConsumableArray(firstInvalidNgForm.find('umb-range-slider .noUi-handle,input,textarea,select,button'));
                    if (focusableFields.length !== 0) {
                        var firstErrorEl = focusableFields.find(function (el) {
                            return el.type !== 'hidden' && el.hasAttribute('readonly') === false;
                        });
                        if (firstErrorEl !== undefined) {
                            firstErrorEl.focus();
                        }
                    }
                }
            },
            /**
     * @ngdoc function
     * @name umbraco.services.formHelper#submitForm
     * @methodOf umbraco.services.formHelper
     * @function
     *
     * @description
     * Called by controllers when a form has been successfully submitted, this ensures the correct events are raised.
     *
     * @param {object} args An object containing arguments for form submission
     */
            resetForm: function resetForm(args) {
                var currentForm;
                if (!args) {
                    throw 'args cannot be null';
                }
                if (!args.scope) {
                    throw 'args.scope cannot be null';
                }
                if (!args.formCtrl) {
                    //try to get the closest form controller
                    currentForm = angularHelper.getRequiredCurrentForm(args.scope);
                } else {
                    currentForm = args.formCtrl;
                }
                // Set the form state to pristine
                currentForm.$setPristine();
                currentForm.$setUntouched();
                args.scope.$broadcast(args.hasErrors ? 'formSubmittedValidationFailed' : 'formSubmitted', { scope: args.scope });
            },
            showNotifications: function showNotifications(args) {
                if (!args || !args.notifications) {
                    return false;
                }
                if (Utilities.isArray(args.notifications)) {
                    for (var i = 0; i < args.notifications.length; i++) {
                        notificationsService.showNotification(args.notifications[i]);
                    }
                    return true;
                }
                return false;
            },
            /**
     * @ngdoc function
     * @name umbraco.services.formHelper#handleError
     * @methodOf umbraco.services.formHelper
     * @function
     *
     * @description
     * Needs to be called when a form submission fails, this will wire up all server validation errors in ModelState and
     * add the correct messages to the notifications. If a server error has occurred this will show a ysod.
     *
     * @param {object} err The error object returned from the http promise
     */
            handleError: function handleError(err) {
                //TODO: Potentially add in the logic to showNotifications like the contentEditingHelper.handleSaveError does so that
                // non content editors can just use this method instead of contentEditingHelper.handleSaveError which they should not use
                // and they won't need to manually do it.
                //When the status is a 400 status with a custom header: X-Status-Reason: Validation failed, we have validation errors.
                //Otherwise the error is probably due to invalid data (i.e. someone mucking around with the ids or something).
                //Or, some strange server error
                if (err.status === 400) {
                    //now we need to look through all the validation errors
                    if (err.data && err.data.ModelState) {
                        //wire up the server validation errs
                        this.handleServerValidation(err.data.ModelState);
                        //execute all server validation events and subscribers
                        serverValidationManager.notifyAndClearAllSubscriptions();
                    }
                } else {
                    // TODO: All YSOD handling should be done with an interceptor
                    overlayService.ysod(err);
                }
            },
            /**
     * @ngdoc function
     * @name umbraco.services.formHelper#handleServerValidation
     * @methodOf umbraco.services.formHelper
     * @function
     *
     * @description
     * This wires up all of the server validation model state so that valServer and valServerField directives work
     *
     * @param {object} err The error object returned from the http promise
     */
            handleServerValidation: function handleServerValidation(modelState) {
                serverValidationManager.addErrorsForModelState(modelState);
            }
        };
    }
    angular.module('umbraco.services').factory('formHelper', formHelper);
    'use strict';
    angular.module('umbraco.services').factory('gridService', function ($http, $q) {
        var configPath = Umbraco.Sys.ServerVariables.umbracoUrls.gridConfig;
        var service = {
            getGridEditors: function getGridEditors() {
                return $http.get(configPath);
            }
        };
        return service;
    });
    'use strict';
    angular.module('umbraco.services').factory('helpService', function ($http, $q, umbRequestHelper, dashboardResource) {
        var helpTopics = {};
        var defaultUrl = 'rss/help';
        var tvUrl = 'feeds/help';
        function getCachedHelp(url) {
            if (helpTopics[url]) {
                return helpTopics[cacheKey];
            } else {
                return null;
            }
        }
        function setCachedHelp(url, data) {
            helpTopics[url] = data;
        }
        function fetchUrl(site, url) {
            var deferred = $q.defer();
            var found = getCachedHelp(url);
            if (found) {
                deferred.resolve(found);
            } else {
                dashboardResource.getRemoteXmlData(site, url).then(function (data) {
                    var feed = $(data.data);
                    var topics = [];
                    $('item', feed).each(function (i, item) {
                        var topic = {};
                        topic.thumbnail = $(item).find('thumbnail').attr('url');
                        topic.title = $('title', item).text();
                        topic.link = $('guid', item).text();
                        topic.description = $('description', item).text();
                        topics.push(topic);
                    });
                    setCachedHelp(topics);
                    deferred.resolve(topics);
                }, function (exception) {
                    console.error('ex from remote data', exception);
                });
            }
            return deferred.promise;
        }
        var service = {
            findHelp: function findHelp(args) {
                var url = service.getUrl(defaultUrl, args);
                return fetchUrl('OUR', url);
            },
            findVideos: function findVideos(args) {
                var url = service.getUrl(tvUrl, args);
                return fetchUrl('TV', url);
            },
            getContextHelpForPage: function getContextHelpForPage(section, tree, baseurl) {
                var qs = '?section=' + section + '&tree=' + tree;
                if (tree) {
                    qs += '&tree=' + tree;
                }
                if (baseurl) {
                    qs += '&baseurl=' + encodeURIComponent(baseurl);
                }
                var url = umbRequestHelper.getApiUrl('helpApiBaseUrl', 'GetContextHelpForPage' + qs);
                return umbRequestHelper.resourcePromise($http.get(url), 'Failed to get lessons content');
            },
            getUrl: function getUrl(url, args) {
                return url + '?' + $.param(args);
            }
        };
        return service;
    });
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.historyService
 *
 * @requires $rootScope 
 * @requires $timeout
 * @requires angularHelper
 *	
 * @description
 * Service to handle the main application navigation history. Responsible for keeping track
 * of where a user navigates to, stores an icon, url and name in a collection, to make it easy
 * for the user to go back to a previous editor / action
 *
 * **Note:** only works with new angular-based editors, not legacy ones
 *
 * ##usage
 * To use, simply inject the historyService into any controller that needs it, and make
 * sure the umbraco.services module is accesible - which it should be by default.
 *
 * <pre>
 *      angular.module("umbraco").controller("my.controller". function(historyService){
 *         historyService.add({
 *								icon: "icon-class",
 *								name: "Editing 'articles',
 *								link: "/content/edit/1234"}
 *							);
 *      }); 
 * </pre> 
 */
    angular.module('umbraco.services').factory('historyService', function ($rootScope, $timeout, angularHelper, eventsService) {
        var nArray = [];
        function _add(item) {
            if (!item) {
                return null;
            }
            var listWithoutThisItem = _.reject(nArray, function (i) {
                return i.link === item.link;
            });
            //put it at the top and reassign
            listWithoutThisItem.splice(0, 0, item);
            nArray = listWithoutThisItem;
            return nArray[0];
        }
        return {
            /**
     * @ngdoc method
     * @name umbraco.services.historyService#add
     * @methodOf umbraco.services.historyService
     *
     * @description
     * Adds a given history item to the users history collection.
     *
     * @param {Object} item the history item
     * @param {String} item.icon icon css class for the list, ex: "icon-image", "icon-doc"
     * @param {String} item.link route to the editor, ex: "/content/edit/1234"
     * @param {String} item.name friendly name for the history listing
     * @returns {Object} history item object
     */
            add: function add(item) {
                var icon = item.icon || 'icon-file';
                angularHelper.safeApply($rootScope, function () {
                    var result = _add({
                        name: item.name,
                        icon: icon,
                        link: item.link,
                        time: new Date()
                    });
                    eventsService.emit('historyService.add', {
                        added: result,
                        all: nArray
                    });
                    return result;
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.historyService#remove
     * @methodOf umbraco.services.historyService
     *
     * @description
     * Removes a history item from the users history collection, given an index to remove from.
     *
     * @param {Int} index index to remove item from
     */
            remove: function remove(index) {
                angularHelper.safeApply($rootScope, function () {
                    var result = nArray.splice(index, 1);
                    eventsService.emit('historyService.remove', {
                        removed: result,
                        all: nArray
                    });
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.historyService#removeAll
     * @methodOf umbraco.services.historyService
     *
     * @description
     * Removes all history items from the users history collection
     */
            removeAll: function removeAll() {
                angularHelper.safeApply($rootScope, function () {
                    nArray = [];
                    eventsService.emit('historyService.removeAll');
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.historyService#getCurrent
     * @methodOf umbraco.services.historyService
     *
     * @description
     * Method to return the current history collection.
     *
     */
            getCurrent: function getCurrent() {
                return nArray;
            },
            /**
    * @ngdoc method
    * @name umbraco.services.historyService#getLastAccessedItemForSection
    * @methodOf umbraco.services.historyService
    *
    * @description
    * Method to return the item that was last accessed in the given section
    *
     * @param {string} sectionAlias Alias of the section to return the last accessed item for.
    */
            getLastAccessedItemForSection: function getLastAccessedItemForSection(sectionAlias) {
                for (var i = 0, len = nArray.length; i < len; i++) {
                    var item = nArray[i];
                    if (item.link.indexOf(sectionAlias + '/') === 0) {
                        return item;
                    }
                }
                return null;
            }
        };
    });
    'use strict';
    function _slicedToArray(arr, i) {
        return _arrayWithHoles(arr) || _iterableToArrayLimit(arr, i) || _nonIterableRest();
    }
    function _nonIterableRest() {
        throw new TypeError('Invalid attempt to destructure non-iterable instance');
    }
    function _iterableToArrayLimit(arr, i) {
        if (!(Symbol.iterator in Object(arr) || Object.prototype.toString.call(arr) === '[object Arguments]')) {
            return;
        }
        var _arr = [];
        var _n = true;
        var _d = false;
        var _e = undefined;
        try {
            for (var _i = arr[Symbol.iterator](), _s; !(_n = (_s = _i.next()).done); _n = true) {
                _arr.push(_s.value);
                if (i && _arr.length === i)
                    break;
            }
        } catch (err) {
            _d = true;
            _e = err;
        } finally {
            try {
                if (!_n && _i['return'] != null)
                    _i['return']();
            } finally {
                if (_d)
                    throw _e;
            }
        }
        return _arr;
    }
    function _arrayWithHoles(arr) {
        if (Array.isArray(arr))
            return arr;
    }
    /**
* @ngdoc service
* @name umbraco.services.iconHelper
* @description A helper service for dealing with icons, mostly dealing with legacy tree icons
**/
    function iconHelper($http, $q, $sce, $timeout) {
        var converter = [
            {
                oldIcon: '.sprNew',
                newIcon: 'add'
            },
            {
                oldIcon: '.sprDelete',
                newIcon: 'remove'
            },
            {
                oldIcon: '.sprMove',
                newIcon: 'enter'
            },
            {
                oldIcon: '.sprCopy',
                newIcon: 'documents'
            },
            {
                oldIcon: '.sprSort',
                newIcon: 'navigation-vertical'
            },
            {
                oldIcon: '.sprPublish',
                newIcon: 'globe'
            },
            {
                oldIcon: '.sprRollback',
                newIcon: 'undo'
            },
            {
                oldIcon: '.sprProtect',
                newIcon: 'lock'
            },
            {
                oldIcon: '.sprAudit',
                newIcon: 'time'
            },
            {
                oldIcon: '.sprNotify',
                newIcon: 'envelope'
            },
            {
                oldIcon: '.sprDomain',
                newIcon: 'home'
            },
            {
                oldIcon: '.sprPermission',
                newIcon: 'lock'
            },
            {
                oldIcon: '.sprRefresh',
                newIcon: 'refresh'
            },
            {
                oldIcon: '.sprBinEmpty',
                newIcon: 'trash'
            },
            {
                oldIcon: '.sprExportDocumentType',
                newIcon: 'download-alt'
            },
            {
                oldIcon: '.sprImportDocumentType',
                newIcon: 'page-up'
            },
            {
                oldIcon: '.sprLiveEdit',
                newIcon: 'edit'
            },
            {
                oldIcon: '.sprCreateFolder',
                newIcon: 'add'
            },
            {
                oldIcon: '.sprPackage2',
                newIcon: 'box'
            },
            {
                oldIcon: '.sprLogout',
                newIcon: 'logout'
            },
            {
                oldIcon: '.sprSave',
                newIcon: 'save'
            },
            {
                oldIcon: '.sprSendToTranslate',
                newIcon: 'envelope-alt'
            },
            {
                oldIcon: '.sprToPublish',
                newIcon: 'mail-forward'
            },
            {
                oldIcon: '.sprTranslate',
                newIcon: 'comments'
            },
            {
                oldIcon: '.sprUpdate',
                newIcon: 'save'
            },
            {
                oldIcon: '.sprTreeSettingDomain',
                newIcon: 'icon-home'
            },
            {
                oldIcon: '.sprTreeDoc',
                newIcon: 'icon-document'
            },
            {
                oldIcon: '.sprTreeDoc2',
                newIcon: 'icon-diploma-alt'
            },
            {
                oldIcon: '.sprTreeDoc3',
                newIcon: 'icon-notepad'
            },
            {
                oldIcon: '.sprTreeDoc4',
                newIcon: 'icon-newspaper-alt'
            },
            {
                oldIcon: '.sprTreeDoc5',
                newIcon: 'icon-notepad-alt'
            },
            {
                oldIcon: '.sprTreeDocPic',
                newIcon: 'icon-picture'
            },
            {
                oldIcon: '.sprTreeFolder',
                newIcon: 'icon-folder'
            },
            {
                oldIcon: '.sprTreeFolder_o',
                newIcon: 'icon-folder'
            },
            {
                oldIcon: '.sprTreeMediaFile',
                newIcon: 'icon-music'
            },
            {
                oldIcon: '.sprTreeMediaMovie',
                newIcon: 'icon-movie'
            },
            {
                oldIcon: '.sprTreeMediaPhoto',
                newIcon: 'icon-picture'
            },
            {
                oldIcon: '.sprTreeMember',
                newIcon: 'icon-user'
            },
            {
                oldIcon: '.sprTreeMemberGroup',
                newIcon: 'icon-users'
            },
            {
                oldIcon: '.sprTreeMemberType',
                newIcon: 'icon-users'
            },
            {
                oldIcon: '.sprTreeNewsletter',
                newIcon: 'icon-file-text-alt'
            },
            {
                oldIcon: '.sprTreePackage',
                newIcon: 'icon-box'
            },
            {
                oldIcon: '.sprTreeRepository',
                newIcon: 'icon-server-alt'
            },
            {
                oldIcon: '.sprTreeSettingDataType',
                newIcon: 'icon-autofill'
            },
            // TODO: Something needs to be done with the old tree icons that are commented out.
            /*
  { oldIcon: ".sprTreeSettingAgent", newIcon: "" },
  { oldIcon: ".sprTreeSettingCss", newIcon: "" },
  { oldIcon: ".sprTreeSettingCssItem", newIcon: "" },
    { oldIcon: ".sprTreeSettingDataTypeChild", newIcon: "" },
  { oldIcon: ".sprTreeSettingDomain", newIcon: "" },
  { oldIcon: ".sprTreeSettingLanguage", newIcon: "" },
  { oldIcon: ".sprTreeSettingScript", newIcon: "" },
  { oldIcon: ".sprTreeSettingTemplate", newIcon: "" },
  { oldIcon: ".sprTreeSettingXml", newIcon: "" },
  { oldIcon: ".sprTreeStatistik", newIcon: "" },
  { oldIcon: ".sprTreeUser", newIcon: "" },
  { oldIcon: ".sprTreeUserGroup", newIcon: "" },
  { oldIcon: ".sprTreeUserType", newIcon: "" },
  */
            {
                oldIcon: 'folder.png',
                newIcon: 'icon-folder'
            },
            {
                oldIcon: 'mediaphoto.gif',
                newIcon: 'icon-picture'
            },
            {
                oldIcon: 'mediafile.gif',
                newIcon: 'icon-document'
            },
            {
                oldIcon: '.sprTreeDeveloperCacheItem',
                newIcon: 'icon-box'
            },
            {
                oldIcon: '.sprTreeDeveloperCacheTypes',
                newIcon: 'icon-box'
            },
            {
                oldIcon: '.sprTreeDeveloperMacro',
                newIcon: 'icon-cogs'
            },
            {
                oldIcon: '.sprTreeDeveloperRegistry',
                newIcon: 'icon-windows'
            },
            {
                oldIcon: '.sprTreeDeveloperPython',
                newIcon: 'icon-linux'
            }
        ];
        var collectedIcons;
        var imageConverter = [{
                oldImage: 'contour.png',
                newIcon: 'icon-umb-contour'
            }];
        var iconCache = [];
        var promiseQueue = [];
        var resourceLoadStatus = 'none';
        /**
   * This is the same approach as use for loading the localized text json 
   * We don't want multiple requests for the icon collection, so need to track
   * the current request state, and resolve the queued requests once the icons arrive
   * Subsequent requests are returned immediately as the icons are cached into 
   */
        function init() {
            var deferred = $q.defer();
            if (resourceLoadStatus === 'loaded') {
                deferred.resolve(iconCache);
                return deferred.promise;
            }
            if (resourceLoadStatus === 'loading') {
                promiseQueue.push(deferred);
                return deferred.promise;
            }
            resourceLoadStatus = 'loading';
            $http({
                method: 'GET',
                url: Umbraco.Sys.ServerVariables.umbracoUrls.iconApiBaseUrl + 'GetIcons'
            }).then(function (response) {
                resourceLoadStatus = 'loaded';
                for (var _i = 0, _Object$entries = Object.entries(response.data.Data); _i < _Object$entries.length; _i++) {
                    var _Object$entries$_i = _slicedToArray(_Object$entries[_i], 2), key = _Object$entries$_i[0], value = _Object$entries$_i[1];
                    iconCache.push({
                        name: key,
                        svgString: $sce.trustAsHtml(value)
                    });
                }
                deferred.resolve(iconCache);
                //ensure all other queued promises are resolved
                for (var p in promiseQueue) {
                    promiseQueue[p].resolve(iconCache);
                }
            }, function (err) {
                deferred.reject('Something broke');
                //ensure all other queued promises are resolved
                for (var p in promiseQueue) {
                    promiseQueue[p].reject('Something broke');
                }
            });
            return deferred.promise;
        }
        return {
            /** Used by the create dialogs for content/media types to format the data so that the thumbnails are styled properly */
            formatContentTypeThumbnails: function formatContentTypeThumbnails(contentTypes) {
                for (var i = 0; i < contentTypes.length; i++) {
                    if (contentTypes[i].thumbnailIsClass === undefined || contentTypes[i].thumbnailIsClass) {
                        contentTypes[i].cssClass = this.convertFromLegacyIcon(contentTypes[i].thumbnail);
                    } else {
                        contentTypes[i].style = 'background-image: url(\'' + contentTypes[i].thumbnailFilePath + '\');height:36px; background-position:4px 0px; background-repeat: no-repeat;background-size: 35px 35px;';
                        //we need an 'icon-' class in there for certain styles to work so if it is image based we'll add this
                        contentTypes[i].cssClass = 'custom-file';
                    }
                }
                return contentTypes;
            },
            formatContentTypeIcons: function formatContentTypeIcons(contentTypes) {
                for (var i = 0; i < contentTypes.length; i++) {
                    if (!contentTypes[i].icon) {
                        //just to be safe (e.g. when focus was on close link and hitting save)
                        contentTypes[i].icon = 'icon-document';    // default icon
                    } else {
                        contentTypes[i].icon = this.convertFromLegacyIcon(contentTypes[i].icon);
                    }
                    //couldnt find replacement
                    if (contentTypes[i].icon.indexOf('.') > 0) {
                        contentTypes[i].icon = 'icon-document-dashed-line';
                    }
                }
                return contentTypes;
            },
            /** If the icon is file based (i.e. it has a file path) */
            isFileBasedIcon: function isFileBasedIcon(icon) {
                //if it doesn't start with a '.' but contains one then we'll assume it's file based
                if (icon.startsWith('..') || !icon.startsWith('.') && icon.indexOf('.') > 1) {
                    return true;
                }
                return false;
            },
            /** If the icon is legacy */
            isLegacyIcon: function isLegacyIcon(icon) {
                if (!icon) {
                    return false;
                }
                if (icon.startsWith('..')) {
                    return false;
                }
                if (icon.startsWith('.')) {
                    return true;
                }
                return false;
            },
            /** If the tree node has a legacy icon */
            isLegacyTreeNodeIcon: function isLegacyTreeNodeIcon(treeNode) {
                if (treeNode.iconIsClass) {
                    return this.isLegacyIcon(treeNode.icon);
                }
                return false;
            },
            /** Converts the icon from legacy to a new one if an old one is detected */
            convertFromLegacyIcon: function convertFromLegacyIcon(icon) {
                if (this.isLegacyIcon(icon)) {
                    //its legacy so convert it if we can
                    var found = _.find(converter, function (item) {
                        return item.oldIcon.toLowerCase() === icon.toLowerCase();
                    });
                    return found ? found.newIcon : icon;
                }
                return icon;
            },
            convertFromLegacyImage: function convertFromLegacyImage(icon) {
                var found = _.find(imageConverter, function (item) {
                    return item.oldImage.toLowerCase() === icon.toLowerCase();
                });
                return found ? found.newIcon : undefined;
            },
            /** If we detect that the tree node has legacy icons that can be converted, this will convert them */
            convertFromLegacyTreeNodeIcon: function convertFromLegacyTreeNodeIcon(treeNode) {
                if (this.isLegacyTreeNodeIcon(treeNode)) {
                    return this.convertFromLegacyIcon(treeNode.icon);
                }
                return treeNode.icon;
            },
            /** Gets a single IconModel */
            getIcon: function getIcon(iconName) {
                return init().then(function (icons) {
                    return icons.find(function (i) {
                        return i.name === iconName;
                    });
                });
            },
            /** Gets all the available icons in the backoffice icon folder and returns them as an array of IconModels */
            getAllIcons: function getAllIcons() {
                return init().then(function (icons) {
                    return icons;
                });
            },
            /** LEGACY - Return a list of icons from icon fonts, optionally filter them */
            /** It fetches them directly from the active stylesheets in the browser */
            getIcons: function getIcons() {
                var deferred = $q.defer();
                $timeout(function () {
                    if (collectedIcons) {
                        deferred.resolve(collectedIcons);
                    } else {
                        collectedIcons = [];
                        var c = '.icon-';
                        for (var i = document.styleSheets.length - 1; i >= 0; i--) {
                            var classes = null;
                            try {
                                classes = document.styleSheets[i].rules || document.styleSheets[i].cssRules;
                            } catch (e) {
                                console.warn('Can\'t read the css rules of: ' + document.styleSheets[i].href, e);
                                continue;
                            }
                            if (classes !== null) {
                                for (var x = 0; x < classes.length; x++) {
                                    var cur = classes[x];
                                    if (cur.selectorText && cur.selectorText.indexOf(c) === 0) {
                                        var s = cur.selectorText.substring(1);
                                        var hasSpace = s.indexOf(' ');
                                        if (hasSpace > 0) {
                                            s = s.substring(0, hasSpace);
                                        }
                                        var hasPseudo = s.indexOf(':');
                                        if (hasPseudo > 0) {
                                            s = s.substring(0, hasPseudo);
                                        }
                                        if (collectedIcons.indexOf(s) < 0) {
                                            collectedIcons.push(s);
                                        }
                                    }
                                }
                            }
                        }
                        deferred.resolve(collectedIcons);
                    }
                }, 100);
                return deferred.promise;
            },
            /** Creates a icon object, and caches it in a runtime cache */
            defineIcon: function defineIcon(name, svg) {
                var icon = iconCache.find(function (x) {
                    return x.name === name;
                });
                if (icon === undefined) {
                    icon = {
                        name: name,
                        svgString: $sce.trustAsHtml(svg)
                    };
                    iconCache.push(icon);
                }
                return icon;
            },
            /** Returns the cached icon or undefined */
            _getIconFromCache: function _getIconFromCache(iconName) {
                return iconCache.find(function (icon) {
                    return icon.name === iconName;
                });
            }
        };
    }
    angular.module('umbraco.services').factory('iconHelper', iconHelper);
    'use strict';
    /**
* @ngdoc service
* @name umbraco.services.imageHelper
* @deprecated
**/
    function imageHelper(umbRequestHelper, mediaHelper) {
        return {
            /**
     * @ngdoc function
     * @name umbraco.services.imageHelper#getImagePropertyValue
     * @methodOf umbraco.services.imageHelper
     * @function    
     *
     * @deprecated
     */
            getImagePropertyValue: function getImagePropertyValue(options) {
                return mediaHelper.getImagePropertyValue(options);
            },
            /**
     * @ngdoc function
     * @name umbraco.services.imageHelper#getThumbnail
     * @methodOf umbraco.services.imageHelper
     * @function    
     *
     * @deprecated
     */
            getThumbnail: function getThumbnail(options) {
                return mediaHelper.getThumbnail(options);
            },
            /**
     * @ngdoc function
     * @name umbraco.services.imageHelper#scaleToMaxSize
     * @methodOf umbraco.services.imageHelper
     * @function    
     *
     * @deprecated
     */
            scaleToMaxSize: function scaleToMaxSize(maxSize, width, height) {
                return mediaHelper.scaleToMaxSize(maxSize, width, height);
            },
            /**
     * @ngdoc function
     * @name umbraco.services.imageHelper#getThumbnailFromPath
     * @methodOf umbraco.services.imageHelper
     * @function    
     *
     * @deprecated
     */
            getThumbnailFromPath: function getThumbnailFromPath(imagePath) {
                return mediaHelper.getThumbnailFromPath(imagePath);
            },
            /**
     * @ngdoc function
     * @name umbraco.services.imageHelper#detectIfImageByExtension
     * @methodOf umbraco.services.imageHelper
     * @function    
     *
     * @deprecated
     */
            detectIfImageByExtension: function detectIfImageByExtension(imagePath) {
                return mediaHelper.detectIfImageByExtension(imagePath);
            }
        };
    }
    angular.module('umbraco.services').factory('imageHelper', imageHelper);
    'use strict';
    // This service was based on OpenJS library available in BSD License
    // http://www.openjs.com/scripts/events/keyboard_shortcuts/index.php
    function keyboardService($window, $timeout) {
        var keyboardManagerService = {};
        var defaultOpt = {
            'type': 'keydown',
            'propagate': false,
            'inputDisabled': false,
            'target': $window.document,
            'keyCode': false
        };
        // Work around for stupid Shift key bug created by using lowercase - as a result the shift+num combination was broken
        var shift_nums = {
            '`': '~',
            '1': '!',
            '2': '@',
            '3': '#',
            '4': '$',
            '5': '%',
            '6': '^',
            '7': '&',
            '8': '*',
            '9': '(',
            '0': ')',
            '-': '_',
            '=': '+',
            ';': ':',
            '\'': '"',
            ',': '<',
            '.': '>',
            '/': '?',
            '\\': '|'
        };
        // Special Keys - and their codes
        var special_keys = {
            'esc': 27,
            'escape': 27,
            'tab': 9,
            'space': 32,
            'return': 13,
            'enter': 13,
            'backspace': 8,
            'scrolllock': 145,
            'scroll_lock': 145,
            'scroll': 145,
            'capslock': 20,
            'caps_lock': 20,
            'caps': 20,
            'numlock': 144,
            'num_lock': 144,
            'num': 144,
            'pause': 19,
            'break': 19,
            'insert': 45,
            'home': 36,
            'delete': 46,
            'end': 35,
            'pageup': 33,
            'page_up': 33,
            'pu': 33,
            'pagedown': 34,
            'page_down': 34,
            'pd': 34,
            'left': 37,
            'up': 38,
            'right': 39,
            'down': 40,
            'f1': 112,
            'f2': 113,
            'f3': 114,
            'f4': 115,
            'f5': 116,
            'f6': 117,
            'f7': 118,
            'f8': 119,
            'f9': 120,
            'f10': 121,
            'f11': 122,
            'f12': 123
        };
        var isMac = navigator.platform.toUpperCase().indexOf('MAC') >= 0;
        // The event handler for bound element events
        function eventHandler(e) {
            e = e || $window.event;
            var code, k;
            // Find out which key is pressed
            if (e.keyCode) {
                code = e.keyCode;
            } else if (e.which) {
                code = e.which;
            }
            var character = String.fromCharCode(code).toLowerCase();
            if (code === 188) {
                character = ',';
            }
            // If the user presses , when the type is onkeydown
            if (code === 190) {
                character = '.';
            }
            // If the user presses , when the type is onkeydown
            var propagate = true;
            //Now we need to determine which shortcut this event is for, we'll do this by iterating over each 
            //registered shortcut to find the match. We use Find here so that the loop exits as soon
            //as we've found the one we're looking for
            _.find(_.keys(keyboardManagerService.keyboardEvent), function (key) {
                var shortcutLabel = key;
                var shortcutVal = keyboardManagerService.keyboardEvent[key];
                // Key Pressed - counts the number of valid keypresses - if it is same as the number of keys, the shortcut function is invoked
                var kp = 0;
                // Some modifiers key
                var modifiers = {
                    shift: {
                        wanted: false,
                        pressed: e.shiftKey ? true : false
                    },
                    ctrl: {
                        wanted: false,
                        pressed: e.ctrlKey ? true : false
                    },
                    alt: {
                        wanted: false,
                        pressed: e.altKey ? true : false
                    },
                    meta: {
                        //Meta is Mac specific
                        wanted: false,
                        pressed: e.metaKey ? true : false
                    }
                };
                var keys = shortcutLabel.split('+');
                var opt = shortcutVal.opt;
                var callback = shortcutVal.callback;
                // Foreach keys in label (split on +)
                var l = keys.length;
                for (var i = 0; i < l; i++) {
                    var k = keys[i];
                    switch (k) {
                    case 'ctrl':
                    case 'control':
                        kp++;
                        modifiers.ctrl.wanted = true;
                        break;
                    case 'shift':
                    case 'alt':
                    case 'meta':
                        kp++;
                        modifiers[k].wanted = true;
                        break;
                    }
                    if (k.length > 1) {
                        // If it is a special key
                        if (special_keys[k] === code) {
                            kp++;
                        }
                    } else if (opt['keyCode']) {
                        // If a specific key is set into the config
                        if (opt['keyCode'] === code) {
                            kp++;
                        }
                    } else {
                        // The special keys did not match
                        if (character === k) {
                            kp++;
                        } else {
                            if (shift_nums[character] && e.shiftKey) {
                                // Stupid Shift key bug created by using lowercase
                                character = shift_nums[character];
                                if (character === k) {
                                    kp++;
                                }
                            }
                        }
                    }
                }
                //for end
                if (kp === keys.length && modifiers.ctrl.pressed === modifiers.ctrl.wanted && modifiers.shift.pressed === modifiers.shift.wanted && modifiers.alt.pressed === modifiers.alt.wanted && modifiers.meta.pressed === modifiers.meta.wanted) {
                    //found the right callback!
                    // Disable event handler when focus input and textarea
                    if (opt['inputDisabled']) {
                        var elt;
                        if (e.target) {
                            elt = e.target;
                        } else if (e.srcElement) {
                            elt = e.srcElement;
                        }
                        if (elt.nodeType === 3) {
                            elt = elt.parentNode;
                        }
                        if (elt.tagName === 'INPUT' || elt.tagName === 'TEXTAREA' || elt.hasAttribute('disable-hotkeys')) {
                            //This exits the Find loop
                            return true;
                        }
                    }
                    $timeout(function () {
                        callback(e);
                    }, 1);
                    if (!opt['propagate']) {
                        // Stop the event
                        propagate = false;
                    }
                    //This exits the Find loop
                    return true;
                }
                //we haven't found one so continue looking
                return false;
            });
            // Stop the event if required
            if (!propagate) {
                // e.cancelBubble is supported by IE - this will kill the bubbling process.
                e.cancelBubble = true;
                e.returnValue = false;
                // e.stopPropagation works in Firefox.
                if (e.stopPropagation) {
                    e.stopPropagation();
                    e.preventDefault();
                }
                return false;
            }
        }
        // Store all keyboard combination shortcuts
        keyboardManagerService.keyboardEvent = {};
        // Add a new keyboard combination shortcut
        keyboardManagerService.bind = function (label, callback, opt) {
            //replace ctrl key with meta key
            if (isMac && label !== 'ctrl+space') {
                label = label.replace('ctrl', 'meta');
            }
            var elt;
            // Initialize opt object
            opt = angular.extend({}, defaultOpt, opt);
            label = label.toLowerCase();
            elt = opt.target;
            if (typeof opt.target === 'string') {
                elt = document.getElementById(opt.target);
            }
            //Ensure we aren't double binding to the same element + type otherwise we'll end up multi-binding
            // and raising events for now reason. So here we'll check if the event is already registered for the element
            var boundValues = _.values(keyboardManagerService.keyboardEvent);
            var found = _.find(boundValues, function (i) {
                return i.target === elt && i.event === opt['type'];
            });
            // Store shortcut
            keyboardManagerService.keyboardEvent[label] = {
                'callback': callback,
                'target': elt,
                'opt': opt
            };
            if (!found) {
                //Attach the function with the event
                if (elt.addEventListener) {
                    elt.addEventListener(opt['type'], eventHandler, false);
                } else if (elt.attachEvent) {
                    elt.attachEvent('on' + opt['type'], eventHandler);
                } else {
                    elt['on' + opt['type']] = eventHandler;
                }
            }
        };
        // Remove the shortcut - just specify the shortcut and I will remove the binding
        keyboardManagerService.unbind = function (label) {
            label = label.toLowerCase();
            var binding = keyboardManagerService.keyboardEvent[label];
            delete keyboardManagerService.keyboardEvent[label];
            if (!binding) {
                return;
            }
            var type = binding['event'], elt = binding['target'], callback = binding['callback'];
            if (elt.detachEvent) {
                elt.detachEvent('on' + type, callback);
            } else if (elt.removeEventListener) {
                elt.removeEventListener(type, callback, false);
            } else {
                elt['on' + type] = false;
            }
        };
        //
        return keyboardManagerService;
    }
    angular.module('umbraco.services').factory('keyboardService', [
        '$window',
        '$timeout',
        keyboardService
    ]);
    'use strict';
    /**
 @ngdoc service
 * @name umbraco.services.listViewHelper
 *
 *
 * @description
 * Service for performing operations against items in the list view UI. Used by the built-in internal listviews
 * as well as custom listview.
 *
 * A custom listview is always used inside a wrapper listview, so there are a number of inherited values on its
 * scope by default:
 *
 * **$scope.selection**: Array containing all items currently selected in the listview
 *
 * **$scope.items**: Array containing all items currently displayed in the listview
 *
 * **$scope.folders**: Array containing all folders in the current listview (only for media)
 *
 * **$scope.options**: configuration object containing information such as pagesize, permissions, order direction etc.
 *
 * **$scope.model.config.layouts**: array of available layouts to apply to the listview (grid, list or custom layout)
 *
 * ##Usage##
 * To use, inject listViewHelper into custom listview controller, listviewhelper expects you
 * to pass in the full collection of items in the listview in several of its methods
 * this collection is inherited from the parent controller and is available on $scope.selection
 *
 * <pre>
 *      angular.module("umbraco").controller("my.listVieweditor". function($scope, listViewHelper){
 *
 *          //current items in the listview
 *          var items = $scope.items;
 *
 *          //current selection
 *          var selection = $scope.selection;
 *
 *          //deselect an item , $scope.selection is inherited, item is picked from inherited $scope.items
 *          listViewHelper.deselectItem(item, $scope.selection);
 *
 *          //test if all items are selected, $scope.items + $scope.selection are inherited
 *          listViewhelper.isSelectedAll($scope.items, $scope.selection);
 *      });
 * </pre>
 */
    (function () {
        'use strict';
        function listViewHelper($location, $rootScope, localStorageService, urlHelper, editorService) {
            var firstSelectedIndex = 0;
            var localStorageKey = 'umblistViewLayout';
            /**
    * @ngdoc method
    * @name umbraco.services.listViewHelper#getLayout
    * @methodOf umbraco.services.listViewHelper
    *
    * @description
    * Method for internal use, based on the collection of layouts passed, the method selects either
    * any previous layout from local storage, or picks the first allowed layout
    *
    * @param {Any} id The identifier of the current node or application displayed in the content editor
    * @param {Array} availableLayouts Array of all allowed layouts, available from $scope.model.config.layouts
    */
            function getLayout(id, availableLayouts) {
                var storedLayouts = [];
                if (localStorageService.get(localStorageKey)) {
                    storedLayouts = localStorageService.get(localStorageKey);
                }
                if (storedLayouts && storedLayouts.length > 0) {
                    for (var i = 0; storedLayouts.length > i; i++) {
                        var layout = storedLayouts[i];
                        if (isMatchingLayout(id, layout)) {
                            return setLayout(id, layout, availableLayouts);
                        }
                    }
                }
                return getFirstAllowedLayout(availableLayouts);
            }
            /**
    * @ngdoc method
    * @name umbraco.services.listViewHelper#setLayout
    * @methodOf umbraco.services.listViewHelper
    *
    * @description
    * Changes the current layout used by the listview to the layout passed in. Stores selection in localstorage
    *
    * @param {Any} id The identifier of the current node or application displayed in the content editor
    * @param {Object} selectedLayout Layout selected as the layout to set as the current layout
    * @param {Array} availableLayouts Array of all allowed layouts, available from $scope.model.config.layouts
    */
            function setLayout(id, selectedLayout, availableLayouts) {
                var activeLayout = {};
                var layoutFound = false;
                for (var i = 0; availableLayouts.length > i; i++) {
                    var layout = availableLayouts[i];
                    if (layout.path === selectedLayout.path) {
                        activeLayout = layout;
                        layout.active = true;
                        layoutFound = true;
                    } else {
                        layout.active = false;
                    }
                }
                if (!layoutFound) {
                    activeLayout = getFirstAllowedLayout(availableLayouts);
                }
                saveLayoutInLocalStorage(id, activeLayout);
                return activeLayout;
            }
            /**
    * @ngdoc method
    * @name umbraco.services.listViewHelper#saveLayoutInLocalStorage
    * @methodOf umbraco.services.listViewHelper
    *
    * @description
    * Stores a given layout as the current default selection in local storage
    *
    * @param {Any} id The identifier of the current node or application displayed in the content editor
    * @param {Object} selectedLayout Layout selected as the layout to set as the current layout
    */
            function saveLayoutInLocalStorage(id, selectedLayout) {
                var layoutFound = false;
                var storedLayouts = [];
                if (localStorageService.get(localStorageKey)) {
                    storedLayouts = localStorageService.get(localStorageKey);
                }
                if (storedLayouts.length > 0) {
                    for (var i = 0; storedLayouts.length > i; i++) {
                        var layout = storedLayouts[i];
                        if (isMatchingLayout(id, layout)) {
                            layout.path = selectedLayout.path;
                            layoutFound = true;
                        }
                    }
                }
                if (!layoutFound) {
                    var storageObject = {
                        'id': id,
                        'path': selectedLayout.path
                    };
                    storedLayouts.push(storageObject);
                }
                localStorageService.set(localStorageKey, storedLayouts);
            }
            /**
    * @ngdoc method
    * @name umbraco.services.listViewHelper#getFirstAllowedLayout
    * @methodOf umbraco.services.listViewHelper
    *
    * @description
    * Returns currently selected layout, or alternatively the first layout in the available layouts collection
    *
    * @param {Array} layouts Array of all allowed layouts, available from $scope.model.config.layouts
    */
            function getFirstAllowedLayout(layouts) {
                var firstAllowedLayout = {};
                if (layouts != null) {
                    for (var i = 0; layouts.length > i; i++) {
                        var layout = layouts[i];
                        if (layout.selected === true) {
                            firstAllowedLayout = layout;
                            break;
                        }
                    }
                }
                return firstAllowedLayout;
            }
            /**
    * @ngdoc method
    * @name umbraco.services.listViewHelper#selectHandler
    * @methodOf umbraco.services.listViewHelper
    *
    * @description
    * Helper method for working with item selection via a checkbox, internally it uses selectItem and deselectItem.
    * Working with this method, requires its triggered via a checkbox which can then pass in its triggered $event
    * When the checkbox is clicked, this method will toggle selection of the associated item so it matches the state of the checkbox
    *
    * @param {Object} selectedItem Item being selected or deselected by the checkbox
    * @param {Number} selectedIndex Index of item being selected/deselected, usually passed as $index
    * @param {Array} items All items in the current listview, available as $scope.items
    * @param {Array} selection All selected items in the current listview, available as $scope.selection
    * @param {Event} $event Event triggered by the checkbox being checked to select / deselect an item
    */
            function selectHandler(selectedItem, selectedIndex, items, selection, $event) {
                var start = 0;
                var end = 0;
                var item = null;
                if ($event.shiftKey === true) {
                    if (selectedIndex > firstSelectedIndex) {
                        start = firstSelectedIndex;
                        end = selectedIndex;
                        for (; end >= start; start++) {
                            item = items[start];
                            selectItem(item, selection);
                        }
                    } else {
                        start = firstSelectedIndex;
                        end = selectedIndex;
                        for (; end <= start; start--) {
                            item = items[start];
                            selectItem(item, selection);
                        }
                    }
                } else {
                    if (selectedItem.selected) {
                        deselectItem(selectedItem, selection);
                    } else {
                        selectItem(selectedItem, selection);
                    }
                    firstSelectedIndex = selectedIndex;
                }
            }
            /**
    * @ngdoc method
    * @name umbraco.services.listViewHelper#selectItem
    * @methodOf umbraco.services.listViewHelper
    *
    * @description
    * Selects a given item to the listview selection array, requires you pass in the inherited $scope.selection collection
    *
    * @param {Object} item Item to select
    * @param {Array} selection Listview selection, available as $scope.selection
    */
            function selectItem(item, selection) {
                var isSelected = false;
                for (var i = 0; selection.length > i; i++) {
                    var selectedItem = selection[i];
                    // if item.id is 2147483647 (int.MaxValue) use item.key
                    if (item.id !== 2147483647 && item.id === selectedItem.id || item.key && item.key === selectedItem.key) {
                        isSelected = true;
                    }
                }
                if (!isSelected) {
                    var obj = { id: item.id };
                    if (item.key) {
                        obj.key = item.key;
                    }
                    selection.push(obj);
                    item.selected = true;
                    $rootScope.$broadcast('listView.itemsChanged', { items: selection });
                }
            }
            /**
    * @ngdoc method
    * @name umbraco.services.listViewHelper#deselectItem
    * @methodOf umbraco.services.listViewHelper
    *
    * @description
    * Deselects a given item from the listviews selection array, requires you pass in the inherited $scope.selection collection
    *
    * @param {Object} item Item to deselect
    * @param {Array} selection Listview selection, available as $scope.selection
    */
            function deselectItem(item, selection) {
                for (var i = 0; selection.length > i; i++) {
                    var selectedItem = selection[i];
                    // if item.id is 2147483647 (int.MaxValue) use item.key
                    if (item.id !== 2147483647 && item.id === selectedItem.id || item.key && item.key === selectedItem.key) {
                        selection.splice(i, 1);
                        item.selected = false;
                        $rootScope.$broadcast('listView.itemsChanged', { items: selection });
                    }
                }
            }
            /**
    * @ngdoc method
    * @name umbraco.services.listViewHelper#clearSelection
    * @methodOf umbraco.services.listViewHelper
    *
    * @description
    * Removes a given number of items and folders from the listviews selection array
    * Folders can only be passed in if the listview is used in the media section which has a concept of folders.
    *
    * @param {Array} items Items to remove, can be null
    * @param {Array} folders Folders to remove, can be null
    * @param {Array} selection Listview selection, available as $scope.selection
    */
            function clearSelection(items, folders, selection) {
                var i = 0;
                selection.length = 0;
                if (Utilities.isArray(items)) {
                    for (i = 0; items.length > i; i++) {
                        var item = items[i];
                        item.selected = false;
                    }
                }
                if (Utilities.isArray(folders)) {
                    for (i = 0; folders.length > i; i++) {
                        var folder = folders[i];
                        folder.selected = false;
                    }
                }
                $rootScope.$broadcast('listView.itemsChanged', { items: selection });
            }
            /**
    * @ngdoc method
    * @name umbraco.services.listViewHelper#selectAllItems
    * @methodOf umbraco.services.listViewHelper
    *
    * @description
    * Helper method for toggling the select state on all items in the active listview
    * Can only be used from a checkbox as a checkbox $event is required to pass in.
    *
    * @param {Array} items Items to toggle selection on, should be $scope.items
    * @param {Array} selection Listview selection, available as $scope.selection
    * @param {$event} $event Event passed from the checkbox being toggled
    */
            function selectAllItems(items, selection, $event) {
                var checkbox = $event.target;
                var clearSelection = false;
                if (!Utilities.isArray(items)) {
                    return;
                }
                selection.length = 0;
                for (var i = 0; i < items.length; i++) {
                    var item = items[i];
                    var obj = { id: item.id };
                    if (item.key) {
                        obj.key = item.key;
                    }
                    if (checkbox.checked) {
                        selection.push(obj);
                    } else {
                        clearSelection = true;
                    }
                    item.selected = checkbox.checked;
                }
                if (clearSelection) {
                    selection.length = 0;
                }
                $rootScope.$broadcast('listView.itemsChanged', { items: selection });
            }
            /**
    * @ngdoc method
    * @name umbraco.services.listViewHelper#selectAllItemsToggle
    * @methodOf umbraco.services.listViewHelper
    *
    * @description
    * Helper method for toggling the select state on all items.
    *
    * @param {Array} items Items to toggle selection on, should be $scope.items
    * @param {Array} selection Listview selection, available as $scope.selection
    */
            function selectAllItemsToggle(items, selection) {
                if (!Utilities.isArray(items)) {
                    return;
                }
                if (isSelectedAll(items, selection)) {
                    // unselect all items
                    items.forEach(function (item) {
                        item.selected = false;
                    });
                    // reset selection without loosing reference.
                    selection.length = 0;
                } else {
                    // reset selection without loosing reference.
                    selection.length = 0;
                    // select all items
                    items.forEach(function (item) {
                        var obj = { id: item.id };
                        if (item.key) {
                            obj.key = item.key;
                        }
                        item.selected = true;
                        selection.push(obj);
                    });
                }
                $rootScope.$broadcast('listView.itemsChanged', { items: selection });
            }
            /**
    * @ngdoc method
    * @name umbraco.services.listViewHelper#isSelectedAll
    * @methodOf umbraco.services.listViewHelper
    *
    * @description
    * Method to determine if all items on the current page in the list has been selected
    * Given the current items in the view, and the current selection, it will return true/false
    *
    * @param {Array} items Items to test if all are selected, should be $scope.items
    * @param {Array} selection Listview selection, available as $scope.selection
    * @returns {Boolean} boolean indicate if all items in the listview have been selected
    */
            function isSelectedAll(items, selection) {
                var numberOfSelectedItem = 0;
                for (var itemIndex = 0; items.length > itemIndex; itemIndex++) {
                    var item = items[itemIndex];
                    for (var selectedIndex = 0; selection.length > selectedIndex; selectedIndex++) {
                        var selectedItem = selection[selectedIndex];
                        // if item.id is 2147483647 (int.MaxValue) use item.key
                        if (item.id !== 2147483647 && item.id === selectedItem.id || item.key && item.key === selectedItem.key) {
                            numberOfSelectedItem++;
                        }
                    }
                }
                if (numberOfSelectedItem === items.length) {
                    return true;
                }
            }
            /**
    * @ngdoc method
    * @name umbraco.services.listViewHelper#setSortingDirection
    * @methodOf umbraco.services.listViewHelper
    *
    * @description
    * *Internal* method for changing sort order icon
    * @param {String} col Column alias to order after
    * @param {String} direction Order direction `asc` or `desc`
    * @param {Object} options object passed from the parent listview available as $scope.options
    */
            function setSortingDirection(col, direction, options) {
                return options.orderBy.toUpperCase() === col.toUpperCase() && options.orderDirection === direction;
            }
            /**
    * @ngdoc method
    * @name umbraco.services.listViewHelper#setSorting
    * @methodOf umbraco.services.listViewHelper
    *
    * @description
    * Method for setting the field on which the listview will order its items after.
    *
    * @param {String} field Field alias to order after
    * @param {Boolean} allow Determines if the user is allowed to set this field, normally true
    * @param {Object} options Options object passed from the parent listview available as $scope.options
    */
            function setSorting(field, allow, options) {
                if (allow) {
                    if (options.orderBy === field && options.orderDirection === 'asc') {
                        options.orderDirection = 'desc';
                    } else {
                        options.orderDirection = 'asc';
                    }
                    options.orderBy = field;
                }
            }
            //This takes in a dictionary of Ids with Permissions and determines
            // the intersect of all permissions to return an object representing the
            // listview button permissions
            function getButtonPermissions(unmergedPermissions, currentIdsWithPermissions) {
                if (currentIdsWithPermissions == null) {
                    currentIdsWithPermissions = {};
                }
                //merge the newly retrieved permissions to the main dictionary
                _.each(unmergedPermissions, function (value, key, list) {
                    currentIdsWithPermissions[key] = value;
                });
                //get the intersect permissions
                var arr = [];
                _.each(currentIdsWithPermissions, function (value, key, list) {
                    arr.push(value);
                });
                //we need to use 'apply' to call intersection with an array of arrays,
                //see: https://stackoverflow.com/a/16229480/694494
                var intersectPermissions = _.intersection.apply(_, arr);
                return {
                    canCopy: _.contains(intersectPermissions, 'O'),
                    //Magic Char = O
                    canCreate: _.contains(intersectPermissions, 'C'),
                    //Magic Char = C
                    canDelete: _.contains(intersectPermissions, 'D'),
                    //Magic Char = D
                    canMove: _.contains(intersectPermissions, 'M'),
                    //Magic Char = M
                    canPublish: _.contains(intersectPermissions, 'U'),
                    //Magic Char = U
                    canUnpublish: _.contains(intersectPermissions, 'U')    //Magic Char = Z (however UI says it can't be set, so if we can publish 'U' we can unpublish)
                };
            }
            /**
    * @ngdoc method
    * @name umbraco.services.listViewHelper#editItem
    * @methodOf umbraco.services.listViewHelper
    *
    * @description
    * Method for opening an item in a list view for editing.
    *
    * @param {Object} item The item to edit
    * @param {Object} scope The scope with options
    */
            function editItem(item, scope) {
                if (!item.editPath) {
                    return;
                }
                if (scope && scope.options && scope.options.useInfiniteEditor) {
                    var editorModel = {
                        id: item.id,
                        submit: function submit(model) {
                            editorService.close();
                            scope.getContent(scope.contentId);
                        },
                        close: function close() {
                            editorService.close();
                            scope.getContent(scope.contentId);
                        }
                    };
                    if (item.editPath.indexOf('/content/') == 0) {
                        editorService.contentEditor(editorModel);
                        return;
                    }
                    if (item.editPath.indexOf('/media/') == 0) {
                        editorService.mediaEditor(editorModel);
                        return;
                    }
                    if (item.editPath.indexOf('/member/') == 0) {
                        editorModel.id = item.key;
                        editorService.memberEditor(editorModel);
                        return;
                    }
                }
                var parts = item.editPath.split('?');
                var path = parts[0];
                var params = parts[1] ? urlHelper.getQueryStringParams('?' + parts[1]) : {};
                $location.path(path);
                for (var p in params) {
                    $location.search(p, params[p]);
                }
            }
            function isMatchingLayout(id, layout) {
                // legacy format uses "nodeId", be sure to look for both
                return layout.id === id || layout.nodeId === id;
            }
            var service = {
                getLayout: getLayout,
                getFirstAllowedLayout: getFirstAllowedLayout,
                setLayout: setLayout,
                saveLayoutInLocalStorage: saveLayoutInLocalStorage,
                selectHandler: selectHandler,
                selectItem: selectItem,
                deselectItem: deselectItem,
                clearSelection: clearSelection,
                selectAllItems: selectAllItems,
                selectAllItemsToggle: selectAllItemsToggle,
                isSelectedAll: isSelectedAll,
                setSortingDirection: setSortingDirection,
                setSorting: setSorting,
                getButtonPermissions: getButtonPermissions,
                editItem: editItem
            };
            return service;
        }
        angular.module('umbraco.services').factory('listViewHelper', listViewHelper);
    }());
    'use strict';
    /**
 @ngdoc service
 * @name umbraco.services.listViewPrevalueHelper
 *
 *
 * @description
 * Service for accessing the prevalues of a list view being edited in the inline list view editor in the doctype editor
 */
    (function () {
        'use strict';
        function listViewPrevalueHelper() {
            var prevalues = [];
            /**
    * @ngdoc method
    * @name umbraco.services.listViewPrevalueHelper#getPrevalues
    * @methodOf umbraco.services.listViewPrevalueHelper
    *
    * @description
    * Set the collection of prevalues
    */
            function getPrevalues() {
                return prevalues;
            }
            /**
    * @ngdoc method
    * @name umbraco.services.listViewPrevalueHelper#setPrevalues
    * @methodOf umbraco.services.listViewPrevalueHelper
    *
    * @description
    * Changes the current layout used by the listview to the layout passed in. Stores selection in localstorage
    *
    * @param {Array} values Array of prevalues
    */
            function setPrevalues(values) {
                prevalues = values;
            }
            var service = {
                getPrevalues: getPrevalues,
                setPrevalues: setPrevalues
            };
            return service;
        }
        angular.module('umbraco.services').factory('listViewPrevalueHelper', listViewPrevalueHelper);
    }());
    'use strict';
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
    angular.module('umbraco.services').factory('localizationService', function ($http, $q, eventsService, $window, $filter, userService) {
        // TODO: This should be injected as server vars
        var url = 'LocalizedText';
        var resourceFileLoadStatus = 'none';
        var resourceLoadingPromise = [];
        // array to hold the localized resource string entries
        var innerDictionary = [];
        function _lookup(alias, tokens, dictionary, fallbackValue) {
            //strip the key identifier if its there
            if (alias && alias[0] === '@') {
                alias = alias.substring(1);
            }
            var underscoreIndex = alias.indexOf('_');
            //if no area specified, add general_
            if (alias && underscoreIndex < 0) {
                alias = 'general_' + alias;
                underscoreIndex = alias.indexOf('_');
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
            if (fallbackValue)
                return fallbackValue;
            return '[' + alias + ']';
        }
        var service = {
            // loads the language resource file from the server
            initLocalizedResources: function initLocalizedResources() {
                // TODO: This promise handling is super ugly, we should just be returnning the promise from $http and returning inner values. 
                var deferred = $q.defer();
                if (resourceFileLoadStatus === 'loaded') {
                    deferred.resolve(innerDictionary);
                    return deferred.promise;
                }
                //if the resource is already loading, we don't want to force it to load another one in tandem, we'd rather
                // wait for that initial http promise to finish and then return this one with the dictionary loaded
                if (resourceFileLoadStatus === 'loading') {
                    //add to the list of promises waiting
                    resourceLoadingPromise.push(deferred);
                    //exit now it's already loading
                    return deferred.promise;
                }
                resourceFileLoadStatus = 'loading';
                // build the url to retrieve the localized resource file
                $http({
                    method: 'GET',
                    url: url,
                    cache: false
                }).then(function (response) {
                    resourceFileLoadStatus = 'loaded';
                    innerDictionary = response.data;
                    eventsService.emit('localizationService.updated', response.data);
                    deferred.resolve(response.data);
                    //ensure all other queued promises are resolved
                    for (var p in resourceLoadingPromise) {
                        resourceLoadingPromise[p].resolve(response.data);
                    }
                }, function (err) {
                    deferred.reject('Something broke');
                    //ensure all other queued promises are resolved
                    for (var p in resourceLoadingPromise) {
                        resourceLoadingPromise[p].reject('Something broke');
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
            tokenize: function tokenize(value, scope) {
                if (value) {
                    var localizer = value.split(':');
                    var retval = {
                        tokens: undefined,
                        key: localizer[0].substring(0)
                    };
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
            tokenReplace: function tokenReplace(text, tokens) {
                if (tokens) {
                    for (var i = 0; i < tokens.length; i++) {
                        text = text.replace('%' + i + '%', tokens[i]);
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
            localize: function localize(value, tokens, fallbackValue) {
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
            localizeMany: function localizeMany(keys) {
                if (keys) {
                    //The LocalizationService.localize promises we want to resolve
                    var promises = [];
                    for (var i = 0; i < keys.length; i++) {
                        promises.push(service.localize(keys[i], undefined));
                    }
                    return $q.all(promises).then(function (localizedValues) {
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
            concat: function concat(keys) {
                if (keys) {
                    //The LocalizationService.localize promises we want to resolve
                    var promises = [];
                    for (var i = 0; i < keys.length; i++) {
                        promises.push(service.localize(keys[i], undefined));
                    }
                    return $q.all(promises).then(function (localizedValues) {
                        //Build a concat string by looping over the array of resolved promises/translations
                        var returnValue = '';
                        for (var j = 0; j < localizedValues.length; j++) {
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
            format: function format(keys, message) {
                if (keys) {
                    //The LocalizationService.localize promises we want to resolve
                    var promises = [];
                    for (var i = 0; i < keys.length; i++) {
                        promises.push(service.localize(keys[i], undefined));
                    }
                    return $q.all(promises).then(function (localizedValues) {
                        //Replace {0} and {1} etc in message with the localized values
                        for (var j = 0; j < localizedValues.length; j++) {
                            var token = '%' + j + '%';
                            var regex = new RegExp(token, 'g');
                            message = message.replace(regex, localizedValues[j]);
                        }
                        return message;
                    });
                }
            }
        };
        //This happens after login / auth and assets loading
        eventsService.on('app.authenticated', function () {
            resourceFileLoadStatus = 'none';
            resourceLoadingPromise = [];
        });
        // return the local instance when called
        return service;
    });
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.macroService
 *
 *  
 * @description
 * A service to return macro information such as generating syntax to insert a macro into an editor
 */
    function macroService() {
        return {
            /** parses the special macro syntax like <?UMBRACO_MACRO macroAlias="Map" /> and returns an object with the macro alias and it's parameters */
            parseMacroSyntax: function parseMacroSyntax(syntax) {
                //This regex will match an alias of anything except characters that are quotes or new lines (for legacy reasons, when new macros are created
                // their aliases are cleaned an invalid chars are stripped)
                var expression = /(<\?UMBRACO_MACRO (?:.+?)?macroAlias=["']([^\"\'\n\r]+?)["'][\s\S]+?)(\/>|>.*?<\/\?UMBRACO_MACRO>)/i;
                var match = expression.exec(syntax);
                if (!match || match.length < 3) {
                    return null;
                }
                var alias = match[2];
                //this will leave us with just the parameters
                var paramsChunk = match[1].trim().replace(new RegExp('UMBRACO_MACRO macroAlias=["\']' + alias + '["\']'), '').trim();
                var paramExpression = /(\w+?)=['\"]([\s\S]*?)['\"]/g;
                var paramMatch;
                var returnVal = {
                    macroAlias: alias,
                    macroParamsDictionary: {}
                };
                while (paramMatch = paramExpression.exec(paramsChunk)) {
                    returnVal.macroParamsDictionary[paramMatch[1]] = paramMatch[2];
                }
                return returnVal;
            },
            /**
     * @ngdoc function
     * @name umbraco.services.macroService#generateMacroSyntax
     * @methodOf umbraco.services.macroService
     * @function    
     *
     * @description
     * generates the syntax for inserting a macro into a rich text editor - this is the very old umbraco style syntax
     * 
     * @param {object} args an object containing the macro alias and it's parameter values
     */
            generateMacroSyntax: function generateMacroSyntax(args) {
                // <?UMBRACO_MACRO macroAlias="BlogListPosts" />
                var macroString = '<?UMBRACO_MACRO macroAlias="' + args.macroAlias + '" ';
                if (args.macroParamsDictionary) {
                    _.each(args.macroParamsDictionary, function (val, key) {
                        //check for null
                        val = val ? val : '';
                        //need to detect if the val is a string or an object
                        var keyVal;
                        if (Utilities.isString(val)) {
                            keyVal = key + '="' + (val ? val : '') + '" ';
                        } else {
                            //if it's not a string we'll send it through the json serializer
                            var json = Utilities.toJson(val);
                            //then we need to url encode it so that it's safe
                            var encoded = encodeURIComponent(json);
                            keyVal = key + '="' + encoded + '" ';
                        }
                        macroString += keyVal;
                    });
                }
                macroString += '/>';
                return macroString;
            },
            /**
     * @ngdoc function
     * @name umbraco.services.macroService#generateMvcSyntax
     * @methodOf umbraco.services.macroService
     * @function    
     *
     * @description
     * generates the syntax for inserting a macro into an mvc template
     * 
     * @param {object} args an object containing the macro alias and it's parameter values
     */
            generateMvcSyntax: function generateMvcSyntax(args) {
                var macroString = '@Umbraco.RenderMacro("' + args.macroAlias + '"';
                var hasParams = false;
                var paramString;
                if (args.macroParamsDictionary) {
                    paramString = ', new {';
                    _.each(args.macroParamsDictionary, function (val, key) {
                        hasParams = true;
                        var keyVal = key + '="' + (val ? val : '') + '", ';
                        paramString += keyVal;
                    });
                    //remove the last , 
                    paramString = paramString.trimEnd(', ');
                    paramString += '}';
                }
                if (hasParams) {
                    macroString += paramString;
                }
                macroString += ')';
                return macroString;
            },
            collectValueData: function collectValueData(macro, macroParams, renderingEngine) {
                var paramDictionary = {};
                var macroAlias = macro.alias;
                if (!macroAlias) {
                    throw 'The macro object does not contain an alias';
                }
                var syntax;
                _.each(macroParams, function (item) {
                    var val = item.value;
                    if (item.value !== null && item.value !== undefined && !_.isString(item.value)) {
                        try {
                            val = Utilities.toJson(val);
                        } catch (e) {
                        }
                    }
                    //each value needs to be xml escaped!! since the value get's stored as an xml attribute
                    paramDictionary[item.alias] = _.escape(val);
                });
                //get the syntax based on the rendering engine
                if (renderingEngine && renderingEngine === 'Mvc') {
                    syntax = this.generateMvcSyntax({
                        macroAlias: macroAlias,
                        macroParamsDictionary: paramDictionary
                    });
                } else {
                    syntax = this.generateMacroSyntax({
                        macroAlias: macroAlias,
                        macroParamsDictionary: paramDictionary
                    });
                }
                var macroObject = {
                    'macroParamsDictionary': paramDictionary,
                    'macroAlias': macroAlias,
                    'syntax': syntax
                };
                return macroObject;
            }
        };
    }
    angular.module('umbraco.services').factory('macroService', macroService);
    'use strict';
    /**
* @ngdoc service
* @name umbraco.services.mediaHelper
* @description A helper object used for dealing with media items
**/
    function mediaHelper(umbRequestHelper, $log) {
        //container of fileresolvers
        var _mediaFileResolvers = {};
        return {
            /**
     * @ngdoc function
     * @name umbraco.services.mediaHelper#getImagePropertyValue
     * @methodOf umbraco.services.mediaHelper
     * @function
     *
     * @description
     * Returns the file path associated with the media property if there is one
     *
     * @param {object} options Options object
     * @param {object} options.mediaModel The media object to retrieve the image path from
     * @param {object} options.imageOnly Optional, if true then will only return a path if the media item is an image
     */
            getMediaPropertyValue: function getMediaPropertyValue(options) {
                if (!options || !options.mediaModel) {
                    throw 'The options objet does not contain the required parameters: mediaModel';
                }
                //combine all props, TODO: we really need a better way then this
                var props = [];
                if (options.mediaModel.properties) {
                    props = options.mediaModel.properties;
                } else {
                    $(options.mediaModel.tabs).each(function (i, tab) {
                        props = props.concat(tab.properties);
                    });
                }
                var mediaRoot = Umbraco.Sys.ServerVariables.umbracoSettings.mediaPath;
                var imageProp = _.find(props, function (item) {
                    if (item.alias === 'umbracoFile') {
                        return true;
                    }
                    //this performs a simple check to see if we have a media file as value
                    //it doesnt catch everything, but better then nothing
                    if (Utilities.isString(item.value) && item.value.indexOf(mediaRoot) === 0) {
                        return true;
                    }
                    return false;
                });
                if (!imageProp) {
                    return '';
                }
                var mediaVal;
                //our default images might store one or many images (as csv)
                var split = imageProp.value.split(',');
                var self = this;
                mediaVal = _.map(split, function (item) {
                    return {
                        file: item,
                        isImage: self.detectIfImageByExtension(item)
                    };
                });
                //for now we'll just return the first image in the collection.
                // TODO: we should enable returning many to be displayed in the picker if the uploader supports many.
                if (mediaVal.length && mediaVal.length > 0) {
                    if (!options.imageOnly || options.imageOnly === true && mediaVal[0].isImage) {
                        return mediaVal[0].file;
                    }
                }
                return '';
            },
            /**
     * @ngdoc function
     * @name umbraco.services.mediaHelper#getImagePropertyValue
     * @methodOf umbraco.services.mediaHelper
     * @function
     *
     * @description
     * Returns the actual image path associated with the image property if there is one
     *
     * @param {object} options Options object
     * @param {object} options.imageModel The media object to retrieve the image path from
     */
            getImagePropertyValue: function getImagePropertyValue(options) {
                if (!options || !options.imageModel && !options.mediaModel) {
                    throw 'The options objet does not contain the required parameters: imageModel';
                }
                //required to support backwards compatibility.
                options.mediaModel = options.imageModel ? options.imageModel : options.mediaModel;
                options.imageOnly = true;
                return this.getMediaPropertyValue(options);
            },
            /**
     * @ngdoc function
     * @name umbraco.services.mediaHelper#getThumbnail
     * @methodOf umbraco.services.mediaHelper
     * @function
     *
     * @description
     * formats the display model used to display the content to the model used to save the content
     *
     * @param {object} options Options object
     * @param {object} options.imageModel The media object to retrieve the image path from
     */
            getThumbnail: function getThumbnail(options) {
                if (!options || !options.imageModel) {
                    throw 'The options objet does not contain the required parameters: imageModel';
                }
                var imagePropVal = this.getImagePropertyValue(options);
                if (imagePropVal !== '') {
                    return this.getThumbnailFromPath(imagePropVal);
                }
                return '';
            },
            registerFileResolver: function registerFileResolver(propertyEditorAlias, func) {
                _mediaFileResolvers[propertyEditorAlias] = func;
            },
            /**
     * @ngdoc function
     * @name umbraco.services.mediaHelper#resolveFileFromEntity
     * @methodOf umbraco.services.mediaHelper
     * @function
     *
     * @description
     * Gets the media file url for a media entity returned with the entityResource
     *
     * @param {object} mediaEntity A media Entity returned from the entityResource
     * @param {boolean} thumbnail Whether to return the thumbnail url or normal url
     */
            resolveFileFromEntity: function resolveFileFromEntity(mediaEntity, thumbnail) {
                var mediaPath = Utilities.isObject(mediaEntity.metaData) ? mediaEntity.metaData.MediaPath : null;
                if (!mediaPath) {
                    //don't throw since this image legitimately might not contain a media path, but output a warning
                    $log.warn('Cannot resolve the file url from the mediaEntity, it does not contain the required metaData');
                    return null;
                }
                if (thumbnail) {
                    if (this.detectIfImageByExtension(mediaPath)) {
                        return this.getThumbnailFromPath(mediaPath);
                    } else if (this.getFileExtension(mediaPath) === 'svg') {
                        return this.getThumbnailFromPath(mediaPath);
                    } else {
                        return null;
                    }
                } else {
                    return mediaPath;
                }
            },
            /**
     * @ngdoc function
     * @name umbraco.services.mediaHelper#resolveFile
     * @methodOf umbraco.services.mediaHelper
     * @function
     *
     * @description
     * Gets the media file url for a media object returned with the mediaResource
     *
     * @param {object} mediaEntity A media Entity returned from the entityResource
     * @param {boolean} thumbnail Whether to return the thumbnail url or normal url
     */
            /*jshint loopfunc: true */
            resolveFile: function resolveFile(mediaItem, thumbnail) {
                function iterateProps(props) {
                    var res = null;
                    for (var resolver in _mediaFileResolvers) {
                        var property = _.find(props, function (prop) {
                            return prop.editor === resolver;
                        });
                        if (property) {
                            res = _mediaFileResolvers[resolver](property, mediaItem, thumbnail);
                            break;
                        }
                    }
                    return res;
                }
                //we either have properties raw on the object, or spread out on tabs
                var result = '';
                if (mediaItem.properties) {
                    result = iterateProps(mediaItem.properties);
                } else if (mediaItem.tabs) {
                    for (var tab in mediaItem.tabs) {
                        if (mediaItem.tabs[tab].properties) {
                            result = iterateProps(mediaItem.tabs[tab].properties);
                            if (result) {
                                break;
                            }
                        }
                    }
                }
                return result;
            },
            /*jshint loopfunc: true */
            hasFilePropertyType: function hasFilePropertyType(mediaItem) {
                function iterateProps(props) {
                    var res = false;
                    for (var resolver in _mediaFileResolvers) {
                        var property = _.find(props, function (prop) {
                            return prop.editor === resolver;
                        });
                        if (property) {
                            res = true;
                            break;
                        }
                    }
                    return res;
                }
                //we either have properties raw on the object, or spread out on tabs
                var result = false;
                if (mediaItem.properties) {
                    result = iterateProps(mediaItem.properties);
                } else if (mediaItem.tabs) {
                    for (var tab in mediaItem.tabs) {
                        if (mediaItem.tabs[tab].properties) {
                            result = iterateProps(mediaItem.tabs[tab].properties);
                            if (result) {
                                break;
                            }
                        }
                    }
                }
                return result;
            },
            /**
     * @ngdoc function
     * @name umbraco.services.mediaHelper#scaleToMaxSize
     * @methodOf umbraco.services.mediaHelper
     * @function
     *
     * @description
     * Finds the corrct max width and max height, given maximum dimensions and keeping aspect ratios
     *
     * @param {number} maxSize Maximum width & height
     * @param {number} width Current width
     * @param {number} height Current height
     */
            scaleToMaxSize: function scaleToMaxSize(maxSize, width, height) {
                var retval = {
                    width: width,
                    height: height
                };
                var maxWidth = maxSize;
                // Max width for the image
                var maxHeight = maxSize;
                // Max height for the image
                var ratio = 0;
                // Used for aspect ratio
                // Check if the current width is larger than the max
                if (width > maxWidth) {
                    ratio = maxWidth / width;
                    // get ratio for scaling image
                    retval.width = maxWidth;
                    retval.height = height * ratio;
                    height = height * ratio;
                    // Reset height to match scaled image
                    width = width * ratio;    // Reset width to match scaled image
                }
                // Check if current height is larger than max
                if (height > maxHeight) {
                    ratio = maxHeight / height;
                    // get ratio for scaling image
                    retval.height = maxHeight;
                    retval.width = width * ratio;
                    width = width * ratio;    // Reset width to match scaled image
                }
                return retval;
            },
            /**
     * @ngdoc function
     * @name umbraco.services.mediaHelper#getThumbnailFromPath
     * @methodOf umbraco.services.mediaHelper
     * @function
     *
     * @description
     * Returns the path to the thumbnail version of a given media library image path
     *
     * @param {string} imagePath Image path, ex: /media/1234/my-image.jpg
     */
            getThumbnailFromPath: function getThumbnailFromPath(imagePath) {
                // Check if file is a svg
                if (this.getFileExtension(imagePath) === 'svg') {
                    return imagePath;
                }
                // Check if file is a svg
                if (this.getFileExtension(imagePath) === 'svg') {
                    return imagePath;
                }
                // If the path is not an image we cannot get a thumb
                if (!this.detectIfImageByExtension(imagePath)) {
                    return null;
                }
                //get the proxy url for big thumbnails (this ensures one is always generated)
                var thumbnailUrl = umbRequestHelper.getApiUrl('imagesApiBaseUrl', 'GetBigThumbnail', [{ originalImagePath: imagePath }]) + '&rnd=' + Math.random();
                return thumbnailUrl;
            },
            /**
     * @ngdoc function
     * @name umbraco.services.mediaHelper#detectIfImageByExtension
     * @methodOf umbraco.services.mediaHelper
     * @function
     *
     * @description
     * Returns true/false, indicating if the given path has an allowed image extension
     *
     * @param {string} imagePath Image path, ex: /media/1234/my-image.jpg
     */
            detectIfImageByExtension: function detectIfImageByExtension(imagePath) {
                if (!imagePath) {
                    return false;
                }
                var lowered = imagePath.toLowerCase();
                var ext = lowered.substr(lowered.lastIndexOf('.') + 1);
                return (',' + Umbraco.Sys.ServerVariables.umbracoSettings.imageFileTypes + ',').indexOf(',' + ext + ',') !== -1;
            },
            /**
     * @ngdoc function
     * @name umbraco.services.mediaHelper#formatFileTypes
     * @methodOf umbraco.services.mediaHelper
     * @function
     *
     * @description
     * Returns a string with correctly formated file types for ng-file-upload
     *
     * @param {string} file types, ex: jpg,png,tiff
     */
            formatFileTypes: function formatFileTypes(fileTypes) {
                var fileTypesArray = fileTypes.split(',');
                var newFileTypesArray = [];
                for (var i = 0; i < fileTypesArray.length; i++) {
                    var fileType = fileTypesArray[i].trim();
                    if (!fileType) {
                        continue;
                    }
                    if (fileType.indexOf('.') !== 0) {
                        fileType = '.'.concat(fileType);
                    }
                    newFileTypesArray.push(fileType);
                }
                return newFileTypesArray.join(',');
            },
            /**
     * @ngdoc function
     * @name umbraco.services.mediaHelper#getFileExtension
     * @methodOf umbraco.services.mediaHelper
     * @function
     *
     * @description
     * Returns file extension
     *
     * @param {string} filePath File path, ex /media/1234/my-image.jpg
     */
            getFileExtension: function getFileExtension(filePath) {
                if (!filePath) {
                    return null;
                }
                var lowered = filePath.toLowerCase();
                var ext = lowered.substr(lowered.lastIndexOf('.') + 1);
                return ext;
            }
        };
    }
    angular.module('umbraco.services').factory('mediaHelper', mediaHelper);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.mediaTypeHelper
 * @description A helper service for the media types
 **/
    function mediaTypeHelper(mediaTypeResource, $q) {
        var mediaTypeHelperService = {
            isFolderType: function isFolderType(mediaEntity) {
                if (!mediaEntity) {
                    throw 'mediaEntity is null';
                }
                if (!mediaEntity.contentTypeAlias) {
                    throw 'mediaEntity.contentTypeAlias is null';
                }
                //if you create a media type, which has an alias that ends with ...Folder then its a folder: ex: "secureFolder", "bannerFolder", "Folder"
                //this is the exact same logic that is performed in MediaController.GetChildFolders
                return mediaEntity.contentTypeAlias.endsWith('Folder');
            },
            getAllowedImagetypes: function getAllowedImagetypes(mediaId) {
                // TODO: This is horribly inneficient - why make one request per type!?
                //This should make a call to c# to get exactly what it's looking for instead of returning every single media type and doing 
                //some filtering on the client side.
                //This is also called multiple times when it's not needed! Example, when launching the media picker, this will be called twice 
                //which means we'll be making at least 6 REST calls to fetch each media type
                // Get All allowedTypes
                return mediaTypeResource.getAllowedTypes(mediaId).then(function (types) {
                    var allowedQ = types.map(function (type) {
                        return mediaTypeResource.getById(type.id);
                    });
                    // Get full list
                    return $q.all(allowedQ).then(function (fullTypes) {
                        // Find all the media types with an Image Cropper property editor
                        var filteredTypes = mediaTypeHelperService.getTypeWithEditor(fullTypes, ['Umbraco.ImageCropper']);
                        // If there is only one media type with an Image Cropper we will return this one
                        if (filteredTypes.length === 1) {
                            return filteredTypes;    // If there is more than one Image cropper, custom media types have been added, and we return all media types with and Image cropper or UploadField
                        } else {
                            return mediaTypeHelperService.getTypeWithEditor(fullTypes, [
                                'Umbraco.ImageCropper',
                                'Umbraco.UploadField'
                            ]);
                        }
                    });
                });
            },
            getTypeWithEditor: function getTypeWithEditor(types, editors) {
                return types.filter(function (mediatype) {
                    for (var i = 0; i < mediatype.groups.length; i++) {
                        var group = mediatype.groups[i];
                        for (var j = 0; j < group.properties.length; j++) {
                            var property = group.properties[j];
                            if (editors.indexOf(property.editor) !== -1) {
                                return mediatype;
                            }
                        }
                    }
                });
            }
        };
        return mediaTypeHelperService;
    }
    angular.module('umbraco.services').factory('mediaTypeHelper', mediaTypeHelper);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.umbracoMenuActions
 *
 * @requires q
 * @requires treeService
 *	
 * @description
 * Defines the methods that are called when menu items declare only an action to execute
 */
    function umbracoMenuActions(treeService, $location, navigationService, appState, localizationService, usersResource, umbRequestHelper, notificationsService) {
        return {
            'ExportMember': function ExportMember(args) {
                var url = umbRequestHelper.getApiUrl('memberApiBaseUrl', 'ExportMemberData', [{ key: args.entity.id }]);
                umbRequestHelper.downloadFile(url).then(function () {
                    localizationService.localize('speechBubbles_memberExportedSuccess').then(function (value) {
                        notificationsService.success(value);
                    });
                }, function (data) {
                    localizationService.localize('speechBubbles_memberExportedError').then(function (value) {
                        notificationsService.error(value);
                    });
                });
            },
            'DisableUser': function DisableUser(args) {
                localizationService.localize('defaultdialogs_confirmdisable').then(function (txtConfirmDisable) {
                    var currentMenuNode = UmbClientMgr.mainTree().getActionNode();
                    if (confirm(txtConfirmDisable + ' "' + args.entity.name + '"?\n\n')) {
                        usersResource.disableUser(args.entity.id).then(function () {
                            navigationService.syncTree({
                                tree: args.treeAlias,
                                path: [
                                    args.entity.parentId,
                                    args.entity.id
                                ],
                                forceReload: true
                            });
                        });
                    }
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.umbracoMenuActions#RefreshNode
     * @methodOf umbraco.services.umbracoMenuActions
     * @function
     *
     * @description
     * Clears all node children and then gets it's up-to-date children from the server and re-assigns them
     * @param {object} args An arguments object
     * @param {object} args.entity The basic entity being acted upon
     * @param {object} args.treeAlias The tree alias associated with this entity
     * @param {object} args.section The current section
     */
            'RefreshNode': function RefreshNode(args) {
                ////just in case clear any tree cache for this node/section
                //treeService.clearCache({
                //    cacheKey: "__" + args.section, //each item in the tree cache is cached by the section name
                //    childrenOf: args.entity.parentId //clear the children of the parent
                //});
                //since we're dealing with an entity, we need to attempt to find it's tree node, in the main tree
                // this action is purely a UI thing so if for whatever reason there is no loaded tree node in the UI
                // we can safely ignore this process.
                //to find a visible tree node, we'll go get the currently loaded root node from appState
                var treeRoot = appState.getTreeState('currentRootNode');
                if (treeRoot && treeRoot.root) {
                    var treeNode = treeService.getDescendantNode(treeRoot.root, args.entity.id, args.treeAlias);
                    if (treeNode) {
                        treeService.loadNodeChildren({
                            node: treeNode,
                            section: args.section
                        }).then(function () {
                            navigationService.hideMenu();
                        });
                    }
                }
            },
            /**
     * @ngdoc method
     * @name umbraco.services.umbracoMenuActions#CreateChildEntity
     * @methodOf umbraco.services.umbracoMenuActions
     * @function
     *
     * @description
     * This will re-route to a route for creating a new entity as a child of the current node
     * @param {object} args An arguments object
     * @param {object} args.entity The basic entity being acted upon
     * @param {object} args.treeAlias The tree alias associated with this entity
     * @param {object} args.section The current section
     */
            'CreateChildEntity': function CreateChildEntity(args) {
                navigationService.hideNavigation();
                var route = '/' + args.section + '/' + args.treeAlias + '/edit/' + args.entity.id;
                //change to new path
                $location.path(route).search({ create: true });
            }
        };
    }
    angular.module('umbraco.services').factory('umbracoMenuActions', umbracoMenuActions);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.navigationService
 *
 * @requires $rootScope
 * @requires $routeParams
 * @requires $location
 * @requires treeService
 * @requires sectionResource
 *
 * @description
 * Service to handle the main application navigation. Responsible for invoking the tree
 * Section navigation and search, and maintain their state for the entire application lifetime
 *
 */
    function navigationService($routeParams, $location, $q, $injector, eventsService, umbModelMapper, treeService, appState, backdropService) {
        //the promise that will be resolved when the navigation is ready
        var navReadyPromise = $q.defer();
        //the main tree's API reference, this is acquired when the tree has initialized
        var mainTreeApi = null;
        eventsService.on('app.navigationReady', function (e, args) {
            mainTreeApi = args.treeApi;
            navReadyPromise.resolve(mainTreeApi);
        });
        eventsService.on('appState.backdrop', function (e, args) {
            var element = $(args.element);
            element.addClass('above-backdrop');
        });
        //A list of query strings defined that when changed will not cause a reload of the route
        var nonRoutingQueryStrings = [
            'mculture',
            'cculture',
            'csegment',
            'lq',
            'sr'
        ];
        var retainedQueryStrings = ['mculture'];
        function setMode(mode) {
            switch (mode) {
            case 'tree':
                appState.setGlobalState('navMode', 'tree');
                appState.setGlobalState('showNavigation', true);
                appState.setMenuState('showMenu', false);
                appState.setMenuState('showMenuDialog', false);
                appState.setGlobalState('stickyNavigation', false);
                appState.setGlobalState('showTray', false);
                break;
            case 'menu':
                appState.setGlobalState('navMode', 'menu');
                appState.setGlobalState('showNavigation', true);
                appState.setMenuState('showMenu', true);
                appState.setMenuState('showMenuDialog', false);
                appState.setGlobalState('stickyNavigation', true);
                break;
            case 'dialog':
                appState.setGlobalState('navMode', 'dialog');
                appState.setGlobalState('stickyNavigation', true);
                appState.setGlobalState('showNavigation', true);
                appState.setMenuState('showMenu', false);
                appState.setMenuState('showMenuDialog', true);
                appState.setMenuState('allowHideMenuDialog', true);
                break;
            case 'search':
                appState.setGlobalState('navMode', 'search');
                appState.setGlobalState('stickyNavigation', false);
                appState.setGlobalState('showNavigation', true);
                appState.setMenuState('showMenu', false);
                appState.setSectionState('showSearchResults', true);
                appState.setMenuState('showMenuDialog', false);
                break;
            default:
                appState.setGlobalState('navMode', 'default');
                appState.setMenuState('showMenu', false);
                appState.setMenuState('showMenuDialog', false);
                appState.setMenuState('allowHideMenuDialog', true);
                appState.setSectionState('showSearchResults', false);
                appState.setGlobalState('stickyNavigation', false);
                appState.setGlobalState('showTray', false);
                appState.setMenuState('currentNode', null);
                if (appState.getGlobalState('isTablet') === true) {
                    appState.setGlobalState('showNavigation', false);
                }
                break;
            }
        }
        /**
   * Converts a string request path to a dictionary of route params
   * @param {any} requestPath
   */
        function pathToRouteParts(requestPath) {
            if (!Utilities.isString(requestPath)) {
                throw 'The value for requestPath is not a string';
            }
            var pathAndQuery = requestPath.split('#')[1];
            if (pathAndQuery) {
                if (pathAndQuery.indexOf('%253') || pathAndQuery.indexOf('%252')) {
                    pathAndQuery = decodeURIComponent(pathAndQuery);
                }
                var pathParts = pathAndQuery.split('?');
                var path = pathParts[0];
                var qry = pathParts.length === 1 ? '' : pathParts[1];
                var qryParts = qry.split('&');
                var result = { path: path };
                for (var i = 0; i < qryParts.length; i++) {
                    var keyVal = qryParts[i].split('=');
                    if (keyVal.length == 2) {
                        result[keyVal[0]] = keyVal[1];
                    }
                }
                return result;
            }
        }
        function closeBackdrop() {
            var aboveClass = 'above-backdrop';
            var leftColumn = $('#leftcolumn');
            var isLeftColumnOnTop = leftColumn.hasClass(aboveClass);
            if (isLeftColumnOnTop) {
                backdropService.close();
                leftColumn.removeClass(aboveClass);
            }
        }
        function showBackdrop() {
            var backDropOptions = { 'element': $('#leftcolumn')[0] };
            backdropService.open(backDropOptions);
        }
        var service = {
            /**
     * @ngdoc method
     * @name umbraco.services.navigationService#isRouteChangingNavigation
     * @methodOf umbraco.services.navigationService
     *
     * @description
     * Detects if the route param differences will cause a navigation/route change or if the route param differences are
     * only tracking state changes.
     * This is used for routing operations where "reloadOnSearch: false" or "reloadOnUrl: false", when detecting form dirty changes when navigating to a different page,
     * and when we are creating new entities and moving from a route with the ?create=true parameter to an ID based parameter once it's created.
     * @param {object} currUrlParams Either a string path or a dictionary of route parameters
     * @param {object} nextUrlParams Either a string path or a dictionary of route parameters
     */
            isRouteChangingNavigation: function isRouteChangingNavigation(currUrlParams, nextUrlParams) {
                if (Utilities.isString(currUrlParams)) {
                    currUrlParams = pathToRouteParts(currUrlParams);
                }
                if (Utilities.isString(nextUrlParams)) {
                    nextUrlParams = pathToRouteParts(nextUrlParams);
                }
                //check if there is a query string to indicate that a "soft redirect" is taking place, if so we are not changing navigation
                if (nextUrlParams.sr === true) {
                    return false;
                }
                var allowRoute = true;
                //The only time that we want to not route is if only any of the nonRoutingQueryStrings have changed/added.
                //If any of the other parts have changed we do not cancel
                var currRoutingKeys = _.difference(_.keys(currUrlParams), nonRoutingQueryStrings);
                var nextRoutingKeys = _.difference(_.keys(nextUrlParams), nonRoutingQueryStrings);
                var diff1 = _.difference(currRoutingKeys, nextRoutingKeys);
                var diff2 = _.difference(nextRoutingKeys, currRoutingKeys);
                //if the routing parameter keys are the same, we'll compare their values to see if any have changed and if so then the routing will be allowed.
                if (diff1.length === 0 && diff2.length === 0) {
                    var partsChanged = 0;
                    currRoutingKeys.forEach(function (k) {
                        if (currUrlParams[k] != nextUrlParams[k]) {
                            partsChanged++;
                        }
                    });
                    if (partsChanged === 0) {
                        allowRoute = false;    //nothing except our query strings changed, so don't continue routing
                    }
                }
                return allowRoute;
            },
            /**
     * @ngdoc method
     * @name umbraco.services.navigationService#waitForNavReady
     * @methodOf umbraco.services.navigationService
     *
     * @description
     * returns a promise that will resolve when the navigation is ready
     */
            waitForNavReady: function waitForNavReady() {
                return navReadyPromise.promise;
            },
            /**
     * @ngdoc method
     * @name umbraco.services.navigationService#clearSearch
     * @methodOf umbraco.services.navigationService
     *
     * @description
     * utility to clear the querystring/search params while maintaining a known list of parameters that should be maintained throughout the app
     */
            clearSearch: function clearSearch(toRetain) {
                var toRetain = _.union(retainedQueryStrings, toRetain);
                var currentSearch = $location.search();
                $location.search('');
                toRetain.forEach(function (k) {
                    if (currentSearch[k]) {
                        $location.search(k, currentSearch[k]);
                    }
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.navigationService#setSoftRedirect
     * @methodOf umbraco.services.navigationService
     *
     * @description
     * utility to set a special query string to indicate that the pending navigation change is a soft redirect
     */
            setSoftRedirect: function setSoftRedirect() {
                $location.search('sr', true);
            },
            /**
     * @ngdoc method
     * @name umbraco.services.navigationService#retainQueryStrings
     * @methodOf umbraco.services.navigationService
     *
     * @description
     * Will check the next route parameters to see if any of the query strings that should be retained from the previous route are missing,
     * if they are they will be merged and an object containing all route parameters is returned. If nothing should be changed, then null is returned.
     * @param {Object} currRouteParams The current route parameters
     * @param {Object} nextRouteParams The next route parameters
     */
            retainQueryStrings: function retainQueryStrings(currRouteParams, nextRouteParams) {
                var toRetain = Utilities.copy(nextRouteParams);
                var updated = false;
                retainedQueryStrings.forEach(function (r) {
                    // if mculture is set to null in nextRouteParams, the value will be undefined and we will not retain any query string that has a value of "null"
                    if (currRouteParams[r] && nextRouteParams[r] !== undefined && !nextRouteParams[r]) {
                        toRetain[r] = currRouteParams[r];
                        updated = true;
                    }
                });
                return updated ? toRetain : null;
            },
            /**
     * @ngdoc method
     * @name umbraco.services.navigationService#load
     * @methodOf umbraco.services.navigationService
     *
     * @description
     * Shows the legacy iframe and loads in the content based on the source url
     * @param {String} source The URL to load into the iframe
     */
            loadLegacyIFrame: function loadLegacyIFrame(source) {
                $location.path('/' + appState.getSectionState('currentSection') + '/framed/' + encodeURIComponent(source));
            },
            /**
     * @ngdoc method
     * @name umbraco.services.navigationService#changeSection
     * @methodOf umbraco.services.navigationService
     *
     * @description
     * Changes the active section to a given section alias
     * If the navigation is 'sticky' this will load the associated tree
     * and load the dashboard related to the section
     * @param {string} sectionAlias The alias of the section
     */
            changeSection: function changeSection(sectionAlias, force) {
                setMode('default-opensection');
                if (force && appState.getSectionState('currentSection') === sectionAlias) {
                    appState.setSectionState('currentSection', '');
                }
                appState.setSectionState('currentSection', sectionAlias);
                this.showTree(sectionAlias);
                $location.path(sectionAlias);
            },
            /**
     * @ngdoc method
     * @name umbraco.services.navigationService#showTree
     * @methodOf umbraco.services.navigationService
     *
     * @description
     * Displays the tree for a given section alias but turning on the containing dom element
     * only changes if the section is different from the current one
    * @param {string} sectionAlias The alias of the section to load
     * @param {Object} syncArgs Optional object of arguments for syncing the tree for the section being shown
    */
            showTree: function showTree(sectionAlias, syncArgs) {
                if (sectionAlias !== appState.getSectionState('currentSection')) {
                    appState.setSectionState('currentSection', sectionAlias);
                    if (syncArgs) {
                        return this.syncTree(syncArgs);
                    }
                }
                setMode('tree');
                return $q.when(true);
            },
            showTray: function showTray() {
                appState.setGlobalState('showTray', true);
            },
            hideTray: function hideTray() {
                appState.setGlobalState('showTray', false);
            },
            /**     
     * @ngdoc method
     * @name umbraco.services.navigationService#syncTree
     * @methodOf umbraco.services.navigationService
     *
     * @description
     * Syncs a tree with a given path, returns a promise
     * The path format is: ["itemId","itemId"], and so on
     * so to sync to a specific document type node do:
     * <pre>
     * navigationService.syncTree({tree: 'content', path: ["-1","123d"], forceReload: true});
     * </pre>
     * @param {Object} args arguments passed to the function
     * @param {String} args.tree the tree alias to sync to
     * @param {Array} args.path the path to sync the tree to
     * @param {Boolean} args.forceReload optional, specifies whether to force reload the node data from the server even if it already exists in the tree currently
     */
            syncTree: function syncTree(args) {
                if (!args) {
                    throw 'args cannot be null';
                }
                if (!args.path) {
                    throw 'args.path cannot be null';
                }
                if (!args.tree) {
                    throw 'args.tree cannot be null';
                }
                return navReadyPromise.promise.then(function () {
                    return mainTreeApi.syncTree(args);
                });
            },
            /**     
     * @ngdoc method
     * @name umbraco.services.navigationService#hasTree
     * @methodOf umbraco.services.navigationService
     *
     * @description
     * Checks if a tree with the given alias exists.
     * 
     * @param {String} treeAlias the tree alias to check
     */
            hasTree: function hasTree(treeAlias) {
                return navReadyPromise.promise.then(function () {
                    return mainTreeApi.hasTree(treeAlias);
                });
            },
            /**
        Internal method that should ONLY be used by the legacy API wrapper, the legacy API used to
        have to set an active tree and then sync, the new API does this in one method by using syncTree
          TODO: Delete this if not required
    */
            _syncPath: function _syncPath(path, forceReload) {
                return navReadyPromise.promise.then(function () {
                    return mainTreeApi.syncTree({
                        path: path,
                        forceReload: forceReload
                    });
                });
            },
            reloadNode: function reloadNode(node) {
                return navReadyPromise.promise.then(function () {
                    return mainTreeApi.reloadNode(node);
                });
            },
            reloadSection: function reloadSection(sectionAlias) {
                return navReadyPromise.promise.then(function () {
                    treeService.clearCache({ section: sectionAlias });
                    return mainTreeApi.load(sectionAlias);
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.navigationService#hideTree
     * @methodOf umbraco.services.navigationService
     *
     * @description
     * Hides the tree by hiding the containing dom element
     */
            hideTree: function hideTree() {
                if (appState.getGlobalState('isTablet') === true && !appState.getGlobalState('stickyNavigation')) {
                    //reset it to whatever is in the url
                    appState.setSectionState('currentSection', $routeParams.section);
                    setMode('default-hidesectiontree');
                }
            },
            /**
     * @ngdoc method
     * @name umbraco.services.navigationService#showMenu
     * @methodOf umbraco.services.navigationService
     *
     * @description
     * Hides the tree by hiding the containing dom element.
     * This always returns a promise!
     *
     * @param {Event} event the click event triggering the method, passed from the DOM element
     */
            showMenu: function showMenu(args) {
                var self = this;
                return treeService.getMenu({ treeNode: args.node }).then(function (data) {
                    showBackdrop();
                    //check for a default
                    //NOTE: event will be undefined when a call to hideDialog is made so it won't re-load the default again.
                    // but perhaps there's a better way to deal with with an additional parameter in the args ? it works though.
                    if (data.defaultAlias && !args.skipDefault) {
                        var found = _.find(data.menuItems, function (item) {
                            return item.alias = data.defaultAlias;
                        });
                        if (found) {
                            //NOTE: This is assigning the current action node - this is not the same as the currently selected node!
                            appState.setMenuState('currentNode', args.node);
                            self.showDialog({
                                node: args.node,
                                action: found,
                                section: appState.getSectionState('currentSection')
                            });
                            return $q.resolve();
                        }
                    }
                    //there is no default or we couldn't find one so just continue showing the menu
                    setMode('menu');
                    appState.setMenuState('currentNode', args.node);
                    appState.setMenuState('menuActions', data.menuItems);
                    appState.setMenuState('dialogTitle', args.node.name);
                    return $q.resolve();
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.navigationService#hideMenu
     * @methodOf umbraco.services.navigationService
     *
     * @description
     * Hides the menu by hiding the containing dom element
     */
            hideMenu: function hideMenu() {
                //SD: Would we ever want to access the last action'd node instead of clearing it here?
                appState.setMenuState('currentNode', null);
                appState.setMenuState('menuActions', []);
                setMode('tree');
                closeBackdrop();
            },
            /** Executes a given menu action */
            executeMenuAction: function executeMenuAction(action, node, section) {
                if (!action) {
                    throw 'action cannot be null';
                }
                if (!node) {
                    throw 'node cannot be null';
                }
                if (!section) {
                    throw 'section cannot be null';
                }
                appState.setMenuState('currentNode', node);
                if (action.metaData && action.metaData['actionRoute'] && Utilities.isString(action.metaData['actionRoute'])) {
                    //first check if the menu item simply navigates to a route
                    var parts = action.metaData['actionRoute'].split('?');
                    $location.path(parts[0]).search(parts.length > 1 ? parts[1] : '');
                    this.hideNavigation();
                    return;
                } else if (action.metaData && action.metaData['jsAction'] && Utilities.isString(action.metaData['jsAction'])) {
                    //we'll try to get the jsAction from the injector
                    var menuAction = action.metaData['jsAction'].split('.');
                    if (menuAction.length !== 2) {
                        //if it is not two parts long then this most likely means that it's a legacy action
                        var js = action.metaData['jsAction'].replace('javascript:', '');
                        //there's not really a different way to achieve this except for eval
                        eval(js);
                    } else {
                        var menuActionService = $injector.get(menuAction[0]);
                        if (!menuActionService) {
                            throw 'The angular service ' + menuAction[0] + ' could not be found';
                        }
                        var method = menuActionService[menuAction[1]];
                        if (!method) {
                            throw 'The method ' + menuAction[1] + ' on the angular service ' + menuAction[0] + ' could not be found';
                        }
                        method.apply(this, [{
                                //map our content object to a basic entity to pass in to the menu handlers,
                                //this is required for consistency since a menu item needs to be decoupled from a tree node since the menu can
                                //exist standalone in the editor for which it can only pass in an entity (not tree node).
                                entity: umbModelMapper.convertToEntityBasic(node),
                                action: action,
                                section: section,
                                treeAlias: treeService.getTreeAlias(node)
                            }]);
                    }
                } else {
                    showBackdrop();
                    service.showDialog({
                        node: node,
                        action: action,
                        section: section
                    });
                }
            },
            /**
     * @ngdoc method
     * @name umbraco.services.navigationService#showDialog
     * @methodOf umbraco.services.navigationService
     *
     * @description
     * Opens a dialog, for a given action on a given tree node
     * the path to the dialog view is determined by:
     * "views/" + current tree + "/" + action alias + ".html"
     * The dialog controller will get passed a scope object that is created here with the properties:
     * scope.currentNode = the selected tree node
     * scope.title = the title of the menu item
     * scope.view = the path to the view html file
     * so that the dialog controllers can use these properties
     *
     * @param {Object} args arguments passed to the function
     * @param {Scope} args.scope current scope passed to the dialog
     * @param {Object} args.action the clicked action containing `name` and `alias`
     */
            showDialog: function showDialog(args) {
                if (!args) {
                    throw 'showDialog is missing the args parameter';
                }
                if (!args.action) {
                    throw 'The args parameter must have an \'action\' property as the clicked menu action object';
                }
                if (!args.node) {
                    throw 'The args parameter must have a \'node\' as the active tree node';
                }
                //the title might be in the meta data, check there first
                if (args.action.metaData['dialogTitle']) {
                    appState.setMenuState('dialogTitle', args.action.metaData['dialogTitle']);
                } else {
                    appState.setMenuState('dialogTitle', args.action.name);
                }
                var templateUrl;
                if (args.action.metaData['actionView']) {
                    templateUrl = args.action.metaData['actionView'];
                } else {
                    var treeAlias = treeService.getTreeAlias(args.node);
                    if (!treeAlias) {
                        throw 'Could not get tree alias for node ' + args.node.id;
                    }
                    templateUrl = this.getTreeTemplateUrl(treeAlias, args.action.alias);
                }
                setMode('dialog');
                if (templateUrl) {
                    appState.setMenuState('dialogTemplateUrl', templateUrl);
                }
            },
            /**
      * @ngdoc method
      * @name umbraco.services.navigationService#getTreeTemplateUrl
      * @methodOf umbraco.services.navigationService
      *
      * @param {string} treeAlias the alias of the tree to look up
      * @param {string} action the view file name
      * @description
      * creates the templateUrl based on treeAlias and action
      * by convention we will look into the /views/{treetype}/{action}.html
      * for example: /views/content/create.html
      * we will also check for a 'packageName' for the current tree, if it exists then the convention will be:
      * for example: /App_Plugins/{mypackage}/backoffice/{treetype}/create.html
      */
            getTreeTemplateUrl: function getTreeTemplateUrl(treeAlias, action) {
                var packageTreeFolder = treeService.getTreePackageFolder(treeAlias);
                if (packageTreeFolder) {
                    return Umbraco.Sys.ServerVariables.umbracoSettings.appPluginsPath + '/' + packageTreeFolder + '/backoffice/' + treeAlias + '/' + action + '.html';
                } else {
                    return 'views/' + treeAlias + '/' + action + '.html';
                }
            },
            /**
      * @ngdoc method
      * @name umbraco.services.navigationService#allowHideDialog
      * @methodOf umbraco.services.navigationService
      *
      * @param {boolean} allow false if the navigation service should disregard instructions to hide the current dialog, true otherwise
      * @description
      * instructs the navigation service whether it's allowed to hide the current dialog
      */
            allowHideDialog: function allowHideDialog(allow) {
                if (appState.getGlobalState('navMode') !== 'dialog') {
                    return;
                }
                appState.setMenuState('allowHideMenuDialog', allow);
            },
            /**
    * @ngdoc method
    * @name umbraco.services.navigationService#hideDialog
    * @methodOf umbraco.services.navigationService
    *
    * @description
    * hides the currently open dialog
    */
            hideDialog: function hideDialog(showMenu) {
                if (appState.getMenuState('allowHideMenuDialog') === false) {
                    return;
                }
                if (showMenu) {
                    this.showMenu({
                        skipDefault: true,
                        node: appState.getMenuState('currentNode')
                    });
                } else {
                    closeBackdrop();
                    setMode('default');
                }
            },
            /**
      * @ngdoc method
      * @name umbraco.services.navigationService#showSearch
      * @methodOf umbraco.services.navigationService
      *
      * @description
      * shows the search pane
      */
            showSearch: function showSearch() {
                setMode('search');
            },
            /**
      * @ngdoc method
      * @name umbraco.services.navigationService#hideSearch
      * @methodOf umbraco.services.navigationService
      *
      * @description
      * hides the search pane
    */
            hideSearch: function hideSearch() {
                setMode('default-hidesearch');
            },
            /**
      * @ngdoc method
      * @name umbraco.services.navigationService#hideNavigation
      * @methodOf umbraco.services.navigationService
      *
      * @description
      * hides any open navigation panes and resets the tree, actions and the currently selected node
      */
            hideNavigation: function hideNavigation() {
                appState.setMenuState('menuActions', []);
                closeBackdrop();
                setMode('default');
            }
        };
        return service;
    }
    angular.module('umbraco.services').factory('navigationService', navigationService);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.notificationsService
 *
 * @requires $rootScope 
 * @requires $timeout
 * @requires angularHelper
 *	
 * @description
 * Application-wide service for handling notifications, the umbraco application 
 * maintains a single collection of notications, which the UI watches for changes.
 * By default when a notication is added, it is automaticly removed 7 seconds after
 * This can be changed on add()
 *
 * ##usage
 * To use, simply inject the notificationsService into any controller that needs it, and make
 * sure the umbraco.services module is accesible - which it should be by default.
 *
 * <pre>
 *		notificationsService.success("Document Published", "hooraaaay for you!");
 *      notificationsService.error("Document Failed", "booooh");
 * </pre> 
 */
    angular.module('umbraco.services').factory('notificationsService', function ($rootScope, $timeout, angularHelper) {
        var nArray = [];
        function setViewPath(view) {
            if (view.indexOf('/') < 0) {
                view = 'views/common/notifications/' + view;
            }
            if (view.indexOf('.html') < 0) {
                view = view + '.html';
            }
            return view;
        }
        var service = {
            /**
    * @ngdoc method
    * @name umbraco.services.notificationsService#add
    * @methodOf umbraco.services.notificationsService
    *
    * @description
    * Lower level api for adding notifcations, support more advanced options
    * @param {Object} item The notification item
    * @param {String} item.headline Short headline
    * @param {String} item.message longer text for the notication, trimmed after 200 characters, which can then be exanded
    * @param {String} item.type Notification type, can be: "success","warning","error" or "info" 
    * @param {String} item.url url to open when notification is clicked
    * @param {String} item.target the target used together with `url`. Empty if not specified.
    * @param {String} item.view path to custom view to load into the notification box
    * @param {Array} item.actions Collection of button actions to append (label, func, cssClass)
    * @param {Boolean} item.sticky if set to true, the notification will not auto-close
    * @returns {Object} args notification object
    */
            add: function add(item) {
                angularHelper.safeApply($rootScope, function () {
                    if (item.view) {
                        item.view = setViewPath(item.view);
                        item.sticky = true;
                        item.type = 'form';
                        item.headline = null;
                    }
                    //add a colon after the headline if there is a message as well
                    if (item.message) {
                        item.headline += ': ';
                        if (item.message.length > 200) {
                            item.sticky = true;
                        }
                    }
                    //we need to ID the item, going by index isn't good enough because people can remove at different indexes 
                    // whenever they want. Plus once we remove one, then the next index will be different. The only way to 
                    // effectively remove an item is by an Id.
                    item.id = String.CreateGuid();
                    nArray.push(item);
                    if (!item.sticky) {
                        $timeout(function () {
                            var found = _.find(nArray, function (i) {
                                return i.id === item.id;
                            });
                            if (found) {
                                var index = nArray.indexOf(found);
                                nArray.splice(index, 1);
                            }
                        }, 10000);
                    }
                    return item;
                });
            },
            hasView: function hasView(view) {
                if (!view) {
                    return _.find(nArray, function (notification) {
                        return notification.view;
                    });
                } else {
                    view = setViewPath(view).toLowerCase();
                    return _.find(nArray, function (notification) {
                        return notification.view.toLowerCase() === view;
                    });
                }
            },
            addView: function addView(view, args) {
                var item = {
                    args: args,
                    view: view
                };
                service.add(item);
            },
            /**
    * @ngdoc method
    * @name umbraco.services.notificationsService#showNotification
    * @methodOf umbraco.services.notificationsService
    *
    * @description
    * Shows a notification based on the object passed in, normally used to render notifications sent back from the server
    *		 
    * @returns {Object} args notification object
    */
            showNotification: function showNotification(args) {
                if (!args) {
                    throw 'args cannot be null';
                }
                if (args.type === undefined || args.type === null) {
                    throw 'args.type cannot be null';
                }
                if (!args.header) {
                    throw 'args.header cannot be null';
                }
                switch (args.type) {
                case 0:
                    //save
                    this.success(args.header, args.message);
                    break;
                case 1:
                    //info
                    this.info(args.header, args.message);
                    break;
                case 2:
                    //error
                    this.error(args.header, args.message);
                    break;
                case 3:
                    //success
                    this.success(args.header, args.message);
                    break;
                case 4:
                    //warning
                    this.warning(args.header, args.message);
                    break;
                }
            },
            /**
    * @ngdoc method
    * @name umbraco.services.notificationsService#success
    * @methodOf umbraco.services.notificationsService
    *
    * @description
    * Adds a green success notication to the notications collection
    * This should be used when an operations *completes* without errors
    *
    * @param {String} headline Headline of the notification
    * @param {String} message longer text for the notication, trimmed after 200 characters, which can then be exanded
    * @returns {Object} notification object
    */
            success: function success(headline, message) {
                return service.add({
                    headline: headline,
                    message: message,
                    type: 'success',
                    time: new Date()
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.notificationsService#error
     * @methodOf umbraco.services.notificationsService
     *
     * @description
     * Adds a red error notication to the notications collection
     * This should be used when an operations *fails* and could not complete
     * 
     * @param {String} headline Headline of the notification
     * @param {String} message longer text for the notication, trimmed after 200 characters, which can then be exanded
     * @returns {Object} notification object
     */
            error: function error(headline, message) {
                return service.add({
                    headline: headline,
                    message: message,
                    type: 'error',
                    time: new Date()
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.notificationsService#warning
     * @methodOf umbraco.services.notificationsService
     *
     * @description
     * Adds a yellow warning notication to the notications collection
     * This should be used when an operations *completes* but something was not as expected
     * 
     *
     * @param {String} headline Headline of the notification
     * @param {String} message longer text for the notication, trimmed after 200 characters, which can then be exanded
     * @returns {Object} notification object
     */
            warning: function warning(headline, message) {
                return service.add({
                    headline: headline,
                    message: message,
                    type: 'warning',
                    time: new Date()
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.notificationsService#warning
     * @methodOf umbraco.services.notificationsService
     *
     * @description
     * Adds a yellow warning notication to the notications collection
     * This should be used when an operations *completes* but something was not as expected
     * 
     *
     * @param {String} headline Headline of the notification
     * @param {String} message longer text for the notication, trimmed after 200 characters, which can then be exanded
     * @returns {Object} notification object
     */
            info: function info(headline, message) {
                return service.add({
                    headline: headline,
                    message: message,
                    type: 'info',
                    time: new Date()
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.notificationsService#remove
     * @methodOf umbraco.services.notificationsService
     *
     * @description
     * Removes a notification from the notifcations collection at a given index 
     *
     * @param {Int} index index where the notication should be removed from
     */
            remove: function remove(index) {
                if (Utilities.isObject(index)) {
                    var i = nArray.indexOf(index);
                    angularHelper.safeApply($rootScope, function () {
                        nArray.splice(i, 1);
                    });
                } else {
                    angularHelper.safeApply($rootScope, function () {
                        nArray.splice(index, 1);
                    });
                }
            },
            /**
     * @ngdoc method
     * @name umbraco.services.notificationsService#removeAll
     * @methodOf umbraco.services.notificationsService
     *
     * @description
     * Removes all notifications from the notifcations collection 
     */
            removeAll: function removeAll() {
                angularHelper.safeApply($rootScope, function () {
                    nArray = [];
                });
            },
            /**
     * @ngdoc property
     * @name umbraco.services.notificationsService#current
     * @propertyOf umbraco.services.notificationsService
     *
     * @description
     * Returns an array of current notifications to display
     *
     * @returns {string} returns an array
     */
            current: nArray,
            /**
     * @ngdoc method
     * @name umbraco.services.notificationsService#getCurrent
     * @methodOf umbraco.services.notificationsService
     *
     * @description
     * Method to return all notifications from the notifcations collection 
     */
            getCurrent: function getCurrent() {
                return nArray;
            }
        };
        return service;
    });
    'use strict';
    /**
 @ngdoc service
 * @name umbraco.services.overlayService
 *
 * @description
 * <b>Added in Umbraco 8.0</b>. Application-wide service for handling overlays.
 */
    (function () {
        'use strict';
        function overlayService(eventsService, backdropService, focusLockService) {
            var currentOverlay = null;
            function open(newOverlay) {
                // prevent two open overlays at the same time
                if (currentOverlay) {
                    _close();
                }
                var backdropOptions = {};
                var overlay = newOverlay;
                // set the default overlay position to center
                if (!overlay.position) {
                    overlay.position = 'center';
                }
                // set the default overlay size to small
                if (!overlay.size) {
                    overlay.size = 'small';
                }
                // use a default empty view if nothing is set
                if (!overlay.view) {
                    overlay.view = 'views/common/overlays/default/default.html';
                }
                // option to disable backdrop clicks
                if (overlay.disableBackdropClick) {
                    backdropOptions.disableEventsOnClick = true;
                }
                overlay.show = true;
                focusLockService.addInertAttribute();
                backdropService.open(backdropOptions);
                currentOverlay = overlay;
                eventsService.emit('appState.overlay', overlay);
            }
            function _close() {
                focusLockService.removeInertAttribute();
                backdropService.close();
                currentOverlay = null;
                eventsService.emit('appState.overlay', null);
            }
            function ysod(error) {
                var overlay = {
                    view: 'views/common/overlays/ysod/ysod.html',
                    error: error,
                    close: function close() {
                        _close();
                    }
                };
                open(overlay);
            }
            function confirm(overlay) {
                if (!overlay.closeButtonLabelKey)
                    overlay.closeButtonLabelKey = 'general_cancel';
                if (!overlay.view)
                    overlay.view = 'views/common/overlays/confirm/confirm.html';
                if (!overlay.close)
                    overlay.close = function () {
                        _close();
                    };
                switch (overlay.confirmType) {
                case 'delete':
                    if (!overlay.confirmMessageStyle)
                        overlay.confirmMessageStyle = 'danger';
                    if (!overlay.submitButtonStyle)
                        overlay.submitButtonStyle = 'danger';
                    if (!overlay.submitButtonLabelKey)
                        overlay.submitButtonLabelKey = 'actions_delete';
                    break;
                case 'remove':
                    if (!overlay.submitButtonStyle)
                        overlay.submitButtonStyle = 'primary';
                    if (!overlay.submitButtonLabelKey)
                        overlay.submitButtonLabelKey = 'actions_remove';
                    break;
                default:
                    if (!overlay.submitButtonLabelKey)
                        overlay.submitButtonLabelKey = 'general_confirm';
                }
                open(overlay);
            }
            function confirmDelete(overlay) {
                overlay.confirmType = 'delete';
                confirm(overlay);
            }
            function confirmRemove(overlay) {
                overlay.confirmType = 'remove';
                confirm(overlay);
            }
            var service = {
                open: open,
                close: _close,
                ysod: ysod,
                confirm: confirm,
                confirmDelete: confirmDelete,
                confirmRemove: confirmRemove
            };
            return service;
        }
        angular.module('umbraco.services').factory('overlayService', overlayService);
    }());
    'use strict';
    (function () {
        'use strict';
        function overlayHelper() {
            var numberOfOverlays = 0;
            function registerOverlay() {
                numberOfOverlays++;
                return numberOfOverlays;
            }
            function unregisterOverlay() {
                numberOfOverlays--;
                return numberOfOverlays;
            }
            function getNumberOfOverlays() {
                return numberOfOverlays;
            }
            var service = {
                numberOfOverlays: numberOfOverlays,
                registerOverlay: registerOverlay,
                unregisterOverlay: unregisterOverlay,
                getNumberOfOverlays: getNumberOfOverlays
            };
            return service;
        }
        angular.module('umbraco.services').factory('overlayHelper', overlayHelper);
    }());
    'use strict';
    (function () {
        'use strict';
        function platformService() {
            function isMac() {
                return navigator.platform.toUpperCase().indexOf('MAC') >= 0;
            }
            ////////////
            var service = { isMac: isMac };
            return service;
        }
        angular.module('umbraco.services').factory('platformService', platformService);
    }());
    'use strict';
    (function () {
        'use strict';
        /**
   * A service normally used to recover from session expiry
   * @param {any} $q
   * @param {any} $log
   */
        function requestRetryQueue($q, $log) {
            var retryQueue = [];
            var retryUser = null;
            var service = {
                // The security service puts its own handler in here!
                onItemAddedCallbacks: [],
                hasMore: function hasMore() {
                    return retryQueue.length > 0;
                },
                push: function push(retryItem) {
                    retryQueue.push(retryItem);
                    // Call all the onItemAdded callbacks
                    angular.forEach(service.onItemAddedCallbacks, function (cb) {
                        try {
                            cb(retryItem);
                        } catch (e) {
                            $log.error('requestRetryQueue.push(retryItem): callback threw an error' + e);
                        }
                    });
                },
                pushRetryFn: function pushRetryFn(reason, userName, retryFn) {
                    // The reason parameter is optional
                    if (arguments.length === 2) {
                        retryFn = userName;
                        userName = reason;
                        reason = undefined;
                    }
                    if (retryUser && retryUser !== userName || userName === null) {
                        throw new Error('invalid user');
                    }
                    retryUser = userName;
                    // The deferred object that will be resolved or rejected by calling retry or cancel
                    var deferred = $q.defer();
                    var retryItem = {
                        reason: reason,
                        retry: function retry() {
                            // Wrap the result of the retryFn into a promise if it is not already
                            $q.when(retryFn()).then(function (value) {
                                // If it was successful then resolve our deferred
                                deferred.resolve(value);
                            }, function (value) {
                                // Othewise reject it
                                deferred.reject(value);
                            });
                        },
                        cancel: function cancel() {
                            // Give up on retrying and reject our deferred
                            deferred.reject();
                        }
                    };
                    service.push(retryItem);
                    return deferred.promise;
                },
                retryReason: function retryReason() {
                    return service.hasMore() && retryQueue[0].reason;
                },
                cancelAll: function cancelAll() {
                    while (service.hasMore()) {
                        retryQueue.shift().cancel();
                    }
                    retryUser = null;
                },
                retryAll: function retryAll(userName) {
                    if (retryUser == null) {
                        return;
                    }
                    if (retryUser !== userName) {
                        service.cancelAll();
                        return;
                    }
                    while (service.hasMore()) {
                        retryQueue.shift().retry();
                    }
                }
            };
            return service;
        }
        angular.module('umbraco.services').factory('requestRetryQueue', requestRetryQueue);
    }());
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.searchService
 *
 *
 * @description
 * Service for handling the main application search, can currently search content, media and members
 *
 * ##usage
 * To use, simply inject the searchService into any controller that needs it, and make
 * sure the umbraco.services module is accesible - which it should be by default.
 *
 * <pre>
 *      searchService.searchMembers({term: 'bob'}).then(function(results){
 *          results.forEach(function(result){
 *                  //returns:
 *                  {name: "name", id: 1234, menuUrl: "url", editorPath: "url", metaData: {}, subtitle: "/path/etc" }
 *           })
 *           var result =
 *       })
 * </pre>
 */
    angular.module('umbraco.services').factory('searchService', function (entityResource, $injector, searchResultFormatter) {
        return {
            /**
    * @ngdoc method
    * @name umbraco.services.searchService#searchMembers
    * @methodOf umbraco.services.searchService
    *
    * @description
    * Searches the default member search index
    * @param {Object} args argument object
    * @param {String} args.term seach term
    * @returns {Promise} returns promise containing all matching members
    */
            searchMembers: function searchMembers(args) {
                if (!args.term) {
                    throw 'args.term is required';
                }
                return entityResource.search(args.term, 'Member', args.searchFrom).then(function (data) {
                    data.forEach(function (item) {
                        return searchResultFormatter.configureMemberResult(item);
                    });
                    return data;
                });
            },
            /**
    * @ngdoc method
    * @name umbraco.services.searchService#searchContent
    * @methodOf umbraco.services.searchService
    *
    * @description
    * Searches the default internal content search index
    * @param {Object} args argument object
    * @param {String} args.term seach term
    * @returns {Promise} returns promise containing all matching content items
    */
            searchContent: function searchContent(args) {
                if (!args.term) {
                    throw 'args.term is required';
                }
                return entityResource.search(args.term, 'Document', args.searchFrom, args.canceler, args.dataTypeKey).then(function (data) {
                    data.forEach(function (item) {
                        return searchResultFormatter.configureContentResult(item);
                    });
                    return data;
                });
            },
            /**
    * @ngdoc method
    * @name umbraco.services.searchService#searchMedia
    * @methodOf umbraco.services.searchService
    *
    * @description
    * Searches the default media search index
    * @param {Object} args argument object
    * @param {String} args.term seach term
    * @returns {Promise} returns promise containing all matching media items
    */
            searchMedia: function searchMedia(args) {
                if (!args.term) {
                    throw 'args.term is required';
                }
                return entityResource.search(args.term, 'Media', args.searchFrom, args.canceler, args.dataTypeKey).then(function (data) {
                    data.forEach(function (item) {
                        return searchResultFormatter.configureMediaResult(item);
                    });
                    return data;
                });
            },
            /**
    * @ngdoc method
    * @name umbraco.services.searchService#searchAll
    * @methodOf umbraco.services.searchService
    *
    * @description
    * Searches all available indexes and returns all results in one collection
    * @param {Object} args argument object
    * @param {String} args.term seach term
    * @returns {Promise} returns promise containing all matching items
    */
            searchAll: function searchAll(args) {
                var _this = this;
                if (!args.term) {
                    throw 'args.term is required';
                }
                return entityResource.searchAll(args.term, args.canceler).then(function (data) {
                    Object.values(data).forEach(function (resultByType) {
                        //we need to format the search result data to include things like the subtitle, urls, etc...
                        // this is done with registered angular services as part of the SearchableTreeAttribute, if that
                        // is not found, than we format with the default formatter
                        var formatterMethod = searchResultFormatter.configureDefaultResult;
                        //check if a custom formatter is specified...
                        if (resultByType.jsSvc) {
                            var searchFormatterService = $injector.get(resultByType.jsSvc);
                            if (searchFormatterService) {
                                if (!resultByType.jsMethod) {
                                    resultByType.jsMethod = 'format';
                                }
                                formatterMethod = searchFormatterService[resultByType.jsMethod];
                                if (!formatterMethod) {
                                    throw 'The method ' + resultByType.jsMethod + ' on the angular service ' + resultByType.jsSvc + ' could not be found';
                                }
                            }
                        }
                        //now apply the formatter for each result
                        resultByType.results.forEach(function (item) {
                            formatterMethod.apply(_this, [
                                item,
                                resultByType.treeAlias,
                                resultByType.appAlias
                            ]);
                        });
                    });
                    return data;
                });
            },
            // TODO: This doesn't do anything!
            setCurrent: function setCurrent(sectionAlias) {
                var currentSection = sectionAlias;
            }
        };
    });
    'use strict';
    function searchResultFormatter(umbRequestHelper) {
        function configureDefaultResult(content, treeAlias, appAlias) {
            content.editorPath = appAlias + '/' + treeAlias + '/edit/' + content.id;
            angular.extend(content.metaData, { treeAlias: treeAlias });
        }
        function configureContentResult(content, treeAlias, appAlias) {
            content.menuUrl = umbRequestHelper.getApiUrl('contentTreeBaseUrl', 'GetMenu', [
                { id: content.id },
                { application: appAlias }
            ]);
            content.editorPath = appAlias + '/' + treeAlias + '/edit/' + content.id;
            angular.extend(content.metaData, { treeAlias: treeAlias });
            content.subTitle = content.metaData.Url;
        }
        function configureMemberResult(member, treeAlias, appAlias) {
            member.menuUrl = umbRequestHelper.getApiUrl('memberTreeBaseUrl', 'GetMenu', [
                { id: member.id },
                { application: appAlias }
            ]);
            member.editorPath = appAlias + '/' + treeAlias + '/edit/' + (member.key ? member.key : member.id);
            angular.extend(member.metaData, { treeAlias: treeAlias });
            member.subTitle = member.metaData.Email;
        }
        function configureMediaResult(media, treeAlias, appAlias) {
            media.menuUrl = umbRequestHelper.getApiUrl('mediaTreeBaseUrl', 'GetMenu', [
                { id: media.id },
                { application: appAlias }
            ]);
            media.editorPath = appAlias + '/' + treeAlias + '/edit/' + media.id;
            angular.extend(media.metaData, { treeAlias: treeAlias });
        }
        return {
            configureContentResult: configureContentResult,
            configureMemberResult: configureMemberResult,
            configureMediaResult: configureMediaResult,
            configureDefaultResult: configureDefaultResult
        };
    }
    angular.module('umbraco.services').factory('searchResultFormatter', searchResultFormatter);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.sectionService
 *
 *  
 * @description
 * A service to return the sections (applications) to be listed in the navigation which are contextual to the current user
 */
    (function () {
        'use strict';
        function sectionService(userService, $q, sectionResource) {
            function getSectionsForUser() {
                var deferred = $q.defer();
                userService.getCurrentUser().then(function (u) {
                    //if they've already loaded, return them
                    if (u.sections) {
                        deferred.resolve(u.sections);
                    } else {
                        sectionResource.getSections().then(function (sections) {
                            //set these to the user (cached), then the user changes, these will be wiped
                            u.sections = sections;
                            deferred.resolve(u.sections);
                        });
                    }
                });
                return deferred.promise;
            }
            var service = { getSectionsForUser: getSectionsForUser };
            return service;
        }
        angular.module('umbraco.services').factory('sectionService', sectionService);
    }());
    'use strict';
    function _slicedToArray(arr, i) {
        return _arrayWithHoles(arr) || _iterableToArrayLimit(arr, i) || _nonIterableRest();
    }
    function _nonIterableRest() {
        throw new TypeError('Invalid attempt to destructure non-iterable instance');
    }
    function _iterableToArrayLimit(arr, i) {
        if (!(Symbol.iterator in Object(arr) || Object.prototype.toString.call(arr) === '[object Arguments]')) {
            return;
        }
        var _arr = [];
        var _n = true;
        var _d = false;
        var _e = undefined;
        try {
            for (var _i = arr[Symbol.iterator](), _s; !(_n = (_s = _i.next()).done); _n = true) {
                _arr.push(_s.value);
                if (i && _arr.length === i)
                    break;
            }
        } catch (err) {
            _d = true;
            _e = err;
        } finally {
            try {
                if (!_n && _i['return'] != null)
                    _i['return']();
            } finally {
                if (_d)
                    throw _e;
            }
        }
        return _arr;
    }
    function _arrayWithHoles(arr) {
        if (Array.isArray(arr))
            return arr;
    }
    /**
 * @ngdoc service
 * @name umbraco.services.serverValidationManager
 * @function
 *
 * @description
 * Used to handle server side validation and wires up the UI with the messages. There are 2 types of validation messages, one
 * is for user defined properties (called Properties) and the other is for field properties which are attached to the native 
 * model objects (not user defined). The methods below are named according to these rules: Properties vs Fields.
 * 
 * For a more indepth explanation of how server side validation works with the angular app, see this GitHub PR: 
 * https://github.com/umbraco/Umbraco-CMS/pull/8339
 * 
 */
    function serverValidationManager($timeout) {
        // The array of callback objects, each object is:
        // - propertyAlias (this is the property's 'path' if it's a nested error)
        // - culture
        // - fieldName
        // - segment
        // - callback (function)
        // - id (unique identifier, auto-generated, used internally for unsubscribing the callback)
        // - options (used for complex properties, can contain options.matchType which can be either "suffix" or "prefix" or "contains")
        var callbacks = [];
        // The array of error message objects, each object 'key' is:    
        // - propertyAlias (this is the property's 'path' if it's a nested error)
        // - culture
        // - fieldName
        // - segment
        // The object also contains:
        // - errorMsg
        var errorMsgItems = [];
        var defaultMatchOptions = { matchType: null };
        /** calls the callback specified with the errors specified, used internally */
        function executeCallback(errorsForCallback, callback, culture, segment, isValid) {
            callback.apply(instance, [
                isValid,
                // pass in a value indicating it is invalid
                errorsForCallback,
                // pass in the errors for this item
                errorMsgItems,
                // pass in all errors in total
                culture,
                // pass the culture that we are listing for.
                segment    // pass the segment that we are listing for.
            ]);
        }
        /**
   * @ngdoc function
   * @name notify
   * @methodOf umbraco.services.serverValidationManager
   * @function
   *
   * @description
   * Notifies all subscriptions again. Called when there are changes to subscriptions or errors. 
   */
        function notify() {
            $timeout(function () {
                for (var i = 0; i < errorMsgItems.length; i++) {
                    var item = errorMsgItems[i];
                }
                notifyCallbacks();
            });
        }
        function getFieldErrors(fieldName) {
            if (!Utilities.isString(fieldName)) {
                throw 'fieldName must be a string';
            }
            //find errors for this field name
            return _.filter(errorMsgItems, function (item) {
                return item.propertyAlias === null && item.culture === 'invariant' && item.fieldName === fieldName;
            });
        }
        function getPropertyErrors(propertyAlias, culture, segment, fieldName, options) {
            if (!Utilities.isString(propertyAlias)) {
                throw 'propertyAlias must be a string';
            }
            if (fieldName && !Utilities.isString(fieldName)) {
                throw 'fieldName must be a string';
            }
            if (!culture) {
                culture = 'invariant';
            }
            if (!segment) {
                segment = null;
            }
            if (!options) {
                options = defaultMatchOptions;
            }
            //find all errors for this property
            return _.filter(errorMsgItems, function (errMsgItem) {
                if (!errMsgItem.propertyAlias) {
                    return false;
                }
                var matchProp = matchErrMsgItemProperty(errMsgItem, propertyAlias, options);
                return matchProp && errMsgItem.culture === culture && errMsgItem.segment === segment    // ignore field matching if match options are used
&& (options.matchType || errMsgItem.fieldName === fieldName || fieldName === undefined || fieldName === '');
            });
        }
        /**
   * Returns true if the error message item's data matches the property validation key with a match type provided by the options
   * @param {any} errMsgItem The error message item
   * @param {any} propertyValidationKey The property validation key) 
   * @param {any} options The match type options
   */
        function matchErrMsgItemProperty(errMsgItem, propertyValidationKey, options) {
            if (errMsgItem.propertyAlias === propertyValidationKey) {
                return true;
            }
            if (options.matchType === 'prefix' && errMsgItem.propertyAlias.startsWith(propertyValidationKey + '/')) {
                return true;
            }
            if (options.matchType === 'suffix' && errMsgItem.propertyAlias.endsWith('/' + propertyValidationKey)) {
                return true;
            }
            if (options.matchType === 'contains' && errMsgItem.propertyAlias.includes('/' + propertyValidationKey + '/')) {
                return true;
            }
            return false;
        }
        function getVariantErrors(culture, segment) {
            if (!culture) {
                culture = 'invariant';
            }
            if (!segment) {
                segment = null;
            }
            //find all errors for this property
            return _.filter(errorMsgItems, function (item) {
                return item.culture === culture && item.segment === segment;
            });
        }
        function notifyCallback(cb) {
            if (cb.propertyAlias === null && cb.fieldName !== null) {
                //its a field error callback
                var fieldErrors = getFieldErrors(cb.fieldName);
                var valid = fieldErrors.length === 0;
                executeCallback(fieldErrors, cb.callback, cb.culture, cb.segment, valid);
            } else if (cb.propertyAlias != null) {
                //its a property error
                var propErrors = getPropertyErrors(cb.propertyAlias, cb.culture, cb.segment, cb.fieldName, cb.options);
                var _valid = propErrors.length === 0;
                executeCallback(propErrors, cb.callback, cb.culture, cb.segment, _valid);
            } else {
                //its a variant error
                var variantErrors = getVariantErrors(cb.culture, cb.segment);
                var _valid2 = variantErrors.length === 0;
                executeCallback(variantErrors, cb.callback, cb.culture, cb.segment, _valid2);
            }
        }
        /** Call all registered callbacks indicating if the data they are subscribed to is valid or invalid */
        function notifyCallbacks() {
            // nothing to call
            if (errorMsgItems.length === 0) {
                return;
            }
            callbacks.forEach(function (cb) {
                return notifyCallback(cb);
            });
        }
        /**
   * Flattens the complex errror result json into an array of the block's id/parent id and it's corresponding validation ModelState
   * @param {any} errorMsg
   * @param {any} parentPropertyAlias The parent property alias for the json error 
   */
        function parseComplexEditorError(errorMsg, parentPropertyAlias) {
            var json = Utilities.isArray(errorMsg) ? errorMsg : JSON.parse(errorMsg);
            var result = [];
            function extractModelState(validation, parentPath) {
                if (validation.$id && validation.ModelState) {
                    var ms = {
                        validationPath: ''.concat(parentPath, '/').concat(validation.$id),
                        modelState: validation.ModelState
                    };
                    result.push(ms);
                    return ms;
                }
                return null;
            }
            function iterateErrorBlocks(blocks, parentPath) {
                for (var i = 0; i < blocks.length; i++) {
                    var validation = blocks[i];
                    var ms = extractModelState(validation, parentPath);
                    if (!ms) {
                        continue;
                    }
                    var nested = _.omit(validation, '$id', '$elementTypeAlias', 'ModelState');
                    for (var _i = 0, _Object$entries = Object.entries(nested); _i < _Object$entries.length; _i++) {
                        var _Object$entries$_i = _slicedToArray(_Object$entries[_i], 2), key = _Object$entries$_i[0], value = _Object$entries$_i[1];
                        if (Array.isArray(value)) {
                            iterateErrorBlocks(value, ''.concat(ms.validationPath, '/').concat(key));    // recurse
                        }
                    }
                }
            }
            iterateErrorBlocks(json, parentPropertyAlias);
            return result;
        }
        /**
   * @ngdoc function
   * @name getPropertyCallbacks
   * @methodOf umbraco.services.serverValidationManager
   * @function
   *
   * @description
   * Gets all callbacks that has been registered using the subscribe method for the propertyAlias + fieldName combo.
   * This will always return any callbacks registered for just the property (i.e. field name is empty) and for ones with an
   * explicit field name set.
   */
        function getPropertyCallbacks(propertyAlias, culture, fieldName, segment) {
            //normalize culture to "invariant"
            if (!culture) {
                culture = 'invariant';
            }
            //normalize segment to null
            if (!segment) {
                segment = null;
            }
            var found = _.filter(callbacks, function (cb) {
                if (!cb.options) {
                    cb.options = defaultMatchOptions;
                }
                var matchProp = matchCallbackItemProperty(cb, propertyAlias);
                //returns any callback that have been registered directly against the field and for only the property
                return matchProp && cb.culture === culture && cb.segment === segment    // ignore field matching if match options are used
&& (cb.options.matchType || cb.fieldName === fieldName || cb.fieldName === undefined || cb.fieldName === '');
            });
            return found;
        }
        /**
   * Returns true if the callback item's data and match options matches the property validation key 
   * @param {any} cb
   * @param {any} propertyValidationKey
   */
        function matchCallbackItemProperty(cb, propertyValidationKey) {
            if (cb.propertyAlias === propertyValidationKey) {
                return true;
            }
            if (cb.options.matchType === 'prefix' && propertyValidationKey.startsWith(cb.propertyAlias + '/')) {
                return true;
            }
            if (cb.options.matchType === 'suffix' && propertyValidationKey.endsWith('/' + cb.propertyAlias)) {
                return true;
            }
            if (cb.options.matchType === 'contains' && propertyValidationKey.includes('/' + cb.propertyAlias + '/')) {
                return true;
            }
            return false;
        }
        /**
   * @ngdoc function
   * @name getFieldCallbacks
   * @methodOf umbraco.services.serverValidationManager
   * @function
   *
   * @description
   * Gets all callbacks that has been registered using the subscribe method for the field.
   */
        function getFieldCallbacks(fieldName) {
            var found = _.filter(callbacks, function (item) {
                //returns any callback that have been registered directly against the field
                return item.propertyAlias === null && item.culture === 'invariant' && item.segment === null && item.fieldName === fieldName;
            });
            return found;
        }
        /**
   * @ngdoc function
   * @name getVariantCallbacks
   * @methodOf umbraco.services.serverValidationManager
   * @function
   *
   * @description
   * Gets all callbacks that has been registered using the subscribe method for the culture and segment.
   */
        function getVariantCallbacks(culture, segment) {
            var found = _.filter(callbacks, function (item) {
                //returns any callback that have been registered directly against the given culture and given segment.
                return item.culture === culture && item.segment === segment && item.propertyAlias === null && item.fieldName === null;
            });
            return found;
        }
        /**
   * @ngdoc function
   * @name addFieldError
   * @methodOf umbraco.services.serverValidationManager
   * @function
   *
   * @description
   * Adds an error message for a native content item field (not a user defined property, for Example, 'Name')
   */
        function addFieldError(fieldName, errorMsg) {
            if (!fieldName) {
                return;
            }
            //only add the item if it doesn't exist                
            if (!hasFieldError(fieldName)) {
                errorMsgItems.push({
                    propertyAlias: null,
                    culture: 'invariant',
                    segment: null,
                    fieldName: fieldName,
                    errorMsg: errorMsg
                });
            }
            //find all errors for this item
            var errorsForCallback = getFieldErrors(fieldName);
            //we should now call all of the call backs registered for this error
            var cbs = getFieldCallbacks(fieldName);
            //call each callback for this error
            for (var cb in cbs) {
                executeCallback(errorsForCallback, cbs[cb].callback, null, null, false);
            }
        }
        /**
   * @ngdoc function
   * @name addPropertyError
   * @methodOf umbraco.services.serverValidationManager
   * @function
   *
   * @description
   * Adds an error message for the content property
   */
        function _addPropertyError(propertyAlias, culture, fieldName, errorMsg, segment) {
            if (!propertyAlias) {
                return;
            }
            //normalize culture to "invariant"
            if (!culture) {
                culture = 'invariant';
            }
            //normalize segment to null
            if (!segment) {
                segment = null;
            }
            //normalize errorMsg to empty
            if (!errorMsg) {
                errorMsg = '';
            }
            // remove all non printable chars and whitespace from the string
            // (this can be a json string for complex errors and in some test cases contains odd whitespace)
            if (Utilities.isString(errorMsg)) {
                errorMsg = errorMsg.trimStartSpecial().trim();
            }
            // if the error message is json it's a complex editor validation response that we need to parse
            if (Utilities.isString(errorMsg) && errorMsg.startsWith('[') || Utilities.isArray(errorMsg)) {
                // flatten the json structure, create validation paths for each property and add each as a property error
                var idsToErrors = parseComplexEditorError(errorMsg, propertyAlias);
                idsToErrors.forEach(function (x) {
                    return addErrorsForModelState(x.modelState, x.validationPath);
                });
                // We need to clear the error message else it will show up as a giant json block against the property
                errorMsg = '';
            }
            //only add the item if it doesn't exist                
            if (!hasPropertyError(propertyAlias, culture, fieldName, segment)) {
                errorMsgItems.push({
                    propertyAlias: propertyAlias,
                    culture: culture,
                    segment: segment,
                    fieldName: fieldName,
                    errorMsg: errorMsg
                });
            }
        }
        /**
   * @ngdoc function
   * @name hasPropertyError
   * @methodOf umbraco.services.serverValidationManager
   * @function
   *
   * @description
   * Checks if the content property + culture + field name combo has an error
   */
        function hasPropertyError(propertyAlias, culture, fieldName, segment) {
            //normalize culture to null
            if (!culture) {
                culture = 'invariant';
            }
            //normalize segment to null
            if (!segment) {
                segment = null;
            }
            var err = _.find(errorMsgItems, function (item) {
                //return true if the property alias matches and if an empty field name is specified or the field name matches
                return item.propertyAlias === propertyAlias && item.culture === culture && item.segment === segment && (item.fieldName === fieldName || fieldName === undefined || fieldName === '');
            });
            return err ? true : false;
        }
        /**
   * @ngdoc function
   * @name hasFieldError
   * @methodOf umbraco.services.serverValidationManager
   * @function
   *
   * @description
   * Checks if a content field has an error
   */
        function hasFieldError(fieldName) {
            var err = _.find(errorMsgItems, function (item) {
                //return true if the property alias matches and if an empty field name is specified or the field name matches
                return item.propertyAlias === null && item.culture === 'invariant' && item.segment === null && item.fieldName === fieldName;
            });
            return err ? true : false;
        }
        /**
   * @ngdoc function
   * @name addErrorsForModelState
   * @methodOf umbraco.services.serverValidationManager
   * @param {any} modelState the modelState object
   * @param {any} parentValidationPath optional parameter specifying a nested element's UDI for which this property belongs (for complex editors)
   * @description
   * This wires up all of the server validation model state so that valServer and valServerField directives work
   */
        function addErrorsForModelState(modelState, parentValidationPath) {
            if (!Utilities.isObject(modelState)) {
                throw 'modelState is not an object';
            }
            for (var _i2 = 0, _Object$entries2 = Object.entries(modelState); _i2 < _Object$entries2.length; _i2++) {
                var _Object$entries2$_i = _slicedToArray(_Object$entries2[_i2], 2), key = _Object$entries2$_i[0], value = _Object$entries2$_i[1];
                //This is where things get interesting....
                // We need to support validation for all editor types such as both the content and content type editors.
                // The Content editor ModelState is quite specific with the way that Properties are validated especially considering
                // that each property is a User Developer property editor.
                // The way that Content Type Editor ModelState is created is simply based on the ASP.Net validation data-annotations 
                // system. 
                // So, to do this there's some special ModelState syntax we need to know about.
                // For Content Properties, which are user defined, we know that they will exist with a prefixed
                // ModelState of "_Properties.", so if we detect this, then we know it's for a content Property.
                //the alias in model state can be in dot notation which indicates
                // * the first part is the content property alias
                // * the second part is the field to which the valiation msg is associated with
                //There will always be at least 4 parts for content properties since all model errors for properties are prefixed with "_Properties"
                //If it is not prefixed with "_Properties" that means the error is for a field of the object directly.
                // Example: "_Properties.headerImage.en-US.mySegment.myField"
                // * it's for a property since it has a _Properties prefix
                // * it's for the headerImage property type
                // * it's for the en-US culture
                // * it's for the mySegment segment
                // * it's for the myField html field (optional)
                var parts = key.split('.');
                //Check if this is for content properties - specific to content/media/member editors because those are special 
                // user defined properties with custom controls.
                if (parts.length > 1 && parts[0] === '_Properties') {
                    // create the validation key, might just be the prop alias but if it's nested will be validation path
                    // like "myBlockEditor/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/7170A4DD-2441-4B1B-A8D3-437D75C4CBC9/city"
                    var propertyValidationKey = createPropertyValidationKey(parts[1], parentValidationPath);
                    var culture = null;
                    if (parts.length > 2) {
                        culture = parts[2];
                        //special check in case the string is formatted this way
                        if (culture === 'null') {
                            culture = null;
                        }
                    }
                    var segment = null;
                    if (parts.length > 3) {
                        segment = parts[3];
                        //special check in case the string is formatted this way
                        if (segment === 'null') {
                            segment = null;
                        }
                    }
                    var htmlFieldReference = '';
                    if (parts.length > 4) {
                        htmlFieldReference = parts[4] || '';
                    }
                    // add a generic error for the property
                    _addPropertyError(propertyValidationKey, culture, htmlFieldReference, value && Array.isArray(value) && value.length > 0 ? value[0] : null, segment);
                } else {
                    //Everthing else is just a 'Field'... the field name could contain any level of 'parts' though, for example:
                    // Groups[0].Properties[2].Alias
                    addFieldError(key, value[0]);
                }
            }
            if (hasPropertyError) {
                // ensure all callbacks are called after property errors are added
                notifyCallbacks();
            }
        }
        function createPropertyValidationKey(propertyAlias, parentValidationPath) {
            return parentValidationPath ? parentValidationPath + '/' + propertyAlias : propertyAlias;
        }
        /**
   * @ngdoc function
   * @name reset
   * @methodOf umbraco.services.serverValidationManager
   * @function
   *
   * @description
   * Clears all errors and notifies all callbacks that all server errros are now valid - used when submitting a form
   */
        function reset() {
            clear();
            for (var cb in callbacks) {
                callbacks[cb].callback.apply(instance, [
                    true,
                    //pass in a value indicating it is VALID
                    [],
                    //pass in empty collection
                    [],
                    null,
                    null
                ]);
            }
        }
        /**
   * @ngdoc function
   * @name clear
   * @methodOf umbraco.services.serverValidationManager
   * @function
   *
   * @description
   * Clears all errors
   */
        function clear() {
            errorMsgItems = [];
        }
        var instance = {
            addErrorsForModelState: addErrorsForModelState,
            parseComplexEditorError: parseComplexEditorError,
            createPropertyValidationKey: createPropertyValidationKey,
            /**
     * @ngdoc function
     * @name notifyAndClearAllSubscriptions
     * @methodOf umbraco.services.serverValidationManager
     * @function
     *
     * @description
     *  This method can be called once all field and property errors are wired up. 
     * 
     *  In some scenarios where the error collection needs to be persisted over a route change 
     *   (i.e. when a content item (or any item) is created and the route redirects to the editor) 
     *   the controller should call this method once the data is bound to the scope
     *   so that any persisted validation errors are re-bound to their controls. Once they are re-binded this then clears the validation
     *   colleciton so that if another route change occurs, the previously persisted validation errors are not re-bound to the new item.
     *   
     *  In the case of content with complex editors, variants and different views, those editors don't call this method and instead 
     *  manage the server validation manually by calling notify when necessary and clear/reset when necessary.
     */
            notifyAndClearAllSubscriptions: function notifyAndClearAllSubscriptions() {
                $timeout(function () {
                    notifyCallbacks();
                    //now that they are all executed, we're gonna clear all of the errors we have
                    clear();
                });
            },
            notify: notify,
            /**
     * @ngdoc function
     * @name subscribe
     * @methodOf umbraco.services.serverValidationManager
     * @function
     * @description
     *  Adds a callback method that is executed whenever validation changes for the field name + property specified.
     *  This is generally used for server side validation in order to match up a server side validation error with 
     *  a particular field, otherwise we can only pinpoint that there is an error for a content property, not the 
     *  property's specific field. This is used with the val-server directive in which the directive specifies the 
     *  field alias to listen for.
     *  If propertyAlias is null, then this subscription is for a field property (not a user defined property).
     */
            subscribe: function subscribe(propertyAlias, culture, fieldName, callback, segment, options) {
                if (!callback) {
                    return;
                }
                var id = String.CreateGuid();
                //normalize culture to "invariant"
                if (!culture) {
                    culture = 'invariant';
                }
                //normalize segment to null
                if (!segment) {
                    segment = null;
                }
                var cb = null;
                if (propertyAlias === null) {
                    cb = {
                        propertyAlias: null,
                        culture: culture,
                        segment: segment,
                        fieldName: fieldName,
                        callback: callback,
                        id: id
                    };
                } else if (propertyAlias !== undefined) {
                    cb = {
                        propertyAlias: propertyAlias,
                        culture: culture,
                        segment: segment,
                        fieldName: fieldName,
                        callback: callback,
                        id: id,
                        options: options
                    };
                }
                callbacks.push(cb);
                function unsubscribeId() {
                    //remove all callbacks for the content field
                    callbacks = _.reject(callbacks, function (item) {
                        return item.id === id;
                    });
                }
                // Notify the new callback            
                notifyCallback(cb);
                //return a function to unsubscribe this subscription by uniqueId
                return unsubscribeId;
            },
            /**
     * Removes all callbacks registered for the propertyALias, culture and fieldName combination
     * @param {} propertyAlias 
     * @param {} culture 
     * @param {} fieldName 
     * @returns {} 
     */
            unsubscribe: function unsubscribe(propertyAlias, culture, fieldName, segment) {
                //normalize culture to "invariant"
                if (!culture) {
                    culture = 'invariant';
                }
                //normalize segment to null
                if (!segment) {
                    segment = null;
                }
                if (propertyAlias === null) {
                    //remove all callbacks for the content field
                    callbacks = _.reject(callbacks, function (item) {
                        return item.propertyAlias === null && item.culture === culture && item.segment === segment && item.fieldName === fieldName;
                    });
                } else if (propertyAlias !== undefined) {
                    //remove all callbacks for the content property
                    callbacks = _.reject(callbacks, function (item) {
                        return item.propertyAlias === propertyAlias && item.culture === culture && item.segment === segment && (item.fieldName === fieldName || (item.fieldName === undefined || item.fieldName === '') && (fieldName === undefined || fieldName === ''));
                    });
                }
            },
            getPropertyCallbacks: getPropertyCallbacks,
            getFieldCallbacks: getFieldCallbacks,
            /**
     * @ngdoc function
     * @name getCultureCallbacks
     * @methodOf umbraco.services.serverValidationManager
     * @function
     *
     * @description
     * Gets all callbacks that has been registered using the subscribe method for the culture. Not including segments.
     */
            getCultureCallbacks: function getCultureCallbacks(culture) {
                var found = _.filter(callbacks, function (item) {
                    //returns any callback that have been registered directly/ONLY against the culture
                    return item.culture === culture && item.segment === null && item.propertyAlias === null && item.fieldName === null;
                });
                return found;
            },
            getVariantCallbacks: getVariantCallbacks,
            addFieldError: addFieldError,
            addPropertyError: function addPropertyError(propertyAlias, culture, fieldName, errorMsg, segment) {
                _addPropertyError(propertyAlias, culture, fieldName, errorMsg, segment);
                notifyCallbacks();    // ensure all callbacks are called
            },
            /**
     * @ngdoc function
     * @name removePropertyError
     * @methodOf umbraco.services.serverValidationManager
     * @function
     *
     * @description
     * Removes an error message for the content property
     */
            removePropertyError: function removePropertyError(propertyAlias, culture, fieldName, segment, options) {
                var errors = getPropertyErrors(propertyAlias, culture, segment, fieldName, options);
                errorMsgItems = errorMsgItems.filter(function (v) {
                    return errors.indexOf(v) === -1;
                });
                if (errors.length > 0) {
                    // removal was successful, re-notify all subscribers
                    notifyCallbacks();
                }
            },
            reset: reset,
            clear: clear,
            /**
     * @ngdoc function
     * @name getPropertyError
     * @methodOf umbraco.services.serverValidationManager
     * @function
     *
     * @description
     * Gets the error message for the content property
     */
            getPropertyError: function getPropertyError(propertyAlias, culture, fieldName, segment) {
                var errors = getPropertyErrors(propertyAlias, culture, segment, fieldName);
                if (errors.length > 0) {
                    // should only ever contain one
                    return errors[0];
                }
                return undefined;
            },
            getPropertyErrorsByValidationPath: function getPropertyErrorsByValidationPath(propertyValidationPath, culture, segment, options) {
                return getPropertyErrors(propertyValidationPath, culture, segment, '', options);
            },
            /**
     * @ngdoc function
     * @name getFieldError
     * @methodOf umbraco.services.serverValidationManager
     * @function
     *
     * @description
     * Gets the error message for a content field
     */
            getFieldError: function getFieldError(fieldName) {
                var err = _.find(errorMsgItems, function (item) {
                    //return true if the property alias matches and if an empty field name is specified or the field name matches
                    return item.propertyAlias === null && item.culture === 'invariant' && item.segment === null && item.fieldName === fieldName;
                });
                return err;
            },
            hasPropertyError: hasPropertyError,
            hasFieldError: hasFieldError,
            /**
     * @ngdoc function
     * @name hasCultureError
     * @methodOf umbraco.services.serverValidationManager
     * @function
     *
     * @description
     * Checks if the given culture has an error
     */
            hasCultureError: function hasCultureError(culture) {
                //normalize culture to "invariant"
                if (!culture) {
                    culture = 'invariant';
                }
                var err = _.find(errorMsgItems, function (item) {
                    return item.culture === culture && item.segment === null;
                });
                return err ? true : false;
            },
            /**
     * @ngdoc function
     * @name hasVariantError
     * @methodOf umbraco.services.serverValidationManager
     * @function
     *
     * @description
     * Checks if the given culture has an error
     */
            hasVariantError: function hasVariantError(culture, segment) {
                //normalize culture to "invariant"
                if (!culture) {
                    culture = 'invariant';
                }
                //normalize segment to null
                if (!segment) {
                    segment = null;
                }
                var err = _.find(errorMsgItems, function (item) {
                    return item.culture === culture && item.segment === segment;
                });
                return err ? true : false;
            }
        };
        // Used to return the 'items' array as a reference/getter
        Object.defineProperty(instance, 'items', {
            get: function get() {
                return errorMsgItems;
            },
            set: function set(value) {
                throw 'Cannot set the items array';
            }
        });
        return instance;
    }
    angular.module('umbraco.services').factory('serverValidationManager', serverValidationManager);
    'use strict';
    //tabbable JS Lib (Wrapped in angular service)
    //https://github.com/davidtheclark/tabbable
    (function () {
        'use strict';
        function tabbableService() {
            var candidateSelectors = [
                'input',
                'select',
                'textarea',
                'a[href]',
                'button',
                '[tabindex]',
                'audio[controls]',
                'video[controls]',
                '[contenteditable]:not([contenteditable="false"])',
                'iframe[data-mce-style]'
            ];
            var candidateSelector = candidateSelectors.join(',');
            var matches = typeof Element === 'undefined' ? function () {
            } : Element.prototype.matches || Element.prototype.msMatchesSelector || Element.prototype.webkitMatchesSelector;
            function tabbable(el, options) {
                options = options || {};
                var elementDocument = el.ownerDocument || el;
                var regularTabbables = [];
                var orderedTabbables = [];
                var untouchabilityChecker = new UntouchabilityChecker(elementDocument);
                var candidates = el.querySelectorAll(candidateSelector);
                if (options.includeContainer) {
                    if (matches.call(el, candidateSelector)) {
                        candidates = Array.prototype.slice.apply(candidates);
                        candidates.unshift(el);
                    }
                }
                var i, candidate, candidateTabindex;
                for (i = 0; i < candidates.length; i++) {
                    candidate = candidates[i];
                    if (!isNodeMatchingSelectorTabbable(candidate, untouchabilityChecker))
                        continue;
                    candidateTabindex = getTabindex(candidate);
                    if (candidateTabindex === 0) {
                        regularTabbables.push(candidate);
                    } else {
                        orderedTabbables.push({
                            documentOrder: i,
                            tabIndex: candidateTabindex,
                            node: candidate
                        });
                    }
                }
                var tabbableNodes = orderedTabbables.sort(sortOrderedTabbables).map(function (a) {
                    return a.node;
                }).concat(regularTabbables);
                return tabbableNodes;
            }
            tabbable.isTabbable = isTabbable;
            tabbable.isFocusable = isFocusable;
            function isNodeMatchingSelectorTabbable(node, untouchabilityChecker) {
                if (!isNodeMatchingSelectorFocusable(node, untouchabilityChecker) || isNonTabbableRadio(node) || getTabindex(node) < 0) {
                    return false;
                }
                return true;
            }
            function isTabbable(node, untouchabilityChecker) {
                if (!node)
                    throw new Error('No node provided');
                if (matches.call(node, candidateSelector) === false)
                    return false;
                return isNodeMatchingSelectorTabbable(node, untouchabilityChecker);
            }
            function isNodeMatchingSelectorFocusable(node, untouchabilityChecker) {
                untouchabilityChecker = untouchabilityChecker || new UntouchabilityChecker(node.ownerDocument || node);
                if (node.disabled || isHiddenInput(node) || untouchabilityChecker.isUntouchable(node)) {
                    return false;
                }
                return true;
            }
            var focusableCandidateSelector = candidateSelectors.concat('iframe').join(',');
            function isFocusable(node, untouchabilityChecker) {
                if (!node)
                    throw new Error('No node provided');
                if (matches.call(node, focusableCandidateSelector) === false)
                    return false;
                return isNodeMatchingSelectorFocusable(node, untouchabilityChecker);
            }
            function getTabindex(node) {
                var tabindexAttr = parseInt(node.getAttribute('tabindex'), 10);
                if (!isNaN(tabindexAttr))
                    return tabindexAttr;
                // Browsers do not return `tabIndex` correctly for contentEditable nodes;
                // so if they don't have a tabindex attribute specifically set, assume it's 0.
                if (isContentEditable(node))
                    return 0;
                return node.tabIndex;
            }
            function sortOrderedTabbables(a, b) {
                return a.tabIndex === b.tabIndex ? a.documentOrder - b.documentOrder : a.tabIndex - b.tabIndex;
            }
            // Array.prototype.find not available in IE.
            function find(list, predicate) {
                for (var i = 0, length = list.length; i < length; i++) {
                    if (predicate(list[i]))
                        return list[i];
                }
            }
            function isContentEditable(node) {
                return node.contentEditable === 'true';
            }
            function isInput(node) {
                return node.tagName === 'INPUT';
            }
            function isHiddenInput(node) {
                return isInput(node) && node.type === 'hidden';
            }
            function isRadio(node) {
                return isInput(node) && node.type === 'radio';
            }
            function isNonTabbableRadio(node) {
                return isRadio(node) && !isTabbableRadio(node);
            }
            function getCheckedRadio(nodes) {
                for (var i = 0; i < nodes.length; i++) {
                    if (nodes[i].checked) {
                        return nodes[i];
                    }
                }
            }
            function isTabbableRadio(node) {
                if (!node.name)
                    return true;
                // This won't account for the edge case where you have radio groups with the same
                // in separate forms on the same page.
                var radioSet = node.ownerDocument.querySelectorAll('input[type="radio"][name="' + node.name + '"]');
                var checked = getCheckedRadio(radioSet);
                return !checked || checked === node;
            }
            // An element is "untouchable" if *it or one of its ancestors* has
            // `visibility: hidden` or `display: none`.
            function UntouchabilityChecker(elementDocument) {
                this.doc = elementDocument;
                // Node cache must be refreshed on every check, in case
                // the content of the element has changed. The cache contains tuples
                // mapping nodes to their boolean result.
                this.cache = [];
            }
            // getComputedStyle accurately reflects `visibility: hidden` of ancestors
            // but not `display: none`, so we need to recursively check parents.
            UntouchabilityChecker.prototype.hasDisplayNone = function hasDisplayNone(node, nodeComputedStyle) {
                if (node === this.doc.documentElement)
                    return false;
                // Search for a cached result.
                var cached = find(this.cache, function (item) {
                    return item === node;
                });
                if (cached)
                    return cached[1];
                if (!nodeComputedStyle) {
                    if (node instanceof DocumentFragment) {
                        return true;    // though DocumentFragment doesn't directly have display 'none', we know that it will never be visible, and therefore we return true. (and do not cache this, cause it will change if appended to the DOM)
                    } else {
                        nodeComputedStyle = this.doc.defaultView.getComputedStyle(node);
                    }
                }
                var result = false;
                if (nodeComputedStyle.display === 'none') {
                    result = true;
                } else if (node.parentNode) {
                    result = this.hasDisplayNone(node.parentNode);
                }
                this.cache.push([
                    node,
                    result
                ]);
                return result;
            };
            UntouchabilityChecker.prototype.isUntouchable = function isUntouchable(node) {
                if (node === this.doc.documentElement)
                    return false;
                var computedStyle = this.doc.defaultView.getComputedStyle(node);
                if (this.hasDisplayNone(node, computedStyle))
                    return true;
                return computedStyle.visibility === 'hidden';
            };
            //module.exports = tabbable;
            ////////////
            var service = {
                tabbable: tabbable,
                isTabbable: isTabbable,
                isFocusable: isFocusable
            };
            return service;
        }
        angular.module('umbraco.services').factory('tabbableService', tabbableService);
    }());
    'use strict';
    (function () {
        'use strict';
        function templateHelperService(localizationService) {
            //crappy hack due to dictionary items not in umbracoNode table
            function getInsertDictionarySnippet(nodeName) {
                return '@Umbraco.GetDictionaryValue("' + nodeName + '")';
            }
            function getInsertPartialSnippet(parentId, nodeName) {
                var partialViewName = nodeName.replace('.cshtml', '');
                if (parentId) {
                    partialViewName = parentId + '/' + partialViewName;
                }
                return '@Html.Partial("' + partialViewName + '")';
            }
            function getQuerySnippet(queryExpression) {
                var code = '\n@{\n' + '\tvar selection = ' + queryExpression + ';\n}\n';
                code += '<ul>\n' + '\t@foreach (var item in selection)\n' + '\t{\n' + '\t\t<li>\n' + '\t\t\t<a href="@item.Url">@item.Name</a>\n' + '\t\t</li>\n' + '\t}\n' + '</ul>\n\n';
                return code;
            }
            function getRenderBodySnippet() {
                return '@RenderBody()';
            }
            function getRenderSectionSnippet(sectionName, mandatory) {
                return '@RenderSection("' + sectionName + '", ' + mandatory + ')';
            }
            function getAddSectionSnippet(sectionName) {
                return '@section ' + sectionName + '\r\n{\r\n\r\n\t{0}\r\n\r\n}\r\n';
            }
            function getGeneralShortcuts() {
                var keys = [
                    'shortcuts_generalHeader',
                    'buttons_undo',
                    'buttons_redo',
                    'buttons_save'
                ];
                return localizationService.localizeMany(keys).then(function (data) {
                    var labels = {};
                    labels.header = data[0];
                    labels.undo = data[1];
                    labels.redo = data[2];
                    labels.save = data[3];
                    return {
                        'name': labels.header,
                        'shortcuts': [
                            {
                                'description': labels.undo,
                                'keys': [
                                    { 'key': 'ctrl' },
                                    { 'key': 'z' }
                                ]
                            },
                            {
                                'description': labels.redo,
                                'keys': [
                                    { 'key': 'ctrl' },
                                    { 'key': 'y' }
                                ]
                            },
                            {
                                'description': labels.save,
                                'keys': [
                                    { 'key': 'ctrl' },
                                    { 'key': 's' }
                                ]
                            }
                        ]
                    };
                });
            }
            function getEditorShortcuts() {
                var keys = [
                    'shortcuts_editorHeader',
                    'shortcuts_commentLine',
                    'shortcuts_removeLine',
                    'shortcuts_copyLineUp',
                    'shortcuts_copyLineDown',
                    'shortcuts_moveLineUp',
                    'shortcuts_moveLineDown'
                ];
                return localizationService.localizeMany(keys).then(function (data) {
                    var labels = {};
                    labels.header = data[0];
                    labels.commentline = data[1];
                    labels.removeline = data[2];
                    labels.copylineup = data[3];
                    labels.copylinedown = data[4];
                    labels.movelineup = data[5];
                    labels.movelinedown = data[6];
                    return {
                        'name': labels.header,
                        'shortcuts': [
                            {
                                'description': labels.commentline,
                                'keys': [
                                    { 'key': 'ctrl' },
                                    { 'key': '/' }
                                ]
                            },
                            {
                                'description': labels.removeline,
                                'keys': [
                                    { 'key': 'ctrl' },
                                    { 'key': 'd' }
                                ]
                            },
                            {
                                'description': labels.copylineup,
                                'keys': {
                                    'win': [
                                        { 'key': 'alt' },
                                        { 'key': 'shift' },
                                        { 'key': 'up' }
                                    ],
                                    'mac': [
                                        { 'key': 'cmd' },
                                        { 'key': 'alt' },
                                        { 'key': 'up' }
                                    ]
                                }
                            },
                            {
                                'description': labels.copylinedown,
                                'keys': {
                                    'win': [
                                        { 'key': 'alt' },
                                        { 'key': 'shift' },
                                        { 'key': 'down' }
                                    ],
                                    'mac': [
                                        { 'key': 'cmd' },
                                        { 'key': 'alt' },
                                        { 'key': 'down' }
                                    ]
                                }
                            },
                            {
                                'description': labels.movelineup,
                                'keys': [
                                    { 'key': 'alt' },
                                    { 'key': 'up' }
                                ]
                            },
                            {
                                'description': labels.movelinedown,
                                'keys': [
                                    { 'key': 'alt' },
                                    { 'key': 'down' }
                                ]
                            }
                        ]
                    };
                });
            }
            function getTemplateEditorShortcuts() {
                var keys = [
                    'template_insert',
                    'template_insertPageField',
                    'template_insertPartialView',
                    'template_insertDictionaryItem',
                    'template_insertMacro',
                    'template_queryBuilder',
                    'template_insertSections',
                    'template_mastertemplate'
                ];
                return localizationService.localizeMany(keys).then(function (data) {
                    var labels = {};
                    labels.insert = data[0];
                    labels.pagefield = data[1];
                    labels.partialview = data[2];
                    labels.dictionary = data[3];
                    labels.macro = data[4];
                    labels.querybuilder = data[5];
                    labels.sections = data[6];
                    labels.mastertemplate = data[7];
                    return {
                        'name': 'Umbraco',
                        //No need to localise Umbraco is the same in all languages :)
                        'shortcuts': [
                            {
                                'description': labels.insert.concat(' ', labels.pagefield),
                                'keys': [
                                    { 'key': 'alt' },
                                    { 'key': 'shift' },
                                    { 'key': 'v' }
                                ]
                            },
                            {
                                'description': labels.insert.concat(' ', labels.partialview),
                                'keys': [
                                    { 'key': 'alt' },
                                    { 'key': 'shift' },
                                    { 'key': 'p' }
                                ]
                            },
                            {
                                'description': labels.insert.concat(' ', labels.dictionary),
                                'keys': [
                                    { 'key': 'alt' },
                                    { 'key': 'shift' },
                                    { 'key': 'd' }
                                ]
                            },
                            {
                                'description': labels.insert.concat(' ', labels.macro),
                                'keys': [
                                    { 'key': 'alt' },
                                    { 'key': 'shift' },
                                    { 'key': 'm' }
                                ]
                            },
                            {
                                'description': labels.querybuilder,
                                'keys': [
                                    { 'key': 'alt' },
                                    { 'key': 'shift' },
                                    { 'key': 'q' }
                                ]
                            },
                            {
                                'description': labels.insert.concat(' ', labels.sections),
                                'keys': [
                                    { 'key': 'alt' },
                                    { 'key': 'shift' },
                                    { 'key': 's' }
                                ]
                            },
                            {
                                'description': labels.mastertemplate,
                                'keys': [
                                    { 'key': 'alt' },
                                    { 'key': 'shift' },
                                    { 'key': 't' }
                                ]
                            }
                        ]
                    };
                });
            }
            function getPartialViewEditorShortcuts() {
                var keys = [
                    'template_insert',
                    'template_insertPageField',
                    'template_insertDictionaryItem',
                    'template_insertMacro',
                    'template_queryBuilder'
                ];
                return localizationService.localizeMany(keys).then(function (data) {
                    var labels = {};
                    labels.insert = data[0];
                    labels.pagefield = data[1];
                    labels.dictionary = data[2];
                    labels.macro = data[3];
                    labels.querybuilder = data[4];
                    return {
                        'name': 'Umbraco',
                        //No need to localise Umbraco is the same in all languages :)
                        'shortcuts': [
                            {
                                'description': labels.insert.concat(' ', labels.pagefield),
                                'keys': [
                                    { 'key': 'alt' },
                                    { 'key': 'shift' },
                                    { 'key': 'v' }
                                ]
                            },
                            {
                                'description': labels.insert.concat(' ', labels.dictionary),
                                'keys': [
                                    { 'key': 'alt' },
                                    { 'key': 'shift' },
                                    { 'key': 'd' }
                                ]
                            },
                            {
                                'description': labels.insert.concat(' ', labels.macro),
                                'keys': [
                                    { 'key': 'alt' },
                                    { 'key': 'shift' },
                                    { 'key': 'm' }
                                ]
                            },
                            {
                                'description': labels.querybuilder,
                                'keys': [
                                    { 'key': 'alt' },
                                    { 'key': 'shift' },
                                    { 'key': 'q' }
                                ]
                            }
                        ]
                    };
                });
            }
            ////////////
            var service = {
                getInsertDictionarySnippet: getInsertDictionarySnippet,
                getInsertPartialSnippet: getInsertPartialSnippet,
                getQuerySnippet: getQuerySnippet,
                getRenderBodySnippet: getRenderBodySnippet,
                getRenderSectionSnippet: getRenderSectionSnippet,
                getAddSectionSnippet: getAddSectionSnippet,
                getGeneralShortcuts: getGeneralShortcuts,
                getEditorShortcuts: getEditorShortcuts,
                getTemplateEditorShortcuts: getTemplateEditorShortcuts,
                getPartialViewEditorShortcuts: getPartialViewEditorShortcuts
            };
            return service;
        }
        angular.module('umbraco.services').factory('templateHelper', templateHelperService);
    }());
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.tinyMceService
 *
 *
 * @description
 * A service containing all logic for all of the Umbraco TinyMCE plugins
 */
    function tinyMceService($rootScope, $q, imageHelper, $locale, $http, $timeout, stylesheetResource, macroResource, macroService, $routeParams, umbRequestHelper, angularHelper, userService, editorService, entityResource, eventsService, localStorageService) {
        //These are absolutely required in order for the macros to render inline
        //we put these as extended elements because they get merged on top of the normal allowed elements by tiny mce
        var extendedValidElements = '@[id|class|style],-div[id|dir|class|align|style],ins[datetime|cite],-ul[class|style],-li[class|style],-h1[id|dir|class|align|style],-h2[id|dir|class|align|style],-h3[id|dir|class|align|style],-h4[id|dir|class|align|style],-h5[id|dir|class|align|style],-h6[id|style|dir|class|align],span[id|class|style]';
        var fallbackStyles = [
            {
                title: 'Page header',
                block: 'h2'
            },
            {
                title: 'Section header',
                block: 'h3'
            },
            {
                title: 'Paragraph header',
                block: 'h4'
            },
            {
                title: 'Normal',
                block: 'p'
            },
            {
                title: 'Quote',
                block: 'blockquote'
            },
            {
                title: 'Code',
                block: 'code'
            }
        ];
        // these languages are available for localization
        var availableLanguages = [
            'ar',
            'ar_SA',
            'hy',
            'az',
            'eu',
            'be',
            'bn_BD',
            'bs',
            'bg_BG',
            'ca',
            'zh_CN',
            'zh_TW',
            'hr',
            'cs',
            'da',
            'dv',
            'nl',
            'en_CA',
            'en_GB',
            'et',
            'fo',
            'fi',
            'fr_FR',
            'gd',
            'gl',
            'ka_GE',
            'de',
            'de_AT',
            'el',
            'he_IL',
            'hi_IN',
            'hu_HU',
            'is_IS',
            'id',
            'it',
            'ja',
            'kab',
            'kk',
            'km_KH',
            'ko_KR',
            'ku',
            'ku_IQ',
            'lv',
            'lt',
            'lb',
            'ml',
            'ml_IN',
            'mn_MN',
            'nb_NO',
            'fa',
            'fa_IR',
            'pl',
            'pt_BR',
            'pt_PT',
            'ro',
            'ru',
            'sr',
            'si_LK',
            'sk',
            'sl_SI',
            'es',
            'es_MX',
            'sv_SE',
            'tg',
            'ta',
            'ta_IN',
            'tt',
            'th_TH',
            'tr',
            'tr_TR',
            'ug',
            'uk',
            'uk_UA',
            'vi',
            'vi_VN',
            'cy'
        ];
        //define fallback language
        var defaultLanguage = 'en_US';
        /**
   * Returns a promise of an object containing the stylesheets and styleFormats collections
   * @param {any} configuredStylesheets
   */
        function getStyles(configuredStylesheets) {
            var stylesheets = [];
            var styleFormats = [];
            var promises = [$q.when(true)];
            //a collection of promises, the first one is an empty promise
            //queue rules loading
            if (configuredStylesheets) {
                configuredStylesheets.forEach(function (val, key) {
                    if (val.indexOf(Umbraco.Sys.ServerVariables.umbracoSettings.cssPath + '/') === 0) {
                        // current format (full path to stylesheet)
                        stylesheets.push(val);
                    } else {
                        // legacy format (stylesheet name only) - must prefix with stylesheet folder and postfix with ".css"
                        stylesheets.push(Umbraco.Sys.ServerVariables.umbracoSettings.cssPath + '/' + val + '.css');
                    }
                    promises.push(stylesheetResource.getRulesByName(val).then(function (rules) {
                        rules.forEach(function (rule) {
                            var r = {};
                            r.title = rule.name;
                            if (rule.selector[0] == '.') {
                                r.inline = 'span';
                                r.classes = rule.selector.substring(1);
                            } else if (rule.selector[0] === '#') {
                                r.inline = 'span';
                                r.attributes = { id: rule.selector.substring(1) };
                            } else if (rule.selector[0] !== '.' && rule.selector.indexOf('.') > -1) {
                                var split = rule.selector.split('.');
                                r.block = split[0];
                                r.classes = rule.selector.substring(rule.selector.indexOf('.') + 1).replace(/\./g, ' ');
                            } else if (rule.selector[0] != '#' && rule.selector.indexOf('#') > -1) {
                                var split = rule.selector.split('#');
                                r.block = split[0];
                                r.classes = rule.selector.substring(rule.selector.indexOf('#') + 1);
                            } else {
                                r.block = rule.selector;
                            }
                            styleFormats.push(r);
                        });
                    }));
                });
            } else {
                styleFormats = fallbackStyles;
            }
            return $q.all(promises).then(function () {
                // Always push our Umbraco RTE stylesheet
                // So we can style macros, embed items etc...
                stylesheets.push(''.concat(Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath, '/assets/css/rte-content.css'));
                return $q.when({
                    stylesheets: stylesheets,
                    styleFormats: styleFormats
                });
            });
        }
        /** Returns the language to use for TinyMCE */
        function getLanguage() {
            var language = defaultLanguage;
            //get locale from angular and match tinymce format. Angular localization is always in the format of ru-ru, de-de, en-gb, etc.
            //wheras tinymce is in the format of ru, de, en, en_us, etc.
            var localeId = $locale.id.replace('-', '_');
            //try matching the language using full locale format
            var languageMatch = _.find(availableLanguages, function (o) {
                return o.toLowerCase() === localeId;
            });
            //if no matches, try matching using only the language
            if (languageMatch === undefined) {
                var localeParts = localeId.split('_');
                languageMatch = _.find(availableLanguages, function (o) {
                    return o === localeParts[0];
                });
            }
            //if a match was found - set the language
            if (languageMatch !== undefined) {
                language = languageMatch;
            }
            return language;
        }
        /**
   * Gets toolbars for the inlite theme
   * @param {any} configuredToolbar
   * @param {any} tinyMceConfig
   */
        function getToolbars(configuredToolbar, tinyMceConfig) {
            //the commands for selection/all
            var allowedSelectionToolbar = _.map(_.filter(tinyMceConfig.commands, function (f) {
                return f.mode === 'Selection' || f.mode === 'All';
            }), function (f) {
                return f.alias;
            });
            //the commands for insert/all
            var allowedInsertToolbar = _.map(_.filter(tinyMceConfig.commands, function (f) {
                return f.mode === 'Insert' || f.mode === 'All';
            }), function (f) {
                return f.alias;
            });
            var insertToolbar = _.filter(configuredToolbar, function (t) {
                return allowedInsertToolbar.indexOf(t) !== -1;
            }).join(' | ');
            var selectionToolbar = _.filter(configuredToolbar, function (t) {
                return allowedSelectionToolbar.indexOf(t) !== -1;
            }).join(' | ');
            return {
                insertToolbar: insertToolbar,
                selectionToolbar: selectionToolbar
            };
        }
        function uploadImageHandler(blobInfo, success, failure, progress) {
            var xhr, formData;
            xhr = new XMLHttpRequest();
            xhr.open('POST', Umbraco.Sys.ServerVariables.umbracoUrls.tinyMceApiBaseUrl + 'UploadImage');
            xhr.onloadstart = function (e) {
                angularHelper.safeApply($rootScope, function () {
                    eventsService.emit('rte.file.uploading');
                });
            };
            xhr.onloadend = function (e) {
                angularHelper.safeApply($rootScope, function () {
                    eventsService.emit('rte.file.uploaded');
                });
            };
            xhr.upload.onprogress = function (e) {
                progress(e.loaded / e.total * 100);
            };
            xhr.onerror = function () {
                failure('Image upload failed due to a XHR Transport error. Code: ' + xhr.status);
            };
            xhr.onload = function () {
                var json;
                if (xhr.status < 200 || xhr.status >= 300) {
                    failure('HTTP Error: ' + xhr.status);
                    return;
                }
                json = JSON.parse(xhr.responseText);
                if (!json || typeof json.tmpLocation !== 'string') {
                    failure('Invalid JSON: ' + xhr.responseText);
                    return;
                }
                // Put temp location into localstorage (used to update the img with data-tmpimg later on)
                localStorageService.set('tinymce__'.concat(blobInfo.blobUri()), json.tmpLocation);
                // We set the img src url to be the same as we started
                // The Blob URI is stored in TinyMce's cache
                // so the img still shows in the editor
                success(blobInfo.blobUri());
            };
            formData = new FormData();
            formData.append('file', blobInfo.blob(), blobInfo.blob().name);
            xhr.send(formData);
        }
        function cleanupPasteData(plugin, args) {
            // Remove spans
            args.content = args.content.replace(/<\s*span[^>]*>(.*?)<\s*\/\s*span>/g, '$1');
            // Convert b to strong.
            args.content = args.content.replace(/<\s*b([^>]*)>(.*?)<\s*\/\s*b([^>]*)>/g, '<strong$1>$2</strong$3>');
            // convert i to em
            args.content = args.content.replace(/<\s*i([^>]*)>(.*?)<\s*\/\s*i([^>]*)>/g, '<em$1>$2</em$3>');
        }
        function sizeImageInEditor(editor, imageDomElement, imgUrl) {
            var size = editor.dom.getSize(imageDomElement);
            if (editor.settings.maxImageSize && editor.settings.maxImageSize !== 0) {
                var newSize = imageHelper.scaleToMaxSize(editor.settings.maxImageSize, size.w, size.h);
                editor.dom.setAttrib(imageDomElement, 'width', newSize.width);
                editor.dom.setAttrib(imageDomElement, 'height', newSize.height);
                // Images inserted via Media Picker will have a URL we can use for ImageResizer QueryStrings
                // Images pasted/dragged in are not persisted to media until saved & thus will need to be added
                if (imgUrl) {
                    var src = imgUrl + '?width=' + newSize.width + '&height=' + newSize.height;
                    editor.dom.setAttrib(imageDomElement, 'data-mce-src', src);
                }
                editor.execCommand('mceAutoResize', false, null, null);
            }
        }
        function isMediaPickerEnabled(toolbarItemArray) {
            var insertMediaButtonFound = false;
            toolbarItemArray.forEach(function (toolbarItem) {
                if (toolbarItem.indexOf('umbmediapicker') > -1) {
                    insertMediaButtonFound = true;
                }
            });
            return insertMediaButtonFound;
        }
        return {
            /**
     * Returns a promise of the configuration object to initialize the TinyMCE editor
     * @param {} args
     * @returns {}
     */
            getTinyMceEditorConfig: function getTinyMceEditorConfig(args) {
                //global defaults, called before/during init
                tinymce.DOM.events.domLoaded = true;
                tinymce.baseURL = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/lib/tinymce/';
                // trailing slash important
                var promises = [
                    this.configuration(),
                    getStyles(args.stylesheets)
                ];
                return $q.all(promises).then(function (result) {
                    var tinyMceConfig = result[0];
                    var styles = result[1];
                    var toolbars = getToolbars(args.toolbar, tinyMceConfig);
                    var plugins = _.map(tinyMceConfig.plugins, function (plugin) {
                        return plugin.name;
                    });
                    //plugins that must always be active
                    plugins.push('autoresize');
                    plugins.push('noneditable');
                    var modeTheme = '';
                    var modeInline = false;
                    //Based on mode set
                    //classic = Theme: modern, inline: false
                    //inline = Theme: modern, inline: true,
                    //distraction-free = Theme: inlite, inline: true
                    switch (args.mode) {
                    case 'classic':
                        modeTheme = 'modern';
                        modeInline = false;
                        break;
                    case 'distraction-free':
                        modeTheme = 'inlite';
                        modeInline = true;
                        break;
                    default:
                        //Will default to 'classic'
                        modeTheme = 'modern';
                        modeInline = false;
                        break;
                    }
                    //create a baseline Config to exten upon
                    var config = {
                        theme: modeTheme,
                        inline: modeInline,
                        plugins: plugins,
                        valid_elements: tinyMceConfig.validElements,
                        invalid_elements: tinyMceConfig.inValidElements,
                        extended_valid_elements: extendedValidElements,
                        menubar: false,
                        statusbar: false,
                        relative_urls: false,
                        autoresize_bottom_margin: 10,
                        content_css: styles.stylesheets,
                        style_formats: styles.styleFormats,
                        language: getLanguage(),
                        //this would be for a theme other than inlite
                        toolbar: args.toolbar.join(' '),
                        //these are for the inlite theme to work
                        insert_toolbar: toolbars.insertToolbar,
                        selection_toolbar: toolbars.selectionToolbar,
                        body_class: 'umb-rte',
                        //see http://archive.tinymce.com/wiki.php/Configuration:cache_suffix
                        cache_suffix: '?umb__rnd=' + Umbraco.Sys.ServerVariables.application.cacheBuster
                    };
                    // Need to check if we are allowed to UPLOAD images
                    // This is done by checking if the insert image toolbar button is available
                    if (isMediaPickerEnabled(args.toolbar)) {
                        // Update the TinyMCE Config object to allow pasting
                        config.images_upload_handler = uploadImageHandler;
                        config.automatic_uploads = false;
                        config.images_replace_blob_uris = false;
                        // This allows images to be pasted in & stored as Base64 until they get uploaded to server
                        config.paste_data_images = true;
                    }
                    if (args.htmlId) {
                        config.selector = '#' + args.htmlId;
                    } else if (args.target) {
                        config.target = args.target;
                    }
                    /*
        // We are not ready to limit the pasted elements further than default, we will return to this feature. ( TODO: Make this feature an option. )
        // We keep spans here, cause removing spans here also removes b-tags inside of them, instead we strip them out later. (TODO: move this definition to the config file... )
        var validPasteElements = "-strong/b,-em/i,-u,-span,-p,-ol,-ul,-li,-p/div,-a[href|name],sub,sup,strike,br,del,table[width],tr,td[colspan|rowspan|width],th[colspan|rowspan|width],thead,tfoot,tbody,img[src|alt|width|height],ul,ol,li,hr,pre,dl,dt,figure,figcaption,wbr"
        
        // add elements from user configurated styleFormats to our list of validPasteElements.
        // (This means that we only allow H3-element if its configured as a styleFormat on this specific propertyEditor.)
        var style, i = 0;
        for(; i < styles.styleFormats.length; i++) {
            style = styles.styleFormats[i];
            if(style.block) {
                validPasteElements += "," + style.block;
            }
        }
        */
                    /**
         The default paste config can be overwritten by defining these properties in the customConfig.
         */
                    var pasteConfig = {
                        paste_remove_styles: true,
                        paste_text_linebreaktype: true,
                        //Converts plaintext linebreaks to br or p elements.
                        paste_strip_class_attributes: 'none',
                        //paste_word_valid_elements: validPasteElements,
                        paste_preprocess: cleanupPasteData
                    };
                    angular.extend(config, pasteConfig);
                    if (tinyMceConfig.customConfig) {
                        //if there is some custom config, we need to see if the string value of each item might actually be json and if so, we need to
                        // convert it to json instead of having it as a string since this is what tinymce requires
                        for (var i in tinyMceConfig.customConfig) {
                            var val = tinyMceConfig.customConfig[i];
                            if (val) {
                                val = val.toString().trim();
                                if (val.detectIsJson()) {
                                    try {
                                        tinyMceConfig.customConfig[i] = JSON.parse(val);
                                        //now we need to check if this custom config key is defined in our baseline, if it is we don't want to
                                        //overwrite the baseline config item if it is an array, we want to concat the items in the array, otherwise
                                        //if it's an object it will overwrite the baseline
                                        if (Utilities.isArray(config[i]) && Utilities.isArray(tinyMceConfig.customConfig[i])) {
                                            //concat it and below this concat'd array will overwrite the baseline in angular.extend
                                            tinyMceConfig.customConfig[i] = config[i].concat(tinyMceConfig.customConfig[i]);
                                        }
                                    } catch (e) {
                                    }
                                }
                                if (val === 'true') {
                                    tinyMceConfig.customConfig[i] = true;
                                }
                                if (val === 'false') {
                                    tinyMceConfig.customConfig[i] = false;
                                }
                            }
                        }
                        angular.extend(config, tinyMceConfig.customConfig);
                    }
                    return config;
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.tinyMceService#configuration
     * @methodOf umbraco.services.tinyMceService
     *
     * @description
     * Returns a collection of plugins available to the tinyMCE editor
     *
     */
            configuration: function configuration() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('rteApiBaseUrl', 'GetConfiguration'), { cache: true }), 'Failed to retrieve tinymce configuration');
            },
            /**
     * @ngdoc method
     * @name umbraco.services.tinyMceService#defaultPrevalues
     * @methodOf umbraco.services.tinyMceService
     *
     * @description
     * Returns a default configration to fallback on in case none is provided
     *
     */
            defaultPrevalues: function defaultPrevalues() {
                var cfg = {};
                cfg.toolbar = [
                    'ace',
                    'styleselect',
                    'bold',
                    'italic',
                    'alignleft',
                    'aligncenter',
                    'alignright',
                    'bullist',
                    'numlist',
                    'outdent',
                    'indent',
                    'link',
                    'umbmediapicker',
                    'umbmacro',
                    'umbembeddialog'
                ];
                cfg.stylesheets = [];
                cfg.maxImageSize = 500;
                return cfg;
            },
            /**
     * @ngdoc method
     * @name umbraco.services.tinyMceService#createInsertEmbeddedMedia
     * @methodOf umbraco.services.tinyMceService
     *
     * @description
     * Creates the umbraco insert embedded media tinymce plugin
     *
     * @param {Object} editor the TinyMCE editor instance
     */
            createInsertEmbeddedMedia: function createInsertEmbeddedMedia(editor, callback) {
                editor.addButton('umbembeddialog', {
                    icon: 'custom icon-tv',
                    tooltip: 'Embed',
                    stateSelector: 'div[data-embed-url]',
                    onclick: function onclick() {
                        // Get the selected element
                        // Check nodename is a DIV and the claslist contains 'embeditem'
                        var selectedElm = editor.selection.getNode();
                        var nodeName = selectedElm.nodeName;
                        var modify = null;
                        if (nodeName.toUpperCase() === 'DIV' && selectedElm.classList.contains('embeditem')) {
                            // See if we can go and get the attributes
                            var embedUrl = editor.dom.getAttrib(selectedElm, 'data-embed-url');
                            var embedWidth = editor.dom.getAttrib(selectedElm, 'data-embed-width');
                            var embedHeight = editor.dom.getAttrib(selectedElm, 'data-embed-height');
                            var embedConstrain = editor.dom.getAttrib(selectedElm, 'data-embed-constrain');
                            modify = {
                                url: embedUrl,
                                width: parseInt(embedWidth) || 0,
                                height: parseInt(embedHeight) || 0,
                                constrain: embedConstrain
                            };
                        }
                        if (callback) {
                            angularHelper.safeApply($rootScope, function () {
                                // pass the active element along so we can retrieve it later
                                callback(selectedElm, modify);
                            });
                        }
                    }
                });
            },
            insertEmbeddedMediaInEditor: function insertEmbeddedMediaInEditor(editor, embed, activeElement) {
                // Wrap HTML preview content here in a DIV with non-editable class of .mceNonEditable
                // This turns it into a selectable/cutable block to move about
                var wrapper = tinymce.activeEditor.dom.create('div', {
                    'class': 'mceNonEditable embeditem',
                    'data-embed-url': embed.url,
                    'data-embed-height': embed.height,
                    'data-embed-width': embed.width,
                    'data-embed-constrain': embed.constrain,
                    'contenteditable': false
                }, embed.preview);
                // Only replace if activeElement is an Embed element.
                if (activeElement && activeElement.nodeName.toUpperCase() === 'DIV' && activeElement.classList.contains('embeditem')) {
                    activeElement.replaceWith(wrapper);    // directly replaces the html node
                } else {
                    editor.selection.setNode(wrapper);
                }
            },
            createAceCodeEditor: function createAceCodeEditor(editor, callback) {
                editor.addButton('ace', {
                    icon: 'code',
                    tooltip: 'View Source Code',
                    onclick: function onclick() {
                        if (callback) {
                            angularHelper.safeApply($rootScope, function () {
                                callback();
                            });
                        }
                    }
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.tinyMceService#createMediaPicker
     * @methodOf umbraco.services.tinyMceService
     *
     * @description
     * Creates the umbraco insert media tinymce plugin
     *
     * @param {Object} editor the TinyMCE editor instance
     */
            createMediaPicker: function createMediaPicker(editor, callback) {
                editor.addButton('umbmediapicker', {
                    icon: 'custom icon-picture',
                    tooltip: 'Media Picker',
                    stateSelector: 'img[data-udi]',
                    onclick: function onclick() {
                        var selectedElm = editor.selection.getNode(), currentTarget, imgDomElement;
                        if (selectedElm.nodeName === 'IMG') {
                            var img = $(selectedElm);
                            imgDomElement = selectedElm;
                            var hasUdi = img.attr('data-udi') ? true : false;
                            var hasDataTmpImg = img.attr('data-tmpimg') ? true : false;
                            currentTarget = {
                                altText: img.attr('alt'),
                                url: img.attr('src')
                            };
                            if (hasUdi) {
                                currentTarget['udi'] = img.attr('data-udi');
                            } else {
                                currentTarget['id'] = img.attr('rel');
                            }
                            if (hasDataTmpImg) {
                                currentTarget['tmpimg'] = img.attr('data-tmpimg');
                            }
                        }
                        userService.getCurrentUser().then(function (userData) {
                            if (callback) {
                                angularHelper.safeApply($rootScope, function () {
                                    callback(currentTarget, userData, imgDomElement);
                                });
                            }
                        });
                    }
                });
            },
            insertMediaInEditor: function insertMediaInEditor(editor, img, imgDomElement) {
                if (img) {
                    // imgElement is only definied if updating an image
                    // if null/undefinied then its a BRAND new image
                    if (imgDomElement) {
                        // Check if the img src has changed
                        // If it has we will need to do some resizing/recalc again
                        var hasImageSrcChanged = false;
                        if (img.url !== editor.dom.getAttrib(imgDomElement, 'src')) {
                            hasImageSrcChanged = true;
                        }
                        // If null/undefinied it will remove the attribute
                        editor.dom.setAttrib(imgDomElement, 'alt', img.altText);
                        // It's possible to pick a NEW image - so need to ensure this gets updated
                        if (img.udi) {
                            editor.dom.setAttrib(imgDomElement, 'data-udi', img.udi);
                        }
                        // It's possible to pick a NEW image - so need to ensure this gets updated
                        if (img.url) {
                            editor.dom.setAttrib(imgDomElement, 'src', img.url);
                        }
                        // Remove width & height attributes (ONLY if imgSrc changed)
                        // So native image size is used as this needed to re-calc width & height
                        // For the function sizeImageInEditor() & apply the image resizing querystrings etc..
                        if (hasImageSrcChanged) {
                            editor.dom.setAttrib(imgDomElement, 'width', null);
                            editor.dom.setAttrib(imgDomElement, 'height', null);
                            //Re-calc the image dimensions
                            sizeImageInEditor(editor, imgDomElement, img.url);
                        }
                    } else {
                        // We need to create a NEW DOM <img> element to insert
                        // setting an attribute of ID to __mcenew, so we can gather a reference to the node, to be able to update its size accordingly to the size of the image.
                        var data = {
                            alt: img.altText || '',
                            src: img.url ? img.url : 'nothing.jpg',
                            id: '__mcenew',
                            'data-udi': img.udi
                        };
                        editor.selection.setContent(editor.dom.createHTML('img', data));
                        // Using settimeout to wait for a DoM-render, so we can find the new element by ID.
                        $timeout(function () {
                            var imgElm = editor.dom.get('__mcenew');
                            editor.dom.setAttrib(imgElm, 'id', null);
                            // When image is loaded we are ready to call sizeImageInEditor.
                            var onImageLoaded = function onImageLoaded() {
                                sizeImageInEditor(editor, imgElm, img.url);
                                editor.fire('Change');
                            };
                            // Check if image already is loaded.
                            if (imgElm.complete === true) {
                                onImageLoaded();
                            } else {
                                imgElm.onload = onImageLoaded;
                            }
                        });
                    }
                }
            },
            /**
     * @ngdoc method
     * @name umbraco.services.tinyMceService#createUmbracoMacro
     * @methodOf umbraco.services.tinyMceService
     *
     * @description
     * Creates the insert umbraco macro tinymce plugin
     *
     * @param {Object} editor the TinyMCE editor instance
     */
            createInsertMacro: function createInsertMacro(editor, callback) {
                var self = this;
                var activeMacroElement = null;
                //track an active macro element
                /** Adds custom rules for the macro plugin and custom serialization */
                editor.on('preInit', function (args) {
                    //this is requires so that we tell the serializer that a 'div' is actually allowed in the root, otherwise the cleanup will strip it out
                    editor.serializer.addRules('div');
                    /** This checks if the div is a macro container, if so, checks if its wrapped in a p tag and then unwraps it (removes p tag) */
                    editor.serializer.addNodeFilter('div', function (nodes, name) {
                        for (var i = 0; i < nodes.length; i++) {
                            if (nodes[i].attr('class') === 'umb-macro-holder' && nodes[i].parent && nodes[i].parent.name.toUpperCase() === 'P') {
                                nodes[i].parent.unwrap();
                            }
                        }
                    });
                });
                /** when the contents load we need to find any macros declared and load in their content */
                editor.on('SetContent', function (o) {
                    //get all macro divs and load their content
                    $(editor.dom.select('.umb-macro-holder.mceNonEditable')).each(function () {
                        self.loadMacroContent($(this), null, editor);
                    });
                });
                /**
       * Because the macro gets wrapped in a P tag because of the way 'enter' works, this
       * method will return the macro element if not wrapped in a p, or the p if the macro
       * element is the only one inside of it even if we are deep inside an element inside the macro
       */
                function getRealMacroElem(element) {
                    var e = $(element).closest('.umb-macro-holder');
                    if (e.length > 0) {
                        if (e.get(0).parentNode.nodeName === 'P') {
                            //now check if we're the only element
                            if (element.parentNode.childNodes.length === 1) {
                                return e.get(0).parentNode;
                            }
                        }
                        return e.get(0);
                    }
                    return null;
                }
                /** Adds the button instance */
                editor.addButton('umbmacro', {
                    icon: 'custom icon-settings-alt',
                    tooltip: 'Insert macro',
                    onPostRender: function onPostRender() {
                        var ctrl = this;
                        /**
           * Check if the macro is currently selected and toggle the menu button
           */
                        function onNodeChanged(evt) {
                            //set our macro button active when on a node of class umb-macro-holder
                            activeMacroElement = getRealMacroElem(evt.element);
                            //set the button active/inactive
                            ctrl.active(activeMacroElement !== null);
                        }
                        //NOTE: This could be another way to deal with the active/inactive state
                        //editor.on('ObjectSelected', function (e) {});
                        //set onNodeChanged event listener
                        editor.on('NodeChange', onNodeChanged);
                    },
                    /** The insert macro button click event handler */
                    onclick: function onclick() {
                        var dialogData = {
                            //flag for use in rte so we only show macros flagged for the editor
                            richTextEditor: true
                        };
                        //when we click we could have a macro already selected and in that case we'll want to edit the current parameters
                        //so we'll need to extract them and submit them to the dialog.
                        if (activeMacroElement) {
                            //we have a macro selected so we'll need to parse it's alias and parameters
                            var contents = $(activeMacroElement).contents();
                            var comment = _.find(contents, function (item) {
                                return item.nodeType === 8;
                            });
                            if (!comment) {
                                throw 'Cannot parse the current macro, the syntax in the editor is invalid';
                            }
                            var syntax = comment.textContent.trim();
                            var parsed = macroService.parseMacroSyntax(syntax);
                            dialogData = {
                                macroData: parsed,
                                activeMacroElement: activeMacroElement    //pass the active element along so we can retrieve it later
                            };
                        }
                        if (callback) {
                            angularHelper.safeApply($rootScope, function () {
                                callback(dialogData);
                            });
                        }
                    }
                });
            },
            insertMacroInEditor: function insertMacroInEditor(editor, macroObject, activeMacroElement) {
                //Important note: the TinyMce plugin "noneditable" is used here so that the macro cannot be edited,
                // for this to work the mceNonEditable class needs to come last and we also need to use the attribute contenteditable = false
                // (even though all the docs and examples say that is not necessary)
                //put the macro syntax in comments, we will parse this out on the server side to be used
                //for persisting.
                var macroSyntaxComment = '<!-- ' + macroObject.syntax + ' -->';
                //create an id class for this element so we can re-select it after inserting
                var uniqueId = 'umb-macro-' + editor.dom.uniqueId();
                var macroDiv = editor.dom.create('div', {
                    'class': 'umb-macro-holder ' + macroObject.macroAlias + ' ' + uniqueId + ' mceNonEditable',
                    'contenteditable': 'false'
                }, macroSyntaxComment + '<ins>Macro alias: <strong>' + macroObject.macroAlias + '</strong></ins>');
                //if there's an activeMacroElement then replace it, otherwise set the contents of the selected node
                if (activeMacroElement) {
                    activeMacroElement.replaceWith(macroDiv);    //directly replaces the html node
                } else {
                    editor.selection.setNode(macroDiv);
                }
                var $macroDiv = $(editor.dom.select('div.umb-macro-holder.' + uniqueId));
                editor.setDirty(true);
                //async load the macro content
                this.loadMacroContent($macroDiv, macroObject, editor);
            },
            /** loads in the macro content async from the server */
            loadMacroContent: function loadMacroContent($macroDiv, macroData, editor) {
                //if we don't have the macroData, then we'll need to parse it from the macro div
                if (!macroData) {
                    var contents = $macroDiv.contents();
                    var comment = _.find(contents, function (item) {
                        return item.nodeType === 8;
                    });
                    if (!comment) {
                        throw 'Cannot parse the current macro, the syntax in the editor is invalid';
                    }
                    var syntax = comment.textContent.trim();
                    var parsed = macroService.parseMacroSyntax(syntax);
                    macroData = parsed;
                }
                var $ins = $macroDiv.find('ins');
                //show the throbber
                $macroDiv.addClass('loading');
                // Add the contenteditable="false" attribute
                // As just the CSS class of .mceNonEditable is not working by itself?!
                // TODO: At later date - use TinyMCE editor DOM manipulation as opposed to jQuery
                $macroDiv.attr('contenteditable', 'false');
                var contentId = $routeParams.id;
                //need to wrap in safe apply since this might be occuring outside of angular
                angularHelper.safeApply($rootScope, function () {
                    macroResource.getMacroResultAsHtmlForEditor(macroData.macroAlias, contentId, macroData.macroParamsDictionary).then(function (htmlResult) {
                        $macroDiv.removeClass('loading');
                        htmlResult = htmlResult.trim();
                        if (htmlResult !== '') {
                            var wasDirty = editor.isDirty();
                            $ins.html(htmlResult);
                            if (!wasDirty) {
                                editor.undoManager.clear();
                            }
                        }
                    });
                });
            },
            createLinkPicker: function createLinkPicker(editor, onClick) {
                function createLinkList(callback) {
                    return function () {
                        var linkList = editor.settings.link_list;
                        if (typeof linkList === 'string') {
                            tinymce.util.XHR.send({
                                url: linkList,
                                success: function success(text) {
                                    callback(tinymce.util.JSON.parse(text));
                                }
                            });
                        } else {
                            callback(linkList);
                        }
                    };
                }
                function showDialog(linkList) {
                    var data = {}, selection = editor.selection, dom = editor.dom, selectedElm, anchorElm, initialText;
                    var win, linkListCtrl, relListCtrl, targetListCtrl;
                    function linkListChangeHandler(e) {
                        var textCtrl = win.find('#text');
                        if (!textCtrl.value() || e.lastControl && textCtrl.value() === e.lastControl.text()) {
                            textCtrl.value(e.control.text());
                        }
                        win.find('#href').value(e.control.value());
                    }
                    function buildLinkList() {
                        var linkListItems = [{
                                text: 'None',
                                value: ''
                            }];
                        tinymce.each(linkList, function (link) {
                            linkListItems.push({
                                text: link.text || link.title,
                                value: link.value || link.url,
                                menu: link.menu
                            });
                        });
                        return linkListItems;
                    }
                    function buildRelList(relValue) {
                        var relListItems = [{
                                text: 'None',
                                value: ''
                            }];
                        tinymce.each(editor.settings.rel_list, function (rel) {
                            relListItems.push({
                                text: rel.text || rel.title,
                                value: rel.value,
                                selected: relValue === rel.value
                            });
                        });
                        return relListItems;
                    }
                    function buildTargetList(targetValue) {
                        var targetListItems = [{
                                text: 'None',
                                value: ''
                            }];
                        if (!editor.settings.target_list) {
                            targetListItems.push({
                                text: 'New window',
                                value: '_blank'
                            });
                        }
                        tinymce.each(editor.settings.target_list, function (target) {
                            targetListItems.push({
                                text: target.text || target.title,
                                value: target.value,
                                selected: targetValue === target.value
                            });
                        });
                        return targetListItems;
                    }
                    function buildAnchorListControl(url) {
                        var anchorList = [];
                        tinymce.each(editor.dom.select('a:not([href])'), function (anchor) {
                            var id = anchor.name || anchor.id;
                            if (id) {
                                anchorList.push({
                                    text: id,
                                    value: '#' + id,
                                    selected: url.indexOf('#' + id) !== -1
                                });
                            }
                        });
                        if (anchorList.length) {
                            anchorList.unshift({
                                text: 'None',
                                value: ''
                            });
                            return {
                                name: 'anchor',
                                type: 'listbox',
                                label: 'Anchors',
                                values: anchorList,
                                onselect: linkListChangeHandler
                            };
                        }
                    }
                    function updateText() {
                        if (!initialText && data.text.length === 0) {
                            this.parent().parent().find('#text')[0].value(this.value());
                        }
                    }
                    selectedElm = selection.getNode();
                    anchorElm = dom.getParent(selectedElm, 'a[href]');
                    data.text = initialText = anchorElm ? anchorElm.innerText || anchorElm.textContent : selection.getContent({ format: 'text' });
                    data.href = anchorElm ? dom.getAttrib(anchorElm, 'href') : '';
                    data.target = anchorElm ? dom.getAttrib(anchorElm, 'target') : '';
                    data.rel = anchorElm ? dom.getAttrib(anchorElm, 'rel') : '';
                    if (selectedElm.nodeName === 'IMG') {
                        data.text = initialText = ' ';
                    }
                    if (linkList) {
                        linkListCtrl = {
                            type: 'listbox',
                            label: 'Link list',
                            values: buildLinkList(),
                            onselect: linkListChangeHandler
                        };
                    }
                    if (editor.settings.target_list !== false) {
                        targetListCtrl = {
                            name: 'target',
                            type: 'listbox',
                            label: 'Target',
                            values: buildTargetList(data.target)
                        };
                    }
                    if (editor.settings.rel_list) {
                        relListCtrl = {
                            name: 'rel',
                            type: 'listbox',
                            label: 'Rel',
                            values: buildRelList(data.rel)
                        };
                    }
                    var currentTarget = null;
                    //if we already have a link selected, we want to pass that data over to the dialog
                    if (anchorElm) {
                        var anchor = $(anchorElm);
                        currentTarget = {
                            name: anchor.attr('title'),
                            url: anchor.attr('href'),
                            target: anchor.attr('target')
                        };
                        // drop the lead char from the anchor text, if it has a value
                        var anchorVal = anchor[0].dataset.anchor;
                        if (anchorVal) {
                            currentTarget.anchor = anchorVal.substring(1);
                        }
                        //locallink detection, we do this here, to avoid poluting the editorService
                        //so the editor service can just expect to get a node-like structure
                        if (currentTarget.url.indexOf('localLink:') > 0) {
                            // if the current link has an anchor, it needs to be considered when getting the udi/id
                            // if an anchor exists, reduce the substring max by its length plus two to offset the removed prefix and trailing curly brace
                            var linkId = currentTarget.url.substring(currentTarget.url.indexOf(':') + 1, currentTarget.url.lastIndexOf('}'));
                            //we need to check if this is an INT or a UDI
                            var parsedIntId = parseInt(linkId, 10);
                            if (isNaN(parsedIntId)) {
                                //it's a UDI
                                currentTarget.udi = linkId;
                            } else {
                                currentTarget.id = linkId;
                            }
                        }
                    }
                    angularHelper.safeApply($rootScope, function () {
                        if (onClick) {
                            onClick(currentTarget, anchorElm);
                        }
                    });
                }
                editor.addButton('link', {
                    icon: 'link',
                    tooltip: 'Insert/edit link',
                    shortcut: 'Ctrl+K',
                    onclick: createLinkList(showDialog),
                    stateSelector: 'a[href]'
                });
                editor.addButton('unlink', {
                    icon: 'unlink',
                    tooltip: 'Remove link',
                    cmd: 'unlink',
                    stateSelector: 'a[href]'
                });
                editor.addShortcut('Ctrl+K', '', createLinkList(showDialog));
                this.showDialog = showDialog;
                editor.addMenuItem('link', {
                    icon: 'link',
                    text: 'Insert link',
                    shortcut: 'Ctrl+K',
                    onclick: createLinkList(showDialog),
                    stateSelector: 'a[href]',
                    context: 'insert',
                    prependToContext: true
                });
                // the editor frame catches Ctrl+S and handles it with the system save dialog
                // - we want to handle it in the content controller, so we'll emit an event instead
                editor.addShortcut('Ctrl+S', '', function () {
                    angularHelper.safeApply($rootScope, function () {
                        eventsService.emit('rte.shortcut.save');
                    });
                });
            },
            insertLinkInEditor: function insertLinkInEditor(editor, target, anchorElm) {
                var href = target.url;
                // We want to use the Udi. If it is set, we use it, else fallback to id, and finally to null
                var hasUdi = target.udi ? true : false;
                var id = hasUdi ? target.udi : target.id ? target.id : null;
                // if an anchor exists, check that it is appropriately prefixed
                if (target.anchor && target.anchor[0] !== '?' && target.anchor[0] !== '#') {
                    target.anchor = (target.anchor.indexOf('=') === -1 ? '#' : '?') + target.anchor;
                }
                // the href might be an external url, so check the value for an anchor/qs
                // href has the anchor re-appended later, hence the reset here to avoid duplicating the anchor
                if (!target.anchor) {
                    var urlParts = href.split(/(#|\?)/);
                    if (urlParts.length === 3) {
                        href = urlParts[0];
                        target.anchor = urlParts[1] + urlParts[2];
                    }
                }
                //Create a json obj used to create the attributes for the tag
                function createElemAttributes() {
                    var a = {
                        href: href,
                        title: target.name,
                        target: target.target ? target.target : null,
                        rel: target.rel ? target.rel : null
                    };
                    if (target.anchor) {
                        a['data-anchor'] = target.anchor;
                        a.href = a.href + target.anchor;
                    } else {
                        a['data-anchor'] = null;
                    }
                    return a;
                }
                function insertLink() {
                    if (anchorElm) {
                        editor.dom.setAttribs(anchorElm, createElemAttributes());
                        editor.selection.select(anchorElm);
                        editor.execCommand('mceEndTyping');
                    } else {
                        editor.execCommand('mceInsertLink', false, createElemAttributes());
                    }
                }
                if (!href && !target.anchor) {
                    editor.execCommand('unlink');
                    return;
                }
                //if we have an id, it must be a locallink:id
                if (id) {
                    href = '/{localLink:' + id + '}';
                    insertLink();
                    return;
                }
                if (!href) {
                    href = '';
                }
                // Is email and not //user@domain.com and protocol (e.g. mailto:, sip:) is not specified
                if (href.indexOf('@') > 0 && href.indexOf('//') === -1 && href.indexOf(':') === -1) {
                    // assume it's a mailto link
                    href = 'mailto:' + href;
                    insertLink();
                    return;
                }
                // Is www. prefixed
                if (/^\s*www\./i.test(href)) {
                    href = 'http://' + href;
                    insertLink();
                    return;
                }
                insertLink();
            },
            pinToolbar: function pinToolbar(editor) {
                //we can't pin the toolbar if this doesn't exist (i.e. when in distraction free mode)
                if (!editor.editorContainer) {
                    return;
                }
                var tinyMce = $(editor.editorContainer);
                var toolbar = tinyMce.find('.mce-toolbar');
                var toolbarHeight = toolbar.height();
                var tinyMceRect = editor.editorContainer.getBoundingClientRect();
                var tinyMceTop = tinyMceRect.top;
                var tinyMceBottom = tinyMceRect.bottom;
                var tinyMceWidth = tinyMceRect.width;
                var tinyMceEditArea = tinyMce.find('.mce-edit-area');
                // set padding in top of mce so the content does not "jump" up
                tinyMceEditArea.css('padding-top', toolbarHeight);
                if (tinyMceTop < 177 && 177 + toolbarHeight < tinyMceBottom) {
                    toolbar.css('position', 'fixed').css('top', '177px').css('left', 'auto').css('right', 'auto').css('width', tinyMceWidth);
                } else {
                    toolbar.css('position', 'absolute').css('left', '').css('right', '').css('top', '').css('width', '');
                }
            },
            unpinToolbar: function unpinToolbar(editor) {
                var tinyMce = $(editor.editorContainer);
                var toolbar = tinyMce.find('.mce-toolbar');
                var tinyMceEditArea = tinyMce.find('.mce-edit-area');
                // reset padding in top of mce so the content does not "jump" up
                tinyMceEditArea.css('padding-top', '0');
                toolbar.css('position', 'static');
            },
            /** Helper method to initialize the tinymce editor within Umbraco */
            initializeEditor: function initializeEditor(args) {
                if (!args.editor) {
                    throw 'args.editor is required';
                }
                //if (!args.model.value) {
                //    throw "args.model.value is required";
                //}
                // force TinyMCE to load plugins/themes from minified files (see http://archive.tinymce.com/wiki.php/api4:property.tinymce.suffix.static)
                args.editor.suffix = '.min';
                var unwatch = null;
                //Starts a watch on the model value so that we can update TinyMCE if the model changes behind the scenes or from the server
                function startWatch() {
                    unwatch = $rootScope.$watch(function () {
                        return args.model.value;
                    }, function (newVal, oldVal) {
                        if (newVal !== oldVal) {
                            //update the display val again if it has changed from the server;
                            //uses an empty string in the editor when the value is null
                            args.editor.setContent(newVal || '', { format: 'raw' });
                            //we need to manually fire this event since it is only ever fired based on loading from the DOM, this
                            // is required for our plugins listening to this event to execute
                            args.editor.fire('LoadContent', null);
                        }
                    });
                }
                //Stops the watch on model.value which is done anytime we are manually updating the model.value
                function stopWatch() {
                    if (unwatch) {
                        unwatch();
                    }
                }
                function syncContent() {
                    //stop watching before we update the value
                    stopWatch();
                    angularHelper.safeApply($rootScope, function () {
                        args.model.value = args.editor.getContent();
                    });
                    //re-watch the value
                    startWatch();
                }
                // If we can not find the insert image/media toolbar button
                // Then we need to add an event listener to the editor
                // That will update native browser drag & drop events
                // To update the icon to show you can NOT drop something into the editor
                var toolbarItems = args.editor.settings.toolbar.split(' ');
                if (isMediaPickerEnabled(toolbarItems) === false) {
                    // Wire up the event listener
                    args.editor.on('dragend dragover draggesture dragdrop drop drag', function (e) {
                        e.preventDefault();
                        e.dataTransfer.effectAllowed = 'none';
                        e.dataTransfer.dropEffect = 'none';
                        e.stopPropagation();
                    });
                }
                args.editor.on('SetContent', function (e) {
                    var content = e.content;
                    // Upload BLOB images (dragged/pasted ones)
                    // find src attribute where value starts with `blob:`
                    // search is case-insensitive and allows single or double quotes 
                    if (content.search(/src=["']blob:.*?["']/gi) !== -1) {
                        args.editor.uploadImages(function (data) {
                            // Once all images have been uploaded
                            data.forEach(function (item) {
                                // Select img element
                                var img = item.element;
                                // Get img src
                                var imgSrc = img.getAttribute('src');
                                var tmpLocation = localStorageService.get('tinymce__'.concat(imgSrc));
                                // Select the img & add new attr which we can search for
                                // When its being persisted in RTE property editor
                                // To create a media item & delete this tmp one etc
                                tinymce.activeEditor.$(img).attr({ 'data-tmpimg': tmpLocation });
                                // Resize the image to the max size configured
                                // NOTE: no imagesrc passed into func as the src is blob://...
                                // We will append ImageResizing Querystrings on perist to DB with node save
                                sizeImageInEditor(args.editor, img);
                            });
                        });
                        // Get all img where src starts with blob: AND does NOT have a data=tmpimg attribute
                        // This is most likely seen as a duplicate image that has already been uploaded
                        // editor.uploadImages() does not give us any indiciation that the image been uploaded already
                        var blobImageWithNoTmpImgAttribute = args.editor.dom.select('img[src^=\'blob:\']:not([data-tmpimg])');
                        //For each of these selected items
                        blobImageWithNoTmpImgAttribute.forEach(function (imageElement) {
                            var blobSrcUri = args.editor.dom.getAttrib(imageElement, 'src');
                            // Find the same image uploaded (Should be in LocalStorage)
                            // May already exist in the editor as duplicate image
                            // OR added to the RTE, deleted & re-added again
                            // So lets fetch the tempurl out of localstorage for that blob URI item
                            var tmpLocation = localStorageService.get('tinymce__'.concat(blobSrcUri));
                            if (tmpLocation) {
                                sizeImageInEditor(args.editor, imageElement);
                                args.editor.dom.setAttrib(imageElement, 'data-tmpimg', tmpLocation);
                            }
                        });
                    }
                });
                args.editor.on('init', function (e) {
                    if (args.model.value) {
                        args.editor.setContent(args.model.value);
                    }
                    //enable browser based spell checking
                    args.editor.getBody().setAttribute('spellcheck', true);
                    //start watching the value
                    startWatch();
                });
                args.editor.on('Change', function (e) {
                    syncContent();
                });
                //when we leave the editor (maybe)
                args.editor.on('blur', function (e) {
                    syncContent();
                });
                args.editor.on('ObjectResized', function (e) {
                    var qs = '?width=' + e.width + '&height=' + e.height + '&mode=max';
                    var srcAttr = $(e.target).attr('src');
                    var path = srcAttr.split('?')[0];
                    $(e.target).attr('data-mce-src', path + qs);
                    syncContent();
                });
                args.editor.on('Dirty', function (e) {
                    syncContent();
                    // Set model.value to the RTE's content
                    //make the form dirty manually so that the track changes works, setting our model doesn't trigger
                    // the angular bits because tinymce replaces the textarea.
                    if (args.currentForm) {
                        args.currentForm.$setDirty();
                    }
                });
                var self = this;
                //create link picker
                self.createLinkPicker(args.editor, function (currentTarget, anchorElement) {
                    entityResource.getAnchors(args.model.value).then(function (anchorValues) {
                        var linkPicker = {
                            currentTarget: currentTarget,
                            dataTypeKey: args.model.dataTypeKey,
                            ignoreUserStartNodes: args.model.config.ignoreUserStartNodes,
                            anchors: anchorValues,
                            submit: function submit(model) {
                                self.insertLinkInEditor(args.editor, model.target, anchorElement);
                                editorService.close();
                            },
                            close: function close() {
                                editorService.close();
                            }
                        };
                        editorService.linkPicker(linkPicker);
                    });
                });
                //Create the insert media plugin
                self.createMediaPicker(args.editor, function (currentTarget, userData, imgDomElement) {
                    var startNodeId, startNodeIsVirtual;
                    if (!args.model.config.startNodeId) {
                        if (args.model.config.ignoreUserStartNodes === true) {
                            startNodeId = -1;
                            startNodeIsVirtual = true;
                        } else {
                            startNodeId = userData.startMediaIds.length !== 1 ? -1 : userData.startMediaIds[0];
                            startNodeIsVirtual = userData.startMediaIds.length !== 1;
                        }
                    }
                    var mediaPicker = {
                        currentTarget: currentTarget,
                        onlyImages: true,
                        showDetails: true,
                        disableFolderSelect: true,
                        disableFocalPoint: true,
                        startNodeId: startNodeId,
                        startNodeIsVirtual: startNodeIsVirtual,
                        dataTypeKey: args.model.dataTypeKey,
                        submit: function submit(model) {
                            self.insertMediaInEditor(args.editor, model.selection[0], imgDomElement);
                            editorService.close();
                        },
                        close: function close() {
                            editorService.close();
                        }
                    };
                    editorService.mediaPicker(mediaPicker);
                });
                //Create the embedded plugin
                self.createInsertEmbeddedMedia(args.editor, function (activeElement, modify) {
                    var embed = {
                        modify: modify,
                        submit: function submit(model) {
                            self.insertEmbeddedMediaInEditor(args.editor, model.embed, activeElement);
                            editorService.close();
                        },
                        close: function close() {
                            editorService.close();
                        }
                    };
                    editorService.embed(embed);
                });
                //Create the insert macro plugin
                self.createInsertMacro(args.editor, function (dialogData) {
                    var macroPicker = {
                        dialogData: dialogData,
                        submit: function submit(model) {
                            var macroObject = macroService.collectValueData(model.selectedMacro, model.macroParams, dialogData.renderingEngine);
                            self.insertMacroInEditor(args.editor, macroObject, dialogData.activeMacroElement);
                            editorService.close();
                        },
                        close: function close() {
                            editorService.close();
                        }
                    };
                    editorService.macroPicker(macroPicker);
                });
                self.createAceCodeEditor(args.editor, function () {
                    // TODO: CHECK TO SEE WHAT WE NEED TO DO WIT MACROS (See code block?)
                    /*
        var html = editor.getContent({source_view: true});
        html = html.replace(/<span\s+class="CmCaReT"([^>]*)>([^<]*)<\/span>/gm, String.fromCharCode(chr));
        editor.dom.remove(editor.dom.select('.CmCaReT'));
        html = html.replace(/(<div class=".*?umb-macro-holder.*?mceNonEditable.*?"><!-- <\?UMBRACO_MACRO macroAlias="(.*?)".*?\/> --> *<ins>)[\s\S]*?(<\/ins> *<\/div>)/ig, "$1Macro alias: <strong>$2</strong>$3");
        */
                    var aceEditor = {
                        content: args.editor.getContent(),
                        view: 'views/propertyeditors/rte/codeeditor.html',
                        submit: function submit(model) {
                            args.editor.setContent(model.content);
                            args.editor.fire('Change');
                            editorService.close();
                        },
                        close: function close() {
                            editorService.close();
                        }
                    };
                    editorService.open(aceEditor);
                });
            }
        };
    }
    angular.module('umbraco.services').factory('tinyMceService', tinyMceService);
    'use strict';
    /**
 @ngdoc service
 * @name umbraco.services.tourService
 *
 * @description
 * <b>Added in Umbraco 7.8</b>. Application-wide service for handling tours.
 */
    (function () {
        'use strict';
        function tourService(eventsService, currentUserResource, $q, tourResource) {
            var tours = [];
            var currentTour = null;
            /**
     * Registers all tours from the server and returns a promise
     */
            function registerAllTours() {
                tours = [];
                return tourResource.getTours().then(function (tourFiles) {
                    Utilities.forEach(tourFiles, function (tourFile) {
                        Utilities.forEach(tourFile.tours, function (newTour) {
                            validateTour(newTour);
                            validateTourRegistration(newTour);
                            tours.push(newTour);
                        });
                    });
                    eventsService.emit('appState.tour.updatedTours', tours);
                });
            }
            /**
     * Method to return all of the tours as a new instance
     */
            function getTours() {
                return tours;
            }
            /**
     * @ngdoc method
     * @name umbraco.services.tourService#startTour
     * @methodOf umbraco.services.tourService
     *
     * @description
     * Raises an event to start a tour
     * @param {Object} tour The tour which should be started
     */
            function startTour(tour) {
                validateTour(tour);
                eventsService.emit('appState.tour.start', tour);
                currentTour = tour;
            }
            /**
     * @ngdoc method
     * @name umbraco.services.tourService#endTour
     * @methodOf umbraco.services.tourService
     *
     * @description
     * Raises an event to end the current tour
     */
            function endTour(tour) {
                eventsService.emit('appState.tour.end', tour);
                currentTour = null;
            }
            /**
     * Disables a tour for the user, raises an event and returns a promise
     * @param {any} tour
     */
            function disableTour(tour) {
                var deferred = $q.defer();
                tour.disabled = true;
                currentUserResource.saveTourStatus({
                    alias: tour.alias,
                    disabled: tour.disabled,
                    completed: tour.completed
                }).then(function () {
                    eventsService.emit('appState.tour.end', tour);
                    currentTour = null;
                    deferred.resolve(tour);
                });
                return deferred.promise;
            }
            /**
     * @ngdoc method
     * @name umbraco.services.tourService#completeTour
     * @methodOf umbraco.services.tourService
     *
     * @description
     * Completes a tour for the user, raises an event and returns a promise
     * @param {Object} tour The tour which should be completed
     */
            function completeTour(tour) {
                var deferred = $q.defer();
                tour.completed = true;
                currentUserResource.saveTourStatus({
                    alias: tour.alias,
                    disabled: tour.disabled,
                    completed: tour.completed
                }).then(function () {
                    eventsService.emit('appState.tour.complete', tour);
                    currentTour = null;
                    deferred.resolve(tour);
                });
                return deferred.promise;
            }
            /**
     * @ngdoc method
     * @name umbraco.services.tourService#getCurrentTour
     * @methodOf umbraco.services.tourService
     *
     * @description
     * Returns the current tour
     * @returns {Object} Returns the current tour
     */
            function getCurrentTour() {
                // TODO: This should be reset if a new user logs in
                return currentTour;
            }
            /**
     * @ngdoc method
     * @name umbraco.services.tourService#getGroupedTours
     * @methodOf umbraco.services.tourService
     *
     * @description
     * Returns a promise of grouped tours with the current user statuses
     * @returns {Array} All registered tours grouped by tour group
     */
            function getGroupedTours() {
                var deferred = $q.defer();
                var tours = getTours();
                setTourStatuses(tours).then(function () {
                    var groupedTours = [];
                    tours.forEach(function (item) {
                        if (item.contentType === null || item.contentType === '') {
                            var groupExists = false;
                            var newGroup = {
                                'group': '',
                                'tours': []
                            };
                            groupedTours.forEach(function (group) {
                                // extend existing group if it is already added
                                if (group.group === item.group) {
                                    if (item.groupOrder) {
                                        group.groupOrder = item.groupOrder;
                                    }
                                    groupExists = true;
                                    if (item.hidden === false) {
                                        group.tours.push(item);
                                    }
                                }
                            });
                            // push new group to array if it doesn't exist
                            if (!groupExists) {
                                newGroup.group = item.group;
                                if (item.groupOrder) {
                                    newGroup.groupOrder = item.groupOrder;
                                }
                                if (item.hidden === false) {
                                    newGroup.tours.push(item);
                                    groupedTours.push(newGroup);
                                }
                            }
                        }
                    });
                    deferred.resolve(groupedTours);
                });
                return deferred.promise;
            }
            /**
     * @ngdoc method
     * @name umbraco.services.tourService#getTourByAlias
     * @methodOf umbraco.services.tourService
     *
     * @description
     * Returns a promise of the tour found by alias with the current user statuses
     * @param {Object} tourAlias The tour alias of the tour which should be returned
     * @returns {Object} Tour object
     */
            function getTourByAlias(tourAlias) {
                var deferred = $q.defer();
                var tours = getTours();
                setTourStatuses(tours).then(function () {
                    var tour = _.findWhere(tours, { alias: tourAlias });
                    deferred.resolve(tour);
                });
                return deferred.promise;
            }
            /**
    * @ngdoc method
    * @name umbraco.services.tourService#getToursForDoctype
    * @methodOf umbraco.services.tourService
    *
    * @description
    * Returns a promise of the tours found by documenttype alias.
    * @param {Object} doctypeAlias The doctype alias for which  the tours which should be returned
    * @returns {Array} An array of tour objects for the doctype
    */
            function getToursForDoctype(doctypeAlias) {
                var deferred = $q.defer();
                tourResource.getToursForDoctype(doctypeAlias).then(function (tours) {
                    deferred.resolve(tours);
                });
                return deferred.promise;
            }
            ///////////
            /**
     * Validates a tour object and makes sure it consists of the correct properties needed to start a tour
     * @param {any} tour
     */
            function validateTour(tour) {
                if (!tour) {
                    throw 'A tour is not specified';
                }
                if (!tour.alias) {
                    throw 'A tour alias is required';
                }
                if (!tour.steps) {
                    throw 'Tour ' + tour.alias + ' is missing tour steps';
                }
                if (tour.steps && tour.steps.length === 0) {
                    throw 'Tour ' + tour.alias + ' is missing tour steps';
                }
                if (tour.requiredSections.length === 0) {
                    throw 'Tour ' + tour.alias + ' is missing the required sections';
                }
            }
            /**
     * Validates a tour before it gets registered in the service
     * @param {any} tour
     */
            function validateTourRegistration(tour) {
                // check for existing tours with the same alias
                Utilities.forEach(tours, function (existingTour) {
                    if (existingTour.alias === tour.alias) {
                        throw 'A tour with the alias ' + tour.alias + ' is already registered';
                    }
                });
            }
            /**
     * Based on the tours given, this will set each of the tour statuses (disabled/completed) based on what is stored against the current user
     * @param {any} tours
     */
            function setTourStatuses(tours) {
                var deferred = $q.defer();
                currentUserResource.getTours().then(function (storedTours) {
                    Utilities.forEach(storedTours, function (storedTour) {
                        if (storedTour.completed === true) {
                            Utilities.forEach(tours, function (tour) {
                                if (storedTour.alias === tour.alias) {
                                    tour.completed = true;
                                }
                            });
                        }
                        if (storedTour.disabled === true) {
                            Utilities.forEach(tours, function (tour) {
                                if (storedTour.alias === tour.alias) {
                                    tour.disabled = true;
                                }
                            });
                        }
                    });
                    deferred.resolve(tours);
                });
                return deferred.promise;
            }
            var service = {
                registerAllTours: registerAllTours,
                startTour: startTour,
                endTour: endTour,
                disableTour: disableTour,
                completeTour: completeTour,
                getCurrentTour: getCurrentTour,
                getGroupedTours: getGroupedTours,
                getTourByAlias: getTourByAlias,
                getToursForDoctype: getToursForDoctype
            };
            return service;
        }
        angular.module('umbraco.services').factory('tourService', tourService);
    }());
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.treeService
 * @function
 *
 * @description
 * The tree service factory, used internally by the umbTree and umbTreeItem directives
 */
    function treeService($q, treeResource, iconHelper, notificationsService, eventsService) {
        //SD: Have looked at putting this in sessionStorage (not localStorage since that means you wouldn't be able to work
        // in multiple tabs) - however our tree structure is cyclical, meaning a node has a reference to it's parent and it's children
        // which you cannot serialize to sessionStorage. There's really no benefit of session storage except that you could refresh
        // a tab and have the trees where they used to be - supposed that is kind of nice but would mean we'd have to store the parent
        // as a nodeid reference instead of a variable with a getParent() method.
        var treeCache = {};
        var standardCssClass = 'icon umb-tree-icon sprTree';
        function getCacheKey(args) {
            //if there is no cache key they return null - it won't be cached.
            if (!args || !args.cacheKey) {
                return null;
            }
            var cacheKey = args.cacheKey;
            cacheKey += '_' + args.section;
            return cacheKey;
        }
        // Adapted from: https://stackoverflow.com/a/2140723
        // Please note, we can NOT test this functionality correctly in Phantom because it implements
        // the localeCompare method incorrectly: https://github.com/ariya/phantomjs/issues/11063
        function invariantEquals(a, b) {
            return typeof a === 'string' && typeof b === 'string' ? a.localeCompare(b, undefined, { sensitivity: 'base' }) === 0 : a === b;
        }
        return {
            /** Internal method to return the tree cache */
            _getTreeCache: function _getTreeCache() {
                return treeCache;
            },
            /** Internal method to track expanded paths on a tree */
            _trackExpandedPaths: function _trackExpandedPaths(node, expandedPaths) {
                if (!node.children || !Utilities.isArray(node.children) || node.children.length == 0) {
                    return;
                }
                //take the last child
                var childPath = this.getPath(node.children[node.children.length - 1]).join(',');
                //check if this already exists, if so exit
                if (expandedPaths.includes(childPath)) {
                    return;
                }
                if (expandedPaths.length === 0) {
                    expandedPaths.push(childPath);
                    //track it
                    return;
                }
                var clonedPaths = expandedPaths.slice(0);
                //make a copy to iterate over so we can modify the original in the iteration
                clonedPaths.forEach(function (p) {
                    if (childPath.startsWith(p + ',')) {
                        //this means that the node's path supercedes this path stored so we can remove the current 'p' and replace it with node.path
                        expandedPaths.splice(expandedPaths.indexOf(p), 1);
                        //remove it
                        if (expandedPaths.includes(childPath) === false) {
                            expandedPaths.push(childPath);    //replace it
                        }
                    } else if (p.startsWith(childPath + ',')) {
                    } else if (expandedPaths.includes(childPath) === false) {
                        expandedPaths.push(childPath);    //track it
                    }
                });
            },
            /** Internal method that ensures there's a routePath, parent and level property on each tree node and adds some icon specific properties so that the nodes display properly */
            _formatNodeDataForUseInUI: function _formatNodeDataForUseInUI(parentNode, treeNodes, section, level) {
                //if no level is set, then we make it 1
                var childLevel = level ? level : 1;
                //set the section if it's not already set
                if (!parentNode.section) {
                    parentNode.section = section;
                }
                if (parentNode.metaData && parentNode.metaData.noAccess === true) {
                    if (!parentNode.cssClasses) {
                        parentNode.cssClasses = [];
                    }
                    parentNode.cssClasses.push('no-access');
                }
                //create a method outside of the loop to return the parent - otherwise jshint blows up
                var funcParent = function funcParent() {
                    return parentNode;
                };
                for (var i = 0; i < treeNodes.length; i++) {
                    var treeNode = treeNodes[i];
                    treeNode.level = childLevel;
                    //create a function to get the parent node, we could assign the parent node but
                    // then we cannot serialize this entity because we have a cyclical reference.
                    // Instead we just make a function to return the parentNode.
                    treeNode.parent = funcParent;
                    //set the section for each tree node - this allows us to reference this easily when accessing tree nodes
                    treeNode.section = section;
                    //if there is not route path specified, then set it automatically,
                    //if this is a tree root node then we want to route to the section's dashboard
                    if (!treeNode.routePath) {
                        if (treeNode.metaData && treeNode.metaData['treeAlias']) {
                            //this is a root node
                            treeNode.routePath = section;
                        } else {
                            var treeAlias = this.getTreeAlias(treeNode);
                            treeNode.routePath = section + '/' + treeAlias + '/edit/' + treeNode.id;
                        }
                    }
                    //now, format the icon data
                    if (treeNode.iconIsClass === undefined || treeNode.iconIsClass) {
                        var converted = iconHelper.convertFromLegacyTreeNodeIcon(treeNode);
                        treeNode.cssClass = standardCssClass + ' ' + converted;
                        if (converted && converted.startsWith('.')) {
                            //its legacy so add some width/height
                            treeNode.style = 'height:16px;width:16px;';
                        } else {
                            treeNode.style = '';
                        }
                    } else {
                        treeNode.style = 'background-image: url(\'' + treeNode.iconFilePath + '\');';
                        //we need an 'icon-' class in there for certain styles to work so if it is image based we'll add this
                        treeNode.cssClass = standardCssClass + ' legacy-custom-file';
                    }
                    if (treeNode.metaData && treeNode.metaData.noAccess === true) {
                        if (!treeNode.cssClasses) {
                            treeNode.cssClasses = [];
                        }
                        treeNode.cssClasses.push('no-access');
                    }
                }
            },
            /**
     * @ngdoc method
     * @name umbraco.services.treeService#getTreePackageFolder
     * @methodOf umbraco.services.treeService
     * @function
     *
     * @description
     * Determines if the current tree is a plugin tree and if so returns the package folder it has declared
     * so we know where to find it's views, otherwise it will just return undefined.
     *
     * @param {String} treeAlias The tree alias to check
     */
            getTreePackageFolder: function getTreePackageFolder(treeAlias) {
                //we determine this based on the server variables
                if (Umbraco.Sys.ServerVariables.umbracoPlugins && Umbraco.Sys.ServerVariables.umbracoPlugins.trees && Utilities.isArray(Umbraco.Sys.ServerVariables.umbracoPlugins.trees)) {
                    var found = _.find(Umbraco.Sys.ServerVariables.umbracoPlugins.trees, function (item) {
                        return invariantEquals(item.alias, treeAlias);
                    });
                    return found ? found.packageFolder : undefined;
                }
                return undefined;
            },
            /**
     * @ngdoc method
     * @name umbraco.services.treeService#clearCache
     * @methodOf umbraco.services.treeService
     * @function
     *
     * @description
     * Clears the tree cache - with optional cacheKey, optional section or optional filter.
     *
     * @param {Object} args arguments
     * @param {String} args.cacheKey optional cachekey - this is used to clear specific trees in dialogs
     * @param {String} args.section optional section alias - clear tree for a given section
     * @param {String} args.childrenOf optional parent ID - only clear the cache below a specific node
     */
            clearCache: function clearCache(args) {
                //clear all if not specified
                if (!args) {
                    treeCache = {};
                } else {
                    //if section and cache key specified just clear that cache
                    if (args.section && args.cacheKey) {
                        var cacheKey = getCacheKey(args);
                        if (cacheKey && treeCache && treeCache[cacheKey] != null) {
                            treeCache = _.omit(treeCache, cacheKey);
                        }
                    } else if (args.childrenOf) {
                        //if childrenOf is supplied a cacheKey must be supplied as well
                        if (!args.cacheKey) {
                            throw 'args.cacheKey is required if args.childrenOf is supplied';
                        }
                        //this will clear out all children for the parentId passed in to this parameter, we'll
                        // do this by recursing and specifying a filter
                        var self = this;
                        this.clearCache({
                            cacheKey: args.cacheKey,
                            filter: function filter(cc) {
                                //get the new parent node from the tree cache
                                var parent = self.getDescendantNode(cc.root, args.childrenOf);
                                if (parent) {
                                    //clear it's children and set to not expanded
                                    parent.children = null;
                                    parent.expanded = false;
                                }
                                //return the cache to be saved
                                return cc;
                            }
                        });
                    } else if (args.filter && Utilities.isFunction(args.filter)) {
                        //if a filter is supplied a cacheKey must be supplied as well
                        if (!args.cacheKey) {
                            throw 'args.cacheKey is required if args.filter is supplied';
                        }
                        //if a filter is supplied the function needs to return the data to keep
                        var byKey = treeCache[args.cacheKey];
                        if (byKey) {
                            var result = args.filter(byKey);
                            if (result) {
                                //set the result to the filtered data
                                treeCache[args.cacheKey] = result;
                            } else {
                                //remove the cache
                                treeCache = _.omit(treeCache, args.cacheKey);
                            }
                        }
                    } else if (args.cacheKey) {
                        //if only the cache key is specified, then clear all cache starting with that key
                        var allKeys1 = _.keys(treeCache);
                        var toRemove1 = _.filter(allKeys1, function (k) {
                            return k.startsWith(args.cacheKey + '_');
                        });
                        treeCache = _.omit(treeCache, toRemove1);
                    } else if (args.section) {
                        //if only the section is specified then clear all cache regardless of cache key by that section
                        var allKeys2 = _.keys(treeCache);
                        var toRemove2 = _.filter(allKeys2, function (k) {
                            return k.endsWith('_' + args.section);
                        });
                        treeCache = _.omit(treeCache, toRemove2);
                    }
                }
            },
            /**
     * @ngdoc method
     * @name umbraco.services.treeService#loadNodeChildren
     * @methodOf umbraco.services.treeService
     * @function
     *
     * @description
     * Clears all node children, gets it's up-to-date children from the server and re-assigns them and then
     * returns them in a promise.
     * @param {object} args An arguments object
     * @param {object} args.node The tree node
     * @param {object} args.section The current section
     */
            loadNodeChildren: function loadNodeChildren(args) {
                if (!args) {
                    throw 'No args object defined for loadNodeChildren';
                }
                if (!args.node) {
                    throw 'No node defined on args object for loadNodeChildren';
                }
                // don't remove the children for container nodes in dialogs, as it'll remove the right arrow indicator
                if (!args.isDialog || !args.node.metaData.isContainer) {
                    this.removeChildNodes(args.node);
                }
                args.node.loading = true;
                return this.getChildren(args).then(function (data) {
                    //set state to done and expand (only if there actually are children!)
                    args.node.loading = false;
                    args.node.children = data;
                    if (args.node.children && args.node.children.length > 0) {
                        args.node.expanded = true;
                        args.node.hasChildren = true;
                        //Since we've removed the children &  reloaded them, we need to refresh the UI now because the tree node UI doesn't operate on normal angular $watch since that will be pretty slow
                        if (Utilities.isFunction(args.node.updateNodeData)) {
                            args.node.updateNodeData(args.node);
                        }
                    }
                    return $q.when(data);
                }, function (reason) {
                    //in case of error, emit event
                    eventsService.emit('treeService.treeNodeLoadError', { error: reason });
                    //stop show the loading indicator
                    args.node.loading = false;
                    //tell notications about the error
                    notificationsService.error(reason);
                    return $q.reject(reason);
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.treeService#removeNode
     * @methodOf umbraco.services.treeService
     * @function
     *
     * @description
     * Removes a given node from the tree
     * @param {object} treeNode the node to remove
     */
            removeNode: function removeNode(treeNode) {
                if (!Utilities.isFunction(treeNode.parent)) {
                    return;
                }
                if (treeNode.parent() == null) {
                    throw 'Cannot remove a node that doesn\'t have a parent';
                }
                //remove the current item from it's siblings
                var parent = treeNode.parent();
                parent.children.splice(parent.children.indexOf(treeNode), 1);
                parent.hasChildren = parent.children.length !== 0;
                //Notify that the node has been removed
                eventsService.emit('treeService.removeNode', { node: treeNode });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.treeService#removeChildNodes
     * @methodOf umbraco.services.treeService
     * @function
     *
     * @description
     * Removes all child nodes from a given tree node
     * @param {object} treeNode the node to remove children from
     */
            removeChildNodes: function removeChildNodes(treeNode) {
                treeNode.expanded = false;
                treeNode.children = [];
                treeNode.hasChildren = false;
            },
            /**
     * @ngdoc method
     * @name umbraco.services.treeService#getChildNode
     * @methodOf umbraco.services.treeService
     * @function
     *
     * @description
     * Gets a child node with a given ID, from a specific treeNode
     * @param {object} treeNode to retrive child node from
     * @param {int} id id of child node
     */
            getChildNode: function getChildNode(treeNode, id) {
                if (!treeNode.children) {
                    return null;
                }
                var found = _.find(treeNode.children, function (child) {
                    return String(child.id).toLowerCase() === String(id).toLowerCase();
                });
                return found === undefined ? null : found;
            },
            /**
     * @ngdoc method
     * @name umbraco.services.treeService#getDescendantNode
     * @methodOf umbraco.services.treeService
     * @function
     *
     * @description
     * Gets a descendant node by id
     * @param {object} treeNode to retrive descendant node from
     * @param {int} id id of descendant node
     * @param {string} treeAlias - optional tree alias, if fetching descendant node from a child of a listview document
     */
            getDescendantNode: function getDescendantNode(treeNode, id, treeAlias) {
                //validate if it is a section container since we'll need a treeAlias if it is one
                if (treeNode.isContainer === true && !treeAlias) {
                    throw 'Cannot get a descendant node from a section container node without a treeAlias specified';
                }
                //the treeNode passed in could be a section container, or it could be a section group
                //in either case we need to go through the children until we can find the actual tree root with the treeAlias
                var self = this;
                function getTreeRoot(tn) {
                    //if it is a section container, we need to find the tree to be searched
                    if (tn.isContainer) {
                        for (var c = 0; c < tn.children.length; c++) {
                            if (tn.children[c].isContainer) {
                                //recurse
                                var root = getTreeRoot(tn.children[c]);
                                //only return if we found the root in this child, otherwise continue.
                                if (root) {
                                    return root;
                                }
                            } else if (self.getTreeAlias(tn.children[c]) === treeAlias) {
                                return tn.children[c];
                            }
                        }
                        return null;
                    } else {
                        return tn;
                    }
                }
                var foundRoot = getTreeRoot(treeNode);
                if (!foundRoot) {
                    throw 'Could not find a tree in the current section with alias ' + treeAlias;
                }
                treeNode = foundRoot;
                //check this node
                if (treeNode.id === id) {
                    return treeNode;
                }
                //check the first level
                var found = this.getChildNode(treeNode, id);
                if (found) {
                    return found;
                }
                //check each child of this node
                if (!treeNode.children) {
                    return null;
                }
                for (var i = 0; i < treeNode.children.length; i++) {
                    var child = treeNode.children[i];
                    if (child.children && Utilities.isArray(child.children) && child.children.length > 0) {
                        //recurse
                        found = this.getDescendantNode(child, id);
                        if (found) {
                            return found;
                        }
                    }
                }
                //not found
                return found === undefined ? null : found;
            },
            /**
     * @ngdoc method
     * @name umbraco.services.treeService#getTreeRoot
     * @methodOf umbraco.services.treeService
     * @function
     *
     * @description
     * Gets the root node of the current tree type for a given tree node
     * @param {object} treeNode to retrive tree root node from
     */
            getTreeRoot: function getTreeRoot(treeNode) {
                if (!treeNode) {
                    throw 'treeNode cannot be null';
                }
                //all root nodes have metadata key 'treeAlias'
                var root = null;
                var current = treeNode;
                while (root === null && current) {
                    if (current.metaData && current.metaData['treeAlias']) {
                        root = current;
                    } else if (Utilities.isFunction(current.parent)) {
                        //we can only continue if there is a parent() method which means this
                        // tree node was loaded in as part of a real tree, not just as a single tree
                        // node from the server.
                        current = current.parent();
                    } else {
                        current = null;
                    }
                }
                return root;
            },
            /** Gets the node's tree alias, this is done by looking up the meta-data of the current node's root node */
            /**
     * @ngdoc method
     * @name umbraco.services.treeService#getTreeAlias
     * @methodOf umbraco.services.treeService
     * @function
     *
     * @description
     * Gets the node's tree alias, this is done by looking up the meta-data of the current node's root node
     * @param {object} treeNode to retrive tree alias from
     */
            getTreeAlias: function getTreeAlias(treeNode) {
                var root = this.getTreeRoot(treeNode);
                if (root) {
                    return root.metaData['treeAlias'];
                }
                return '';
            },
            /**
     * @ngdoc method
     * @name umbraco.services.treeService#getTree
     * @methodOf umbraco.services.treeService
     * @function
     *
     * @description
     * gets the tree, returns a promise
     * @param {object} args Arguments
     * @param {string} args.section Section alias
     * @param {string} args.cacheKey Optional cachekey
     */
            getTree: function getTree(args) {
                //set defaults
                if (!args) {
                    args = {
                        section: 'content',
                        cacheKey: null
                    };
                } else if (!args.section) {
                    args.section = 'content';
                }
                var cacheKey = getCacheKey(args);
                //return the cache if it exists
                if (cacheKey && treeCache[cacheKey] !== undefined) {
                    return $q.when(treeCache[cacheKey]);
                }
                var self = this;
                return treeResource.loadApplication(args).then(function (data) {
                    //this will be called once the tree app data has loaded
                    var result = {
                        name: data.name,
                        alias: args.section,
                        root: data
                    };
                    //format the root
                    self._formatNodeDataForUseInUI(result.root, result.root.children, args.section);
                    //if this is a root that contains group nodes, we need to format those manually too
                    if (result.root.containsGroups) {
                        for (var i = 0; i < result.root.children.length; i++) {
                            var group = result.root.children[i];
                            //we need to format/modify some of the node data to be used in our app.
                            self._formatNodeDataForUseInUI(group, group.children, args.section);
                        }
                    }
                    //cache this result if a cache key is specified - generally a cache key should ONLY
                    // be specified for application trees, dialog trees should not be cached.
                    if (cacheKey) {
                        treeCache[cacheKey] = result;
                        return $q.when(treeCache[cacheKey]);
                    }
                    //return un-cached
                    return $q.when(result);
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.treeService#getMenu
     * @methodOf umbraco.services.treeService
     * @function
     *
     * @description
     * Returns available menu actions for a given tree node
     * @param {object} args Arguments
     * @param {string} args.treeNode tree node object to retrieve the menu for
     */
            getMenu: function getMenu(args) {
                if (!args) {
                    throw 'args cannot be null';
                }
                if (!args.treeNode) {
                    throw 'args.treeNode cannot be null';
                }
                return treeResource.loadMenu(args.treeNode).then(function (data) {
                    //need to convert the icons to new ones
                    for (var i = 0; i < data.length; i++) {
                        data[i].cssclass = iconHelper.convertFromLegacyIcon(data[i].cssclass);
                    }
                    return data;
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.treeService#getChildren
     * @methodOf umbraco.services.treeService
     * @function
     *
     * @description
     * Gets the children from the server for a given node
     * @param {object} args Arguments
     * @param {object} args.node tree node object to retrieve the children for
     * @param {string} args.section current section alias
     */
            getChildren: function getChildren(args) {
                if (!args) {
                    throw 'No args object defined for getChildren';
                }
                if (!args.node) {
                    throw 'No node defined on args object for getChildren';
                }
                var section = args.section || 'content';
                var treeItem = args.node;
                var self = this;
                return treeResource.loadNodes({ node: treeItem }).then(function (data) {
                    //now that we have the data, we need to add the level property to each item and the view
                    self._formatNodeDataForUseInUI(treeItem, data, section, treeItem.level + 1);
                    return $q.when(data);
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.treeService#reloadNode
     * @methodOf umbraco.services.treeService
     * @function
     *
     * @description
     * Re-loads the single node from the server
     * @param {object} node Tree node to reload
     */
            reloadNode: function reloadNode(node) {
                if (!node) {
                    throw 'node cannot be null';
                }
                if (!node.parent()) {
                    throw 'cannot reload a single node without a parent';
                }
                if (!node.section) {
                    throw 'cannot reload a single node without an assigned node.section';
                }
                //set the node to loading
                node.loading = true;
                return this.getChildren({
                    node: node.parent(),
                    section: node.section
                }).then(function (data) {
                    //ok, now that we have the children, find the node we're reloading
                    var found = _.find(data, function (item) {
                        return item.id === node.id;
                    });
                    if (found) {
                        //now we need to find the node in the parent.children collection to replace
                        var index = _.indexOf(node.parent().children, node);
                        //the trick here is to not actually replace the node - this would cause the delete animations
                        //to fire, instead we're just going to replace all the properties of this node.
                        //there should always be a method assigned but we'll check anyways
                        if (Utilities.isFunction(node.parent().children[index].updateNodeData)) {
                            node.parent().children[index].updateNodeData(found);
                        } else {
                            //just update as per normal - this means styles, etc.. won't be applied
                            _.extend(node.parent().children[index], found);
                        }
                        //set the node loading
                        node.parent().children[index].loading = false;
                        //return
                        return $q.when(node.parent().children[index]);
                    } else {
                        return $q.reject();
                    }
                }, function () {
                    return $q.reject();
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.services.treeService#getPath
     * @methodOf umbraco.services.treeService
     * @function
     *
     * @description
     * This will return the current node's path by walking up the tree
     * @param {object} node Tree node to retrieve path for
     */
            getPath: function getPath(node) {
                if (!node) {
                    throw 'node cannot be null';
                }
                if (!Utilities.isFunction(node.parent)) {
                    throw 'node.parent is not a function, the path cannot be resolved';
                }
                var reversePath = [];
                var current = node;
                while (current != null) {
                    reversePath.push(current.id);
                    //all tree root nodes (non group, not section root) have a treeAlias so exit if that is the case
                    //or exit if we cannot traverse further up
                    if (current.metaData && current.metaData['treeAlias'] || !current.parent) {
                        current = null;
                    } else {
                        current = current.parent();
                    }
                }
                return reversePath.reverse();
            },
            syncTree: function syncTree(args) {
                if (!args) {
                    throw 'No args object defined for syncTree';
                }
                if (!args.node) {
                    throw 'No node defined on args object for syncTree';
                }
                if (!args.path) {
                    throw 'No path defined on args object for syncTree';
                }
                if (!Utilities.isArray(args.path)) {
                    throw 'Path must be an array';
                }
                if (args.path.length < 1) {
                    //if there is no path, make -1 the path, and that should sync the tree root
                    args.path.push('-1');
                }
                //get the rootNode for the current node, we'll sync based on that
                var root = this.getTreeRoot(args.node);
                if (!root) {
                    throw 'Could not get the root tree node based on the node passed in';
                }
                //now we want to loop through the ids in the path, first we'll check if the first part
                //of the path is the root node, otherwise we'll search it's children.
                var currPathIndex = 0;
                //if the first id is the root node and there's only one... then consider it synced
                if (String(args.path[currPathIndex]).toLowerCase() === String(args.node.id).toLowerCase()) {
                    if (args.path.length === 1) {
                        //return the root
                        return $q.when(root);
                    } else {
                        //move to the next path part and continue
                        currPathIndex = 1;
                    }
                }
                var deferred = $q.defer();
                //now that we have the first id to lookup, we can start the process
                var self = this;
                var node = args.node;
                var doSync = function doSync() {
                    //check if it exists in the already loaded children
                    var child = self.getChildNode(node, args.path[currPathIndex]);
                    if (child) {
                        if (args.path.length === currPathIndex + 1) {
                            //woot! synced the node
                            if (!args.forceReload) {
                                return $q.when(child);
                            } else {
                                //even though we've found the node if forceReload is specified
                                //we want to go update this single node from the server
                                return self.reloadNode(child);
                            }
                        } else {
                            //now we need to recurse with the updated node/currPathIndex
                            currPathIndex++;
                            node = child;
                            //recurse
                            return doSync();
                        }
                    } else {
                        //couldn't find it in the
                        return self.loadNodeChildren({
                            node: node,
                            section: node.section
                        }).then(function (children) {
                            //send back some progress to allow the caller to deal with expanded nodes
                            deferred.notify({
                                type: 'treeNodeExpanded',
                                node: node,
                                children: children
                            });
                            //ok, got the children, let's find it
                            var found = self.getChildNode(node, args.path[currPathIndex]);
                            if (found) {
                                if (args.path.length === currPathIndex + 1) {
                                    //woot! synced the node
                                    return $q.when(found);
                                } else {
                                    //now we need to recurse with the updated node/currPathIndex
                                    currPathIndex++;
                                    node = found;
                                    //recurse
                                    return doSync();
                                }
                            } else {
                                //fail!
                                return $q.reject();
                            }
                        }, function () {
                            //fail!
                            return $q.reject();
                        });
                    }
                };
                //start
                var wrappedPromise = doSync();
                //then wrap it 
                wrappedPromise.then(function (args) {
                    deferred.resolve(args);
                }, function (args) {
                    deferred.reject(args);
                });
                return deferred.promise;
            }
        };
    }
    angular.module('umbraco.services').factory('treeService', treeService);
    'use strict';
    (function () {
        'use strict';
        /**
  * @ngdoc service
  * @name umbraco.services.udiService
  * @description A service for UDIs
  **/
        function udiService() {
            return {
                /**
       * @ngdoc method
       * @name umbraco.services.udiService#create
       * @methodOf umbraco.services.udiService
       * @function
       *
       * @description
       * Generates a Udi string with a new ID
       *
       * @param {string} entityType The entityType as a string.
       * @returns {string} The generated UDI
       */
                create: function create(entityType) {
                    return this.build(entityType, String.CreateGuid());
                },
                build: function build(entityType, guid) {
                    return 'umb://' + entityType + '/' + guid.replace(/-/g, '');
                },
                getKey: function getKey(udi) {
                    if (!Utilities.isString(udi)) {
                        throw 'udi is not a string';
                    }
                    if (!udi.startsWith('umb://')) {
                        throw 'udi does not start with umb://';
                    }
                    var withoutScheme = udi.substr('umb://'.length);
                    var withoutHost = withoutScheme.substr(withoutScheme.indexOf('/') + 1).trim();
                    if (withoutHost.length !== 32) {
                        throw 'udi is not 32 chars';
                    }
                    return ''.concat(withoutHost.substr(0, 8), '-').concat(withoutHost.substr(8, 4), '-').concat(withoutHost.substr(12, 4), '-').concat(withoutHost.substr(16, 4), '-').concat(withoutHost.substr(20));
                }
            };
        }
        angular.module('umbraco.services').factory('udiService', udiService);
    }());
    'use strict';
    (function () {
        'use strict';
        /**
  * @ngdoc service
  * @name umbraco.services.udiParser
  * @description A object used to parse UDIs
  **/
        function udiParser() {
            return {
                /**
       * @ngdoc method
       * @name umbraco.services.udiParser#parse
       * @methodOf umbraco.services.udiParser
       * @function
       *
       * @description
       * Converts the string representation of an entity identifier into the equivalent Udi instance.
       *
       * @param {string} input The string to parse
       * @returns {Object} The parsed UDI or null if input isn't a valid UDI
       */
                parse: function parse(input) {
                    if (!input || typeof input !== 'string' || !input.startsWith('umb://'))
                        return null;
                    var lastIndexOfSlash = input.substring('umb://'.length).lastIndexOf('/');
                    var entityType = lastIndexOfSlash === -1 ? input.substring('umb://'.length) : input.substr('umb://'.length, lastIndexOfSlash);
                    var value = lastIndexOfSlash === -1 ? null : input.substring('umb://'.length + lastIndexOfSlash + 1);
                    return {
                        entityType: entityType,
                        value: value,
                        toString: function toString() {
                            return 'umb://' + entityType + (value === null ? '' : '/' + value);
                        }
                    };
                }
            };
        }
        angular.module('umbraco.services').factory('udiParser', udiParser);
    }());
    'use strict';
    (function () {
        'use strict';
        /**
  * @ngdoc service
  * @name umbraco.services.umbDataFormatter
  * @description A helper object used to format/transform JSON Umbraco data, mostly used for persisting data to the server
  **/
        function umbDataFormatter() {
            /**
     * maps the display properties to a property collection for persisting/POSTing
     * @param {any} tabs
     */
            function getContentProperties(tabs) {
                var properties = [];
                _.each(tabs, function (tab) {
                    _.each(tab.properties, function (prop) {
                        //don't include the custom generic tab properties
                        //don't include a property that is marked readonly
                        if (!prop.alias.startsWith('_umb_') && !prop.readonly) {
                            properties.push({
                                id: prop.id,
                                alias: prop.alias,
                                value: prop.value
                            });
                        }
                    });
                });
                return properties;
            }
            return {
                formatChangePasswordModel: function formatChangePasswordModel(model) {
                    if (!model) {
                        return null;
                    }
                    var trimmed = _.omit(model, [
                        'confirm',
                        'generatedPassword'
                    ]);
                    //ensure that the pass value is null if all child properties are null
                    var allNull = true;
                    var vals = _.values(trimmed);
                    for (var k = 0; k < vals.length; k++) {
                        if (vals[k] !== null && vals[k] !== undefined) {
                            allNull = false;
                        }
                    }
                    if (allNull) {
                        return null;
                    }
                    return trimmed;
                },
                formatContentTypePostData: function formatContentTypePostData(displayModel, action) {
                    //create the save model from the display model
                    var saveModel = _.pick(displayModel, 'compositeContentTypes', 'isContainer', 'allowAsRoot', 'allowedTemplates', 'allowedContentTypes', 'alias', 'description', 'thumbnail', 'name', 'id', 'icon', 'trashed', 'key', 'parentId', 'alias', 'path', 'allowCultureVariant', 'allowSegmentVariant', 'isElement');
                    // TODO: Map these
                    saveModel.allowedTemplates = _.map(displayModel.allowedTemplates, function (t) {
                        return t.alias;
                    });
                    saveModel.defaultTemplate = displayModel.defaultTemplate ? displayModel.defaultTemplate.alias : null;
                    var realGroups = _.reject(displayModel.groups, function (g) {
                        //do not include these tabs
                        return g.tabState === 'init';
                    });
                    saveModel.groups = _.map(realGroups, function (g) {
                        var saveGroup = _.pick(g, 'inherited', 'id', 'sortOrder', 'name');
                        var realProperties = _.reject(g.properties, function (p) {
                            //do not include these properties
                            return p.propertyState === 'init' || p.inherited === true;
                        });
                        var saveProperties = _.map(realProperties, function (p) {
                            var saveProperty = _.pick(p, 'id', 'alias', 'description', 'validation', 'label', 'sortOrder', 'dataTypeId', 'groupId', 'memberCanEdit', 'showOnMemberProfile', 'isSensitiveData', 'allowCultureVariant', 'allowSegmentVariant', 'labelOnTop');
                            return saveProperty;
                        });
                        saveGroup.properties = saveProperties;
                        //if this is an inherited group and there are not non-inherited properties on it, then don't send up the data
                        if (saveGroup.inherited === true && saveProperties.length === 0) {
                            return null;
                        }
                        return saveGroup;
                    });
                    //we don't want any null groups
                    saveModel.groups = _.reject(saveModel.groups, function (g) {
                        return !g;
                    });
                    return saveModel;
                },
                /** formats the display model used to display the data type to the model used to save the data type */
                formatDataTypePostData: function formatDataTypePostData(displayModel, preValues, action) {
                    var saveModel = {
                        parentId: displayModel.parentId,
                        id: displayModel.id,
                        name: displayModel.name,
                        selectedEditor: displayModel.selectedEditor,
                        //set the action on the save model
                        action: action,
                        preValues: []
                    };
                    for (var i = 0; i < preValues.length; i++) {
                        saveModel.preValues.push({
                            key: preValues[i].alias,
                            value: preValues[i].value
                        });
                    }
                    return saveModel;
                },
                /** formats the display model used to display the dictionary to the model used to save the dictionary */
                formatDictionaryPostData: function formatDictionaryPostData(dictionary, nameIsDirty) {
                    var saveModel = {
                        parentId: dictionary.parentId,
                        id: dictionary.id,
                        name: dictionary.name,
                        nameIsDirty: nameIsDirty,
                        translations: [],
                        key: dictionary.key
                    };
                    for (var i = 0; i < dictionary.translations.length; i++) {
                        saveModel.translations.push({
                            isoCode: dictionary.translations[i].isoCode,
                            languageId: dictionary.translations[i].languageId,
                            translation: dictionary.translations[i].translation
                        });
                    }
                    return saveModel;
                },
                /** formats the display model used to display the user to the model used to save the user */
                formatUserPostData: function formatUserPostData(displayModel) {
                    //create the save model from the display model
                    var saveModel = _.pick(displayModel, 'id', 'parentId', 'name', 'username', 'culture', 'email', 'startContentIds', 'startMediaIds', 'userGroups', 'message');
                    //make sure the userGroups are just a string array
                    var currGroups = saveModel.userGroups;
                    var formattedGroups = [];
                    for (var i = 0; i < currGroups.length; i++) {
                        if (!Utilities.isString(currGroups[i])) {
                            formattedGroups.push(currGroups[i].alias);
                        } else {
                            formattedGroups.push(currGroups[i]);
                        }
                    }
                    saveModel.userGroups = formattedGroups;
                    //make sure the startnodes are just a string array
                    var props = [
                        'startContentIds',
                        'startMediaIds'
                    ];
                    for (var m = 0; m < props.length; m++) {
                        var startIds = saveModel[props[m]];
                        if (!startIds) {
                            continue;
                        }
                        var formattedIds = [];
                        for (var j = 0; j < startIds.length; j++) {
                            formattedIds.push(Number(startIds[j].id));
                        }
                        saveModel[props[m]] = formattedIds;
                    }
                    return saveModel;
                },
                /** formats the display model used to display the user group to the model used to save the user group*/
                formatUserGroupPostData: function formatUserGroupPostData(displayModel, action) {
                    //create the save model from the display model
                    var saveModel = _.pick(displayModel, 'id', 'alias', 'name', 'icon', 'sections', 'users', 'defaultPermissions', 'assignedPermissions');
                    // the start nodes cannot be picked as the property name needs to change - assign manually
                    saveModel.startContentId = displayModel['contentStartNode'];
                    saveModel.startMediaId = displayModel['mediaStartNode'];
                    //set the action on the save model
                    saveModel.action = action;
                    if (!saveModel.id) {
                        saveModel.id = 0;
                    }
                    //the permissions need to just be the array of permission letters, currently it will be a dictionary of an array
                    var currDefaultPermissions = saveModel.defaultPermissions;
                    var saveDefaultPermissions = [];
                    _.each(currDefaultPermissions, function (value, key, list) {
                        _.each(value, function (element, index, list) {
                            if (element.checked) {
                                saveDefaultPermissions.push(element.permissionCode);
                            }
                        });
                    });
                    saveModel.defaultPermissions = saveDefaultPermissions;
                    //now format that assigned/content permissions
                    var currAssignedPermissions = saveModel.assignedPermissions;
                    var saveAssignedPermissions = {};
                    _.each(currAssignedPermissions, function (nodePermissions, index) {
                        saveAssignedPermissions[nodePermissions.id] = [];
                        _.each(nodePermissions.allowedPermissions, function (permission, index) {
                            if (permission.checked) {
                                saveAssignedPermissions[nodePermissions.id].push(permission.permissionCode);
                            }
                        });
                    });
                    saveModel.assignedPermissions = saveAssignedPermissions;
                    //make sure the sections are just a string array
                    var currSections = saveModel.sections;
                    var formattedSections = [];
                    for (var i = 0; i < currSections.length; i++) {
                        if (!Utilities.isString(currSections[i])) {
                            formattedSections.push(currSections[i].alias);
                        } else {
                            formattedSections.push(currSections[i]);
                        }
                    }
                    saveModel.sections = formattedSections;
                    //make sure the user are just an int array
                    var currUsers = saveModel.users;
                    var formattedUsers = [];
                    for (var j = 0; j < currUsers.length; j++) {
                        if (!Utilities.isNumber(currUsers[j])) {
                            formattedUsers.push(currUsers[j].id);
                        } else {
                            formattedUsers.push(currUsers[j]);
                        }
                    }
                    saveModel.users = formattedUsers;
                    //make sure the startnodes are just an int if one is set
                    var props = [
                        'startContentId',
                        'startMediaId'
                    ];
                    for (var m = 0; m < props.length; m++) {
                        var startId = saveModel[props[m]];
                        if (!startId) {
                            continue;
                        }
                        saveModel[props[m]] = startId.id;
                    }
                    saveModel.parentId = -1;
                    return saveModel;
                },
                /** formats the display model used to display the member to the model used to save the member */
                formatMemberPostData: function formatMemberPostData(displayModel, action) {
                    //this is basically the same as for media but we need to explicitly add the username,email, password to the save model
                    var saveModel = this.formatMediaPostData(displayModel, action);
                    saveModel.key = displayModel.key;
                    var genericTab = _.find(displayModel.tabs, function (item) {
                        return item.id === 0;
                    });
                    //map the member login, email, password and groups
                    var propLogin = _.find(genericTab.properties, function (item) {
                        return item.alias === '_umb_login';
                    });
                    var propEmail = _.find(genericTab.properties, function (item) {
                        return item.alias === '_umb_email';
                    });
                    var propPass = _.find(genericTab.properties, function (item) {
                        return item.alias === '_umb_password';
                    });
                    var propGroups = _.find(genericTab.properties, function (item) {
                        return item.alias === '_umb_membergroup';
                    });
                    saveModel.email = propEmail.value.trim();
                    saveModel.username = propLogin.value.trim();
                    saveModel.password = this.formatChangePasswordModel(propPass.value);
                    var selectedGroups = [];
                    for (var n in propGroups.value) {
                        if (propGroups.value[n] === true) {
                            selectedGroups.push(n);
                        }
                    }
                    saveModel.memberGroups = selectedGroups;
                    //turn the dictionary into an array of pairs
                    var memberProviderPropAliases = _.pairs(displayModel.fieldConfig);
                    _.each(displayModel.tabs, function (tab) {
                        _.each(tab.properties, function (prop) {
                            var foundAlias = _.find(memberProviderPropAliases, function (item) {
                                return prop.alias === item[1];
                            });
                            if (foundAlias) {
                                //we know the current property matches an alias, now we need to determine which membership provider property it was for
                                // by looking at the key
                                switch (foundAlias[0]) {
                                case 'umbracoMemberLockedOut':
                                    saveModel.isLockedOut = Object.toBoolean(prop.value);
                                    break;
                                case 'umbracoMemberApproved':
                                    saveModel.isApproved = Object.toBoolean(prop.value);
                                    break;
                                case 'umbracoMemberComments':
                                    saveModel.comments = prop.value;
                                    break;
                                }
                            }
                        });
                    });
                    return saveModel;
                },
                /** formats the display model used to display the media to the model used to save the media */
                formatMediaPostData: function formatMediaPostData(displayModel, action) {
                    //NOTE: the display model inherits from the save model so we can in theory just post up the display model but
                    // we don't want to post all of the data as it is unecessary.
                    var saveModel = {
                        id: displayModel.id,
                        properties: getContentProperties(displayModel.tabs),
                        name: displayModel.name,
                        contentTypeAlias: displayModel.contentTypeAlias,
                        parentId: displayModel.parentId,
                        //set the action on the save model
                        action: action
                    };
                    return saveModel;
                },
                /** formats the display model used to display the content to the model used to save the content  */
                formatContentPostData: function formatContentPostData(displayModel, action) {
                    //NOTE: the display model inherits from the save model so we can in theory just post up the display model but
                    // we don't want to post all of the data as it is unecessary.
                    var saveModel = {
                        id: displayModel.id,
                        name: displayModel.name,
                        contentTypeAlias: displayModel.contentTypeAlias,
                        parentId: displayModel.parentId,
                        //set the action on the save model
                        action: action,
                        variants: _.map(displayModel.variants, function (v) {
                            return {
                                name: v.name || '',
                                //if its null/empty,we must pass up an empty string else we get json converter errors
                                properties: getContentProperties(v.tabs),
                                culture: v.language ? v.language.culture : null,
                                segment: v.segment,
                                publish: v.publish,
                                save: v.save,
                                releaseDate: v.releaseDate,
                                expireDate: v.expireDate
                            };
                        })
                    };
                    var propExpireDate = displayModel.removeDate;
                    var propReleaseDate = displayModel.releaseDate;
                    var propTemplate = displayModel.template;
                    saveModel.expireDate = propExpireDate ? propExpireDate : null;
                    saveModel.releaseDate = propReleaseDate ? propReleaseDate : null;
                    saveModel.templateAlias = propTemplate ? propTemplate : null;
                    return saveModel;
                },
                /**
       * This formats the server GET response for a content display item
       * @param {} displayModel
       * @returns {}
       */
                formatContentGetData: function formatContentGetData(displayModel) {
                    // We need to check for invariant properties among the variant variants,
                    // as the value of an invariant property is shared between different variants.
                    // A property can be culture invariant, segment invariant, or both.
                    // When we detect this, we want to make sure that the property object instance is the
                    // same reference object between all variants instead of a copy (which it will be when
                    // return from the JSON structure).
                    if (displayModel.variants && displayModel.variants.length > 1) {
                        // Collect all invariant properties from the variants that are either the
                        // default language variant or the default segment variant.
                        var defaultVariants = _.filter(displayModel.variants, function (variant) {
                            var isDefaultLanguage = variant.language && variant.language.isDefault;
                            var isDefaultSegment = variant.segment == null;
                            return isDefaultLanguage || isDefaultSegment;
                        });
                        if (defaultVariants.length > 0) {
                            _.each(defaultVariants, function (defaultVariant) {
                                var invariantProps = [];
                                _.each(defaultVariant.tabs, function (tab, tabIndex) {
                                    _.each(tab.properties, function (property, propIndex) {
                                        // culture == null -> property is culture invariant
                                        // segment == null -> property is *possibly* segment invariant
                                        if (!property.culture || !property.segment) {
                                            invariantProps.push({
                                                tabIndex: tabIndex,
                                                propIndex: propIndex,
                                                property: property
                                            });
                                        }
                                    });
                                });
                                var otherVariants = _.filter(displayModel.variants, function (variant) {
                                    return variant !== defaultVariant;
                                });
                                // now assign this same invariant property instance to the same index of the other variants property array
                                _.each(otherVariants, function (variant) {
                                    _.each(invariantProps, function (invProp) {
                                        var tab = variant.tabs[invProp.tabIndex];
                                        var prop = tab.properties[invProp.propIndex];
                                        var inheritsCulture = prop.culture === invProp.property.culture && prop.segment == null && invProp.property.segment == null;
                                        var inheritsSegment = prop.segment === invProp.property.segment && !prop.culture;
                                        if (inheritsCulture || inheritsSegment) {
                                            tab.properties[invProp.propIndex] = invProp.property;
                                        }
                                    });
                                });
                            });
                        }
                    }
                    return displayModel;
                },
                /**
       * Formats the display model used to display the relation type to a model used to save the relation type.
       * @param {Object} relationType
       */
                formatRelationTypePostData: function formatRelationTypePostData(relationType) {
                    var saveModel = {
                        id: relationType.id,
                        name: relationType.name,
                        alias: relationType.alias,
                        key: relationType.key,
                        isBidirectional: relationType.isBidirectional,
                        parentObjectType: relationType.parentObjectType,
                        childObjectType: relationType.childObjectType
                    };
                    return saveModel;
                }
            };
        }
        angular.module('umbraco.services').factory('umbDataFormatter', umbDataFormatter);
    }());
    'use strict';
    function _typeof(obj) {
        if (typeof Symbol === 'function' && typeof Symbol.iterator === 'symbol') {
            _typeof = function _typeof(obj) {
                return typeof obj;
            };
        } else {
            _typeof = function _typeof(obj) {
                return obj && typeof Symbol === 'function' && obj.constructor === Symbol && obj !== Symbol.prototype ? 'symbol' : typeof obj;
            };
        }
        return _typeof(obj);
    }
    /**
* @ngdoc service
* @name umbraco.services.umbRequestHelper
* @description A helper object used for sending requests to the server
**/
    function umbRequestHelper($http, $q, notificationsService, eventsService, formHelper, overlayService) {
        return {
            /**
     * @ngdoc method
     * @name umbraco.services.umbRequestHelper#convertVirtualToAbsolutePath
     * @methodOf umbraco.services.umbRequestHelper
     * @function
     *
     * @description
     * This will convert a virtual path (i.e. ~/App_Plugins/Blah/Test.html ) to an absolute path
     *
     * @param {string} a virtual path, if this is already an absolute path it will just be returned, if this is a relative path an exception will be thrown
     */
            convertVirtualToAbsolutePath: function convertVirtualToAbsolutePath(virtualPath) {
                if (virtualPath.startsWith('/')) {
                    return virtualPath;
                }
                if (!virtualPath.startsWith('~/')) {
                    throw 'The path ' + virtualPath + ' is not a virtual path';
                }
                if (!Umbraco.Sys.ServerVariables.application.applicationPath) {
                    throw 'No applicationPath defined in Umbraco.ServerVariables.application.applicationPath';
                }
                return Umbraco.Sys.ServerVariables.application.applicationPath + virtualPath.trimStart('~/');
            },
            /**
     * @ngdoc method
     * @name umbraco.services.umbRequestHelper#dictionaryToQueryString
     * @methodOf umbraco.services.umbRequestHelper
     * @function
     *
     * @description
     * This will turn an array of key/value pairs or a standard dictionary into a query string
     *
     * @param {Array} queryStrings An array of key/value pairs
     */
            dictionaryToQueryString: function dictionaryToQueryString(queryStrings) {
                if (Utilities.isArray(queryStrings)) {
                    return _.map(queryStrings, function (item) {
                        var key = null;
                        var val = null;
                        for (var k in item) {
                            key = k;
                            val = item[k];
                            break;
                        }
                        if (key === null || val === null) {
                            throw 'The object in the array was not formatted as a key/value pair';
                        }
                        return encodeURIComponent(key) + '=' + encodeURIComponent(val);
                    }).join('&');
                } else if (Utilities.isObject(queryStrings)) {
                    //this allows for a normal object to be passed in (ie. a dictionary)
                    return decodeURIComponent($.param(queryStrings));
                }
                throw 'The queryString parameter is not an array or object of key value pairs';
            },
            /**
     * @ngdoc method
     * @name umbraco.services.umbRequestHelper#getApiUrl
     * @methodOf umbraco.services.umbRequestHelper
     * @function
     *
     * @description
     * This will return the webapi Url for the requested key based on the servervariables collection
     *
     * @param {string} apiName The webapi name that is found in the servervariables["umbracoUrls"] dictionary
     * @param {string} actionName The webapi action name
     * @param {object} queryStrings Can be either a string or an array containing key/value pairs
     */
            getApiUrl: function getApiUrl(apiName, actionName, queryStrings) {
                if (!Umbraco || !Umbraco.Sys || !Umbraco.Sys.ServerVariables || !Umbraco.Sys.ServerVariables['umbracoUrls']) {
                    throw 'No server variables defined!';
                }
                if (!Umbraco.Sys.ServerVariables['umbracoUrls'][apiName]) {
                    throw 'No url found for api name ' + apiName;
                }
                return Umbraco.Sys.ServerVariables['umbracoUrls'][apiName] + actionName + (!queryStrings ? '' : '?' + (Utilities.isString(queryStrings) ? queryStrings : this.dictionaryToQueryString(queryStrings)));
            },
            /**
     * @ngdoc function
     * @name umbraco.services.umbRequestHelper#resourcePromise
     * @methodOf umbraco.services.umbRequestHelper
     * @function
     *
     * @description
     * This returns a promise with an underlying http call, it is a helper method to reduce
     *  the amount of duplicate code needed to query http resources and automatically handle any
     *  Http errors. See /docs/source/using-promises-resources.md
     *
     * @param {object} opts A mixed object which can either be a string representing the error message to be
     *   returned OR an object containing either:
     *     { success: successCallback, errorMsg: errorMessage }
     *          OR
     *     { success: successCallback, error: errorCallback }
     *   In both of the above, the successCallback must accept these parameters: data, status, headers, config
     *   If using the errorCallback it must accept these parameters: data, status, headers, config
     *   The success callback must return the data which will be resolved by the deferred object.
     *   The error callback must return an object containing: {errorMsg: errorMessage, data: originalData, status: status }
     */
            resourcePromise: function resourcePromise(httpPromise, opts) {
                /** The default success callback used if one is not supplied in the opts */
                function defaultSuccess(data, status, headers, config) {
                    //when it's successful, just return the data
                    return data;
                }
                /** The default error callback used if one is not supplied in the opts */
                function defaultError(data, status, headers, config) {
                    var err = {
                        //NOTE: the default error message here should never be used based on the above docs!
                        errorMsg: Utilities.isString(opts) ? opts : 'An error occurred!',
                        data: data,
                        status: status
                    };
                    // if "opts" is a promise, we set "err.errorMsg" to be that promise
                    if (_typeof(opts) == 'object' && typeof opts.then == 'function') {
                        err.errorMsg = opts;
                    }
                    return err;
                }
                //create the callbacs based on whats been passed in.
                var callbacks = {
                    success: !opts || !opts.success ? defaultSuccess : opts.success,
                    error: !opts || !opts.error ? defaultError : opts.error
                };
                return httpPromise.then(function (response) {
                    //invoke the callback
                    var result = callbacks.success.apply(this, [
                        response.data,
                        response.status,
                        response.headers,
                        response.config
                    ]);
                    formHelper.showNotifications(response.data);
                    //when it's successful, just return the data
                    return $q.resolve(result);
                }, function (response) {
                    if (!response) {
                        return;    //sometimes oddly this happens, nothing we can do
                    }
                    if (!response.status) {
                        //this is a JS/angular error
                        return $q.reject(response);
                    }
                    //invoke the callback
                    var result = callbacks.error.apply(this, [
                        response.data,
                        response.status,
                        response.headers,
                        response.config
                    ]);
                    //when there's a 500 (unhandled) error show a YSOD overlay if debugging is enabled.
                    if (response.status >= 500 && response.status < 600) {
                        //show a ysod dialog
                        if (Umbraco.Sys.ServerVariables['isDebuggingEnabled'] === true) {
                            var error = {
                                errorMsg: 'An error occurred',
                                data: response.data
                            };
                            // TODO: All YSOD handling should be done with an interceptor
                            overlayService.ysod(error);
                        } else {
                            //show a simple error notification
                            notificationsService.error('Server error', 'Contact administrator, see log for full details.<br/><i>' + result.errorMsg + '</i>');
                        }
                    } else {
                        formHelper.showNotifications(result.data);
                    }
                    //return an error object including the error message for UI
                    return $q.reject({
                        errorMsg: result.errorMsg,
                        data: result.data,
                        status: result.status
                    });
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#postSaveContent
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Used for saving content/media/members specifically
     *
     * @param {Object} args arguments object
     * @returns {Promise} http promise object.
     */
            postSaveContent: function postSaveContent(args) {
                if (!args.restApiUrl) {
                    throw 'args.restApiUrl is a required argument';
                }
                if (!args.content) {
                    throw 'args.content is a required argument';
                }
                if (!args.action) {
                    throw 'args.action is a required argument';
                }
                if (!args.files) {
                    throw 'args.files is a required argument';
                }
                if (!args.dataFormatter) {
                    throw 'args.dataFormatter is a required argument';
                }
                if (args.showNotifications === null || args.showNotifications === undefined) {
                    args.showNotifications = true;
                }
                //save the active tab id so we can set it when the data is returned.
                var activeTab = _.find(args.content.tabs, function (item) {
                    return item.active;
                });
                var activeTabIndex = activeTab === undefined ? 0 : _.indexOf(args.content.tabs, activeTab);
                //save the data
                return this.postMultiPartRequest(args.restApiUrl, {
                    key: 'contentItem',
                    value: args.dataFormatter(args.content, args.action)
                }, //data transform callback:
                function (data, formData) {
                    //now add all of the assigned files
                    for (var f in args.files) {
                        //each item has a property alias and the file object, we'll ensure that the alias is suffixed to the key
                        // so we know which property it belongs to on the server side
                        var file = args.files[f];
                        var fileKey = 'file_' + (file.alias || '').replace(/_/g, '\\_') + '_' + (file.culture ? file.culture.replace(/_/g, '\\_') : '') + '_' + (file.segment ? file.segment.replace(/_/g, '\\_') : '');
                        if (Utilities.isArray(file.metaData) && file.metaData.length > 0) {
                            fileKey += '_' + _.map(file.metaData, function (x) {
                                return ('' + x).replace(/_/g, '\\_');
                            }).join('_');
                        }
                        formData.append(fileKey, file.file);
                    }
                }).then(function (response) {
                    //success callback
                    //reset the tabs and set the active one
                    if (response.data.tabs && response.data.tabs.length > 0) {
                        response.data.tabs.forEach(function (item) {
                            return item.active = false;
                        });
                        response.data.tabs[activeTabIndex].active = true;
                    }
                    if (args.showNotifications) {
                        formHelper.showNotifications(response.data);
                    }
                    // TODO: Do we need to pass the result through umbDataFormatter.formatContentGetData? Right now things work so not sure but we should check
                    //the data returned is the up-to-date data so the UI will refresh
                    return $q.resolve(response.data);
                }, function (response) {
                    if (!response.status) {
                        //this is a JS/angular error
                        return $q.reject(response);
                    }
                    //when there's a 500 (unhandled) error show a YSOD overlay if debugging is enabled.
                    if (response.status >= 500 && response.status < 600) {
                        //This is a bit of a hack to check if the error is due to a file being uploaded that is too large,
                        // we have to just check for the existence of a string value but currently that is the best way to
                        // do this since it's very hacky/difficult to catch this on the server
                        if (typeof response.data !== 'undefined' && typeof response.data.indexOf === 'function' && response.data.indexOf('Maximum request length exceeded') >= 0) {
                            notificationsService.error('Server error', 'The uploaded file was too large, check with your site administrator to adjust the maximum size allowed');
                        } else if (Umbraco.Sys.ServerVariables['isDebuggingEnabled'] === true) {
                            //show a ysod dialog
                            var error = {
                                errorMsg: 'An error occurred',
                                data: response.data
                            };
                            // TODO: All YSOD handling should be done with an interceptor
                            overlayService.ysod(error);
                        } else {
                            //show a simple error notification
                            notificationsService.error('Server error', 'Contact administrator, see log for full details.<br/><i>' + response.data.ExceptionMessage + '</i>');
                        }
                    } else if (args.showNotifications) {
                        formHelper.showNotifications(response.data);
                    }
                    //return an error object including the error message for UI
                    return $q.reject({
                        errorMsg: 'An error occurred',
                        data: response.data,
                        status: response.status
                    });
                });
            },
            /** Posts a multi-part mime request to the server */
            postMultiPartRequest: function postMultiPartRequest(url, jsonData, transformCallback) {
                //validate input, jsonData can be an array of key/value pairs or just one key/value pair.
                if (!jsonData) {
                    throw 'jsonData cannot be null';
                }
                if (Utilities.isArray(jsonData)) {
                    jsonData.forEach(function (item) {
                        if (!item.key || !item.value) {
                            throw 'jsonData array item must have both a key and a value property';
                        }
                    });
                } else if (!jsonData.key || !jsonData.value) {
                    throw 'jsonData object must have both a key and a value property';
                }
                return $http({
                    method: 'POST',
                    url: url,
                    //IMPORTANT!!! You might think this should be set to 'multipart/form-data' but this is not true because when we are sending up files
                    // the request needs to include a 'boundary' parameter which identifies the boundary name between parts in this multi-part request
                    // and setting the Content-type manually will not set this boundary parameter. For whatever reason, setting the Content-type to 'undefined'
                    // will force the request to automatically populate the headers properly including the boundary parameter.
                    headers: { 'Content-Type': undefined },
                    transformRequest: function transformRequest(data) {
                        var formData = new FormData();
                        //add the json data
                        if (Utilities.isArray(data)) {
                            data.forEach(function (item) {
                                formData.append(item.key, !Utilities.isString(item.value) ? Utilities.toJson(item.value) : item.value);
                            });
                        } else {
                            formData.append(data.key, !Utilities.isString(data.value) ? Utilities.toJson(data.value) : data.value);
                        }
                        //call the callback
                        if (transformCallback) {
                            transformCallback.apply(this, [
                                data,
                                formData
                            ]);
                        }
                        return formData;
                    },
                    data: jsonData
                }).then(function (response) {
                    return $q.resolve(response);
                }, function (response) {
                    return $q.reject(response);
                });
            },
            /**
     * @ngdoc method
     * @name umbraco.resources.contentResource#downloadFile
     * @methodOf umbraco.resources.contentResource
     *
     * @description
     * Downloads a file to the client using AJAX/XHR
     *
     * @param {string} httpPath the path (url) to the resource being downloaded
     * @returns {Promise} http promise object.
     */
            downloadFile: function downloadFile(httpPath) {
                /**
       * Based on an implementation here: web.student.tuwien.ac.at/~e0427417/jsdownload.html
       * See https://stackoverflow.com/a/24129082/694494
       */
                // Use an arraybuffer
                return $http.get(httpPath, { responseType: 'arraybuffer' }).then(function (response) {
                    var octetStreamMime = 'application/octet-stream';
                    var success = false;
                    // Get the headers
                    var headers = response.headers();
                    // Get the filename from the x-filename header or default to "download.bin"
                    var filename = headers['x-filename'] || 'download.bin';
                    filename = decodeURIComponent(filename);
                    // Determine the content type from the header or default to "application/octet-stream"
                    var contentType = headers['content-type'] || octetStreamMime;
                    try {
                        // Try using msSaveBlob if supported
                        var blob = new Blob([response.data], { type: contentType });
                        if (navigator.msSaveBlob)
                            navigator.msSaveBlob(blob, filename);
                        else {
                            // Try using other saveBlob implementations, if available
                            var saveBlob = navigator.webkitSaveBlob || navigator.mozSaveBlob || navigator.saveBlob;
                            if (saveBlob === undefined)
                                throw 'Not supported';
                            saveBlob(blob, filename);
                        }
                        success = true;
                    } catch (ex) {
                        console.log('saveBlob method failed with the following exception:');
                        console.log(ex);
                    }
                    if (!success) {
                        // Get the blob url creator
                        var urlCreator = window.URL || window.webkitURL || window.mozURL || window.msURL;
                        if (urlCreator) {
                            // Try to use a download link
                            var link = document.createElement('a');
                            if ('download' in link) {
                                // Try to simulate a click
                                try {
                                    // Prepare a blob URL
                                    var blob = new Blob([response.data], { type: contentType });
                                    var url = urlCreator.createObjectURL(blob);
                                    link.setAttribute('href', url);
                                    // Set the download attribute (Supported in Chrome 14+ / Firefox 20+)
                                    link.setAttribute('download', filename);
                                    // Simulate clicking the download link
                                    var event = document.createEvent('MouseEvents');
                                    event.initMouseEvent('click', true, true, window, 1, 0, 0, 0, 0, false, false, false, false, 0, null);
                                    link.dispatchEvent(event);
                                    success = true;
                                } catch (ex) {
                                    console.log('Download link method with simulated click failed with the following exception:');
                                    console.log(ex);
                                }
                            }
                            if (!success) {
                                // Fallback to window.location method
                                try {
                                    // Prepare a blob URL
                                    // Use application/octet-stream when using window.location to force download
                                    var blob = new Blob([response.data], { type: octetStreamMime });
                                    var url = urlCreator.createObjectURL(blob);
                                    window.location = url;
                                    success = true;
                                } catch (ex) {
                                    console.log('Download link method with window.location failed with the following exception:');
                                    console.log(ex);
                                }
                            }
                        }
                    }
                    if (!success) {
                        // Fallback to window.open method
                        window.open(httpPath, '_blank', '');
                    }
                    return $q.resolve(response);
                }, function (response) {
                    return $q.reject({
                        errorMsg: 'An error occurred downloading the file',
                        data: response.data,
                        status: response.status
                    });
                });
            }
        };
    }
    angular.module('umbraco.services').factory('umbRequestHelper', umbRequestHelper);
    'use strict';
    /**
* @ngdoc service
* @name umbraco.services.urlHelper
* @description A helper used to work with URLs
**/
    (function () {
        'use strict';
        function urlHelper($window) {
            var pl = /\+/g;
            // Regex for replacing addition symbol with a space
            var search = /([^&=]+)=?([^&]*)/g;
            var decode = function decode(s) {
                return decodeURIComponent(s.replace(pl, ' '));
            };
            //Used for browsers that don't support $window.URL
            function polyFillUrl(url) {
                var parser = document.createElement('a');
                // Let the browser do the work
                parser.href = url;
                return {
                    protocol: parser.protocol,
                    host: parser.host,
                    hostname: parser.hostname,
                    port: parser.port,
                    pathname: parser.pathname,
                    search: parser.search,
                    hash: parser.hash
                };
            }
            return {
                /**
       * @ngdoc function
       * @name parseUrl
       * @methodOf umbraco.services.urlHelper
       * @function
       *
       * @description
       * Returns an object representing each part of the url
       * 
       * @param {string} url the url string to parse
       */
                parseUrl: function parseUrl(url) {
                    //create a URL object based on either the native URL method or the polyfill method
                    var urlObj = $window.URL ? new $window.URL(url) : polyFillUrl(url);
                    //append the searchObject
                    urlObj.searchObject = this.getQueryStringParams(urlObj.search);
                    return urlObj;
                },
                /**
       * @ngdoc function
       * @name parseHashIntoUrl
       * @methodOf umbraco.services.urlHelper
       * @function
       *
       * @description
       * If the hash of a URL contains a path + query strings, this will parse the hash into a url object
       * 
       * @param {string} url the url string to parse
       */
                parseHashIntoUrl: function parseHashIntoUrl(url) {
                    var urlObj = this.parseUrl(url);
                    if (!urlObj.hash) {
                        throw new 'No hash found in url: '() + url;
                    }
                    if (!urlObj.hash.startsWith('#/')) {
                        throw new 'The hash in url does not contain a path to parse: '() + url;
                    }
                    //now create a fake full URL with the hash
                    var fakeUrl = 'http://fakeurl.com' + urlObj.hash.trimStart('#');
                    var fakeUrlObj = this.parseUrl(fakeUrl);
                    return fakeUrlObj;
                },
                /**
       * @ngdoc function
       * @name getQueryStringParams
       * @methodOf umbraco.services.urlHelper
       * @function
       *
       * @description
       * Returns a dictionary of query string key/vals
       * 
       * @param {string} location optional URL to parse, the default will use $window.location
       */
                getQueryStringParams: function getQueryStringParams(location) {
                    var match;
                    //use the current location if none specified
                    var query = location ? location.substring(1) : $window.location.search.substring(1);
                    var urlParams = {};
                    while (match = search.exec(query)) {
                        urlParams[decode(match[1])] = decode(match[2]);
                    }
                    return urlParams;
                }
            };
        }
        angular.module('umbraco.services').factory('urlHelper', urlHelper);
    }());
    'use strict';
    angular.module('umbraco.services').factory('userService', function ($rootScope, eventsService, $q, $location, $window, requestRetryQueue, authResource, emailMarketingResource, $timeout, angularHelper) {
        var currentUser = null;
        var lastUserId = null;
        //this tracks the last date/time that the user's remainingAuthSeconds was updated from the server
        // this is used so that we know when to go and get the user's remaining seconds directly.
        var lastServerTimeoutSet = null;
        function openLoginDialog(isTimedOut) {
            //broadcast a global event that the user is no longer logged in
            var args = { isTimedOut: isTimedOut };
            eventsService.emit('app.notAuthenticated', args);
        }
        function retryRequestQueue(success) {
            if (success) {
                requestRetryQueue.retryAll(currentUser.name);
            } else {
                requestRetryQueue.cancelAll();
                $location.path('/');
            }
        }
        /**
  This methods will set the current user when it is resolved and
  will then start the counter to count in-memory how many seconds they have
  remaining on the auth session
  */
        function setCurrentUser(usr) {
            if (!usr.remainingAuthSeconds) {
                throw 'The user object is invalid, the remainingAuthSeconds is required.';
            }
            currentUser = usr;
            lastServerTimeoutSet = new Date();
            //start the timer
            countdownUserTimeout();
        }
        /**
  Method to count down the current user's timeout seconds,
  this will continually count down their current remaining seconds every 5 seconds until
  there are no more seconds remaining.
  */
        function countdownUserTimeout() {
            $timeout(function () {
                if (currentUser) {
                    //countdown by 5 seconds since that is how long our timer is for.
                    currentUser.remainingAuthSeconds -= 5;
                    //if there are more than 30 remaining seconds, recurse!
                    if (currentUser.remainingAuthSeconds > 30) {
                        //we need to check when the last time the timeout was set from the server, if
                        // it has been more than 30 seconds then we'll manually go and retrieve it from the
                        // server - this helps to keep our local countdown in check with the true timeout.
                        if (lastServerTimeoutSet != null) {
                            var now = new Date();
                            var seconds = (now.getTime() - lastServerTimeoutSet.getTime()) / 1000;
                            if (seconds > 30) {
                                //first we'll set the lastServerTimeoutSet to null - this is so we don't get back in to this loop while we
                                // wait for a response from the server otherwise we'll be making double/triple/etc... calls while we wait.
                                lastServerTimeoutSet = null;
                                //now go get it from the server
                                //NOTE: the safeApply because our timeout is set to not run digests (performance reasons)
                                angularHelper.safeApply($rootScope, function () {
                                    authResource.getRemainingTimeoutSeconds().then(function (result) {
                                        setUserTimeoutInternal(result);
                                    });
                                });
                            }
                        }
                        //recurse the countdown!
                        countdownUserTimeout();
                    } else {
                        //we are either timed out or very close to timing out so we need to show the login dialog.
                        if (Umbraco.Sys.ServerVariables.umbracoSettings.keepUserLoggedIn !== true) {
                            //NOTE: the safeApply because our timeout is set to not run digests (performance reasons)
                            angularHelper.safeApply($rootScope, function () {
                                try {
                                    //NOTE: We are calling this again so that the server can create a log that the timeout has expired, we
                                    // don't actually care about this result.
                                    authResource.getRemainingTimeoutSeconds();
                                } finally {
                                    userAuthExpired();
                                }
                            });
                        } else {
                            //we've got less than 30 seconds remaining so let's check the server
                            if (lastServerTimeoutSet != null) {
                                //first we'll set the lastServerTimeoutSet to null - this is so we don't get back in to this loop while we
                                // wait for a response from the server otherwise we'll be making double/triple/etc... calls while we wait.
                                lastServerTimeoutSet = null;
                                //now go get it from the server
                                //NOTE: the safeApply because our timeout is set to not run digests (performance reasons)
                                angularHelper.safeApply($rootScope, function () {
                                    authResource.getRemainingTimeoutSeconds().then(function (result) {
                                        setUserTimeoutInternal(result);
                                    });
                                });
                            }
                            //recurse the countdown!
                            countdownUserTimeout();
                        }
                    }
                }
            }, 5000, //every 5 seconds
            false);    //false = do NOT execute a digest for every iteration
        }
        /** Called to update the current user's timeout */
        function setUserTimeoutInternal(newTimeout) {
            var asNumber = parseFloat(newTimeout);
            if (!isNaN(asNumber) && currentUser && Utilities.isNumber(asNumber)) {
                currentUser.remainingAuthSeconds = newTimeout;
                lastServerTimeoutSet = new Date();
            }
        }
        /** resets all user data, broadcasts the notAuthenticated event and shows the login dialog */
        function userAuthExpired(isLogout) {
            //store the last user id and clear the user
            if (currentUser && currentUser.id !== undefined) {
                lastUserId = currentUser.id;
            }
            if (currentUser) {
                currentUser.remainingAuthSeconds = 0;
            }
            lastServerTimeoutSet = null;
            currentUser = null;
            openLoginDialog(isLogout === undefined ? true : !isLogout);
        }
        // Register a handler for when an item is added to the retry queue
        requestRetryQueue.onItemAddedCallbacks.push(function (retryItem) {
            if (requestRetryQueue.hasMore()) {
                userAuthExpired();
            }
        });
        return {
            /** Internal method to display the login dialog */
            _showLoginDialog: function _showLoginDialog() {
                openLoginDialog();
            },
            /** Internal method to retry all request after sucessfull login */
            _retryRequestQueue: function _retryRequestQueue(success) {
                retryRequestQueue(success);
            },
            /** Returns a promise, sends a request to the server to check if the current cookie is authorized  */
            isAuthenticated: function isAuthenticated() {
                //if we've got a current user then just return true
                if (currentUser) {
                    var deferred = $q.defer();
                    deferred.resolve(true);
                    return deferred.promise;
                }
                return authResource.isAuthenticated();
            },
            /** Returns a promise, sends a request to the server to validate the credentials  */
            authenticate: function authenticate(login, password) {
                return authResource.performLogin(login, password).then(function (data) {
                    // Check if user has a start node set.
                    if (data.startContentIds.length === 0 && data.startMediaIds.length === 0) {
                        var errorMsg = 'User has no start-nodes';
                        var result = {
                            errorMsg: errorMsg,
                            user: data,
                            authenticated: false,
                            lastUserId: lastUserId,
                            loginType: 'credentials'
                        };
                        eventsService.emit('app.notAuthenticated', result);
                        // TODO: How does this make sense? How can you throw from a promise? Does this get caught by the rejection?
                        // If so then return $q.reject should be used.
                        throw result;
                    }
                    return data;
                }, function (err) {
                    return $q.reject(err);
                }).then(this.setAuthenticationSuccessful);
            },
            setAuthenticationSuccessful: function setAuthenticationSuccessful(data) {
                //when it's successful, return the user data
                setCurrentUser(data);
                var result = {
                    user: data,
                    authenticated: true,
                    lastUserId: lastUserId,
                    loginType: 'credentials'
                };
                //broadcast a global event
                eventsService.emit('app.authenticated', result);
                return result;
            },
            /** Logs the user out
     */
            logout: function logout() {
                return authResource.performLogout().then(function (data) {
                    userAuthExpired();
                    if (data && data.signOutRedirectUrl) {
                        $window.location.replace(data.signOutRedirectUrl);
                    } else {
                        //done!
                        return null;
                    }
                });
            },
            /** Refreshes the current user data with the data stored for the user on the server and returns it */
            refreshCurrentUser: function refreshCurrentUser() {
                var deferred = $q.defer();
                authResource.getCurrentUser().then(function (data) {
                    var result = {
                        user: data,
                        authenticated: true,
                        lastUserId: lastUserId,
                        loginType: 'implicit'
                    };
                    setCurrentUser(data);
                    deferred.resolve(currentUser);
                }, function (err) {
                    //it failed, so they are not logged in
                    deferred.reject(err);
                });
                return deferred.promise;
            },
            /** Returns the current user object in a promise  */
            getCurrentUser: function getCurrentUser(args) {
                if (!currentUser) {
                    return authResource.getCurrentUser().then(function (data) {
                        var result = {
                            user: data,
                            authenticated: true,
                            lastUserId: lastUserId,
                            loginType: 'implicit'
                        };
                        if (args && args.broadcastEvent) {
                            //broadcast a global event, will inform listening controllers to load in the user specific data
                            eventsService.emit('app.authenticated', result);
                        }
                        setCurrentUser(data);
                        return $q.when(currentUser);
                    }, function (err) {
                        //it failed, so they are not logged in
                        return $q.reject(err);
                    });
                } else {
                    return $q.when(currentUser);
                }
            },
            /** Called whenever a server request is made that contains a x-umb-user-seconds response header for which we can update the user's remaining timeout seconds */
            setUserTimeout: function setUserTimeout(newTimeout) {
                setUserTimeoutInternal(newTimeout);
            },
            /** Calls out to a Remote Azure Function to deal with email marketing service */
            addUserToEmailMarketing: function addUserToEmailMarketing(user) {
                return emailMarketingResource.postAddUserToEmailMarketing(user);
            }
        };
    });
    'use strict';
    (function () {
        'use strict';
        function usersHelperService(localizationService) {
            var userStates = [
                {
                    'name': 'All',
                    'key': 'All'
                },
                {
                    'value': 0,
                    'name': 'Active',
                    'key': 'Active',
                    'color': 'success'
                },
                {
                    'value': 1,
                    'name': 'Disabled',
                    'key': 'Disabled',
                    'color': 'danger'
                },
                {
                    'value': 2,
                    'name': 'Locked out',
                    'key': 'LockedOut',
                    'color': 'danger'
                },
                {
                    'value': 3,
                    'name': 'Invited',
                    'key': 'Invited',
                    'color': 'warning'
                },
                {
                    'value': 4,
                    'name': 'Inactive',
                    'key': 'Inactive',
                    'color': 'warning'
                }
            ];
            localizationService.localizeMany(userStates.map(function (userState) {
                return 'user_state' + userState.key;
            })).then(function (data) {
                var reg = /^\[[\S\s]*]$/g;
                data.forEach(function (value, index) {
                    if (!reg.test(value)) {
                        // Only translate if key exists
                        userStates[index].name = value;
                    }
                });
            });
            function getUserStateFromValue(value) {
                return userStates.find(function (userState) {
                    return userState.value === value;
                });
            }
            function getUserStateByKey(key) {
                return userStates.find(function (userState) {
                    return userState.key === key;
                });
            }
            function getUserStatesFilter(userStatesObject) {
                var userStatesFilter = [];
                for (var key in userStatesObject) {
                    if (hasOwnProperty.call(userStatesObject, key)) {
                        var userState = getUserStateByKey(key);
                        if (userState) {
                            userState.count = userStatesObject[key];
                            userStatesFilter.push(userState);
                        }
                    }
                }
                return userStatesFilter;
            }
            ////////////
            var service = {
                getUserStateFromValue: getUserStateFromValue,
                getUserStateByKey: getUserStateByKey,
                getUserStatesFilter: getUserStatesFilter
            };
            return service;
        }
        angular.module('umbraco.services').factory('usersHelper', usersHelperService);
    }());
    'use strict';
    /*Contains multiple services for various helper tasks */
    function versionHelper() {
        return {
            //see: https://gist.github.com/TheDistantSea/8021359
            versionCompare: function versionCompare(v1, v2, options) {
                var lexicographical = options && options.lexicographical, zeroExtend = options && options.zeroExtend, v1parts = v1.split('.'), v2parts = v2.split('.');
                function isValidPart(x) {
                    return (lexicographical ? /^\d+[A-Za-z]*$/ : /^\d+$/).test(x);
                }
                if (!v1parts.every(isValidPart) || !v2parts.every(isValidPart)) {
                    return NaN;
                }
                if (zeroExtend) {
                    while (v1parts.length < v2parts.length) {
                        v1parts.push('0');
                    }
                    while (v2parts.length < v1parts.length) {
                        v2parts.push('0');
                    }
                }
                if (!lexicographical) {
                    v1parts = v1parts.map(Number);
                    v2parts = v2parts.map(Number);
                }
                for (var i = 0; i < v1parts.length; ++i) {
                    if (v2parts.length === i) {
                        return 1;
                    }
                    if (v1parts[i] === v2parts[i]) {
                        continue;
                    } else if (v1parts[i] > v2parts[i]) {
                        return 1;
                    } else {
                        return -1;
                    }
                }
                if (v1parts.length !== v2parts.length) {
                    return -1;
                }
                return 0;
            }
        };
    }
    angular.module('umbraco.services').factory('versionHelper', versionHelper);
    function dateHelper() {
        return {
            convertToServerStringTime: function convertToServerStringTime(momentLocal, serverOffsetMinutes, format) {
                //get the formatted offset time in HH:mm (server time offset is in minutes)
                var formattedOffset = (serverOffsetMinutes > 0 ? '+' : '-') + moment().startOf('day').minutes(Math.abs(serverOffsetMinutes)).format('HH:mm');
                var server = moment.utc(momentLocal).utcOffset(formattedOffset);
                return server.format(format ? format : 'YYYY-MM-DD HH:mm:ss');
            },
            convertToLocalMomentTime: function convertToLocalMomentTime(strVal, serverOffsetMinutes) {
                //get the formatted offset time in HH:mm (server time offset is in minutes)
                var formattedOffset = (serverOffsetMinutes > 0 ? '+' : '-') + moment().startOf('day').minutes(Math.abs(serverOffsetMinutes)).format('HH:mm');
                //if the string format already denotes that it's in "Roundtrip UTC" format (i.e. "2018-02-07T00:20:38.173Z")
                //otherwise known as https://en.wikipedia.org/wiki/ISO_8601. This is the default format returned from the server
                //since that is the default formatter for newtonsoft.json. When it is in this format, we need to tell moment
                //to load the date as UTC so it's not changed, otherwise load it normally
                var isoFormat;
                if (strVal.indexOf('T') > -1 && strVal.endsWith('Z')) {
                    isoFormat = moment.utc(strVal).format('YYYY-MM-DDTHH:mm:ss') + formattedOffset;
                } else {
                    isoFormat = moment(strVal).format('YYYY-MM-DDTHH:mm:ss') + formattedOffset;
                }
                //create a moment with the iso format which will include the offset with the correct time
                // then convert it to local time
                return moment.parseZone(isoFormat).local();
            },
            getLocalDate: function getLocalDate(date, culture, format) {
                if (date) {
                    var dateVal;
                    var serverOffset = Umbraco.Sys.ServerVariables.application.serverTimeOffset;
                    var localOffset = new Date().getTimezoneOffset();
                    var serverTimeNeedsOffsetting = -serverOffset !== localOffset;
                    if (serverTimeNeedsOffsetting) {
                        dateVal = this.convertToLocalMomentTime(date, serverOffset);
                    } else {
                        dateVal = moment(date, 'YYYY-MM-DD HH:mm:ss');
                    }
                    return dateVal.locale(culture).format(format);
                }
            }
        };
    }
    angular.module('umbraco.services').factory('dateHelper', dateHelper);
    function packageHelper(assetsService, treeService, eventsService, $templateCache) {
        return {
            /** Called when a package is installed, this resets a bunch of data and ensures the new package assets are loaded in */
            packageInstalled: function packageInstalled() {
                //clears the tree
                treeService.clearCache();
                //clears the template cache
                $templateCache.removeAll();
                //emit event to notify anything else
                eventsService.emit('app.reInitialize');
            }
        };
    }
    angular.module('umbraco.services').factory('packageHelper', packageHelper);
    /**
 * @ngdoc function
 * @name umbraco.services.umbModelMapper
 * @function
 *
 * @description
 * Utility class to map/convert models
 */
    function umbModelMapper() {
        return {
            /**
     * @ngdoc function
     * @name umbraco.services.umbModelMapper#convertToEntityBasic
     * @methodOf umbraco.services.umbModelMapper
     * @function
     *
     * @description
     * Converts the source model to a basic entity model, it will throw an exception if there isn't enough data to create the model.
     * @param {Object} source The source model
     * @param {Number} source.id The node id of the model
     * @param {String} source.name The node name
     * @param {String} source.icon The models icon as a css class (.icon-doc)
     * @param {Number} source.parentId The parentID, if no parent, set to -1
     * @param {path} source.path comma-separated string of ancestor IDs (-1,1234,1782,1234)
     */
            /** This converts the source model to a basic entity model, it will throw an exception if there isn't enough data to create the model */
            convertToEntityBasic: function convertToEntityBasic(source) {
                var required = [
                    'id',
                    'name',
                    'icon',
                    'parentId',
                    'path'
                ];
                required.forEach(function (k) {
                    if (!hasOwnProperty.call(source, k)) {
                        throw 'The source object does not contain the property '.concat(k);
                    }
                });
                var optional = [
                    'metaData',
                    'key',
                    'alias'
                ];
                //now get the basic object
                var result = _.pick(source, required.concat(optional));
                return result;
            }
        };
    }
    angular.module('umbraco.services').factory('umbModelMapper', umbModelMapper);
    /**
 * @ngdoc function
 * @name umbraco.services.umbSessionStorage
 * @function
 *
 * @description
 * Used to get/set things in browser sessionStorage but always prefixes keys with "umb_" and converts json vals so there is no overlap 
 * with any sessionStorage created by a developer.
 */
    function umbSessionStorage($window) {
        //gets the sessionStorage object if available, otherwise just uses a normal object
        // - required for unit tests.
        var storage = $window['sessionStorage'] ? $window['sessionStorage'] : {};
        return {
            get: function get(key) {
                return Utilities.fromJson(storage['umb_' + key]);
            },
            set: function set(key, value) {
                storage['umb_' + key] = Utilities.toJson(value);
            }
        };
    }
    angular.module('umbraco.services').factory('umbSessionStorage', umbSessionStorage);
    /**
 * @ngdoc function
 * @name umbraco.services.updateChecker
 * @function
 *
 * @description
 * used to check for updates and display a notifcation
 */
    function updateChecker($http, umbRequestHelper) {
        return {
            /**
     * @ngdoc function
     * @name umbraco.services.updateChecker#check
     * @methodOf umbraco.services.updateChecker
     * @function
     *
     * @description
     * Called to load in the legacy tree js which is required on startup if a user is logged in or 
     * after login, but cannot be called until they are authenticated which is why it needs to be lazy loaded. 
     */
            check: function check() {
                return umbRequestHelper.resourcePromise($http.get(umbRequestHelper.getApiUrl('updateCheckApiBaseUrl', 'GetCheck')), 'Failed to retrieve update status');
            }
        };
    }
    angular.module('umbraco.services').factory('updateChecker', updateChecker);
    /**
* @ngdoc service
* @name umbraco.services.umbPropertyEditorHelper
* @description A helper object used for property editors
**/
    function umbPropEditorHelper() {
        return {
            /**
     * @ngdoc function
     * @name getImagePropertyValue
     * @methodOf umbraco.services.umbPropertyEditorHelper
     * @function    
     *
     * @description
     * Returns the correct view path for a property editor, it will detect if it is a full virtual path but if not then default to the internal umbraco one
     * 
     * @param {string} input the view path currently stored for the property editor
     */
            getViewPath: function getViewPath(input, isPreValue) {
                var path = String(input);
                if (path.startsWith('/')) {
                    //This is an absolute path, so just leave it
                    return path;
                } else {
                    if (path.indexOf('/') >= 0) {
                        //This is a relative path, so just leave it
                        return path;
                    } else {
                        if (!isPreValue) {
                            //i.e. views/propertyeditors/fileupload/fileupload.html
                            return 'views/propertyeditors/' + path + '/' + path + '.html';
                        } else {
                            //i.e. views/prevalueeditors/requiredfield.html
                            return 'views/prevalueeditors/' + path + '.html';
                        }
                    }
                }
            }
        };
    }
    angular.module('umbraco.services').factory('umbPropEditorHelper', umbPropEditorHelper);
    'use strict';
    (function () {
        'use strict';
        function validationMessageService($q, localizationService) {
            // Gets the message to use for when a mandatory field isn't completed.
            // Will either use the one provided on the property type's validation object
            // or a localised default.
            function getMandatoryMessage(validation) {
                if (!validation) {
                    return $q.when('');
                }
                if (validation.mandatoryMessage) {
                    return $q.when(validation.mandatoryMessage);
                } else {
                    return localizationService.localize('general_required').then(function (value) {
                        return $q.when(value);
                    });
                }
            }
            var service = { getMandatoryMessage: getMandatoryMessage };
            return service;
        }
        angular.module('umbraco.services').factory('validationMessageService', validationMessageService);
    }());
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.windowResizeListener
 * @function
 *
 * @description
 * A single window resize listener... we don't want to have more than one in theory to ensure that
 * there aren't too many events raised. This will debounce the event with 100 ms intervals and force
 * a $rootScope.$apply when changed and notify all listeners
 *
 */
    function windowResizeListener($rootScope) {
        var WinReszier = function () {
            var registered = [];
            var inited = false;
            var resize = _.debounce(function (ev) {
                notify();
            }, 100);
            var notify = function notify() {
                var h = $(window).height();
                var w = $(window).width();
                //execute all registrations inside of a digest
                $rootScope.$apply(function () {
                    for (var i = 0, cnt = registered.length; i < cnt; i++) {
                        registered[i].apply($(window), [{
                                width: w,
                                height: h
                            }]);
                    }
                });
            };
            return {
                register: function register(fn) {
                    registered.push(fn);
                    if (inited === false) {
                        $(window).on('resize', resize);
                        inited = true;
                    }
                },
                unregister: function unregister(fn) {
                    var index = registered.indexOf(fn);
                    if (index > -1) {
                        registered.splice(index, 1);
                    }
                }
            };
        }();
        return {
            /**
     * Register a callback for resizing
     * @param {Function} cb 
     */
            register: function register(cb) {
                WinReszier.register(cb);
            },
            /**
     * Removes a registered callback
     * @param {Function} cb 
     */
            unregister: function unregister(cb) {
                WinReszier.unregister(cb);
            }
        };
    }
    angular.module('umbraco.services').factory('windowResizeListener', windowResizeListener);
    'use strict';
    /**
 * @ngdoc service
 * @name umbraco.services.xmlhelper
 * @function
 *
 * @description
 * Used to convert legacy xml data to json and back again
 */
    function xmlhelper($http) {
        /*
   Copyright 2011 Abdulla Abdurakhmanov
   Original sources are available at https://code.google.com/p/x2js/
     Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
     http://www.apache.org/licenses/LICENSE-2.0
     Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
   */
        function X2JS() {
            var VERSION = '1.0.11';
            var escapeMode = false;
            var DOMNodeTypes = {
                ELEMENT_NODE: 1,
                TEXT_NODE: 3,
                CDATA_SECTION_NODE: 4,
                DOCUMENT_NODE: 9
            };
            function getNodeLocalName(node) {
                var nodeLocalName = node.localName;
                if (nodeLocalName == null) {
                    nodeLocalName = node.baseName;
                }
                // Yeah, this is IE!! 
                if (nodeLocalName === null || nodeLocalName === '') {
                    nodeLocalName = node.nodeName;
                }
                // =="" is IE too
                return nodeLocalName;
            }
            function getNodePrefix(node) {
                return node.prefix;
            }
            function escapeXmlChars(str) {
                if (typeof str === 'string') {
                    return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#x27;').replace(/\//g, '&#x2F;');
                } else {
                    return str;
                }
            }
            function unescapeXmlChars(str) {
                return str.replace(/&amp;/g, '&').replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&quot;/g, '"').replace(/&#x27;/g, '\'').replace(/&#x2F;/g, '/');
            }
            function parseDOMChildren(node) {
                var result, child, childName;
                if (node.nodeType === DOMNodeTypes.DOCUMENT_NODE) {
                    result = {};
                    child = node.firstChild;
                    childName = getNodeLocalName(child);
                    result[childName] = parseDOMChildren(child);
                    return result;
                } else {
                    if (node.nodeType === DOMNodeTypes.ELEMENT_NODE) {
                        result = {};
                        result.__cnt = 0;
                        var nodeChildren = node.childNodes;
                        // Children nodes
                        for (var cidx = 0; cidx < nodeChildren.length; cidx++) {
                            child = nodeChildren.item(cidx);
                            // nodeChildren[cidx];
                            childName = getNodeLocalName(child);
                            result.__cnt++;
                            if (result[childName] === null) {
                                result[childName] = parseDOMChildren(child);
                                result[childName + '_asArray'] = new Array(1);
                                result[childName + '_asArray'][0] = result[childName];
                            } else {
                                if (result[childName] !== null) {
                                    if (!(result[childName] instanceof Array)) {
                                        var tmpObj = result[childName];
                                        result[childName] = [];
                                        result[childName][0] = tmpObj;
                                        result[childName + '_asArray'] = result[childName];
                                    }
                                }
                                var aridx = 0;
                                while (result[childName][aridx] !== null) {
                                    aridx++;
                                }
                                result[childName][aridx] = parseDOMChildren(child);
                            }
                        }
                        // Attributes
                        for (var aidx = 0; aidx < node.attributes.length; aidx++) {
                            var attr = node.attributes.item(aidx);
                            // [aidx];
                            result.__cnt++;
                            result['_' + attr.name] = attr.value;
                        }
                        // Node namespace prefix
                        var nodePrefix = getNodePrefix(node);
                        if (nodePrefix !== null && nodePrefix !== '') {
                            result.__cnt++;
                            result.__prefix = nodePrefix;
                        }
                        if (result.__cnt === 1 && result['#text'] !== null) {
                            result = result['#text'];
                        }
                        if (result['#text'] !== null) {
                            result.__text = result['#text'];
                            if (escapeMode) {
                                result.__text = unescapeXmlChars(result.__text);
                            }
                            delete result['#text'];
                            delete result['#text_asArray'];
                        }
                        if (result['#cdata-section'] != null) {
                            result.__cdata = result['#cdata-section'];
                            delete result['#cdata-section'];
                            delete result['#cdata-section_asArray'];
                        }
                        if (result.__text != null || result.__cdata != null) {
                            result.toString = function () {
                                return (this.__text != null ? this.__text : '') + (this.__cdata != null ? this.__cdata : '');
                            };
                        }
                        return result;
                    } else {
                        if (node.nodeType === DOMNodeTypes.TEXT_NODE || node.nodeType === DOMNodeTypes.CDATA_SECTION_NODE) {
                            return node.nodeValue;
                        }
                    }
                }
            }
            function startTag(jsonObj, element, attrList, closed) {
                var resultStr = '<' + (jsonObj != null && jsonObj.__prefix != null ? jsonObj.__prefix + ':' : '') + element;
                if (attrList != null) {
                    for (var aidx = 0; aidx < attrList.length; aidx++) {
                        var attrName = attrList[aidx];
                        var attrVal = jsonObj[attrName];
                        resultStr += ' ' + attrName.substr(1) + '=\'' + attrVal + '\'';
                    }
                }
                if (!closed) {
                    resultStr += '>';
                } else {
                    resultStr += '/>';
                }
                return resultStr;
            }
            function endTag(jsonObj, elementName) {
                return '</' + (jsonObj.__prefix !== null ? jsonObj.__prefix + ':' : '') + elementName + '>';
            }
            function endsWith(str, suffix) {
                return str.indexOf(suffix, str.length - suffix.length) !== -1;
            }
            function jsonXmlSpecialElem(jsonObj, jsonObjField) {
                if (endsWith(jsonObjField.toString(), '_asArray') || jsonObjField.toString().indexOf('_') === 0 || jsonObj[jsonObjField] instanceof Function) {
                    return true;
                } else {
                    return false;
                }
            }
            function jsonXmlElemCount(jsonObj) {
                var elementsCnt = 0;
                if (jsonObj instanceof Object) {
                    for (var it in jsonObj) {
                        if (jsonXmlSpecialElem(jsonObj, it)) {
                            continue;
                        }
                        elementsCnt++;
                    }
                }
                return elementsCnt;
            }
            function parseJSONAttributes(jsonObj) {
                var attrList = [];
                if (jsonObj instanceof Object) {
                    for (var ait in jsonObj) {
                        if (ait.toString().indexOf('__') === -1 && ait.toString().indexOf('_') === 0) {
                            attrList.push(ait);
                        }
                    }
                }
                return attrList;
            }
            function parseJSONTextAttrs(jsonTxtObj) {
                var result = '';
                if (jsonTxtObj.__cdata != null) {
                    result += '<![CDATA[' + jsonTxtObj.__cdata + ']]>';
                }
                if (jsonTxtObj.__text != null) {
                    if (escapeMode) {
                        result += escapeXmlChars(jsonTxtObj.__text);
                    } else {
                        result += jsonTxtObj.__text;
                    }
                }
                return result;
            }
            function parseJSONTextObject(jsonTxtObj) {
                var result = '';
                if (jsonTxtObj instanceof Object) {
                    result += parseJSONTextAttrs(jsonTxtObj);
                } else {
                    if (jsonTxtObj != null) {
                        if (escapeMode) {
                            result += escapeXmlChars(jsonTxtObj);
                        } else {
                            result += jsonTxtObj;
                        }
                    }
                }
                return result;
            }
            function parseJSONArray(jsonArrRoot, jsonArrObj, attrList) {
                var result = '';
                if (jsonArrRoot.length === 0) {
                    result += startTag(jsonArrRoot, jsonArrObj, attrList, true);
                } else {
                    for (var arIdx = 0; arIdx < jsonArrRoot.length; arIdx++) {
                        result += startTag(jsonArrRoot[arIdx], jsonArrObj, parseJSONAttributes(jsonArrRoot[arIdx]), false);
                        result += parseJSONObject(jsonArrRoot[arIdx]);
                        result += endTag(jsonArrRoot[arIdx], jsonArrObj);
                    }
                }
                return result;
            }
            function parseJSONObject(jsonObj) {
                var result = '';
                var elementsCnt = jsonXmlElemCount(jsonObj);
                if (elementsCnt > 0) {
                    for (var it in jsonObj) {
                        if (jsonXmlSpecialElem(jsonObj, it)) {
                            continue;
                        }
                        var subObj = jsonObj[it];
                        var attrList = parseJSONAttributes(subObj);
                        if (subObj === null || subObj === undefined) {
                            result += startTag(subObj, it, attrList, true);
                        } else {
                            if (subObj instanceof Object) {
                                if (subObj instanceof Array) {
                                    result += parseJSONArray(subObj, it, attrList);
                                } else {
                                    var subObjElementsCnt = jsonXmlElemCount(subObj);
                                    if (subObjElementsCnt > 0 || subObj.__text !== null || subObj.__cdata !== null) {
                                        result += startTag(subObj, it, attrList, false);
                                        result += parseJSONObject(subObj);
                                        result += endTag(subObj, it);
                                    } else {
                                        result += startTag(subObj, it, attrList, true);
                                    }
                                }
                            } else {
                                result += startTag(subObj, it, attrList, false);
                                result += parseJSONTextObject(subObj);
                                result += endTag(subObj, it);
                            }
                        }
                    }
                }
                result += parseJSONTextObject(jsonObj);
                return result;
            }
            this.parseXmlString = function (xmlDocStr) {
                var xmlDoc;
                if (window.DOMParser) {
                    var parser = new window.DOMParser();
                    xmlDoc = parser.parseFromString(xmlDocStr, 'text/xml');
                } else {
                    // IE :(
                    if (xmlDocStr.indexOf('<?') === 0) {
                        xmlDocStr = xmlDocStr.substr(xmlDocStr.indexOf('?>') + 2);
                    }
                    xmlDoc = new ActiveXObject('Microsoft.XMLDOM');
                    xmlDoc.async = 'false';
                    xmlDoc.loadXML(xmlDocStr);
                }
                return xmlDoc;
            };
            this.xml2json = function (xmlDoc) {
                return parseDOMChildren(xmlDoc);
            };
            this.xml_str2json = function (xmlDocStr) {
                var xmlDoc = this.parseXmlString(xmlDocStr);
                return this.xml2json(xmlDoc);
            };
            this.json2xml_str = function (jsonObj) {
                return parseJSONObject(jsonObj);
            };
            this.json2xml = function (jsonObj) {
                var xmlDocStr = this.json2xml_str(jsonObj);
                return this.parseXmlString(xmlDocStr);
            };
            this.getVersion = function () {
                return VERSION;
            };
            this.escapeMode = function (enabled) {
                escapeMode = enabled;
            };
        }
        var x2js = new X2JS();
        return {
            /** Called to load in the legacy tree js which is required on startup if a user is logged in or 
     after login, but cannot be called until they are authenticated which is why it needs to be lazy loaded. */
            toJson: function toJson(xml) {
                var json = x2js.xml_str2json(xml);
                return json;
            },
            fromJson: function fromJson(json) {
                var xml = x2js.json2xml_str(json);
                return xml;
            }
        };
    }
    angular.module('umbraco.services').factory('xmlhelper', xmlhelper);
    'use strict';
    function _typeof(obj) {
        if (typeof Symbol === 'function' && typeof Symbol.iterator === 'symbol') {
            _typeof = function _typeof(obj) {
                return typeof obj;
            };
        } else {
            _typeof = function _typeof(obj) {
                return obj && typeof Symbol === 'function' && obj.constructor === Symbol && obj !== Symbol.prototype ? 'symbol' : typeof obj;
            };
        }
        return _typeof(obj);
    }
    /**
 * A friendly utility collection to replace AngularJs' ng-functions
 * If it doesn't exist here, it's probably available as vanilla JS
 * 
 * Still carries a dependency on underscore, but if usages of underscore from 
 * elsewhere in the codebase can instead use these methods, the underscore
 * dependency will be nicely abstracted and can be removed/swapped later
 * 
 * This collection is open to extension...
 */
    (function (window) {
        /**
   * Equivalent to angular.noop
   */
        var noop = function noop() {
        };
        /**
   * Facade to angular.copy
   */
        var copy = function copy(src, dst) {
            return angular.copy(src, dst);
        };
        /**
   * Equivalent to angular.isArray
   */
        var isArray = function isArray(val) {
            return Array.isArray(val) || val instanceof Array;
        };
        /**
   * Facade to angular.equals
   */
        var equals = function equals(a, b) {
            return angular.equals(a, b);
        };
        /**
   * Facade to angular.extend
   * Use this with Angular objects, for vanilla JS objects, use Object.assign()
   */
        var extend = function extend(dst, src) {
            return angular.extend(dst, src);
        };
        /**
   * Equivalent to angular.isFunction
   */
        var isFunction = function isFunction(val) {
            return typeof val === 'function';
        };
        /**
   * Equivalent to angular.isUndefined
   */
        var isUndefined = function isUndefined(val) {
            return typeof val === 'undefined';
        };
        /**
   * Equivalent to angular.isDefined. Inverts result of const isUndefined
   */
        var isDefined = function isDefined(val) {
            return !isUndefined(val);
        };
        /**
   * Equivalent to angular.isString
   */
        var isString = function isString(val) {
            return typeof val === 'string';
        };
        /**
   * Equivalent to angular.isNumber
   */
        var isNumber = function isNumber(val) {
            return typeof val === 'number';
        };
        /**
   * Equivalent to angular.isObject
   */
        var isObject = function isObject(val) {
            return val !== null && _typeof(val) === 'object';
        };
        var isWindow = function isWindow(obj) {
            return obj && obj.window === obj;
        };
        var isScope = function isScope(obj) {
            return obj && obj.$evalAsync && obj.$watch;
        };
        var toJsonReplacer = function toJsonReplacer(key, value) {
            var val = value;
            if (typeof key === 'string' && key.charAt(0) === '$' && key.charAt(1) === '$') {
                val = undefined;
            } else if (isWindow(value)) {
                val = '$WINDOW';
            } else if (value && window.document === value) {
                val = '$DOCUMENT';
            } else if (isScope(value)) {
                val = '$SCOPE';
            }
            return val;
        };
        /**
   * Equivalent to angular.toJson
   */
        var toJson = function toJson(obj, pretty) {
            if (isUndefined(obj))
                return undefined;
            if (!isNumber(pretty)) {
                pretty = pretty ? 2 : null;
            }
            return JSON.stringify(obj, toJsonReplacer, pretty);
        };
        /**
   * Equivalent to angular.fromJson
   */
        var fromJson = function fromJson(val) {
            if (!isString(val)) {
                return val;
            }
            return JSON.parse(val);
        };
        /**
   * Not equivalent to angular.forEach. But like the angularJS method this does not fail on null or undefined.
   */
        var forEach = function forEach(obj, iterator) {
            if (obj) {
                return obj.forEach(iterator);
            }
            return obj;
        };
        var _utilities = {
            noop: noop,
            copy: copy,
            isArray: isArray,
            equals: equals,
            extend: extend,
            isFunction: isFunction,
            isUndefined: isUndefined,
            isDefined: isDefined,
            isString: isString,
            isNumber: isNumber,
            isObject: isObject,
            fromJson: fromJson,
            toJson: toJson,
            forEach: forEach
        };
        if (typeof window.Utilities === 'undefined') {
            window.Utilities = _utilities;
        }
    }(window));
}());