(function () {
    'use strict';

    function EditorContentHeader(serverValidationManager, localizationService, editorState, contentEditingHelper, contentTypeHelper) {
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
                    "placeholders_a11yName",
                    scope.isNew ? "general_new" : "general_edit"
                ]
            ).then(function (data) {
                scope.a11yMessage = data[0];
                scope.a11yName = data[1];
                var title = data[2] + ": ";
                if (!scope.isNew) {
                    scope.a11yMessage += " " + scope.editor.content.name;
                    title += scope.editor.content.name;
                } else {
                    var name = editorState.current.contentTypeName;
                    scope.a11yMessage += " " + name;
                    scope.a11yName = name + " " + scope.a11yName;
                    title += name;
                }

                scope.$emit("$changeTitle", title);
            });
            scope.vm = {};
            scope.vm.hasVariants = false;
            scope.vm.hasSubVariants = false;
            scope.vm.hasCulture = false;
            scope.vm.hasSegments = false;
            scope.vm.dropdownOpen = false;
            scope.vm.variantsWithError = [];
            scope.vm.defaultVariant = null;
            scope.vm.errorsOnOtherVariants = false;// indicating wether to show that other variants, than the current, have errors.

            function updateVaraintErrors() {
                scope.content.variants.forEach(function (variant) {
                    variant.hasError = scope.variantHasError(variant);

                });
                checkErrorsOnOtherVariants();
            }

            function checkErrorsOnOtherVariants() {
                var check = false;
                scope.content.variants.forEach(function (variant) {
                    if (variant.active !== true && variant.hasError) {
                        check = true;
                    }
                });
                scope.vm.errorsOnOtherVariants = check;
            }

            function onVariantValidation(valid, errors, allErrors, culture, segment) {

                // only want to react to property errors:
                if (errors.findIndex(error => { return error.propertyAlias !== null; }) === -1) {
                    // we dont have any errors for properties, meaning we will back out.
                    return;
                }

                // If error coming back is invariant, we will assign the error to the default variant by picking the defaultVariant language.
                if (culture === "invariant" && scope.vm.defaultVariant) {
                    culture = scope.vm.defaultVariant.language.culture;
                }

                var index = scope.vm.variantsWithError.findIndex((item) => item.culture === culture && item.segment === segment)
                if (valid === true) {
                    if (index !== -1) {
                        scope.vm.variantsWithError.splice(index, 1);
                    }
                } else {
                    if (index === -1) {
                        scope.vm.variantsWithError.push({ "culture": culture, "segment": segment });
                    }
                }
                scope.$evalAsync(updateVaraintErrors);
            }

            function onInit() {
                // find default + check if we have variants.
                scope.content.variants.forEach(function (variant) {
                    if (variant.language !== null && variant.language.isDefault) {
                        scope.vm.defaultVariant = variant;
                    }
                    if (variant.language !== null) {
                        scope.vm.hasCulture = true;
                    }
                    if (variant.segment !== null) {
                        scope.vm.hasSegments = true;
                    }
                });

                scope.vm.hasVariants = scope.content.variants.length > 1 && (scope.vm.hasCulture || scope.vm.hasSegments);
                scope.vm.hasSubVariants = scope.content.variants.length > 1 &&(scope.vm.hasCulture && scope.vm.hasSegments);

                updateVaraintErrors();

                scope.vm.variantMenu = [];
                if (scope.vm.hasCulture) {
                    scope.content.variants.forEach((v) => {
                        if (v.language !== null && v.segment === null) {
                            const subVariants = scope.content.variants.filter((subVariant) => subVariant.language.culture === v.language.culture && subVariant.segment !== null).sort(contentEditingHelper.sortVariants);

                            var variantMenuEntry = {
                                key: String.CreateGuid(),
                                open: v.language && v.language.culture === scope.editor.culture,
                                variant: v,
                                subVariants
                            };
                            scope.vm.variantMenu.push(variantMenuEntry);
                        }
                    });
                } else {
                    scope.content.variants.forEach((v) => {
                        scope.vm.variantMenu.push({
                            key: String.CreateGuid(),
                            variant: v
                        });
                    });
                }

                scope.editor.variantApps.forEach((app) => {
                    // only render quick links on the content app if there are no tabs
                    if (app.alias === "umbContent") {
                        const hasTabs = scope.editor.content.tabs && scope.editor.content.tabs.filter(group => group.type === contentTypeHelper.TYPE_TAB).length > 0;
                        app.anchors = hasTabs ? [] : scope.editor.content.tabs;
                    }
                });

                scope.content.variants.forEach(function (variant) {

                    // if we are looking for the variant with default language then we also want to check for invariant variant.
                    if (variant.language && scope.vm.defaultVariant && variant.language.culture === scope.vm.defaultVariant.language.culture && variant.segment === null) {
                        unsubscribe.push(serverValidationManager.subscribe(null, "invariant", null, onVariantValidation, null));
                    }
                    unsubscribe.push(serverValidationManager.subscribe(null, variant.language !== null ? variant.language.culture : null, null, onVariantValidation, variant.segment));
                });
                
                scope.vm.variantMenu.sort(sortVariantsMenu);
            }

            function sortVariantsMenu (a, b) {
                return contentEditingHelper.sortVariants(a.variant, b.variant);
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

            scope.selectNavigationItem = function (item) {
                if (scope.onSelectNavigationItem) {
                    scope.onSelectNavigationItem({ "item": item });
                }
            }

            scope.selectAnchorItem = function (item, anchor) {
                if (scope.onSelectAnchorItem) {
                    scope.onSelectAnchorItem({ "item": item, "anchor": anchor });
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
            scope.variantHasError = function (variant) {
                if (scope.vm.variantsWithError.find((item) => (!variant.language || item.culture === variant.language.culture) && item.segment === variant.segment) !== undefined) {
                    return true;
                }
                return false;
            }

            scope.toggleDropdown = function () {
                scope.vm.dropdownOpen = !scope.vm.dropdownOpen;
                
                if (scope.vm.dropdownOpen) {
                    scope.vm.variantMenu.sort(sortVariantsMenu);
                }
            };

            unsubscribe.push(scope.$watch('splitViewOpen', (newVal) => {
                scope.vm.navigationItemLimit = newVal === true ? 0 : undefined;
            }));

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
                nameReadonly: "<?",
                menu: "=",
                hideActionsMenu: "<?",
                disableActionsMenu: "<?",
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
