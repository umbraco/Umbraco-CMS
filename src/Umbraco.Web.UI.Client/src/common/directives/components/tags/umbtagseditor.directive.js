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
                onValueChanged: "&"
            }
        });

    function umbTagsEditorController($rootScope, assetsService, umbRequestHelper, angularHelper, $timeout, $element) {

        var vm = this;

        var typeahead;
        var tagsHound;

        vm.$onInit = onInit;
        vm.$onChanges = onChanges;
        vm.$onDestroy = onDestroy;

        vm.validateMandatory = validateMandatory;
        vm.addTagOnEnter = addTagOnEnter;
        vm.addTag = addTag;
        vm.removeTag = removeTag;
        vm.showPrompt = showPrompt;
        vm.hidePrompt = hidePrompt;

        vm.htmlId = "t" + String.CreateGuid();
        vm.isLoading = true;
        vm.tagToAdd = "";
        vm.promptIsVisible = "-1";
        vm.viewModel = [];

        function onInit() {

            assetsService.loadJs("lib/typeahead.js/typeahead.bundle.min.js").then(function () {

                vm.isLoading = false;

                configureViewModel();

                // Set the visible prompt to -1 to ensure it will not be visible
                vm.promptIsVisible = "-1";

                tagsHound = new Bloodhound({
                    datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
                    queryTokenizer: Bloodhound.tokenizers.whitespace,
                    //pre-fetch the tags for this category
                    prefetch: {
                        url: umbRequestHelper.getApiUrl("tagsDataBaseUrl", "GetTags", { tagGroup: vm.config.group, culture: vm.culture }),
                        //TTL = 5 minutes
                        ttl: 300000,
                        transform: dataTransform
                    },
                    //dynamically get the tags for this category (they may have changed on the server)
                    remote: {
                        url: umbRequestHelper.getApiUrl("tagsDataBaseUrl", "GetTags", { tagGroup: vm.config.group, culture: vm.culture }),
                        transform: dataTransform
                    }
                });

                tagsHound.initialize(true);

                //configure the type ahead
                $timeout(function () {

                    var sources = {
                        //see: https://github.com/twitter/typeahead.js/blob/master/doc/jquery_typeahead.md#options
                        // name = the data set name, we'll make this the tag group name
                        name: vm.config.group,
                        display: "value",
                        //source: tagsHound
                        source: function (query, cb) {
                            tagsHound.search(query,
                                function(suggestions) {
                                    cb(removeCurrentTagsFromSuggestions(suggestions));
                                });
                        }
                    };

                    var opts = {
                        //This causes some strangeness as it duplicates the textbox, best leave off for now.
                        hint: false,
                        highlight: true,
                        cacheKey: new Date(),  // Force a cache refresh each time the control is initialized
                        minLength: 1
                    };

                    typeahead = $element.find('.tags-' + vm.htmlId).typeahead(opts, sources)
                        .bind("typeahead:selected", function (obj, datum, name) {
                            angularHelper.safeApply($rootScope, function () {
                                addTagInternal(datum["value"]);
                                vm.tagToAdd = "";
                                // clear the typed text
                                typeahead.typeahead('val', '');
                            });
                        }).bind("typeahead:autocompleted", function (obj, datum, name) {
                            angularHelper.safeApply($rootScope, function () {
                                addTagInternal(datum["value"]);
                                vm.tagToAdd = "";
                            });

                        }).bind("typeahead:opened", function (obj) {
                            console.log("opened ");
                        });
                });

            });
        }

        function onChanges(changes) {

            // watch for value changes externally
            if (changes.value) {
                if (!changes.value.isFirstChange() && changes.value.currentValue !== changes.value.previousValue) {

                    configureViewModel();
                    //this is required to re-validate
                    vm.tagEditorForm.tagCount.$setViewValue(vm.viewModel.length);

                }
            }
        }

        function onDestroy() {
            if (tagsHound) {
                tagsHound.clearPrefetchCache();
                tagsHound.clearRemoteCache();
                tagsHound = null;
            }
            $element.find('.tags-' + vm.htmlId).typeahead('destroy');
        }

        function configureViewModel() {
            if (vm.value) {
                if (angular.isString(vm.value) && vm.value.length > 0) {
                    if (vm.config.storageType === "Json") {
                        //json storage
                        vm.viewModel = JSON.parse(vm.value);
                        updateModelValue(vm.viewModel);
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

                        updateModelValue(vm.viewModel);
                    }
                }
                else if (angular.isArray(vm.value)) {
                    vm.viewModel = vm.value;
                }
            }
        }

        function updateModelValue(val) {
            if (val) {
                vm.onValueChanged({ value: val });
            }
            else {
                vm.onValueChanged({ value: [] });
            }
            
            //this is required to re-validate
            $scope.propertyForm.tagCount.$setViewValue($scope.model.value.length);
        }

        /**
         * Method required by the valPropertyValidator directive (returns true if the property editor has at least one tag selected)
         */
        function validateMandatory() {
            return {
                isValid: !vm.validation.mandatory || (vm.viewModel != null && vm.viewModel.length > 0),
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
                if ($element.find('.tags-' + vm.htmlId).parent().find(".tt-menu .tt-cursor").length === 0) {
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

        //helper method to format the data for bloodhound
        function dataTransform(list) {
            //transform the result to what bloodhound wants
            var tagList = _.map(list, function (i) {
                return { value: i.text };
            });
            // remove current tags from the list
            return $.grep(tagList, function (tag) {
                return ($.inArray(tag.value, vm.viewModel) === -1);
            });
        }

        // helper method to remove current tags
        function removeCurrentTagsFromSuggestions(suggestions) {
            return $.grep(suggestions, function (suggestion) {
                return ($.inArray(suggestion.value, vm.viewModel) === -1);
            });
        }


    }

})();
