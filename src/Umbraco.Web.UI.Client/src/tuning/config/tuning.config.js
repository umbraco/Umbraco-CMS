
/*********************************************************************************************************/
/* Tuning setting panel config */
/*********************************************************************************************************/

var tuningConfig = {
    categories: [{
        name: "Background",
        sections: [{
            name: "Main",
            subSections: [{
                name: "Body",
                schema: "body",
                fields: [
                    {
                        name: "Background color",
                        alias: "backgroundBodyColor",
                        description: "Background body color",
                        type: "colorPicker",
                        value: "",
                        colorPaletteProperty: "colorBodyBackground"
                    },
                    {
                        name: "Background gradient",
                        alias: "backgroundBodyGradientColor",
                        description: "Fade the background to this colour at the bottom",
                        type: "colorPicker",
                        value: ""
                    },
                    {
                        name: "Image/Pattern",
                        alias: "backgroundBodyImageOrPattern",
                        description: "Use an image for the background instead of a solid colour/gradient",
                        type: "bgImagePicker",
                        value: ""
                    },
                    {
                        name: "Image position",
                        alias: "backgroundBodyPosition",
                        description: "Background body position",
                        type: "bgPositionPicker",
                        value: ""
                    },
                    {
                        name: "Stretch background",
                        alias: "backgroundBodyCover",
                        description: "Checked: stretches the chosen image to fill the.\nUnchecked: the image is tiled according to the Repeat setting below",
                        type: "checkbox",
                        value: ""
                    },
                    {
                        name: "Background tiling",
                        alias: "backgroundBodyRepeat",
                        description: "How to tile the background image",
                        type: "bgRepeatPicker",
                        value: ""
                    },
                    {
                        name: "Background scrolling behaviour",
                        alias: "backgroundBodyAttachment",
                        description: "When fixed the background doesn't scroll with the content",
                        type: "bgAttachmentPicker",
                        value: ""
                    }
                ]
            },
            {
                name: "Main",
                schema: ".content",
                fields: [
                    {
                        name: "Color",
                        alias: "backgroundMainColumnColor",
                        description: "Background main column color",
                        type: "colorPicker",
                        value: "",
                        colorPaletteProperty: "colorMainColumnBackground"
                    },
                    {
                        name: "Gradient Color",
                        alias: "backgroundMainColumnGradientColor",
                        description: "Background main column gradient color",
                        type: "colorPicker",
                        value: ""
                    },
                    {
                        name: "Image/Pattern",
                        alias: "backgroundMainColumnImageOrPattern",
                        description: "Background main column image/pattern",
                        type: "bgImagePicker",
                        value: ""
                    },
                    {
                        name: "Position",
                        alias: "backgroundMainColumnPosition",
                        description: "Background main column position",
                        type: "bgPositionPicker",
                        value: ""
                    },
                    {
                        name: "Cover",
                        alias: "backgroundMainColumnCover",
                        description: "Background MainColumn cover",
                        type: "checkbox",
                        value: ""
                    },
                    {
                        name: "Repeat",
                        alias: "backgroundMainColumnRepeat",
                        description: "Background main column repeat",
                        type: "bgRepeatPicker",
                        value: ""
                    },
                    {
                        name: "Attachment",
                        alias: "backgroundMainColumnAttachment",
                        description: "Background main column attachment",
                        type: "bgAttachmentPicker",
                        value: ""
                    }
                ]
            },
            {
                name: "Header",
                schema: "header",
                fields: [
                    {
                        name: "Color",
                        alias: "backgroundHeaderColor",
                        description: "Background header color",
                        type: "colorPicker",
                        value: "",
                        colorPaletteProperty: "colorHeaderBackground"
                    },
                    {
                        name: "Gradient Color",
                        alias: "backgroundHeaderGradientColor",
                        description: "Background header gradient color",
                        type: "colorPicker",
                        value: ""
                    },
                    {
                        name: "Image/Pattern",
                        alias: "backgroundHeaderImageOrPattern",
                        description: "Background header image/pattern",
                        type: "bgImagePicker",
                        value: ""
                    },
                    {
                        name: "Position",
                        alias: "backgroundHeaderPosition",
                        description: "Background header position",
                        type: "bgPositionPicker",
                        value: ""
                    },
                    {
                        name: "Cover",
                        alias: "backgroundHeaderCover",
                        description: "Background Header cover",
                        type: "checkbox",
                        value: ""
                    },
                    {
                        name: "Repeat",
                        alias: "backgroundHeaderRepeat",
                        description: "Background header repeat",
                        type: "bgRepeatPicker",
                        value: ""
                    },
                    {
                        name: "Attachment",
                        alias: "backgroundHeaderAttachment",
                        description: "Background header attachment",
                        type: "bgAttachmentPicker",
                        value: ""
                    }
                ]
            },
            {
                name: "Footer",
                schema: ".footer",
                fields: [
                    {
                        name: "Color",
                        alias: "backgroundFooterColor",
                        description: "Background footer color",
                        type: "colorPicker",
                        value: "",
                        colorPaletteProperty: "colorHeaderBackground"
                    },
                    {
                        name: "Gradient Color",
                        alias: "backgroundFooterGradientColor",
                        description: "Background footer gradient color",
                        type: "colorPicker",
                        value: ""
                    },
                    {
                        name: "Image/Pattern",
                        alias: "backgroundFooterImageOrPattern",
                        description: "Background footer image/pattern",
                        type: "bgImagePicker",
                        value: ""
                    },
                    {
                        name: "Position",
                        alias: "backgroundFooterPosition",
                        description: "Background footer position",
                        type: "bgPositionPicker",
                        value: ""
                    },
                    {
                        name: "Cover",
                        alias: "backgroundFooterCover",
                        description: "Background Footer cover",
                        type: "checkbox",
                        value: ""
                    },
                    {
                        name: "Repeat",
                        alias: "backgroundFooterRepeat",
                        description: "Background footer repeat",
                        type: "bgRepeatPicker",
                        value: ""
                    },
                    {
                        name: "Attachment",
                        alias: "backgroundFooterAttachment",
                        description: "Background footer attachment",
                        type: "bgAttachmentPicker",
                        value: ""
                    }
                ]
            }]
        }]
    },
    {
        name: "Styling",
        sections: [{
            name: "General",
            subSections: [{
                name: "Main",
                schema: ".content",
                fields: [{
                    name: "Layout",
                    alias: "stylingMainColumnBoxed",
                    description: "Main column layout",
                    type: "layoutPicker",
                    value: "boxed"
                },
                {
                    name: "Header Top Margin",
                    alias: "stylingHeaderTopMargin",
                    description: "Header top margin",
                    type: "slider",
                    min: "0",
                    max: "100",
                    value: "0"
                },
                {
                    name: "Main Top Margin",
                    alias: "stylingMainColumnTopMargin",
                    description: "Main column top margin",
                    type: "slider",
                    min: "0",
                    max: "100",
                    value: "0"
                },
                {
                    name: "Footer Top Margin",
                    alias: "stylingFooterTopMargin",
                    description: "Footer top margin",
                    type: "slider",
                    min: "0",
                    max: "100",
                    value: "0"
                },
                {
                    name: "Footer Bottom Margin",
                    alias: "stylingFooterBottompMargin",
                    description: "Footer Bottom margin",
                    type: "slider",
                    min: "0",
                    max: "100",
                    value: "0"
                },
                {
                    name: "Radius",
                    alias: "stylingMainColumnRadius",
                    description: "Main column radius",
                    type: "slider",
                    min: "0",
                    max: "20",
                    value: "0"
                },
                {
                    name: "Shadow",
                    alias: "stylingMainColumnShadow",
                    description: "Main column shadow",
                    type: "slider",
                    min: "0",
                    max: "100",
                    value: "0"
                }]
            },
            {
                name: "Header",
                schema: "header",
                fields: [{
                    name: "Top Border Size",
                    alias: "stylingHeaderTopBorderSize",
                    description: "Header top border size",
                    type: "slider",
                    min: "0",
                    max: "50",
                    value: "0"
                },
                {
                    name: "Bottom Border Size",
                    alias: "stylingHeaderBottomBorderSize",
                    description: "Header bottom border size",
                    type: "slider",
                    min: "0",
                    max: "50",
                    value: "0"
                },
                {
                    name: "Top Border color",
                    alias: "stylingHeaderTopBorderColor",
                    description: "Header top border color",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorBase"
                },
                {
                    name: "Bottom Border color",
                    alias: "stylingHeaderBottomBorderColor",
                    description: "Header bottom border color",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorBase"
                },
                {
                    name: "Min Height",
                    alias: "stylingHeaderMinHeight",
                    description: "Header min height",
                    type: "slider",
                    min: "0",
                    max: "500",
                    value: "0"
                },
                {
                    name: "Logo Top Margin",
                    alias: "stylingHeaderLogoTopMargin",
                    description: "Header logo top margin",
                    type: "slider",
                    min: "-100",
                    max: "100",
                    value: "0"
                }]
            },
            {
                name: "Navigation",
                schema: ".navbar-collapse",
                fields: [{
                    name: "Display",
                    alias: "stylingNavDisplay",
                    description: "Navigation display",
                    type: "displayPicker",
                    value: ""
                },
                {
                    name: "Background Color",
                    alias: "stylingNavBackgroundColor",
                    description: "Navigation background color",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorNavBackground"
                },
                {
                    name: "Background Color L2",
                    alias: "stylingNavBackgroundDdl",
                    description: "Navigation background color for Level 2",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorNavBackground"
                },
                {
                    name: "Active Background Color",
                    alias: "stylingNavBackgroundActiveColor",
                    description: "Navigation active background color",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorBase"
                },
                {
                    name: "Margin Top",
                    alias: "stylingNavMarginTop",
                    description: "Navigation Margin Top",
                    type: "slider",
                    min: "-200",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Radius",
                    alias: "stylingNavRadius",
                    description: "Navigation radius",
                    type: "slider",
                    min: "0",
                    max: "20",
                    value: "0"
                },
                {
                    name: "Radius Only Top",
                    alias: "stylingNavRadiusOnlyTop",
                    description: "Navigation radius only top",
                    type: "checkbox",
                    value: ""
                },
                {
                    name: "Active Top Border Size",
                    alias: "stylingNavItemTopBorderActiveSize",
                    description: "Navigation active top border size",
                    type: "slider",
                    min: "0",
                    max: "50",
                    value: "0"
                },
                {
                    name: "Active Bottom Border Size",
                    alias: "stylingNavItemBottomBorderActiveSize",
                    description: "Navigation active bottom border size",
                    type: "slider",
                    min: "0",
                    max: "50",
                    value: "0"
                },
                {
                    name: "Active Top Border Color",
                    alias: "stylingNavItemTopBorderActiveColor",
                    description: "Navigation active top border color",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorBase"
                },
                {
                    name: "Active Bottom Border Color",
                    alias: "stylingNavItemBottomBorderActiveColor",
                    description: "Navigation active bottom border color",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorBase"
                }]
            },
            {
                name: "Social Links",
                schema: ".social-row",
                fields: [{
                    name: "Display",
                    alias: "stySocialDisplay",
                    description: "Social links list display",
                    type: "displayPicker",
                    value: ""
                },
                {
                    name: "Background Color",
                    alias: "stySocialBackgroundColor",
                    description: "Social links list background color",
                    type: "colorPicker",
                    value: ""
                },
                {
                    name: "Top Margin",
                    alias: "stySocialTopMargin",
                    description: "Social links list top margin",
                    type: "slider",
                    min: "0",
                    max: "500",
                    value: "0"
                },
                {
                    name: "Border Top Size",
                    alias: "stySocialBorderTopSize",
                    description: "Social links list border top size",
                    type: "slider",
                    min: "0",
                    max: "50",
                    value: "0"
                },
                {
                    name: "Border Bottom Size",
                    alias: "stySocialBorderBottomSize",
                    description: "Social links list border bottom size",
                    type: "slider",
                    min: "0",
                    max: "50",
                    value: "0"
                },
                {
                    name: "Border Top Color",
                    alias: "stySocialBorderTopColor",
                    description: "Social links list border top color",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorBase"
                },
                {
                    name: "Border Bottom Color",
                    alias: "stySocialBorderBottomColor",
                    description: "Social links list border bottom color",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorBase"
                }]
            },
            {
                name: "Boxes",
                fields: [{
                    name: "Background Color",
                    alias: "stylingBoxesBackgroundColor",
                    description: "Boxes background color",
                    type: "colorPicker",
                    value: ""
                },
                {
                    name: "Min Height",
                    alias: "stylingBoxesMinHeight",
                    description: "Boxes min height",
                    type: "slider",
                    min: "0",
                    max: "500",
                    value: "0"
                },
                {
                    name: "Radius",
                    alias: "stylingBoxesRadius",
                    description: "Boxes radius",
                    type: "slider",
                    min: "0",
                    max: "20",
                    value: "0"
                },
                {
                    name: "Border Size",
                    alias: "stylingBoxesBorderSize",
                    description: "Boxes border size",
                    type: "slider",
                    min: "0",
                    max: "50",
                    value: "0"
                },
                {
                    name: "Border Color",
                    alias: "stylingBoxesBorderColor",
                    description: "Boxes border color",
                    type: "colorPicker",
                    value: ""
                }]
            },
            {
                name: "Thumbnails",
                schema: ".thumbnail",
                fields: [{
                    name: "Background Color",
                    alias: "stylingThumbnailsBackgroundColor",
                    description: "Thumbnails background color",
                    type: "colorPicker",
                    value: ""
                },
                {
                    name: "Min Height",
                    alias: "stylingThumbnailsMinHeight",
                    description: "Thumbnails min height",
                    type: "slider",
                    min: "0",
                    max: "500",
                    value: "0"
                },
                {
                    name: "Radius",
                    alias: "stylingThumbnailsRadius",
                    description: "Thumbnails radius",
                    type: "slider",
                    min: "0",
                    max: "20",
                    value: "0"
                },
                {
                    name: "Border Size",
                    alias: "stylingThumbnailsBorderSize",
                    description: "Thumbnails border size",
                    type: "slider",
                    min: "0",
                    max: "50",
                    value: "0"
                },
                {
                    name: "Border Color",
                    alias: "stylingThumbnailsBorderColor",
                    description: "Thumbnails border color",
                    type: "colorPicker",
                    value: ""
                }]
            }]
        }]
    },
    {
        name: "Fonts",
        sections: [{
            name: "Main",
            subSections: [{
                name: "Body",
                schema: "p",
                fields: [{
                    name: "Color",
                    alias: "FontBodyColor",
                    description: "",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorFontDefault"
                },
                {
                    name: "Size",
                    alias: "FontBodySize",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Line Height",
                    alias: "FontBodyLineHeight",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Family",
                    alias: "FontBodyFamily",
                    description: "",
                    type: "fontFamilyPicker",
                    fontType: "",
                    fontWeight: "",
                    fontStyle: "",
                    value: ""
                }]
            },
            {
                name: "Navigation",
                schema: ".nav",
                fields: [{
                    name: "Font Color",
                    alias: "FontNavFontColor",
                    description: "Navigation font color",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorFontRev"
                },
                {
                    name: "Active Font Color",
                    alias: "FontNavFontActiveColor",
                    description: "Navigation active font color",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorFontRevActive"
                },
                {
                    name: "Size",
                    alias: "FontNavSize",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Line Height",
                    alias: "FontNavLineHeight",
                    description: "Navigation line height",
                    type: "slider",
                    min: "20",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Family",
                    alias: "FontNavFamily",
                    description: "",
                    type: "fontFamilyPicker",
                    fontType: "",
                    fontWeight: "",
                    fontStyle: "",
                    value: ""
                }]
            },
            {
                name: "Social Link",
                schema: "social-row",
                fields: [{
                    name: "Font Color",
                    alias: "FontSocialFontColor",
                    description: "Social links list font color",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorFontRev"
                },
                {
                    name: "Font Color Hover",
                    alias: "FontSocialFontColorHover",
                    description: "Social links list font color hover",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorFontRevActive"
                },
                {
                    name: "Font Size",
                    alias: "FontSocialFontSize",
                    description: "Social links list font size",
                    type: "slider",
                    min: "8",
                    max: "36",
                    value: "0"
                },
                {
                    name: "Line Height",
                    alias: "FontSocialLineHeight",
                    description: "Social links list line height",
                    type: "slider",
                    min: "20",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Family",
                    alias: "FontSocialFamily",
                    description: "",
                    type: "fontFamilyPicker",
                    fontType: "",
                    fontWeight: "",
                    fontStyle: "",
                    value: ""
                }
                ]
            },
            {
                name: "H1",
                schema: "h1",
                fields: [{
                    name: "Color",
                    alias: "FontH1Color",
                    description: "",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorBase"
                },
                {
                    name: "Size",
                    alias: "FontH1Size",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Line Height",
                    alias: "FontH1LineHeight",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Top margin",
                    alias: "FontH1TopMargin",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Bottom margin",
                    alias: "FontH1BottomMargin",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Family",
                    alias: "FontH1Family",
                    description: "",
                    type: "fontFamilyPicker",
                    fontType: "",
                    fontWeight: "",
                    fontStyle: "",
                    value: ""
                }]
            }, {
                name: "H2",
                schema: "h2",
                fields: [{
                    name: "Color",
                    alias: "FontH2Color",
                    description: "",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorBase"
                },
                {
                    name: "Size",
                    alias: "FontH2Size",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Line Height",
                    alias: "FontH2LineHeight",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Top margin",
                    alias: "FontH2TopMargin",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Bottom margin",
                    alias: "FontH2BottomMargin",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Family",
                    alias: "FontH2Family",
                    description: "",
                    type: "fontFamilyPicker",
                    fontType: "",
                    fontWeight: "",
                    fontStyle: "",
                    value: ""
                }]
            }, {
                name: "H3",
                schema: "h3",
                fields: [{
                    name: "Color",
                    alias: "FontH3Color",
                    description: "",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorBase"
                },
                {
                    name: "Size",
                    alias: "FontH3Size",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Line Height",
                    alias: "FontH3LineHeight",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Top margin",
                    alias: "FontH3TopMargin",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Bottom margin",
                    alias: "FontH3BottomMargin",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Family",
                    alias: "FontH3Family",
                    description: "",
                    type: "fontFamilyPicker",
                    fontType: "",
                    fontWeight: "",
                    fontStyle: "",
                    value: ""
                }]
            }, {
                name: "H4",
                schema: "h4",
                fields: [{
                    name: "Color",
                    alias: "FontH4Color",
                    description: "",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorFontDefault"
                },
                {
                    name: "Size",
                    alias: "FontH4Size",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Line Height",
                    alias: "FontH4LineHeight",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Top margin",
                    alias: "FontH4TopMargin",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Bottom margin",
                    alias: "FontH4BottomMargin",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Family",
                    alias: "FontH4Family",
                    description: "",
                    type: "fontFamilyPicker",
                    fontType: "",
                    fontWeight: "",
                    fontStyle: "",
                    value: ""
                }]
            }, {
                name: "H5",
                schema: "h5",
                fields: [{
                    name: "Color",
                    alias: "FontH5Color",
                    description: "",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorFontDefault"
                },
                {
                    name: "Size",
                    alias: "FontH5Size",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Line Height",
                    alias: "FontH5LineHeight",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Top margin",
                    alias: "FontH5TopMargin",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Bottom margin",
                    alias: "FontH5BottomMargin",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Family",
                    alias: "FontH5Family",
                    description: "",
                    type: "fontFamilyPicker",
                    fontType: "",
                    fontWeight: "",
                    fontStyle: "",
                    value: ""
                }]
            }, {
                name: "H6",
                schema: "h6",
                fields: [{
                    name: "Color",
                    alias: "FontH6Color",
                    description: "",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorFontDefault"
                },
                {
                    name: "Size",
                    alias: "FontH6Size",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Line Height",
                    alias: "FontH6LineHeight",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Top margin",
                    alias: "FontH6TopMargin",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Bottom margin",
                    alias: "FontH6BottomMargin",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Family",
                    alias: "FontH6Family",
                    description: "",
                    type: "fontFamilyPicker",
                    fontType: "",
                    fontWeight: "",
                    fontStyle: "",
                    value: ""
                }]
            }, {
                name: "Medium",
                schema: ".medium",
                fields: [{
                    name: "Color",
                    alias: "FontMediumColor",
                    description: "",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorFontDefault"
                }, {
                    name: "Size",
                    alias: "FontMediumSize",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Line Height",
                    alias: "FontMediumLineHeight",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Top margin",
                    alias: "FontMediumTopMargin",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Bottom margin",
                    alias: "FontMediumBottomMargin",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Family",
                    alias: "FontMediumFamily",
                    description: "",
                    type: "fontFamilyPicker",
                    fontType: "",
                    fontWeight: "",
                    fontStyle: "",
                    value: ""
                }]
            }, {
                name: "Highlighted",
                schema: ".highlighted",
                fields: [{
                    name: "Color",
                    alias: "FontHighlightedColor",
                    description: "",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorFontRev"
                },
                {
                    name: "Background Color",
                    alias: "FontHighlightedBackgroundColor",
                    description: "",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorBase"
                },
                {
                    name: "Family",
                    alias: "FontHighlightedFamily",
                    description: "",
                    type: "fontFamilyPicker",
                    fontType: "",
                    fontWeight: "",
                    fontStyle: "",
                    value: ""
                }]
            }, {
                name: "Big",
                schema: ".big",
                fields: [{
                    name: "Color",
                    alias: "FontBigColor",
                    description: "",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorBase"
                },
                {
                    name: "Size",
                    alias: "FontBigSize",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Line Height",
                    alias: "FontBigLineHeight",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Top margin",
                    alias: "FontBigTopMargin",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Bottom margin",
                    alias: "FontBigBottomMargin",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Family",
                    alias: "FontBigFamily",
                    description: "",
                    type: "fontFamilyPicker",
                    fontType: "",
                    fontWeight: "",
                    fontStyle: "",
                    value: ""
                }]
            }, {
                name: "Button",
                schema: ".button",
                fields: [{
                    name: "Color",
                    alias: "FontButtonColor",
                    description: "",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorFontRev"
                },
                {
                    name: "Background Color",
                    alias: "FontButtonBackgroundColor",
                    description: "",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorBase"
                },
                {
                    name: "Color Hover",
                    alias: "FontButtonColorHover",
                    description: "",
                    type: "colorPicker",
                    value: ""
                },
                {
                    name: "Background Color Hover",
                    alias: "FontButtonBackgroundColorHover",
                    description: "",
                    type: "colorPicker",
                    value: ""
                },
                {
                    name: "Size",
                    alias: "FontButtonSize",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Line Height",
                    alias: "FontButtonLineHeight",
                    description: "",
                    type: "slider",
                    min: "8",
                    max: "64",
                    value: "0"
                },
                {
                    name: "Padding",
                    alias: "FontButtonPadding",
                    description: "",
                    type: "slider",
                    min: "0",
                    max: "200",
                    value: "0"
                },
                {
                    name: "Family",
                    alias: "FontButtonFamily",
                    description: "",
                    type: "fontFamilyPicker",
                    fontType: "",
                    fontWeight: "",
                    fontStyle: "",
                    value: ""
                }]
            },
            {
                name: "Color2",
                schema: ".color2",
                fields: [{
                    name: "Color",
                    alias: "FontColor2Color",
                    description: "",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorBase"
                }]
            },
            {
                name: "Color3",
                schema: ".color3",
                fields: [{
                    name: "Color",
                    alias: "FontColor3Color",
                    description: "",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorFontRev"
                }]
            },
            {
                name: "Color4",
                schema: ".color4",
                fields: [{
                    name: "Color",
                    alias: "FontColor4Color",
                    description: "",
                    type: "colorPicker",
                    value: ""
                }]
            },
            {
                name: "Link",
                schema: "p a",
                fields: [{
                    name: "Color",
                    alias: "FontLinkColor",
                    description: "",
                    type: "colorPicker",
                    value: "",
                    colorPaletteProperty: "colorBase"
                }, {
                    name: "Color Hover",
                    alias: "FontLinkColorHover",
                    description: "",
                    type: "colorPicker",
                    value: ""
                }]
            }]
        }]
    }]
}

var rowModel = {
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
        },
        {
            name: "Full size",
            alias: "rowFullSize",
            description: "",
            type: "checkbox",
            value: ""
        }
    ]
};
