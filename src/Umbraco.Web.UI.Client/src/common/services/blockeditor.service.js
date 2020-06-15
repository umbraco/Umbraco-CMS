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
 * See {@link umbraco.services.BlockEditorModelObject BlockEditorModelObject} for more samples.
 * 
 */
(function () {
    'use strict';


    function blockEditorService(blockEditorModelObject) {

        /**
         * @ngdocs function
         * @name createModelObject
         * @methodOf umbraco.services.blockEditorService
         * 
         * @description
         * Create a new Block Editor Model Object.
         * See {@link umbraco.services.blockEditorModelObject BlockEditorModelObject}
         * 
         * @see umbraco.services.BlockEditorModelObject
         * @return {BlockEditorModelObject} A instance of the BlockEditorModelObject class.
         */
        function createModelObject(propertyModelValue, propertyEditorAlias, blockConfigurations, propertyScope) {
            return new blockEditorModelObject(propertyModelValue, propertyEditorAlias, blockConfigurations, propertyScope);
        }

        return {
            createModelObject: createModelObject
        }
    }

    angular.module('umbraco.services').service('blockEditorService', blockEditorService);

})();
