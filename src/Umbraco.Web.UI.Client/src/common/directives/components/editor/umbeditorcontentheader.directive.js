(function () {
    'use strict';

    function EditorContentHeader() {

        function link(scope, el, attr, ctrl) {

            if (!scope.serverValidationNameField) {
                scope.serverValidationNameField = "Name";
            }
            if (!scope.serverValidationAliasField) {
                scope.serverValidationAliasField = "Alias";
            }
            
            scope.vm = {};
            scope.vm.dropdownOpen = false;
            scope.vm.currentVariant = "";

            function onInit() {
                
                setCurrentVariant();
                
                angular.forEach(scope.content.apps, (app) => {
                    if (app.alias === "umbContent") {
                        app.anchors = scope.content.tabs;
                    }
                });
                
            }

            function setCurrentVariant() {
                angular.forEach(scope.content.variants, function (variant) {
                    if (variant.active) {
                        scope.vm.currentVariant = variant;
                    }
                });
            }

            scope.goBack = function () {
                if (scope.onBack) {
                    scope.onBack();
                }
            };

            scope.selectVariant = function (event, variant) {

                if (scope.onSelectVariant) {
                    scope.vm.dropdownOpen = false;
                    scope.onSelectVariant({ "variant": variant });
                }
            };

            scope.selectNavigationItem = function(item) {
                if(scope.onSelectNavigationItem) {
                    scope.onSelectNavigationItem({"item": item});
                }
            }

            scope.selectAnchorItem = function(item, anchor) {
                if(scope.onSelectAnchorItem) {
                    scope.onSelectAnchorItem({"item": item, "anchor": anchor});
                }
            }

            scope.closeSplitView = function () {
                if (scope.onCloseSplitView) {
                    scope.onCloseSplitView();
                }
            };

            scope.openInSplitView = function (event, variant) {
                if (scope.onOpenInSplitView) {
                    scope.vm.dropdownOpen = false;
                    scope.onOpenInSplitView({ "variant": variant });
                }
            };

            /**
             * keep track of open variants - this is used to prevent the same variant to be open in more than one split view
             * @param {any} culture
             */
            scope.variantIsOpen = function(culture) {
                if(scope.openVariants.indexOf(culture) !== -1) {
                    return true;
                }
            }

            onInit();

            //watch for the active culture changing, if it changes, update the current variant
            if (scope.content.variants) {
                scope.$watch(function () {
                    for (var i = 0; i < scope.content.variants.length; i++) {
                        var v = scope.content.variants[i];
                        if (v.active) {
                            return v.language.culture;
                        }
                    }
                    return scope.vm.currentVariant.language.culture; //should never get here
                }, function (newValue, oldValue) {
                    if (newValue !== scope.vm.currentVariant.language.culture) {
                        setCurrentVariant();
                    }
                });
            }
        }


        var directive = {
            transclude: true,
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/editor/umb-editor-content-header.html',
            scope: {
                name: "=",
                nameDisabled: "<?",
                menu: "=",
                hideActionsMenu: "<?",
                content: "=",
                openVariants: "<",
                hideChangeVariant: "<?",
                onSelectNavigationItem: "&?",
                onSelectAnchorItem: "&?",
                showBackButton: "<?",
                onBack: "&?",
                splitViewOpen: "=?",
                onOpenInSplitView: "&?",
                onCloseSplitView: "&?",
                onSelectVariant: "&?",
                serverValidationNameField: "@?",
                serverValidationAliasField: "@?"
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbEditorContentHeader', EditorContentHeader);

})();
