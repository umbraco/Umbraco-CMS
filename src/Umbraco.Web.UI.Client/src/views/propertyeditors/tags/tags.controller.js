angular.module("umbraco")
.controller("Umbraco.PropertyEditors.TagsController",
    function ($rootScope, $scope, $log, assetsService, umbRequestHelper, angularHelper, $timeout, $element) {

        var $typeahead;

        $scope.isLoading = true;
        $scope.tagToAdd = "";

        assetsService.loadJs("lib/typeahead-js/typeahead.bundle.min.js").then(function () {

            $scope.isLoading = false;

            //load current value

            if ($scope.model.value) {
                if (!$scope.model.config.storageType || $scope.model.config.storageType !== "Json") {
                    //it is csv
                    if (!$scope.model.value) {
                        $scope.model.value = [];
                    }
                    else {
                        $scope.model.value = $scope.model.value.split(",");
                    }
                }
            }
            else {
                $scope.model.value = [];
            }

            // Method required by the valPropertyValidator directive (returns true if the property editor has at least one tag selected)
            $scope.validateMandatory = function () {
                return {
                    isValid: !$scope.model.validation.mandatory || ($scope.model.value != null && $scope.model.value.length > 0),
                    errorMsg: "Value cannot be empty",
                    errorKey: "required"
                };
            }

            //Helper method to add a tag on enter or on typeahead select
            function addTag(tagToAdd) {
                if (tagToAdd != null && tagToAdd.length > 0) {
                    if ($scope.model.value.indexOf(tagToAdd) < 0) {
                        $scope.model.value.push(tagToAdd);
                        //this is required to re-validate
                        $scope.propertyForm.tagCount.$setViewValue($scope.model.value.length);
                    }
                }
            }

            $scope.addTagOnEnter = function (e) {
                var code = e.keyCode || e.which;
                if (code == 13) { //Enter keycode   
                    if ($element.find('.tags-' + $scope.model.alias).parent().find(".tt-dropdown-menu .tt-cursor").length === 0) {
                        //this is required, otherwise the html form will attempt to submit.
                        e.preventDefault();
                        $scope.addTag();
                    }
                }
            };

            $scope.addTag = function () {
                //ensure that we're not pressing the enter key whilst selecting a typeahead value from the drop down
                //we need to use jquery because typeahead duplicates the text box
                addTag($scope.tagToAdd);
                $scope.tagToAdd = "";
                //this clears the value stored in typeahead so it doesn't try to add the text again
                // http://issues.umbraco.org/issue/U4-4947
                $typeahead.typeahead('val', '');
            };



            $scope.removeTag = function (tag) {
                var i = $scope.model.value.indexOf(tag);
                if (i >= 0) {
                    $scope.model.value.splice(i, 1);
                    //this is required to re-validate
                    $scope.propertyForm.tagCount.$setViewValue($scope.model.value.length);
                }
            };

            //vice versa
            $scope.model.onValueChanged = function (newVal, oldVal) {
                //update the display val again if it has changed from the server
                $scope.model.value = newVal;

                if (!$scope.model.config.storageType || $scope.model.config.storageType !== "Json") {
                    //it is csv
                    if (!$scope.model.value) {
                        $scope.model.value = [];
                    }
                    else {
                        $scope.model.value = $scope.model.value.split(",");
                    }
                }
            };

            //configure the tags data source

            //helper method to format the data for bloodhound
            function dataTransform(list) {
                //transform the result to what bloodhound wants
                var tagList = _.map(list, function (i) {
                    return { value: i.text };
                });
                // remove current tags from the list
                return $.grep(tagList, function (tag) {
                    return ($.inArray(tag.value, $scope.model.value) === -1);
                });
            }

            // helper method to remove current tags
            function removeCurrentTagsFromSuggestions(suggestions) {
                return $.grep(suggestions, function (suggestion) {
                    return ($.inArray(suggestion.value, $scope.model.value) === -1);
                });
            }

            var tagsHound = new Bloodhound({
                datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
                queryTokenizer: Bloodhound.tokenizers.whitespace,
                dupDetector : function(remoteMatch, localMatch) {
                    return (remoteMatch["value"] == localMatch["value"]);
                },
                //pre-fetch the tags for this category
                prefetch: {
                    url: umbRequestHelper.getApiUrl("tagsDataBaseUrl", "GetTags", [{ tagGroup: $scope.model.config.group }]),
                    //TTL = 5 minutes
                    ttl: 300000,
                    filter: dataTransform
                },
                //dynamically get the tags for this category (they may have changed on the server)
                remote: {
                    url: umbRequestHelper.getApiUrl("tagsDataBaseUrl", "GetTags", [{ tagGroup: $scope.model.config.group }]),
                    filter: dataTransform
                }
            });

            tagsHound.initialize(true);

            //configure the type ahead
            $timeout(function () {

                $typeahead = $element.find('.tags-' + $scope.model.alias).typeahead(
                {
                    //This causes some strangeness as it duplicates the textbox, best leave off for now.
                    hint: false,
                    highlight: true,
                    cacheKey: new Date(),  // Force a cache refresh each time the control is initialized
                    minLength: 1
                }, {
                    //see: https://github.com/twitter/typeahead.js/blob/master/doc/jquery_typeahead.md#options
                    // name = the data set name, we'll make this the tag group name
                    name: $scope.model.config.group,
                    displayKey: "value",
                    source: function (query, cb) {
                        tagsHound.get(query, function (suggestions) {
                            cb(removeCurrentTagsFromSuggestions(suggestions));
                        });
                    },
                }).bind("typeahead:selected", function (obj, datum, name) {
                    angularHelper.safeApply($scope, function () {
                        addTag(datum["value"]);
                        $scope.tagToAdd = "";
                        // clear the typed text
                        $typeahead.typeahead('val', '');
                    });

                }).bind("typeahead:autocompleted", function (obj, datum, name) {
                    angularHelper.safeApply($scope, function () {
                        addTag(datum["value"]);
                        $scope.tagToAdd = "";
                    });

                }).bind("typeahead:opened", function (obj) {
                    //console.log("opened ");
                });
            });

            $scope.$on('$destroy', function () {
                tagsHound.clearPrefetchCache();
                tagsHound.clearRemoteCache();
                $element.find('.tags-' + $scope.model.alias).typeahead('destroy');
                delete tagsHound;
            });

        });

    }
);