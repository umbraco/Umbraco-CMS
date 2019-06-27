angular.module("umbraco").controller("Umbraco.PropertyEditors.RadioButtonsController",
    function ($scope, validationHelper) {

        function init() {

            //we can't really do anything if the config isn't an object
            if (angular.isObject($scope.model.config.items)) {

                //now we need to format the items in the dictionary because we always want to have an array
                var configItems = [];
                var vals = _.values($scope.model.config.items);
                var keys = _.keys($scope.model.config.items);
                for (var i = 0; i < vals.length; i++) {
                    configItems.push({ id: keys[i], sortOrder: vals[i].sortOrder, value: vals[i].value });
                }

                //ensure the items are sorted by the provided sort order
                configItems.sort(function (a, b) { return (a.sortOrder > b.sortOrder) ? 1 : ((b.sortOrder > a.sortOrder) ? -1 : 0); });

                $scope.configItems = configItems;
            }

            // Set the message to use for when a mandatory field isn't completed.
            // Will either use the one provided on the property type or a localised default.
            validationHelper.getMandatoryMessage($scope.model.validation).then(function (value) {
                $scope.mandatoryMessage = value;
            });    
            
        }

        init();

    });
