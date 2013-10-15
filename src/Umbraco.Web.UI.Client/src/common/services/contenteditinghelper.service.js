
/**
* @ngdoc service
* @name umbraco.services.contentEditingHelper
* @description A helper service for content/media/member controllers when editing/creating/saving content.
**/
function contentEditingHelper($location, $routeParams, notificationsService, serverValidationManager, dialogService) {

    return {

        /**
         * @ngdoc method
         * @name umbraco.services.contentEditingHelper#getAllProps
         * @methodOf umbraco.services.contentEditingHelper
         * @function
         *
         * @description
         * Returns all propertes contained for the content item (since the normal model has properties contained inside of tabs)
         */
        getAllProps: function (content) {
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
         * @name umbraco.services.contentEditingHelper#reBindChangedProperties
         * @methodOf umbraco.services.contentEditingHelper
         * @function
         *
         * @description
         * re-binds all changed property values to the origContent object from the newContent object and returns an array of changed properties.
         */
        reBindChangedProperties: function (origContent, newContent) {

            var changed = [];

            //get a list of properties since they are contained in tabs
            var allOrigProps = this.getAllProps(origContent);
            var allNewProps = this.getAllProps(newContent);

            function getNewProp(alias) {                
                return _.find(allNewProps, function (item) {
                    return item.alias === alias;
                });
            }

            //a method to ignore built-in prop changes
            var shouldIgnore = function(propName) {
                return _.some(["tabs", "notifications", "ModelState", "tabs", "properties"], function(i) {
                    return i === propName;
                });
            };
            //check for changed built-in properties of the content
            for (var o in origContent) {
                
                //ignore the ones listed in the array
                if (shouldIgnore(o)) {
                    continue;
                }
                
                if (!_.isEqual(origContent[o], newContent[o])) {
                    origContent[o] = newContent[o];
                }
            }

            //check for changed properties of the content
            for (var p in allOrigProps) {
                var newProp = getNewProp(allOrigProps[p].alias);
                if (newProp && !_.isEqual(allOrigProps[p].value, newProp.value)) {

                    //they have changed so set the origContent prop to the new one
                    var origVal = allOrigProps[p].value;
                    allOrigProps[p].value = newProp.value;
                    
                    //instead of having a property editor $watch their expression to check if it has 
                    // been updated, instead we'll check for the existence of a special method on their model
                    // and just call it.
                    if (angular.isFunction(allOrigProps[p].onValueChanged)) {
                        //send the newVal + oldVal
                        allOrigProps[p].onValueChanged(allOrigProps[p].value, origVal);
                    }

                    changed.push(allOrigProps[p]);
                }
            }

            return changed;
        },

        /**
         * @ngdoc method
         * @name umbraco.services.contentEditingHelper#handleValidationErrors
         * @methodOf umbraco.services.contentEditingHelper
         * @function
         *
         * @description
         * A function to handle the validation (modelState) errors collection which will happen on a 400 error indicating validation errors
         *  It's worth noting that when a 400 occurs, the data is still saved just never published, though this depends on if the entity is a new
         *  entity and whether or not the data fulfils the absolute basic requirements like having a mandatory Name.
         */
        handleValidationErrors: function (allProps, modelState) {
            
            //find the content property for the current error, for use in the loop below
            function findContentProp(props, propAlias) {
                return _.find(props, function (item) {
                    return (item.alias === propAlias);
                });
            }

            for (var e in modelState) {
                //the alias in model state can be in dot notation which indicates
                // * the first part is the content property alias
                // * the second part is the field to which the valiation msg is associated with
                //There will always be at least 2 parts since all model errors for properties are prefixed with "Properties"
                var parts = e.split(".");
                if (parts.length > 1) {
                    var propertyAlias = parts[1];

                    //find the content property for the current error
                    var contentProperty = findContentProp(allProps, propertyAlias);

                    if (contentProperty) {
                        //if it contains 2 '.' then we will wire it up to a property's field
                        if (parts.length > 2) {
                            //add an error with a reference to the field for which the validation belongs too
                            serverValidationManager.addPropertyError(contentProperty.alias, parts[2], modelState[e][0]);
                        }
                        else {
                            //add a generic error for the property, no reference to a specific field
                            serverValidationManager.addPropertyError(contentProperty.alias, "", modelState[e][0]);
                        }
                    }
                }
                else {
                    //the parts are only 1, this means its not a property but a native content property
                    serverValidationManager.addFieldError(parts[0], modelState[e][0]);
                }

                //add to notifications
                notificationsService.error("Validation", modelState[e][0]);
            }
        },

        /**
         * @ngdoc function
         * @name umbraco.services.contentEditingHelper#handleSaveError
         * @methodOf umbraco.services.contentEditingHelper
         * @function
         *
         * @description
         * A function to handle what happens when we have validation issues from the server side
         */
        handleSaveError: function (args) {
             
            if (!args.err) {
                throw "args.err cannot be null";
            }
            if (!args.allNewProps && !angular.isArray(args.allNewProps)) {
                throw "args.allNewProps must be a valid array";
            }
            
            //When the status is a 400 status with a custom header: X-Status-Reason: Validation failed, we have validation errors.
            //Otherwise the error is probably due to invalid data (i.e. someone mucking around with the ids or something).
            //Or, some strange server error
            if (args.err.status === 400) {
                //now we need to look through all the validation errors
                if (args.err.data && (args.err.data.ModelState)) {
                    
                    this.handleValidationErrors(args.allNewProps, args.err.data.ModelState);

                    if (!args.redirectOnFailure || !this.redirectToCreatedContent(args.err.data.id, args.err.data.ModelState)) {
                        //we are not redirecting because this is not new content, it is existing content. In this case
                        // we need to detect what properties have changed and re-bind them with the server data. Then we need
                        // to re-bind any server validation errors after the digest takes place.

                        if (args.rebindCallback && angular.isFunction(args.rebindCallback)) {
                            args.rebindCallback();
                        }
                        
                        serverValidationManager.executeAndClearAllSubscriptions();
                    }

                    //indicates we've handled the server result
                    return true;
                }
                else {
                    dialogService.ysodDialog(args.err);
                }
            }
            else {
                dialogService.ysodDialog(args.err);
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
        handleSuccessfulSave: function (args) {

            if (!args) {
                throw "args cannot be null";
            }
            if (!args.newContent) {
                throw "args.newContent cannot be null";
            }

            if (!this.redirectToCreatedContent(args.redirectId ? args.redirectId : args.newContent.id)) {
                
                //we are not redirecting because this is not new content, it is existing content. In this case
                // we need to detect what properties have changed and re-bind them with the server data.
                //call the callback
                if (args.rebindCallback && angular.isFunction(args.rebindCallback)) {
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
         * We will only need to maintain create mode if we have not fulfilled the basic requirements for creating an entity which is at least having a name.
         */
        redirectToCreatedContent: function (id, modelState) {

            //only continue if we are currently in create mode and if there is no 'Name' modelstate errors
            // since we need at least a name to create content.
            if ($routeParams.create && (!modelState || !modelState["Name"])) {

                //need to change the location to not be in 'create' mode. Currently the route will be something like:
                // /belle/#/content/edit/1234?doctype=newsArticle&create=true
                // but we need to remove everything after the query so that it is just:
                // /belle/#/content/edit/9876 (where 9876 is the new id)

                //clear the query strings
                $location.search("");
                
                //change to new path
                $location.path("/" + $routeParams.section + "/" + $routeParams.tree  + "/" + $routeParams.method + "/" + id);
                //don't add a browser history for this
                $location.replace();
                return true;
            }
            return false;
        }
    };
}
angular.module('umbraco.services').factory('contentEditingHelper', contentEditingHelper);