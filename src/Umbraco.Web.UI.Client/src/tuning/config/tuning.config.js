
/*********************************************************************************************************/
/* Tuning setting panel config */
/*********************************************************************************************************/

var tuningConfig = {
    configs: [{
        name: "Body",
        schema: "body",
        selector: "body",
        editors: [
            {
                type: "background",
                category: "Color",
                name: "Background",
                css: "color"
            },
            {
                type: "color",
                category: "Font",
                name: "Font Color",
                css: "color"
            },
            {
                type: "googlefontpicker",
                category: "Font",
                name: "Font Family",
                css: "color"
            },
            {
                type: "border",
                category: "Styling",
                name: "Border",
                schema: ".wrapper"
            },
            {
                type: "radius",
                category: "Styling",
                name: "Radius",
                schema: ".wrapper"
            },
            {
                type: "padding",
                category: "Position",
                name: "Padding"
            },
            {
                type: "margin",
                category: "Position",
                name: "Margin" 
            }
        ]
    }]
};

