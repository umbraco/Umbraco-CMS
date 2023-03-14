/**
 * @ngdoc controller
 * @name Umbraco.Editors.RelationTypes.SettingsController
 * @function
 *
 * @description
 * The controller for editing relation type settings.
 */
function RelationTypeSettingsController($scope, localizationService) {

    var vm = this;

    vm.labels = {};

    function init() {

        var labelKeys = [
            "relationType_parentToChild",
            "relationType_bidirectional",
            "relationType_dependency",
            "relationType_noDependency"

        ];

        localizationService.localizeMany(labelKeys).then(function (data) {
            vm.labels.parentToChild = data[0];
            vm.labels.bidirectional = data[1];
            vm.labels.dependency = data[2];
            vm.labels.noDependency = data[3];
        });
    }

    init();
}

angular.module("umbraco").controller("Umbraco.Editors.RelationTypes.SettingsController", RelationTypeSettingsController);
