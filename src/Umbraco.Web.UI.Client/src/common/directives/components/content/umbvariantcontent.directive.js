(function () {
    'use strict';

    /**
     * A component to encapsulate each variant editor which includes the name header and all content apps for a given variant
     */
    var umbVariantContent = {
        templateUrl: 'views/components/content/umb-variant-content.html',
        bindings: {
            content: "<",
            page: "<",
            editor: "<",
            editorIndex: "<",
            editorCount: "<",
            openVariants: "<",
            onCloseSplitView: "&",
            onSelectVariant: "&",
            onOpenSplitView: "&",
            onSelectApp: "&",
            onSelectAppAnchor: "&"
        },
        controllerAs: 'vm',
        controller: umbVariantContentController
    };
    
    function umbVariantContentController($scope, $element, $location) {

        var unsubscribe = [];

        var vm = this;

        vm.$onInit = onInit;
        vm.$postLink = postLink;
        vm.$onDestroy = onDestroy;

        vm.selectVariant = selectVariant;
        vm.openSplitView = openSplitView;
        vm.selectApp = selectApp;
        vm.selectAppAnchor = selectAppAnchor;

        function onInit() {
            // disable the name field if the active content app is not "Content"
            vm.nameDisabled = false;
            angular.forEach(vm.editor.content.apps, function(app){
                if(app.active && app.alias !== "umbContent" && app.alias !== "umbInfo") {
                    vm.nameDisabled = true;
                }
            });
        }
        
        /** Called when the component has linked all elements, this is when the form controller is available */
        function postLink() {
            //set the content to dirty if the header changes
            unsubscribe.push($scope.$watch("contentHeaderForm.$dirty",
                function(newValue, oldValue) {
                    if (newValue === true) {
                        vm.editor.content.isDirty = true;
                    }
                }));
        }
        
        function onDestroy() {
            for (var i = 0; i < unsubscribe.length; i++) {
                unsubscribe[i]();
            }
        }

        /**
         * Used to proxy a callback
         * @param {any} variant
         */
        function selectVariant(variant) {
            if (vm.onSelectVariant) {
                vm.onSelectVariant({ "variant": variant });
            }
        }

        /**
         * Used to proxy a callback
         * @param {any} item
         */
        function selectApp(item) {
            // disable the name field if the active content app is not "Content" or "Info"
            vm.nameDisabled = false;
            if(item && item.alias !== "umbContent" && item.alias !== "umbInfo") {
                vm.nameDisabled = true;
            }
            // call the callback if any is registered
            if(vm.onSelectApp) {
                vm.onSelectApp({"app": item});
            }
        }

        /**
         * Used to proxy a callback
         * @param {any} item
         */
        function selectAppAnchor(item, anchor) {
            // call the callback if any is registered
            if(vm.onSelectAppAnchor) {
                vm.onSelectAppAnchor({"app": item, "anchor": anchor});
            }
        }

        /**
         * Used to proxy a callback
         * @param {any} variant
         */
        function openSplitView(variant) {
            if (vm.onOpenSplitView) {
                vm.onOpenSplitView({ "variant": variant });
            }
        }
    }

    angular.module('umbraco.directives').component('umbVariantContent', umbVariantContent);

})();
