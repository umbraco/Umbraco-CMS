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
            editorCount: "<",
            onCloseSplitView: "&",
            onSelectVariant: "&",
            onOpenSplitView: "&"
        },
        controllerAs: 'vm',
        controller: umbVariantContentController
    };
    
    function umbVariantContentController($scope, $element, $location) {

        var unsubscribe = [];

        var vm = this;

        vm.$postLink = postLink;
        vm.$onDestroy = onDestroy;

        vm.selectVariant = selectVariant;
        vm.openSplitView = openSplitView;
        vm.backToListView = backToListView;
        
        /** Called when the component has linked all elements, this is when the form controller is available */
        function postLink() {
            //set the content to dirty if the header changes
            unsubscribe.push($scope.$watch("contentHeaderForm.$dirty",
                function(newValue, oldValue) {
                    if (newValue === true) {
                        scope.editor.content.isDirty = true;
                    }
                }));
        }
        
        function onDestroy() {
            for (var i = 0; i < unsubscribe.length; i++) {
                unsubscribe[i]();
            }
        }

        function backToListView() {
            $location.path(vm.page.listViewPath);
        };

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
