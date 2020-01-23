(function () {
    'use strict';

    function EditorContentHeader(serverValidationManager, localizationService, editorState, variantHelper) {

        function link(scope) {
            
            var unsubscribe = [];
            
            if (!scope.serverValidationNameField) {
                scope.serverValidationNameField = "Name";
            }
            if (!scope.serverValidationAliasField) {
                scope.serverValidationAliasField = "Alias";
            }

            scope.isNew = scope.content.state == "NotCreated";

            localizationService.localizeMany([
                    scope.isNew ? "placeholders_a11yCreateItem" : "placeholders_a11yEdit",
                    "placeholders_a11yName"]
            ).then(function (data) {
                scope.a11yMessage = data[0];
                scope.a11yName = data[1];
                if (!scope.isNew) {
                    scope.a11yMessage += " " + scope.content.name;

                } else {
                    var name = editorState.current.contentTypeName;
                    scope.a11yMessage += " " + name;
                    scope.a11yName = name + " " + scope.a11yName;
                }
            });
            scope.vm = {};
            scope.vm.dropdownOpen = false;
            scope.vm.currentVariant = "";
            scope.vm.variantsWithError = [];
            scope.vm.defaultVariant = null;
            
            scope.vm.errorsOnOtherVariants = false;// indicating wether to show that other variants, than the current, have errors.
            
            function checkErrorsOnOtherVariants() {
                var check = false;
                angular.forEach(scope.content.variants, function (variant) {
                    // SEGMENTS_TODO: Check that this correction is okay, can we even see the active var here?
                    if (variant.active !== true && scope.variantHasError(variant)) {
                        check = true;
                    }
                });
                scope.vm.errorsOnOtherVariants = check;
            }
            
            function onVariantValidation(valid, errors, allErrors, culture, segment) {
                // SEGMENTS_TODO: See wether we can use errors, allErrors?
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
                
                // find default.
                angular.forEach(scope.content.variants, function (variant) {
                    if (variant.language !== null && variant.language.isDefault) {
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
                    unsubscribe.push(serverValidationManager.subscribe(null, variant.language !== null ? variant.language.culture : null, variant.segment, null, onVariantValidation));
                });
                
                unsubscribe.push(serverValidationManager.subscribe(null, null, null, null, onVariantValidation));
                
                
                
            }

            function setCurrentVariant() {
                angular.forEach(scope.content.variants, function (variant) {
                    if (variant.active) {
                        scope.vm.currentVariant = variant;
                        checkErrorsOnOtherVariants();
                    }
                });
            }

            function getCultureFromVariant(variant) {
                return variant.language ? variant.language.culture : null;
            }

            scope.getVariantDisplayName = variantHelper.getDisplayName;

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
             * @param {any} variant
             */
            scope.variantIsOpen = function (variant) {

                // SEGMENTS_TODO: ... does this work?
                if (scope.content.variants.find((v) => variant.active === true && (getCultureFromVariant(v) === getCultureFromVariant(variant)) && variant.segment === variant.segment)) {
                    console.log("VARIANT IS OPEN")
                    return;
                }
                console.log("VARIANT IS closed", scope.content.variants)
            }
            
            /**
             * Check whether a variant has a error, used to display errors in variant switcher.
             * @param {any} culture
             */
            scope.variantHasError = function(variant) {
                // if we are looking for the default language we also want to check for invariant.
                if (variant.language.culture === scope.vm.defaultVariant.language.culture && variant.segment === null) {
                    if(scope.vm.variantsWithError.find((item) => item.culture === "invariant" && item.segment === null) !== undefined) {
                        return true;
                    }
                }
                if(scope.vm.variantsWithError.find((item) => item.culture === variant.language.culture && item.segment === variant.segment) !== undefined) {
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
