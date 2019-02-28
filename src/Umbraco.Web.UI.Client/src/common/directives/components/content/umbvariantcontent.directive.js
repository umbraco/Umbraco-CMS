﻿(function () {
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
            onSelectAppAnchor: "&",
            onBack: "&?",
            showBack: "<?"
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
        vm.showBackButton = showBackButton;

        function onInit() {
            // disable the name field if the active content app is not "Content"
            vm.nameDisabled = false;
            angular.forEach(vm.editor.content.apps, function(app){
                if(app.active && app.alias !== "umbContent" && app.alias !== "umbInfo") {
                    vm.nameDisabled = true;
                }
            });
        }
        
        function showBackButton() {
            return vm.page.listViewPath !== null && vm.showBack;
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
            // call the callback if any is registered
            if(vm.onSelectApp) {
                vm.onSelectApp({"app": item});
            }
        }
        
        $scope.$on("editors.apps.appChanged", function($event, $args) {
            var app = $args.app;
            // disable the name field if the active content app is not "Content" or "Info"
            vm.nameDisabled = false;
            if(app && app.alias !== "umbContent" && app.alias !== "umbInfo") {
                vm.nameDisabled = true;
            }
        });

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
