(function () {
    'use strict';


    function blockEditorService($interpolate, udiService, contentResource) {


        /**
         * Simple mapping from property model content entry to editing model,
         * needs to stay simple to avoid deep watching.
         */
        function mapToElementModel(elementModel, dataModel) {

            var variant = elementModel.variants[0];
            
            for (var t = 0; t < variant.tabs.length; t++) {
                var tab = variant.tabs[t];

                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];
                    if (dataModel[prop.alias]) {
                        prop.value = dataModel[prop.alias];
                    }
                }
            }
        }

        /**
         * Simple mapping from elementModel to property model content entry,
         * needs to stay simple to avoid deep watching.
         */
        function mapToPropertyModel(elementModel, dataModel) {
            
            var variant = elementModel.variants[0];
            
            for (var t = 0; t < variant.tabs.length; t++) {
                var tab = variant.tabs[t];

                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];
                    if (prop.value) {
                        dataModel[prop.alias] = prop.value;
                    }
                }
            }
        }

        function mapValueToPropertyModel(value, alias, dataModel) {
            dataModel[alias] = value;
        }

        /**
         * Map property values from an ElementModel to another ElementModel.
         * Used to tricker watchers for synchronization.
         * @param {Object} fromModel ElementModel to recive property values from.
         * @param {Object} toModel ElementModel to recive property values from.
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
                var toTab = toVariant.tabs[t];

                for (var p = 0; p < fromTab.properties.length; p++) {
                    var fromProp = fromTab.properties[p];
                    var toProp = toTab.properties[p];
                    toProp.value = fromProp.value;
                }
            }
        }


        function getBlockLabel(blockModel) {
            if(blockModel.labelInterpolator !== undefined) {
                // We are just using the data model, since its a key/value object that is live synced. (if we need to add additional vars, we could make a shallow copy and apply those.)
                return blockModel.labelInterpolator(blockModel.data);
            }
            return blockModel.content.contentTypeName;
        }


        /**
         * Used to add watchers on all properties in a content or settings model
         */
        function addWatchers(blockModel, isolatedScope, forSettings) {
            var model = forSettings ? blockModel.settings : blockModel.content;
            if (!model || !model.variants || !model.variants.length) { return; }

            // Start watching each property value.
            var variant = model.variants[0];
            var field = forSettings ? "settings" : "content";
            var watcherCreator = forSettings ? createSettingsModelPropWatcher : createDataModelPropWatcher;
            for (var t = 0; t < variant.tabs.length; t++) {
                var tab = variant.tabs[t];
                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];

                    // Watch value of property since this is the only value we want to keep synced.
                    // Do notice that it is not performing a deep watch, meaning that we are only watching primatives and changes directly to the object of property-value.
                    // But we like to sync non-primative values as well! Yes, and this does happen, just not through this code, but through the nature of JavaScript. 
                    // Non-primative values act as references to the same data and are therefor synced.
                    blockModel.watchers.push(isolatedScope.$watch("blockModels._" + blockModel.key + "." + field + ".variants[0].tabs[" + t + "].properties[" + p + "].value", watcherCreator(blockModel, prop)));
                }
            }
            if (blockModel.watchers.length === 0) {
                // If no watcher where created, it means we have no properties to watch. This means that nothing will activate our generate the label, since its only triggered by watchers.
                blockModel.updateLabel();
            }
        }

        /**
         * Used to create a scoped watcher for a content property on a blockModel.
         */
        function createDataModelPropWatcher(blockModel, prop)  {
            return function() {
                // sync data:
                blockModel.data[prop.alias] = prop.value;

                blockModel.updateLabel();
            }
        }

        /**
         * Used to create a scoped watcher for a settings property on a blockModel.
         */
        function createSettingsModelPropWatcher(blockModel, prop)  {
            return function() {
                // sync data:
                blockModel.layout.settings[prop.alias] = prop.value;
            }
        }


        /**
         * Used to highlight unsupported properties for the user, changes unsupported properties into a unsupported-property.
         */
        var notSupportedProperties = [
            "Umbraco.Tags",
            "Umbraco.UploadField",
            "Umbraco.ImageCropper"
        ];
        function replaceUnsupportedProperties(scaffold) {
            scaffold.variants.forEach((variant) => {
                variant.tabs.forEach((tab) => {
                    tab.properties.forEach((property) => {
                        if (notSupportedProperties.indexOf(property.editor) !== -1) {
                            property.view = "notsupported";
                        }
                    });
                });
            });
            return scaffold;
        }


        /**
        * @ngdoc factory
        * @name umbraco.factory.BlockEditorModelObject
        * @description A model object used to handle Block Editor data.
        **/
        function BlockEditorModelObject(propertyModelValue, propertyEditorAlias, blockConfigurations, propertyScope) {

            if (!propertyModelValue) {
                throw new Error("propertyModelValue cannot be undefined, to ensure we keep the binding to the angular model we need minimum an empty object.");
            }

            // ensure basic part of data-structure is in place:
            this.value = propertyModelValue;
            this.value.layout = this.value.layout || {};
            this.value.data = this.value.data || [];

            this.propertyEditorAlias = propertyEditorAlias;
            this.blockConfigurations = blockConfigurations;

            this.scaffolds = [];

            this.watchers = [];

            this.isolatedScope = propertyScope.$new(true);
            this.isolatedScope.blockModels = {};
            
            this.isolatedScope.$on("$destroy", this.onDestroyed.bind(this));

        };
        
        BlockEditorModelObject.prototype = {

            getBlockConfiguration: function(alias) {
                return this.blockConfigurations.find(bc => bc.contentTypeAlias === alias);
            },

            loadScaffolding: function() {
                var tasks = [];

                var scaffoldAliases = [];

                this.blockConfigurations.forEach(blockConfiguration => {
                    scaffoldAliases.push(blockConfiguration.contentTypeAlias);
                    if (blockConfiguration.settingsElementTypeAlias != null) {
                        scaffoldAliases.push(blockConfiguration.settingsElementTypeAlias);
                    }
                });

                // removing dublicates.
                scaffoldAliases = scaffoldAliases.filter((value, index, self) => self.indexOf(value) === index);

                scaffoldAliases.forEach((elementTypeAlias => {
                    tasks.push(contentResource.getScaffold(-20, elementTypeAlias).then(scaffold => {
                        this.scaffolds.push(replaceUnsupportedProperties(scaffold));
                    }));
                }));

                return Promise.all(tasks);
            },

            /**
             * Retrive a list of aliases that are available for content of blocks in this property editor, does not contain aliases of block settings.
             * @return {Array} array of strings representing alias.
             */
            getAvailableAliasesForBlockContent: function() {
                return this.blockConfigurations.map(blockConfiguration => blockConfiguration.contentTypeAlias);
            },

            getAvailableBlocksForBlockPicker: function() {

                var blocks = [];

                this.blockConfigurations.forEach(blockConfiguration => {
                    var scaffold = this.getScaffoldFor(blockConfiguration.contentTypeAlias);
                    if(scaffold) {
                        blocks.push({
                            blockConfigModel: blockConfiguration,
                            elementTypeModel: scaffold.documentType
                        });
                    }
                });

                return blocks;
            },

            getScaffoldFor: function(contentTypeAlias) {
                return this.scaffolds.find(o => o.contentTypeAlias === contentTypeAlias);
            },

            /**
             * Retrieve editor friendly model of a block.
             * @param {Object} layoutEntry the layout entry to build the block model from.
             * @return {Object} Scaffolded Block Content object.
             */
            getBlockModel: function(layoutEntry) {

                var udi = layoutEntry.udi;

                var dataModel = this._getDataByUdi(udi);

                if (dataModel === null) {
                    console.error("Couldnt find content model of "+udi)
                    return null;
                }

                var blockConfiguration = this.getBlockConfiguration(dataModel.contentTypeAlias);

                if (blockConfiguration === null) {
                    console.error("The block entry of "+udi+" is not begin initialized cause its contentTypeAlias is not allowed for this PropertyEditor")
                    // This is not an allowed block type, therefor we return null;
                    return null;
                }

                var blockModel = {};
                blockModel.key = String.CreateGuid().replace(/-/g, "");
                blockModel.config = angular.copy(blockConfiguration);
                if (blockModel.config.label && blockModel.config.label !== "") {
                    blockModel.labelInterpolator = $interpolate(blockModel.config.label);
                }
                blockModel.updateLabel = _.debounce(function() {
                    this.label = getBlockLabel(this);
                }, 100);

                var contentScaffold = this.getScaffoldFor(blockConfiguration.contentTypeAlias);
                if(contentScaffold === null) {
                    return null;
                }

                // make basics from scaffold
                blockModel.content = angular.copy(contentScaffold);
                blockModel.content.udi = udi;

                mapToElementModel(blockModel.content, dataModel);

                blockModel.data = dataModel;
                blockModel.layout = layoutEntry;
                blockModel.watchers = [];

                if (blockConfiguration.settingsElementTypeAlias) {
                    var settingsScaffold = this.getScaffoldFor(blockConfiguration.settingsElementTypeAlias);
                    if (settingsScaffold === null) {
                        return null;
                    }

                    // make basics from scaffold
                    blockModel.settings = angular.copy(settingsScaffold);
                    layoutEntry.settings = layoutEntry.settings || { key: String.CreateGuid(), contentTypeAlias: blockConfiguration.settingsElementTypeAlias };
                    if (!layoutEntry.settings.key) { layoutEntry.settings.key = String.CreateGuid(); }
                    if (!layoutEntry.settings.contentTypeAlias) { layoutEntry.settings.contentTypeAlias = blockConfiguration.settingsElementTypeAlias; }
                    mapToElementModel(blockModel.settings, layoutEntry.settings);
                } else {
                    layoutEntry.settings = null;
                }

                // Add blockModel to our isolated scope to enable watching its values:
                this.isolatedScope.blockModels["_"+blockModel.key] = blockModel;
                addWatchers(blockModel, this.isolatedScope);
                addWatchers(blockModel, this.isolatedScope, true);

                return blockModel;

            },

            removeDataAndDestroyModel: function (blockModel) {
                this.destroyBlockModel(blockModel);
                this.removeDataByUdi(blockModel.content.udi);
            },

            destroyBlockModel: function(blockModel) {

                // remove property value watchers:
                for (const w of blockModel.watchers) {
                    w();
                }
                
                // remove model from isolatedScope.
                delete this.isolatedScope.blockModels[blockModel.key];

            },


            /**
             * Retrieve block model of a layout entry
             * @return {Object} Scaffolded Block Content object.
             */
            setDataFromBlockModel: function(blockModel) {

                var udi = blockModel.content.key;

                mapToPropertyModel(blockModel.content, blockModel.data);

                // TODO: implement settings, sync settings to layout entry.
                // mapToPropertyModel(blockModel.settings, blockModel.layout.settings)

            },

            /**
             * Retrieve the layout object for this specific property editor.
             * @return {Object} Layout object.
             */
            getLayout: function() {
                if (!this.value.layout[this.propertyEditorAlias]) {
                    this.value.layout[this.propertyEditorAlias] = [];
                }
                return this.value.layout[this.propertyEditorAlias];
            },
            
            /**
             * Create a empty layout entry
             * @param {Object} blockConfiguration
             * @return {Object} Layout entry object, to be inserted at a decired location in the layout object.
             */
            create: function(contentTypeAlias) {
                
                var blockConfiguration = this.getBlockConfiguration(contentTypeAlias);
                if(blockConfiguration === null) {
                    return null;
                }

                var entry = {
                    udi: this._createDataEntry(contentTypeAlias)
                }

                if (blockConfiguration.settingsElementTypeAlias != null) {
                    entry.settings = { key: String.CreateGuid(), contentTypeAlias: blockConfiguration.settingsElementTypeAlias };
                }
                
                return entry;
            },

            /**
             * Insert data from ElementType Model
             * @return {Object} Layout entry object, to be inserted at a decired location in the layout object.
             */
            createFromElementType: function(elementTypeDataModel) {

                elementTypeDataModel = angular.copy(elementTypeDataModel);

                var contentTypeAlias = elementTypeDataModel.contentTypeAlias;

                var layoutEntry = this.create(contentTypeAlias);
                if(layoutEntry === null) {
                    return null;
                }

                var dataModel = this._getDataByUdi(layoutEntry.udi);
                if(dataModel === null) {
                    return null;
                }

                mapToPropertyModel(elementTypeDataModel, dataModel);

                return layoutEntry;

            },

            // private
            _createDataEntry: function(elementTypeAlias) {
                var content = {
                    contentTypeAlias: elementTypeAlias,
                    udi: udiService.create("element")
                };
                this.value.data.push(content);
                return content.udi;
            },
            // private
            _getDataByUdi: function(udi) {
                return this.value.data.find(entry => entry.udi === udi) || null;
            },

            removeDataByUdi: function(udi) {
                const index = this.value.data.findIndex(o => o.udi === udi);
                if (index !== -1) {
                    this.value.data.splice(index, 1);
                }
            },

            onDestroyed: function() {

                for (const key in this.isolatedScope.blockModels) {
                    this.destroyBlockModel(this.isolatedScope.blockModels[key]);
                }
                
                delete this.value;
                delete this.propertyEditorAlias;
                delete this.blockConfigurations;
                delete this.scaffolds;
                delete this.watchers;
                delete this.isolatedScope;
            }
        }

        return {
            createModelObject: function(propertyModelValue, propertyEditorAlias, blockConfigurations, propertyScope) {
                return new BlockEditorModelObject(propertyModelValue, propertyEditorAlias, blockConfigurations, propertyScope);
            },
            mapElementValues: mapElementValues, 
            getBlockLabel: getBlockLabel
        }
    }

    angular.module('umbraco.services').service('blockEditorService', blockEditorService);

})();
