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
    function executeCallback(self, errorsForCallback, callback, culture, segment) {

        callback.apply(self, [
                false,                 // pass in a value indicating it is invalid
                errorsForCallback,     // pass in the errors for this item
                self.items,            // pass in all errors in total
                culture,               // pass the culture that we are listing for.
                segment                // pass the segment that we are listing for.
            ]
        );
    }

    function getFieldErrors(self, fieldName) {
        if (!Utilities.isString(fieldName)) {
            throw "fieldName must be a string";
        }

        //find errors for this field name
        return _.filter(self.items, function (item) {
            return (item.propertyAlias === null && item.culture === "invariant" && item.fieldName === fieldName);
        });
    }
    

    function getPropertyErrors(self, propertyAlias, culture, segment, fieldName) {
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

        //find all errors for this property
        return _.filter(self.items, function (item) {
            return (item.propertyAlias === propertyAlias && item.culture === culture && item.segment === segment && (item.fieldName === fieldName || (fieldName === undefined || fieldName === "")));
        });
    }
    
    function getVariantErrors(self, culture, segment) {
        
        if (!culture) {
            culture = "invariant";
        }
        if (!segment) {
            segment = null;
        }
        
        //find all errors for this property
        return _.filter(self.items, function (item) {
            return (item.culture === culture && item.segment === segment);
        });
    }

    function notifyCallbacks(self) {
        for (var cb in callbacks) {
            if (callbacks[cb].propertyAlias === null && callbacks[cb].fieldName !== null) {
                //its a field error callback
                var fieldErrors = getFieldErrors(self, callbacks[cb].fieldName);
                if (fieldErrors.length > 0) {
                    executeCallback(self, fieldErrors, callbacks[cb].callback, callbacks[cb].culture, callbacks[cb].segment);
                }
            }
            else if (callbacks[cb].propertyAlias != null) {
                //its a property error
                var propErrors = getPropertyErrors(self, callbacks[cb].propertyAlias, callbacks[cb].culture, callbacks[cb].segment, callbacks[cb].fieldName);
                if (propErrors.length > 0) {
                    executeCallback(self, propErrors, callbacks[cb].callback, callbacks[cb].culture, callbacks[cb].segment);
                }
            }
            else {
                //its a variant error
                var variantErrors = getVariantErrors(self, callbacks[cb].culture, callbacks[cb].segment);
                if (variantErrors.length > 0) {
                    executeCallback(self, variantErrors, callbacks[cb].callback, callbacks[cb].culture, callbacks[cb].segment);
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
        subscribe: function (propertyAlias, culture, fieldName, callback, segment) {
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
            
            if (propertyAlias === null) {
                callbacks.push({
                    propertyAlias: null,
                    culture: culture,
                    segment: segment,
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
                    segment: segment,
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
        getPropertyCallbacks: function (propertyAlias, culture, fieldName, segment) {

            //normalize culture to "invariant"
            if (!culture) {
                culture = "invariant";
            }
            //normalize segment to null
            if (!segment) {
                segment = null;
            }

            var found = _.filter(callbacks, function (item) {
                //returns any callback that have been registered directly against the field and for only the property
                return (item.propertyAlias === propertyAlias && item.culture === culture && item.segment === segment && (item.fieldName === fieldName || (item.fieldName === undefined || item.fieldName === "")));
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
                return (item.propertyAlias === null && item.culture === "invariant" && item.segment === null && item.fieldName === fieldName);
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
         * Gets all callbacks that has been registered using the subscribe method for the culture. Not including segments.
         */
        getCultureCallbacks: function (culture) {
            var found = _.filter(callbacks, function (item) {
                //returns any callback that have been registered directly/ONLY against the culture
                return (item.culture === culture && item.segment === null && item.propertyAlias === null && item.fieldName === null);
            });
            return found;
        },

        /**
         * @ngdoc function
         * @name getVariantCallbacks
         * @methodOf umbraco.services.serverValidationManager
         * @function
         *
         * @description
         * Gets all callbacks that has been registered using the subscribe method for the culture and segment.         
         */
        getVariantCallbacks: function (culture, segment) {
            var found = _.filter(callbacks, function (item) {
                //returns any callback that have been registered directly against the given culture and given segment.
                return (item.culture === culture && item.segment === segment && item.propertyAlias === null && item.fieldName === null);
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
                    segment: null,
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
                executeCallback(this, errorsForCallback, cbs[cb].callback, null, null);
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
        addPropertyError: function (propertyAlias, culture, fieldName, errorMsg, segment) {
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

            //only add the item if it doesn't exist                
            if (!this.hasPropertyError(propertyAlias, culture, fieldName, segment)) {
                this.items.push({
                    propertyAlias: propertyAlias,
                    culture: culture,
                    segment: segment,
                    fieldName: fieldName,
                    errorMsg: errorMsg
                });
            }
            
            //find all errors for this item
            var errorsForCallback = getPropertyErrors(this, propertyAlias, culture, segment, fieldName);
            //we should now call all of the call backs registered for this error
            var cbs = this.getPropertyCallbacks(propertyAlias, culture, fieldName, segment);
            //call each callback for this error
            for (var cb in cbs) {
                executeCallback(this, errorsForCallback, cbs[cb].callback, culture, segment);
            }

            //execute variant specific callbacks here too when a propery error is added
            var variantCbs = this.getVariantCallbacks(culture, segment);
            //call each callback for this error
            for (var cb in variantCbs) {
                executeCallback(this, errorsForCallback, variantCbs[cb].callback, culture, segment);
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
        removePropertyError: function (propertyAlias, culture, fieldName, segment) {

            if (!propertyAlias) {
                return;
            }

            //normalize culture to null
            if (!culture) {
                culture = "invariant";
            }
            //normalize segment to null
            if (!segment) {
                segment = null;
            }

            //remove the item
            this.items = _.reject(this.items, function (item) {
                return (item.propertyAlias === propertyAlias && item.culture === culture && item.segment === segment && (item.fieldName === fieldName || (fieldName === undefined || fieldName === "")));
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
                        [],
                        null,
                        null]
                    );
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
        getPropertyError: function (propertyAlias, culture, fieldName, segment) {

            //normalize culture to "invariant"
            if (!culture) {
                culture = "invariant";
            }
            //normalize segment to null
            if (!segment) {
                segment = null;
            }

            var err = _.find(this.items, function (item) {
                //return true if the property alias matches and if an empty field name is specified or the field name matches
                return (item.propertyAlias === propertyAlias && item.culture === culture && item.segment === segment && (item.fieldName === fieldName || (fieldName === undefined || fieldName === "")));
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
                return (item.propertyAlias === null && item.culture === "invariant" && item.segment === null && item.fieldName === fieldName);
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
        hasPropertyError: function (propertyAlias, culture, fieldName, segment) {

            //normalize culture to null
            if (!culture) {
                culture = "invariant";
            }
            //normalize segment to null
            if (!segment) {
                segment = null;
            }

            var err = _.find(this.items, function (item) {
                //return true if the property alias matches and if an empty field name is specified or the field name matches
                return (item.propertyAlias === propertyAlias && item.culture === culture && item.segment === segment && (item.fieldName === fieldName || (fieldName === undefined || fieldName === "")));
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
                return (item.propertyAlias === null && item.culture === "invariant" && item.segment === null && item.fieldName === fieldName);
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

            //normalize culture to "invariant"
            if (!culture) {
                culture = "invariant";
            }

            var err = _.find(this.items, function (item) {
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
            
            var err = _.find(this.items, function (item) {
                return (item.culture === culture && item.segment === segment);
            });
            return err ? true : false;
        },
        /** The array of error messages */
        items: []
    };
}

angular.module('umbraco.services').factory('serverValidationManager', serverValidationManager);
