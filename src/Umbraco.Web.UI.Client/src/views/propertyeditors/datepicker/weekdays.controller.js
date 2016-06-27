angular.module("umbraco").controller("Umbraco.PrevalueEditors.WeekdaysController",
    function ($scope, $timeout, assetsService, angularHelper, $element) {
        
        $scope.weekdays = [
            { id: 1, name: 'Monday' },
            { id: 2, name: 'Tuesday' },
            { id: 3, name: 'Wednesday' },
            { id: 4, name: 'Thursday' },
            { id: 5, name: 'Friday' },
            { id: 6, name: 'Saturday' },
            { id: 0, name: 'Sunday' }
        ];

        $scope.selectedDays = $scope.model.value || [];

        if (!angular.isArray($scope.model.value)) {
            //make an array from the dictionary
            var items = [];
            for (var i in $scope.model.value) {
                items.push($scope.model.value[i]);
            }
            //now make the editor model the array
            $scope.model.value = items;
        }

    });