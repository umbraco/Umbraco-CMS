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
                    if (!prop.alias.startsWith("_umb_") && !prop.readonly) {
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

            formatChangePasswordModel: function (model) {
                if (!model) {
                    return null;
                }
                var trimmed = _.omit(model, ["confirm"]);

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
                // Create the save model from the display model
                var saveModel = _.pick(displayModel,
                    'compositeContentTypes', 'isContainer', 'allowAsRoot', 'allowedTemplates', 'allowedContentTypes',
                    'alias', 'description', 'thumbnail', 'name', 'id', 'icon', 'trashed',
                    'key', 'parentId', 'alias', 'path', 'allowCultureVariant', 'allowSegmentVariant', 'isElement', 'historyCleanup');

                saveModel.allowedTemplates = _.map(displayModel.allowedTemplates, function (t) { return t.alias; });
                saveModel.defaultTemplate = displayModel.defaultTemplate ? displayModel.defaultTemplate.alias : null;
                var realGroups = _.reject(displayModel.groups, function (g) {
                    // Do not include groups with init state
                    return g.tabState === "init";
                });
                saveModel.groups = _.map(realGroups, function (g) {
                    var saveGroup = _.pick(g, 'id', 'sortOrder', 'name', 'key', 'alias', 'type');

                    var realProperties = _.reject(g.properties, function (p) {
                        // Do not include properties with init state or inherited from a composition
                        return p.propertyState === "init" || p.inherited === true;
                    });

                    var saveProperties = _.map(realProperties, function (p) {
                        var saveProperty = _.pick(p, 'id', 'alias', 'description', 'validation', 'label', 'sortOrder', 'dataTypeId', 'groupId', 'memberCanEdit', 'showOnMemberProfile', 'isSensitiveData', 'allowCultureVariant', 'allowSegmentVariant', 'labelOnTop');
                        return saveProperty;
                    });

                    saveGroup.properties = saveProperties;

                    if (g.inherited === true) {
                        if (saveProperties.length === 0) {
                            // All properties are inherited from the compositions, no need to save this group
                            return null;
                        } else if (g.contentTypeId != saveModel.id) {
                            // We have local properties, but the group id is not local, ensure a new id/key is generated on save
                            saveGroup = _.omit(saveGroup, 'id', 'key');
                        }
                    }

                    return saveGroup;
                });

                saveModel.groups = _.reject(saveModel.groups, function (g) {
                    // Do not include empty/null groups
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
            formatDictionaryPostData: function (dictionary, nameIsDirty) {
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
            formatUserPostData: function (displayModel) {

                //create the save model from the display model
                var saveModel = _.pick(displayModel, 'id', 'parentId', 'name', 'username', 'culture', 'email', 'startContentIds', 'startMediaIds', 'userGroups', 'message', 'key');

                //make sure the userGroups are just a string array
                var currGroups = saveModel.userGroups;
                var formattedGroups = [];
                for (var i = 0; i < currGroups.length; i++) {
                    if (!Utilities.isString(currGroups[i])) {
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
                var saveModel = _.pick(displayModel, 'id', 'alias', 'name', 'icon', 'sections', 'users', 'defaultPermissions', 'assignedPermissions', 'languages', 'hasAccessToAllLanguages');

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
                    if (!Utilities.isNumber(currUsers[j])) {
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

                // make sure the allowed languages are just an array of id's
                saveModel.allowedLanguages = saveModel.languages && saveModel.languages.length > 0 ? saveModel.languages.map(language => language.id) : [];

                saveModel.parentId = -1;
                return saveModel;
            },

            /** formats the display model used to display the member to the model used to save the member */
            formatMemberPostData: function (displayModel, action) {
                //this is basically the same as for media but we need to explicitly add the username, email, password to the save model

                var saveModel = this.formatMediaPostData(displayModel, action);

                saveModel.key = displayModel.key;

                // Map membership properties
                _.each(displayModel.membershipProperties, prop => {
                    switch (prop.alias) {
                        case '_umb_login':
                            saveModel.username = prop.value.trim();
                            break;
                        case '_umb_email':
                            saveModel.email = prop.value.trim();
                            break;
                        case '_umb_password':
                            saveModel.password = this.formatChangePasswordModel(prop.value);
                            break;
                        case '_umb_membergroup':
                            saveModel.memberGroups = _.keys(_.pick(prop.value, value => value === true));
                            break;
                        case '_umb_approved':
                            saveModel.isApproved = prop.value;
                            break;
                        case '_umb_lockedOut':
                            saveModel.isLockedOut = prop.value;
                            break;
                    }
                });

                // saveModel.password = this.formatChangePasswordModel(propPass.value);
                //
                // var selectedGroups = [];
                // for (var n in propGroups.value) {
                //     if (propGroups.value[n] === true) {
                //         selectedGroups.push(n);
                //     }
                // }
                // saveModel.memberGroups = selectedGroups;

                // Map custom member provider properties
                var memberProviderPropAliases = _.pairs(displayModel.fieldConfig);
                _.each(displayModel.tabs, tab => {
                    _.each(tab.properties, prop => {
                        var foundAlias = _.find(memberProviderPropAliases, item => prop.alias === item[1]);
                        if (foundAlias) {
                            // we know the current property matches an alias, now we need to determine which membership provider property it was for
                            // by looking at the key
                            switch (foundAlias[0]) {
                                case "umbracoMemberLockedOut":
                                    saveModel.isLockedOut = Object.toBoolean(prop.value);
                                    break;
                                case "umbracoMemberApproved":
                                    saveModel.isApproved = Object.toBoolean(prop.value);
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
            formatContentPostData: function (displayModel, action) {

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
                            name: v.name || "", //if its null/empty,we must pass up an empty string else we get json converter errors
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
            formatContentGetData: function (displayModel) {

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
            formatRelationTypePostData: function (relationType) {
                var saveModel = {
                    id: relationType.id,
                    name: relationType.name,
                    alias: relationType.alias,
                    key: relationType.key,
                    isBidirectional: relationType.isBidirectional,
                    isDependency: relationType.isDependency,
                    parentObjectType: relationType.parentObjectType,
                    childObjectType: relationType.childObjectType
                };

                return saveModel;
            }
        };
    }
    angular.module('umbraco.services').factory('umbDataFormatter', umbDataFormatter);

})();
