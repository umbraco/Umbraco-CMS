
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
        if (id === "00000000-0000-0000-0000-000000000000") {
            return false;
        }

        //empty string / alias
        if (id === "") {
            return false;
        }

        return true;
    }

    return {

        //TODO: We need to move some of this to formHelper for saving, too many editors use this method for saving when this entire
        //service should only be used for content/media/members

        /** Used by the content editor and mini content editor to perform saving operations */
        contentEditorPerformSave: function (args) {
            if (!Utilities.isObject(args)) {
                throw "args must be an object";
            }
            if (!args.scope) {
                throw "args.scope is not defined";
            }
            if (!args.content) {
                throw "args.content is not defined";
            }
            if (!args.saveMethod) {
                throw "args.saveMethod is not defined";
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
            var rebindCallback = args.rebindCallback === undefined ? self.reBindChangedProperties : args.rebindCallback;

            if (formHelper.submitForm({ scope: args.scope, action: args.action })) {

                return args.saveMethod(args.content, args.create, fileManager.getFiles(), args.showNotifications)
                    .then(function (data) {

                        formHelper.resetForm({ scope: args.scope });

                        if (!args.infiniteMode) {
                            self.handleSuccessfulSave({
                                scope: args.scope,
                                savedContent: data,
                                softRedirect: args.softRedirect,
                                rebindCallback: function () {
                                    rebindCallback.apply(self, [args.content, data]);
                                }
                            });

                            //update editor state to what is current
                            editorState.set(args.content);
                        }

                        return $q.resolve(data);

                    }, function (err) {
                        self.handleSaveError({
                            showNotifications: args.showNotifications,
                            softRedirect: args.softRedirect,
                            err: err,
                            rebindCallback: function () {
                                rebindCallback.apply(self, [args.content, err.data]);
                            }
                        });

                        //update editor state to what is current
                        editorState.set(args.content);

                        return $q.reject(err);
                    });
            }
            else {
                return $q.reject();
            }

        },

        /** Used by the content editor and media editor to add an info tab to the tabs array (normally known as the properties tab) */
        addInfoTab: function (tabs) {

            var infoTab = {
                "alias": "_umb_infoTab",
                "id": -1,
                "label": "Info",
                "properties": []
            };

            // first check if tab is already added
            var foundInfoTab = false;

            angular.forEach(tabs, function (tab) {
                if (tab.id === infoTab.id && tab.alias === infoTab.alias) {
                    foundInfoTab = true;
                }
            });

            // add info tab if is is not found
            if (!foundInfoTab) {
                localizationService.localize("general_info").then(function (value) {
                    infoTab.label = value;
                    tabs.push(infoTab);
                });
            }

        },

        /** Returns the action button definitions based on what permissions the user has.
        The content.allowedActions parameter contains a list of chars, each represents a button by permission so
        here we'll build the buttons according to the chars of the user. */
        configureContentEditorButtons: function (args) {

            if (!Utilities.isObject(args)) {
                throw "args must be an object";
            }
            if (!args.content) {
                throw "args.content is not defined";
            }
            if (!args.methods) {
                throw "args.methods is not defined";
            }
            if (!args.methods.saveAndPublish || !args.methods.sendToPublish || !args.methods.unpublish || !args.methods.schedulePublish || !args.methods.publishDescendants) {
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
                        return {
                            letter: ch,
                            labelKey: "buttons_saveAndPublish",
                            handler: args.methods.saveAndPublish,
                            hotKey: "ctrl+p",
                            hotKeyWhenHidden: true,
                            alias: "saveAndPublish",
                            addEllipsis: args.content.variants && args.content.variants.length > 1 ? "true" : "false"
                        };
                    case "H":
                        //send to publish
                        return {
                            letter: ch,
                            labelKey: "buttons_saveToPublish",
                            handler: args.methods.sendToPublish,
                            hotKey: "ctrl+p",
                            hotKeyWhenHidden: true,
                            alias: "sendToPublish",
                            addEllipsis: args.content.variants && args.content.variants.length > 1 ? "true" : "false"
                        };
                    case "Z":
                        //unpublish
                        return {
                            letter: ch,
                            labelKey: "content_unpublish",
                            handler: args.methods.unpublish,
                            hotKey: "ctrl+u",
                            hotKeyWhenHidden: true,
                            alias: "unpublish",
                            addEllipsis: "true"
                        };
                    case "SCHEDULE":
                        //schedule publish - schedule doesn't have a permission letter so
                        // the button letter is made unique so it doesn't collide with anything else
                        return {
                            letter: ch,
                            labelKey: "buttons_schedulePublish",
                            handler: args.methods.schedulePublish,
                            hotKey: "alt+shift+s",
                            hotKeyWhenHidden: true,
                            alias: "schedulePublish",
                            addEllipsis: "true"
                        };
                    case "PUBLISH_DESCENDANTS":
                        // Publish descendants - it doesn't have a permission letter so
                        // the button letter is made unique so it doesn't collide with anything else
                        return {
                            letter: ch,
                            labelKey: "buttons_publishDescendants",
                            handler: args.methods.publishDescendants,
                            hotKey: "alt+shift+p",
                            hotKeyWhenHidden: true,
                            alias: "publishDescendant",
                            addEllipsis: "true"
                        };
                    default:
                        return null;
                }
            }

            //reset
            buttons.subButtons = [];

            //This is the ideal button order but depends on circumstance, we'll use this array to create the button list
            // Publish, SendToPublish
            var buttonOrder = ["U", "H", "SCHEDULE", "PUBLISH_DESCENDANTS"];

            //Create the first button (primary button)
            //We cannot have the Save or SaveAndPublish buttons if they don't have create permissions when we are creating a new item.
            //Another tricky rule is if they only have Create + Browse permissions but not Save but if it's being created then they will
            // require the Save button in order to create.
            //So this code is going to create the primary button (either Publish, SendToPublish, Save) if we are not in create mode
            // or if the user has access to create.
            if (!args.create || _.contains(args.content.allowedActions, "C")) {
                for (var b in buttonOrder) {
                    if (_.contains(args.content.allowedActions, buttonOrder[b])) {
                        buttons.defaultButton = createButtonDefinition(buttonOrder[b]);
                        break;
                    }
                }

                //Here's the special check, if the button still isn't set and we are creating and they have create access
                //we need to add the Save button
                if (!buttons.defaultButton && args.create && _.contains(args.content.allowedActions, "C")) {
                    buttons.defaultButton = createButtonDefinition("A");
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
                if (_.contains(args.content.allowedActions, "U")) {
                    buttons.subButtons.push(createButtonDefinition("SCHEDULE"));
                    buttons.subButtons.push(createButtonDefinition("PUBLISH_DESCENDANTS"));
                }

                // if we are not creating, then we should add unpublish too,
                // so long as it's already published and if the user has access to publish
                // and the user has access to unpublish (may have been removed via Event)
                if (!args.create) {
                    var hasPublishedVariant = args.content.variants.filter(function (variant) { return (variant.state === "Published" || variant.state === "PublishedPendingChanges"); }).length > 0;
                    if (hasPublishedVariant && _.contains(args.content.allowedActions, "U") && _.contains(args.content.allowedActions, "Z")) {
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
         * Returns all propertes contained for the tabbed content item
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
         * @name umbraco.services.contentEditingHelper#buildCompositeVariantId
         * @methodOf umbraco.services.contentEditingHelper
         * @function
         *
         * @description
         * Returns a id for the variant that is unique between all variants on the content
         * Note "invariant" is used for the invariant culture,
         * "null" is used for the NULL segment
         */
        buildCompositeVariantId: function (variant) {
            return (variant.language ? variant.language.culture : "invariant") + "_" + (variant.segment ? variant.segment : "null");
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
        getAllowedActions: function (content, creating) {

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
                    if (content.publishDate && _.contains(content.allowedActions, "U")) {
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
        getButtonFromAction: function (ch) {
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
                        labelKey: "content_unpublish",
                        handler: "unpublish"
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
        reBindChangedProperties: function (origContent, savedContent) {

            // TODO: We should probably split out this logic to deal with media/members separately to content

            //a method to ignore built-in prop changes
            var shouldIgnore = function (propName) {
                return _.some([
                    "variants",

                    "tabs",
                    "properties",
                    "apps",
                    "createDateFormatted",
                    "releaseDateFormatted",
                    "expireDateFormatted",
                    "releaseDate",
                    "expireDate"
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
            }
            else {
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
                    var variantPropertiesSync = ["state"];

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
                        if (angular.isFunction(origProp.onValueChanged)) {
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

        handleSaveError: function (args) {

            if (!args.err) {
                throw "args.err cannot be null";
            }
            
            //When the status is a 400 status with a custom header: X-Status-Reason: Validation failed, we have validation errors.
            //Otherwise the error is probably due to invalid data (i.e. someone mucking around with the ids or something).
            //Or, some strange server error
            if (args.err.status === 400) {
                //now we need to look through all the validation errors
                if (args.err.data && (args.err.data.ModelState)) {

                    //wire up the server validation errs
                    formHelper.handleServerValidation(args.err.data.ModelState);

                    //add model state errors to notifications
                    if (args.showNotifications) {
                        for (var e in args.err.data.ModelState) {
                            notificationsService.error("Validation", args.err.data.ModelState[e][0]);
                        }
                    }

                    if (!this.redirectToCreatedContent(args.err.data.id, args.softRedirect) || args.softRedirect) {
                        // If we are not redirecting it's because this is not newly created content, else in some cases we are
                        // soft-redirecting which means the URL will change but the route wont (i.e. creating content). 

                        // In this case we need to detect what properties have changed and re-bind them with the server data.
                        if (args.rebindCallback && angular.isFunction(args.rebindCallback)) {
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

        handleSuccessfulSave: function (args) {

            if (!args) {
                throw "args cannot be null";
            }
            if (!args.savedContent) {
                throw "args.savedContent cannot be null";
            }

            if (!this.redirectToCreatedContent(args.redirectId ? args.redirectId : args.savedContent.id, args.softRedirect) || args.softRedirect) {

                // If we are not redirecting it's because this is not newly created content, else in some cases we are
                // soft-redirecting which means the URL will change but the route wont (i.e. creating content). 

                // In this case we need to detect what properties have changed and re-bind them with the server data.
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
        redirectToCreatedContent: function (id, softRedirect) {

            //only continue if we are currently in create mode and not in infinite mode and if the resulting ID is valid
            if ($routeParams.create && (isValidIdentifier(id))) {

                //need to change the location to not be in 'create' mode. Currently the route will be something like:
                // /belle/#/content/edit/1234?doctype=newsArticle&create=true
                // but we need to remove everything after the query so that it is just:
                // /belle/#/content/edit/9876 (where 9876 is the new id)

                //clear the query strings
                navigationService.clearSearch(["cculture", "csegment"]);
                if (softRedirect) {
                    navigationService.setSoftRedirect();
                }
                //change to new path
                $location.path("/" + $routeParams.section + "/" + $routeParams.tree + "/" + $routeParams.method + "/" + id);                
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

        redirectToRenamedContent: function (id) {
            //clear the query strings
            navigationService.clearSearch();
            //change to new path
            $location.path("/" + $routeParams.section + "/" + $routeParams.tree + "/" + $routeParams.method + "/" + id);
            //don't add a browser history for this
            $location.replace();
            return true;
        }
    };
}
angular.module('umbraco.services').factory('contentEditingHelper', contentEditingHelper);
