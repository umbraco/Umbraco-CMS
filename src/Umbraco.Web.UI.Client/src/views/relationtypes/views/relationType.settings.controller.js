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

    function init() {

        var labelKeys = [
            "relationType_parentToChild",
            "relationType_bidirectional"
        ];

        localizationService.localizeMany(labelKeys).then(function (data) {
            vm.labels.parentToChild = data[0];
            vm.labels.bidirectional = data[1];
        });
    }

    init();
}

angular.module("umbraco").controller("Umbraco.Editors.RelationTypes.SettingsController", RelationTypeSettingsController);
