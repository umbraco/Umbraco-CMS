/**
 * @ngdoc controller
 * @name Umbraco.Editors.Settings.Template.EditController
 * @function
 * 
 * @description
 * The controller for editing templates
 */
function TemplateEditController($scope, navigationService) {
    $scope.template = "<html><body><h1>Hej</h1></body></html>";
}

angular.module("umbraco").controller("Umbraco.Editors.Settings.Template.EditController", TemplateEditController);
