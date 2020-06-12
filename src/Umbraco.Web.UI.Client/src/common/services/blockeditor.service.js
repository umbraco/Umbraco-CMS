/**
 @ngdoc service
 * @name umbraco.services.blockEditorService
 *
 * @description
 * Block Editor Service provides the basic features for a block editor.
 * The main feature is the ability to create a Model Object which takes care of your data for your Block Editor.
 * 
 * 
 * ##Samples
 *
 * ####Instantiate a Model Object for your property editor:
 * 
 * <pre>
 *     modelObject = blockEditorService.createModelObject(vm.model.value, vm.model.editor, vm.model.config.blocks, $scope);
 *     modelObject.loadScaffolding().then(onLoaded);
 * </pre>
 *
 * ####Use the Model Object to retrive Content Models for the blocks you want to edit. Content Models contains all the data about the properties of that content and as well handles syncroniztion so your data is always up-to-date.
 *
 * <pre>
 *     // Store a reference to the layout model, because we need to maintain this model.
 *     var layout = modelObject.getLayout();
 * 
 *     // maps layout entries to editor friendly models aka. BlockModels.
 *     layout.forEach(entry => {
 *         var block = modelObject.getBlockModel(entry);
 *         // If this entry was not supported by our property-editor it would return 'null'.
 *         if(block !== null) {
 *             // store this block in an array we use in our view.
 *             vm.myViewModelOfBlocks.push(block);
 *         }
 *     });
 *
 * </pre>
 * 
 * ####Use the Model Object to retrive Content Models for the blocks you want to edit. Content Models contains all the data about the properties of that content and as well handles syncroniztion so your data is always up-to-date.
 * 
 * <pre>
 * function addNewBlock(index, contentTypeKey) {
 * 
 *     // Create layout entry. (not added to property model jet.)
 *     var layoutEntry = modelObject.create(contentTypeKey);
 *     if (layoutEntry === null) {
 *         return false;
 *     }
 * 
 *     // make block model
 *     var blockModel = getBlockModel(layoutEntry);
 *     if (blockModel === null) {
 *         return false;
 *     }
 *     
 *     // If we reach this line, we are good to add the layoutEntry and blockModel to layout model and view model.
 *     // add layout entry at the decired location in layout, depending on your layout structure.
 *     layout.splice(index, 0, layoutEntry);
 * 
 *     // apply block model at decired location in our model used for the view.
 *     vm.myViewModelOfBlocks.splice(index, 0, blockModel);
 *     
 *     // at this stage we know everything went well.
 *     return true;
 * }
 * </pre>
 * 
 */
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

        
        /**
         * Generate label for Block, uses either the labelInterpolator or falls back to the contentTypeName.
         * @param {Object} blockModel BlockModel to recive data values from.
         */
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

            /**
             * Get block configuration object for a given contentTypeKey.
             * @param {string} key contentTypeKey to recive the configuration model for.
             * @returns {Object | null} Configuration model for the that specific block. Or ´null´ if the contentTypeKey isnt available in the current block configurations.
             */
            getBlockConfiguration: function(key) {
                return this.blockConfigurations.find(bc => bc.contentTypeKey === key);
            },

            /**
             * Load the scaffolding models for the given configuration, these are needed to provide usefull models for each block.
             * @param {Object} blockModel BlockModel to recive data values from.
             * @returns {Promise} A Promise object which resolves when all scaffold models are loaded.
             */
            loadScaffolding: function() {
                var tasks = [];

                var scaffoldKeys = [];

                this.blockConfigurations.forEach(blockConfiguration => {
                    scaffoldKeys.push(blockConfiguration.contentTypeKey);
                    if (blockConfiguration.settingsElementTypeKey != null) {
                        scaffoldKeys.push(blockConfiguration.settingsElementTypeKey);
                    }
                });

                // removing dublicates.
                scaffoldKeys = scaffoldKeys.filter((value, index, self) => self.indexOf(value) === index);

                scaffoldKeys.forEach((contentTypeKey => {
                    tasks.push(contentResource.getScaffoldByKey(-20, contentTypeKey).then(scaffold => {
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
                return this.blockConfigurations.map(blockConfiguration => this.getScaffoldFor(blockConfiguration.contentTypeKey).contentTypeKey);
            },

            /**
             * Retrive a list of available blocks, the list containing object with the confirugation model(blockConfigModel) and the element type model(elementTypeModel).
             * The purpose of this data is to provide it for the Block Picker.
             * @return {Array} array of objects representing available blocks, each object containing properties blockConfigModel and elementTypeModel.
             */
            getAvailableBlocksForBlockPicker: function() {

                var blocks = [];

                this.blockConfigurations.forEach(blockConfiguration => {
                    var scaffold = this.getScaffoldFor(blockConfiguration.contentTypeKey);
                    if(scaffold) {
                        blocks.push({
                            blockConfigModel: blockConfiguration,
                            elementTypeModel: scaffold.documentType
                        });
                    }
                });

                return blocks;
            },

            /**
             * Get scaffold model for a given contentTypeKey.
             * @param {string} key contentTypeKey to recive the scaffold model for.
             * @returns {Object | null} Scaffold model for the that content type. Or null if the scaffolding model dosnt exist in this context.
             */
            getScaffoldFor: function(contentTypeKey) {
                return this.scaffolds.find(o => o.contentTypeKey === contentTypeKey);
            },

            /**
             * Retrieve editor friendly model of a block.
             * BlockModel is a class instance which setups live syncronization of content and settings models back to the data of your property editor model.
             * The returned object, named ´BlockModel´, contains several usefull models to make editing of this block happen.
             * The ´BlockModel´ contains the following properties:
             * - key {string}: runtime generated key, usefull for tracking of this object
             * - content {Object}: Content model, the content type model for content merged with the content data of this block.
             * - settings {Object}: Settings model, the content type model for settings merged with the settings data of this block.
             * - config {Object}: A deep copy of the block configuration model.
             * - label {string}: The label for this block.
             * - updateLabel {Method}: Method to trigger an update of the label for this block.
             * - data {Object}: A reference to the data object from your property editor model.
             * - layout {Object}: A refernce to the layout entry from your property editor model.
             * @param {Object} layoutEntry the layout entry object to build the block model from.
             * @return {Object | null} The BlockModel for the given layout entry. Or null if data or configuration wasnt found for this block.
             */
            getBlockModel: function(layoutEntry) {

                var udi = layoutEntry.udi;

                var dataModel = this._getDataByUdi(udi);

                if (dataModel === null) {
                    console.error("Couldnt find content model of " + udi)
                    return null;
                }

                var blockConfiguration = this.getBlockConfiguration(dataModel.contentTypeKey);

                if (blockConfiguration === null) {
                    console.error("The block entry of "+udi+" is not begin initialized cause its contentTypeKey is not allowed for this PropertyEditor")
                    // This is not an allowed block type, therefor we return null;
                    return null;
                }

                var contentScaffold = this.getScaffoldFor(blockConfiguration.contentTypeKey);
                if(contentScaffold === null) {
                    return null;
                }

                var blockModel = {};
                blockModel.key = String.CreateGuid().replace(/-/g, "");
                blockModel.config = Utilities.copy(blockConfiguration);
                if (blockModel.config.label && blockModel.config.label !== "") {
                    blockModel.labelInterpolator = $interpolate(blockModel.config.label);
                }
                blockModel.__scope = this.isolatedScope;
                blockModel.updateLabel = _.debounce(function () {this.__scope.$evalAsync(function() {
                    this.label = getBlockLabel(this);
                }.bind(this))}.bind(blockModel), 10);

                // make basics from scaffold
                blockModel.content = Utilities.copy(contentScaffold);
                blockModel.content.udi = udi;

                mapToElementModel(blockModel.content, dataModel);

                blockModel.data = dataModel;
                blockModel.layout = layoutEntry;
                blockModel.watchers = [];

                if (blockConfiguration.settingsElementTypeKey) {
                    var settingsScaffold = this.getScaffoldFor(blockConfiguration.settingsElementTypeKey);
                    if (settingsScaffold === null) {
                        return null;
                    }

                    // make basics from scaffold
                    blockModel.settings = Utilities.copy(settingsScaffold);
                    layoutEntry.settings = layoutEntry.settings || {};
                    if (!layoutEntry.settings.key) { layoutEntry.settings.key = String.CreateGuid(); }
                    if (!layoutEntry.settings.contentTypeKey) { layoutEntry.settings.contentTypeKey = blockConfiguration.settingsElementTypeKey; }
                    mapToElementModel(blockModel.settings, layoutEntry.settings);
                } else {
                    layoutEntry.settings = null;
                }

                blockModel.retriveValuesFrom = function(content, settings) {
                    if (this.content !== null) {
                        mapElementValues(content, this.content);
                    }
                    if (this.config.settingsElementTypeKey !== null) {
                        mapElementValues(settings, this.settings);
                    }
                }

                // Add blockModel to our isolated scope to enable watching its values:
                this.isolatedScope.blockModels["_"+blockModel.key] = blockModel;
                addWatchers(blockModel, this.isolatedScope);
                addWatchers(blockModel, this.isolatedScope, true);

                return blockModel;

            },

            /**
             * Removes the data and destroys the Block Model.
             * Notive this method does not remove the block from your layout, this will need to be handlede by the Property Editor since this services donst know about your layout structure.
             * @param {Object} blockModel The BlockModel to be removed and destroyed.
             */
            removeDataAndDestroyModel: function (blockModel) {
                this.destroyBlockModel(blockModel);
                this.removeDataByUdi(blockModel.content.udi);
            },

            /**
             * Destroys the Block Model, but all data is kept.
             * @param {Object} blockModel The BlockModel to be destroyed.
             */
            destroyBlockModel: function(blockModel) {

                // remove property value watchers:
                blockModel.watchers.forEach(w => { w(); });
                
                // remove model from isolatedScope.
                delete this.isolatedScope.blockModels[blockModel.key];

            },

            /**
             * Retrieve the layout object from this specific property editor model.
             * @return {Object} Layout object, structure depends on the model of your property editor.
             */
            getLayout: function() {
                if (!this.value.layout[this.propertyEditorAlias]) {
                    this.value.layout[this.propertyEditorAlias] = [];
                }
                return this.value.layout[this.propertyEditorAlias];
            },
            
            /**
             * Create a empty layout entry, notice the layout entry is not added to the property editors model layout object, since the layout sturcture depends on the property editor.
             * @param {string} contentTypeKey, the contentTypeKey of the block you wish to create, if contentTypeKey is not avaiable in the block configuration then ´null´ will be returned.
             * @return {Object | null} Layout entry object, to be inserted at a decired location in the layout object. Or null if contentTypeKey is unavaiaible.
             */
            create: function(contentTypeKey) {
                
                var blockConfiguration = this.getBlockConfiguration(contentTypeKey);
                if(blockConfiguration === null) {
                    return null;
                }

                var entry = {
                    udi: this._createDataEntry(contentTypeKey)
                }

                if (blockConfiguration.settingsElementTypeKey != null) {
                    entry.settings = { key: String.CreateGuid(), contentTypeKey: blockConfiguration.settingsElementTypeKey };
                }
                
                return entry;
            },

            /**
             * Insert data from ElementType Model
             * @return {Object | null} Layout entry object, to be inserted at a decired location in the layout object. Or ´null´ if the given ElementType isnt supported by the block configuration.
             */
            createFromElementType: function(elementTypeDataModel) {

                elementTypeDataModel = Utilities.copy(elementTypeDataModel);

                var contentTypeKey = elementTypeDataModel.contentTypeKey;

                var layoutEntry = this.create(contentTypeKey);
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
            _createDataEntry: function(elementTypeKey) {
                var content = {
                    contentTypeKey: elementTypeKey,
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
                this.isolatedScope.$destroy();
                delete this.isolatedScope;
            }
        }

        return {
            /**
             * Create a new Block Editor Model Object, used to deal with editing of the Block Editor Model.
             * @return {BlockEditorModelObject} A instance of the BlockEditorModelObject class.
             */
            createModelObject: function(propertyModelValue, propertyEditorAlias, blockConfigurations, propertyScope) {
                return new BlockEditorModelObject(propertyModelValue, propertyEditorAlias, blockConfigurations, propertyScope);
            }
        }
    }

    angular.module('umbraco.services').service('blockEditorService', blockEditorService);

})();
