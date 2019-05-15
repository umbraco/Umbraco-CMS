(function () {
    'use strict';

    function EditorContentHeader(serverValidationManager) {

        function link(scope, el, attr, ctrl) {
            
            var unsubscribe = [];
            
            if (!scope.serverValidationNameField) {
                scope.serverValidationNameField = "Name";
            }
            if (!scope.serverValidationAliasField) {
                scope.serverValidationAliasField = "Alias";
            }
            
            scope.vm = {};
            scope.vm.dropdownOpen = false;
            scope.vm.currentVariant = "";
            scope.vm.variantsWithError = [];
            scope.vm.defaultVariant = null;
            
            scope.vm.errorsOnOtherVariants = false;// indicating wether to show that other variants, than the current, have errors.
            
            function checkErrorsOnOtherVariants() {
                var check = false;
                angular.forEach(scope.content.variants, function (variant) {
                    if (scope.openVariants.indexOf(variant.language.culture) === -1 && scope.variantHasError(variant.language.culture)) {
                        check = true;
                    }
                });
                scope.vm.errorsOnOtherVariants = check;
            }
            
            function onCultureValidation(valid, errors, allErrors, culture) {
                var index = scope.vm.variantsWithError.indexOf(culture);
                if(valid === true) {
                    if (index !== -1) {
                        scope.vm.variantsWithError.splice(index, 1);
                    }
                } else {
                    if (index === -1) {
                        scope.vm.variantsWithError.push(culture);
                    }
                }
                checkErrorsOnOtherVariants();
            }
            
            function onInit() {
                
                // find default.
                angular.forEach(scope.content.variants, function (variant) {
                    if (variant.language.isDefault) {
                        scope.vm.defaultVariant = variant;
                    }
                });
                
                setCurrentVariant();
                
                angular.forEach(scope.content.apps, (app) => {
                    if (app.alias === "umbContent") {
                        app.anchors = scope.content.tabs;
                    }
                });
                
                
                angular.forEach(scope.content.variants, function (variant) {
                    unsubscribe.push(serverValidationManager.subscribe(null, variant.language.culture, null, onCultureValidation));
                });
                
                unsubscribe.push(serverValidationManager.subscribe(null, null, null, onCultureValidation));
                
                
                
            }

            function setCurrentVariant() {
                angular.forEach(scope.content.variants, function (variant) {
                    if (variant.active) {
                        scope.vm.currentVariant = variant;
                        checkErrorsOnOtherVariants();
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
                return (scope.openVariants.indexOf(culture) !== -1);
            }
            
            /**
             * Check whether a variant has a error, used to display errors in variant switcher.
             * @param {any} culture
             */
            scope.variantHasError = function(culture) {
                // if we are looking for the default language we also want to check for invariant (null)
                if (culture === scope.vm.defaultVariant.language.culture) {
                    if(scope.vm.variantsWithError.indexOf(null) !== -1) {
                        return true;
                    }
                }
                if(scope.vm.variantsWithError.indexOf(culture) !== -1) {
                    return true;
                }
                return false;
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
            
            scope.$on('$destroy', function () {
                for (var u in unsubscribe) {
                    unsubscribe[u]();
                }
            });
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
