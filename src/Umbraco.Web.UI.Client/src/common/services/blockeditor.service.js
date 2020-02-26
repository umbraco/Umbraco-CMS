(function () {
    'use strict';


    function blockEditorService($interpolate, udiService) {


        function applyModelToScaffold(scaffold, contentModel) {
            
            scaffold.key = contentModel.key;

            var variant = scaffold.variants[0];
            
            for (var t = 0; t < variant.tabs.length; t++) {
                var tab = variant.tabs[t];

                for (var p = 0; p < tab.properties.length; p++) {
                    var prop = tab.properties[p];
                    if (contentModel[prop.propertyAlias]) {
                        prop.value = contentModel[prop.propertyAlias];
                    }
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
            this.value.layout = this.value.layout || [];
            this.value.data = this.value.data || [];

            this.propertyEditorAlias = propertyEditorAlias;
            this.blockConfigurations = blockConfigurations;

            this.scaffolds = [];

        };
        
        BlockEditorModelObject.prototype = {

            getBlockConfiguration: function(alias) {
                return this.blockConfigurations.find(blockConfiguration => blockConfiguration.contentTypeAlias === alias);
            },

            loadScaffolds: function(contentResource) {
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
                        console.log(scaffold);
                        this.scaffolds.push(scaffold);
                    }));
                }));

                return Promise.all(tasks);
            },

            getAvailableBlocksForItemPicker: function() {

                var blocks = [];

                this.blockConfigurations.forEach(blockConfiguration => {

                    var scaffold = this.getScaffoldFor(blockConfiguration.contentTypeAlias);

                    blocks.push({
                        alias: scaffold.contentTypeAlias,
                        name: scaffold.contentTypeName,
                        icon: scaffold.icon
                    });
                });

                return blocks;
            },

            getScaffoldFor: function(contentTypeAlias, data) {
                return this.scaffolds.find(o => o.contentTypeAlias === contentTypeAlias);
            },

            /**
             * Retrieve editing model of a layout entry
             * @return {Object} Scaffolded Block Content object.
             */
            getEditingModel: function(layoutEntry) {

                var contentModel = this.getContentByUdi(layoutEntry.udi);

                var blockConfiguration = this.getBlockConfiguration(contentModel.contentTypeAlias);

                // TODO: make blockConfiguration the base for model, remeber to make a copy.
                var model = {
                    label: "",
                    labelInterpolate: $interpolate(blockConfiguration.label),
                    editor: blockConfiguration.view,
                    overlaySize: "medium" 
                };

                var scaffold = this.getScaffoldFor(blockConfiguration.contentTypeAlias);
                if(scaffold === null) {
                    return null;
                }

                model.content = angular.copy(scaffold);
                applyModelToScaffold(model.content, contentModel);

                // TODO: settings

                return model;

            },

            /**
             * Retrieve layout data
             * @return layout object.
             */
            getLayout: function() {
                if (!this.value.layout[this.propertyEditorAlias]) {
                    this.value.layout[this.propertyEditorAlias] = [];
                }
                return this.value.layout[this.propertyEditorAlias];
            },
            
            /**
             * Create layout entry
             * @param {object} blockConfiguration 
             * @return layout entry, to be added in the layout.
             */
            createLayoutEntry: function(contentTypeAlias) {
                
                var blockConfiguration = this.getBlockConfiguration(contentTypeAlias);

                var entry = {
                    udi: this.createContent(contentTypeAlias)
                }

                if (blockConfiguration.settingsElementTypeAlias != null) {
                    // TODO: Settings.
                }
                
                return entry;
            },

            // You make entries in your layout your self.
            
            getContentByUdi: function(udi) {
                return this.value.data.find(entry => entry.udi === udi);
            },

            createContent: function(elementTypeAlias) {
                var content = {
                    contentTypeAlias: elementTypeAlias,
                    udi: udiService.create("element")
                };
                this.value.data.push(content);
                return content.udi;
            },

            removeContent: function(entry) {
                const index = this.value.data.indexOf(entry)
                if (index > -1) {
                    this.value.splice(index, 1);
                }
            },

            removeContentByUdi: function(udi) {
                const index = this.value.data.findIndex(o => o.udi === udi);
                if (index > -1) {
                    this.value.splice(index, 1);
                }
            }
        }

        return {
            createModelObject: function(propertyModelValue, propertyEditorAlias, blockConfigurations) {
                return new BlockEditorModelObject(propertyModelValue, propertyEditorAlias, blockConfigurations);
            },
            getBlockLabel: function(blockModelObject, labelIndex) {

                console.log("getBlockLabel", blockModelObject);

                // TODO: we should do something about this for performance.
    
                var vars = new Object();
                vars["$index"] = labelIndex;
                
                var variant = blockModelObject.content.variants[0];
                var tab = variant.tabs[0];
                // TODO: need to look up all tabs...
                for(const property of tab.properties) {
                    vars[property.alias] = property.value;
                }
    
                if(blockModelObject.labelInterpolate) {
                    return blockModelObject.labelInterpolate(vars);
                }
    
                return blockModelObject.contentTypeName;
            }
        }
    }

    angular.module('umbraco.services').service('blockEditorService', blockEditorService);

})();
