(function () {
    'use strict';

    /**
    * @ngdoc service
    * @name umbraco.services.umbDataFormatter
    * @description A helper object used to format/transform JSON Umbraco data, mostly used for persisting data to the server
    **/
    function umbDataFormatter() {
        
        return {

            formatChangePasswordModel: function(model) {
                if (!model) {
                    return null;
                }
                var trimmed = _.omit(model, ["confirm", "generatedPassword"]);

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

            formatContentTypePostData: function (displayModel, action) {

                //create the save model from the display model
                var saveModel = _.pick(displayModel,
                    'compositeContentTypes', 'isContainer', 'allowAsRoot', 'allowedTemplates', 'allowedContentTypes',
                    'alias', 'description', 'thumbnail', 'name', 'id', 'icon', 'trashed',
                    'key', 'parentId', 'alias', 'path');

                //TODO: Map these
                saveModel.allowedTemplates = _.map(displayModel.allowedTemplates, function (t) { return t.alias; });
                saveModel.defaultTemplate = displayModel.defaultTemplate ? displayModel.defaultTemplate.alias : null;
                var realGroups = _.reject(displayModel.groups, function (g) {
                    //do not include these tabs
                    return g.tabState === "init";
                });
                saveModel.groups = _.map(realGroups, function (g) {

                    var saveGroup = _.pick(g, 'inherited', 'id', 'sortOrder', 'name');

                    var realProperties = _.reject(g.properties, function (p) {
                        //do not include these properties
                        return p.propertyState === "init" || p.inherited === true;
                    });

                    var saveProperties = _.map(realProperties, function (p) {
                        var saveProperty = _.pick(p, 'id', 'alias', 'description', 'validation', 'label', 'sortOrder', 'dataTypeId', 'groupId', 'memberCanEdit', 'showOnMemberProfile', 'isSensitiveData');
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
            formatDataTypePostData: function (displayModel, preValues, action) {
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
            formatDictionaryPostData : function(dictionary, nameIsDirty) {
                var saveModel = {
                    parentId: dictionary.parentId,
                    id: dictionary.id,
                    name: dictionary.name,
                    nameIsDirty: nameIsDirty,
                    translations: [],
                    key : dictionary.key
                };

                for(var i = 0; i < dictionary.translations.length; i++) {
                    saveModel.translations.push({
                        isoCode: dictionary.translations[i].isoCode,
                        languageId: dictionary.translations[i].languageId,
                        translation: dictionary.translations[i].translation
                    });
                }

                return saveModel;
            },

            /** formats the display model used to display the user to the model used to save the user */
            formatUserPostData: function (displayModel) {

                //create the save model from the display model
                var saveModel = _.pick(displayModel, 'id', 'parentId', 'name', 'username', 'culture', 'email', 'startContentIds', 'startMediaIds', 'userGroups', 'message', 'changePassword');
                saveModel.changePassword = this.formatChangePasswordModel(saveModel.changePassword);

                //make sure the userGroups are just a string array
                var currGroups = saveModel.userGroups;
                var formattedGroups = [];
                for (var i = 0; i < currGroups.length; i++) {
                    if (!angular.isString(currGroups[i])) {
                        formattedGroups.push(currGroups[i].alias);
                    }
                    else {
                        formattedGroups.push(currGroups[i]);
                    }
                }
                saveModel.userGroups = formattedGroups;

                //make sure the startnodes are just a string array
                var props = ["startContentIds", "startMediaIds"];
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
            formatUserGroupPostData: function (displayModel, action) {
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
                    if (!angular.isString(currSections[i])) {
                        formattedSections.push(currSections[i].alias);
                    }
                    else {
                        formattedSections.push(currSections[i]);
                    }
                }
                saveModel.sections = formattedSections;

                //make sure the user are just an int array
                var currUsers = saveModel.users;
                var formattedUsers = [];
                for (var j = 0; j < currUsers.length; j++) {
                    if (!angular.isNumber(currUsers[j])) {
                        formattedUsers.push(currUsers[j].id);
                    }
                    else {
                        formattedUsers.push(currUsers[j]);
                    }
                }
                saveModel.users = formattedUsers;

                //make sure the startnodes are just an int if one is set
                var props = ["startContentId", "startMediaId"];
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
            formatMemberPostData: function (displayModel, action) {
                //this is basically the same as for media but we need to explicitly add the username,email, password to the save model

                var saveModel = this.formatMediaPostData(displayModel, action);

                saveModel.key = displayModel.key;

                var genericTab = _.find(displayModel.tabs, function (item) {
                    return item.id === 0;
                });

                //map the member login, email, password and groups
                var propLogin = _.find(genericTab.properties, function (item) {
                    return item.alias === "_umb_login";
                });
                var propEmail = _.find(genericTab.properties, function (item) {
                    return item.alias === "_umb_email";
                });
                var propPass = _.find(genericTab.properties, function (item) {
                    return item.alias === "_umb_password";
                });
                var propGroups = _.find(genericTab.properties, function (item) {
                    return item.alias === "_umb_membergroup";
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
                                case "umbracoMemberLockedOut":
                                    saveModel.isLockedOut = prop.value ? (prop.value.toString() === "1" ? true : false) : false;
                                    break;
                                case "umbracoMemberApproved":
                                    saveModel.isApproved = prop.value ? (prop.value.toString() === "1" ? true : false) : true;
                                    break;
                                case "umbracoMemberComments":
                                    saveModel.comments = prop.value;
                                    break;
                            }
                        }
                    });
                });



                return saveModel;
            },

            /** formats the display model used to display the media to the model used to save the media */
            formatMediaPostData: function (displayModel, action) {
                //NOTE: the display model inherits from the save model so we can in theory just post up the display model but 
                // we don't want to post all of the data as it is unecessary.
                var saveModel = {
                    id: displayModel.id,
                    properties: [],
                    name: displayModel.name,
                    contentTypeAlias: displayModel.contentTypeAlias,
                    parentId: displayModel.parentId,
                    //set the action on the save model
                    action: action
                };

                _.each(displayModel.tabs, function (tab) {

                    _.each(tab.properties, function (prop) {

                        //don't include the custom generic tab properties
                        //don't include a property that is marked readonly
                        if (!prop.alias.startsWith("_umb_") && !prop.readonly) {
                            saveModel.properties.push({
                                id: prop.id,
                                alias: prop.alias,
                                value: prop.value
                            });
                        }
                    });
                });

                return saveModel;
            },

            /** formats the display model used to display the content to the model used to save the content  */
            formatContentPostData: function (displayModel, action) {

                //this is basically the same as for media but we need to explicitly add some extra properties
                var saveModel = this.formatMediaPostData(displayModel, action);

                var propExpireDate = displayModel.removeDate;
                var propReleaseDate = displayModel.releaseDate;
                var propTemplate = displayModel.template;
            
                saveModel.expireDate = propExpireDate ? propExpireDate : null;
                saveModel.releaseDate = propReleaseDate ? propReleaseDate : null;
                saveModel.templateAlias = propTemplate ? propTemplate : null;

                return saveModel;
            }
        };
    }
    angular.module('umbraco.services').factory('umbDataFormatter', umbDataFormatter);

})();
