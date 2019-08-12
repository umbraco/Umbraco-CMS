/**
 * @ngdoc controller
 * @name Umbraco.PropertyEditors.BlockEditor.EditSettingsController
 * @function
 *
 * @description
 * The controller for editing the configuration of an individual block
 */

//fixme: Need to figure out the name of a block settings, is it block config or block settings?
function BlockEditorEditSettingsController($scope) {

    this.submit = () => {
        if($scope.model.submit) {
            $scope.model.submit($scope.model);
        }
    }

    this.close = () => {
        if($scope.model.close) {
            $scope.model.close();
        }
    } 
}
angular.module("umbraco")
    .controller("Umbraco.PropertyEditors.BlockEditor.EditSettingsController", ['$scope', BlockEditorEditSettingsController]);
