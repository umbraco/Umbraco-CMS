/**
 * @ngdoc service
 * @name umbraco.services.serverValidationManager
 * @function
 *
 * @description
 * Used to handle server side validation and wires up the UI with the messages. There are 2 types of validation messages, one
 * is for user defined properties (called Properties) and the other is for field properties which are attached to the native 
 * model objects (not user defined). The methods below are named according to these rules: Properties vs Fields.
 */
function serverValidationManager($timeout) {

    var callbacks = [];
    
    /** calls the callback specified with the errors specified, used internally */
    function executeCallback(self, errorsForCallback, callback, culture) {

        callback.apply(self, [
                false,                 // pass in a value indicating it is invalid
                errorsForCallback,     // pass in the errors for this item
                self.items,            // pass in all errors in total
                culture                // pass the culture that we are listing for.
            ]
        );
    }

    function getFieldErrors(self, fieldName) {
        if (!angular.isString(fieldName)) {
            throw "fieldName must be a string";
        }

        //find errors for this field name
        return _.filter(self.items, function (item) {
            return (item.propertyAlias === null && item.culture === "invariant" && item.fieldName === fieldName);
        });
    }
    
    function getPropertyErrors(self, propertyAlias, culture, fieldName) {
        if (!angular.isString(propertyAlias)) {
            throw "propertyAlias must be a string";
        }
        if (fieldName && !angular.isString(fieldName)) {
            throw "fieldName must be a string";
        }
        
        if (!culture) {
            culture = "invariant";
        }

        //find all errors for this property
        return _.filter(self.items, function (item) {
            return (item.propertyAlias === propertyAlias && item.culture === culture && (item.fieldName === fieldName || (fieldName === undefined || fieldName === "")));
        });
    }
    
    function getCultureErrors(self, culture) {
        
        if (!culture) {
            culture = "invariant";
        }
        
        //find all errors for this property
        return _.filter(self.items, function (item) {
            return (item.culture === culture);
        });
    }

    function notifyCallbacks(self) {
        for (var cb in callbacks) {
            if (callbacks[cb].propertyAlias === null && callbacks[cb].fieldName !== null) {
                //its a field error callback
                var fieldErrors = getFieldErrors(self, callbacks[cb].fieldName);
                if (fieldErrors.length > 0) {
                    executeCallback(self, fieldErrors, callbacks[cb].callback, callbacks[cb].culture);
                }
            }
            else if (callbacks[cb].propertyAlias != null) {
                //its a property error
                var propErrors = getPropertyErrors(self, callbacks[cb].propertyAlias, callbacks[cb].culture, callbacks[cb].fieldName);
                if (propErrors.length > 0) {
                    executeCallback(self, propErrors, callbacks[cb].callback, callbacks[cb].culture);
                }
            }
            else {
                //its a culture error
                var cultureErrors = getCultureErrors(self, callbacks[cb].culture);
                if (cultureErrors.length > 0) {
                    executeCallback(self, cultureErrors, callbacks[cb].callback, callbacks[cb].culture);
                }
            }
        }
    }
    
    return {
        
        /**
         * @ngdoc function
         * @name notifyAndClearAllSubscriptions
         * @methodOf umbraco.services.serverValidationManager
         * @function
         *
         * @description
         *  This method needs to be called once all field and property errors are wired up. 
         * 
         *  In some scenarios where the error collection needs to be persisted over a route change 
         *   (i.e. when a content item (or any item) is created and the route redirects to the editor) 
         *   the controller should call this method once the data is bound to the scope
         *   so that any persisted validation errors are re-bound to their controls. Once they are re-binded this then clears the validation
         *   colleciton so that if another route change occurs, the previously persisted validation errors are not re-bound to the new item.
         */
        notifyAndClearAllSubscriptions: function() {

            var self = this;

            $timeout(function () {
                notifyCallbacks(self);
                //now that they are all executed, we're gonna clear all of the errors we have
                self.clear();
            });
        },

        /**
         * @ngdoc function
         * @name notify
         * @methodOf umbraco.services.serverValidationManager
         * @function
         *
         * @description
         * This method isn't used very often but can be used if all subscriptions need to be notified again. This can be
         * handy if a view needs to be reloaded/rebuild like when switching variants in the content editor.
         */
        notify: function() {
            var self = this;

            $timeout(function () {
                notifyCallbacks(self);
            });
        },

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
        subscribe: function (propertyAlias, culture, fieldName, callback) {
            if (!callback) {
                return;
            }

            var id = String.CreateGuid();
            if (!culture) {
                culture = "invariant";
            }
            
            if (propertyAlias === null) {
                callbacks.push({
                    propertyAlias: null,
                    culture: culture,
                    fieldName: fieldName,
                    callback: callback,
                    id: id
                });
            }
            else if (propertyAlias !== undefined) {
                //normalize culture to null
                
                callbacks.push({
                    propertyAlias: propertyAlias,
                    culture: culture, 
                    fieldName: fieldName,
                    callback: callback,
                    id: id
                });
            }

            function unsubscribeId() {
                //remove all callbacks for the content field
                callbacks = _.reject(callbacks, function (item) {
                    return item.id === id;
                });
            }

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
        unsubscribe: function (propertyAlias, culture, fieldName) {
            
            //normalize culture to null
            if (!culture) {
                culture = "invariant";
            }
            
            if (propertyAlias === null) {

                //remove all callbacks for the content field
                callbacks = _.reject(callbacks, function (item) {
                    return item.propertyAlias === null && item.culture === culture && item.fieldName === fieldName;
                });

            }
            else if (propertyAlias !== undefined) {
                //remove all callbacks for the content property
                callbacks = _.reject(callbacks, function (item) {
                    return item.propertyAlias === propertyAlias && item.culture === culture &&
                    (item.fieldName === fieldName ||
                        ((item.fieldName === undefined || item.fieldName === "") && (fieldName === undefined || fieldName === "")));
                });
            }
        },
        
        
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
        getPropertyCallbacks: function (propertyAlias, culture, fieldName) {

            //normalize culture to null
            if (!culture) {
                culture = "invariant";
            }

            var found = _.filter(callbacks, function (item) {
                //returns any callback that have been registered directly against the field and for only the property
                return (item.propertyAlias === propertyAlias && item.culture === culture && (item.fieldName === fieldName || (item.fieldName === undefined || item.fieldName === "")));
            });
            return found;
        },
        
        /**
         * @ngdoc function
         * @name getFieldCallbacks
         * @methodOf umbraco.services.serverValidationManager
         * @function
         *
         * @description
         * Gets all callbacks that has been registered using the subscribe method for the field.         
         */
        getFieldCallbacks: function (fieldName) {
            var found = _.filter(callbacks, function (item) {
                //returns any callback that have been registered directly against the field
                return (item.propertyAlias === null && item.culture === "invariant" && item.fieldName === fieldName);
            });
            return found;
        },
        
        /**
         * @ngdoc function
         * @name getCultureCallbacks
         * @methodOf umbraco.services.serverValidationManager
         * @function
         *
         * @description
         * Gets all callbacks that has been registered using the subscribe method for the culture.         
         */
        getCultureCallbacks: function (culture) {
            var found = _.filter(callbacks, function (item) {
                //returns any callback that have been registered directly/ONLY against the culture
                return (item.culture === culture && item.propertyAlias === null && item.fieldName === null);
            });
            return found;
        },
        
        /**
         * @ngdoc function
         * @name addFieldError
         * @methodOf umbraco.services.serverValidationManager
         * @function
         *
         * @description
         * Adds an error message for a native content item field (not a user defined property, for Example, 'Name')
         */
        addFieldError: function(fieldName, errorMsg) {
            if (!fieldName) {
                return;
            }
            
            //only add the item if it doesn't exist                
            if (!this.hasFieldError(fieldName)) {
                this.items.push({
                    propertyAlias: null,
                    culture: "invariant",
                    fieldName: fieldName,
                    errorMsg: errorMsg
                });
            }
            
            //find all errors for this item
            var errorsForCallback = getFieldErrors(this, fieldName);
            //we should now call all of the call backs registered for this error
            var cbs = this.getFieldCallbacks(fieldName);
            //call each callback for this error
            for (var cb in cbs) {
                executeCallback(this, errorsForCallback, cbs[cb].callback, null);
            }
        },

        /**
         * @ngdoc function
         * @name addPropertyError
         * @methodOf umbraco.services.serverValidationManager
         * @function
         *
         * @description
         * Adds an error message for the content property
         */
        addPropertyError: function (propertyAlias, culture, fieldName, errorMsg) {
            if (!propertyAlias) {
                return;
            }

            //normalize culture to "invariant"
            if (!culture) {
                culture = "invariant";
            }

            //only add the item if it doesn't exist                
            if (!this.hasPropertyError(propertyAlias, culture, fieldName)) {
                this.items.push({
                    propertyAlias: propertyAlias,
                    culture: culture,
                    fieldName: fieldName,
                    errorMsg: errorMsg
                });
            }
            
            //find all errors for this item
            var errorsForCallback = getPropertyErrors(this, propertyAlias, culture, fieldName);
            //we should now call all of the call backs registered for this error
            var cbs = this.getPropertyCallbacks(propertyAlias, culture, fieldName);
            //call each callback for this error
            for (var cb in cbs) {
                executeCallback(this, errorsForCallback, cbs[cb].callback, culture);
            }

            //execute culture specific callbacks here too when a propery error is added
            var cultureCbs = this.getCultureCallbacks(culture);
            //call each callback for this error
            for (var cb in cultureCbs) {
                executeCallback(this, errorsForCallback, cultureCbs[cb].callback, culture);
            }
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
        removePropertyError: function (propertyAlias, culture, fieldName) {

            if (!propertyAlias) {
                return;
            }

            //normalize culture to null
            if (!culture) {
                culture = "invariant";
            }

            //remove the item
            this.items = _.reject(this.items, function (item) {
                return (item.propertyAlias === propertyAlias && item.culture === culture && (item.fieldName === fieldName || (fieldName === undefined || fieldName === "")));
            });
        },
        
        /**
         * @ngdoc function
         * @name reset
         * @methodOf umbraco.services.serverValidationManager
         * @function
         *
         * @description
         * Clears all errors and notifies all callbacks that all server errros are now valid - used when submitting a form
         */
        reset: function () {
            this.clear();
            for (var cb in callbacks) {
                callbacks[cb].callback.apply(this, [
                        true,       //pass in a value indicating it is VALID
                        [],         //pass in empty collection
                        []]);       //pass in empty collection
            }
        },
        
        /**
         * @ngdoc function
         * @name clear
         * @methodOf umbraco.services.serverValidationManager
         * @function
         *
         * @description
         * Clears all errors
         */
        clear: function() {
            this.items = [];
        },
        
        /**
         * @ngdoc function
         * @name getPropertyError
         * @methodOf umbraco.services.serverValidationManager
         * @function
         *
         * @description
         * Gets the error message for the content property
         */
        getPropertyError: function (propertyAlias, culture, fieldName) {

            //normalize culture to null
            if (!culture) {
                culture = "invariant";
            }

            var err = _.find(this.items, function (item) {
                //return true if the property alias matches and if an empty field name is specified or the field name matches
                return (item.propertyAlias === propertyAlias && item.culture === culture && (item.fieldName === fieldName || (fieldName === undefined || fieldName === "")));
            });
            return err;
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
            var err = _.find(this.items, function (item) {
                //return true if the property alias matches and if an empty field name is specified or the field name matches
                return (item.propertyAlias === null && item.culture === "invariant" && item.fieldName === fieldName);
            });
            return err;
        },
        
        /**
         * @ngdoc function
         * @name hasPropertyError
         * @methodOf umbraco.services.serverValidationManager
         * @function
         *
         * @description
         * Checks if the content property + culture + field name combo has an error
         */
        hasPropertyError: function (propertyAlias, culture, fieldName) {

            //normalize culture to null
            if (!culture) {
                culture = "invariant";
            }

            var err = _.find(this.items, function (item) {
                //return true if the property alias matches and if an empty field name is specified or the field name matches
                return (item.propertyAlias === propertyAlias && item.culture === culture && (item.fieldName === fieldName || (fieldName === undefined || fieldName === "")));
            });
            return err ? true : false;
        },
        
        /**
         * @ngdoc function
         * @name hasFieldError
         * @methodOf umbraco.services.serverValidationManager
         * @function
         *
         * @description
         * Checks if a content field has an error
         */
        hasFieldError: function (fieldName) {
            var err = _.find(this.items, function (item) {
                //return true if the property alias matches and if an empty field name is specified or the field name matches
                return (item.propertyAlias === null && item.culture === "invariant" && item.fieldName === fieldName);
            });
            return err ? true : false;
        },
        
        
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
            
            //normalize culture to null
            if (!culture) {
                culture = "invariant";
            }
            
            var err = _.find(this.items, function (item) {
                return item.culture === culture;
            });
            return err ? true : false;
        },
        /** The array of error messages */
        items: []
    };
}

angular.module('umbraco.services').factory('serverValidationManager', serverValidationManager);
