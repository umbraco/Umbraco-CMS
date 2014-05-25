/*********************************************************************************************************/
/* tuning panel app and controller */
/*********************************************************************************************************/

// tuning main app
angular.module("umbraco.tuning", ['ui.bootstrap', 'spectrumcolorpicker', 'ui.slider', 'umbraco.resources', 'umbraco.services'])

// panel main controller
.controller("Umbraco.tuningController", function ($scope, $modal, $http, $window, $timeout, $location) {

    $scope.isOpen = false;
    $scope.frameLoaded = 0;
    $scope.frameFirstLoaded = false;
    $scope.tuningParameterUrl = "";
    $scope.tuningGridStyleUrl = "";
    $scope.tuningGridList = "";
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

    // Load parameters from GetLessParameters and init data of the tuning config
    var initTuning = function () {

        $http.get('/Umbraco/Api/tuning/Load', { params: { tuningStyleUrl: $scope.tuningParameterUrl, tuningGridStyleUrl: $scope.tuningGridStyleUrl } })
            .success(function (data) {

                $.each(tuningConfig.categories, function (indexCategory, category) {
                    $.each(category.sections, function (indexSection, section) {
                        $.each(section.subSections, function (indexSubSection, subSection) {
                            $.each(subSection.fields, function (indexField, field) {

                                try {

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
                                }
                                catch (err) {
                                    console.info("Style parameter not found " + field.alias);
                                }

                            })
                        })
                    })
                });

                $scope.tuningModel = tuningConfig;
                $scope.tuningPalette = tuningPalette;

                if ($scope.settingIsOpen == "setting") {
                    openIntelTuning();
                }

            });

    }

    // Add Less parameters for each grid row
    var initGridConfig = function () {

        var rowModel = {
            name: "Grid",
            sections: [{
                name: "Main",
                subSections: []
            }]
        };

        $.each($scope.tuningGridList, function (index, row) {

            var newIndex = rowModel.sections[0].subSections.length + 1;

            var rowFieldModel = {
                name: "Row",
                schema: "",
                fields: [
                    {
                        name: "Background color",
                        alias: "backgroundRowColor",
                        description: "Background body color",
                        type: "colorPicker",
                        value: "",
                        colorPaletteProperty: "colorBodyBackground"
                    },
                    {
                        name: "Background gradient",
                        alias: "backgroundRowGradientColor",
                        description: "Fade the background to this colour at the bottom",
                        type: "colorPicker",
                        value: ""
                    },
                    {
                        name: "Image/Pattern",
                        alias: "backgroundRowImageOrPattern",
                        description: "Use an image for the background instead of a solid colour/gradient",
                        type: "bgImagePicker",
                        value: ""
                    },
                    {
                        name: "Image position",
                        alias: "backgroundRowPosition",
                        description: "Background body position",
                        type: "bgPositionPicker",
                        value: ""
                    },
                    {
                        name: "Stretch background",
                        alias: "backgroundRowCover",
                        description: "Checked: stretches the chosen image to fill the.\nUnchecked: the image is tiled according to the Repeat setting below",
                        type: "checkbox",
                        value: ""
                    },
                    {
                        name: "Background tiling",
                        alias: "backgroundRowRepeat",
                        description: "How to tile the background image",
                        type: "bgRepeatPicker",
                        value: ""
                    },
                    {
                        name: "Background scrolling behaviour",
                        alias: "backgroundRowAttachment",
                        description: "When fixed the background doesn't scroll with the content",
                        type: "bgAttachmentPicker",
                        value: ""
                    }
                ]
            };

            rowModel.sections[0].subSections.splice(newIndex, 0, rowFieldModel);
            rowModel.sections[0].subSections[newIndex - 1].schema = "." + row;
            $.each(rowModel.sections[0].subSections[newIndex - 1].fields, function (indexField, field) {
                field.alias = field.alias + "__" + row;
            });

        })

        tuningConfig.categories.splice(tuningConfig.categories.length + 1, 0, rowModel);

    }

    // Refresh all less parameters for every changes watching tuningModel 
    var refreshtuning = function () {
        var parameters = [];
        if ($scope.tuningModel) {
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
    }

    var openIntelTuning = function () {
        document.getElementById("resultFrame").contentWindow.initIntelTuning($scope.tuningModel);
    }

    var closeIntelTuning = function () {
        document.getElementById("resultFrame").contentWindow.closeIntelTuning($scope.tuningModel);
    }

    var setSelectedSchema = function (schema) {
        document.getElementById("resultFrame").contentWindow.setSelectedSchema(schema);
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
        var parametersGrid = [];
        $.each($scope.tuningModel.categories, function (indexCategory, category) {
            $.each(category.sections, function (indexSection, section) {
                $.each(section.subSections, function (indexSubSection, subSection) {
                    $.each(subSection.fields, function (indexField, field) {

                        if (subSection.schema && subSection.schema.indexOf("grid-row-") >= 0) {
                            var value = (field.value != 0 && (field.value == undefined || field.value == "")) ? "''" : field.value;
                            parametersGrid.splice(parametersGrid.length + 1, 0, "@" + field.alias + ":" + value + ";");
                        }
                        else {
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
                        }

                    })
                })
            })
        });

        var resultParameters = { parameters: parameters.join(""), parametersGrid: parametersGrid.join(""), pageId: $location.search().id };
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
            $scope.frameLoaded++;
            $scope.pageId = $scope.pageId + "&n=123456";
            $('.btn-default-delete').attr("disabled", false);
        })

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
        $scope.data = {
            modalField: field
        };
        var modalInstance = $modal.open({
            scope: $scope,
            templateUrl: 'fontFamilyPickerModel.html',
            controller: 'tuning.fontfamilypickercontroller',
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

    // Focus schema in front
    $scope.accordionWillBeOpened = function (schema) {
        setSelectedSchema(schema);
    }

    // Preload of the google font
    $http.get('/Umbraco/Api/tuning/GetGoogleFont').success(function (data) {
        $scope.googleFontFamilies = data;
    })

    // watch framLoaded
    $scope.$watch("frameLoaded", function () {
        if ($scope.frameLoaded > 0) {
            initGridConfig();
            initTuning();
            $scope.$watch('tuningModel', function () {
                refreshtuning();
            }, true);
        }
    }, true)

    // first panel init
    initTuning();

    // toggle panel
    $scope.togglePanel();

})

// Image picker controller
.controller('tuning.mediapickercontroller', function ($scope, $modalInstance, items, $http, mediaResource, umbRequestHelper, entityResource, mediaHelper) {

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

    var modalFieldvalue = $scope.data.modalField.value;

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
                        child.thumbnail = mediaHelper.resolveFile(child, true);
                        child.image = mediaHelper.resolveFile(child, false);
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
            $scope.data.modalField.value = "url(" + $scope.selectedMedia.image + ")";
        }
        else {
            $scope.gotoFolder(media);
        }
    };

    //default root item
    if (!$scope.selectedMedia) {
        $scope.gotoFolder();
    }

    $scope.ok = function () {
        $modalInstance.close($scope.data.modalField.value);
    };

    $scope.cancel = function () {
        $scope.data.modalField.value = modalFieldvalue;
        $modalInstance.dismiss('cancel');
    };

})

// Font picker controller
.controller('tuning.fontfamilypickercontroller', function ($scope, $modalInstance, item, googleFontFamilies, $http) {

    $scope.safeFonts = ["Arial, Helvetica", "Impact", "Lucida Sans Unicode", "Tahoma", "Trebuchet MS", "Verdana", "Georgia", "Times New Roman", "Courier New, Courier"];
    $scope.fonts = [];
    $scope.selectedFont = {};

    var originalFont = {};
    originalFont.fontFamily = $scope.data.modalField.value;
    originalFont.fontType = $scope.data.modalField.fontType;
    originalFont.fontWeight = $scope.data.modalField.fontWeight;
    originalFont.fontStyle = $scope.data.modalField.fontStyle;

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
        if (font != undefined && font.fontFamily != "" && font.fontType == "google") {
            // Font needs to be independently loaded in the iframe for live preview to work.
            document.getElementById("resultFrame").contentWindow.getFont(font);

            WebFont.load({
                google: {
                    families: [font.fontFamily + ":" + font.variant]
                },
                loading: function () {
                    console.log('loading');
                },
                active: function () {
                    // If $apply isn't called, the new font family isn't applied until the next user click.
                    $scope.$apply(function () {
                        $scope.selectedFont = font;
                        $scope.selectedFont.fontWeight = googleGetWeight($scope.selectedFont.variant);
                        $scope.selectedFont.fontStyle = googleGetStyle($scope.selectedFont.variant);
                        // Apply to the page content as a preview.
                        $scope.data.modalField.value = $scope.selectedFont.fontFamily;
                        $scope.data.modalField.fontType = $scope.selectedFont.fontType;
                        $scope.data.modalField.fontWeight = $scope.selectedFont.fontWeight;
                        $scope.data.modalField.fontStyle = $scope.selectedFont.fontStyle;
                    });
                }
            });
        }
        else {
            // Font is available, apply it immediately in modal preview.
            $scope.selectedFont = font;
            // And to page content.
            $scope.data.modalField.value = $scope.selectedFont.fontFamily;
            $scope.data.modalField.fontType = $scope.selectedFont.fontType;
            $scope.data.modalField.fontWeight = $scope.selectedFont.fontWeight;
            $scope.data.modalField.fontStyle = $scope.selectedFont.fontStyle;
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
        // Discard font change.
        $modalInstance.close({
            fontFamily: originalFont.fontFamily,
            fontType: originalFont.fontType,
            fontWeight: originalFont.fontWeight,
            fontStyle: originalFont.fontStyle,
        });
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