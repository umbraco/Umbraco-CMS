/**
 * @ngdoc service
 * @name umbraco.services.blockEditorModelObject
 *
 * @description
 * <b>Added in Umbraco 8.7</b>. Model Object for dealing with data of Block Editors.
 *
 * Block Editor Model Object provides the basic features for editing data of a block editor.<br/>
 * Use the Block Editor Service to instantiate the Model Object.<br/>
 * See {@link umbraco.services.blockEditorService blockEditorService}
 *
 */
(function () {
    'use strict';

    function blockEditorModelObjectFactory($interpolate, $q, udiService, contentResource, localizationService, umbRequestHelper, clipboardService, notificationsService, $compile) {

        /**
         * Simple mapping from property model content entry to editing model,
         * needs to stay simple to avoid deep watching.
         */
        function mapToElementModel(elementModel, dataModel) {

            if (!elementModel || !elementModel.variants || !elementModel.variants.length) { return; }

            var variant = elementModel.variants[0];

            for (var t = 0; t < variant.tabs.length; t++) {
                var tab = variant.tabs[t];

                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];

                    prop.value = dataModel[prop.alias];
                }
            }

        }

        /**
         * Simple mapping from elementModel to property model content entry,
         * needs to stay simple to avoid deep watching.
         */
        function mapToPropertyModel(elementModel, dataModel) {

            if (!elementModel || !elementModel.variants || !elementModel.variants.length) { return; }

            var variant = elementModel.variants[0];

            for (var t = 0; t < variant.tabs.length; t++) {
                var tab = variant.tabs[t];

                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];

                    dataModel[prop.alias] = prop.value;
                }
            }

        }

        /**
         * Map property values from an ElementModel to another ElementModel.
         * Used to tricker watchers for synchronization.
         * @param {Object} fromModel ElementModel to receive property values from.
         * @param {Object} toModel ElementModel to receive property values from.
         */
        function mapElementValues(fromModel, toModel) {
            if (!fromModel || !fromModel.variants) {
                toModel.variants = null;
                return;
            }
            if (!fromModel.variants.length) {
                toModel.variants = [];
                return;
            }

            var fromVariant = fromModel.variants[0];
            if (!fromVariant) {
                toModel.variants = [null];
                return;
            }

            var toVariant = toModel.variants[0];

            for (var t = 0; t < fromVariant.tabs.length; t++) {
                var fromTab = fromVariant.tabs[t];
                var toTab = toVariant.tabs.find(tab => tab.alias === fromTab.alias);

                if (fromTab && fromTab.properties && fromTab.properties.length > 0 && toTab && toTab.properties && toTab.properties.length > 0) {
                    for (var p = 0; p < fromTab.properties.length; p++) {
                        var fromProp = fromTab.properties[p];
                        var toProp = toTab.properties[p];
                        toProp.value = fromProp.value;
                    }
                }
            }
        }

        /**
         * Used to add watchers on all properties in a content or settings model
         */
        function addWatchers(blockObject, isolatedScope, forSettings) {
            var model = forSettings ? blockObject.settings : blockObject.content;
            if (!model || !model.variants || !model.variants.length) { return; }

            // Start watching each property value.
            var variant = model.variants[0];
            var field = forSettings ? "settings" : "content";
            var watcherCreator = forSettings ? createSettingsModelPropWatcher : createContentModelPropWatcher;
            for (var t = 0; t < variant.tabs.length; t++) {
                var tab = variant.tabs[t];
                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];

                    // Watch value of property since this is the only value we want to keep synced.
                    // Do notice that it is not performing a deep watch, meaning that we are only watching primitive and changes directly to the object of property-value.
                    // But we like to sync non-primitive values as well! Yes, and this does happen, just not through this code, but through the nature of JavaScript.
                    // Non-primitive values act as references to the same data and are therefor synced.
                    blockObject.__watchers.push(isolatedScope.$watch("blockObjects._" + blockObject.key + "." + field + ".variants[0].tabs[" + t + "].properties[" + p + "].value", watcherCreator(blockObject, prop)));

                    // We also like to watch our data model to be able to capture changes coming from other places.
                    if (forSettings === true) {
                        blockObject.__watchers.push(isolatedScope.$watch("blockObjects._" + blockObject.key + "." + "settingsData" + "." + prop.alias, createLayoutSettingsModelWatcher(blockObject, prop)));
                    } else {
                        blockObject.__watchers.push(isolatedScope.$watch("blockObjects._" + blockObject.key + "." + "data" + "." + prop.alias, createDataModelWatcher(blockObject, prop)));
                    }
                }
            }
        }

        /**
         * Used to create a prop watcher for the data in the property editor data model.
         */
        function createDataModelWatcher(blockObject, prop) {
            return function () {
                if (prop.value !== blockObject.data[prop.alias]) {

                    // sync data:
                    prop.value = blockObject.data[prop.alias];
                }
            }
        }

        /**
         * Used to create a prop watcher for the settings in the property editor data model.
         */
        function createLayoutSettingsModelWatcher(blockObject, prop) {
            return function () {
                if (prop.value !== blockObject.settingsData[prop.alias]) {
                    // sync data:
                    prop.value = blockObject.settingsData[prop.alias];
                }
            }
        }

        /**
         * Used to create a scoped watcher for a content property on a blockObject.
         */
        function createContentModelPropWatcher(blockObject, prop) {
            return function () {
                if (blockObject.data[prop.alias] !== prop.value) {
                    // sync data:
                    blockObject.data[prop.alias] = prop.value;
                }
            }
        }

        /**
         * Used to create a scoped watcher for a settings property on a blockObject.
         */
        function createSettingsModelPropWatcher(blockObject, prop) {
            return function () {
                if (blockObject.settingsData[prop.alias] !== prop.value) {
                    // sync data:
                    blockObject.settingsData[prop.alias] = prop.value;
                }
            }
        }

        function createDataEntry(elementTypeKey, dataItems) {
            var data = {
                contentTypeKey: elementTypeKey,
                udi: udiService.create("element")
            };
            dataItems.push(data);
            return data.udi;
        }

        function getDataByUdi(udi, dataItems) {
            return dataItems.find(entry => entry.udi === udi) || null;
        }

        /**
         * Set the udi and key property for the content item
         * @param {any} contentData
         * @param {any} udi
         */
        function ensureUdiAndKey(contentData, udi) {
            contentData.udi = udi;
            // Change the content.key to the GUID part of the udi, else it's just random which we don't want, it must be consistent
            contentData.key = udiService.getKey(udi);
        }

        /**
         * Used to highlight unsupported properties for the user, changes unsupported properties into a unsupported-property.
         */
        var notSupportedProperties = [
            "Umbraco.Tags",
            "Umbraco.UploadField",
            "Umbraco.ImageCropper",
            "Umbraco.NestedContent"
        ];


        /**
         * Formats the content apps and ensures unsupported property's have the notsupported view
         * @param {any} scaffold
         */
        function formatScaffoldData(scaffold) {

            // deal with not supported props
            scaffold.variants.forEach((variant) => {
                variant.tabs.forEach((tab) => {
                    tab.properties.forEach((property) => {
                        if (notSupportedProperties.indexOf(property.editor) !== -1) {
                            property.view = "notsupported";
                        }
                    });
                });
            });

            // could be empty in tests
            if (!scaffold.apps) {
                console.warn("No content apps found in scaffold");
                return scaffold;
            }

            // replace view of content app

            var contentApp = scaffold.apps.find(entry => entry.alias === "umbContent");
            if (contentApp) {
                contentApp.view = "views/common/infiniteeditors/blockeditor/blockeditor.content.html";
            }

            // remove info app
            var infoAppIndex = scaffold.apps.findIndex(entry => entry.alias === "umbInfo");
            if (infoAppIndex >= 0) {
                scaffold.apps.splice(infoAppIndex, 1);
            }

            return scaffold;
        }

        /**
         * Creates a settings content app, we only want to do this if settings is present on the specific block.
         * @param {any} contentModel
         */
        function appendSettingsContentApp(contentModel, settingsName) {
            if (!contentModel.apps) {
                return
            }

            // add the settings app
            var settingsTab = {
                "name": settingsName,
                "alias": "settings",
                "icon": "icon-settings",
                "view": "views/common/infiniteeditors/blockeditor/blockeditor.settings.html",
                "hasError": false
            };
            contentModel.apps.push(settingsTab);
        }

        /**
         * @ngdoc method
         * @name constructor
         * @methodOf umbraco.services.blockEditorModelObject
         * @description Constructor of the model object used to handle Block Editor data.
         * @param {object} propertyModelValue data object of the property editor, usually model.value.
         * @param {string} propertyEditorAlias alias of the property.
         * @param {object} blockConfigurations block configurations.
         * @param {angular-scope} scopeOfExistence A local angularJS scope that exists as long as the data exists.
         * @param {angular-scope} propertyEditorScope A local angularJS scope that represents the property editors scope.
         * @returns {BlockEditorModelObject} A instance of BlockEditorModelObject.
         */
        function BlockEditorModelObject(propertyModelValue, propertyEditorAlias, blockConfigurations, scopeOfExistence, propertyEditorScope) {

            if (!propertyModelValue) {
                throw new Error("propertyModelValue cannot be undefined, to ensure we keep the binding to the angular model we need minimum an empty object.");
            }

            this.__watchers = [];

            this.__labels = {};

            // ensure basic part of data-structure is in place:
            this.value = propertyModelValue;
            this.value.layout = this.value.layout || {};
            this.value.contentData = this.value.contentData || [];
            this.value.settingsData = this.value.settingsData || [];

            this.propertyEditorAlias = propertyEditorAlias;
            this.blockConfigurations = blockConfigurations;

            this.blockConfigurations.forEach(blockConfiguration => {
                if (blockConfiguration.view != null && blockConfiguration.view !== "") {
                    blockConfiguration.view = umbRequestHelper.convertVirtualToAbsolutePath(blockConfiguration.view);
                }
                if (blockConfiguration.stylesheet != null && blockConfiguration.stylesheet !== "") {
                    blockConfiguration.stylesheet = umbRequestHelper.convertVirtualToAbsolutePath(blockConfiguration.stylesheet);
                }
                if (blockConfiguration.thumbnail != null && blockConfiguration.thumbnail !== "") {
                    blockConfiguration.thumbnail = umbRequestHelper.convertVirtualToAbsolutePath(blockConfiguration.thumbnail);
                }
            });

            this.scaffolds = [];
            this.__scopeOfExistence = scopeOfExistence;
            this.isolatedScope = scopeOfExistence.$new(true);
            this.isolatedScope.blockObjects = {};

            this.__watchers.push(this.isolatedScope.$on("$destroy", this.destroy.bind(this)));
            this.__watchers.push(propertyEditorScope.$on("formSubmittingFinalPhase", this.sync.bind(this)));

        };

        BlockEditorModelObject.prototype = {

            update: function (propertyModelValue, propertyEditorScope) {
                // clear watchers
                this.__watchers.forEach(w => { w(); });
                delete this.__watchers;

                // clear block objects
                for (const key in this.isolatedScope.blockObjects) {
                    this.destroyBlockObject(this.isolatedScope.blockObjects[key]);
                }
                this.isolatedScope.blockObjects = {};

                // update our values
                this.value = propertyModelValue;
                this.value.layout = this.value.layout || {};
                this.value.contentData = this.value.contentData || [];
                this.value.settingsData = this.value.settingsData || [];

                // re-create the watchers
                this.__watchers = [];
                this.__watchers.push(this.isolatedScope.$on("$destroy", this.destroy.bind(this)));
                this.__watchers.push(propertyEditorScope.$on("formSubmittingFinalPhase", this.sync.bind(this)));
            },

            /**
             * @ngdoc method
             * @name getBlockConfiguration
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Get block configuration object for a given contentElementTypeKey.
             * @param {string} key contentElementTypeKey to receive the configuration model for.
             * @returns {Object | null} Configuration model for the that specific block. Or ´null´ if the contentElementTypeKey isnt available in the current block configurations.
             */
            getBlockConfiguration: function (key) {
                return this.blockConfigurations.find(bc => bc.contentElementTypeKey === key) || null;
            },

            /**
             * @ngdoc method
             * @name load
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Load the scaffolding models for the given configuration, these are needed to provide useful models for each block.
             * @param {Object} blockObject BlockObject to receive data values from.
             * @returns {Promise} A Promise object which resolves when all scaffold models are loaded.
             */
            load: function () {

                var self = this;

                var tasks = [];

                tasks.push(localizationService.localize("blockEditor_tabBlockSettings").then(
                    function (settingsName) {
                        // self.__labels might not exists anymore, this happens if this instance has been destroyed before the load is complete.
                        if(self.__labels) {
                            self.__labels.settingsName = settingsName;
                        }
                    }
                ));

                var scaffoldKeys = [];

                this.blockConfigurations.forEach(blockConfiguration => {
                    scaffoldKeys.push(blockConfiguration.contentElementTypeKey);
                    if (blockConfiguration.settingsElementTypeKey != null) {
                        scaffoldKeys.push(blockConfiguration.settingsElementTypeKey);
                    }
                });

                // removing duplicates.
                scaffoldKeys = scaffoldKeys.filter((value, index, self) => self.indexOf(value) === index);

                tasks.push(contentResource.getScaffoldByKeys(-20, scaffoldKeys).then(scaffolds => {
                    Object.values(scaffolds).forEach(scaffold => {
                        // self.scaffolds might not exists anymore, this happens if this instance has been destroyed before the load is complete.
                        if (self.scaffolds) {
                            self.scaffolds.push(formatScaffoldData(scaffold));
                        }
                    });
                }).catch(
                    () => {
                        // Do nothing if we get an error.
                    }
                ));

                return $q.all(tasks);
            },

            /**
             * @ngdoc method
             * @name getAvailableAliasesForBlockContent
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Retrieve a list of aliases that are available for content of blocks in this property editor, does not contain aliases of block settings.
             * @return {Array} array of strings representing alias.
             */
            getAvailableAliasesForBlockContent: function () {
                return this.blockConfigurations.map(
                    (blockConfiguration) => {
                        var scaffold = this.getScaffoldFromKey(blockConfiguration.contentElementTypeKey);
                        if (scaffold) {
                            return scaffold.contentTypeAlias;
                        }
                    }
                );
            },

            /**
             * @ngdoc method
             * @name getAvailableBlocksForBlockPicker
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Retrieve a list of available blocks, the list containing object with the configuration model(blockConfigModel) and the element type model(elementTypeModel).
             * The purpose of this data is to provide it for the Block Picker.
             * @return {Array} array of objects representing available blocks, each object containing properties blockConfigModel and elementTypeModel.
             */
            getAvailableBlocksForBlockPicker: function () {

                var blocks = [];

                this.blockConfigurations.forEach(blockConfiguration => {
                    var scaffold = this.getScaffoldFromKey(blockConfiguration.contentElementTypeKey);
                    if (scaffold) {
                        blocks.push({
                            blockConfigModel: blockConfiguration,
                            elementTypeModel: scaffold.documentType
                        });
                    }
                });

                return blocks;
            },

            /**
             * @ngdoc method
             * @name getScaffoldFromKey
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Get scaffold model for a given contentTypeKey.
             * @param {string} key contentTypeKey to receive the scaffold model for.
             * @returns {Object | null} Scaffold model for the that content type. Or null if the scaffolding model dosnt exist in this context.
             */
            getScaffoldFromKey: function (contentTypeKey) {
                return this.scaffolds.find(o => o.contentTypeKey === contentTypeKey) || null;
            },

            /**
             * @ngdoc method
             * @name getScaffoldFromAlias
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Get scaffold model for a given contentTypeAlias, used by clipboardService.
             * @param {string} alias contentTypeAlias to receive the scaffold model for.
             * @returns {Object | null} Scaffold model for the that content type. Or null if the scaffolding model dosnt exist in this context.
             */
            getScaffoldFromAlias: function (contentTypeAlias) {
                return this.scaffolds.find(o => o.contentTypeAlias === contentTypeAlias) || null;
            },

            /**
             * @ngdoc method
             * @name getBlockObject
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Retrieve a Block Object for the given layout entry.
             * The Block Object offers the necessary data to display and edit a block.
             * The Block Object setups live synchronization of content and settings models back to the data of your Property Editor model.
             * The returned object, named ´BlockObject´, contains several useful models to make editing of this block happen.
             * The ´BlockObject´ contains the following properties:
             * - key {string}: runtime generated key, useful for tracking of this object
             * - content {Object}: Content model, the content data in a ElementType model.
             * - settings {Object}: Settings model, the settings data in a ElementType model.
             * - config {Object}: A local deep copy of the block configuration model.
             * - label {string}: The compiled label for this block.
             * - data {Object}: A reference to the content data object from your property editor model.
             * - settingsData {Object}: A reference to the settings data object from your property editor model.
             * - layout {Object}: A reference to the layout entry from your property editor model.
             * @param {Object} layoutEntry the layout entry object to build the block model from.
             * @return {Object | null} The BlockObject for the given layout entry. Or null if data or configuration wasn't found for this block.
             */
            getBlockObject: function (layoutEntry) {
                var contentUdi = layoutEntry.contentUdi;

                var dataModel = getDataByUdi(contentUdi, this.value.contentData);

                if (dataModel === null) {
                    console.error("Couldn't find content data of " + contentUdi)
                    return null;
                }

                var blockConfiguration = this.getBlockConfiguration(dataModel.contentTypeKey);
                var contentScaffold = null;

                if (blockConfiguration === null) {
                    console.warn("The block of " + contentUdi + " is not being initialized because its contentTypeKey('" + dataModel.contentTypeKey + "') is not allowed for this PropertyEditor");
                } else {
                    contentScaffold = this.getScaffoldFromKey(blockConfiguration.contentElementTypeKey);
                    if (contentScaffold === null) {
                        console.error("The block of " + contentUdi + " is not begin initialized cause its Element Type was not loaded.");
                    }
                }

                if (blockConfiguration === null || contentScaffold === null) {

                    blockConfiguration = {
                        label: "Unsupported",
                        unsupported: true
                    };
                }

                var blockObject = {};
                // Set an angularJS cloneNode method, to avoid this object begin cloned.
                blockObject.cloneNode = function () {
                    return null;// angularJS accept this as a cloned value as long as the
                }
                blockObject.key = String.CreateGuid().replace(/-/g, "");
                blockObject.config = Utilities.copy(blockConfiguration);
                if (blockObject.config.label && blockObject.config.label !== "") {
                  /**
                   * @deprecated use blockObject.label instead
                   */
                  blockObject.labelInterpolator = $interpolate(blockObject.config.label);
                }
                blockObject.__scope = this.isolatedScope;

                // make basics from scaffold
                if(contentScaffold !== null) {// We might not have contentScaffold
                    blockObject.content = Utilities.copy(contentScaffold);
                    ensureUdiAndKey(blockObject.content, contentUdi);

                    mapToElementModel(blockObject.content, dataModel);
                } else {
                    blockObject.content = null;
                }

                blockObject.data = dataModel;
                blockObject.layout = layoutEntry;
                blockObject.__watchers = [];

                if (blockConfiguration.settingsElementTypeKey) {
                    var settingsScaffold = this.getScaffoldFromKey(blockConfiguration.settingsElementTypeKey);
                    if (settingsScaffold !== null) {

                        if (!layoutEntry.settingsUdi) {
                            // if this block does not have settings data, then create it. This could happen because settings model has been added later than this content was created.
                            layoutEntry.settingsUdi = createDataEntry(blockConfiguration.settingsElementTypeKey, this.value.settingsData);
                        }

                        var settingsUdi = layoutEntry.settingsUdi;

                        var settingsData = getDataByUdi(settingsUdi, this.value.settingsData);
                        if (settingsData === null) {
                            console.error("Couldnt find settings data of " + settingsUdi)
                            return null;
                        }

                        // the Settings model has been changed to a new Element Type.
                        // we need to update the settingsData with the new Content Type key
                        if (settingsData.contentTypeKey !== settingsScaffold.contentTypeKey) {
                            settingsData.contentTypeKey = settingsScaffold.contentTypeKey;
                        }

                        blockObject.settingsData = settingsData;

                        // make basics from scaffold
                        if (settingsScaffold !== null) {// We might not have settingsScaffold
                            blockObject.settings = Utilities.copy(settingsScaffold);
                            ensureUdiAndKey(blockObject.settings, settingsUdi);

                            mapToElementModel(blockObject.settings, settingsData);
                        } else {
                            blockObject.settings = null;
                        }

                        // add settings content-app
                        appendSettingsContentApp(blockObject.content, this.__labels.settingsName);
                    }
                }

                blockObject.retrieveValuesFrom = function (content, settings) {
                    if (this.content !== null) {
                        mapElementValues(content, this.content);
                    }
                    if (this.config.settingsElementTypeKey !== null) {
                        mapElementValues(settings, this.settings);
                    }

                };

                blockObject.sync = function () {
                    if (this.content !== null) {
                        mapToPropertyModel(this.content, this.data);
                    }
                    if (this.config.settingsElementTypeKey !== null) {
                        mapToPropertyModel(this.settings, this.settingsData);
                    }
                };

                // first time instant update of label.
                blockObject.label = blockObject.content.contentTypeName;
                blockObject.index = 0;

                if (blockObject.config.label && blockObject.config.label !== "") {
                    var labelElement = $('<div></div>', { text: blockObject.config.label});

                    var observer = new MutationObserver(function(mutations) {
                        mutations.forEach(function(mutation) {
                            blockObject.label = mutation.target.textContent;
                            blockObject.__scope.$evalAsync();
                        });
                    });
    
                    observer.observe(labelElement[0], {characterData: true, subtree:true});

                    blockObject.__watchers.push(() => {
                        observer.disconnect();
                    })

                    blockObject.__labelScope = this.__scopeOfExistence.$new(true);
                    blockObject.__renderLabel = function() {

                        var labelVars = {
                            $contentTypeName: this.content.contentTypeName,
                            $settings: this.settingsData || {},
                            $layout: this.layout || {},
                            $index: this.index + 1,
                            ... this.data
                        };
    
                        this.__labelScope = Object.assign(this.__labelScope, labelVars);
    
                        $compile(labelElement.contents())(this.__labelScope);
                    }.bind(blockObject)
                } else {
                    blockObject.__renderLabel = function() {};
                }

                blockObject.updateLabel = _.debounce(blockObject.__renderLabel, 10);


                // label rendering watchers:
                blockObject.__watchers.push(blockObject.__scope.$watchCollection(function () {
                return blockObject.data;
                }, blockObject.__renderLabel));
                blockObject.__watchers.push(blockObject.__scope.$watchCollection(function () {
                return blockObject.settingsData;
                }, blockObject.__renderLabel));
                blockObject.__watchers.push(blockObject.__scope.$watchCollection(function () {
                return blockObject.layout;
                }, blockObject.__renderLabel));
                blockObject.__watchers.push(blockObject.__scope.$watch(function () {
                return blockObject.index;
                }, blockObject.__renderLabel));


                // Add blockObject to our isolated scope to enable watching its values:
                this.isolatedScope.blockObjects["_" + blockObject.key] = blockObject;
                addWatchers(blockObject, this.isolatedScope);
                addWatchers(blockObject, this.isolatedScope, true);

                blockObject.destroy = function () {
                    // remove property value watchers:
                    this.__watchers.forEach(w => { w(); });
                    delete this.__watchers;

                    // help garbage collector:
                    delete this.config;
                    delete this.layout;
                    delete this.data;
                    delete this.settingsData;
                    delete this.content;
                    delete this.settings;

                    // remove model from isolatedScope.
                    delete this.__scope.blockObjects["_" + this.key];
                    // NOTE: It seems like we should call this.__scope.$destroy(); since that is the only way to remove a scope from it's parent,
                    // however that is not the case since __scope is actually this.isolatedScope which gets cleaned up when the outer scope is
                    // destroyed. If we do that here it breaks the scope chain and validation.
                    delete this.__scope;

                    if(this.__labelScope) {
                        this.__labelScope.$destroy();
                        delete this.__labelScope;
                    }

                    // removes this method, making it impossible to destroy again.
                    delete this.destroy;

                    // lets remove the key to make things blow up if this is still referenced:
                    delete this.key;
                }

                return blockObject;
            },

            /**
             * @ngdoc method
             * @name removeDataAndDestroyModel
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Removes the data and destroys the Block Model.
             * Notice this method does not remove the block from your layout, this will need to be handlede by the Property Editor since this services donst know about your layout structure.
             * @param {Object} blockObject The BlockObject to be removed and destroyed.
             */
            removeDataAndDestroyModel: function (blockObject) {
                var udi = blockObject.layout.contentUdi;
                var settingsUdi = blockObject.layout.settingsUdi || null;
                this.destroyBlockObject(blockObject);
                this.removeDataByUdi(udi);
                if (settingsUdi) {
                    this.removeSettingsByUdi(settingsUdi);
                }
            },

            /**
             * @ngdoc method
             * @name destroyBlockObject
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Destroys the Block Model, but all data is kept.
             * @param {Object} blockObject The BlockObject to be destroyed.
             */
            destroyBlockObject: function (blockObject) {
                blockObject.destroy();
            },

            /**
             * @ngdoc method
             * @name getLayout
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Retrieve the layout object from this specific property editor model.
             * @param {object} defaultStructure if no data exist the layout of your poerty editor will be set to this object.
             * @return {Object} Layout object, structure depends on the model of your property editor.
             */
            getLayout: function (defaultStructure) {
                if (!this.value.layout[this.propertyEditorAlias]) {
                    this.value.layout[this.propertyEditorAlias] = defaultStructure;
                }
                return this.value.layout[this.propertyEditorAlias];
            },

            /**
             * @ngdoc method
             * @name create
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Create a empty layout entry, notice the layout entry is not added to the property editors model layout object, since the layout sturcture depends on the property editor.
             * @param {string} contentElementTypeKey the contentElementTypeKey of the block you wish to create, if contentElementTypeKey is not avaiable in the block configuration then ´null´ will be returned.
             * @return {Object | null} Layout entry object, to be inserted at a decired location in the layout object. Or null if contentElementTypeKey is unavaiaible.
             */
            create: function (contentElementTypeKey) {

                var blockConfiguration = this.getBlockConfiguration(contentElementTypeKey);
                if (blockConfiguration === null) {
                    return null;
                }

                var entry = {
                    contentUdi: createDataEntry(contentElementTypeKey, this.value.contentData)
                }

                if (blockConfiguration.settingsElementTypeKey != null) {
                    entry.settingsUdi = createDataEntry(blockConfiguration.settingsElementTypeKey, this.value.settingsData)
                }

                return entry;
            },

            /**
             * @ngdoc method
             * @name createFromElementType
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Insert data from ElementType Model
             * @return {Object | null} Layout entry object, to be inserted at a decired location in the layout object. Or ´null´ if the given ElementType isnt supported by the block configuration.
             */
            createFromElementType: function (elementTypeDataModel) {

                elementTypeDataModel = clipboardService.parseContentForPaste(elementTypeDataModel, clipboardService.TYPES.ELEMENT_TYPE);

                var contentElementTypeKey = elementTypeDataModel.contentTypeKey;

                var layoutEntry = this.create(contentElementTypeKey);
                if (layoutEntry === null) {
                    return null;
                }

                var dataModel = getDataByUdi(layoutEntry.contentUdi, this.value.contentData);
                if (dataModel === null) {
                    return null;
                }

                mapToPropertyModel(elementTypeDataModel, dataModel);

                return layoutEntry;

            },
            /**
             * @ngdoc method
             * @name createFromBlockData
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Insert data from raw models
             * @return {Object | null} Layout entry object, to be inserted at a decired location in the layout object. Or ´null´ if the given ElementType isnt supported by the block configuration.
             */
            createFromBlockData: function (blockData) {

                blockData = clipboardService.parseContentForPaste(blockData, clipboardService.TYPES.BLOCK);

                // As the blockData is a cloned object we can use its layout part for our layout entry.
                var layoutEntry = blockData.layout;
                if (layoutEntry === null) {
                    return null;
                }

                var blockConfiguration;

                if (blockData.data) {
                    // Ensure that we support the alias:
                    blockConfiguration = this.getBlockConfiguration(blockData.data.contentTypeKey);
                    if(blockConfiguration === null) {
                        return null;
                    }

                    this.value.contentData.push(blockData.data);
                } else {
                    // We do not have data, this cannot be succesful paste.
                    return null;
                }

                if (blockData.settingsData) {
                    // Ensure that we support the alias:
                    if(blockConfiguration.settingsElementTypeKey) {
                        // If we have settings for this Block Configuration, we need to check that they align, if we dont we do not want to fail.
                        if(blockConfiguration.settingsElementTypeKey === blockData.settingsData.contentTypeKey) {
                            this.value.settingsData.push(blockData.settingsData);
                        } else {
                            notificationsService.error("Clipboard", "Couldn't paste because settings-data is not compatible.");
                            return null;
                        }
                    } else {
                        // We do not have settings currently, so lets get rid of the settings part and move on with the paste.
                        delete layoutEntry.settingUdi;
                    }
                }

                return layoutEntry;

            },

            /**
             * @ngdoc method
             * @name sync
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Force immidiate update of the blockobject models to the property model.
             */
            sync: function () {
                for (const key in this.isolatedScope.blockObjects) {
                    this.isolatedScope.blockObjects[key].sync();
                }
            },

            /**
             * @ngdoc method
             * @name removeDataByUdi
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Removes the content data of a given UDI.
             * Notice this method does not remove the block from your layout, this will need to be handled by the Property Editor since this services don't know about your layout structure.
             * @param {string} udi The UDI of the content data to be removed.
             */
            removeDataByUdi: function (udi) {
                const index = this.value.contentData.findIndex(o => o.udi === udi);
                if (index !== -1) {
                    this.value.contentData.splice(index, 1);
                }
            },

            /**
             * @ngdoc method
             * @name removeSettingsByUdi
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Removes the settings data of a given UDI.
             * Notice this method does not remove the settingsUdi from your layout, this will need to be handled by the Property Editor since this services don't know about your layout structure.
             * @param {string} udi The UDI of the settings data to be removed.
             */
            removeSettingsByUdi: function (udi) {
                const index = this.value.settingsData.findIndex(o => o.udi === udi);
                if (index !== -1) {
                    this.value.settingsData.splice(index, 1);
                }
            },

            /**
             * @ngdoc method
             * @name destroy
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Notice you should not need to destroy the BlockEditorModelObject since it will automaticly be destroyed when the scope of existance gets destroyed.
             */
            destroy: function () {

                this.__watchers.forEach(w => { w(); });
                for (const key in this.isolatedScope.blockObjects) {
                    this.destroyBlockObject(this.isolatedScope.blockObjects[key]);
                }

                delete this.__watchers;
                delete this.value;
                delete this.propertyEditorAlias;
                delete this.blockConfigurations;
                delete this.scaffolds;
                this.isolatedScope.$destroy();
                delete this.isolatedScope;
                delete this.__scopeOfExistence;
                delete this.destroy;
            }
        }

        return BlockEditorModelObject;
    }

    angular.module('umbraco.services').service('blockEditorModelObject', blockEditorModelObjectFactory);

})();
