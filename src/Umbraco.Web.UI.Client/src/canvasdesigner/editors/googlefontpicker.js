
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