
/*********************************************************************************************************/
/* Canvasdesigner panel app and controller */
/*********************************************************************************************************/

var app = angular.module("Umbraco.canvasdesigner", ['colorpicker', 'ui.slider', 'umbraco.resources', 'umbraco.services'])

.controller("Umbraco.canvasdesignerController", function ($scope, $http, $window, $timeout, $location, dialogService) {

    var isInit = $location.search().init;
    if (isInit === "true") {
        //do not continue, this is the first load of this new window, if this is passed in it means it's been
        //initialized by the content editor and then the content editor will actually re-load this window without
        //this flag. This is a required trick to get around chrome popup mgr. We don't want to double load preview.aspx
        //since that will double prepare the preview documents
        return;
    }

    $scope.isOpen = false;
    $scope.frameLoaded = false;
    $scope.enableCanvasdesigner = 0;
    $scope.googleFontFamilies = {};
    var pageId = $location.search().id;    
    $scope.pageId = pageId;
    $scope.pageUrl = "../dialogs/Preview.aspx?id=" + pageId;
    $scope.valueAreLoaded = false;
    $scope.devices = [
        { name: "desktop", css: "desktop", icon: "icon-display", title: "Desktop" },
        { name: "laptop - 1366px", css: "laptop border", icon: "icon-laptop", title: "Laptop" },
        { name: "iPad portrait - 768px", css: "iPad-portrait border", icon: "icon-ipad", title: "Tablet portrait" },
        { name: "iPad landscape - 1024px", css: "iPad-landscape border", icon: "icon-ipad flip", title: "Tablet landscape" },
        { name: "smartphone portrait - 480px", css: "smartphone-portrait border", icon: "icon-iphone", title: "Smartphone portrait" },
        { name: "smartphone landscape  - 320px", css: "smartphone-landscape border", icon: "icon-iphone flip", title: "Smartphone landscape" }
    ];
    $scope.previewDevice = $scope.devices[0];

    var apiController = "../Api/Canvasdesigner/";

    /*****************************************************************************/
    /* Preview devices */
    /*****************************************************************************/

    // Set preview device
    $scope.updatePreviewDevice = function (device) {
        $scope.previewDevice = device;
    };

    /*****************************************************************************/
    /* Exit Preview */
    /*****************************************************************************/

    $scope.exitPreview = function () {
        window.top.location.href = "../endPreview.aspx?redir=%2f" + $scope.pageId;
    };

    /*****************************************************************************/
    /* UI designer managment */
    /*****************************************************************************/

    // Update all Canvasdesigner config's values from data
    var updateConfigValue = function (data) {

        var fonts = [];

        $.each($scope.canvasdesignerModel.configs, function (indexConfig, config) {
            if (config.editors) {
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

                });
            }

        });

        // Load google font
        $.each(fonts, function (indexFont, font) {
            loadGoogleFont(font);
            loadGoogleFontInFront(font);
        });

        $scope.valueAreLoaded = true;

    };

    // Load parameters from GetLessParameters and init data of the Canvasdesigner config
    $scope.initCanvasdesigner = function () {

        LazyLoad.js(['https://ajax.googleapis.com/ajax/libs/webfont/1/webfont.js']);

        $http.get(apiController + 'Load', { params: { pageId: $scope.pageId } })
            .success(function (data) {

                updateConfigValue(data);

                $timeout(function () {
                    $scope.frameLoaded = true;
                }, 200);

            });

    };

    // Refresh all less parameters for every changes watching canvasdesignerModel
    var refreshCanvasdesigner = function () {

        var parameters = [];

        if ($scope.canvasdesignerModel) {

            angular.forEach($scope.canvasdesignerModel.configs, function (config, indexConfig) {

                // Get currrent selected element
                // TODO
                //if ($scope.schemaFocus && angular.lowercase($scope.schemaFocus) == angular.lowercase(config.name)) {
                //    $scope.currentSelected = config.selector ? config.selector : config.schema;
                //}

                if (config.editors) {
                    angular.forEach(config.editors, function (item, indexItem) {

                        // Add new style
                        if (item.values) {
                            angular.forEach(Object.keys(item.values), function (key, indexKey) {
                                var propertyAlias = key.toLowerCase() + item.alias.toLowerCase();
                                var value = eval("item.values." + key);
                                parameters.splice(parameters.length + 1, 0, "'@" + propertyAlias + "':'" + value + "'");
                            })
                        }
                    });
                }

            });

            // Refresh page style
            refreshFrontStyles(parameters);

            // Refresh layout of selected element
            //$timeout(function () {
            $scope.positionSelectedHide();
                if ($scope.currentSelected) {
                    refreshOutlineSelected($scope.currentSelected);
                }
            //}, 200);



        }
    }

    $scope.createStyle = function (){
        $scope.saveLessParameters(false);
    }

    $scope.saveStyle = function () {
        $scope.saveLessParameters(true);
    }

    // Save all parameter in CanvasdesignerParameters.less file
    $scope.saveLessParameters = function (inherited) {

        var parameters = [];
        $.each($scope.canvasdesignerModel.configs, function (indexConfig, config) {
            if (config.editors) {
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
                            var gimport = "@import url('https://fonts.googleapis.com/css?family=" + item.values.fontFamily + variant + "');";
                            if ($.inArray(gimport, parameters) < 0) {
                                parameters.splice(0, 0, gimport);
                            }
                        }
                    }

                });
            }
        });

        var resultParameters = { parameters: parameters.join(""), pageId: $scope.pageId, inherited: inherited };
        var transform = function (result) {
            return $.param(result);
        }

        $('.btn-default-save').attr("disabled", true);
        $http.defaults.headers.post["Content-Type"] = "application/x-www-form-urlencoded";
        $http.post(apiController + 'Save', resultParameters, {
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            transformRequest: transform
        })
        .success(function (data) {
            $('.btn-default-save').attr("disabled", false);
            $('#speechbubble').fadeIn('slow').delay(5000).fadeOut('slow');
        });

    }

    // Delete current page Canvasdesigner
    $scope.deleteCanvasdesigner = function () {
        $('.btn-default-delete').attr("disabled", true);
        $http.get(apiController + 'Delete', { params: { pageId: $scope.pageId } })
        .success(function (data) {
            location.reload();
        })
    }


    /*****************************************************************************/
    /* Preset design */
    /*****************************************************************************/

    // Refresh with selected Canvasdesigner palette
    $scope.refreshCanvasdesignerByPalette = function (palette) {
        updateConfigValue(palette.data);
        refreshCanvasdesigner();
    };

    // Hidden botton to make preset from the current settings
    $scope.makePreset = function () {

        var parameters = [];
        $.each($scope.canvasdesignerModel.configs, function (indexConfig, config) {
            if (config.editors) {
                $.each(config.editors, function (indexItem, item) {
                    if (item.values) {
                        angular.forEach(Object.keys(item.values), function (key, indexKey) {
                            var propertyAlias = key.toLowerCase() + item.alias.toLowerCase();
                            var value = eval("item.values." + key);
                            var value = (value != 0 && (value == undefined || value == "")) ? "''" : value;
                            parameters.splice(parameters.length + 1, 0, "\"" + propertyAlias + "\":" + " \"" + value + "\"");
                        })
                    }
                });
            }
        });

        $(".btn-group").append("<textarea>{name:\"\", color1:\"\", color2:\"\", color3:\"\", color4:\"\", color5:\"\", data:{" + parameters.join(",") + "}}</textarea>");

    };

    /*****************************************************************************/
    /* Panel managment */
    /*****************************************************************************/

    $scope.openPreviewDevice = function () {
        $scope.showDevicesPreview = true;
        $scope.closeIntelCanvasdesigner();
    }

    $scope.closePreviewDevice = function(){
        $scope.showDevicesPreview = false;
        if ($scope.showStyleEditor) {
            $scope.openIntelCanvasdesigner();
        }
    }

    $scope.openPalettePicker = function () {
        $scope.showPalettePicker = true;
        $scope.showStyleEditor = false;
        $scope.closeIntelCanvasdesigner();
    };

    $scope.openStyleEditor = function () {
        $scope.showStyleEditor = true;
        $scope.showPalettePicker = false;
        $scope.outlineSelectedHide()
        $scope.openIntelCanvasdesigner()
    }

    // Remove value from field
    $scope.removeField = function (field) {
        field.value = "";
    };

    // Check if category existe
    $scope.hasEditor = function (editors, category) {
        var result = false;
        angular.forEach(editors, function (item, index) {
            if (item.category == category) {
                result = true;
            }
        });
        return result;
    };

    $scope.closeFloatPanels = function () {

        /* hack to hide color picker */
        $(".spectrumcolorpicker input").spectrum("hide");

        dialogService.close();
        $scope.showPalettePicker = false;
        $scope.$apply();
    };

    $scope.clearHighlightedItems = function () {
        $.each($scope.canvasdesignerModel.configs, function (indexConfig, config) {
            config.highlighted = false;
        });
    }

    $scope.setCurrentHighlighted = function (item) {
        $scope.clearHighlightedItems();
        item.highlighted = true;
    }

    $scope.setCurrentSelected = function(item) {
        $scope.currentSelected = item;
        $scope.clearSelectedCategory();
        refreshOutlineSelected($scope.currentSelected);
    }

    /* Editor categories */

    $scope.getCategories = function (item) {

        var propertyCategories = [];

        $.each(item.editors, function (indexItem, editor) {
            if (editor.category) {
                if ($.inArray(editor.category, propertyCategories) < 0) {
                    propertyCategories.splice( propertyCategories.length + 1, 0, editor.category);
                }
            }
        });

        return propertyCategories;

    }

    $scope.setSelectedCategory = function (item) {
        $scope.categoriesVisibility = $scope.categoriesVisibility || {};
        $scope.categoriesVisibility[item] = !$scope.categoriesVisibility[item];
    }

    $scope.clearSelectedCategory = function () {
        $scope.categoriesVisibility = null;
    }

    /*****************************************************************************/
    /* Call function into the front-end   */
    /*****************************************************************************/

    var loadGoogleFontInFront = function (font) {
        if (document.getElementById("resultFrame").contentWindow.getFont)
            document.getElementById("resultFrame").contentWindow.getFont(font);
    };

    var refreshFrontStyles = function (parameters) {
        if (document.getElementById("resultFrame").contentWindow.refreshLayout)
            document.getElementById("resultFrame").contentWindow.refreshLayout(parameters);
    };

    var hideUmbracoPreviewBadge = function () {
        var iframe = (document.getElementById("resultFrame").contentWindow || document.getElementById("resultFrame").contentDocument);
        if(iframe.document.getElementById("umbracoPreviewBadge"))
			iframe.document.getElementById("umbracoPreviewBadge").style.display = "none";
    };

    $scope.openIntelCanvasdesigner = function () {
        if (document.getElementById("resultFrame").contentWindow.initIntelCanvasdesigner)
            document.getElementById("resultFrame").contentWindow.initIntelCanvasdesigner($scope.canvasdesignerModel);
    };

    $scope.closeIntelCanvasdesigner = function () {
        if (document.getElementById("resultFrame").contentWindow.closeIntelCanvasdesigner)
            document.getElementById("resultFrame").contentWindow.closeIntelCanvasdesigner($scope.canvasdesignerModel);
        $scope.outlineSelectedHide();
    };

    var refreshOutlineSelected = function (config) {
        var schema = config.selector ? config.selector : config.schema;
        if (document.getElementById("resultFrame").contentWindow.refreshOutlineSelected)
            document.getElementById("resultFrame").contentWindow.refreshOutlineSelected(schema);
    }

    $scope.outlineSelectedHide = function () {
        $scope.currentSelected = null;
        if (document.getElementById("resultFrame").contentWindow.outlineSelectedHide)
            document.getElementById("resultFrame").contentWindow.outlineSelectedHide();
    };

    $scope.refreshOutlinePosition = function (config) {
        var schema = config.selector ? config.selector : config.schema;
        if (document.getElementById("resultFrame").contentWindow.refreshOutlinePosition)
            document.getElementById("resultFrame").contentWindow.refreshOutlinePosition(schema);
    }

    $scope.positionSelectedHide = function () {
        if (document.getElementById("resultFrame").contentWindow.outlinePositionHide)
            document.getElementById("resultFrame").contentWindow.outlinePositionHide();
    }

    /*****************************************************************************/
    /* Google font loader, TODO: put together from directive, front and back */
    /*****************************************************************************/

    var webFontScriptLoaded = false;
    var loadGoogleFont = function (font) {

        if (!webFontScriptLoaded) {
            $.getScript('https://ajax.googleapis.com/ajax/libs/webfont/1/webfont.js')
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

    };

    /*****************************************************************************/
    /* Init */
    /*****************************************************************************/

    // Preload of the google font
    if ($scope.showStyleEditor) {
        $http.get(apiController + 'GetGoogleFont').success(function (data) {
            $scope.googleFontFamilies = data;
        });
    }

    // watch framLoaded, only if iframe page have enableCanvasdesigner()
    $scope.$watch("enableCanvasdesigner", function () {
        $timeout(function () {
            if ($scope.enableCanvasdesigner > 0) {

                $scope.$watch('ngRepeatFinished', function (ngRepeatFinishedEvent) {
                    $timeout(function () {
                        $scope.initCanvasdesigner();
                    }, 200);
                });

                $scope.$watch('canvasdesignerModel', function () {
                    refreshCanvasdesigner();
                }, true);

            }
        }, 100);
    }, true);

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
    };
})

.directive('iframeIsLoaded', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            element.load(function () {
                var iframe = (element.context.contentWindow || element.context.contentDocument);
                if(iframe && iframe.document.getElementById("umbracoPreviewBadge"))
					iframe.document.getElementById("umbracoPreviewBadge").style.display = "none";
                if (!document.getElementById("resultFrame").contentWindow.refreshLayout) {
                    scope.frameLoaded = true;
                    scope.$apply();
                }
            });
        }
    };
})
