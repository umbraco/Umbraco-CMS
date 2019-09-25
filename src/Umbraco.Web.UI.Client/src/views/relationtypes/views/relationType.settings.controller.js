/**
 * @ngdoc controller
 * @name Umbraco.Editors.RelationTypes.EditController
 * @function
 *
 * @description
 * The controller for editing relation types.
 */
function RelationTypeSettingsController($scope, localizationService) {

    var vm = this;

    vm.labels = {};

    vm.changeDirection = changeDirection;

    function changeDirection(model, value) {
        console.log("model", model, value);
    }

    function init() {

        var labelKeys = [
            "relationType_parentToChild",
            "relationType_bidirectional"
        ];

        localizationService.localizeMany(labelKeys).then(function (data) {
            console.log("data", data);
            vm.labels.parentToChild = data[0];
            vm.labels.bidirectional = data[1];
        });
    }

    init();
}

angular.module("umbraco").controller("Umbraco.Editors.RelationTypes.SettingsController", RelationTypeSettingsController);
