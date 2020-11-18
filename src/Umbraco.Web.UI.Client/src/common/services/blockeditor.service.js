/**
 * @ngdoc service
 * @name umbraco.services.blockEditorService
 *
 * @description
 * <b>Added in Umbraco 8.7</b>. Service for dealing with Block Editors.
 *
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
 *     modelObject.load().then(onLoaded);
 * </pre>
 *
 *
 * See {@link umbraco.services.blockEditorModelObject BlockEditorModelObject} for more samples.
 *
 */
(function () {
    'use strict';



    /**
     * When performing a runtime copy of Block Editors entries, we copy the ElementType Data Model and inner IDs are kept identical, to ensure new IDs are changed on paste we need to provide a resolver for the ClipboardService.
     */
    angular.module('umbraco').run(['clipboardService', 'udiService', function (clipboardService, udiService) {

        function replaceUdi(obj, key, dataObject) {
            var udi = obj[key];
            var newUdi = udiService.create("element");
            obj[key] = newUdi;
            dataObject.forEach((data) => {
                if (data.udi === udi) {
                    data.udi = newUdi;
                }
            });
        }
        function replaceUdisOfObject(obj, propValue) {
            for (var k in obj) {
                if(k === "contentUdi") {
                    replaceUdi(obj, k, propValue.contentData);
                } else if(k === "settingsUdi") {
                    replaceUdi(obj, k, propValue.settingsData);
                } else {
                    // lets crawl through all properties of layout to make sure get captured all `contentUdi` and `settingsUdi` properties.
                    var propType = typeof obj[k];
                    if(propType != null && (propType === "object" || propType === "array")) {
                        replaceUdisOfObject(obj[k], propValue)
                    }
                }
            }
        }
        function removeBlockReferences(obj) {
            for (var k in obj) {
                if(k === "contentUdi") {
                    delete obj[k];
                } else if(k === "settingsUdi") {
                    delete obj[k];
                } else {
                    // lets crawl through all properties of layout to make sure get captured all `contentUdi` and `settingsUdi` properties.
                    var propType = typeof obj[k];
                    if(propType != null && (propType === "object" || propType === "array")) {
                        removeBlockReferences(obj[k])
                    }
                }
            }
        }


        function elementTypeBlockResolver(obj, propPasteResolverMethod) {
            // we could filter for specific Property Editor Aliases, but as the Block Editor structure can be used by many Property Editor we do not in this code know a good way to detect that this is a Block Editor and will therefor leave it to the value structure to determin this.
            rawBlockResolver(obj.value, propPasteResolverMethod);
        }

        clipboardService.registerPastePropertyResolver(elementTypeBlockResolver, clipboardService.TYPES.ELEMENT_TYPE);


        function rawBlockResolver(value, propPasteResolverMethod) {
            if (value != null && typeof value === "object") {

                // we got an object, and it has these three props then we are most likely dealing with a Block Editor.
                if ((value.layout !== undefined && value.contentData !== undefined && value.settingsData !== undefined)) {

                    replaceUdisOfObject(value.layout, value);

                    // run resolvers for inner properties of this Blocks content data.
                    if(value.contentData.length > 0) {
                        value.contentData.forEach((item) => {
                            for (var k in item) {
                                propPasteResolverMethod(item[k], clipboardService.TYPES.RAW);
                            }
                        });
                    }
                    // run resolvers for inner properties of this Blocks settings data.
                    if(value.settingsData.length > 0) {
                        value.settingsData.forEach((item) => {
                            for (var k in item) {
                                propPasteResolverMethod(item[k], clipboardService.TYPES.RAW);
                            }
                        });
                    }

                }
            }
        }

        clipboardService.registerPastePropertyResolver(rawBlockResolver, clipboardService.TYPES.RAW);


        function provideNewUdisForBlockResolver(block, propPasteResolverMethod) {

            if(block.layout) {
                // We do not support layout child blocks currently, these should be stripped out as we only will be copying a single entry.
                removeBlockReferences(block.layout);
            }

            if(block.data) {
                // Make new UDI for content-element
                block.data.udi = block.layout.contentUdi = udiService.create("element");
            }

            if(block.settingsData) {
                // Make new UDI for settings-element
                block.settingsData.udi = block.layout.settingsUdi = udiService.create("element");
            }

        }

        clipboardService.registerPastePropertyResolver(provideNewUdisForBlockResolver, clipboardService.TYPES.BLOCK);

    }]);




    function blockEditorService(blockEditorModelObject) {

        /**
         * @ngdocs function
         * @name createModelObject
         * @methodOf umbraco.services.blockEditorService
         *
         * @description
         * Create a new Block Editor Model Object.
         * See {@link umbraco.services.blockEditorModelObject blockEditorModelObject}
         *
         * @see umbraco.services.blockEditorModelObject
         * @param {object} propertyModelValue data object of the property editor, usually model.value.
         * @param {string} propertyEditorAlias alias of the property.
         * @param {object} blockConfigurations block configurations.
         * @param {angular-scope} scopeOfExistance A local angularJS scope that exists as long as the data exists.
         * @param {angular-scope} propertyEditorScope A local angularJS scope that represents the property editors scope.
         * @return {blockEditorModelObject} A instance of the BlockEditorModelObject class.
         */
        function createModelObject(propertyModelValue, propertyEditorAlias, blockConfigurations, scopeOfExistance, propertyEditorScope) {
            return new blockEditorModelObject(propertyModelValue, propertyEditorAlias, blockConfigurations, scopeOfExistance, propertyEditorScope);
        }

        return {
            createModelObject: createModelObject
        }
    }

    angular.module('umbraco.services').service('blockEditorService', blockEditorService);

})();
