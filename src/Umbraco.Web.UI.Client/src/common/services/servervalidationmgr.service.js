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

    var defaultMatchOptions = {
        matchType: null
    }

    /** calls the callback specified with the errors specified, used internally */
    function executeCallback(errorsForCallback, callback, culture, segment, isValid) {

        callback.apply(instance, [
                isValid,               // pass in a value indicating it is invalid
                errorsForCallback,     // pass in the errors for this item
                errorMsgItems,         // pass in all errors in total
                culture,               // pass the culture that we are listing for.
                segment                // pass the segment that we are listing for.
            ]
        );
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
            throw "fieldName must be a string";
        }

        //find errors for this field name
        return _.filter(errorMsgItems, function (item) {
            return (item.propertyAlias === null && item.culture === "invariant" && item.fieldName === fieldName);
        });
    }
    
    function getPropertyErrors(propertyAlias, culture, segment, fieldName, options) {
        if (!Utilities.isString(propertyAlias)) {
            throw "propertyAlias must be a string";
        }
        if (fieldName && !Utilities.isString(fieldName)) {
            throw "fieldName must be a string";
        }
        
        if (!culture) {
            culture = "invariant";
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

            return matchProp
                && errMsgItem.culture === culture
                && errMsgItem.segment === segment
                // ignore field matching if match options are used
                && (options.matchType || (errMsgItem.fieldName === fieldName || (fieldName === undefined || fieldName === "")));
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
        if (options.matchType === "prefix" && errMsgItem.propertyAlias.startsWith(propertyValidationKey + '/')) {
            return true;
        }
        if (options.matchType === "suffix" && errMsgItem.propertyAlias.endsWith('/' + propertyValidationKey)) {
            return true;
        }
        if (options.matchType === "contains" && errMsgItem.propertyAlias.includes('/' + propertyValidationKey + '/')) {
            return true;
        }
        return false;
    }

    function getVariantErrors(culture, segment) {
        
        if (!culture) {
            culture = "invariant";
        }
        if (!segment) {
            segment = null;
        }
        
        //find all errors for this property
        return _.filter(errorMsgItems, function (item) {
            return (item.culture === culture && item.segment === segment);
        });
    }

    function notifyCallback(cb) {
        if (cb.propertyAlias === null && cb.fieldName !== null) {
            //its a field error callback
            const fieldErrors = getFieldErrors(cb.fieldName);
            const valid = fieldErrors.length === 0;
            executeCallback(fieldErrors, cb.callback, cb.culture, cb.segment, valid);
        }
        else if (cb.propertyAlias != null) {
            //its a property error
            const propErrors = getPropertyErrors(cb.propertyAlias, cb.culture, cb.segment, cb.fieldName, cb.options);
            const valid = propErrors.length === 0;
            executeCallback(propErrors, cb.callback, cb.culture, cb.segment, valid);
        }
        else {
            //its a variant error
            const variantErrors = getVariantErrors(cb.culture, cb.segment);
            const valid = variantErrors.length === 0;
            executeCallback(variantErrors, cb.callback, cb.culture, cb.segment, valid);
        }
    }

    /** Call all registered callbacks indicating if the data they are subscribed to is valid or invalid */
    function notifyCallbacks() {

        // nothing to call
        if (errorMsgItems.length === 0) {
            return;
        }

        callbacks.forEach(cb => notifyCallback(cb));
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
                    validationPath: `${parentPath}/${validation.$id}`,
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
                var nested = _.omit(validation, "$id", "$elementTypeAlias", "ModelState");                
                for (const [key, value] of Object.entries(nested)) {
                    if (Array.isArray(value)) {
                        iterateErrorBlocks(value, `${ms.validationPath}/${key}`); // recurse
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
            culture = "invariant";
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
            return matchProp
                && cb.culture === culture
                && cb.segment === segment
                // ignore field matching if match options are used
                && (cb.options.matchType || (cb.fieldName === fieldName || (cb.fieldName === undefined || cb.fieldName === "")));
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
        if (cb.options.matchType === "prefix" && propertyValidationKey.startsWith(cb.propertyAlias + '/')) {
            return true;
        }
        if (cb.options.matchType === "suffix" && propertyValidationKey.endsWith('/' + cb.propertyAlias)) {
            return true;
        }
        if (cb.options.matchType === "contains" && propertyValidationKey.includes('/' + cb.propertyAlias + '/')) {
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
            return (item.propertyAlias === null && item.culture === "invariant" && item.segment === null && item.fieldName === fieldName);
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
            return (item.culture === culture && item.segment === segment && item.propertyAlias === null && item.fieldName === null);
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
                culture: "invariant",
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
    function addPropertyError(propertyAlias, culture, fieldName, errorMsg, segment) {

        if (!propertyAlias) {
            return;
        }

        //normalize culture to "invariant"
        if (!culture) {
            culture = "invariant";
        }
        //normalize segment to null
        if (!segment) {
            segment = null;
        }
        //normalize errorMsg to empty
        if (!errorMsg) {
            errorMsg = "";
        }

        // remove all non printable chars and whitespace from the string
        // (this can be a json string for complex errors and in some test cases contains odd whitespace)
        if (Utilities.isString(errorMsg)) {
            errorMsg = errorMsg.trimStartSpecial().trim();
        }

        // if the error message is json it's a complex editor validation response that we need to parse
        if ((Utilities.isString(errorMsg) && errorMsg.startsWith("[")) || Utilities.isArray(errorMsg)) {

            // flatten the json structure, create validation paths for each property and add each as a property error
            var idsToErrors = parseComplexEditorError(errorMsg, propertyAlias);
            idsToErrors.forEach(x => addErrorsForModelState(x.modelState, x.validationPath));

            // We need to clear the error message else it will show up as a giant json block against the property
             errorMsg = "";
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
            culture = "invariant";
        }
        //normalize segment to null
        if (!segment) {
            segment = null;
        }

        var err = _.find(errorMsgItems, function (item) {
            //return true if the property alias matches and if an empty field name is specified or the field name matches
            return (item.propertyAlias === propertyAlias && item.culture === culture && item.segment === segment && (item.fieldName === fieldName || (fieldName === undefined || fieldName === "")));
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
            return (item.propertyAlias === null && item.culture === "invariant" && item.segment === null && item.fieldName === fieldName);
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
            throw "modelState is not an object";
        }

        for (const [key, value] of Object.entries(modelState)) {

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

            var parts = key.split(".");

            //Check if this is for content properties - specific to content/media/member editors because those are special 
            // user defined properties with custom controls.
            if (parts.length > 1 && parts[0] === "_Properties") {

                // create the validation key, might just be the prop alias but if it's nested will be validation path
                // like "myBlockEditor/34E3A26C-103D-4A05-AB9D-7E14032309C3/addresses/7170A4DD-2441-4B1B-A8D3-437D75C4CBC9/city"
                var propertyValidationKey = createPropertyValidationKey(parts[1], parentValidationPath);

                var culture = null;
                if (parts.length > 2) {
                    culture = parts[2];
                    //special check in case the string is formatted this way
                    if (culture === "null") {
                        culture = null;
                    }
                }

                var segment = null;
                if (parts.length > 3) {
                    segment = parts[3];
                    //special check in case the string is formatted this way
                    if (segment === "null") {
                        segment = null;
                    }
                }

                var htmlFieldReference = "";
                if (parts.length > 4) {
                    htmlFieldReference = parts[4] || "";
                }

                // add a generic error for the property
                addPropertyError(propertyValidationKey, culture, htmlFieldReference, value && Array.isArray(value) && value.length > 0 ? value[0] : null, segment);
            }
            else {

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
        return parentValidationPath ? (parentValidationPath + "/" + propertyAlias) : propertyAlias;
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
                true,       //pass in a value indicating it is VALID
                [],         //pass in empty collection
                [],
                null,
                null]
            );
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
        notifyAndClearAllSubscriptions: function() {

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
        subscribe: function (propertyAlias, culture, fieldName, callback, segment, options) {
            if (!callback) {
                return;
            }

            var id = String.CreateGuid();

            //normalize culture to "invariant"
            if (!culture) {
                culture = "invariant";
            }
            //normalize segment to null
            if (!segment) {
                segment = null;
            }

            let cb = null;

            if (propertyAlias === null) {
                cb = {
                    propertyAlias: null,
                    culture: culture,
                    segment: segment,
                    fieldName: fieldName,
                    callback: callback,
                    id: id
                };
            }
            else if (propertyAlias !== undefined) {
                                
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
        unsubscribe: function (propertyAlias, culture, fieldName, segment) {
            
            //normalize culture to "invariant"
            if (!culture) {
                culture = "invariant";
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

            }
            else if (propertyAlias !== undefined) {
                //remove all callbacks for the content property
                callbacks = _.reject(callbacks, function (item) {
                    return item.propertyAlias === propertyAlias && item.culture === culture && item.segment === segment &&
                    (item.fieldName === fieldName ||
                        ((item.fieldName === undefined || item.fieldName === "") && (fieldName === undefined || fieldName === "")));
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
        getCultureCallbacks: function (culture) {
            var found = _.filter(callbacks, function (item) {
                //returns any callback that have been registered directly/ONLY against the culture
                return (item.culture === culture && item.segment === null && item.propertyAlias === null && item.fieldName === null);
            });
            return found;
        },

        getVariantCallbacks: getVariantCallbacks,
        addFieldError: addFieldError,        

        addPropertyError: function (propertyAlias, culture, fieldName, errorMsg, segment) {
            addPropertyError(propertyAlias, culture, fieldName, errorMsg, segment);
            notifyCallbacks(); // ensure all callbacks are called
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
        removePropertyError: function (propertyAlias, culture, fieldName, segment, options) {

            var errors = getPropertyErrors(propertyAlias, culture, segment, fieldName, options);
            errorMsgItems = errorMsgItems.filter(v => errors.indexOf(v) === -1);

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
        getPropertyError: function (propertyAlias, culture, fieldName, segment) {
            var errors = getPropertyErrors(propertyAlias, culture, segment, fieldName);
            if (errors.length > 0) { // should only ever contain one
                return errors[0];
            }
            return undefined;
        },

        getPropertyErrorsByValidationPath: function (propertyValidationPath, culture, segment, options) {
            return getPropertyErrors(propertyValidationPath, culture, segment, "", options);
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
        getFieldError: function (fieldName) {
            var err = _.find(errorMsgItems, function (item) {
                //return true if the property alias matches and if an empty field name is specified or the field name matches
                return (item.propertyAlias === null && item.culture === "invariant" && item.segment === null && item.fieldName === fieldName);
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
        hasCultureError: function (culture) {

            //normalize culture to "invariant"
            if (!culture) {
                culture = "invariant";
            }

            var err = _.find(errorMsgItems, function (item) {
                return (item.culture === culture && item.segment === null);
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
        hasVariantError: function (culture, segment) {
            
            //normalize culture to "invariant"
            if (!culture) {
                culture = "invariant";
            }
            //normalize segment to null
            if (!segment) {
                segment = null;
            }
            
            var err = _.find(errorMsgItems, function (item) {
                return (item.culture === culture && item.segment === segment);
            });
            return err ? true : false;
        }

    };

    // Used to return the 'items' array as a reference/getter
    Object.defineProperty(instance, "items", {
        get: function () {
            return errorMsgItems;
        },
        set: function (value) {
            throw "Cannot set the items array";
        }
    });

    return instance;
}

angular.module('umbraco.services').factory('serverValidationManager', serverValidationManager);
