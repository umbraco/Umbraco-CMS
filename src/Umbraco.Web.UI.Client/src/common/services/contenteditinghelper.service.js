
/**
* @ngdoc service
* @name umbraco.services.contentEditingHelper
* @description A helper service for most editors, some methods are specific to content/media/member model types but most are used by
* all editors to share logic and reduce the amount of replicated code among editors.
**/
function contentEditingHelper(fileManager, $q, $location, $routeParams, notificationsService, serverValidationManager, dialogService, formHelper, appState, keyboardService) {

    function isValidIdentifier(id){
        //empty id <= 0
        if(angular.isNumber(id) && id > 0){
            return true;
        }

        //empty guid
        if(id === "00000000-0000-0000-0000-000000000000"){
            return false;
        }

        //empty string / alias
        if(id === ""){
            return false;
        }

        return true;
    }

    return {

        /** Used by the content editor and mini content editor to perform saving operations */
        contentEditorPerformSave: function (args) {
            if (!angular.isObject(args)) {
                throw "args must be an object";
            }
            if (!args.scope) {
                throw "args.scope is not defined";
            }
            if (!args.content) {
                throw "args.content is not defined";
            }
            if (!args.statusMessage) {
                throw "args.statusMessage is not defined";
            }
            if (!args.saveMethod) {
                throw "args.saveMethod is not defined";
            }

            var self = this;

            var deferred = $q.defer();

            if (!args.scope.busy && formHelper.submitForm({ scope: args.scope, statusMessage: args.statusMessage })) {

                args.scope.busy = true;

                args.saveMethod(args.content, $routeParams.create, fileManager.getFiles())
                    .then(function (data) {

                        formHelper.resetForm({ scope: args.scope, notifications: data.notifications });

                        self.handleSuccessfulSave({
                            scope: args.scope,
                            savedContent: data,
                            rebindCallback: self.reBindChangedProperties(args.content, data)
                        });

                        args.scope.busy = false;
                        deferred.resolve(data);

                    }, function (err) {
                        self.handleSaveError({
                            redirectOnFailure: true,
                            err: err,
                            rebindCallback: self.reBindChangedProperties(args.content, err.data)
                        });
                        //show any notifications
                        if (angular.isArray(err.data.notifications)) {
                            for (var i = 0; i < err.data.notifications.length; i++) {
                                notificationsService.showNotification(err.data.notifications[i]);
                            }
                        }
                        args.scope.busy = false;
                        deferred.reject(err);
                    });
            }
            else {
                deferred.reject();
            }

            return deferred.promise;
        },

        /** Returns the action button definitions based on what permissions the user has.
        The content.allowedActions parameter contains a list of chars, each represents a button by permission so
        here we'll build the buttons according to the chars of the user. */
        configureContentEditorButtons: function (args) {

            if (!angular.isObject(args)) {
                throw "args must be an object";
            }
            if (!args.content) {
                throw "args.content is not defined";
            }
            if (!args.methods) {
                throw "args.methods is not defined";
            }
            if (!args.methods.saveAndPublish || !args.methods.sendToPublish || !args.methods.save || !args.methods.unPublish) {
                throw "args.methods does not contain all required defined methods";
            }

            var buttons = {
                defaultButton: null,
                subButtons: []
            };

            function createButtonDefinition(ch) {
                switch (ch) {
                    case "U":
                        //publish action
                        keyboardService.bind("ctrl+p", args.methods.saveAndPublish);

                        return {
                            letter: ch,
                            labelKey: "buttons_saveAndPublish",
                            handler: args.methods.saveAndPublish,
                            hotKey: "ctrl+p"
                        };
                    case "H":
                        //send to publish
                        keyboardService.bind("ctrl+p", args.methods.sendToPublish);

                        return {
                            letter: ch,
                            labelKey: "buttons_saveToPublish",
                            handler: args.methods.sendToPublish,
                            hotKey: "ctrl+p"
                        };
                    case "A":
                        //save
                        keyboardService.bind("ctrl+s", args.methods.save);
                        return {
                            letter: ch,
                            labelKey: "buttons_save",
                            handler: args.methods.save,
                            hotKey: "ctrl+s"
                        };
                    case "Z":
                        //unpublish
                        keyboardService.bind("ctrl+u", args.methods.unPublish);

                        return {
                            letter: ch,
                            labelKey: "content_unPublish",
                            handler: args.methods.unPublish
                        };
                    default:
                        return null;
                }
            }

            //reset
            buttons.subButtons = [];

            //This is the ideal button order but depends on circumstance, we'll use this array to create the button list
            // Publish, SendToPublish, Save
            var buttonOrder = ["U", "H", "A"];

            //Create the first button (primary button)
            //We cannot have the Save or SaveAndPublish buttons if they don't have create permissions when we are creating a new item.
            if (!args.create || _.contains(args.content.allowedActions, "C")) {
                for (var b in buttonOrder) {
                    if (_.contains(args.content.allowedActions, buttonOrder[b])) {
                        buttons.defaultButton = createButtonDefinition(buttonOrder[b]);
                        break;
                    }
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


                //if we are not creating, then we should add unpublish too,
                // so long as it's already published and if the user has access to publish
                if (!args.create) {
                    if (args.content.publishDate && _.contains(args.content.allowedActions, "U")) {
                        buttons.subButtons.push(createButtonDefinition("Z"));
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
         * @name umbraco.services.contentEditingHelper#configureButtons
         * @methodOf umbraco.services.contentEditingHelper
         * @function
         *
         * @description
         * Returns a letter array for buttons, with the primary one first based on content model, permissions and editor state
         */
         getAllowedActions : function(content, creating){

                //This is the ideal button order but depends on circumstance, we'll use this array to create the button list
                // Publish, SendToPublish, Save
                var actionOrder = ["U", "H", "A"];
                var defaultActions;
                var actions = [];

                //Create the first button (primary button)
                //We cannot have the Save or SaveAndPublish buttons if they don't have create permissions when we are creating a new item.
                if (!creating || _.contains(content.allowedActions, "C")) {
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
                        if (content.publishDate && _.contains(content.allowedActions,"U")) {
                            actions.push("Z");
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
          getButtonFromAction : function(ch){
            switch (ch) {
                case "U":
                    return {
                        letter: ch,
                        labelKey: "buttons_saveAndPublish",
                        handler: "saveAndPublish",
                        hotKey: "ctrl+p"
                    };
                case "H":
                    //send to publish
                    return {
                        letter: ch,
                        labelKey: "buttons_saveToPublish",
                        handler: "sendToPublish",
                        hotKey: "ctrl+p"
                    };
                case "A":
                    return {
                        letter: ch,
                        labelKey: "buttons_save",
                        handler: "save",
                        hotKey: "ctrl+s"
                    };
                case "Z":
                    return {
                        letter: ch,
                        labelKey: "content_unPublish",
                        handler: "unPublish"
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
         * re-binds all changed property values to the origContent object from the savedContent object and returns an array of changed properties.
         */
        reBindChangedProperties: function (origContent, savedContent) {

            var changed = [];

            //get a list of properties since they are contained in tabs
            var allOrigProps = this.getAllProps(origContent);
            var allNewProps = this.getAllProps(savedContent);

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

                if (!_.isEqual(origContent[o], savedContent[o])) {
                    origContent[o] = savedContent[o];
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
            if (args.redirectOnFailure === undefined || args.redirectOnFailure === null) {
                throw "args.redirectOnFailure must be set to true or false";
            }

            //When the status is a 400 status with a custom header: X-Status-Reason: Validation failed, we have validation errors.
            //Otherwise the error is probably due to invalid data (i.e. someone mucking around with the ids or something).
            //Or, some strange server error
            if (args.err.status === 400) {
                //now we need to look through all the validation errors
                if (args.err.data && (args.err.data.ModelState)) {

                    //wire up the server validation errs
                    formHelper.handleServerValidation(args.err.data.ModelState);

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
            if (!args.savedContent) {
                throw "args.savedContent cannot be null";
            }

            if (!this.redirectToCreatedContent(args.redirectId ? args.redirectId : args.savedContent.id)) {

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
         * We will only need to maintain create mode if we have not fulfilled the basic requirements for creating an entity which is at least having a name and ID
         */
        redirectToCreatedContent: function (id, modelState) {

            //only continue if we are currently in create mode and if there is no 'Name' modelstate errors
            // since we need at least a name to create content.
            if ($routeParams.create && (isValidIdentifier(id) && (!modelState || !modelState["Name"]))) {

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
