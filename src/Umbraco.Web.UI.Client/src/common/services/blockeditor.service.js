(function () {
    'use strict';


    function blockEditorService($interpolate, udiService, contentResource) {


        /**
         * Simple mapping from property model content entry to editing model,
         * needs to stay simple to avoid deep watching.
         */
        function mapToElementTypeModel(elementTypeModel, contentModel) {

            var variant = elementTypeModel.variants[0];
            
            for (var t = 0; t < variant.tabs.length; t++) {
                var tab = variant.tabs[t];

                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];
                    if (contentModel[prop.alias]) {
                        prop.value = contentModel[prop.alias];
                    }
                }
            }
        }

        /**
         * Simple mapping from elementTypeModel to property model content entry,
         * needs to stay simple to avoid deep watching.
         */
        function mapToPropertyModel(elementTypeModel, contentModel) {
            
            var variant = elementTypeModel.variants[0];
            
            for (var t = 0; t < variant.tabs.length; t++) {
                var tab = variant.tabs[t];

                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];
                    if (prop.value) {
                        contentModel[prop.alias] = prop.value;
                    }
                }
            }
        }

        function mapValueToPropertyModel(value, alias, contentModel) {
            contentModel[alias] = value;
        }

        /**
         * Map property values from an ElementTypeModel to another ElementTypeModel.
         * Used to tricker watchers for synchronization.
         * @param {Object} fromModel ElementTypeModel to recive property values from.
         * @param {Object} toModel ElementTypeModel to recive property values from.
         */
        function mapElementTypeValues(fromModel, toModel) {
            var fromVariant = fromModel.variants[0];
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


        /**
        * @ngdoc factory
        * @name umbraco.factory.BlockEditorModelObject
        * @description A model object used to handle Block Editor data.
        **/
        function BlockEditorModelObject(propertyModelValue, propertyEditorAlias, blockConfigurations) {

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
                        scaffoldAliases.push(elementType.settingsElementTypeAlias);
                    }
                });

                // removing dublicates.
                scaffoldAliases = scaffoldAliases.filter((value, index, self) => self.indexOf(value) === index);

                scaffoldAliases.forEach((elementTypeAlias => {
                    tasks.push(contentResource.getScaffold(-20, elementTypeAlias).then(scaffold => {
                        this.scaffolds.push(scaffold);
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

            getAvailableBlocksForItemPicker: function() {

                var blocks = [];

                this.blockConfigurations.forEach(blockConfiguration => {
                    var scaffold = this.getScaffoldFor(blockConfiguration.contentTypeAlias);
                    if(scaffold) {
                        blocks.push({
                            alias: scaffold.contentTypeAlias,
                            name: scaffold.contentTypeName,
                            icon: scaffold.icon
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

                var contentModel = this._getContentByUdi(udi);

                var blockConfiguration = this.getBlockConfiguration(contentModel.contentTypeAlias);

                if (blockConfiguration === null) {
                    // This is not an allowed block type, therefor we return null;
                    return null;
                }

                var blockModel = {};
                blockModel.key = String.CreateGuid();
                blockModel.config = angular.copy(blockConfiguration);
                blockModel.labelInterpolator = $interpolate(blockModel.config.label);

                var scaffold = this.getScaffoldFor(blockConfiguration.contentTypeAlias);
                if(scaffold === null) {
                    return null;
                }

                // make basics from scaffold
                blockModel.content = angular.copy(scaffold);
                blockModel.content.udi = udi;

                mapToElementTypeModel(blockModel.content, contentModel);

                blockModel.contentModel = contentModel;
                blockModel.layoutModel = layoutEntry;

                // TODO: settings

                return blockModel;

            },


            /**
             * Retrieve block model of a layout entry
             * @return {Object} Scaffolded Block Content object.
             */
            setDataFromBlockModel: function(blockModel) {

                var udi = blockModel.content.key;

                mapToPropertyModel(blockModel.content, blockModel.contentModel);

                // TODO: sync settings to layout entry.

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
                    udi: this._createContent(contentTypeAlias)
                }

                if (blockConfiguration.settingsElementTypeAlias != null) {
                    // TODO: Settings.
                }
                
                return entry;
            },

            /**
             * Insert data from ElementType Model
             * @return {Object} Layout entry object, to be inserted at a decired location in the layout object.
             */
            createFromElementType: function(elementTypeContentModel) {

                elementTypeContentModel = angular.copy(elementTypeContentModel);

                var contentTypeAlias = elementTypeContentModel.contentTypeAlias;

                var layoutEntry = this.create(contentTypeAlias);
                if(layoutEntry === null) {
                    return null;
                }

                var contentModel = this._getContentByUdi(layoutEntry.udi);

                mapToPropertyModel(elementTypeContentModel, contentModel);

                return layoutEntry;

            },

            // private
            _createContent: function(elementTypeAlias) {
                var content = {
                    contentTypeAlias: elementTypeAlias,
                    udi: udiService.create("element")
                };
                this.value.data.push(content);
                return content.udi;
            },
            // private
            _getContentByUdi: function(udi) {
                return this.value.data.find(entry => entry.udi === udi);
            },
            
            removeContent: function(entry) {
                const index = this.value.data.indexOf(entry)
                if (index !== -1) {
                    this.value.splice(index, 1);
                }
            },

            removeContentByUdi: function(udi) {
                const index = this.value.data.findIndex(o => o.udi === udi);
                if (index !== -1) {
                    this.value.splice(index, 1);
                }
            }
        }

        return {
            createModelObject: function(propertyModelValue, propertyEditorAlias, blockConfigurations) {
                return new BlockEditorModelObject(propertyModelValue, propertyEditorAlias, blockConfigurations);
            },
            mapElementTypeValues: mapElementTypeValues, 
            getBlockLabel: function(blockModel) {
    
                
                if(blockModel.labelInterpolator) {
                    // We are just using the contentModel, since its a key/value object that is live synced. (if we need to add additional vars, we could make a shallow copy and apply those.)
                    return blockModel.labelInterpolator(blockModel.contentModel);
                }
    
                return blockModel.contentTypeName;
            }
        }
    }

    angular.module('umbraco.services').service('blockEditorService', blockEditorService);

})();
