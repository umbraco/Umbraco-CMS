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

    function umbVariantContentController($scope, contentAppHelper) {

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

            // Make copy of apps, so we can have a variant specific model for the App. (needed for validation etc.)
            vm.editor.variantApps = Utilities.copy(vm.content.apps);

            var activeApp = vm.content.apps.find((app) => app.active);

            onAppChanged(activeApp);
        }

        function showBackButton() {
            return vm.page.listViewPath !== null && vm.showBack;
        }

        /** Called when the component has linked all elements, this is when the form controller is available */
        function postLink() {
            //set the content to dirty if the header changes
            unsubscribe.push($scope.$watch("vm.editor.content.name",
                function (newValue, oldValue) {
                    if (newValue !== oldValue) {
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
            if (vm.onSelectApp) {
                vm.onSelectApp({ "app": item });
            }
        }

        $scope.$on("editors.apps.appChanged", function ($event, $args) {
            var activeApp = $args.app;

            // sync varaintApps active with new active.
            _.forEach(vm.editor.variantApps, function (app) {
                app.active = (app.alias === activeApp.alias);
            });

            onAppChanged(activeApp);
        });

        $scope.$on("listView.itemsChanged", function ($event, $args) {
            vm.disableActionsMenu = $args.items.length > 0;
        });

        function onAppChanged(activeApp) {
            // set the name field to readonly if the user don't have update permissions or the active content app is not "Content" or "Info"
            const allowUpdate = vm.editor.content.allowedActions.includes('A');
            const isContentBasedApp = activeApp && contentAppHelper.isContentBasedApp(activeApp);
            vm.nameReadonly = !allowUpdate || !isContentBasedApp;
        }

        /**
         * Used to proxy a callback
         * @param {any} item
         */
        function selectAppAnchor(item, anchor) {
            // call the callback if any is registered
            if (vm.onSelectAppAnchor) {
                vm.onSelectAppAnchor({ "app": item, "anchor": anchor });
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
