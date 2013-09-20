/**
 * @ngdoc controller
 * @name Umbraco.Editors.Templates.InsertMacroController
 * @function
 * 
 * @description
 * The controller for the custom insert macro dialog. Until we upgrade the template editor to be angular this 
 * is actually loaded into an iframe with full html.
 */
function InsertMacroController($scope, entityResource, macroResource, umbPropEditorHelper, macroService) {

    /** changes the view to edit the params of the selected macro */
    function editParams() {
        //get the macro params if there are any
        macroResource.getMacroParameters($scope.selectedMacro)
            .then(function (data) {

                //go to next page if there are params otherwise we can just exit
                if (!angular.isArray(data) || data.length === 0) {
                    //we can just exist!
                    $scope.submit({ selectedMacro: $scope.selectedMacro });
                } else {
                    $scope.wizardStep = "paramSelect";
                    ////update the view on each editor to be correct
                    //_.each(data, function (item) {
                    //    item.view = umbPropEditorHelper.getViewPath(item.view);
                    //});
                    $scope.macroParams = data;
                }
            });
    }

    /** submit the filled out macro params */
    function submitForm() {
        
        //collect the value data, close the dialog and send the data back to the caller
        var vals = _.map($scope.macroParams, function (item) {
            return { value: item.value, alias: item.alias };
        });

        //need to find the macro alias for the selected id
        var macroAlias = _.find($scope.macros, function (item) {
            return item.id == $scope.selectedMacro;
        }).alias;

        //get the syntax based on the rendering engine
        var syntax;
        if ($scope.dialogData.renderingEngine && $scope.dialogData.renderingEngine === "WebForms") {
            syntax = macroService.generateWebFormsSyntax({ macroAlias: macroAlias, macroParams: vals });
        }
        else {
            syntax = macroService.generateMvcSyntax({ macroAlias: macroAlias, macroParams: vals });
        }

        $scope.submit(syntax);
    }

    $scope.macros = [];
    $scope.selectedMacro = null;
    $scope.wizardStep = "macroSelect";
    $scope.macroParams = [];
    
    $scope.submitForm = function () {

        if ($scope.wizardStep === "paramSelect") {
            //we need to broadcast the saving event for the toggle validators to work
            $scope.$broadcast("saving");
        }

        //ensure the drop down is dirty so the styles validate
        $scope.insertMacroForm.$setDirty(true);
        if ($scope.insertMacroForm.$invalid) {
            return;
        }
        
        if ($scope.wizardStep === "macroSelect") {
            editParams();
        }
        else {
            submitForm();
        }
        
    };

    //get the macro list
    entityResource.getAll("Macro")
        .then(function (data) {            

            $scope.macros = data;
            
            //check if there's a pre-selected macro and if it exists
            if ($scope.dialogData.selectedAlias) {
                var found = _.find(data, function(item) {
                    return item.alias === $scope.dialogData.selectedAlias;
                });
                if (found) {
                    //select the macro and go to next screen
                    $scope.selectedMacro = found.id;
                    editParams();
                }
                
            }
        });

}

angular.module("umbraco").controller("Umbraco.Editors.Templates.InsertMacroController", InsertMacroController);
