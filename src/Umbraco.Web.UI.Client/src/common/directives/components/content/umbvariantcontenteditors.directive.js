(function () {
    'use strict';

    /**
     * A component for split view content editing
     */
    var umbVariantContentEditors = {
        templateUrl: 'views/components/content/umb-variant-content-editors.html',
        bindings: {
            page: "<",
            content: "<",
            culture: "<",
            segment: "<",
            onSelectApp: "&?",
            onSelectAppAnchor: "&?",
            onBack: "&?",
            showBack: "<?"
        },
        controllerAs: 'vm',
        controller: umbVariantContentEditorsController
    };

    function umbVariantContentEditorsController($scope, $location, eventsService) {

        var prevContentDateUpdated = null;

        var vm = this;

        vm.$onInit = onInit;
        vm.$onChanges = onChanges;
        vm.$doCheck = doCheck;
        vm.$postLink = postLink;

        vm.openSplitView = openSplitView;
        vm.closeSplitView = closeSplitView;
        vm.selectVariant = selectVariant;
        vm.selectApp = selectApp;
        vm.selectAppAnchor = selectAppAnchor;
        vm.requestSplitView = requestSplitView;

        vm.getScope = getScope;// used by property editors to get a scope that is the root of split view, content apps etc.

        //Used to track how many content views there are (for split view there will be 2, it could support more in theory)
        vm.editors = [];

        /** Called when the component initializes */
        function onInit() {
            prevContentDateUpdated = Utilities.copy(vm.content.updateDate);
            setActiveVariant();
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
                setActiveVariant();
            } else if (changes.segment && !changes.segment.isFirstChange() && changes.segment.currentValue !== changes.segment.previousValue) {
                setActiveVariant();
            }
        }

        /** Allows us to deep watch whatever we want - executes on every digest cycle */
        function doCheck() {
            if (!Utilities.equals(vm.content.updateDate, prevContentDateUpdated)) {
                setActiveVariant();
                prevContentDateUpdated = Utilities.copy(vm.content.updateDate);
            }
        }

        /** This is called when the split view changes based on the umb-variant-content */
        function splitViewChanged() {
            //send an event downwards
            $scope.$broadcast("editors.content.splitViewChanged", { editors: vm.editors });
        }

        /**
         * Set the active variant based on the current culture or segment (query string)
         */
        function setActiveVariant() {
            // set the active variant
            var activeVariant = null;
            vm.content.variants.forEach(v => {
                if ((vm.culture === "invariant" || v.language && v.language.culture === vm.culture) && v.segment === vm.segment) {
                    activeVariant = v;
                }
            });

            if (!activeVariant) {
                // Set the first variant to active if we can't find it.
                // If the content item is invariant, then only one item exists in the array.
                activeVariant = vm.content.variants[0];
            }

            insertVariantEditor(0, activeVariant);

            if (vm.editors.length > 1) {
                //now re-sync any other editor content (i.e. if split view is open)
                for (var s = 1; s < vm.editors.length; s++) {
                    //get the variant from the scope model
                    var variant = vm.content.variants.find(v =>
                        (!v.language || v.language.culture === vm.editors[s].content.language.culture) && v.segment === vm.editors[s].content.segment);

                    vm.editors[s].content = variant;
                }
            }
            
            if (vm.content.variants.length > 1) {
                eventsService.emit('editors.content.cultureChanged', activeVariant.language);
            }
        }

        /**
         * Updates the editors collection for a given index for the specified variant
         * @param {any} index
         * @param {any} variant
         */
        function insertVariantEditor(index, variant) {

            if (vm.editors[index]) {
                if (vm.editors[index].content === variant) {
                    // This variant is already the content of the editor in this index.
                    return;
                }
                vm.editors[index].content.active = false;
            }
            variant.active = true;

            var variantCulture = variant.language ? variant.language.culture : "invariant";
            var variantSegment = variant.segment;

            var currentCulture = index < vm.editors.length ? vm.editors[index].culture : null;
            var currentSegment = index < vm.editors.length ? vm.editors[index].segment : null;
            
            // if index not already exists or if the culture or segment isnt identical then we do a replacement.
            if (index >= vm.editors.length || currentCulture !== variantCulture || currentSegment !== variantSegment) {

                //Not the current culture or segment which means we need to modify the array.
                //NOTE: It is not good enough to just replace the `content` object at a given index in the array
                // since that would mean that directives are not re-initialized.
                vm.editors.splice(index, 1, {
                    compositeId: variant.compositeId,
                    content: variant,
                    culture: variantCulture,
                    segment: variantSegment
                });
            }
            else {
                //replace the content of the editor, since the culture and segment is the same.
                vm.editors[index].content = variant;
            }
            
        }
        
        /**
         * Adds a new editor to the editors array to show content in a split view
         * @param {any} selectedVariant
         */
        function openSplitView(selectedVariant) {
            // enforce content contentApp in splitview.
            var contentApp = vm.content.apps.find(app => app.alias === "umbContent");
            if(contentApp) {
                selectApp(contentApp);
            }
            
            insertVariantEditor(vm.editors.length, selectedVariant);
            
            splitViewChanged();            
        }
        
        function requestSplitView(args) {
            var culture = args.culture;
            var segment = args.segment;

            var variant = vm.content.variants.find(v =>
                (!v.language || v.language.culture === culture) && v.segment === segment);

            if (variant != null) {
                openSplitView(variant);
            }
        }

        var unbindSplitViewRequest = eventsService.on("editors.content.splitViewRequest", (_, args) => requestSplitView(args));
        /** Closes the split view */
        function closeSplitView(editorIndex) {
            // TODO: hacking animation states - these should hopefully be easier to do when we upgrade angular
            var editor = vm.editors[editorIndex];
            vm.editors.splice(editorIndex, 1);
            editor.content.active = false;
            
            //update the current culture to reflect the last open variant (closing the split view corresponds to selecting the other variant)
            const culture = vm.editors[0].content.language ? vm.editors[0].content.language.culture : null;

            $location.search({"cculture": culture, "csegment": vm.editors[0].content.segment});
            splitViewChanged();
            unbindSplitViewRequest();
        }
        
        // if split view was never closed, the listener is not disposed when changing nodes - this unbinds it
        $scope.$on('$destroy', () => unbindSplitViewRequest());

        /**
         * Changes the currently selected variant
         * @param {any} variant This is the model of the variant/language drop down item in the editor header
         * @param {any} editorIndex The index of the editor being changed
         */
        function selectVariant(variant, editorIndex) {

            var variantCulture = variant.language ? variant.language.culture : "invariant";
            var variantSegment = variant.segment || null;
            
            // Check if we already have this editor open, if so, do nothing.
            if (vm.editors.find((editor) => (!editor.content.language || editor.content.language.culture === variantCulture) && editor.content.segment === variantSegment)) {
                return;
            }
            
            //if the editor index is zero, then update the query string to track the lang selection, otherwise if it's part
            //of a 2nd split view editor then update the model directly.
            if (editorIndex === 0) {
                //If we've made it this far, then update the query string.
                //The editor will respond to this query string changing.
                $location.search("cculture", variantCulture).search("csegment", variantSegment);
            }
            else {
                //update the editors collection
                insertVariantEditor(editorIndex, variant);                
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
        function getScope() {
            return $scope;
        }

    }

    angular.module('umbraco.directives').component('umbVariantContentEditors', umbVariantContentEditors);

})();
