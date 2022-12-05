/**
@ngdoc directive
@name umbraco.directives.directive:umbTagsEditor
**/

(function () {
    'use strict';

    angular
        .module('umbraco.directives')
        .component('umbTagsEditor', {
            transclude: true,
            templateUrl: 'views/components/tags/umb-tags-editor.html',
            controller: umbTagsEditorController,
            controllerAs: 'vm',
            bindings: {
                value: "<",
                config: "<",
                validation: "<",
                culture: "<?",
                inputId: "@?",
                onValueChanged: "&"
            }
        });

    function umbTagsEditorController($rootScope, assetsService, umbRequestHelper, angularHelper, $timeout, $element, $attrs) {

        let vm = this;

        let typeahead;
        let tagsHound;

        let initLoad = true;

        vm.$onInit = onInit;
        vm.$onChanges = onChanges;
        vm.$onDestroy = onDestroy;

        vm.validateMandatory = validateMandatory;
        vm.addTagOnEnter = addTagOnEnter;
        vm.addTag = addTag;
        vm.removeTag = removeTag;
        vm.showPrompt = showPrompt;
        vm.hidePrompt = hidePrompt;
        vm.onKeyUpOnTag = onKeyUpOnTag;

        vm.isLoading = true;
        vm.tagToAdd = "";
        vm.promptIsVisible = "-1";
        vm.viewModel = [];
        vm.readonly = false;

        $attrs.$observe('readonly', (value) => {
            vm.readonly = value !== undefined;
        });

        function onInit() {
            vm.inputId = vm.inputId || "t" + String.CreateGuid();

            assetsService.loadJs("lib/typeahead.js/typeahead.bundle.min.js").then(function () {

                vm.isLoading = false;

                //ensure that the models are formatted correctly
                configureViewModel(true);

                // Set the visible prompt to -1 to ensure it will not be visible
                vm.promptIsVisible = "-1";

                tagsHound = new Bloodhound({
                    initialize: false,
                    identify: function (obj) { return obj.id; },
                    datumTokenizer: Bloodhound.tokenizers.obj.whitespace('text'),
                    queryTokenizer: Bloodhound.tokenizers.whitespace,
                    //pre-fetch the tags for this category
                    prefetch: {
                        url: umbRequestHelper.getApiUrl("tagsDataBaseUrl", "GetTags", { tagGroup: vm.config.group, culture: vm.culture }),
                        //TTL = 5 minutes
                        ttl: 300000
                    },
                    //dynamically get the tags for this category (they may have changed on the server)
                    remote: {
                        url: umbRequestHelper.getApiUrl("tagsDataBaseUrl", "GetTags", { tagGroup: vm.config.group, culture: vm.culture, query: "%QUERY" }),
                        wildcard: "%QUERY"
                    }
                });

                tagsHound.initialize().then(function() {

                    //configure the type ahead
                    
                    var sources = {
                        //see: https://github.com/twitter/typeahead.js/blob/master/doc/jquery_typeahead.md#options
                        // name = the data set name, we'll make this the tag group name + culture
                        name: vm.config.group + (vm.culture ? vm.culture : ""),
                        display: "text",
                        //source: tagsHound
                        source: function (query, syncCallback, asyncCallback) {
                            tagsHound.search(query,
                                function(suggestions) {
                                    syncCallback(removeCurrentTagsFromSuggestions(suggestions));
                                }, function(suggestions) {
                                    asyncCallback(removeCurrentTagsFromSuggestions(suggestions));
                                });
                        }
                    };

                    var opts = {
                        hint: true,
                        highlight: true,
                        cacheKey: new Date(),  // Force a cache refresh each time the control is initialized
                        minLength: 1
                    };

                    typeahead = $element.find('.tags-' + vm.inputId).typeahead(opts, sources)
                        .bind("typeahead:selected", function (obj, datum, name) {
                            angularHelper.safeApply($rootScope, function () {
                                addTagInternal(datum["text"]);
                                vm.tagToAdd = "";
                                // clear the typed text
                                typeahead.typeahead('val', '');
                            });
                        }).bind("typeahead:autocompleted", function (obj, datum, name) {
                            angularHelper.safeApply($rootScope, function () {
                                addTagInternal(datum["text"]);
                                vm.tagToAdd = "";
                                // clear the typed text
                                typeahead.typeahead('val', '');
                            });

                        }).bind("typeahead:opened", function (obj) {

                        });

                });
                
            });
        }

        /**
         * Watch for value changes
         * @param {any} changes
         */
        function onChanges(changes) {

            //when the model 'value' changes, sync the viewModel object
            if (changes.value) {
                if (!changes.value.isFirstChange() && changes.value.currentValue !== changes.value.previousValue) {

                    configureViewModel();
                    reValidate();
                }
            }
        }

        function onDestroy() {
            if (tagsHound) {
                tagsHound.clearPrefetchCache();
                tagsHound.clearRemoteCache();
                tagsHound = null;
            }
            $element.find('.tags-' + vm.inputId).typeahead('destroy');
        }

        function configureViewModel(isInitLoad) {
            if (vm.value) {
                if (Utilities.isString(vm.value) && vm.value.length > 0) {
                    if (vm.config.storageType === "Json") {
                        //json storage
                        vm.viewModel = JSON.parse(vm.value);

                        //if this is the first load, we are just re-formatting the underlying model to be consistent
                        //we don't want to notify the component parent of any changes, that will occur if the user actually
                        //changes a value. If we notify at this point it will signal a form dirty change which we don't want.
                        if (!isInitLoad) {
                            updateModelValue(vm.viewModel);
                        }
                    }
                    else {
                        //csv storage

                        // split the csv string, and remove any duplicate values
                        let tempArray = vm.value.split(',').map(function (v) {
                            return v.trim();
                        });

                        vm.viewModel = tempArray.filter(function (v, i, self) {
                            return self.indexOf(v) === i;
                        });

                        //if this is the first load, we are just re-formatting the underlying model to be consistent
                        //we don't want to notify the component parent of any changes, that will occur if the user actually
                        //changes a value. If we notify at this point it will signal a form dirty change which we don't want.
                        if (!isInitLoad) {
                            updateModelValue(vm.viewModel);
                        }
                    }
                }
                else if (Utilities.isArray(vm.value)) {
                    vm.viewModel = vm.value;
                }
            }
        }

        function updateModelValue(val) {

            val = val ? val : [];

            vm.onValueChanged({ value: val });

            reValidate();
        }

        /**
         * Method required by the valPropertyValidator directive (returns true if the property editor has at least one tag selected)
         */
        function validateMandatory() {
            return {
                isValid: !vm.validation.mandatory || (vm.viewModel != null && vm.viewModel.length > 0)|| (vm.value != null && vm.value.length > 0),
                errorMsg: "Value cannot be empty",
                errorKey: "required"
            };
        }

        function addTagInternal(tagToAdd) {
            if (tagToAdd != null && tagToAdd.length > 0) {
                if (vm.viewModel.indexOf(tagToAdd) < 0) {
                    vm.viewModel.push(tagToAdd);
                    updateModelValue(vm.viewModel);
                }
            }
        }

        function addTagOnEnter(e) {
            var code = e.keyCode || e.which;
            if (code == 13) { //Enter keycode
                if ($element.find('.tags-' + vm.inputId).parent().find(".tt-menu .tt-cursor").length === 0) {
                    //this is required, otherwise the html form will attempt to submit.
                    e.preventDefault();
                    addTag();
                }
            }
        }
        function addTag() {
            //ensure that we're not pressing the enter key whilst selecting a typeahead value from the drop down
            //we need to use jquery because typeahead duplicates the text box
            addTagInternal(vm.tagToAdd);
            vm.tagToAdd = "";
            //this clears the value stored in typeahead so it doesn't try to add the text again
            // https://issues.umbraco.org/issue/U4-4947
            typeahead.typeahead('val', '');
        }

        function removeTag(tag) {
            var i = vm.viewModel.indexOf(tag);

            if (i >= 0) {
                // Make sure to hide the prompt so it does not stay open because another item gets a new number in the array index
                vm.promptIsVisible = "-1";

                // Remove the tag from the index
                vm.viewModel.splice(i, 1);

                updateModelValue(vm.viewModel);
            }
        }

        function showPrompt(idx, tag) {

            var i = vm.viewModel.indexOf(tag);

            // Make the prompt visible for the clicked tag only
            if (i === idx) {
                vm.promptIsVisible = i;
            }
        }

        function hidePrompt() {
            vm.promptIsVisible = "-1";
        }
        
        function onKeyUpOnTag(tag, $event) {
            if ($event.keyCode === 8 || $event.keyCode === 46) {
                removeTag(tag);
            }
        }
        
        // helper method to remove current tags
        function removeCurrentTagsFromSuggestions(suggestions) {
            return $.grep(suggestions, function (suggestion) {
                return ($.inArray(suggestion.text, vm.viewModel) === -1);
            });
        }

        function reValidate() {
            //this is required to re-validate for the mandatory validation
            if (vm.tagEditorForm && vm.tagEditorForm.tagCount) {
                vm.tagEditorForm.tagCount.$setViewValue(vm.viewModel.length);
            }
        }

    }

})();
