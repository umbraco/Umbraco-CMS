angular.module("umbraco")
.controller("Umbraco.PropertyEditors.TagsController",
    function ($rootScope, $scope, $log, assetsService, umbRequestHelper, angularHelper, $timeout, $element) {

        $scope.isLoading = true;
        $scope.tagToAdd = "";

        assetsService.loadJs("lib/typeahead/typeahead.bundle.min.js").then(function () {

            $scope.isLoading = false;

            //load current value
            $scope.currentTags = [];
            if ($scope.model.value) {
                $scope.currentTags = $scope.model.value.split(",");
            }

            //Helper method to add a tag on enter or on typeahead select
            function addTag(tagToAdd) {
                if (tagToAdd.length > 0) {
                    if ($scope.currentTags.indexOf(tagToAdd) < 0) {
                        $scope.currentTags.push(tagToAdd);
                    }
                }
            }

            $scope.addTag = function (e) {
                var code = e.keyCode || e.which;
                if (code == 13) { //Enter keycode   

                    //ensure that we're not pressing the enter key whilst selecting a typeahead value from the drop down
                    if ($element.find('.tags-' + $scope.model.alias).parent().find(".tt-dropdown-menu .tt-cursor").length === 0) {
                        //this is required, otherwise the html form will attempt to submit.
                        e.preventDefault();
                        //we need to use jquery because typeahead duplicates the text box
                        addTag($scope.tagToAdd);
                        $scope.tagToAdd = "";
                    }

                }
            };

            $scope.removeTag = function (tag) {
                var i = $scope.currentTags.indexOf(tag);
                if (i >= 0) {
                    $scope.currentTags.splice(i, 1);
                }
            };

            //sync model on submit (needed since we convert an array to string)	
            $scope.$on("formSubmitting", function (ev, args) {
                $scope.model.value = $scope.currentTags.join();
            });

            //vice versa
            $scope.model.onValueChanged = function (newVal, oldVal) {
                //update the display val again if it has changed from the server
                $scope.model.val = newVal;
                $scope.currentTags = $scope.model.value.split(",");
            };

            //configure the tags data source
            //TODO: We'd like to be able to filter the shown list items to not show the tags that are currently
            // selected but that is difficult, i've tried a number of things and also this link suggests we cannot do 
            // it currently without a lot of hacking:
            // http://stackoverflow.com/questions/21044906/twitter-typeahead-js-remove-datum-upon-selection

            //helper method to format the data for bloodhound
            function dataTransform(list) {
                //transform the result to what bloodhound wants
                return _.map(list, function (i) {
                    return { value: i.text };
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

            tagsHound.initialize();

            //configure the type ahead
            $timeout(function() {
                $element.find('.tags-' + $scope.model.alias).typeahead(
                {
                    //This causes some strangeness as it duplicates the textbox, best leave off for now.
                    hint: false,
                    highlight: true,
                    minLength: 1
                }, {
                    //see: https://github.com/twitter/typeahead.js/blob/master/doc/jquery_typeahead.md#options
                    // name = the data set name, we'll make this the tag group name
                    name: $scope.model.config.group,
                    displayKey: "value",
                    source: tagsHound.ttAdapter(),
                }).bind("typeahead:selected", function (obj, datum, name) {
                    angularHelper.safeApply($scope, function () {
                        addTag(datum["value"]);
                        $scope.tagToAdd = "";
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
                $element.find('.tags-' + $scope.model.alias).typeahead('destroy');
                delete tagsHound;
            });

        });

    }
);