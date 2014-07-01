
/*********************************************************************************************************/
/* tuning panel app and controller */
/*********************************************************************************************************/

angular.module("umbraco.tuning", ['ui.bootstrap', 'spectrumcolorpicker', 'ui.slider', 'umbraco.resources', 'umbraco.services'])

.controller("Umbraco.tuningController", function ($scope, $modal, $http, $window, $timeout, $location) {

    $scope.isOpen = false;
    $scope.frameLoaded = false;
    $scope.enableTuning = 0;
    $scope.schemaFocus = "body";
    $scope.settingIsOpen = 'previewDevice';
    $scope.BackgroundPositions = ['center', 'left', 'right', 'bottom center', 'bottom left', 'bottom right', 'top center', 'top left', 'top right'];
    $scope.BackgroundRepeats = ['no-repeat', 'repeat', 'repeat-x', 'repeat-y'];
    $scope.BackgroundAttachments = ['scroll', 'fixed'];
    $scope.Layouts = ['boxed', 'wide', 'full'];
    $scope.displays = ['float-left', 'float-right', 'block-left', 'block-right', 'none'];
    $scope.optionHomes = ['icon', 'text', 'none'];
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
                $scope.frameLoaded = true;

                if ($scope.settingIsOpen == "setting") {
                    openIntelTuning();
                }

            });

    }

    // Refresh all less parameters for every changes watching tuningModel 
    var refreshtuning = function () {
        var parameters = [];

        if ($scope.tuningModel) {


            angular.forEach($scope.tuningModel.configs, function (config, indexConfig) {
                angular.forEach(config.editors, function (item, indexItem) {
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

    // Accordion open event
    $scope.accordionOpened = function (schema) {
        $scope.schemaFocus = schema;
    }

    // Focus schema in front
    $scope.accordionWillBeOpened = function (editor) {
        var selector = editor.selector ? editor.selector : editor.schema
        setSelectedSchema(selector);
    }

    /*****************************************************************************/
    /* Call function into the front-end   */
    /*****************************************************************************/

    var openIntelTuning = function () {
        if (document.getElementById("resultFrame").contentWindow.initIntelTuning)
            document.getElementById("resultFrame").contentWindow.initIntelTuning($scope.tuningModel);
    }

    var closeIntelTuning = function () {
        if (document.getElementById("resultFrame").contentWindow.closeIntelTuning)
            document.getElementById("resultFrame").contentWindow.closeIntelTuning($scope.tuningModel);
    }

    var loadGoogleFontInFront = function (font) {
        if (document.getElementById("resultFrame").contentWindow.getFont)
            document.getElementById("resultFrame").contentWindow.getFont(font);
    }

    var setSelectedSchema = function (schema) {
        if (document.getElementById("resultFrame").contentWindow.setSelectedSchema)
            document.getElementById("resultFrame").contentWindow.setSelectedSchema(schema);
    }

    var refreshFrontStyles = function (parameters) {
        if (document.getElementById("resultFrame").contentWindow.refrechLayout)
            document.getElementById("resultFrame").contentWindow.refrechLayout(parameters);
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
                $scope.initTuning();
                $scope.$watch('tuningModel', function () {
                    refreshtuning();
                }, true);
            }
        }, 200);
    }, true)

})
