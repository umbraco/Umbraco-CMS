angular.module("umbraco").controller("Umbraco.PropertyEditors.RadioButtonsController",
    function ($scope, validationMessageService) {

        const vm = this;

        vm.viewItems = [];

        function init() {

            vm.uniqueId = String.CreateGuid();

            //we can't really do anything if the config isn't an object
            if (Utilities.isObject($scope.model.config.items)) {

                // formatting the items in the dictionary into an array
                let sortedItems = [];
                let vals = _.values($scope.model.config.items);
                let keys = _.keys($scope.model.config.items);
                
                for (let i = 0; i < vals.length; i++) {
                    sortedItems.push({ key: keys[i], sortOrder: vals[i].sortOrder, value: vals[i].value });
                }

                // ensure the items are sorted by the provided sort order
                sortedItems.sort((a, b) => (a.sortOrder > b.sortOrder) ? 1 : ((b.sortOrder > a.sortOrder) ? -1 : 0) );

                vm.viewItems = sortedItems;
            }

            // Set the message to use for when a mandatory field isn't completed.
            // Will either use the one provided on the property type or a localised default.
            validationMessageService.getMandatoryMessage($scope.model.validation).then(value => {
                $scope.mandatoryMessage = value;
            });
            
        }

        init();

    });
