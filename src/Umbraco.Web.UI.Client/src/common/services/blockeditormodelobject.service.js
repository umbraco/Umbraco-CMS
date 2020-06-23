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
 * ## <b>Basic knowledge for understanding how to work with Block Editor data.</b>
 * There is a few things we need to understand before we can use the Model Object(BlockEditorModelObject).
 * The data structure of a Block Editor contains two main properties 'layout' and 'data'.
 * - The 'layout' property is the origin of the data, this object defines the blocks of this property including the the order and layout of those.
 * - The 'data' property is the data of your blocks and is managed by the Model Object therefor it can be ignored for most use.
 * 
 * To get a better understanding of what this means as a user of the Model Object, we need to look at some simple usages of the Model Object:
 * 
 * ## <b>Maintain and work with the Layout of a Block Editor.</b>
 * The 'layout' of a Block Editor can be of any structure. Therefor the Model Object(BlockEditorModelObject) cannot maintain this data.
 * Since the origin of blocks is in the 'layout' the Model Object only can serve as a helper to maintain and create data.
 * Therefor the Property Editor code will be using the 'layout' as origin, using the Model Object help managing speicfic parts.<br/>
 * To give an unstanding of what that means please read the following documentation of how to create a block.
 * 
 * ## <b>The basic setup for a Block Editor.</b>
 * ####Instantiate a Model Object and load dependencies. And provide the basic structure for the 'layout' property when reciving the reference to it:
 * 
 * <pre>
 * 
 *     // We must get a scope that exists in all the lifetime of this data. Across variants and split-view.
 *     var scopeOfExistence = $scope;
 *     // Setup your component to require umbVariantContentEditors and use the method getScope to retrive a shared scope for multiple editors of this content.
 *     if(vm.umbVariantContentEditors && vm.umbVariantContentEditors.getScope) {
 *         scopeOfExistence = vm.umbVariantContentEditors.getScope();
 *     }
 * 
 *     // Define variables for layout and modelObject as you will be using these through our your property-editor.
 *     var layout;
 *     var modelObject;
 *     
 *     // When we are ready we can instantiate the Model Object can load the dependencies of it.
 *     vm.$onInit = function() {
 *         modelObject = blockEditorService.createModelObject(vm.model.value, vm.model.editor, vm.model.config.blocks, scopeOfExistence);
 *         modelObject.load().then(onLoaded);
 *     }
 * 
 *     function onLoaded() {
 *
 *          // Define the default layout, this is used when there is no data jet stored for this property.
 *          var defaultLayout = [];
 * 
 *          // We store a reference to layout as we have to maintain this.
 *          layout = modelObject.getLayout(defaultLayout);
 * 
 *      }
 * </pre>
 * 
 * ## <b>Create a Block.</b>
 * Use the Model Object to create a Block and append the returned layout-entry to the 'layout'.
 * 
 * #### In the following example we will create a new block and append it at the decidered location in the 'layout' object:
 * 
 * <pre>
 *     // continuing from previous example.
 *     
 *     // Creates a block and returns a layout entry. The layout entry is not part of layout jet as its not managed by the Model Object.
 *     var layoutEntry = modelObject.create(contentTypeKey);
 *     if (layoutEntry === null) {
 *         // The creation was not successful, therefore exit and without appending anything to our 'layout' object.
 *         return false;
 *     }
 * 
 *     // If we reach this line, we are good to add the layoutEntry to layout model.
 *     // In this example our layout is an array and we would like to append the new block as the last entry.
 *     layout.push(layoutEntry);
 *     
 * </pre>
 *
 * ## <b>Working with Blocks</b>
 * 
 * The layout-entries does not provide much value when it comes to displaying or editing Blocks.
 * Our Model Object provides the option to get a Block Object for a given Block. Retrived by parsing the layout-entry of the block we would like.
 * The Block Object provides data of interest, the most important of these properties are: Block configuration, A label and the Block content in the Element Type Data Model format, this Content-model is very usefull to make UI for editing the Content of a Block.
 * 
 * #### This example uses the Model Object to retrive a Block Object for outputting its label in the console.<br/>
 *
 * <pre>
 *     // We store blocks in the layout
 *     var layout = modelObject.getLayout([]);
 * 
 *     if (layout.length > 0) {
 * 
 *         // Get first entry of from the layout, which is an array in this sample.
 *         var firstLayoutEntry = layout[0];
 * 
 *         // Create a Block Object for that entry.
 *         var block = modelObject.getBlockObject(firstLayoutEntry);
 * 
 *         // Check if the Block Object creation went well. (If a block isnt supported by the configuration of the Property Editor)
 *         if(block !== null) {
 *             console.log(block.label);
 *         }
 * 
 *     }
 * </pre>
 * 
 * #### This similar example uses the Block Object for settings a value on the first property in the Blocks Content.<br/>
 *
 * <pre>
 *     // We store blocks in the layout
 *     var layout = modelObject.getLayout([]);
 * 
 *     if (layout.length > 0) {
 * 
 *         // Get first entry of from the layout, which is an array in this sample.
 *         var firstLayoutEntry = layout[0];
 * 
 *         // Create a Block Object for that entry.
 *         var block = modelObject.getBlockObject(firstLayoutEntry);
 * 
 *         // Check if the Block Object creation went well. (If a block isnt supported by the configuration of the Property Editor)
 *         if(block !== null) {
 *             block.content.variants[0].tabs[0].properties[0].value = "Hello world";// This value will automaticly be synced to the Property Editors Data Model.
 *         }
 * 
 *     }
 * </pre>
 * 
 * See {@link umbraco.services.blockEditorModelObject#methods_getBlockObject getBlockObject} method for more information on the properties avaiable on a Block Object.
 * 
 * ## <b>Remove a Block</b>
 * 
 * Removing a Block and destroying the data of it is done by calling one method of the Model Object, but we have remember that we need to maintain the 'layout' object and this case is a great example of how thats done.
 * You will find that your code will very much be based on working with Block Objects and therefor removal of a Block is be done by refering a Block Object.
 * 
 * #### This example shows how to remove the first Block of our imaginary Block Editor and removing the block from our layout.
 *
 * <pre>
 *     var layout = modelObject.getLayout([]);
 *     if (layout.length > 0) {
 * 
 *         // Get first entry of from the layout, which is an array in this sample.
 *         var firstLayoutEntry = layout[0];
 * 
 *         // Create a Block Object for that entry.
 *         var block = modelObject.getBlockObject(firstLayoutEntry);
 * 
 *         // Check if the Block Object creation went well. (If a block isnt supported by the configuration of the Property Editor)
 *         if(block !== null) {
 *             modelObject.removeDataAndDestroyModel(block);// Removing the data of our block and destroying the Block Object for performance reasons.
 *             
 *             // We need to maintain the 'layout' object, so therefor its up to our code to remove the block from the 'layout' object.
 *             const index = array.indexOf(5);
 *             if (index > -1) {
 *                 layout.splice(index, 1);
 *             }
 *         }
 * 
 *     }
 *
 * </pre>
 * 
 * 
 * ## <b>Manage a Render Model for Displaying Blocks in the Property Editor</b>
 * 
 * For Rendering a Block in our AngularJS view
 * 
 * <pre>
 *     // TODO to be done.
 * </pre>
 * 
 * 
 */
(function () {
    'use strict';


    function blockEditorModelObjectFactory($interpolate, udiService, contentResource) {

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
            
            if (!elementModel || !elementModel.variants || !elementModel.variants.length) { return; }

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
         * @param {Object} blockObject BlockObject to recive data values from.
         */
        function getBlockLabel(blockObject) {
            if(blockObject.labelInterpolator !== undefined) {
                // We are just using the data model, since its a key/value object that is live synced. (if we need to add additional vars, we could make a shallow copy and apply those.)
                return blockObject.labelInterpolator(blockObject.data);
            }
            return blockObject.content.contentTypeName;
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
                    // Do notice that it is not performing a deep watch, meaning that we are only watching primatives and changes directly to the object of property-value.
                    // But we like to sync non-primative values as well! Yes, and this does happen, just not through this code, but through the nature of JavaScript. 
                    // Non-primative values act as references to the same data and are therefor synced.
                    blockObject.__watchers.push(isolatedScope.$watch("blockObjects._" + blockObject.key + "." + field + ".variants[0].tabs[" + t + "].properties[" + p + "].value", watcherCreator(blockObject, prop)));
                    
                    // We also like to watch our data model to be able to capture changes coming from other places.
                    if (forSettings === true) {
                        blockObject.__watchers.push(isolatedScope.$watch("blockObjects._" + blockObject.key + "." + "layout.settings" + "." + prop.alias, createLayoutSettingsModelWatcher(blockObject, prop)));
                    } else {
                        blockObject.__watchers.push(isolatedScope.$watch("blockObjects._" + blockObject.key + "." + "data" + "." + prop.alias, createDataModelWatcher(blockObject, prop)));
                    }
                }
            }
            if (blockObject.__watchers.length === 0) {
                // If no watcher where created, it means we have no properties to watch. This means that nothing will activate our generate the label, since its only triggered by watchers.
                blockObject.updateLabel();
            }
        }

        /**
         * Used to create a prop watcher for the data in the property editor data model.
         */
        function createDataModelWatcher(blockObject, prop)  {
            return function() {
                // sync data:
                prop.value = blockObject.data[prop.alias];

                blockObject.updateLabel();
            }
        }
        /**
         * Used to create a prop watcher for the settings in the property editor data model.
         */
        function createLayoutSettingsModelWatcher(blockObject, prop)  {
            return function() {
                // sync data:
                prop.value = blockObject.layout.settings[prop.alias];
            }
        }

        /**
         * Used to create a scoped watcher for a content property on a blockObject.
         */
        function createContentModelPropWatcher(blockObject, prop)  {
            return function() {
                // sync data:
                blockObject.data[prop.alias] = prop.value;

                blockObject.updateLabel();
            }
        }

        /**
         * Used to create a scoped watcher for a settings property on a blockObject.
         */
        function createSettingsModelPropWatcher(blockObject, prop)  {
            return function() {
                // sync data:
                blockObject.layout.settings[prop.alias] = prop.value;
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
         * @ngdoc method
         * @name constructor
         * @methodOf umbraco.services.blockEditorModelObject
         * @description Constructor of the model object used to handle Block Editor data.
         * @param {object} propertyModelValue data object of the property editor, usually model.value.
         * @param {string} propertyEditorAlias alias of the property.
         * @param {object} blockConfigurations block configurations.
         * @param {angular-scope} scopeOfExistance A local angularJS scope that exists as long as the data exists.
         * @param {angular-scope} propertyEditorScope A local angularJS scope that represents the property editors scope.
         * @returns {BlockEditorModelObject} A instance of BlockEditorModelObject.
         */
        function BlockEditorModelObject(propertyModelValue, propertyEditorAlias, blockConfigurations, scopeOfExistance, propertyEditorScope) {

            if (!propertyModelValue) {
                throw new Error("propertyModelValue cannot be undefined, to ensure we keep the binding to the angular model we need minimum an empty object.");
            }

            this.__watchers = [];

            // ensure basic part of data-structure is in place:
            this.value = propertyModelValue;
            this.value.layout = this.value.layout || {};
            this.value.data = this.value.data || [];

            this.propertyEditorAlias = propertyEditorAlias;
            this.blockConfigurations = blockConfigurations;

            this.scaffolds = [];

            this.isolatedScope = scopeOfExistance.$new(true);
            this.isolatedScope.blockObjects = {};
            
            this.__watchers.push(this.isolatedScope.$on("$destroy", this.destroy.bind(this)));

            this.__watchers.push(propertyEditorScope.$on("postFormSubmitting", this.sync.bind(this)));

        };
        
        BlockEditorModelObject.prototype = {

            /**
             * @ngdoc method
             * @name getBlockConfiguration
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Get block configuration object for a given contentTypeKey.
             * @param {string} key contentTypeKey to recive the configuration model for.
             * @returns {Object | null} Configuration model for the that specific block. Or ´null´ if the contentTypeKey isnt available in the current block configurations.
             */
            getBlockConfiguration: function(key) {
                return this.blockConfigurations.find(bc => bc.contentTypeKey === key) || null;
            },

            /**
             * @ngdoc method
             * @name load
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Load the scaffolding models for the given configuration, these are needed to provide usefull models for each block.
             * @param {Object} blockObject BlockObject to recive data values from.
             * @returns {Promise} A Promise object which resolves when all scaffold models are loaded.
             */
            load: function() {
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
             * @ngdoc method
             * @name getAvailableAliasesForBlockContent
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Retrive a list of aliases that are available for content of blocks in this property editor, does not contain aliases of block settings.
             * @return {Array} array of strings representing alias.
             */
            getAvailableAliasesForBlockContent: function() {
                return this.blockConfigurations.map(blockConfiguration => this.getScaffoldFromKey(blockConfiguration.contentTypeKey).contentTypeAlias);
            },

            /**
             * @ngdoc method
             * @name getAvailableBlocksForBlockPicker
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Retrive a list of available blocks, the list containing object with the confirugation model(blockConfigModel) and the element type model(elementTypeModel).
             * The purpose of this data is to provide it for the Block Picker.
             * @return {Array} array of objects representing available blocks, each object containing properties blockConfigModel and elementTypeModel.
             */
            getAvailableBlocksForBlockPicker: function() {

                var blocks = [];

                this.blockConfigurations.forEach(blockConfiguration => {
                    var scaffold = this.getScaffoldFromKey(blockConfiguration.contentTypeKey);
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
             * @ngdoc method
             * @name getScaffoldFromKey
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Get scaffold model for a given contentTypeKey.
             * @param {string} key contentTypeKey to recive the scaffold model for.
             * @returns {Object | null} Scaffold model for the that content type. Or null if the scaffolding model dosnt exist in this context.
             */
            getScaffoldFromKey: function(contentTypeKey) {
                return this.scaffolds.find(o => o.contentTypeKey === contentTypeKey);
            },

            /**
             * @ngdoc method
             * @name getScaffoldFromAlias
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Get scaffold model for a given contentTypeAlias, used by clipboardService.
             * @param {string} alias contentTypeAlias to recive the scaffold model for.
             * @returns {Object | null} Scaffold model for the that content type. Or null if the scaffolding model dosnt exist in this context.
             */
            getScaffoldFromAlias: function(contentTypeAlias) {
                return this.scaffolds.find(o => o.contentTypeAlias === contentTypeAlias);
            },

            /**
             * @ngdoc method
             * @name getBlockObject
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Retrieve a Block Object for the given layout entry.
             * The Block Object offers the nesecary data to display and edit a block.
             * The Block Object setups live syncronization of content and settings models back to the data of your Property Editor model.
             * The returned object, named ´BlockObject´, contains several usefull models to make editing of this block happen.
             * The ´BlockObject´ contains the following properties:
             * - key {string}: runtime generated key, usefull for tracking of this object
             * - content {Object}: Content model, the content data in a ElementType model.
             * - settings {Object}: Settings model, the settings data in a ElementType model.
             * - config {Object}: A local deep copy of the block configuration model.
             * - label {string}: The label for this block.
             * - updateLabel {Method}: Method to trigger an update of the label for this block.
             * - data {Object}: A reference to the content data object from your property editor model.
             * - settingsData {Object}: A reference to the settings data object from your property editor model.
             * - layout {Object}: A refernce to the layout entry from your property editor model.
             * @param {Object} layoutEntry the layout entry object to build the block model from.
             * @return {Object | null} The BlockObject for the given layout entry. Or null if data or configuration wasnt found for this block.
             */
            getBlockObject: function(layoutEntry) {

                var udi = layoutEntry.udi;

                var dataModel = this._getDataByUdi(udi);

                if (dataModel === null) {
                    console.error("Couldnt find content model of " + udi)
                    return null;
                }

                var blockConfiguration = this.getBlockConfiguration(dataModel.contentTypeKey);
                var contentScaffold;

                if (blockConfiguration === null) {
                    console.error("The block entry of "+udi+" is not begin initialized cause its contentTypeKey is not allowed for this PropertyEditor");
                } else {
                    var contentScaffold = this.getScaffoldFromKey(blockConfiguration.contentTypeKey);
                    if(contentScaffold === null) {
                        console.error("The block entry of "+udi+" is not begin initialized cause its Element Type was not loaded.");
                    }
                }

                if (blockConfiguration === null || contentScaffold === null) {

                    blockConfiguration = {
                        label: "Unsupported Block",
                        unsupported: true
                    };
                    contentScaffold = {};
                    
                }

                var blockObject = {};
                // Set an angularJS cloneNode method, to avoid this object begin cloned.
                blockObject.cloneNode = function() {
                    return null;// angularJS accept this as a cloned value as long as the 
                }
                blockObject.key = String.CreateGuid().replace(/-/g, "");
                blockObject.config = Utilities.copy(blockConfiguration);
                if (blockObject.config.label && blockObject.config.label !== "") {
                    blockObject.labelInterpolator = $interpolate(blockObject.config.label);
                }
                blockObject.__scope = this.isolatedScope;
                blockObject.updateLabel = _.debounce(function () {this.__scope.$evalAsync(function() {
                    this.label = getBlockLabel(this);
                }.bind(this))}.bind(blockObject), 10);

                // make basics from scaffold
                blockObject.content = Utilities.copy(contentScaffold);
                blockObject.content.udi = udi;

                mapToElementModel(blockObject.content, dataModel);

                blockObject.data = dataModel;
                blockObject.layout = layoutEntry;
                blockObject.__watchers = [];

                if (blockConfiguration.settingsElementTypeKey) {
                    var settingsScaffold = this.getScaffoldFromKey(blockConfiguration.settingsElementTypeKey);
                    if (settingsScaffold !== null) {

                        layoutEntry.settings = layoutEntry.settings || {};
                        
                        blockObject.settingsData = layoutEntry.settings;

                        // make basics from scaffold
                        blockObject.settings = Utilities.copy(settingsScaffold);
                        layoutEntry.settings = layoutEntry.settings || {};
                        if (!layoutEntry.settings.key) { layoutEntry.settings.key = String.CreateGuid(); }
                        if (!layoutEntry.settings.contentTypeKey) { layoutEntry.settings.contentTypeKey = blockConfiguration.settingsElementTypeKey; }
                        mapToElementModel(blockObject.settings, layoutEntry.settings);
                    }
                }

                blockObject.retriveValuesFrom = function(content, settings) {
                    if (this.content !== null) {
                        mapElementValues(content, this.content);
                    }
                    if (this.config.settingsElementTypeKey !== null) {
                        mapElementValues(settings, this.settings);
                    }
                }


                blockObject.sync = function() {
                    if (this.content !== null) {
                        mapToPropertyModel(this.content, this.data);
                    }
                    if (this.config.settingsElementTypeKey !== null) {
                        mapToPropertyModel(this.settings, this.layout.settings);
                    }
                }

                // first time instant update of label.
                blockObject.label = getBlockLabel(blockObject);

                // Add blockObject to our isolated scope to enable watching its values:
                this.isolatedScope.blockObjects["_" + blockObject.key] = blockObject;
                addWatchers(blockObject, this.isolatedScope);
                addWatchers(blockObject, this.isolatedScope, true);

                blockObject.destroy = function() {
                    // remove property value watchers:
                    this.__watchers.forEach(w => { w(); });
                    delete this.__watchers;

                    // help carbage collector:
                    delete this.layout;
                    delete this.data;
                    
                    // remove model from isolatedScope.
                    delete this.__scope.blockObjects["_" + this.key];
                    delete this.__scope;

                    // removes this method, making it unposible to destroy again.
                    delete this.destroy;
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
                this.destroyBlockObject(blockObject);
                this.removeDataByUdi(blockObject.content.udi);
            },

            /**
             * @ngdoc method
             * @name destroyBlockObject
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Destroys the Block Model, but all data is kept.
             * @param {Object} blockObject The BlockObject to be destroyed.
             */
            destroyBlockObject: function(blockObject) {
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
            getLayout: function(defaultStructure) {
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
             * @param {string} contentTypeKey the contentTypeKey of the block you wish to create, if contentTypeKey is not avaiable in the block configuration then ´null´ will be returned.
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
             * @ngdoc method
             * @name createFromElementType
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Insert data from ElementType Model
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



            /**
             * @ngdoc method
             * @name sync
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Force immidiate update of the blockobject models to the property model.
             */
            sync: function() {
                for (const key in this.isolatedScope.blockObjects) {
                    this.isolatedScope.blockObjects[key].sync();
                }
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

            /**
             * @ngdoc method
             * @name removeDataByUdi
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Removes the data of a given UDI.
             * Notice this method does not remove the block from your layout, this will need to be handlede by the Property Editor since this services donst know about your layout structure.
             * @param {string} udi The UDI of the data to be removed.
             */
            removeDataByUdi: function(udi) {
                const index = this.value.data.findIndex(o => o.udi === udi);
                if (index !== -1) {
                    this.value.data.splice(index, 1);
                }
            },

            /**
             * @ngdoc method
             * @name destroy
             * @methodOf umbraco.services.blockEditorModelObject
             * @description Notice you should not need to destroy the BlockEditorModelObject since it will automaticly be destroyed when the scope of existance gets destroyed.
             */
            destroy: function() {

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
                delete this.destroy;
            }
        }

        return BlockEditorModelObject;
    }

    angular.module('umbraco.services').service('blockEditorModelObject', blockEditorModelObjectFactory);

})();
