
/*********************************************************************************************************/
/* Canvasdesigner setting panel config */
/*********************************************************************************************************/

var canvasdesignerConfig = {
    configs: [{
        name: "Body",
        schema: "body",
        selector: "body",
        editors: [
            {
                type: "background",
                category: "Color",
                name: "Background",
            },
            {
                type: "color",
                category: "Font",
                name: "Font Color (main)",
                css: "color",
                schema: "body, h1, h2, h3, h4, h5, h6, h7, #nav li a"
            },
            {
                type: "color",
                category: "Font",
                name: "Font Color (secondary)",
                css: "color",
                schema: "ul.meta, .byline"
            },
            {
                type: "googlefontpicker",
                category: "Font",
                name: "Font Family",
                css: "color",
                schema: "body, h1, h2, h3, h4, h5, h6, h7, .byline, #nav, .button"
            }
        ]
    },
    {
        name: "Nav",
        schema: "#nav",
        selector: "nav",
        editors: [
            {
                type: "background",
                category: "Color",
                name: "Background",
            },
            {
                type: "border",
                category: "Color",
                name: "Border",
            },
            {
                type: "color",
                category: "Nav",
                name: "Font Color",
                css: "color",
                schema: "#nav li a"
            },
            {
                type: "color",
                category: "Nav",
                name: "Font Color (hover / selected)",
                css: "color",
                schema: "#nav li:hover a"
            },
            {
                type: "color",
                category: "Nav",
                name: "Background Color (hover)",
                css: "background-color",
                schema: "#nav li:hover a"
            },
            {
                type: "color",
                category: "Nav",
                name: "Background Color (selected)",
                css: "background-color",
                schema: "#nav li.current_page_item a"
            },
            {
                type: "googlefontpicker",
                category: "Font",
                name: "Font familly",
            }
        ]
    },
    {
        name: "Logo",
        schema: "#header .logo div",
        selector: "#header .logo div",
        editors: [
            {
                type: "color",
                category: "Color",
                name: "Border color",
                css: "border-top-color",
                schema: "#header .logo"
            },
            {
                type: "padding",
                category: "Position",
                name: "Margin",
                enable: ["top", "bottom"],
                schema: "#header"
            },
        ]
    },
    {
        name: "h2",
        schema: "h2",
        selector: "h2 span",
        editors: [
            {
                type: "color",
                category: "Color",
                name: "Border color",
                css: "border-top-color",
                schema: "h2.major"
            },
            {
                type: "color",
                category: "Font",
                name: "Font color",
                css: "color",
            }
        ]
    },
    {
        name: "h3",
        schema: "h3",
        selector: "h3",
        editors: [
            {
                type: "color",
                category: "Font",
                name: "Font color",
                css: "color",
            }
        ]
    },
    {
        name: "Banner Title",
        schema: "#banner h2",
        selector: "#banner h2",
        editors: [
            {
                type: "color",
                category: "Font",
                name: "Font color",
                css: "color",
            },
            {
                type: "slider",
                category: "Font",
                name: "Font size",
                css: "font-size",
                min: 18,
                max: 100
            },
            {
                type: "margin",
                category: "Position",
                name: "Margin",
            }

        ]
    },
    {
        name: "Banner Sub-title",
        schema: "#banner .byline",
        selector: "#banner .byline",
        editors: [
            {
                type: "color",
                category: "Font",
                name: "Font color",
                css: "color",
            },
            {
                type: "slider",
                category: "Font",
                name: "Font size",
                css: "font-size",
                min: 18,
                max: 100
            },
            {
                type: "margin",
                category: "Position",
                name: "Margin",
            }

        ]
    },
    {
        name: "Banner",
        schema: "#banner",
        selector: "#banner",
        editors: [
            {
                type: "background",
                category: "Color",
                name: "Background",
                css: "color"
            }
        ]
    },
    {
        name: "Banner-wrapper",
        schema: "#banner-wrapper",
        selector: "#banner-wrapper",
        editors: [
            {
                type: "background",
                category: "Color",
                name: "Background",
            },
            {
                type: "padding",
                category: "Position",
                name: "Padding",
                enable: ["top", "bottom"]
            }
        ]
    },
    {
        name: "#main-wrapper",
        schema: "#main-wrapper",
        selector: "#main-wrapper",
        editors: [
            {
                type: "border",
                category: "Styling",
                name: "Border",
                enable: ["top", "bottom"]
            }
        ]
    },
    {
        name: "Image",
        schema: ".image,.image img,.image:before",
        selector: ".image",
        editors: [
            {
                type: "radius",
                category: "Styling",
                name: "Radius"
            }
        ]
    },
    {
        name: "Button",
        schema: ".button",
        selector: ".button",
        editors: [
            {
                type: "color",
                category: "Color",
                name: "Color",
                css: "color"
            },
            {
                type: "color",
                category: "Color",
                name: "Background",
                css: "background"
            },
            {
                type: "color",
                category: "Color",
                name: "Background Hover",
                css: "background",
                schema: ".button:hover"
            },
            {
                type: "radius",
                category: "Styling",
                name: "Radius"
            }
        ]
    },
    {
        name: "Button Alt",
        schema: ".button-alt",
        selector: ".button-alt",
        editors: [
            {
                type: "color",
                category: "Color",
                name: "Color",
                css: "color"
            },
            {
                type: "color",
                category: "Color",
                name: "Background",
                css: "background"
            },
            {
                type: "color",
                category: "Color",
                name: "Background Hover",
                css: "background",
                schema: ".button-alt:hover"
            },
        ]
    }
]
};

