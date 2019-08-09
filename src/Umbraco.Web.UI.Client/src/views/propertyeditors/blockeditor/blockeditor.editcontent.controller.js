/**
 * @ngdoc controller
 * @name Umbraco.PropertyEditors.BlockEditor.EditContentController
 * @function
 *
 * @description
 * The controller for editing content with the block editor
 */

function BlockEditorEditContentController($scope) {
    var vm = this;
    vm.submit = submit;
    vm.close = close;
    vm.content = $scope.model.element.variants[0];

    function init() {

    }

    function submit() {
        if($scope.model.submit) {
            $scope.model.submit($scope.model);
        }
    }

    function close() {
        if($scope.model.close) {
            $scope.model.close();
        }
    }

    init();
}
angular.module("umbraco").controller("Umbraco.PropertyEditors.BlockEditor.EditContentController", BlockEditorEditContentController);
