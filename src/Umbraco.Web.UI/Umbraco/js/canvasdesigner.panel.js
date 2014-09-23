
/*********************************************************************************************************/
/* Global function and variable for panel/page com  */
/*********************************************************************************************************/

/* Called for every tuning-over rollover */
var refrechIntelTuning = function (name) {

    var scope = angular.element($("#tuningPanel")).scope();

    if (scope.schemaFocus != name.toLowerCase()) {

        var notFound = true;
        $.each(scope.tuningModel.configs, function (indexConfig, config) {
            if (config.name && name.toLowerCase() == config.name.toLowerCase()) {
                notFound = false
            }
        });

        if (notFound) {
            scope.schemaFocus = "body";
        }
        else {
            scope.schemaFocus = name.toLowerCase();
        }

    }

    scope.closeFloatPanels();

    scope.$apply();

}

/* Called when the iframe is first loaded */
var setFrameIsLoaded = function (tuningConfig, tuningPalette) {

    var scope = angular.element($("#tuningPanel")).scope();

    scope.tuningModel = tuningConfig;
    scope.tuningPalette = tuningPalette;
    scope.enableTuning++;
    scope.$apply();
}

/* Iframe body click */
var iframeBodyClick = function () {

    var scope = angular.element($("#tuningPanel")).scope();

    scope.closeFloatPanels();
}


/*********************************************************************************************************/
/* tuning panel app and controller */
/*********************************************************************************************************/

var app = angular.module("Umbraco.canvasdesigner", ['spectrumcolorpicker', 'ui.slider', 'umbraco.resources', 'umbraco.services'])

.controller("Umbraco.canvasdesignerController", function ($scope, $http, $window, $timeout, $location, dialogService) {

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

    var apiController = "/Umbraco/Api/CanvasDesigner/";

    /*****************************************************************************/
    /* Preview devices */
    /*****************************************************************************/

    // Set preview device
    $scope.updatePreviewDevice = function (device) {
        $scope.previewDevice = device;
    };

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
                        });
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
        });

        // Load google font
        $.each(fonts, function (indexFont, font) {
            loadGoogleFont(font);
            loadGoogleFontInFront(font);
        });

        $scope.valueAreLoaded = true;

    };

    // Load parameters from GetLessParameters and init data of the tuning config
    $scope.initTuning = function () {

        $http.get(apiController + 'Load', { params: { pageId: $scope.pageId } })
            .success(function (data) {

                updateConfigValue(data);

                $timeout(function () {
                    $scope.frameLoaded = true;
                }, 200);

            });

    };

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
        $http.post(apiController + 'Save', resultParameters, {
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
        $http.get(apiController + 'Delete', { params: { pageId: $scope.pageId } })
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
    };

    // Hidden botton to make preset from the current settings
    $scope.makePreset = function () {

        var parameters = [];
        $.each($scope.tuningModel.configs, function (indexConfig, config) {
            $.each(config.editors, function (indexItem, item) {
                if (item.values) {
                    angular.forEach(Object.keys(item.values), function (key, indexKey) {
                        var propertyAlias = key.toLowerCase() + item.alias.toLowerCase();
                        var value = eval("item.values." + key);
                        value = (value !== 0 && (value === undefined || value === "")) ? "''" : value;
                        parameters.splice(parameters.length + 1, 0, "\"" + propertyAlias + "\":" + " \"" + value + "\"");
                    });
                }
            });
        });

        $(".btn-group").append("<textarea>{name:\"\", color1:\"\", color2:\"\", color3:\"\", color4:\"\", color5:\"\", data:{" + parameters.join(",") + "}}</textarea>");

    };

    /*****************************************************************************/
    /* Panel managment */
    /*****************************************************************************/

    $scope.openPreviewDevice = function () {
        $scope.showDevicesPreview = true;
        $scope.closeIntelTuning();
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
        $scope.closeIntelTuning();
    };

    $scope.openStyleEditor = function () {
        $scope.showStyleEditor = true;
        $scope.showPalettePicker = false;
        $scope.openIntelTuning();
    };

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

    /*****************************************************************************/
    /* Call function into the front-end   */
    /*****************************************************************************/

    var loadGoogleFontInFront = function (font) {
        if (document.getElementById("resultFrame").contentWindow.getFont)
            document.getElementById("resultFrame").contentWindow.getFont(font);
    };

    var setSelectedSchema = function () {
        if (document.getElementById("resultFrame").contentWindow.outlineSelected)
            document.getElementById("resultFrame").contentWindow.outlineSelected();
    };

    var refreshFrontStyles = function (parameters) {
        if (document.getElementById("resultFrame").contentWindow.refrechLayout)
            document.getElementById("resultFrame").contentWindow.refrechLayout(parameters);
    };

    var hideUmbracoPreviewBadge = function () {
        var iframe = (document.getElementById("resultFrame").contentWindow || document.getElementById("resultFrame").contentDocument);
        if(iframe.document.getElementById("umbracoPreviewBadge"))
			iframe.document.getElementById("umbracoPreviewBadge").style.display = "none";
    };

    $scope.openIntelTuning = function () {
        if (document.getElementById("resultFrame").contentWindow.initIntelTuning)
            document.getElementById("resultFrame").contentWindow.initIntelTuning($scope.tuningModel);

        // Refrech layout of selected element
        if ($scope.currentSelected) {
            setSelectedSchema();
        }

    };

    $scope.closeIntelTuning = function () {
        if (document.getElementById("resultFrame").contentWindow.closeIntelTuning)
            document.getElementById("resultFrame").contentWindow.closeIntelTuning($scope.tuningModel);
        $scope.outlineSelectedHide();
    };

    $scope.outlineSelectedHide = function () {
        if (document.getElementById("resultFrame").contentWindow.outlineSelectedHide)
            document.getElementById("resultFrame").contentWindow.outlineSelectedHide();
        $scope.schemaFocus = "body";
    };



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
    };

    /*****************************************************************************/
    /* Init */
    /*****************************************************************************/

    // Preload of the google font
    $http.get(apiController + 'GetGoogleFont').success(function (data) {
        $scope.googleFontFamilies = data;
    });

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

                    });
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
                if (!document.getElementById("resultFrame").contentWindow.refrechLayout) {
                    scope.frameLoaded = true;
                    scope.$apply();
                }
            });
        }
    };
});


/*********************************************************************************************************/
/* Background editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.background", function ($scope, dialogService) {

    if (!$scope.item.values) {
        $scope.item.values = {
            imageorpattern: '',
            color:''
        };
    }

    $scope.open = function (field) {

        var config = {
            template: "mediaPickerModal.html",
            change: function (data) {
                $scope.item.values.imageorpattern = data;
            },
            callback: function (data) {
                $scope.item.values.imageorpattern = data;
            },
            cancel: function (data) {
                $scope.item.values.imageorpattern = data;
            },
            dialogData: $scope.googleFontFamilies,
            dialogItem: $scope.item.values.imageorpattern
        };

        dialogService.open(config);

    };

})

.controller('tuning.mediaPickerModal', function ($scope, $http, mediaResource, umbRequestHelper, entityResource, mediaHelper) {

    if (mediaHelper && mediaHelper.registerFileResolver) {
        mediaHelper.registerFileResolver("Umbraco.UploadField", function (property, entity, thumbnail) {
            if (thumbnail) {

                if (mediaHelper.detectIfImageByExtension(property.value)) {
                    var thumbnailUrl = umbRequestHelper.getApiUrl(
                        "imagesApiBaseUrl",
                        "GetBigThumbnail",
                        [{ originalImagePath: property.value }]);

                    return thumbnailUrl;
                }
                else {
                    return null;
                }

            }
            else {
                return property.value;
            }
        });
    }

    var modalFieldvalue = $scope.dialogItem;

    $scope.currentFolder = {};
    $scope.currentFolder.children = [];
    $scope.currentPath = [];
    $scope.startNodeId = -1;

    $scope.options = {
        url: umbRequestHelper.getApiUrl("mediaApiBaseUrl", "PostAddFile"),
        formData: {
            currentFolder: $scope.startNodeId
        }
    };

    //preload selected item
    $scope.selectedMedia = undefined;

    $scope.submitFolder = function (e) {
        if (e.keyCode === 13) {
            e.preventDefault();
            $scope.$parent.data.showFolderInput = false;

            if ($scope.$parent.data.newFolder && $scope.$parent.data.newFolder != "") {
                mediaResource
                    .addFolder($scope.$parent.data.newFolder, $scope.currentFolder.id)
                    .then(function (data) {
                        $scope.$parent.data.newFolder = undefined;
                        $scope.gotoFolder(data);
                    });
            }
        }
    };

    $scope.gotoFolder = function (folder) {

        if (!folder) {
            folder = { id: $scope.startNodeId, name: "Media", icon: "icon-folder" };
        }

        if (folder.id > 0) {
            var matches = _.filter($scope.currentPath, function (value, index) {
                if (value.id == folder.id) {
                    value.indexInPath = index;
                    return value;
                }
            });

            if (matches && matches.length > 0) {
                $scope.currentPath = $scope.currentPath.slice(0, matches[0].indexInPath + 1);
            }
            else {
                $scope.currentPath.push(folder);
            }
        }
        else {
            $scope.currentPath = [];
        }

        //mediaResource.rootMedia()
        mediaResource.getChildren(folder.id)
            .then(function (data) {
                folder.children = data.items ? data.items : [];

                angular.forEach(folder.children, function (child) {
                    child.isFolder = child.contentTypeAlias == "Folder" ? true : false;
                    if (!child.isFolder) {
                        angular.forEach(child.properties, function (property) {
                            // TODO, resolve with thumbnail
                            if (property.alias = "umbracoFile" && property.value.src)
                            {
                                child.thumbnail = property.value.src;
                                child.image = property.value.src;
                            }
                        })
                    }
                });

                $scope.options.formData.currentFolder = folder.id;
                $scope.currentFolder = folder;
            });
    };

    $scope.iconFolder = "glyphicons-icon folder-open"

    $scope.selectMedia = function (media) {

        if (!media.isFolder) {
            //we have 3 options add to collection (if multi) show details, or submit it right back to the callback
            $scope.selectedMedia = media;
            modalFieldvalue = "url(" + $scope.selectedMedia.image + ")";
            $scope.change(modalFieldvalue);
        }
        else {
            $scope.gotoFolder(media);
        }

    };

    //default root item
    if (!$scope.selectedMedia) {
        $scope.gotoFolder();
    }

    $scope.submitAndClose = function () {
        if (modalFieldvalue != "") {
            $scope.submit(modalFieldvalue);
        } else {
            $scope.cancel();
        }

    };

    $scope.cancelAndClose = function () {
        $scope.cancel();
    }

})


/*********************************************************************************************************/
/* Background editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.border", function ($scope, dialogService) {

    $scope.defaultBorderList = ["all", "left", "right", "top", "bottom"];
    $scope.borderList = [];

    $scope.bordertypes = ["solid", "dashed", "dotted"];
    $scope.selectedBorder = {
        name: "all",
        size: 0,
        color: '',
        type: ''
    };

    $scope.setselectedBorder = function (bordertype) {

        if (bordertype == "all") {
            $scope.selectedBorder.name="all";
            $scope.selectedBorder.size= $scope.item.values.bordersize;
            $scope.selectedBorder.color= $scope.item.values.bordercolor;
            $scope.selectedBorder.type= $scope.item.values.bordertype;
        }

        if (bordertype == "left") {
            $scope.selectedBorder.name= "left";
            $scope.selectedBorder.size= $scope.item.values.leftbordersize;
            $scope.selectedBorder.color= $scope.item.values.leftbordercolor;
            $scope.selectedBorder.type= $scope.item.values.leftbordertype;
        }

        if (bordertype == "right") {
            $scope.selectedBorder.name= "right";
            $scope.selectedBorder.size= $scope.item.values.rightbordersize;
            $scope.selectedBorder.color= $scope.item.values.rightbordercolor;
            $scope.selectedBorder.type= $scope.item.values.rightbordertype;
        }

        if (bordertype == "top") {
            $scope.selectedBorder.name= "top";
            $scope.selectedBorder.size= $scope.item.values.topbordersize;
            $scope.selectedBorder.color= $scope.item.values.topbordercolor;
            $scope.selectedBorder.type= $scope.item.values.topbordertype;
        }

        if (bordertype == "bottom") {
            $scope.selectedBorder.name= "bottom";
            $scope.selectedBorder.size= $scope.item.values.bottombordersize;
            $scope.selectedBorder.color= $scope.item.values.bottombordercolor;
            $scope.selectedBorder.type= $scope.item.values.bottombordertype;
        }

    }

    if (!$scope.item.values) {
        $scope.item.values = {
            bordersize: '',
            bordercolor: '',
            bordertype: 'solid',
            leftbordersize: '',
            leftbordercolor: '',
            leftbordertype: 'solid',
            rightbordersize: '',
            rightbordercolor: '',
            rightbordertype: 'solid',
            topbordersize: '',
            topbordercolor: '',
            topbordertype: 'solid',
            bottombordersize: '',
            bottombordercolor: '',
            bottombordertype: 'solid',
        };
    }

    if ($scope.item.enable) {
        angular.forEach($scope.defaultBorderList, function (key, indexKey) {
            if ($.inArray(key, $scope.item.enable) >= 0) {
                $scope.borderList.splice($scope.borderList.length + 1, 0, key);
            }
        })
    }
    else {
        $scope.borderList = $scope.defaultBorderList;
    }

    $scope.$watch("valueAreLoaded", function () {
        $scope.setselectedBorder($scope.borderList[0]);
    }, false);

    $scope.$watch( "selectedBorder", function () {

        if ($scope.selectedBorder.name == "all") {
            $scope.item.values.bordersize = $scope.selectedBorder.size;
            $scope.item.values.bordercolor = $scope.selectedBorder.color;
            $scope.item.values.bordertype =$scope.selectedBorder.type;
        }

        if ($scope.selectedBorder.name == "left") {
            $scope.item.values.leftbordersize = $scope.selectedBorder.size;
            $scope.item.values.leftbordercolor = $scope.selectedBorder.color;
            $scope.item.values.leftbordertype = $scope.selectedBorder.type;
        }

        if ($scope.selectedBorder.name == "right") {
            $scope.item.values.rightbordersize = $scope.selectedBorder.size;
            $scope.item.values.rightbordercolor = $scope.selectedBorder.color;
            $scope.item.values.rightbordertype = $scope.selectedBorder.type;
        }

        if ($scope.selectedBorder.name == "top") {
            $scope.item.values.topbordersize = $scope.selectedBorder.size;
            $scope.item.values.topbordercolor = $scope.selectedBorder.color;
            $scope.item.values.topbordertype = $scope.selectedBorder.type;
        }

        if ($scope.selectedBorder.name == "bottom") {
            $scope.item.values.bottombordersize = $scope.selectedBorder.size;
            $scope.item.values.bottombordercolor = $scope.selectedBorder.color;
            $scope.item.values.bottombordertype = $scope.selectedBorder.type;
        }

    }, true)

})

/*********************************************************************************************************/
/* color editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.color", function ($scope) {

    if (!$scope.item.values) {
        $scope.item.values = {
            color: '',
        };
    }

})

/*********************************************************************************************************/
/* google font editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.googlefontpicker", function ($scope, dialogService) {

    if (!$scope.item.values) {
        $scope.item.values = {
            fontFamily: '',
            fontType: '',
            fontWeight: '',
            fontStyle: '',
        }
    }

    $scope.setStyleVariant = function () {
        if ($scope.item.values != undefined) {
            return {
                'font-family': $scope.item.values.fontFamily,
                'font-weight': $scope.item.values.fontWeight,
                'font-style': $scope.item.values.fontStyle
            }
        }
    };

    $scope.open = function (field) {

        var config = {
            template: "googlefontdialog.html",
            change: function (data) {
                $scope.item.values = data;
            },
            callback: function (data) {
                $scope.item.values = data;
            },
            cancel: function (data) {
                $scope.item.values = data;
            },
            dialogData: $scope.googleFontFamilies,
            dialogItem: $scope.item.values
        };

        dialogService.open(config);

    };

})

.controller("googlefontdialog.controller", function ($scope) {

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

    angular.forEach($scope.dialogData.items, function (value, key) {
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
        if ($scope.dialogItem != undefined) {
            return {
                'font-family': $scope.selectedFont.fontFamily,
                'font-weight': $scope.selectedFont.fontWeight,
                'font-style': $scope.selectedFont.fontStyle
            }
        }
    };

    $scope.showFontPreview = function (font, variant) {

        if (!variant)
            variant = font.variant;

        if (font != undefined && font.fontFamily != "" && font.fontType == "google") {

            // Font needs to be independently loaded in the iframe for live preview to work.
            document.getElementById("resultFrame").contentWindow.getFont(font.fontFamily + ":" + variant);

            WebFont.load({
                google: {
                    families: [font.fontFamily + ":" + variant]
                },
                loading: function () {
                    console.log('loading');
                },
                active: function () {
                    $scope.selectedFont = font;
                    $scope.selectedFont.fontWeight = googleGetWeight(variant);
                    $scope.selectedFont.fontStyle = googleGetStyle(variant);
                    // If $apply isn't called, the new font family isn't applied until the next user click.
                    $scope.change({
                        fontFamily: $scope.selectedFont.fontFamily,
                        fontType: $scope.selectedFont.fontType,
                        fontWeight: $scope.selectedFont.fontWeight,
                        fontStyle: $scope.selectedFont.fontStyle,
                    });
                }
            });

        }
        else {

            // Font is available, apply it immediately in modal preview.
            $scope.selectedFont = font;
            // If $apply isn't called, the new font family isn't applied until the next user click.
            $scope.change({
                fontFamily: $scope.selectedFont.fontFamily,
                fontType: $scope.selectedFont.fontType,
                fontWeight: $scope.selectedFont.fontWeight,
                fontStyle: $scope.selectedFont.fontStyle,
            });
        }



    }

    $scope.cancelAndClose = function () {
        $scope.cancel();
    }

    $scope.submitAndClose = function () {
        $scope.submit({
            fontFamily: $scope.selectedFont.fontFamily,
            fontType: $scope.selectedFont.fontType,
            fontWeight: $scope.selectedFont.fontWeight,
            fontStyle: $scope.selectedFont.fontStyle,
        });
    };

    if ($scope.dialogItem != undefined) {
        angular.forEach($scope.fonts, function (value, key) {
            if (value.fontFamily == $scope.dialogItem.fontFamily) {
                $scope.selectedFont = value;
                $scope.selectedFont.variant = $scope.dialogItem.fontWeight + $scope.dialogItem.fontStyle;
                $scope.selectedFont.fontWeight = $scope.dialogItem.fontWeight;
                $scope.selectedFont.fontStyle = $scope.dialogItem.fontStyle;
            }
        });
    }

});

/*********************************************************************************************************/
/* grid row editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.gridRow", function ($scope, $modal) {

    if (!$scope.item.values) {
        $scope.item.values = {
            color: '',
            imageorpattern: '',
            fullSize:false
        };
    }

    // Open image picker modal
    $scope.open = function (field) {

        $scope.data = {
            newFolder: "",
            modalField: field
        };

        var modalInstance = $modal.open({
            scope: $scope,
            templateUrl: 'myModalContent.html',
            controller: 'tuning.mediapickercontroller',
            resolve: {
                items: function () {
                    return field.imageorpattern;
                }
            }
        });
        modalInstance.result.then(function (selectedItem) {
            field.imageorpattern = selectedItem;
        });
    };

})

/*********************************************************************************************************/
/* margin editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.margin", function ($scope, dialogService) {

    $scope.defaultmarginList = ["all", "left", "right", "top", "bottom"];
    $scope.marginList = [];

    $scope.selectedmargin = {
        name: "",
        value: 0,
    };

    $scope.setSelectedmargin = function (margintype) {

        if (margintype == "all") {
            $scope.selectedmargin.name = "all";
            $scope.selectedmargin.value = $scope.item.values.marginvalue;
        }

        if (margintype == "left") {
            $scope.selectedmargin.name = "left";
            $scope.selectedmargin.value = $scope.item.values.leftmarginvalue;
        }

        if (margintype == "right") {
            $scope.selectedmargin.name = "right";
            $scope.selectedmargin.value = $scope.item.values.rightmarginvalue;
        }

        if (margintype == "top") {
            $scope.selectedmargin.name = "top";
            $scope.selectedmargin.value = $scope.item.values.topmarginvalue;
        }

        if (margintype == "bottom") {
            $scope.selectedmargin.name = "bottom";
            $scope.selectedmargin.value = $scope.item.values.bottommarginvalue;
        }

    }

    if (!$scope.item.values) {
        $scope.item.values = {
            marginvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 0 ? $scope.item.defaultValue[0] : '',
            leftmarginvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 1 ? $scope.item.defaultValue[1] : '',
            rightmarginvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 2 ? $scope.item.defaultValue[2] : '',
            topmarginvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 3 ? $scope.item.defaultValue[3] : '',
            bottommarginvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 4 ? $scope.item.defaultValue[4] : '',
        };
    }

    if ($scope.item.enable) {
        angular.forEach($scope.defaultmarginList, function (key, indexKey) {
            if ($.inArray(key, $scope.item.enable) >= 0) {
                $scope.marginList.splice($scope.marginList.length + 1, 0, key);
            }
        })
    }
    else {
        $scope.marginList = $scope.defaultmarginList;
    }

    $scope.$watch("valueAreLoaded", function () {
        $scope.setSelectedmargin($scope.marginList[0]);
    }, false);

    $scope.$watch("selectedmargin", function () {

        if ($scope.selectedmargin.name == "all") {
            $scope.item.values.marginvalue = $scope.selectedmargin.value;
        }

        if ($scope.selectedmargin.name == "left") {
            $scope.item.values.leftmarginvalue = $scope.selectedmargin.value;
        }

        if ($scope.selectedmargin.name == "right") {
            $scope.item.values.rightmarginvalue = $scope.selectedmargin.value;
        }

        if ($scope.selectedmargin.name == "top") {
            $scope.item.values.topmarginvalue = $scope.selectedmargin.value;
        }

        if ($scope.selectedmargin.name == "bottom") {
            $scope.item.values.bottommarginvalue = $scope.selectedmargin.value;
        }

    }, true)



})

/*********************************************************************************************************/
/* padding editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.padding", function ($scope, dialogService) {

    $scope.defaultPaddingList = ["all", "left", "right", "top", "bottom"];
    $scope.paddingList = [];
   
    $scope.selectedpadding = {
        name: "",
        value: 0,
    };

    $scope.setSelectedpadding = function (paddingtype) {

        if (paddingtype == "all") {
            $scope.selectedpadding.name="all";
            $scope.selectedpadding.value= $scope.item.values.paddingvalue;
        }

        if (paddingtype == "left") {
            $scope.selectedpadding.name= "left";
            $scope.selectedpadding.value= $scope.item.values.leftpaddingvalue;
        }

        if (paddingtype == "right") {
            $scope.selectedpadding.name= "right";
            $scope.selectedpadding.value= $scope.item.values.rightpaddingvalue;
        }

        if (paddingtype == "top") {
            $scope.selectedpadding.name= "top";
            $scope.selectedpadding.value= $scope.item.values.toppaddingvalue;
        }

        if (paddingtype == "bottom") {
            $scope.selectedpadding.name= "bottom";
            $scope.selectedpadding.value= $scope.item.values.bottompaddingvalue;
        }

    }

    if (!$scope.item.values) {
        $scope.item.values = {
            paddingvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 0 ? $scope.item.defaultValue[0] : '',
            leftpaddingvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 1 ? $scope.item.defaultValue[1] : '',
            rightpaddingvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 2 ? $scope.item.defaultValue[2] : '',
            toppaddingvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 3 ? $scope.item.defaultValue[3] : '',
            bottompaddingvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 4 ? $scope.item.defaultValue[4] : '',
        };
    }

    if ($scope.item.enable) {
        angular.forEach($scope.defaultPaddingList, function (key, indexKey) {
            if ($.inArray(key, $scope.item.enable) >= 0) {
                $scope.paddingList.splice($scope.paddingList.length + 1, 0, key);
            }
        })
    }
    else {
        $scope.paddingList = $scope.defaultPaddingList;
    }

    $scope.$watch("valueAreLoaded", function () {
        $scope.setSelectedpadding($scope.paddingList[0]);
    }, false);

    $scope.$watch( "selectedpadding", function () {

        if ($scope.selectedpadding.name == "all") {
            $scope.item.values.paddingvalue = $scope.selectedpadding.value;
        }

        if ($scope.selectedpadding.name == "left") {
            $scope.item.values.leftpaddingvalue = $scope.selectedpadding.value;
        }

        if ($scope.selectedpadding.name == "right") {
            $scope.item.values.rightpaddingvalue = $scope.selectedpadding.value;
        }

        if ($scope.selectedpadding.name == "top") {
            $scope.item.values.toppaddingvalue = $scope.selectedpadding.value;
        }

        if ($scope.selectedpadding.name == "bottom") {
            $scope.item.values.bottompaddingvalue = $scope.selectedpadding.value;
        }

    }, true)



})

/*********************************************************************************************************/
/* radius editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.radius", function ($scope, dialogService) {

    $scope.defaultRadiusList = ["all", "topleft", "topright", "bottomleft", "bottomright"];
    $scope.radiusList = [];
   
    $scope.selectedradius = {
        name: "",
        value: 0,
    };

    $scope.setSelectedradius = function (radiustype) {

        if (radiustype == "all") {
            $scope.selectedradius.name="all";
            $scope.selectedradius.value= $scope.item.values.radiusvalue;
        }

        if (radiustype == "topleft") {
            $scope.selectedradius.name = "topleft";
            $scope.selectedradius.value = $scope.item.values.topleftradiusvalue;
        }

        if (radiustype == "topright") {
            $scope.selectedradius.name = "topright";
            $scope.selectedradius.value = $scope.item.values.toprightradiusvalue;
        }

        if (radiustype == "bottomleft") {
            $scope.selectedradius.name = "bottomleft";
            $scope.selectedradius.value = $scope.item.values.bottomleftradiusvalue;
        }

        if (radiustype == "bottomright") {
            $scope.selectedradius.name = "bottomright";
            $scope.selectedradius.value = $scope.item.values.bottomrightradiusvalue;
        }

    }

    if (!$scope.item.values) {
        $scope.item.values = {
            radiusvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 0 ? $scope.item.defaultValue[0] : '',
            topleftradiusvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 1 ? $scope.item.defaultValue[1] : '',
            toprightradiusvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 2 ? $scope.item.defaultValue[2] : '',
            bottomleftradiusvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 3 ? $scope.item.defaultValue[3] : '',
            bottomrightradiusvalue: $scope.item.defaultValue && $scope.item.defaultValue.length > 4 ? $scope.item.defaultValue[4] : '',
        };
    }

    if ($scope.item.enable) {
        angular.forEach($scope.defaultRadiusList, function (key, indexKey) {
            if ($.inArray(key, $scope.item.enable) >= 0) {
                $scope.radiusList.splice($scope.radiusList.length + 1, 0, key);
            }
        })
    }
    else {
        $scope.radiusList = $scope.defaultRadiusList;
    }

    $scope.$watch("valueAreLoaded", function () {
        $scope.setSelectedradius($scope.radiusList[0]);
    }, false);

    $scope.$watch( "selectedradius", function () {

        if ($scope.selectedradius.name == "all") {
            $scope.item.values.radiusvalue = $scope.selectedradius.value;
        }

        if ($scope.selectedradius.name == "topleft") {
            $scope.item.values.topleftradiusvalue = $scope.selectedradius.value;
        }

        if ($scope.selectedradius.name == "topright") {
            $scope.item.values.toprightradiusvalue = $scope.selectedradius.value;
        }

        if ($scope.selectedradius.name == "bottomleft") {
            $scope.item.values.bottomleftradiusvalue = $scope.selectedradius.value;
        }

        if ($scope.selectedradius.name == "bottomright") {
            $scope.item.values.bottomrightradiusvalue = $scope.selectedradius.value;
        }

    }, true)

})

/*********************************************************************************************************/
/* slider editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.slider", function ($scope) {

    if (!$scope.item.values) {
        $scope.item.values = {
            slider: ''
        }
    }

})

/*********************************************************************************************************/
/* slider editor */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner")

.controller("Umbraco.canvasdesigner.wide", function ($scope) {

    if (!$scope.item.values) {
        $scope.item.values = {
            wide: false
        }
    }

})

/*********************************************************************************************************/
/* jQuery UI Slider plugin wrapper */
/*********************************************************************************************************/

angular.module("Umbraco.canvasdesigner").factory('dialogService', function ($rootScope, $q, $http, $timeout, $compile, $templateCache) {


    function closeDialog(dialog) {
        if (dialog.element) {
            dialog.element.removeClass("selected");
            dialog.element.html("");
            dialog.scope.$destroy();
        }
    }

    function open() {
    }

    return {

        open: function (options) {

            var defaults = {
                template: "",
                callback: undefined,
                change: undefined,
                cancel: undefined,
                element: undefined,
                dialogItem: undefined,
                dialogData: undefined
            };

            var dialog = angular.extend(defaults, options);

            var scope = (options && options.scope) || $rootScope.$new();

            // Save original value for cancel action
            var originalDialogItem = angular.copy(dialog.dialogItem);

            dialog.element = $(".float-right-menu");


            /************************************/
            $(document).mousedown(function (e) {
                var container = dialog.element;
                if (!container.is(e.target) && container.has(e.target).length === 0) {
                    closeDialog(dialog);
                }
            });
            /************************************/

            
            $q.when($templateCache.get(dialog.template) || $http.get(dialog.template, { cache: true }).then(function (res) { return res.data; }))
            .then(function onSuccess(template) {

                dialog.element.html(template);

                $timeout(function () {
                    $compile(dialog.element)(scope);
                });

                dialog.element.addClass("selected")

                scope.cancel = function () {
                    if (dialog.cancel) {
                        dialog.cancel(originalDialogItem);
                    }
                    closeDialog(dialog);
                }

                scope.change = function (data) {
                    if (dialog.change) {
                        dialog.change(data);
                    }
                }

                scope.submit = function (data) {
                    if (dialog.callback) {
                        dialog.callback(data);
                    }
                    closeDialog(dialog);
                };

                scope.close = function () {
                    closeDialog(dialog);
                }

                scope.dialogData = dialog.dialogData;
                scope.dialogItem = dialog.dialogItem;

                dialog.scope = scope;

            });

            return dialog;

        },

        close: function() {
            var modal = $(".float-right-menu");
            modal.removeClass("selected")
        }

    }


});

/*********************************************************************************************************/
/* jQuery UI Slider plugin wrapper */
/*********************************************************************************************************/

angular.module('ui.slider', []).value('uiSliderConfig', {}).directive('uiSlider', ['uiSliderConfig', '$timeout', function (uiSliderConfig, $timeout) {
    uiSliderConfig = uiSliderConfig || {};
    return {
        require: 'ngModel',
        template: '<div><div class="slider" /><input class="slider-input" style="display:none" ng-model="value"></div>',
        replace: true,
        compile: function () {
            return function (scope, elm, attrs, ngModel) {

                scope.value = ngModel.$viewValue;

                function parseNumber(n, decimals) {
                    return (decimals) ? parseFloat(n) : parseInt(n);
                };

                var options = angular.extend(scope.$eval(attrs.uiSlider) || {}, uiSliderConfig);
                // Object holding range values
                var prevRangeValues = {
                    min: null,
                    max: null
                };

                // convenience properties
                var properties = ['min', 'max', 'step'];
                var useDecimals = (!angular.isUndefined(attrs.useDecimals)) ? true : false;

                var init = function () {
                    // When ngModel is assigned an array of values then range is expected to be true.
                    // Warn user and change range to true else an error occurs when trying to drag handle
                    if (angular.isArray(ngModel.$viewValue) && options.range !== true) {
                        console.warn('Change your range option of ui-slider. When assigning ngModel an array of values then the range option should be set to true.');
                        options.range = true;
                    }

                    // Ensure the convenience properties are passed as options if they're defined
                    // This avoids init ordering issues where the slider's initial state (eg handle
                    // position) is calculated using widget defaults
                    // Note the properties take precedence over any duplicates in options
                    angular.forEach(properties, function (property) {
                        if (angular.isDefined(attrs[property])) {
                            options[property] = parseNumber(attrs[property], useDecimals);
                        }
                    });

                    elm.find(".slider").slider(options);
                    init = angular.noop;
                };

                // Find out if decimals are to be used for slider
                angular.forEach(properties, function (property) {
                    // support {{}} and watch for updates
                    attrs.$observe(property, function (newVal) {
                        if (!!newVal) {
                            init();
                            elm.find(".slider").slider('option', property, parseNumber(newVal, useDecimals));
                        }
                    });
                });
                attrs.$observe('disabled', function (newVal) {
                    init();
                    elm.find(".slider").slider('option', 'disabled', !!newVal);
                });

                // Watch ui-slider (byVal) for changes and update
                scope.$watch(attrs.uiSlider, function (newVal) {
                    init();
                    if (newVal != undefined) {
                        elm.find(".slider").slider('option', newVal);
                        elm.find(".ui-slider-handle").html("<span>" + ui.value + "px</span>")
                    }
                }, true);

                // Late-bind to prevent compiler clobbering
                $timeout(init, 0, true);

                // Update model value from slider
                elm.find(".slider").bind('slidestop', function (event, ui) {
                    ngModel.$setViewValue(ui.values || ui.value);
                    scope.$apply();
                });

                elm.bind('slide', function (event, ui) {
                    event.stopPropagation();
                    elm.find(".slider-input").val(ui.value);
                    elm.find(".ui-slider-handle").html("<span>" + ui.value + "px</span>")
                });

                // Update slider from model value
                ngModel.$render = function () {
                    init();
                    var method = options.range === true ? 'values' : 'value';

                    if (isNaN(ngModel.$viewValue) && !(ngModel.$viewValue instanceof Array))
                        ngModel.$viewValue = 0;

                    if (ngModel.$viewValue == '')
                        ngModel.$viewValue = 0;

                    scope.value = ngModel.$viewValue;

                    // Do some sanity check of range values
                    if (options.range === true) {

                        // Check outer bounds for min and max values
                        if (angular.isDefined(options.min) && options.min > ngModel.$viewValue[0]) {
                            ngModel.$viewValue[0] = options.min;
                        }
                        if (angular.isDefined(options.max) && options.max < ngModel.$viewValue[1]) {
                            ngModel.$viewValue[1] = options.max;
                        }

                        // Check min and max range values
                        if (ngModel.$viewValue[0] >= ngModel.$viewValue[1]) {
                            // Min value should be less to equal to max value
                            if (prevRangeValues.min >= ngModel.$viewValue[1])
                                ngModel.$viewValue[0] = prevRangeValues.min;
                            // Max value should be less to equal to min value
                            if (prevRangeValues.max <= ngModel.$viewValue[0])
                                ngModel.$viewValue[1] = prevRangeValues.max;
                        }



                        // Store values for later user
                        prevRangeValues.min = ngModel.$viewValue[0];
                        prevRangeValues.max = ngModel.$viewValue[1];

                    }
                    elm.find(".slider").slider(method, ngModel.$viewValue);
                    elm.find(".ui-slider-handle").html("<span>" + ngModel.$viewValue + "px</span>")
                };

                scope.$watch("value", function () {
                    ngModel.$setViewValue(scope.value);
                }, true);

                scope.$watch(attrs.ngModel, function () {
                    if (options.range === true) {
                        ngModel.$render();
                    }
                }, true);

                function destroy() {
                    elm.find(".slider").slider('destroy');
                }
                elm.find(".slider").bind('$destroy', destroy);
            };
        }
    };
}]);



/*********************************************************************************************************/
/* spectrum color picker directive */
/*********************************************************************************************************/

angular.module('spectrumcolorpicker', [])
  .directive('spectrum', function () {
      return {
          restrict: 'E',
          transclude: true,
          scope: {
              colorselected: '='
          },
          link: function (scope, $element) {

              var initColor;

              $element.find("input").spectrum({
                  color: scope.colorselected,
                  allowEmpty: true,
                  preferredFormat: "rgb",
                  showAlpha: true,
                  showInput: true,
                  change: function (color) {

                      if (color) {
                          scope.colorselected = color.toRgbString();
                      }
                      else {
                          scope.colorselected = '';
                      }
                      scope.$apply();
                  },
                  move: function (color) {
                      scope.colorselected = color.toRgbString();
                      scope.$apply();
                  },
                  beforeShow: function (color) {
                      initColor = angular.copy(scope.colorselected);
                      $(this).spectrum("container").find(".sp-cancel").click(function (e) {
                          scope.colorselected = initColor;
                          scope.$apply();
                      });
                  },

              });

              scope.$watch('colorselected', function () {
                  $element.find("input").spectrum("set", scope.colorselected);
              }, true);

          },
          template:
          '      <div class="spectrumcolorpicker"><div class="real-color-preview" style="background-color:{{colorselected}}"></div><input type=\'text\' ng-model=\'colorselected\' /></div>',
          replace: true
      };
  })