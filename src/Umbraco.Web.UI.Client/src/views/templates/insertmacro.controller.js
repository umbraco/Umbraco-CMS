/**
 * @ngdoc controller
 * @name Umbraco.Editors.Templates.InsertMacroController
 * @function
 * 
 * @description
 * The controller for the custom insert macro dialog. Until we upgrade the template editor to be angular this 
 * is actually loaded into an iframe with full html.
 */
function InsertMacroController($scope, entityResource) {

    $scope.macros = [];
    $scope.selectedMacro = null;
    $scope.submitForm = function () {
        //ensure the drop down is dirty so the styles validate
        $scope.insertMacroForm.$setDirty(true);
        if ($scope.insertMacroForm.$invalid) {
            return;
        }
        //go to next page!
    };

    //fetch the authorized status         
    entityResource.getAll("Macro")
        .then(function (data) {            
            $scope.macros = data;
        });

}

angular.module("umbraco").controller("Umbraco.Editors.Templates.InsertMacroController", InsertMacroController);
