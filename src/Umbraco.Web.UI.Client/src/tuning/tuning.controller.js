
/*********************************************************************************************************/
/* tuning panel app and controller */
/*********************************************************************************************************/

var app = angular.module("umbraco.tuning", ['spectrumcolorpicker', 'ui.slider', 'umbraco.resources', 'umbraco.services'])

.controller("Umbraco.tuningController", function ($scope, $http, $window, $timeout, $location, dialogService) {

    $scope.isOpen = false;
    $scope.frameLoaded = false;
    $scope.enableTuning = 0;
    $scope.schemaFocus = "body";
    $scope.settingIsOpen = 'previewDevice';
    $scope.propertyCategories = [];
    $scope.googleFontFamilies = {};
    $scope.pageId = $location.search().id;
    $scope.pageUrl = "../dialogs/Preview.aspx?id=" + $location.search().id;
    $scope.valueAreLoaded = false;
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

        $scope.valueAreLoaded = true;

    }

    // Load parameters from GetLessParameters and init data of the tuning config
    $scope.initTuning = function () {

        $http.get('/Umbraco/Api/tuning/Load', { params: { pageId: $scope.pageId } })
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
                setSelectedSchema();
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
                    if (item.type == "googlefontpicker" && item.values.fontFamily) {
                        var variant = item.values.fontWeight != "" || item.values.fontStyle != "" ? ":" + item.values.fontWeight + item.values.fontStyle : "";
                        var gimport = "@import url('http://fonts.googleapis.com/css?family=" + item.values.fontFamily + variant + "');";
                        if ($.inArray(gimport, parameters) < 0) {
                            parameters.splice(0, 0, gimport);
                        }
                    }

                }

            })
        });

        var resultParameters = { parameters: parameters.join(""), pageId: $scope.pageId, inherited: inherited };
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
        $http.get('/Umbraco/Api/tuning/Delete', { params: { pageId: $scope.pageId } })
        .success(function (data) {
            $scope.enableTuning++;
            $scope.pageUrl = $scope.pageUrl + "&n=123456";
            $('.btn-default-delete').attr("disabled", false);
        })
    }


    /*****************************************************************************/
    /* Preset design */
    /*****************************************************************************/

    // Refresh with selected tuning palette
    $scope.refreshtuningByPalette = function (palette) {
        updateConfigValue(palette.data);
        refreshtuning();
    }

    // Hidden botton to make preset from the current settings
    $scope.makePreset = function () {

        var parameters = [];
        $.each($scope.tuningModel.configs, function (indexConfig, config) {
            $.each(config.editors, function (indexItem, item) {
                if (item.values) {
                    angular.forEach(Object.keys(item.values), function (key, indexKey) {
                        var propertyAlias = key.toLowerCase() + item.alias.toLowerCase();
                        var value = eval("item.values." + key);
                        var value = (value != 0 && (value == undefined || value == "")) ? "''" : value;
                        parameters.splice(parameters.length + 1, 0, "\"" + propertyAlias + "\":" + " \"" + value + "\"");
                    })
                }
            })
        });

        $(".btn-group").append("<textarea>{name:\"\", color1:\"\", color2:\"\", color3:\"\", color4:\"\", color5:\"\", data:{" + parameters.join(",") + "}}</textarea>");

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

    // Remove value from field
    $scope.removeField = function (field) {
        field.value = "";
    }

    // Check if category existe
    $scope.hasEditor = function (editors, category) {
        var result = false;
        angular.forEach(editors, function (item, index) {        
            if (item.category == category) {
                result = true;
            }
        });
        return result;
    }

    $scope.closeFloatPanels = function () {

        /* hack to hide color picker */
        $(".spectrumcolorpicker input").spectrum("hide");

        dialogService.close();
        $scope.showPalettePicker = false;
        $scope.$apply();
    }

    /*****************************************************************************/
    /* Call function into the front-end   */
    /*****************************************************************************/

    var loadGoogleFontInFront = function (font) {
        if (document.getElementById("resultFrame").contentWindow.getFont)
            document.getElementById("resultFrame").contentWindow.getFont(font);
    }

    var setSelectedSchema = function () {
        if (document.getElementById("resultFrame").contentWindow.outlineSelected)
            document.getElementById("resultFrame").contentWindow.outlineSelected();
    }

    var refreshFrontStyles = function (parameters) {
        if (document.getElementById("resultFrame").contentWindow.refrechLayout)
            document.getElementById("resultFrame").contentWindow.refrechLayout(parameters);
    }

    var hideUmbracoPreviewBadge = function () {
        var iframe = (document.getElementById("resultFrame").contentWindow || document.getElementById("resultFrame").contentDocument);
        iframe.document.getElementById("umbracoPreviewBadge").style.display = "none";
    }

    $scope.openIntelTuning = function () {
        if (document.getElementById("resultFrame").contentWindow.initIntelTuning)
            document.getElementById("resultFrame").contentWindow.initIntelTuning($scope.tuningModel);

        // Refrech layout of selected element
        if ($scope.currentSelected) {
            setSelectedSchema();
        }

    }

    $scope.closeIntelTuning = function () {
        if (document.getElementById("resultFrame").contentWindow.closeIntelTuning)
            document.getElementById("resultFrame").contentWindow.closeIntelTuning($scope.tuningModel);
        $scope.outlineSelectedHide();
    }

    $scope.outlineSelectedHide = function () {
        if (document.getElementById("resultFrame").contentWindow.outlineSelectedHide)
            document.getElementById("resultFrame").contentWindow.outlineSelectedHide();
        $scope.schemaFocus = "body";
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
                    //console.log('loading font' + font + ' in UI designer');
                },
                active: function () {
                    //console.log('loaded font ' + font + ' in UI designer');
                },
                inactive: function () {
                    //console.log('error loading font ' + font + ' in UI designer');
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
                    $timeout(function () {
                        $scope.initTuning();
                    }, 200); 
                });

                $scope.$watch('tuningModel', function () {
                    refreshtuning();
                }, true);

            }
        }, 100);
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
})

.directive('iframeIsLoaded', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            element.load(function () {
                var iframe = (element.context.contentWindow || element.context.contentDocument);
                iframe.document.getElementById("umbracoPreviewBadge").style.display = "none";
                if (!document.getElementById("resultFrame").contentWindow.refrechLayout) {
                    scope.frameLoaded = true;
                    scope.$apply();
                }
            });
        }
    }
});


