function MacroPickerController($scope, entityResource, macroResource, umbPropEditorHelper, macroService, formHelper, localizationService) {


    if(!$scope.model.title) {
        $scope.model.title = localizationService.localize("defaultdialogs_selectMacro");
    }

    $scope.macros = [];
    $scope.model.selectedMacro = null;
    $scope.wizardStep = "macroSelect";
    $scope.model.macroParams = [];
    $scope.noMacroParams = false;

    $scope.changeMacro = function() {
        if ($scope.wizardStep === "macroSelect") {
            editParams();
        } else {
            submitForm();
        }
    };

    /** changes the view to edit the params of the selected macro */
    function editParams() {
        //get the macro params if there are any
        macroResource.getMacroParameters($scope.model.selectedMacro.id)
            .then(function (data) {

                //go to next page if there are params otherwise we can just exit
                if (!angular.isArray(data) || data.length === 0) {

                    $scope.noMacroParams = true;

                } else {
                    $scope.wizardStep = "paramSelect";
                    $scope.model.macroParams = data;

                    //fill in the data if we are editing this macro
                    if ($scope.model.dialogData && $scope.model.dialogData.macroData && $scope.model.dialogData.macroData.macroParamsDictionary) {
                        _.each($scope.model.dialogData.macroData.macroParamsDictionary, function (val, key) {
                            var prop = _.find($scope.model.macroParams, function (item) {
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

    //here we check to see if we've been passed a selected macro and if so we'll set the
    //editor to start with parameter editing
    if ($scope.model.dialogData && $scope.model.dialogData.macroData) {
        $scope.wizardStep = "paramSelect";
    }

    //get the macro list - pass in a filter if it is only for rte
    entityResource.getAll("Macro", ($scope.model.dialogData && $scope.model.dialogData.richTextEditor && $scope.model.dialogData.richTextEditor === true) ? "UseInEditor=true" : null)
        .then(function (data) {

            //if 'allowedMacros' is specified, we need to filter
            if (angular.isArray($scope.model.dialogData.allowedMacros) && $scope.model.dialogData.allowedMacros.length > 0) {
                $scope.macros = _.filter(data, function(d) {
                    return _.contains($scope.model.dialogData.allowedMacros, d.alias);
                });
            }
            else {
                $scope.macros = data;
            }


            //check if there's a pre-selected macro and if it exists
            if ($scope.model.dialogData && $scope.model.dialogData.macroData && $scope.model.dialogData.macroData.macroAlias) {
                var found = _.find(data, function (item) {
                    return item.alias === $scope.model.dialogData.macroData.macroAlias;
                });
                if (found) {
                    //select the macro and go to next screen
                    $scope.model.selectedMacro = found;
                    editParams();
                    return;
                }
            }
            //we don't have a pre-selected macro so ensure the correct step is set
            $scope.wizardStep = "macroSelect";
        });


}

angular.module("umbraco").controller("Umbraco.Overlays.MacroPickerController", MacroPickerController);
