angular.module("umbraco")
.controller("Umbraco.PropertyEditors.TagsController",
    function ($rootScope, $scope, $log, assetsService, umbRequestHelper) {
        
        //load current value
        $scope.currentTags = [];
        if ($scope.model.value) {
            $scope.currentTags = $scope.model.value.split(",");
        }

        $scope.addTag = function (e) {
            var code = e.keyCode || e.which;
            if (code == 13) { //Enter keycode   

                //this is required, otherwise the html form will attempt to submit.
                e.preventDefault();

                if ($scope.currentTags.indexOf($scope.tagToAdd) < 0) {
                    $scope.currentTags.push($scope.tagToAdd);
                }
                $scope.tagToAdd = "";
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

        assetsService.loadJs("lib/typeahead/typeahead.bundle.min.js").then(function () {
            
            //configure the tags data source

            var tagsHound = new Bloodhound({
                datumTokenizer: Bloodhound.tokenizers.obj.whitespace("value"),
                queryTokenizer: Bloodhound.tokenizers.whitespace,
                //pre-fetch the tags for this category
                prefetch: {
                    url: umbRequestHelper.getApiUrl("tagsDataBaseUrl", "GetTags", [{ tagGroup: $scope.model.config.group }]),
                    //TTL = 5 minutes
                    ttl: 300000,
                    filter: function (list) { 
                        return _.map(list, function (i) {
                            return { value: i.text };
                        });
                    }
                },
                //dynamically get the tags for this category
                remote: {
                    url: umbRequestHelper.getApiUrl("tagsDataBaseUrl", "GetTags", [{ tagGroup: $scope.model.config.group }]),
                    filter: function (list) {
                        return _.map(list, function (i) {
                            return { value: i.text };
                        });
                    }
                }
            });

            tagsHound.initialize();

            //configure the type ahead

            $('#tags-' + $scope.model.alias).typeahead(
                //use the default options
                null, {
                //see: https://github.com/twitter/typeahead.js/blob/master/doc/jquery_typeahead.md#options
                // name = the data set name, we'll make this the tag group name
                name: $scope.model.config.group,
                // apparently thsi should be the same as the value above in the call to Bloodhound.tokenizers.obj.whitespace
                // this isn't very clear in the docs but you can see that it's consistent with this statement here:
                // http://twitter.github.io/typeahead.js/examples/
                displayKey: "value",
                source: tagsHound.ttAdapter()
            });

        });

        //on destroy:
        // $('.typeahead').typeahead('destroy');

    }
);