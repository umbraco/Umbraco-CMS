angular.module("umbraco").controller("Umbraco.PrevalueEditors.WeekdaysController",
    function ($scope, $timeout, assetsService, angularHelper, $element) {
        
        $scope.weekdays = [
            { id: 0, name: 'Monday' },
            { id: 1, name: 'Tuesday' },
            { id: 2, name: 'Wednesday' },
            { id: 3, name: 'Thursday' },
            { id: 4, name: 'Friday' },
            { id: 5, name: 'Saturday' },
            { id: 6, name: 'Sunday' }
        ];

        $scope.selectedDays = $scope.model.value || [];

        if (!angular.isArray($scope.model.value)) {
            //make an array from the dictionary
            var items = [];
            for (var i in $scope.model.value) {
                items.push($scope.model.value[i]);
                //items.push({
                //    value: $scope.model.value[i],
                //    id: i
                //});
            }
            //now make the editor model the array
            $scope.model.value = items;
        }


    });
