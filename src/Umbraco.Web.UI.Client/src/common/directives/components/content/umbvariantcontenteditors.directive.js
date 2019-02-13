(function () {
    'use strict';

    /**
     * A component for split view content editing
     */
    var umbVariantContentEditors = {
        templateUrl: 'views/components/content/umb-variant-content-editors.html',
        bindings: {
            page: "<",
            content: "<", // TODO: Not sure if this should be = since we are changing the 'active' property of a variant
            culture: "<",
            onSelectApp: "&?",
            onSelectAppAnchor: "&?",
            onBack: "&?",
            showBack: "<?"
        },
        controllerAs: 'vm',
        controller: umbVariantContentEditorsController
    };

    function umbVariantContentEditorsController($scope, $location, $timeout) {

        var prevContentDateUpdated = null;

        var vm = this;
        var activeAppAlias = null;

        vm.$onInit = onInit;
        vm.$onChanges = onChanges;
        vm.$doCheck = doCheck;
        vm.$postLink = postLink;

        vm.openSplitView = openSplitView;
        vm.closeSplitView = closeSplitView;
        vm.selectVariant = selectVariant;
        vm.selectApp = selectApp;
        vm.selectAppAnchor = selectAppAnchor;

        //Used to track how many content views there are (for split view there will be 2, it could support more in theory)
        vm.editors = [];
        //Used to track the open variants across the split views
        vm.openVariants = [];

        /** Called when the component initializes */
        function onInit() {
            prevContentDateUpdated = angular.copy(vm.content.updateDate);
            setActiveCulture();
        }

        /** Called when the component has linked all elements, this is when the form controller is available */
        function postLink() {

        }

        /**
         * Watch for model changes
         * @param {any} changes
         */
        function onChanges(changes) {

            if (changes.culture && !changes.culture.isFirstChange() && changes.culture.currentValue !== changes.culture.previousValue) {
                setActiveCulture();
            }
        }

        /** Allows us to deep watch whatever we want - executes on every digest cycle */
        function doCheck() {
            if (!angular.equals(vm.content.updateDate, prevContentDateUpdated)) {
                setActiveCulture();
                prevContentDateUpdated = angular.copy(vm.content.updateDate);
            }
        }

        /** This is called when the split view changes based on the umb-variant-content */
        function splitViewChanged() {
            //send an event downwards
            $scope.$broadcast("editors.content.splitViewChanged", { editors: vm.editors });
        }

        /**
         * Set the active variant based on the current culture (query string)
         */
        function setActiveCulture() {
            // set the active variant
            var activeVariant = null;
            _.each(vm.content.variants, function (v) {
                if (v.language && v.language.culture === vm.culture) {
                    v.active = true;
                    activeVariant = v;
                }
                else {
                    v.active = false;
                }
            });
            if (!activeVariant) {
                // Set the first variant to active if we can't find it.
                // If the content item is invariant, then only one item exists in the array.
                vm.content.variants[0].active = true;
                activeVariant = vm.content.variants[0];
            }

            insertVariantEditor(0, initVariant(activeVariant, 0));

            if (vm.editors.length > 1) {
                //now re-sync any other editor content (i.e. if split view is open)
                for (var s = 1; s < vm.editors.length; s++) {
                    //get the variant from the scope model
                    var variant = _.find(vm.content.variants, function (v) {
                        return v.language.culture === vm.editors[s].content.language.culture;
                    });
                    vm.editors[s].content = initVariant(variant, s);
                }
            }

        }

        /**
         * Updates the editors collection for a given index for the specified variant
         * @param {any} index
         * @param {any} variant
         */
        function insertVariantEditor(index, variant) {

            var variantCulture = variant.language ? variant.language.culture : "invariant";

            //check if the culture at the index is the same, if it's null an editor will be added
            var currentCulture = vm.editors.length === 0 || vm.editors.length <= index ? null : vm.editors[index].culture;

            if (currentCulture !== variantCulture) {
                //Not the current culture which means we need to modify the array.
                //NOTE: It is not good enough to just replace the `content` object at a given index in the array
                // since that would mean that directives are not re-initialized.
                vm.editors.splice(index, 1, {
                    content: variant,
                    //used for "track-by" ng-repeat
                    culture: variantCulture
                });
            }
            else {
                //replace the editor for the same culture
                vm.editors[index].content = variant;
            }
        }

        function initVariant(variant, editorIndex) {
            //The model that is assigned to the editor contains the current content variant along
            //with a copy of the contentApps. This is required because each editor renders it's own
            //header and content apps section and the content apps contains the view for editing content itself
            //and we need to assign a view model to the subView so that it is scoped to the current
            //editor so that split views work.

            //copy the apps from the main model if not assigned yet to the variant
            if (!variant.apps) {
                variant.apps = angular.copy(vm.content.apps);
            }

            //if this is a variant has a culture/language than we need to assign the language drop down info 
            if (variant.language) {
                //if the variant list that defines the header drop down isn't assigned to the variant then assign it now
                if (!variant.variants) {
                    variant.variants = _.map(vm.content.variants,
                        function (v) {
                            return _.pick(v, "active", "language", "state");
                        });
                }
                else {
                    //merge the scope variants on top of the header variants collection (handy when needing to refresh)
                    angular.extend(variant.variants,
                        _.map(vm.content.variants,
                            function (v) {
                                return _.pick(v, "active", "language", "state");
                            }));
                }

                //ensure the current culture is set as the active one
                for (var i = 0; i < variant.variants.length; i++) {
                    if (variant.variants[i].language.culture === variant.language.culture) {
                        variant.variants[i].active = true;
                    }
                    else {
                        variant.variants[i].active = false;
                    }
                }

                // keep track of the open variants across the different split views
                // push the first variant then update the variant index based on the editor index
                if(vm.openVariants && vm.openVariants.length === 0) {
                    vm.openVariants.push(variant.language.culture);
                } else {
                    vm.openVariants[editorIndex] = variant.language.culture;
                }

            }

            //then assign the variant to a view model to the content app
            var contentApp = _.find(variant.apps, function (a) {
                return a.alias === "umbContent";
            });

            //The view model for the content app is simply the index of the variant being edited
            var variantIndex = vm.content.variants.indexOf(variant);
            contentApp.viewModel = variantIndex;

            // make sure the same app it set to active in the new variant
            if(activeAppAlias) {
                angular.forEach(variant.apps, function(app) {
                    app.active = false;
                    if(app.alias === activeAppAlias) {
                        app.active = true;
                    }
                });
            }

            return variant;
        }
        /**
         * Adds a new editor to the editors array to show content in a split view
         * @param {any} selectedVariant
         */
        function openSplitView(selectedVariant) {
            var selectedCulture = selectedVariant.language.culture;

            //Find the whole variant model based on the culture that was chosen
            var variant = _.find(vm.content.variants, function (v) {
                return v.language.culture === selectedCulture;
            });

            insertVariantEditor(vm.editors.length, initVariant(variant, vm.editors.length));

            //only the content app can be selected since no other apps are shown, and because we copy all of these apps
            //to the "editors" we need to update this across all editors
            for (var e = 0; e < vm.editors.length; e++) {
                var editor = vm.editors[e];
                for (var i = 0; i < editor.content.apps.length; i++) {
                    var app = editor.content.apps[i];
                    if (app.alias === "umbContent") {
                        app.active = true;
                        // tell the world that the app has changed (but do it only once)
                        if (e === 0) {
                            selectApp(app);
                        }
                    }
                    else {
                        app.active = false;
                    }
                }
            }

            // TODO: hacking animation states - these should hopefully be easier to do when we upgrade angular
            editor.collapsed = true;
            editor.loading = true;
            $timeout(function () {
                editor.collapsed = false;
                editor.loading = false;
                splitViewChanged();
            }, 100);
        }

        /** Closes the split view */
        function closeSplitView(editorIndex) {
            // TODO: hacking animation states - these should hopefully be easier to do when we upgrade angular
            var editor = vm.editors[editorIndex];
            editor.loading = true;
            editor.collapsed = true;
            $timeout(function () {
                vm.editors.splice(editorIndex, 1);
                //remove variant from open variants
                vm.openVariants.splice(editorIndex, 1);
                //update the current culture to reflect the last open variant (closing the split view corresponds to selecting the other variant)
                $location.search("cculture", vm.openVariants[0]);
                splitViewChanged();
            }, 400);
        }

        /**
         * Changes the currently selected variant
         * @param {any} variant This is the model of the variant/language drop down item in the editor header
         * @param {any} editorIndex The index of the editor being changed
         */
        function selectVariant(variant, editorIndex) {

            // prevent variants already open in a split view to be opened
            if(vm.openVariants.indexOf(variant.language.culture) !== -1)  {
                return;
            }
            
            //if the editor index is zero, then update the query string to track the lang selection, otherwise if it's part
            //of a 2nd split view editor then update the model directly.
            if (editorIndex === 0) {
                //If we've made it this far, then update the query string.
                //The editor will respond to this query string changing.
                $location.search("cculture", variant.language.culture);
            }
            else {

                //Update the 'active' variant for this editor
                var editor = vm.editors[editorIndex];
                //set all variant drop down items as inactive for this editor and then set the selected one as active
                for (var i = 0; i < editor.content.variants.length; i++) {
                    editor.content.variants[i].active = false;
                }
                variant.active = true;

                //get the variant content model and initialize the editor with that
                var contentVariant = _.find(vm.content.variants,
                    function (v) {
                        return v.language.culture === variant.language.culture;
                    });
                editor.content = initVariant(contentVariant, editorIndex);

                //update the editors collection
                insertVariantEditor(editorIndex, contentVariant);
                
            }
        }

        /**
         * Stores the active app in a variable so we can remember it when changing language
         * @param {any} app This is the model of the selected app
         */
        function selectApp(app) {
            if(vm.onSelectApp) {
                vm.onSelectApp({"app": app});
            }
        }
        
        function selectAppAnchor(app, anchor) {
            if(vm.onSelectAppAnchor) {
                vm.onSelectAppAnchor({"app": app, "anchor": anchor});
            }
        }
        
        
        $scope.$on("editors.apps.appChanged", function($event, $args) {
            var app = $args.app;
            if(app && app.alias) {
                activeAppAlias = app.alias;
            }
        });

    }

    angular.module('umbraco.directives').component('umbVariantContentEditors', umbVariantContentEditors);

})();
