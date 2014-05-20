
/*********************************************************************************************************/
/* Global function and variable for panel/page com  */
/*********************************************************************************************************/

/* Refresh tuning panel with selected fieds */
var refrechIntelTuning = function (schema) {

    var scope = angular.element($("#tuningPanel")).scope();

    var notFound = true;
    angular.forEach(scope.tuningModel.categories, function (category, key) {
        var isContainer = false;
        angular.forEach(category.sections, function (section, key) {
            angular.forEach(section.subSections, function (subSection, key) {
                if (subSection.schema && schema.toLowerCase() == subSection.schema.toLowerCase()) {
                    isContainer = true;
                    notFound = false
                }

            });
        });
        if (!category.active) {
            category.active = isContainer;
        }
    });
    if (notFound) {
        scope.tuningModel.categories[0].active = true;
    }
    scope.$apply();

    if (notFound) {
        scope.schemaFocus = "body";
    }
    else {
        scope.schemaFocus = schema.toLowerCase();
    }

    scope.$apply();

}

var setFrameIsLoaded = function () {
    console.info("iframe id loaded");
    var scope = angular.element($("#tuningPanel")).scope();
    scope.frameLoaded = true;
    scope.$apply();
}

/*********************************************************************************************************/
/* tuning panel app and controller */
/*********************************************************************************************************/

// tuning main app
angular.module("umbraco.tuning", ['ui.bootstrap', 'spectrumcolorpicker', 'ui.slider', 'umbraco.resources', 'umbraco.services'])

// panel main controller
.controller("Umbraco.tuningController", function ($scope, $modal, $http, $window, $timeout, $location) {

    $scope.isOpen = false;
    $scope.frameLoaded = false;
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
        { name: "laptop - 1024px", css: "laptop border", icon: "icon-laptop" },
        { name: "iPad portrait - 768px", css: "iPad-portrait border", icon: "icon-ipad" },
        { name: "iPad landscape - 1024px", css: "iPad-landscape border", icon: "icon-ipad flip" },
        { name: "smartphone portrait - 480px", css: "smartphone-portrait border", icon: "icon-iphone" },
        { name: "smartphone landscape  - 320px", css: "smartphone-landscape border", icon: "icon-iphone flip" }
    ];
    $scope.previewDevice = $scope.devices[0];

    // Load parameters from GetLessParameters and init data of the tuning config
    var initTuning = function () {
        $http.get('/Umbraco/Api/tuning/GetLessParameters')
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

        $('.btn-default-save').attr("disabled", true);
        $http.defaults.headers.post["Content-Type"] = "application/x-www-form-urlencoded";
        $http.post('/Umbraco/Api/tuning/PostLessParameters', resultParameters, {
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            transformRequest: transform
        })
        .success(function (data) {
            $('.btn-default-save').attr("disabled", false);
            $('#speechbubble').fadeIn('slow').delay(5000).fadeOut('slow');
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
        var modalInstance = $modal.open({
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

    $scope.$watch("frameLoaded", function () {
        if ($scope.frameLoaded) {
            console.info("init tuning");
            $scope.$watch('tuningModel', function () {
                refreshtuning();
            }, true);
            $scope.togglePanel();
        }
    }, true)

    initTuning();
    $("#tuningPanel").show();

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
            WebFont.load({
                google: {
                    families: [font.fontFamily + ":" + font.variant]
                },
                loading: function () {
                    console.log('loading');
                },
                active: function () {
                    // Notify angular that data has changed so setStyleVariant is called.
                    $scope.$apply(function () {
                        $scope.selectedFont = font;
                        $scope.selectedFont.fontWeight = googleGetWeight($scope.selectedFont.variant);
                        $scope.selectedFont.fontStyle = googleGetStyle($scope.selectedFont.variant);
                    });
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
/*
 jQuery UI Slider plugin wrapper
*/
angular.module('ui.slider', []).value('uiSliderConfig', {}).directive('uiSlider', ['uiSliderConfig', '$timeout', function (uiSliderConfig, $timeout) {
    uiSliderConfig = uiSliderConfig || {};
    return {
        require: 'ngModel',
        compile: function () {
            return function (scope, elm, attrs, ngModel) {

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

                    elm.slider(options);
                    init = angular.noop;
                };

                // Find out if decimals are to be used for slider
                angular.forEach(properties, function (property) {
                    // support {{}} and watch for updates
                    attrs.$observe(property, function (newVal) {
                        if (!!newVal) {
                            init();
                            elm.slider('option', property, parseNumber(newVal, useDecimals));
                        }
                    });
                });
                attrs.$observe('disabled', function (newVal) {
                    init();
                    elm.slider('option', 'disabled', !!newVal);
                });

                // Watch ui-slider (byVal) for changes and update
                scope.$watch(attrs.uiSlider, function (newVal) {
                    init();
                    if (newVal != undefined) {
                        elm.slider('option', newVal);
                    }
                }, true);

                // Late-bind to prevent compiler clobbering
                $timeout(init, 0, true);

                // Update model value from slider
                elm.bind('slidestop', function (event, ui) {
                    ngModel.$setViewValue(ui.values || ui.value);
                    scope.$apply();
                });

                // Update slider from model value
                ngModel.$render = function () {
                    init();
                    var method = options.range === true ? 'values' : 'value';

                    if (isNaN(ngModel.$viewValue) && !(ngModel.$viewValue instanceof Array))
                        ngModel.$viewValue = 0;

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
                    elm.slider(method, ngModel.$viewValue);
                };

                scope.$watch(attrs.ngModel, function () {
                    if (options.range === true) {
                        ngModel.$render();
                    }
                }, true);

                function destroy() {
                    elm.slider('destroy');
                }
                elm.bind('$destroy', destroy);
            };
        }
    };
}]);

angular.module('spectrumcolorpicker', [])
  .directive('spectrum', function () {
      return {
          restrict: 'E',
          transclude: true,
          scope: {
              colorselected: '='
          },
          link: function (scope, $element) {

              $element.find("input").spectrum({
                  color: scope.colorselected,
                  preferredFormat: "rgb",
                  showAlpha: true,
                  showInput: true,
                  change: function (color) {
                      scope.colorselected = color.toRgbString();
                      scope.$apply();
                  },
                  move: function (color) {
                  }
              });

              scope.$watch('colorselected', function () {
                  $element.find("input").spectrum("set", scope.colorselected);
              }, true);

          },
          template:
          '<div><input type=\'text\' ng-model=\'colorselected\' /></div>',
          replace: true
      };
  })