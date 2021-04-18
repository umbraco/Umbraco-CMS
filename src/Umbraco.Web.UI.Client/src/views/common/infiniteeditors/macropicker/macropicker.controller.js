function MacroPickerController($scope, entityResource, macroResource, umbPropEditorHelper, macroService, formHelper, localizationService) {

    $scope.macros = [];
    $scope.a11yInfo = "";
    $scope.model.selectedMacro = null;
    $scope.model.macroParams = [];
    $scope.displayA11YMessageForFilter = displayA11YMessageForFilter;
    $scope.wizardStep = "macroSelect";
    $scope.noMacroParams = false;
    $scope.model.searchTerm = "";
    function onInit() {
        if (!$scope.model.title) {
            localizationService.localize("defaultdialogs_selectMacro").then(function (value) {
                $scope.model.title = value;
            });
        }
    }

    $scope.selectMacro = function (macro) {

        $scope.model.selectedMacro = macro;

        if ($scope.wizardStep === "macroSelect") {
            editParams(true);
        } else {
            $scope.$broadcast("formSubmitting", { scope: $scope });
            $scope.model.submit($scope.model);
        }
    };

    $scope.close = function () {
        if ($scope.model.close) {
            $scope.model.close();
        }
    }

    /** changes the view to edit the params of the selected macro */
    /** if there is pnly one macro, and it has parameters - editor can skip selecting the Macro **/
    function editParams(insertIfNoParameters) {
        //whether to insert the macro in the rich text editor when editParams is called and there are no parameters see U4-10537 
        insertIfNoParameters = (typeof insertIfNoParameters !== 'undefined') ? insertIfNoParameters : true;
        //get the macro params if there are any
        macroResource.getMacroParameters($scope.model.selectedMacro.id)
            .then(function (data) {

                //go to next page if there are params otherwise we can just exit
                if (!Utilities.isArray(data) || data.length === 0) {

                    if (insertIfNoParameters) {
                        $scope.model.submit($scope.model);
                    } else {
                        $scope.wizardStep = 'macroSelect';
                        displayA11yMessages($scope.macros);
                    }

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
                                            //Parse it from json
                                            prop.value = Utilities.fromJson(val);
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

    function displayA11yMessages(macros) {
        if ($scope.noMacroParams || !macros || macros.length === 0)
            localizationService.localize("general_searchNoResult").then(function (value) {
                $scope.a11yInfo = value;
            });
        else if (macros) {
            if (macros.length === 1) {
                localizationService.localize("treeSearch_searchResult").then(function(value) {
                    $scope.a11yInfo = "1 " + value;
                });
            } else {
                localizationService.localize("treeSearch_searchResults").then(function (value) {
                    $scope.a11yInfo = macros.length + " " + value;
                });
            }
        }
    }

    function displayA11YMessageForFilter() {
        var macros = _.filter($scope.macros, v => v.name.toLowerCase().includes($scope.model.searchTerm.toLowerCase()));
        displayA11yMessages(macros);
    }
    //here we check to see if we've been passed a selected macro and if so we'll set the
    //editor to start with parameter editing
    if ($scope.model.dialogData && $scope.model.dialogData.macroData) {
        $scope.wizardStep = "paramSelect";
    }

    //get the macro list - pass in a filter if it is only for rte
    entityResource.getAll("Macro", ($scope.model.dialogData && $scope.model.dialogData.richTextEditor && $scope.model.dialogData.richTextEditor === true) ? "UseInEditor=true" : null)
        .then(function (data) {

            if (Utilities.isArray(data) && data.length == 0) {
                $scope.nomacros = true;
            }

            //if 'allowedMacros' is specified, we need to filter
            if (Utilities.isArray($scope.model.dialogData.allowedMacros) && $scope.model.dialogData.allowedMacros.length > 0) {
                $scope.macros = _.filter(data, function (d) {
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
                    editParams(true);
                    return;
                }
            }
            //if there is only one macro in the site and it has parameters, let's not make the editor choose it from a selection of one macro (unless there are no parameters - then weirdly it's a better experience to make that selection)
            if ($scope.macros.length == 1) {
                $scope.model.selectedMacro = $scope.macros[0];
                editParams(false);
            }
            else {
                //we don't have a pre-selected macro so ensure the correct step is set
                $scope.wizardStep = 'macroSelect';
            }
            displayA11yMessages($scope.macros);
        });

    onInit();
    
}

angular.module("umbraco").controller("Umbraco.Overlays.MacroPickerController", MacroPickerController);
