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
