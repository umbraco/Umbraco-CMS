
/*********************************************************************************************************/
/* google font editor */
/*********************************************************************************************************/

angular.module("umbraco.tuning")

.controller("Umbraco.tuning.googlefontpicker", function ($scope, $modal) {

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
            field.fontFamily = selectedItem.fontFamily;
            field.fontType = selectedItem.fontType;
            field.fontWeight = selectedItem.fontWeight;
            field.fontStyle = selectedItem.fontStyle;
        });
    };

})

.controller('tuning.fontfamilypickercontroller', function ($scope, $modalInstance, item, googleFontFamilies, $http) {

    $scope.safeFonts = ["Arial, Helvetica", "Impact", "Lucida Sans Unicode", "Tahoma", "Trebuchet MS", "Verdana", "Georgia", "Times New Roman", "Courier New, Courier"];
    $scope.fonts = [];
    $scope.selectedFont = {};

    var originalFont = {};
    originalFont.fontFamily = $scope.data.modalField.fontFamily;
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
            document.getElementById("resultFrame").contentWindow.getFont(font.fontFamily + ":" + font.variant);

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
                        $scope.data.modalField.fontFamily = $scope.selectedFont.fontFamily;
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
            $scope.data.modalField.fontFamily = $scope.selectedFont.fontFamily;
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
            if (value.fontFamily == item.fontFamily) {
                $scope.selectedFont = value;
                $scope.selectedFont.variant = item.fontWeight + item.fontStyle;
                $scope.selectedFont.fontWeight = item.fontWeight;
                $scope.selectedFont.fontStyle = item.fontStyle;
            }
        });
    }

});