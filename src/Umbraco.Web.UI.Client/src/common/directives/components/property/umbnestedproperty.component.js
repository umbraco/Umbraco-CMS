(function () {
    "use strict";


    /**
     * @ngdoc component
     * @name umbraco.umbNestedProperty
     * @function
     *
     * @description
     * Used to have nested property editors within complex editors in order to generate jsonpath for them to be used in validation
     */
    angular
        .module("umbraco")
        .component("umbNestedProperty", {
            transclude: true,
            template: '<div><pre>{{vm.getValidationPath()}}</pre><div ng-transclude></div></div>',
            controller: NestedPropertyController,
            controllerAs: 'vm',
            bindings: {
                propertyTypeAlias: "@",
                elementTypeIndex: "<",
                findInScopeChain: "<" // TODO: Use/enable this
            },
            require: {
                umbNestedProperty: "?^^"
            }
        });

    function NestedPropertyController($scope, angularHelper) {
        var vm = this;
        vm.$onInit = function () {
            if (!vm.propertyTypeAlias) {
                throw "no propertyTypeAlias specified for umbNestedProperty";
            }
        };

        vm.$postLink = function () {
            // if directive inheritance (DOM) doesn't find one, then check scope inheritance
            if (!vm.umbNestedProperty/* && findInScopeChain*/) {
                var found = angularHelper.traverseScopeChain($scope, s => s.vm && s.vm.constructor.name == "NestedPropertyController");
                if (found) {
                    vm.umbNestedProperty = found.vm;
                }
            }
        }

        // returns a jsonpath for where this property is located in a hierarchy
        // this will call into all hierarchical parents
        vm.getValidationPath = function () {
            
            var path = vm.umbNestedProperty ? vm.umbNestedProperty.getValidationPath() : "$";            
            path += ".[nestedValidation].[" + vm.elementTypeIndex + "].[" + vm.propertyTypeAlias + "]";
            return path;
        }
    }


})();
