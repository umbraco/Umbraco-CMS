angular.module("umbraco")
.controller("Umbraco.PropertyEditors.TagsController",
    function ($rootScope, $scope, $log, assetsService) {

        //load current value
        $scope.currentTags = [];
        if ($scope.model.value) {
            $scope.currentTags = $scope.model.value.split(",");
        }

        $scope.addTag = function(e) {
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

    }
);