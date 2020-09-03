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
                    var propType = typeof obj[k];
                    if(propType === "object" || propType === "array") {
                        replaceUdisOfObject(obj[k], propValue)
                    }
                }
            }
        }

        function replaceBlockListUDIsResolver(obj, propClearingMethod) {

            if (typeof obj === "object") {

                // 'obj' can both be a property object or the raw value of a inner property.
                var value = obj;

                // if we got a property object from a ContentTypeModel we need to look at the value. We check for value and editor to, sort of, ensure this is the case.
                if(obj.value !== undefined && obj.editor !== undefined) {
                    value = obj.value;
                    // If value isnt a object, lets break out.
                    if(typeof obj.value !== "object") {
                        return;
                    }
                }

                // we got an object, and it has these three props then we are most likely dealing with a Block Editor.
                if ((value.layout !== undefined && value.contentData !== undefined && value.settingsData !== undefined)) {

                    replaceUdisOfObject(value.layout, value);

                    // replace UDIs for inner properties of this Block Editors content data.
                    if(value.contentData.length > 0) {
                        value.contentData.forEach((item) => {
                            for (var k in item) {
                                propClearingMethod(item[k]);
                            }
                        });
                    }
                    // replace UDIs for inner properties of this Block Editors settings data.
                    if(value.settingsData.length > 0) {
                        value.settingsData.forEach((item) => {
                            for (var k in item) {
                                propClearingMethod(item[k]);
                            }
                        });
                    }

                }
            }
        }

        clipboardService.registerPastePropertyResolver(replaceBlockListUDIsResolver)

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
