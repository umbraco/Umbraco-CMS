'use strict';

(function() {
    
    function csvEditorController($scope, $http, $filter) {

        var values = [];

        //this will be comma delimited
        if ($scope.model && $scope.model.value && (typeof $scope.model.value == "string")) {
            var splitVals = $scope.model.value.split(",");
            //set the values of our object
            for (var i = 0; i < splitVals.length; i++) {
                values.push({
                    index: i,
                    value: splitVals[i].trim()
                });
            }
        }

        //if there was no data then initialize with 5 values... we should configure that in pre-vals
        if (values.length == 0) {
            for (var x = 0; x < 5; x++) {
                values.push({ index: x, value: "" });
            }
        }

        //set the scope values to bind on our view to the new object.
        $scope.values = values;

        //set up listeners for the object to write back to our comma delimited property value
        $scope.$watch('values', function (newValue, oldValue) {
            var csv = [];
            for (var v in newValue) {
                csv.push(newValue[v].value);
            }
            //write the csv value back to the property
            $scope.model.value = csv.join();
        }, true);
    };

    angular.module("myPackage.controllers").controller('MyPackage.PropertyEditors.CsvEditorController', csvEditorController);
})();