/**
 * @ngdoc service
 * @name umbraco.services.serverValidationService
 * @function
 *
 * @description
 * Used to handle server side validation and wires up the UI with the messages. There are 2 types of validation messages, one
 * is for user defined properties (called Properties) and the other is for field properties which are attached to the native 
 * model objects (not user defined). The methods below are named according to these rules: Properties vs Fields.
 */
function serverValidationService() {

    var callbacks = [];
    
    return {
        /**
         * @ngdoc function
         * @name subscribe
         * @methodOf umbraco.services.serverValidationService
         * @function
         *
         * @description
         *  Adds a callback method that is executed whenever validation changes for the field name + property specified.
         *  This is generally used for server side validation in order to match up a server side validation error with 
         *  a particular field, otherwise we can only pinpoint that there is an error for a content property, not the 
         *  property's specific field. This is used with the val-server directive in which the directive specifies the 
         *  field alias to listen for.
         *  If contentProperty is null, then this subscription is for a field property (not a user defined property)
         */
        subscribe: function (contentProperty, fieldName, callback) {
            if (!callback) {
                return;
            }
            
            if (contentProperty === null) {
                //don't add it if it already exists
                var exists1 = _.find(callbacks, function (item) {
                    return item.propertyAlias === null && item.fieldName === fieldName;
                });
                if (!exists1) {
                    callbacks.push({ propertyAlias: contentProperty.alias, fieldName: fieldName, callback: callback });
                }
            }
            else if (contentProperty !== undefined) {
                //don't add it if it already exists
                var exists2 = _.find(callbacks, function (item) {
                    return item.propertyAlias === contentProperty.alias && item.fieldName === fieldName;
                });
                if (!exists2) {
                    callbacks.push({ propertyAlias: contentProperty.alias, fieldName: fieldName, callback: callback });
                }
            }
        },
        
        unsubscribe: function(contentProperty, fieldName) {
            
            if (contentProperty === null) {

                //remove all callbacks for the content field
                callbacks = _.reject(callbacks, function (item) {
                    return item.propertyAlias === null && item.fieldName === fieldName;
                });

            }
            else if (contentProperty !== undefined) {
                
                //remove all callbacks for the content property
                callbacks = _.reject(callbacks, function (item) {
                    return item.propertyAlias === contentProperty.alias &&
                    (item.fieldName === fieldName ||
                        ((item.fieldName === undefined || item.fieldName === "") && (fieldName === undefined || fieldName === "")));
                });
            }

            
        },
        
        
        /**
         * @ngdoc function
         * @name getPropertyCallbacks
         * @methodOf umbraco.services.serverValidationService
         * @function
         *
         * @description
         * Gets all callbacks that has been registered using the subscribe method for the contentProperty + fieldName combo.
         * This will always return any callbacks registered for just the property (i.e. field name is empty) and for ones with an 
         * explicit field name set.
         */
        getPropertyCallbacks: function (contentProperty, fieldName) {            
            var found = _.filter(callbacks, function (item) {
                //returns any callback that have been registered directly against the field and for only the property
                return (item.propertyAlias === contentProperty.alias && (item.fieldName === fieldName || (item.fieldName === undefined || item.fieldName === "")));
            });
            return found;
        },
        
        /**
         * @ngdoc function
         * @name getFieldCallbacks
         * @methodOf umbraco.services.serverValidationService
         * @function
         *
         * @description
         * Gets all callbacks that has been registered using the subscribe method for the field.         
         */
        getFieldCallbacks: function (contentProperty, fieldName) {
            var found = _.filter(callbacks, function (item) {
                //returns any callback that have been registered directly against the field
                return (item.propertyAlias === null && item.fieldName === fieldName);
            });
            return found;
        },
        
        /**
         * @ngdoc function
         * @name addFieldError
         * @methodOf umbraco.services.serverValidationService
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
                    fieldName: fieldName,
                    errorMsg: errorMsg
                });
            }
            
            //find all errors for this item
            var errorsForCallback = _.filter(this.items, function (item) {
                return (item.propertyAlias === null && item.fieldName === fieldName);
            });
            //we should now call all of the call backs registered for this error
            var cbs = this.getFieldCallbacks(fieldName);
            //call each callback for this error
            for (var cb in cbs) {
                cbs[cb].callback.apply(this, [
                    false,                  //pass in a value indicating it is invalid
                    errorsForCallback,      //pass in the errors for this item
                    this.items]);           //pass in all errors in total
            }
        },

        /**
         * @ngdoc function
         * @name addPropertyError
         * @methodOf umbraco.services.serverValidationService
         * @function
         *
         * @description
         * Adds an error message for the content property
         */
        addPropertyError: function (contentProperty, fieldName, errorMsg) {
            if (!contentProperty) {
                return;
            }
            
            //only add the item if it doesn't exist                
            if (!this.hasPropertyError(contentProperty, fieldName)) {
                this.items.push({
                    propertyAlias: contentProperty.alias,
                    fieldName: fieldName,
                    errorMsg: errorMsg
                });
            }
            
            //find all errors for this item
            var errorsForCallback = _.filter(this.items, function (item) {
                return (item.propertyAlias === contentProperty.alias && item.fieldName === fieldName);
            });
            //we should now call all of the call backs registered for this error
            var cbs = this.getPropertyCallbacks(contentProperty, fieldName);
            //call each callback for this error
            for (var cb in cbs) {
                cbs[cb].callback.apply(this, [
                    false,                  //pass in a value indicating it is invalid
                    errorsForCallback,      //pass in the errors for this item
                    this.items]);           //pass in all errors in total
            }
        },        
        
        /**
         * @ngdoc function
         * @name removePropertyError
         * @methodOf umbraco.services.serverValidationService
         * @function
         *
         * @description
         * Removes an error message for the content property
         */
        removePropertyError: function (contentProperty, fieldName) {

            if (!contentProperty) {
                return;
            }
            //remove the item
            this.items = _.reject(this.items, function (item) {
                return (item.propertyAlias === contentProperty.alias && (item.fieldName === fieldName || (fieldName === undefined || fieldName === "")));
            });
        },
        
        /**
         * @ngdoc function
         * @name reset
         * @methodOf umbraco.services.serverValidationService
         * @function
         *
         * @description
         * Clears all errors and notifies all callbacks that all server errros are now valid - used when submitting a form
         */
        reset: function () {
            this.items = [];
            for (var cb in callbacks) {
                callbacks[cb].callback.apply(this, [
                        true,       //pass in a value indicating it is VALID
                        [],         //pass in empty collection
                        []]);       //pass in empty collection
            }
        },
        
        /**
         * @ngdoc function
         * @name getPropertyError
         * @methodOf umbraco.services.serverValidationService
         * @function
         *
         * @description
         * Gets the error message for the content property
         */
        getPropertyError: function (contentProperty, fieldName) {
            var err = _.find(this.items, function (item) {
                //return true if the property alias matches and if an empty field name is specified or the field name matches
                return (item.propertyAlias === contentProperty.alias && (item.fieldName === fieldName || (fieldName === undefined || fieldName === "")));
            });
            //return generic property error message if the error doesn't exist
            return err ? err : "Property has errors";
        },
        
        /**
         * @ngdoc function
         * @name getFieldError
         * @methodOf umbraco.services.serverValidationService
         * @function
         *
         * @description
         * Gets the error message for a content field
         */
        getFieldError: function (fieldName) {
            var err = _.find(this.items, function (item) {
                //return true if the property alias matches and if an empty field name is specified or the field name matches
                return (item.propertyAlias === null && item.fieldName === fieldName);
            });
            //return generic property error message if the error doesn't exist
            return err ? err : "Field has errors";
        },
        
        /**
         * @ngdoc function
         * @name hasPropertyError
         * @methodOf umbraco.services.serverValidationService
         * @function
         *
         * @description
         * Checks if the content property + field name combo has an error
         */
        hasPropertyError: function (contentProperty, fieldName) {
            var err = _.find(this.items, function (item) {
                //return true if the property alias matches and if an empty field name is specified or the field name matches
                return (item.propertyAlias === contentProperty.alias && (item.fieldName === fieldName || (fieldName === undefined || fieldName === "")));
            });
            return err ? true : false;
        },
        
        /**
         * @ngdoc function
         * @name hasFieldError
         * @methodOf umbraco.services.serverValidationService
         * @function
         *
         * @description
         * Checks if a content field has an error
         */
        hasFieldError: function (fieldName) {
            var err = _.find(this.items, function (item) {
                //return true if the property alias matches and if an empty field name is specified or the field name matches
                return (item.propertyAlias === null && item.fieldName === fieldName);
            });
            return err ? true : false;
        },
        
        /** The array of error messages */
        items: []
    };
}

angular.module('umbraco.services').factory('serverValidationService', serverValidationService);