/**
 * @ngdoc factory
 * @name serverValidationService
 * @function
 *
 * @description
 * used to handle server side validation and wires up the UI with the messages
 */
function serverValidationService() {

    var callbacks = [];
    
    return {
        /**
         * @ngdoc function
         * @name subscribe
         * @methodOf serverValidationService
         * @function
         *
         * @description
         *  Adds a callback method that is executed whenever validation changes for the field name + property specified.
         *  This is generally used for server side validation in order to match up a server side validation error with 
         *  a particular field, otherwise we can only pinpoint that there is an error for a content property, not the 
         *  property's specific field. This is used with the val-server directive in which the directive specifies the 
         *  field alias to listen for.
         */
        subscribe: function (contentProperty, fieldName, callback) {
            if (!contentProperty || !callback) {
                return;
            }
            //don't add it if it already exists
            var exists = _.find(callbacks, function(item) {
                return item.propertyAlias === contentProperty.alias && item.fieldName === fieldName;
            });
            if (!exists) {
                callbacks.push({ propertyAlias: contentProperty.alias, fieldName: fieldName, callback: callback });
            }            
        },
        
        /**
         * @ngdoc function
         * @name getCallbacks
         * @methodOf serverValidationService
         * @function
         *
         * @description
         * Gets all callbacks that has been registered using the subscribe method for the contentProperty + fieldName combo.
         * This will always return any callbacks registered for just the property (i.e. field name is empty) and for ones with an 
         * explicit field name set.
         */
        getCallbacks: function (contentProperty, fieldName) {            
            var found = _.filter(callbacks, function (item) {
                //returns any callback that have been registered directly against the field and for only the property
                return (item.propertyAlias === contentProperty.alias && (item.fieldName === fieldName || (item.fieldName === undefined || item.fieldName === "")));
            });
            return found;
        },
        
        /**
         * @ngdoc function
         * @name addError
         * @methodOf serverValidationService
         * @function
         *
         * @description
         * Adds an error message for the content property
         */
        addError: function (contentProperty, fieldName, errorMsg) {
            if (!contentProperty) {
                return;
            }
            
            //only add the item if it doesn't exist                
            if (!this.hasError(contentProperty, fieldName)) {
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
            var cbs = this.getCallbacks(contentProperty, fieldName);
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
         * @name removeError
         * @methodOf serverValidationService
         * @function
         *
         * @description
         * Removes an error message for the content property
         */
        removeError: function (contentProperty, fieldName) {

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
         * @methodOf serverValidationService
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
         * @name getError
         * @methodOf serverValidationService
         * @function
         *
         * @description
         * Gets the error message for the content property
         */
        getError: function (contentProperty, fieldName) {            
            var err = _.find(this.items, function (item) {
                //return true if the property alias matches and if an empty field name is specified or the field name matches
                return (item.propertyAlias === contentProperty.alias && (item.fieldName === fieldName || (fieldName === undefined || fieldName === "")));
            });
            //return generic property error message if the error doesn't exist
            return err ? err : "Property has errors";
        },
        
        /**
         * @ngdoc function
         * @name hasError
         * @methodOf serverValidationService
         * @function
         *
         * @description
         * Checks if the content property + field name combo has an error
         */
        hasError: function (contentProperty, fieldName) {
            var err = _.find(this.items, function (item) {
                //return true if the property alias matches and if an empty field name is specified or the field name matches
                return (item.propertyAlias === contentProperty.alias && (item.fieldName === fieldName || (fieldName === undefined || fieldName === "")));
            });
            return err ? true : false;
        },
        
        /** The array of error messages */
        items: []
    };
}

angular.module('umbraco.services').factory('serverValidationService', serverValidationService);