(function () {
    'use strict';

    function EditorContentHeader(serverValidationManager, localizationService, editorState, contentVariantUtilities) {

        function link(scope) {

            var unsubscribe = [];
            
            if (!scope.serverValidationNameField) {
                scope.serverValidationNameField = "Name";
            }
            if (!scope.serverValidationAliasField) {
                scope.serverValidationAliasField = "Alias";
            }

            scope.isNew = scope.editor.content.state == "NotCreated";

            localizationService.localizeMany(
                [
                    scope.isNew ? "placeholders_a11yCreateItem" : "placeholders_a11yEdit",
                    "placeholders_a11yName"
                ]
            ).then(function (data) {
                scope.a11yMessage = data[0];
                scope.a11yName = data[1];
                if (!scope.isNew) {
                    scope.a11yMessage += " " + scope.editor.content.name;

                } else {
                    var name = editorState.current.contentTypeName;
                    scope.a11yMessage += " " + name;
                    scope.a11yName = name + " " + scope.a11yName;
                }
            });
            scope.vm = {};
            scope.vm.hasVariants = false;
            scope.vm.hasCulture = false;
            scope.vm.hasSegments = false;
            scope.vm.dropdownOpen = false;
            scope.vm.variantsWithError = [];
            scope.vm.defaultVariant = null;
            scope.vm.errorsOnOtherVariants = false;// indicating wether to show that other variants, than the current, have errors.
            
            function checkErrorsOnOtherVariants() {
                var check = false;
                angular.forEach(scope.content.variants, function (variant) {
                    if (variant.active !== true && scope.variantHasError(variant)) {
                        check = true;
                    }
                });
                scope.vm.errorsOnOtherVariants = check;
            }
            
            function onVariantValidation(valid, errors, allErrors, culture, segment) {
                var index = scope.vm.variantsWithError.findIndex((item) => item.culture === culture && item.segment === segment)
                if(valid === true) {
                    if (index !== -1) {
                        scope.vm.variantsWithError.splice(index, 1);
                    }
                } else {
                    if (index === -1) {
                        scope.vm.variantsWithError.push({"culture": culture, "segment": segment});
                    }
                }
                checkErrorsOnOtherVariants();
            }
            
            function onInit() {
                
                // find default + check if we have variants.
                angular.forEach(scope.content.variants, function (variant) {
                    if (variant.language !== null && variant.language.isDefault) {
                        scope.vm.defaultVariant = variant;
                    }
                    if (variant.language !== null) {
                        scope.vm.hasCulture = true;
                    }
                    if (variant.segment !== null) {
                        scope.vm.hasSegment = true;
                    }
                });

                scope.vm.hasVariants = (scope.vm.hasCulture || scope.vm.hasSegment);
                
                checkErrorsOnOtherVariants();

                scope.vm.variantMenu = [];
                if (scope.vm.hasCulture) {
                    angular.forEach(scope.content.variants, (v) => {
                        if (v.language !== null && v.segment === null) {
                            var variantMenuEntry = {
                                open: v.language && v.language.culture === scope.editor.culture,
                                variant: v,
                                subVariants: scope.content.variants.filter( (subVariant) => subVariant.language.culture === v.language.culture && subVariant.segment !== null)
                            };
                            scope.vm.variantMenu.push(variantMenuEntry);
                        }
                    });
                } else {
                    angular.forEach(scope.content.variants, (v) => {
                        scope.vm.variantMenu.push({
                            variant: v
                        });
                    });
                }

                
                angular.forEach(scope.editor.variantApps, (app) => {
                    if (app.alias === "umbContent") {
                        app.anchors = scope.content.tabs;
                    }
                });
                
                angular.forEach(scope.content.variants, function (variant) {
                    unsubscribe.push(serverValidationManager.subscribe(null, variant.language !== null ? variant.language.culture : null, variant.segment, null, onVariantValidation));
                });
                
                unsubscribe.push(serverValidationManager.subscribe(null, null, null, null, onVariantValidation));
                
                
                
            }

            scope.getVariantDisplayName = contentVariantUtilities.getDisplayName;

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
             * Check whether a variant has a error, used to display errors in variant switcher.
             * @param {any} culture
             */
            scope.variantHasError = function(variant) {
                if(variant.language) {
                    // if we are looking for the variant with default language then we also want to check for invariant variant.
                    if (variant.language.culture === scope.vm.defaultVariant.language.culture && variant.segment === null) {
                        if(scope.vm.variantsWithError.find((item) => item.culture === "invariant" && item.segment === null) !== undefined) {
                            return true;
                        }
                    }
                }
                if(scope.vm.variantsWithError.find((item) => (!variant.language || item.culture === variant.language.culture) && item.segment === variant.segment) !== undefined) {
                    return true;
                }
                return false;
            }

            onInit();
            
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
                editor: "=",
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
