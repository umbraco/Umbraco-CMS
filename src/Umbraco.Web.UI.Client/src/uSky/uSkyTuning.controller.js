
/*********************************************************************************************************/
/* uSkyTuning panel app and controller */
/*********************************************************************************************************/

// uSkyTuning main app
angular.module("umbraco.uSkyTuning", ['ui.bootstrap', 'spectrumcolorpicker', 'ui.slider'])

// panel main controller
.controller("Umbraco.uSkyTuningController", function ($scope, $modal, $http, $window) {

    $scope.isOpen = false;
    $scope.schemaFocus = "body";
    $scope.settingIsOpen = 'setting';
    $scope.previewDevice = 'desktop';
    $scope.BackgroundPositions = ['center', 'left', 'right', 'bottom center', 'bottom left', 'bottom right', 'top center', 'top left', 'top right'];
    $scope.BackgroundRepeats = ['no-repeat', 'repeat', 'repeat-x', 'repeat-y'];
    $scope.BackgroundAttachments = ['scroll', 'fixed'];
    $scope.Layouts = ['boxed', 'wide', 'full'];
    $scope.displays = ['float-left', 'float-right', 'block-left', 'block-right', 'none'];
    $scope.optionHomes = ['icon', 'text', 'none'];
    $scope.googleFontFamilies = {}

    // Load parameters from GetLessParameters and init data of the tuning config
    var initTuning = function () {
        $http.get('/Umbraco/Api/uSkyTuning/GetLessParameters')
            .success(function (data) {

                $.each(tuningConfig.categories, function (indexCategory, category) {
                    $.each(category.sections, function (indexSection, section) {
                        $.each(section.subSections, function (indexSubSection, subSection) {
                            $.each(subSection.fields, function (indexField, field) {

                                // value
                                field.value = eval("data." + field.alias.replace("@", ""));
                                if (field.value == "''") { field.value = ""; }

                                // special init for font family picker
                                if (field.type == "fontFamilyPicker") {
                                    field.fontWeight = eval("data." + field.alias.replace("@", "") + "_weight");
                                    field.fontStyle = eval("data." + field.alias.replace("@", "") + "_style");
                                    field.fontType = eval("data." + field.alias.replace("@", "") + "_type");
                                    if (field.fontWeight == "''") { field.fontWeight = ""; }
                                    if (field.fontStyle == "''") { field.fontStyle = ""; }
                                    if (field.fontType == "''") { field.fontType = ""; }
                                }

                            })
                        })
                    })
                });

                $scope.tuningModel = tuningConfig;
                $scope.tuningPalette = tuningPalette;

                refreshtuning();

                $scope.$watch('tuningModel', function () {
                    refreshtuning();
                }, true);

            });
    }

    // Refresh all less parameters for every changes watching tuningModel 
    var refreshtuning = function () {
        var parameters = [];
        $.each($scope.tuningModel.categories, function (indexCategory, category) {
            $.each(category.sections, function (indexSection, section) {
                $.each(section.subSections, function (indexSubSection, subSection) {
                    $.each(subSection.fields, function (indexField, field) {

                        // value
                        parameters.splice(parameters.length + 1, 0, "'@" + field.alias + "':'" + field.value + "'");

                        // special init for font family picker
                        if (field.type == "fontFamilyPicker") {
                            parameters.splice(parameters.length + 1, 0, "'@" + field.alias + "_weight':'" + field.fontWeight + "'");
                            parameters.splice(parameters.length + 1, 0, "'@" + field.alias + "_Style':'" + field.fontStyle + "'");
                        }

                    })
                })
            })
        });

        // Refrech page style
        document.getElementById("resultFrame").contentWindow.refrechLayout(parameters);

    }

    var openIntelTuning = function () {
        document.getElementById("resultFrame").contentWindow.initIntelTuning($scope.tuningModel);
    }

    var closeIntelTuning = function () {
        document.getElementById("resultFrame").contentWindow.closeIntelTuning($scope.tuningModel);
    }

    // Refresh with selected tuning palette
    $scope.refreshtuningByPalette = function (colors) {

        $.each($scope.tuningModel.categories, function (indexCategory, category) {
            $.each(category.sections, function (indexSection, section) {
                $.each(section.subSections, function (indexSubSection, subSection) {
                    $.each(subSection.fields, function (indexField, field) {

                        if (field.type == "colorPicker") {
                            $.each(colors, function (indexColor, color) {
                                if (color.alias == field.alias) {
                                    field.value = color.value;
                                }
                            });
                        }

                    })
                })
            })
        });

        refreshtuning();
    }

    // Save all parameter in tuningParameters.less file
    $scope.saveLessParameters = function () {
        var parameters = [];
        $.each($scope.tuningModel.categories, function (indexCategory, category) {
            $.each(category.sections, function (indexSection, section) {
                $.each(section.subSections, function (indexSubSection, subSection) {
                    $.each(subSection.fields, function (indexField, field) {

                        // value
                        var value = (field.value != 0 && (field.value == undefined || field.value == "")) ? "''" : field.value;
                        parameters.splice(parameters.length + 1, 0, "@" + field.alias + ":" + value + ";");

                        // special init for font family picker
                        if (field.type == "fontFamilyPicker") {
                            if (field.fontType == "google" && value != "''") {
                                var variant = field.fontWeight != "" || field.fontStyle != "" ? ":" + field.fontWeight + field.fontStyle : "";
                                var gimport = "@import url('http://fonts.googleapis.com/css?family=" + value + variant + "');";
                                if ($.inArray(gimport, parameters) < 0) {
                                    parameters.splice(0, 0, gimport);
                                }
                            }
                            var fontWeight = (field.fontWeight != 0 && (field.fontWeight == undefined || field.fontWeight == "")) ? "''" : field.fontWeight;
                            var fontStyle = (field.fontStyle != 0 && (field.fontStyle == undefined || field.fontStyle == "")) ? "''" : field.fontStyle;
                            var fontType = (field.fontType != 0 && (field.fontType == undefined || field.fontType == "")) ? "''" : field.fontType;
                            parameters.splice(parameters.length + 1, 0, "@" + field.alias + "_weight:" + fontWeight + ";");
                            parameters.splice(parameters.length + 1, 0, "@" + field.alias + "_style:" + fontStyle + ";");
                            parameters.splice(parameters.length + 1, 0, "@" + field.alias + "_type:" + fontType + ";");
                        }

                    })
                })
            })
        });

        var resultParameters = { result: parameters.join("") };
        var transform = function (result) {
            return $.param(result);
        }

        $http.defaults.headers.post["Content-Type"] = "application/x-www-form-urlencoded";
        $http.post('/Umbraco/Api/uSkyTuning/PostLessParameters', resultParameters, {
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            transformRequest: transform
        })
        .success(function (data) {
            alert("Change saved !");
        });

    }

    // Toggle panel
    $scope.togglePanel = function () {
        if ($scope.isOpen) {
            $scope.isOpen = false;
            closeIntelTuning();
        }
        else {
            $scope.isOpen = true;
            $scope.settingOpen($scope.settingIsOpen);
        }
    }

    // Toggle setting
    $scope.settingOpen = function (a) {

        if ($scope.settingIsOpen == "setting" && a != "setting") {
            closeIntelTuning();
        }

        if (a == "setting") {
            openIntelTuning();
        }

        $scope.settingIsOpen = a;
    }

    // Remove value from field
    $scope.removeField = function (field) {
        field.value = "";
    }

    // Open image picker modal
    $scope.open = function (field) {
        var modalInstance = $modal.open({
            templateUrl: 'myModalContent.html',
            controller: 'uskytuning.mediapickercontroller',
            resolve: {
                items: function () {
                    return field.value;
                }
            }
        });
        modalInstance.result.then(function (selectedItem) {
            field.value = selectedItem;
        });
    };

    // Open font family picker modal
    $scope.openFontFamilyPickerModal = function (field) {
        var modalInstance = $modal.open({
            templateUrl: 'fontFamilyPickerModel.html',
            controller: 'uskytuning.fontfamilypickercontroller',
            resolve: {
                googleFontFamilies: function () {
                    return $scope.googleFontFamilies;
                },
                item: function () {
                    return field;
                }
            }
        });
        modalInstance.result.then(function (selectedItem) {
            field.value = selectedItem.fontFamily;
            field.fontType = selectedItem.fontType;
            field.fontWeight = selectedItem.fontWeight;
            field.fontStyle = selectedItem.fontStyle;
        });
    };

    // Set preview device
    $scope.updatePreviewDevice = function (device) {
        $scope.previewDevice = device;
    }

    // Accordion open event
    $scope.accordionOpened = function (schema) {
        $scope.schemaFocus = schema;
    }

    // Accordion open event
    $scope.accordionWillBeOpened = function (schema) {
        $scope.futurSchemaFocus = schema;
    }

    // Preload of the google font
    $http.get('/Umbraco/Api/uSkyTuning/GetGoogleFont').success(function (data) {
        $scope.googleFontFamilies = data;
    })

    // Inicial tuning loading
    initTuning();

})

// Image picker controller
.controller('uskytuning.mediapickercontroller', function ($scope, $modalInstance, items, $http) {

    $scope.items = [];

    $http.get('/Umbraco/Api/uSkyTuning/GetBackGroundImage')
            .success(function (data) {
                $scope.items = data;
            });

    $scope.selected = {
        item: $scope.items[0]
    };

    $scope.ok = function () {
        $modalInstance.close($scope.selected.item);
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };

})

// Font picker controller
.controller('uskytuning.fontfamilypickercontroller', function ($scope, $modalInstance, item, googleFontFamilies, $http) {

    $scope.safeFonts = ["Arial, Helvetica", "Impact", "Lucida Sans Unicode", "Tahoma", "Trebuchet MS", "Verdana", "Georgia", "Times New Roman", "Courier New, Courier"];
    $scope.fonts = [];
    $scope.selectedFont = {};

    var googleGetWeight = function (googleVariant) {
        return (googleVariant != undefined && googleVariant != "") ? googleVariant.replace("italic", "") : "";
    };

    var googleGetStyle = function (googleVariant) {
        var variantStyle = "";
        if (googleVariant != undefined && googleVariant != "" && googleVariant.indexOf("italic") >= 0) {
            variantWeight = googleVariant.replace("italic", "");
            variantStyle = "italic";
        }
        return variantStyle;
    };

    angular.forEach($scope.safeFonts, function (value, key) {
        $scope.fonts.push({
            groupName: "Safe fonts",
            fontType: "safe",
            fontFamily: value,
            fontWeight: "normal",
            fontStyle: "normal",
        });
    });

    angular.forEach(googleFontFamilies.items, function (value, key) {
        var variants = value.variants;
        var variant = value.variants.length > 0 ? value.variants[0] : "";
        var fontWeight = googleGetWeight(variant);
        var fontStyle = googleGetStyle(variant);
        $scope.fonts.push({
            groupName: "Google fonts",
            fontType: "google",
            fontFamily: value.family,
            variants: value.variants,
            variant: variant,
            fontWeight: fontWeight,
            fontStyle: fontStyle
        });
    });

    $scope.setStyleVariant = function () {
        if ($scope.selectedFont != undefined) {
            return {
                'font-family': $scope.selectedFont.fontFamily,
                'font-weight': $scope.selectedFont.fontWeight,
                'font-style': $scope.selectedFont.fontStyle
            }
        }
    };

    $scope.showFontPreview = function (font) {
        $scope.selectedFont = font;
        if (font != undefined && font.fontFamily != "" && font.fontType == "google") {
            $scope.selectedFont.fontWeight = googleGetWeight($scope.selectedFont.variant);
            $scope.selectedFont.fontStyle = googleGetStyle($scope.selectedFont.variant);
            WebFont.load({
                google: {
                    families: [font.fontFamily + ":" + font.variant]
                },
                loading: function () {
                    console.log('loading');
                }
            });
        }
    }

    $scope.ok = function () {
        $modalInstance.close({
            fontFamily: $scope.selectedFont.fontFamily,
            fontType: $scope.selectedFont.fontType,
            fontWeight: $scope.selectedFont.fontWeight,
            fontStyle: $scope.selectedFont.fontStyle,
        });
    };

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    };

    if (item != undefined) {
        angular.forEach($scope.fonts, function (value, key) {
            if (value.fontFamily == item.value) {
                $scope.selectedFont = value;
                $scope.selectedFont.variant = item.fontWeight + item.fontStyle;
                $scope.selectedFont.fontWeight = item.fontWeight;
                $scope.selectedFont.fontStyle = item.fontStyle;
            }
        });
    }

});
