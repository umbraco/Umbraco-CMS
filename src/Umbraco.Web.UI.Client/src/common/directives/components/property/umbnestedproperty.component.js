(function () {
    "use strict";


    /**
     * @ngdoc component
     * @name Umbraco.umbBlockListBlockContent
     * @function
     *
     * @description
     * The component for a style-inheriting block of the block list property editor.
     */
    angular
        .module("umbraco")
        .component("umbNestedProperty", {
            transclude: true,
            template: '<div ng-transclude></div>',
            controller: NestedPropertyController,
            controllerAs: 'vm',
            bindings: {
                propertyTypeAlias: "@",
                elementTypeIndex: "@"
            },
            require: {
                umbNestedProperty: "?^^umbNestedProperty"
            }
        });

    function NestedPropertyController($scope) {
        var vm = this;
        vm.$onInit = function () {

        };

        // returns a jsonpath for where this property is located in a hierarchy
        // this will call into all hierarchical parents
        vm.getValidationPath = function () {
            
            var path = vm.umbNestedProperty ? vm.umbNestedProperty.getValidationPath() : "$";
            if (vm.propertyTypeAlias && vm.elementTypeIndex) {
                path += ".[nestedValidation].[" + vm.elementTypeIndex + "].[" + vm.propertyTypeAlias + "]";
                return path;
            }
            return null;
        }
    }


})();
