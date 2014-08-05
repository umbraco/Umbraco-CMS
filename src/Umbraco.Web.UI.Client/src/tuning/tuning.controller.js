
/*********************************************************************************************************/
/* tuning panel app and controller */
/*********************************************************************************************************/

var app = angular.module("umbraco.tuning", ['spectrumcolorpicker', 'ui.slider', 'umbraco.resources', 'umbraco.services', 'antiscroll'])

.controller("Umbraco.tuningController", function ($scope, $http, $window, $timeout, $location, dialogService) {

    $scope.isOpen = false;
    $scope.frameLoaded = false;
    $scope.enableTuning = 0;
    $scope.schemaFocus = "body";
    $scope.settingIsOpen = 'previewDevice';
    $scope.propertyCategories = [];
    $scope.googleFontFamilies = {};
    $scope.pageId = "../dialogs/Preview.aspx?id=" + $location.search().id;
    $scope.devices = [
        { name: "desktop", css: "desktop", icon: "icon-display" },
        { name: "laptop - 1366px", css: "laptop border", icon: "icon-laptop" },
        { name: "iPad portrait - 768px", css: "iPad-portrait border", icon: "icon-ipad" },
        { name: "iPad landscape - 1024px", css: "iPad-landscape border", icon: "icon-ipad flip" },
        { name: "smartphone portrait - 480px", css: "smartphone-portrait border", icon: "icon-iphone" },
        { name: "smartphone landscape  - 320px", css: "smartphone-landscape border", icon: "icon-iphone flip" }
    ];
    $scope.previewDevice = $scope.devices[0];

    /*****************************************************************************/
    /* Preview devices */
    /*****************************************************************************/

    // Set preview device
    $scope.updatePreviewDevice = function (device) {
        $scope.previewDevice = device;
    }

    /*****************************************************************************/
    /* UI designer managment */
    /*****************************************************************************/

    // Update all tuningConfig's values from data
    var updateConfigValue = function (data) {

        var fonts = [];
        $.each($scope.tuningModel.configs, function (indexConfig, config) {
            $.each(config.editors, function (indexItem, item) {

                /* try to get value */
                try {

                    if (item.values) {
                        angular.forEach(Object.keys(item.values), function (key, indexKey) {
                            if (key != "''") {
                                var propertyAlias = key.toLowerCase() + item.alias.toLowerCase();
                                var newValue = eval("data." + propertyAlias.replace("@", ""));
                                if (newValue == "''") {
                                    newValue = "";
                                }
                                item.values[key] = newValue;
                            }
                        })
                    }

                    // TODO: special init for font family picker
                    if (item.type == "googlefontpicker") {
                        if (item.values.fontType == 'google' && item.values.fontFamily + item.values.fontWeight && $.inArray(item.values.fontFamily + ":" + item.values.fontWeight, fonts) < 0) {
                            fonts.splice(0, 0, item.values.fontFamily + ":" + item.values.fontWeight);
                        }
                    }

                }
                catch (err) {
                    console.info("Style parameter not found " + item.alias);
                }

            })
        });

        // Load google font
        $.each(fonts, function (indexFont, font) {
            loadGoogleFont(font);
            loadGoogleFontInFront(font);
        });

    }

    // Load parameters from GetLessParameters and init data of the tuning config
    $scope.initTuning = function () {

        $http.get('/Umbraco/Api/tuning/Load', { params: { pageId: $location.search().id } })
            .success(function (data) {

                updateConfigValue(data);
                
                $timeout(function () {
                    $scope.frameLoaded = true;
                }, 200);

            });

    }

    // Refresh all less parameters for every changes watching tuningModel 
    var refreshtuning = function () {

        var parameters = [];

        if ($scope.tuningModel) {

            angular.forEach($scope.tuningModel.configs, function (config, indexConfig) {

                // Get currrent selected element
                if ($scope.schemaFocus && angular.lowercase($scope.schemaFocus) == angular.lowercase(config.name)) {
                    $scope.currentSelected = config.selector ? config.selector : config.schema;
                }

                angular.forEach(config.editors, function (item, indexItem) {

                    // Add new style
                    if (item.values) {
                        angular.forEach(Object.keys(item.values), function (key, indexKey) {
                            var propertyAlias = key.toLowerCase() + item.alias.toLowerCase();
                            var value = eval("item.values." + key);
                            parameters.splice(parameters.length + 1, 0, "'@" + propertyAlias + "':'" + value + "'");
                        })
                    }
                })
            });

            // Refrech page style
            refreshFrontStyles(parameters);

            // Refrech layout of selected element
            if ($scope.currentSelected) {
                setSelectedSchema($scope.currentSelected);
            }
            
        }
    }

    $scope.createStyle = function (){
        $scope.saveLessParameters(false);
    }

    $scope.saveStyle = function () {
        $scope.saveLessParameters(true);
    }

    // Save all parameter in tuningParameters.less file
    $scope.saveLessParameters = function (inherited) {

        var parameters = [];
        $.each($scope.tuningModel.configs, function (indexConfig, config) {
            $.each(config.editors, function (indexItem, item) {

                if (item.values) {
                    angular.forEach(Object.keys(item.values), function (key, indexKey) {
                        var propertyAlias = key.toLowerCase() + item.alias.toLowerCase();
                        var value = eval("item.values." + key);
                        parameters.splice(parameters.length + 1, 0, "@" + propertyAlias + ":" + value + ";");
                    })

                    // TODO: special init for font family picker
                    if (item.type == "googlefontpicker") {
                        var variant = item.values.fontWeight != "" || item.values.fontStyle != "" ? ":" + item.values.fontWeight + item.values.fontStyle : "";
                        var gimport = "@import url('http://fonts.googleapis.com/css?family=" + item.values.fontFamily + variant + "');";
                        if ($.inArray(gimport, parameters) < 0) {
                            parameters.splice(0, 0, gimport);
                        }
                    }

                }

            })
        });

        var resultParameters = { parameters: parameters.join(""), pageId: $location.search().id, inherited: inherited };
        var transform = function (result) {
            return $.param(result);
        }

        $('.btn-default-save').attr("disabled", true);
        $http.defaults.headers.post["Content-Type"] = "application/x-www-form-urlencoded";
        $http.post('/Umbraco/Api/tuning/Save', resultParameters, {
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            transformRequest: transform
        })
        .success(function (data) {
            $('.btn-default-save').attr("disabled", false);
            $('#speechbubble').fadeIn('slow').delay(5000).fadeOut('slow');
        });

    }

    // Delete current page tuning
    $scope.deleteTuning = function () {
        $('.btn-default-delete').attr("disabled", true);
        $http.get('/Umbraco/Api/tuning/Delete', { params: { pageId: $location.search().id } })
        .success(function (data) {
            $scope.enableTuning++;
            $scope.pageId = $scope.pageId + "&n=123456";
            $('.btn-default-delete').attr("disabled", false);
        })
    }

    /*****************************************************************************/
    /* Preset design */
    /*****************************************************************************/

    // Refresh with selected tuning palette
    $scope.refreshtuningByPalette = function (palette) {
        updateConfigValue(palette.colors);
        refreshtuning();
    }

    // Hidden botton to make preset from the current settings
    $scope.makePreset = function () {

        var parameters = [];
        $.each($scope.tuningModel.categories, function (indexCategory, category) {
            $.each(category.sections, function (indexSection, section) {
                $.each(section.subSections, function (indexSubSection, subSection) {
                    $.each(subSection.fields, function (indexField, field) {

                        if (!subSection.schema || subSection.schema.indexOf("gridrow_") < 0) {

                            // value
                            var value = (field.value != 0 && (field.value == undefined || field.value == "")) ? "''" : field.value;
                            parameters.splice(parameters.length + 1, 0, "\"" + field.alias + "\":" + " \"" + value + "\"");

                            // special init for font family picker
                            if (field.type == "fontFamilyPicker") {
                                var fontWeight = (field.fontWeight != 0 && (field.fontWeight == undefined || field.fontWeight == "")) ? "''" : field.fontWeight;
                                var fontStyle = (field.fontStyle != 0 && (field.fontStyle == undefined || field.fontStyle == "")) ? "''" : field.fontStyle;
                                var fontType = (field.fontType != 0 && (field.fontType == undefined || field.fontType == "")) ? "''" : field.fontType;
                                parameters.splice(parameters.length + 1, 0, "\"" + field.alias + "_weight" + "\":" + " \"" + fontWeight + "\"");
                                parameters.splice(parameters.length + 1, 0, "\"" + field.alias + "_style" + "\":" + " \"" + fontStyle + "\"");
                                parameters.splice(parameters.length + 1, 0, "\"" + field.alias + "_type" + "\":" + " \"" + fontType + "\"");
                            }

                        }

                    })
                })
            })
        });

        $("body").append("<textarea>{name:\"\", mainColor:\"\", colors:{" + parameters.join(",") + "}}</textarea>");

    }

    /*****************************************************************************/
    /* Panel managment */
    /*****************************************************************************/

    $scope.openPreviewDevice = function () {
        $scope.showDevicesPreview = true;
        $scope.closeIntelTuning()
    }

    $scope.closePreviewDevice = function(){
        $scope.showDevicesPreview = false;
        if ($scope.showStyleEditor) {
            $scope.openIntelTuning();
        }
    }

    $scope.openPalettePicker = function () {
        $scope.showPalettePicker = true;
        $scope.showStyleEditor = false;
        $scope.closeIntelTuning()
    }

    $scope.openStyleEditor = function () {
        $scope.showStyleEditor = true;
        $scope.showPalettePicker = false;
        $scope.openIntelTuning()
    }

    // Toggle panel
    //$scope.togglePanel = function () {
    //    if ($scope.isOpen) {
    //        $scope.isOpen = false;
    //        closeIntelTuning();
    //    }
    //    else {
    //        $scope.isOpen = true;
    //        $scope.settingOpen($scope.settingIsOpen);
    //    }
    //}

    // Toggle setting
    //$scope.settingOpen = function (a) {

    //    if ($scope.settingIsOpen == "setting" && a != "setting") {
    //        closeIntelTuning();
    //    }

    //    if (a == "setting") {
    //        openIntelTuning();
    //    }

    //    $scope.settingIsOpen = a;
    //}

    // Remove value from field
    $scope.removeField = function (field) {
        field.value = "";
    }

    //// Accordion open event
    //$scope.accordionOpened = function (schema) {
    //    $scope.schemaFocus = schema;
    //}

    //// Focus schema in front
    //$scope.accordionWillBeOpened = function (editor) {
    //    var selector = editor.selector ? editor.selector : editor.schema
    //    setSelectedSchema(selector);
    //}

    /*****************************************************************************/
    /* Call function into the front-end   */
    /*****************************************************************************/

    var loadGoogleFontInFront = function (font) {
        if (document.getElementById("resultFrame").contentWindow.getFont)
            document.getElementById("resultFrame").contentWindow.getFont(font);
    }

    var setOutlinePosition = function (schema) {
        if (document.getElementById("resultFrame").contentWindow.setOutlinePosition)
            document.getElementById("resultFrame").contentWindow.setOutlinePosition(schema);
    }

    var setSelectedSchema = function (schema) {
        if (document.getElementById("resultFrame").contentWindow.setSelectedSchema)
            document.getElementById("resultFrame").contentWindow.setSelectedSchema(schema);
    }

    var refreshFrontStyles = function (parameters) {
        if (document.getElementById("resultFrame").contentWindow.refrechLayout)
            document.getElementById("resultFrame").contentWindow.refrechLayout(parameters);
    }

    $scope.openIntelTuning = function () {
        if (document.getElementById("resultFrame").contentWindow.initIntelTuning)
            document.getElementById("resultFrame").contentWindow.initIntelTuning($scope.tuningModel);

        // Refrech layout of selected element
        if ($scope.currentSelected) {
            setSelectedSchema($scope.currentSelected);
        }

    }

    $scope.closeIntelTuning = function () {
        if (document.getElementById("resultFrame").contentWindow.closeIntelTuning)
            document.getElementById("resultFrame").contentWindow.closeIntelTuning($scope.tuningModel);
        $scope.outlinePositionHide();
        $scope.outlineSelectedHide();
    }

    $scope.outlinePositionHide = function () {
        if (document.getElementById("resultFrame").contentWindow.outlinePositionHide)
            document.getElementById("resultFrame").contentWindow.outlinePositionHide();
    }

    $scope.outlineSelectedHide = function () {
        if (document.getElementById("resultFrame").contentWindow.outlineSelectedHide)
            document.getElementById("resultFrame").contentWindow.outlineSelectedHide();
    }

    /*****************************************************************************/
    /* Google font loader, TODO: put together from directive, front and back */
    /*****************************************************************************/

    var webFontScriptLoaded = false;
    var loadGoogleFont = function (font) {
        if (!webFontScriptLoaded) {
            $.getScript('http://ajax.googleapis.com/ajax/libs/webfont/1/webfont.js')
            .done(function () {
                webFontScriptLoaded = true;
                // Recursively call once webfont script is available.
                loadGoogleFont(font);
            })
            .fail(function () {
                console.log('error loading webfont');
            });
        }
        else {
            WebFont.load({
                google: {
                    families: [font]
                },
                loading: function () {
                    console.log('loading font' + font + ' in UI designer');
                },
                active: function () {
                    console.log('loaded font ' + font + ' in UI designer');
                },
                inactive: function () {
                    console.log('error loading font ' + font + ' in UI designer');
                }
            });
        }
    }

    /*****************************************************************************/
    /* Init */
    /*****************************************************************************/

    // Preload of the google font
    $http.get('/Umbraco/Api/tuning/GetGoogleFont').success(function (data) {
        $scope.googleFontFamilies = data;
    })

    // watch framLoaded, only if iframe page have EnableTuning()
    $scope.$watch("enableTuning", function () {
        $timeout(function () {
            if ($scope.enableTuning > 0) {
                



                $.each($scope.tuningModel.configs, function (indexConfig, config) {
                    $.each(config.editors, function (indexItem, item) {

                        /* get distinct dategoryies */
                        if (item.category) {
                            if ($.inArray(item.category, $scope.propertyCategories) < 0) {
                                $scope.propertyCategories.splice($scope.propertyCategories.length + 1, 0, item.category);
                            }
                        }

                    })
                });





                $scope.$watch('ngRepeatFinished', function (ngRepeatFinishedEvent) {
                    $scope.initTuning();
                });


                $scope.$watch('tuningModel', function () {
                    refreshtuning();
                }, true);
            }
        }, 200);
    }, true)

})

.directive('onFinishRenderFilters', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            if (scope.$last === true) {
                $timeout(function () {
                    scope.$emit('ngRepeatFinished');
                });
            }
        }
    }
});


