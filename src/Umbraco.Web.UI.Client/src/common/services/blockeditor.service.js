(function () {
    'use strict';


    function blockEditorService($interpolate, udiService) {


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

            loadScaffolding: function(contentResource) {
                var tasks = [];

                var scaffoldAliases = [];

                this.blockConfigurations.forEach(blockConfiguration => {
                    scaffoldAliases.push(blockConfiguration.contentTypeAlias);
                    if (blockConfiguration.settingsElementTypeAlias != null) {
                        scaffoldAliases.push(elementType.settingsElementTypeAlias);
                    }
                });

                // remove dublicates.
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
             * Retrieve editing model of a layout entry
             * @return {Object} Scaffolded Block Content object.
             */
            getEditingModel: function(layoutEntry) {

                var udi = layoutEntry.udi;

                var contentModel = this.getContentByUdi(udi);

                var blockConfiguration = this.getBlockConfiguration(contentModel.contentTypeAlias);

                if (blockConfiguration === null) {
                    // This is not an allowed block type, therefor we return null;
                    return null;
                }

                var editingModel = {};
                editingModel.config = angular.copy(blockConfiguration);
                editingModel.labelInterpolator = $interpolate(editingModel.config.label);

                var scaffold = this.getScaffoldFor(blockConfiguration.contentTypeAlias);
                if(scaffold === null) {
                    return null;
                }

                // make basics from scaffold
                editingModel.content = angular.copy(scaffold);
                editingModel.content.udi = udi;

                mapToElementTypeModel(editingModel.content, contentModel);

                editingModel.contentModel = contentModel;
                editingModel.layoutModel = layoutEntry;

                // TODO: settings

                return editingModel;

            },


            /**
             * Retrieve editing model of a layout entry
             * @return {Object} Scaffolded Block Content object.
             */
            setDataFromEditingModel: function(editingModel) {

                var udi = editingModel.content.key;

                mapToPropertyModel(editingModel.content, editingModel.contentModel);

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
                    udi: this.createContent(contentTypeAlias)
                }

                if (blockConfiguration.settingsElementTypeAlias != null) {
                    // TODO: Settings.
                }
                
                return entry;
            },

            getContentByUdi: function(udi) {
                return this.value.data.find(entry => entry.udi === udi);
            },
            // private
            createContent: function(elementTypeAlias) {
                var content = {
                    contentTypeAlias: elementTypeAlias,
                    udi: udiService.create("element")
                };
                this.value.data.push(content);
                return content.udi;
            },
            // private
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

                var contentModel = this.getContentByUdi(layoutEntry.udi);

                mapToPropertyModel(elementTypeContentModel, contentModel);

                console.log(elementTypeContentModel)
                console.log(contentModel)

                return layoutEntry;

            }
        }

        return {
            createModelObject: function(propertyModelValue, propertyEditorAlias, blockConfigurations) {
                return new BlockEditorModelObject(propertyModelValue, propertyEditorAlias, blockConfigurations);
            },
            getBlockLabel: function(blockModelObject) {

                // TODO: we should do something about this for performance.
    
                var vars = new Object();
                
                var variant = blockModelObject.content.variants[0];
                var tab = variant.tabs[0];
                // TODO: need to look up all tabs...
                for(const property of tab.properties) {
                    vars[property.alias] = property.value;
                }
    
                if(blockModelObject.labelInterpolator) {
                    return blockModelObject.labelInterpolator(vars);
                }
    
                return blockModelObject.contentTypeName;
            }
        }
    }

    angular.module('umbraco.services').service('blockEditorService', blockEditorService);

})();
