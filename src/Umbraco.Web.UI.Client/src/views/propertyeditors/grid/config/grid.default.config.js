var uSkyGridConfig = [
{

    style:[
        {
            label: "Set a background image",
            description: "Set a row background",
            key: "background-image",
            view: "imagepicker",
            modifier: "url({0})"
        },

        {
            label: "Set a font color",
            description: "Pick a color",
            key: "color",
            view: "colorpicker"
        }
    ],

    config:[
        {
            label: "Preview",
            description: "Display a live preview",
            key: "preview",
            view: "boolean"
        },

        {
            label: "Class",
            description: "Set a css class",
            key: "class",
            view: "textstring"
        }
    ],

    layouts: [
    {
        grid: 12,
        percentage: 100,


        rows: [
        {
            name: "Single column",
                columns: [{
                    grid: 12,
                    percentage: 100
                }]
        },

        {
            name: "Article",
                models: [{
                    grid: 4,
                    percentage: 33.3,
                    allowed: ["media","quote"]
                }, {
                    grid: 8,
                    percentage: 66.6,
                    allowed: ["rte"]
                }]
        },

        {
         name: "Article, reverse",
         models: [
         {
          grid: 8,
          percentage: 66.6,
          allowed: ["rte","macro"]
      },
      {
         grid: 4,
         percentage: 33.3,
         allowed: ["media","quote","embed"]
     }]
 },
 {
     name: "Profile page",
     models: [
     {
         grid: 4,
         percentage: 33.3,
         allowed: ["media"]
     },
     {
      grid: 8,
      percentage: 66.6,
      allowed: ["rte"]
  }
  ]
},
{
 name: "Headline",
 models: [
 {
     grid: 12,
     percentage: 100,
     max: 1,
     allowed: ["headline"]
 }
 ]
},
{
    name: "Three columns",
    models: [{
        grid: 4,
        percentage: 33.3,
        allowed: ["rte"]
    },
    {
         grid: 4,
         percentage: 33.3,
         allowed: ["rte"]
    },
    {
        grid: 4,
        percentage: 33.3,
        allowed: ["rte"]
    }]
}
]

}
]
},
{
    columns: [
    {
        grid: 9,
        percentage: 70,

        cellModels: [
        {
            models: [{
                grid: 12,
                percentage: 100
            }]
        }, {
            models: [{
                grid: 6,
                percentage: 50
            }, {
                grid: 6,
                percentage: 50
            }]
        }, {
            models: [{
                grid: 4,
                percentage: 33.3
            }, {
                grid: 4,
                percentage: 33.3
            }, {
                grid: 4,
                percentage: 33.3
            }]
        }, {
            models: [{
                grid: 3,
                percentage: 25
            }, {
                grid: 3,
                percentage: 25
            }, {
                grid: 3,
                percentage: 25
            }, {
                grid: 3,
                percentage: 25
            }, ]
        }, {
            models: [{
                grid: 2,
                percentage: 16.6
            }, {
                grid: 2,
                percentage: 16.6
            }, {
                grid: 2,
                percentage: 16.6
            }, {
                grid: 2,
                percentage: 16.6
            }, {
                grid: 2,
                percentage: 16.6
            }, {
                grid: 2,
                percentage: 16.6
            }]
        }, {
            models: [{
                grid: 8,
                percentage: 60
            }, {
                grid: 4,
                percentage: 40
            }]
        }, {
            models: [{
                grid: 4,
                percentage: 40
            }, {
                grid: 8,
                percentage: 60
            }]
        }
        ]
    },
    {
        grid: 3,
        percentage: 30,
        cellModels: [
        {
            models: [{
                grid: 12,
                percentage: 100
            }]
        }
        ]
    }
    ]
},
{
    columns: [
    {
        grid: 3,
        percentage: 30,
        cellModels: [
        {
            models: [{
                grid: 12,
                percentage: 100
            }]
        }
        ]
    },
    {
        grid: 9,
        percentage: 70,
        cellModels: [
        {
            models: [{
                grid: 12,
                percentage: 100
            }]
        }, {
            models: [{
                grid: 6,
                percentage: 50
            }, {
                grid: 6,
                percentage: 50
            }]
        }, {
            models: [{
                grid: 4,
                percentage: 33.3
            }, {
                grid: 4,
                percentage: 33.3
            }, {
                grid: 4,
                percentage: 33.3
            }]
        }, {
            models: [{
                grid: 3,
                percentage: 25
            }, {
                grid: 3,
                percentage: 25
            }, {
                grid: 3,
                percentage: 25
            }, {
                grid: 3,
                percentage: 25
            }, ]
        }, {
            models: [{
                grid: 2,
                percentage: 16.6
            }, {
                grid: 2,
                percentage: 16.6
            }, {
                grid: 2,
                percentage: 16.6
            }, {
                grid: 2,
                percentage: 16.6
            }, {
                grid: 2,
                percentage: 16.6
            }, {
                grid: 2,
                percentage: 16.6
            }]
        }, {
            models: [{
                grid: 8,
                percentage: 60
            }, {
                grid: 4,
                percentage: 40
            }]
        }, {
            models: [{
                grid: 4,
                percentage: 40
            }, {
                grid: 8,
                percentage: 60
            }]
        }
        ]
    }
    ]
},
{
    columns: [
    {
        grid: 4,
        percentage: 33.3,
        cellModels: [
        {
            models: [{
                grid: 12,
                percentage: 100
            }]
        }
        ]
    },
    {
        grid: 4,
        percentage: 33.3,
        cellModels: [
        {
            models: [{
                grid: 12,
                percentage: 100
            }]
        }
        ]
    },
    {
        grid: 4,
        percentage: 33.3,
        cellModels: [
        {
            models: [{
                grid: 12,
                percentage: 100
            }]
        }
        ]
    }
    ]
}
];
