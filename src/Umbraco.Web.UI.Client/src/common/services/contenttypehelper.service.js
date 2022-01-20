/**
 * @ngdoc service
 * @name umbraco.services.contentTypeHelper
 * @description A helper service for the content type editor
 **/
function contentTypeHelper(contentTypeResource, dataTypeResource, $filter, $injector, $q) {

    var contentTypeHelperService = {

        TYPE_GROUP: 'Group',
        TYPE_TAB: 'Tab',

        isAliasUnique(groups, alias) {
            return groups.find(group => group.alias === alias) ? false : true;
        },

        createUniqueAlias(groups, alias) {
            let i = 1;
            while(this.isAliasUnique(groups, alias + i.toString()) === false) {
                i++;
            }
            return alias + i.toString();
        },

        generateLocalAlias: function(name) {
            return name ? name.toUmbracoAlias() : String.CreateGuid();
        },

        getLocalAlias: function (alias) {
            const lastIndex = alias.lastIndexOf('/');

            return (lastIndex === -1) ? alias : alias.substring(lastIndex + 1);
        },

        updateLocalAlias: function (alias, localAlias) {
            const parentAlias = this.getParentAlias(alias);

            return (parentAlias == null || parentAlias === '') ? localAlias : parentAlias + '/' + localAlias;
        },

        getParentAlias: function (alias) {
            if(alias) {
                const lastIndex = alias.lastIndexOf('/');

                return (lastIndex === -1) ? null : alias.substring(0, lastIndex);
            }
            return null;
        },

        updateParentAlias: function (alias, parentAlias) {
            const localAlias = this.getLocalAlias(alias);

            return (parentAlias == null || parentAlias === '') ? localAlias : parentAlias + '/' + localAlias;
        },

        updateDescendingAliases: function (groups, oldParentAlias, newParentAlias) {
            groups.forEach(group => {
                const parentAlias = this.getParentAlias(group.alias);

                if (parentAlias === oldParentAlias) {
                    const oldAlias = group.alias,
                        newAlias = this.updateParentAlias(oldAlias, newParentAlias);

                    group.alias = newAlias;
                    group.parentAlias = newParentAlias;
                    this.updateDescendingAliases(groups, oldAlias, newAlias);

                }
            });
        },

        defineParentAliasOnGroups: function (groups) {
            groups.forEach(group => {
                group.parentAlias = this.getParentAlias(group.alias);
            });
        },

        relocateDisorientedGroups: function (groups) {
            const existingAliases = groups.map(group => group.alias);
            existingAliases.push(null);
            const disorientedGroups = groups.filter(group => existingAliases.indexOf(group.parentAlias) === -1);
            disorientedGroups.forEach(group => {
                const oldAlias = group.alias,
                        newAlias = this.updateParentAlias(oldAlias, null);

                group.alias = newAlias;
                group.parentAlias = null;
                this.updateDescendingAliases(groups, oldAlias, newAlias);
            });
        },

        convertGroupToTab: function (groups, group) {
            const tabs = groups.filter(group => group.type === this.TYPE_TAB).sort((a, b) => a.sortOrder - b.sortOrder);
            const nextSortOrder = tabs && tabs.length > 0 ? tabs[tabs.length - 1].sortOrder + 1 : 0;

            group.convertingToTab = true;

            group.type = this.TYPE_TAB;

            const newAlias = this.generateLocalAlias(group.name);
            // when checking for alias uniqueness we need to exclude the current group or the alias would get a + 1
            const otherGroups = [...groups].filter(groupCopy => !groupCopy.convertingToTab);

            group.alias = this.isAliasUnique(otherGroups, newAlias) ? newAlias : this.createUniqueAlias(otherGroups, newAlias);
            group.parentAlias = null;
            group.sortOrder = nextSortOrder;
            group.convertingToTab = false;
        },

        createIdArray: function (array) {

            var newArray = [];

            array.forEach(function (arrayItem) {

                if (Utilities.isObject(arrayItem)) {
                    newArray.push(arrayItem.id);
                } else {
                    newArray.push(arrayItem);
                }

            });

            return newArray;

        },

        generateModels: function () {
            var deferred = $q.defer();
            var modelsResource = $injector.has("modelsBuilderManagementResource") ? $injector.get("modelsBuilderManagementResource") : null;
            var modelsBuilderEnabled = Umbraco.Sys.ServerVariables.umbracoPlugins.modelsBuilder.mode !== "Nothing";
            if (modelsBuilderEnabled && modelsResource) {
                modelsResource.buildModels().then(function (result) {
                    deferred.resolve(result);

                    //just calling this to get the servar back to life
                    modelsResource.getModelsOutOfDateStatus();

                }, function (e) {
                    deferred.reject(e);
                });
            }
            else {
                deferred.resolve(false);
            }
            return deferred.promise;
        },

        checkModelsBuilderStatus: function () {
            var deferred = $q.defer();
            var modelsResource = $injector.has("modelsBuilderManagementResource") ? $injector.get("modelsBuilderManagementResource") : null;
            var modelsBuilderEnabled = (Umbraco && Umbraco.Sys && Umbraco.Sys.ServerVariables && Umbraco.Sys.ServerVariables.umbracoPlugins && Umbraco.Sys.ServerVariables.umbracoPlugins.modelsBuilder && Umbraco.Sys.ServerVariables.umbracoPlugins.modelsBuilder.mode !== "Nothing");

            if (modelsBuilderEnabled && modelsResource) {
                modelsResource.getModelsOutOfDateStatus().then(function (result) {
                    //Generate models buttons should be enabled if it is 0
                    deferred.resolve(result.status === 0);
                });
            }
            else {
                deferred.resolve(false);
            }
            return deferred.promise;
        },

        makeObjectArrayFromId: function (idArray, objectArray) {
            var newArray = [];

            for (var idIndex = 0; idArray.length > idIndex; idIndex++) {
                var id = idArray[idIndex];

                for (var objectIndex = 0; objectArray.length > objectIndex; objectIndex++) {
                    var object = objectArray[objectIndex];
                    if (id === object.id) {
                        newArray.push(object);
                    }
                }

            }

            return newArray;
        },

        validateAddingComposition: function (contentType, compositeContentType) {

            //Validate that by adding this group that we are not adding duplicate property type aliases

            var propertiesAdding = _.flatten(_.map(compositeContentType.groups, function (g) {
                return _.map(g.properties, function (p) {
                    return p.alias;
                });
            }));
            var propAliasesExisting = _.filter(_.flatten(_.map(contentType.groups, function (g) {
                return _.map(g.properties, function (p) {
                    return p.alias;
                });
            })), function (f) {
                return f !== null && f !== undefined;
            });

            var intersec = _.intersection(propertiesAdding, propAliasesExisting);
            if (intersec.length > 0) {
                //return the overlapping property aliases
                return intersec;
            }

            //no overlapping property aliases
            return [];
        },

        mergeCompositeContentType: function (contentType, compositeContentType) {

            //Validate that there are no overlapping aliases
            var overlappingAliases = this.validateAddingComposition(contentType, compositeContentType);
            if (overlappingAliases.length > 0) {
                throw new Error("Cannot add this composition, these properties already exist on the content type: " + overlappingAliases.join());
            }

            compositeContentType.groups.forEach(function (compositionGroup) {
                // order composition groups based on sort order
                compositionGroup.properties = $filter('orderBy')(compositionGroup.properties, 'sortOrder');

                // get data type details
                compositionGroup.properties.forEach(function (property) {
                    dataTypeResource.getById(property.dataTypeId)
                        .then(function (dataType) {
                            property.dataTypeIcon = dataType.icon;
                            property.dataTypeName = dataType.name;
                        });
                });

                // set inherited state on tab
                compositionGroup.inherited = true;

                // set inherited state on properties
                compositionGroup.properties.forEach(function (compositionProperty) {
                    compositionProperty.inherited = true;
                });

                // set tab state
                compositionGroup.tabState = "inActive";

                // if groups are named the same - merge the groups
                contentType.groups.forEach(function (contentTypeGroup) {

                    if (contentTypeGroup.name === compositionGroup.name && contentTypeGroup.type === compositionGroup.type) {

                        // set flag to show if properties has been merged into a tab
                        compositionGroup.groupIsMerged = true;

                        // make group inherited
                        contentTypeGroup.inherited = true;

                        // add properties to the top of the array
                        contentTypeGroup.properties = compositionGroup.properties.concat(contentTypeGroup.properties);

                        // update sort order on all properties in merged group
                        contentTypeGroup.properties = contentTypeHelperService.updatePropertiesSortOrder(contentTypeGroup.properties);

                        // make parentTabContentTypeNames to an array so we can push values
                        if (contentTypeGroup.parentTabContentTypeNames === null || contentTypeGroup.parentTabContentTypeNames === undefined) {
                            contentTypeGroup.parentTabContentTypeNames = [];
                        }

                        // push name to array of merged composite content types
                        contentTypeGroup.parentTabContentTypeNames.push(compositeContentType.name);

                        // make parentTabContentTypes to an array so we can push values
                        if (contentTypeGroup.parentTabContentTypes === null || contentTypeGroup.parentTabContentTypes === undefined) {
                            contentTypeGroup.parentTabContentTypes = [];
                        }

                        // push id to array of merged composite content types
                        contentTypeGroup.parentTabContentTypes.push(compositeContentType.id);

                        // get sort order from composition
                        contentTypeGroup.sortOrder = compositionGroup.sortOrder;

                        // splice group to the top of the array
                        var contentTypeGroupCopy = Utilities.copy(contentTypeGroup);
                        var index = contentType.groups.indexOf(contentTypeGroup);
                        contentType.groups.splice(index, 1);
                        contentType.groups.unshift(contentTypeGroupCopy);

                    }

                });

                // if group is not merged - push it to the end of the array - before init tab
                if (compositionGroup.groupIsMerged === false || compositionGroup.groupIsMerged === undefined) {

                    // make parentTabContentTypeNames to an array so we can push values
                    if (compositionGroup.parentTabContentTypeNames === null || compositionGroup.parentTabContentTypeNames === undefined) {
                        compositionGroup.parentTabContentTypeNames = [];
                    }

                    // push name to array of merged composite content types
                    compositionGroup.parentTabContentTypeNames.push(compositeContentType.name);

                    // make parentTabContentTypes to an array so we can push values
                    if (compositionGroup.parentTabContentTypes === null || compositionGroup.parentTabContentTypes === undefined) {
                        compositionGroup.parentTabContentTypes = [];
                    }

                    // push id to array of merged composite content types
                    compositionGroup.parentTabContentTypes.push(compositeContentType.id);

                    // push group before placeholder tab
                    contentType.groups.unshift(compositionGroup);

                }

            });

            // sort all groups by sortOrder property
            contentType.groups = $filter('orderBy')(contentType.groups, 'sortOrder');

            return contentType;

        },

        splitCompositeContentType: function (contentType, compositeContentType) {

            var groups = [];

            contentType.groups.forEach(function (contentTypeGroup) {

                if (contentTypeGroup.tabState !== "init") {

                    var idIndex = contentTypeGroup.parentTabContentTypes.indexOf(compositeContentType.id);
                    var nameIndex = contentTypeGroup.parentTabContentTypeNames.indexOf(compositeContentType.name);
                    var groupIndex = contentType.groups.indexOf(contentTypeGroup);


                    if (idIndex !== -1) {

                        var properties = [];

                        // remove all properties from composite content type
                        contentTypeGroup.properties.forEach(function (property) {
                            if (property.contentTypeId !== compositeContentType.id) {
                                properties.push(property);
                            }
                        });

                        // set new properties array to properties
                        contentTypeGroup.properties = properties;

                        // remove composite content type name and id from inherited arrays
                        contentTypeGroup.parentTabContentTypes.splice(idIndex, 1);
                        contentTypeGroup.parentTabContentTypeNames.splice(nameIndex, 1);

                        // remove inherited state if there are no inherited properties
                        if (contentTypeGroup.parentTabContentTypes.length === 0) {
                            contentTypeGroup.inherited = false;
                        }

                        // remove group if there are no properties left
                        if (contentTypeGroup.properties.length > 0) {
                            groups.push(contentTypeGroup);
                        }

                    } else {
                        groups.push(contentTypeGroup);
                    }

                } else {
                    groups.push(contentTypeGroup);
                }

                // update sort order on properties
                contentTypeGroup.properties = contentTypeHelperService.updatePropertiesSortOrder(contentTypeGroup.properties);

            });

            contentType.groups = groups;

        },

        updatePropertiesSortOrder: function (properties) {

            var sortOrder = 0;

            properties.forEach(function (property) {
                if (!property.inherited && property.propertyState !== "init") {
                    property.sortOrder = sortOrder;
                }
                sortOrder++;
            });

            return properties;

        },

        getTemplatePlaceholder: function () {

            var templatePlaceholder = {
                "name": "",
                "icon": "icon-layout",
                "alias": "templatePlaceholder",
                "placeholder": true
            };

            return templatePlaceholder;

        },

        insertDefaultTemplatePlaceholder: function (defaultTemplate) {

            // get template placeholder
            var templatePlaceholder = contentTypeHelperService.getTemplatePlaceholder();

            // add as default template
            defaultTemplate = templatePlaceholder;

            return defaultTemplate;

        },

        insertTemplatePlaceholder: function (array) {

            // get template placeholder
            var templatePlaceholder = contentTypeHelperService.getTemplatePlaceholder();

            // add as selected item
            array.push(templatePlaceholder);

            return array;

        },

        insertChildNodePlaceholder: function (array, name, icon, id) {

            var placeholder = {
                "name": name,
                "icon": icon,
                "id": id
            };

            array.push(placeholder);

        },

        rebindSavedContentType: function (contentType, savedContentType) {
            // The saved content type might have updated values (eg. new IDs/keys), so make sure the view model is updated
            contentType.ModelState = savedContentType.ModelState;
            contentType.id = savedContentType.id;

            // Prevent rebinding if there was an error: https://github.com/umbraco/Umbraco-CMS/pull/11257
            if (savedContentType.ModelState) {
              return;
            }

            contentType.groups.forEach(function (group) {
                if (!group.alias) return;

                var k = 0;
                while (k < savedContentType.groups.length && savedContentType.groups[k].alias != group.alias)
                    k++;

                if (k == savedContentType.groups.length) {
                    group.id = 0;
                    return;
                }

                var savedGroup = savedContentType.groups[k];
                group.id = savedGroup.id;
                group.key = savedGroup.key;
                group.contentTypeId = savedGroup.contentTypeId;

                group.properties.forEach(function (property) {
                    if (property.id || !property.alias) return;

                    k = 0;
                    while (k < savedGroup.properties.length && savedGroup.properties[k].alias != property.alias)
                        k++;

                    if (k == savedGroup.properties.length) {
                        property.id = 0;
                        return;
                    }

                    var savedProperty = savedGroup.properties[k];
                    property.id = savedProperty.id;
                });
            });
        }

    };

    return contentTypeHelperService;
}
angular.module('umbraco.services').factory('contentTypeHelper', contentTypeHelper);
