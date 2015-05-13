/**
 * @ngdoc controller
 * @name Umbraco.Dialogs.InsertMacroController
 * @function
 * 
 * @description
 * The controller for the custom insert macro dialog. Until we upgrade the template editor to be angular this 
 * is actually loaded into an iframe with full html.
 */
function InsertMacroController($scope, entityResource, macroResource, umbPropEditorHelper, macroService, formHelper) {

    /** changes the view to edit the params of the selected macro */
    function editParams() {
        //get the macro params if there are any
        macroResource.getMacroParameters($scope.selectedMacro.id)
            .then(function (data) {

                //go to next page if there are params otherwise we can just exit
                if (!angular.isArray(data) || data.length === 0) {
                    //we can just exist!
                    submitForm();

                } else {
                    $scope.wizardStep = "paramSelect";
                    $scope.macroParams = data;
                    
                    //fill in the data if we are editing this macro
                    if ($scope.dialogData && $scope.dialogData.macroData && $scope.dialogData.macroData.macroParamsDictionary) {
                        _.each($scope.dialogData.macroData.macroParamsDictionary, function (val, key) {
                            var prop = _.find($scope.macroParams, function (item) {
                                return item.alias == key;
                            });
                            if (prop) {

                                if (_.isString(val)) {
                                    //we need to unescape values as they have most likely been escaped while inserted 
                                    val = _.unescape(val);

                                    //detect if it is a json string
                                    if (val.detectIsJson()) {
                                        try {
                                            //Parse it to json
                                            prop.value = angular.fromJson(val);
                                        }
                                        catch (e) {
                                            // not json
                                            prop.value = val;
                                        }
                                    }
                                    else {
                                        prop.value = val;
                                    }
                                }
                                else {
                                    prop.value = val;
                                }
                            }
                        });

                    }
                }
            });
    }

    /** submit the filled out macro params */
    function submitForm() {
        
        //collect the value data, close the dialog and send the data back to the caller

        //create a dictionary for the macro params
        var paramDictionary = {};
        _.each($scope.macroParams, function (item) {

            var val = item.value;

            if (item.value != null && item.value != undefined && !_.isString(item.value)) {
                try {
                    val = angular.toJson(val);
                }
                catch (e) {
                    // not json           
                }
            }
            
            //each value needs to be xml escaped!! since the value get's stored as an xml attribute
            paramDictionary[item.alias] = _.escape(val);

        });
        
        //need to find the macro alias for the selected id
        var macroAlias = $scope.selectedMacro.alias;

        //get the syntax based on the rendering engine
        var syntax;
        if ($scope.dialogData.renderingEngine && $scope.dialogData.renderingEngine === "WebForms") {
            syntax = macroService.generateWebFormsSyntax({ macroAlias: macroAlias, macroParamsDictionary: paramDictionary });
        }
        else if ($scope.dialogData.renderingEngine && $scope.dialogData.renderingEngine === "Mvc") {
            syntax = macroService.generateMvcSyntax({ macroAlias: macroAlias, macroParamsDictionary: paramDictionary });
        }
        else {
            syntax = macroService.generateMacroSyntax({ macroAlias: macroAlias, macroParamsDictionary: paramDictionary });
        }

        $scope.submit({ syntax: syntax, macroAlias: macroAlias, macroParamsDictionary: paramDictionary });
    }

    $scope.macros = [];
    $scope.selectedMacro = null;
    $scope.wizardStep = "macroSelect";
    $scope.macroParams = [];
    
    $scope.submitForm = function () {
        
        if (formHelper.submitForm({ scope: $scope })) {
        
            formHelper.resetForm({ scope: $scope });

            if ($scope.wizardStep === "macroSelect") {
                editParams();
            }
            else {
                submitForm();
            }

        }
    };

    //here we check to see if we've been passed a selected macro and if so we'll set the
    //editor to start with parameter editing
    if ($scope.dialogData && $scope.dialogData.macroData) {
        $scope.wizardStep = "paramSelect";
    }
    
    //get the macro list - pass in a filter if it is only for rte
    entityResource.getAll("Macro", ($scope.dialogData && $scope.dialogData.richTextEditor && $scope.dialogData.richTextEditor === true) ? "UseInEditor=true" : null)
        .then(function (data) {

            //if 'allowedMacros' is specified, we need to filter
            if (angular.isArray($scope.dialogData.allowedMacros) && $scope.dialogData.allowedMacros.length > 0) {
                $scope.macros = _.filter(data, function(d) {
                    return _.contains($scope.dialogData.allowedMacros, d.alias);
                });
            }
            else {
                $scope.macros = data;
            }
            

            //check if there's a pre-selected macro and if it exists
            if ($scope.dialogData && $scope.dialogData.macroData && $scope.dialogData.macroData.macroAlias) {
                var found = _.find(data, function (item) {
                    return item.alias === $scope.dialogData.macroData.macroAlias;
                });
                if (found) {
                    //select the macro and go to next screen
                    $scope.selectedMacro = found;
                    editParams();
                    return;
                }
            }
            //we don't have a pre-selected macro so ensure the correct step is set
            $scope.wizardStep = "macroSelect";
        });


}

angular.module("umbraco").controller("Umbraco.Dialogs.InsertMacroController", InsertMacroController);
