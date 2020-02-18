(function () {
    "use strict";

    angular
        .module("umbraco")
        .component("blockListPropertyEditor", {
            templateUrl: "views/propertyeditors/blocklist/blocklist.component.html",
            controller: BlockListController,
            controllerAs: "vm",
            bindings: {
                
            },
            require: {
                umbProperty: "?^umbProperty"
            }
        });

    function BlockListController($scope, $interpolate, editorService, clipboardService, localizationService, overlayService) {
        
        var unsubscribe = [];
        var vm = this;
        var model = $scope.$parent.$parent.model;
        vm.propertyForm = $scope.$parent.$parent.propertyForm;

        $scope.moveFocusToBlock = null;

        console.log("config:", model.config);

        vm.validationLimit = model.config.validationLimit;

        console.log("value:", model.value);

        
        vm.availableBlockTypes = [
            {
                alias: "pageModule",
                name: "Module",
                icon: "icon-document",
                prototype_paste_data: {
                    
                    elementType: {
                        alias: "contentTypeAlias",
                        icon: "icon-document",
                        label: "Text"
                    },
                    labelTemplate: "{{pageTitle | truncate:true:36}}",
                    labelInterpolate: $interpolate("{{pageTitle | truncate:true:36}}"),
                    editor: "views/blockelements/labelblock/labelblock.editor.html",
                    overlaySize: "medium",
                    content: {
                        apps: [
                            {
                                name: "Content",
                                alias: "umbContent",
                                weight: -100,
                                icon: "icon-document",
                                view: "views/content/apps/content/content.html",
                                viewModel: 0,
                                active: true,
                                badge: null,
                                anchors: [],
                                hasError: false
                            },
                            {
                                name: "Info",
                                alias: "umbInfo",
                                weight: 100,
                                icon: "icon-info",
                                view: "views/content/apps/info/info.html",
                                viewModel: null,
                                active: false,
                                badge: null,
                                hasError: false
                            }
                        ],
                        variants: [
                            {
                                language: {
                                    isDefault: true
                                }
                            }
                        ],
                        tabs: [
                            {
                                id: 1234,
                                label: "Content",
                                properties: [
                                    {
                                        label: "Page Title",
                                        description: "The title of the page",
                                        view: "textbox",
                                        config: {maxChars: 500},
                                        hideLabel: false,
                                        validation: {mandatory: true, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                        readonly: false,
                                        id: 441,
                                        dataTypeKey: "0cc0eba1-9960-42c9-bf9b-60e150b429ae",
                                        value: "Consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                                        alias: "pageTitle",
                                        editor: "Umbraco.TextBox",
                                        isSensitive: false,
                                        culture: null,
                                        segment: null
                                    },
                                    {
                                        label: "Image",
                                        description: "",
                                        view: "mediapicker",
                                        config: {multiPicker: false, 
                                            onlyImages: true, 
                                            disableFolderSelect: true, 
                                            startNodeId: "umb://media/1fd2ecaff3714c009306867fa4585e7a", 
                                            ignoreUserStartNodes: false, 
                                            idType: "udi"
                                        },
                                        hideLabel: false,
                                        validation: {mandatory: false, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                        readonly: false,
                                        id: 495,
                                        dataTypeKey: "e26a8d91-a9d7-475b-bc3b-2a09f4743754",
                                        value: "umb://media/fa763e0d0ceb408c8720365d57e06e32",
                                        alias: "photo",
                                        editor: "Umbraco.MediaPicker",
                                        isSensitive: false,
                                        culture: null,
                                        segment: null
                                    }, 
                                    {
                                        label: "Image Description",
                                        description: "The title of the page",
                                        view: "textbox",
                                        config: {maxChars: 500},
                                        hideLabel: false,
                                        validation: {mandatory: true, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                        readonly: false,
                                        id: 442,
                                        dataTypeKey: "0cc0eba1-9960-42c9-bf9b-60e150b429ae",
                                        value: "Let´s have a chat",
                                        alias: "imageDesc",
                                        editor: "Umbraco.TextBox",
                                        isSensitive: false,
                                        culture: null,
                                        segment: null
                                    }
                                ]
                            },
                            {
                                id: 1234,
                                label: "Styling",
                                properties: [
                                    {
                                        label: "Background color",
                                        description: "",
                                        view: "textbox",
                                        config: {maxChars: 500},
                                        hideLabel: false,
                                        validation: {mandatory: true, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                        readonly: false,
                                        id: 441,
                                        dataTypeKey: "0cc0eba1-9960-42c9-bf9b-60e150b429ae",
                                        value: "The purpose of lorem ipsum is to create a natural looking block of text (sentence, paragraph, page, etc.) that doesn´t distract from the layout. A practice not without controversy, laying out pages with meaningless filler text can be very useful when the focus is meant to be on design, not content.",
                                        alias: "pageTitle",
                                        editor: "Umbraco.TextBox",
                                        isSensitive: false,
                                        culture: null,
                                        segment: null
                                    }
                                ]
                            }
                        ]
                    }
                }
            },
            {
                alias: "pageModule",
                name: "Inline module",
                icon: "icon-document",
                prototype_paste_data: {
                    elementType: {
                        alias: "contentTypeAlias",
                        icon: "icon-document",
                        label: "Text"
                    },
                    labelTemplate: "{{imageTitle | truncate:true:36}}",
                    labelInterpolate: $interpolate("{{imageTitle | truncate:true:36}}"),
                    key: 1,
                    editor: "views/blockelements/inlineblock/inlineblock.editor.html",
                    overlaySize: "medium",
                    content: {
                        apps: [
                            {
                                name: "Content",
                                alias: "umbContent",
                                weight: -100,
                                icon: "icon-document",
                                view: "views/content/apps/content/content.html",
                                viewModel: 0,
                                active: true,
                                badge: null,
                                anchors: [],
                                hasError: false
                            },
                            {
                                name: "Info",
                                alias: "umbInfo",
                                weight: 100,
                                icon: "icon-info",
                                view: "views/content/apps/info/info.html",
                                viewModel: null,
                                active: false,
                                badge: null,
                                hasError: false
                            }
                        ],
                        variants: [
                            {
                                language: {
                                    isDefault: true
                                }
                            }
                        ],
                        tabs: [
                            {
                                id: 1234,
                                label: "Content",
                                properties: [
                                    {
                                        label: "Image Title",
                                        description: "The title on top of image",
                                        view: "textbox",
                                        config: {maxChars: 500},
                                        hideLabel: false,
                                        validation: {mandatory: true, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                        readonly: false,
                                        id: 441,
                                        dataTypeKey: "0cc0eba1-9960-42c9-bf9b-60e150b429ae",
                                        value: "The purpose of lorem ipsum is to create a natural looking block of text (sentence, paragraph, page, etc.) that doesn´t distract from the layout. A practice not without controversy, laying out pages with meaningless filler text can be very useful when the focus is meant to be on design, not content.",
                                        alias: "imageTitle",
                                        editor: "Umbraco.TextBox",
                                        isSensitive: false,
                                        culture: null,
                                        segment: null
                                    },
                                    {
                                        label: "Image",
                                        description: "",
                                        view: "mediapicker",
                                        config: {multiPicker: false, 
                                            onlyImages: true, 
                                            disableFolderSelect: true, 
                                            startNodeId: "umb://media/1fd2ecaff3714c009306867fa4585e7a", 
                                            ignoreUserStartNodes: false, 
                                            idType: "udi"
                                        },
                                        hideLabel: false,
                                        validation: {mandatory: false, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                        readonly: false,
                                        id: 495,
                                        dataTypeKey: "e26a8d91-a9d7-475b-bc3b-2a09f4743754",
                                        value: "umb://media/fa763e0d0ceb408c8720365d57e06e32",
                                        alias: "photo",
                                        editor: "Umbraco.MediaPicker",
                                        isSensitive: false,
                                        culture: null,
                                        segment: null
                                    }
                                ]
                            }
                        ]
                    }
                }
            },
            {
                alias: "contentTypeAlias",
                name: "Text",
                icon: "icon-info",
                prototype_paste_data: {
                    elementType: {
                        alias: "contentTypeAlias",
                        icon: "icon-document",
                        label: "Text"
                    },
                    labelTemplate: "Label",
                    labelInterpolate: $interpolate("Label"),
                    editor: "views/blockelements/textareablock/textareablock.editor.html",
                    overlaySize: "medium",
                    content: {
                        apps: [
                            {
                                name: "Content",
                                alias: "umbContent",
                                weight: -100,
                                icon: "icon-document",
                                view: "views/content/apps/content/content.html",
                                viewModel: 0,
                                active: true,
                                badge: null,
                                anchors: [],
                                hasError: false
                            },
                            {
                                name: "Info",
                                alias: "umbInfo",
                                weight: 100,
                                icon: "icon-info",
                                view: "views/content/apps/info/info.html",
                                viewModel: null,
                                active: false,
                                badge: null,
                                hasError: false
                            }
                        ],
                        variants: [
                            {
                                language: {
                                    isDefault: true
                                }
                            }
                        ],
                        tabs: [
                            {
                                id: 1234,
                                label: "Content",
                                properties: [
                                    {
                                        label: "Page Title",
                                        description: "The title of the page",
                                        view: "textbox",
                                        config: {maxChars: 500},
                                        hideLabel: false,
                                        validation: {mandatory: true, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                        readonly: false,
                                        id: 441,
                                        dataTypeKey: "0cc0eba1-9960-42c9-bf9b-60e150b429ae",
                                        value: "",
                                        alias: "pageTitle",
                                        editor: "Umbraco.TextBox",
                                        isSensitive: false,
                                        culture: null,
                                        segment: null
                                    }
                                ]
                            }
                        ]
                    }
                }
            },
            {
                alias: "contentTypeAlias",
                name: "Image",
                icon: "icon-picture",
                prototype_paste_data: {
                    elementType: {
                        alias: "contentTypeAlias",
                        icon: "icon-document",
                        label: "Text"
                    },
                    labelTemplate: "Label",
                    labelInterpolate: $interpolate("Label"),
                    editor: "views/blockelements/imageblock/imageblock.editor.html",
                    overlaySize: "medium",
                    content: {
                        apps: [
                            {
                                name: "Content",
                                alias: "umbContent",
                                weight: -100,
                                icon: "icon-document",
                                view: "views/content/apps/content/content.html",
                                viewModel: 0,
                                active: true,
                                badge: null,
                                anchors: [],
                                hasError: false
                            },
                            {
                                name: "Info",
                                alias: "umbInfo",
                                weight: 100,
                                icon: "icon-info",
                                view: "views/content/apps/info/info.html",
                                viewModel: null,
                                active: false,
                                badge: null,
                                hasError: false
                            }
                        ],
                        variants: [
                            {
                                language: {
                                    isDefault: true
                                }
                            }
                        ],
                        tabs: [
                            {
                                id: 1234,
                                label: "Content",
                                properties: [
                                    {
                                        label: "Image",
                                        description: "",
                                        view: "mediapicker",
                                        config: {multiPicker: false, 
                                            onlyImages: true, 
                                            disableFolderSelect: true, 
                                            startNodeId: "umb://media/1fd2ecaff3714c009306867fa4585e7a", 
                                            ignoreUserStartNodes: false, 
                                            idType: "udi"
                                        },
                                        hideLabel: false,
                                        validation: {mandatory: false, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                        readonly: false,
                                        id: 495,
                                        dataTypeKey: "e26a8d91-a9d7-475b-bc3b-2a09f4743754",
                                        value: "umb://media/fa763e0d0ceb408c8720365d57e06e32",
                                        alias: "photo",
                                        editor: "Umbraco.MediaPicker",
                                        isSensitive: false,
                                        culture: null,
                                        segment: null
                                    }, 
                                    {
                                        label: "Image Description",
                                        description: "The title of the page",
                                        view: "textbox",
                                        config: {maxChars: 500},
                                        hideLabel: false,
                                        validation: {mandatory: true, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                        readonly: false,
                                        id: 441,
                                        dataTypeKey: "0cc0eba1-9960-42c9-bf9b-60e150b429ae",
                                        value: "Let´s have a chat",
                                        alias: "imageDesc",
                                        editor: "Umbraco.TextBox",
                                        isSensitive: false,
                                        culture: null,
                                        segment: null
                                    }
                                ]
                            }
                        ],
                        temp_image: "/umbraco/assets/img/login.jpg"
                    }
                }
            }
        ];

        // var defaultBlockType...

        // TODO: get icon, properties etc. from available types?
        vm.blocks = [
            {
                elementType: {
                    alias: "contentTypeAlias",
                    icon: "icon-document",
                    label: "Text"
                },
                labelTemplate: "{{pageTitle | truncate:true:36}}",
                labelInterpolate: $interpolate("{{pageTitle | truncate:true:36}}"),
                key: 1,
                editor: "views/blockelements/labelblock/labelblock.editor.html",
                overlaySize: "medium",
                content: {
                    apps: [
                        {
                            name: "Content",
                            alias: "umbContent",
                            weight: -100,
                            icon: "icon-document",
                            view: "views/common/infiniteeditors/elementeditor/elementeditor.content.html",
                            viewModel: 0,
                            active: true,
                            badge: null,
                            anchors: [],
                            hasError: false
                        },
                        {
                            name: "Info",
                            alias: "umbInfo",
                            weight: 100,
                            icon: "icon-info",
                            view: "views/content/apps/info/info.html",
                            viewModel: null,
                            active: false,
                            badge: null,
                            hasError: false
                        }
                    ],
                    variants: [
                        {
                            language: {
                                isDefault: true
                            }
                        }
                    ],
                    tabs: [
                        {
                            id: 1234,
                            label: "Content",
                            properties: [
                                {
                                    label: "Page Title",
                                    description: "The title of the page",
                                    view: "textbox",
                                    config: {maxChars: 500},
                                    hideLabel: false,
                                    validation: {mandatory: true, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                    readonly: false,
                                    id: 441,
                                    dataTypeKey: "0cc0eba1-9960-42c9-bf9b-60e150b429ae",
                                    value: "The purpose of lorem ipsum is to create a natural looking block of text (sentence, paragraph, page, etc.) that doesn´t distract from the layout. A practice not without controversy, laying out pages with meaningless filler text can be very useful when the focus is meant to be on design, not content.",
                                    alias: "pageTitle",
                                    editor: "Umbraco.TextBox",
                                    isSensitive: false,
                                    culture: null,
                                    segment: null
                                },
                                {
                                    label: "Image",
                                    description: "",
                                    view: "mediapicker",
                                    config: {multiPicker: false, 
                                        onlyImages: true, 
                                        disableFolderSelect: true, 
                                        startNodeId: "umb://media/1fd2ecaff3714c009306867fa4585e7a", 
                                        ignoreUserStartNodes: false, 
                                        idType: "udi"
                                    },
                                    hideLabel: false,
                                    validation: {mandatory: false, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                    readonly: false,
                                    id: 495,
                                    dataTypeKey: "e26a8d91-a9d7-475b-bc3b-2a09f4743754",
                                    value: "umb://media/fa763e0d0ceb408c8720365d57e06e32",
                                    alias: "photo",
                                    editor: "Umbraco.MediaPicker",
                                    isSensitive: false,
                                    culture: null,
                                    segment: null
                                }, 
                                {
                                    label: "Image Description",
                                    description: "The title of the page",
                                    view: "textbox",
                                    config: {maxChars: 500},
                                    hideLabel: false,
                                    validation: {mandatory: true, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                    readonly: false,
                                    id: 442,
                                    dataTypeKey: "0cc0eba1-9960-42c9-bf9b-60e150b429ae",
                                    value: "Let´s have a chat",
                                    alias: "imageDesc",
                                    editor: "Umbraco.TextBox",
                                    isSensitive: false,
                                    culture: null,
                                    segment: null
                                }
                            ]
                        },
                        {
                            id: 1234,
                            label: "Styling",
                            properties: [
                                {
                                    label: "Background color",
                                    description: "",
                                    view: "textbox",
                                    config: {maxChars: 500},
                                    hideLabel: false,
                                    validation: {mandatory: true, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                    readonly: false,
                                    id: 441,
                                    dataTypeKey: "0cc0eba1-9960-42c9-bf9b-60e150b429ae",
                                    value: "The purpose of lorem ipsum is to create a natural looking block of text (sentence, paragraph, page, etc.) that doesn´t distract from the layout. A practice not without controversy, laying out pages with meaningless filler text can be very useful when the focus is meant to be on design, not content.",
                                    alias: "pageTitle",
                                    editor: "Umbraco.TextBox",
                                    isSensitive: false,
                                    culture: null,
                                    segment: null
                                }
                            ]
                        }
                    ]
                }
            },
            {
                elementType: {
                    alias: "contentTypeAlias",
                    icon: "icon-document",
                    label: "Text"
                },
                labelTemplate: "{{pageTitle | truncate:true:36}}",
                labelInterpolate: $interpolate("{{pageTitle | truncate:true:36}}"),
                key: 2,
                editor: "views/blockelements/labelblock/labelblock.editor.html",
                overlaySize: "medium",
                content: {
                    apps: [
                        {
                            name: "Content",
                            alias: "umbContent",
                            weight: -100,
                            icon: "icon-document",
                            view: "views/content/apps/content/content.html",
                            viewModel: 0,
                            active: true,
                            badge: null,
                            anchors: [],
                            hasError: false
                        },
                        {
                            name: "Info",
                            alias: "umbInfo",
                            weight: 100,
                            icon: "icon-info",
                            view: "views/content/apps/info/info.html",
                            viewModel: null,
                            active: false,
                            badge: null,
                            hasError: false
                        }
                    ],
                    variants: [
                        {
                            language: {
                                isDefault: true
                            }
                        }
                    ],
                    tabs: [
                        {
                            id: 1234,
                            label: "Content",
                            properties: [
                                {
                                    label: "Page Title",
                                    description: "The title of the page",
                                    view: "textbox",
                                    config: {maxChars: 500},
                                    hideLabel: false,
                                    validation: {mandatory: true, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                    readonly: false,
                                    id: 441,
                                    dataTypeKey: "0cc0eba1-9960-42c9-bf9b-60e150b429ae",
                                    value: "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                                    alias: "pageTitle",
                                    editor: "Umbraco.TextBox",
                                    isSensitive: false,
                                    culture: null,
                                    segment: null
                                },
                                {
                                    label: "Image",
                                    description: "",
                                    view: "mediapicker",
                                    config: {multiPicker: false, 
                                        onlyImages: true, 
                                        disableFolderSelect: true, 
                                        startNodeId: "umb://media/1fd2ecaff3714c009306867fa4585e7a", 
                                        ignoreUserStartNodes: false, 
                                        idType: "udi"
                                    },
                                    hideLabel: false,
                                    validation: {mandatory: false, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                    readonly: false,
                                    id: 495,
                                    dataTypeKey: "e26a8d91-a9d7-475b-bc3b-2a09f4743754",
                                    value: "umb://media/fa763e0d0ceb408c8720365d57e06e32",
                                    alias: "photo",
                                    editor: "Umbraco.MediaPicker",
                                    isSensitive: false,
                                    culture: null,
                                    segment: null
                                }, 
                                {
                                    label: "Image Description",
                                    description: "The title of the page",
                                    view: "textbox",
                                    config: {maxChars: 500},
                                    hideLabel: false,
                                    validation: {mandatory: true, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                    readonly: false,
                                    id: 442,
                                    dataTypeKey: "0cc0eba1-9960-42c9-bf9b-60e150b429ae",
                                    value: "Let´s have a chat",
                                    alias: "imageDesc",
                                    editor: "Umbraco.TextBox",
                                    isSensitive: false,
                                    culture: null,
                                    segment: null
                                }
                            ]
                        },
                        {
                            id: 1234,
                            label: "Styling",
                            properties: [
                                {
                                    label: "Background color",
                                    description: "",
                                    view: "textbox",
                                    config: {maxChars: 500},
                                    hideLabel: false,
                                    validation: {mandatory: true, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                    readonly: false,
                                    id: 441,
                                    dataTypeKey: "0cc0eba1-9960-42c9-bf9b-60e150b429ae",
                                    value: "The purpose of lorem ipsum is to create a natural looking block of text (sentence, paragraph, page, etc.) that doesn´t distract from the layout. A practice not without controversy, laying out pages with meaningless filler text can be very useful when the focus is meant to be on design, not content.",
                                    alias: "pageTitle",
                                    editor: "Umbraco.TextBox",
                                    isSensitive: false,
                                    culture: null,
                                    segment: null
                                }
                            ]
                        }
                    ]
                }
            },
            {
                
                elementType: {
                    alias: "contentTypeAlias",
                    icon: "icon-document",
                    label: "Text"
                },
                labelTemplate: "{{pageTitle | truncate:true:36}}",
                labelInterpolate: $interpolate("{{pageTitle | truncate:true:36}}"),
                key: 3,
                editor: "views/blockelements/labelblock/labelblock.editor.html",
                overlaySize: "medium",
                content: {
                    apps: [
                        {
                            name: "Content",
                            alias: "umbContent",
                            weight: -100,
                            icon: "icon-document",
                            view: "views/content/apps/content/content.html",
                            viewModel: 0,
                            active: true,
                            badge: null,
                            anchors: [],
                            hasError: false
                        },
                        {
                            name: "Info",
                            alias: "umbInfo",
                            weight: 100,
                            icon: "icon-info",
                            view: "views/content/apps/info/info.html",
                            viewModel: null,
                            active: false,
                            badge: null,
                            hasError: false
                        }
                    ],
                    variants: [
                        {
                            language: {
                                isDefault: true
                            }
                        }
                    ],
                    tabs: [
                        {
                            id: 1234,
                            label: "Content",
                            properties: [
                                {
                                    label: "Page Title",
                                    description: "The title of the page",
                                    view: "textbox",
                                    config: {maxChars: 500},
                                    hideLabel: false,
                                    validation: {mandatory: true, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                    readonly: false,
                                    id: 441,
                                    dataTypeKey: "0cc0eba1-9960-42c9-bf9b-60e150b429ae",
                                    value: "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                                    alias: "pageTitle",
                                    editor: "Umbraco.TextBox",
                                    isSensitive: false,
                                    culture: null,
                                    segment: null
                                },
                                {
                                    label: "Image",
                                    description: "",
                                    view: "mediapicker",
                                    config: {multiPicker: false, 
                                        onlyImages: true, 
                                        disableFolderSelect: true, 
                                        startNodeId: "umb://media/1fd2ecaff3714c009306867fa4585e7a", 
                                        ignoreUserStartNodes: false, 
                                        idType: "udi"
                                    },
                                    hideLabel: false,
                                    validation: {mandatory: false, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                    readonly: false,
                                    id: 495,
                                    dataTypeKey: "e26a8d91-a9d7-475b-bc3b-2a09f4743754",
                                    value: "umb://media/fa763e0d0ceb408c8720365d57e06e32",
                                    alias: "photo",
                                    editor: "Umbraco.MediaPicker",
                                    isSensitive: false,
                                    culture: null,
                                    segment: null
                                }, 
                                {
                                    label: "Image Description",
                                    description: "The title of the page",
                                    view: "textbox",
                                    config: {maxChars: 500},
                                    hideLabel: false,
                                    validation: {mandatory: true, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                    readonly: false,
                                    id: 442,
                                    dataTypeKey: "0cc0eba1-9960-42c9-bf9b-60e150b429ae",
                                    value: "Let´s have a chat",
                                    alias: "imageDesc",
                                    editor: "Umbraco.TextBox",
                                    isSensitive: false,
                                    culture: null,
                                    segment: null
                                }
                            ]
                        },
                        {
                            id: 1234,
                            label: "Styling",
                            properties: [
                                {
                                    label: "Background color",
                                    description: "",
                                    view: "textbox",
                                    config: {maxChars: 500},
                                    hideLabel: false,
                                    validation: {mandatory: true, mandatoryMessage: "", pattern: null, patternMessage: ""},
                                    readonly: false,
                                    id: 441,
                                    dataTypeKey: "0cc0eba1-9960-42c9-bf9b-60e150b429ae",
                                    value: "The purpose of lorem ipsum is to create a natural looking block of text (sentence, paragraph, page, etc.) that doesn´t distract from the layout. A practice not without controversy, laying out pages with meaningless filler text can be very useful when the focus is meant to be on design, not content.",
                                    alias: "pageTitle",
                                    editor: "Umbraco.TextBox",
                                    isSensitive: false,
                                    culture: null,
                                    segment: null
                                }
                            ]
                        }
                    ]
                }
            }
        ];



        

        function setDirty() {
            if (vm.propertyForm) {
                vm.propertyForm.$setDirty();
            }
        };

        function addNewBlock(index, type) {

            var block = angular.copy(type.prototype_paste_data);

            vm.blocks.splice(index, 0, block);
            $scope.moveFocusToBlock = block;

        }

        function getBlockLabel(block) {

            // TODO: we should do something about this for performance.

            var props = new Object();

            var tab = block.content.tabs[0];
            // TODO: need to look up all tabs...
            for(const property of tab.properties) {
                props[property.alias] = property.value;
            }

            if(block.labelInterpolate) {
                return block.labelInterpolate(props);
            }

            return "block.label";
        }

        vm.deleteBlock = function(block) {
            var index = vm.blocks.indexOf(block);
            if(index !== -1) {
                vm.blocks.splice(index, 1);
            }
            if(vm.quickMenuIndex > index) {
                vm.quickMenuIndex--;
            }
        }

        vm.editBlock = function(blockModel) {
            
            var elementEditor = {
                block: blockModel,
                view: "views/common/infiniteeditors/elementeditor/elementeditor.html",
                size: blockModel.overlaySize,
                submit: function(model) {
                    blockModel.content = model.block.content;
                    editorService.close();
                },
                close: function() {
                    editorService.close();
                }
            };

            // open property settings editor
            editorService.open(elementEditor);
        }

        vm.showCreateDialog = function (createIndex, $event) {
            
            if (vm.blockTypePicker) {
                return;
            }
            
            if (vm.availableBlockTypes.length === 0) {
                return;
            }

            vm.blockTypePicker = {
                show: true,
                size: vm.availableBlockTypes.length < 7 ? "small" : "medium",
                filter: vm.availableBlockTypes.length > 12 ? true : false,
                orderBy: "$index",
                view: "itempicker",
                event: $event,
                availableItems: vm.availableBlockTypes,
                submit: function (model) {
                    if (model && model.selectedItem) {
                        addNewBlock(createIndex, model.selectedItem);
                    }
                    vm.blockTypePicker.close();
                },
                close: function () {
                    vm.blockTypePicker.show = false;
                    vm.blockTypePicker = null;
                }
            };

        };

        vm.requestCopyBlock = function(block) {
            console.log("copy")
        }
        vm.requestDeleteBlock = function(block) {
            localizationService.localizeMany(["general_delete", "blockEditor_confirmDeleteBlockMessage", "contentTypeEditor_yesDelete"]).then(function (data) {
                const overlay = {
                    title: data[0],
                    content: localizationService.tokenReplace(data[1], [block.label]),
                    submitButtonLabel: data[2],
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        vm.deleteBlock(block);
                        overlayService.close();
                    }
                };

                overlayService.confirmDelete(overlay);
            });
        }

        vm.showCopy = clipboardService.isSupported();

        

        vm.sorting = false;
        vm.sortableOptions = {
            axis: "y",
            cursor: "grabbing",
            handle: ".blockelement__draggable-element",
            cancel: "input,textarea,select,option",
            classes: ".blockelement--dragging",
            distance: 5,
            tolerance: "pointer",
            scroll: true,
            start: function (ev, ui) {
                $scope.$apply(function () {
                    vm.sorting = true;
                });
            },
            update: function (ev, ui) {
                setDirty();
            },
            stop: function (ev, ui) {
                $scope.$apply(function () {
                    vm.sorting = false;
                });
            }
        };

        $scope.blockApi = {
            removeBlock: vm.removeBlock
        }


        var copyAllEntriesAction = {
            labelKey: "clipboard_labelForCopyAllEntries",
            labelTokens: [model.label],
            icon: "documents",
            method: function () {},
            isDisabled: true
        }

        var propertyActions = [
            copyAllEntriesAction
        ];
        
        this.$onInit = function () {
            if (this.umbProperty) {
                this.umbProperty.setPropertyActions(propertyActions);
            }
        };


        function validateLimits() {
            if (vm.validationLimit.min && vm.blocks.length < vm.validationLimit.min) {
                vm.propertyForm.minCount.$setValidity("minCount", false);
            }
            else {
                vm.propertyForm.minCount.$setValidity("minCount", true);
            }

            if (vm.validationLimit.max && vm.blocks.length > vm.validationLimit.max) {
                vm.propertyForm.maxCount.$setValidity("maxCount", false);
            }
            else {
                vm.propertyForm.maxCount.$setValidity("maxCount", true);
            }
        }




        // TODO: We need to investigate if we can do a specific watch on each block, so we dont re-render all blocks.
        unsubscribe.push($scope.$watch("vm.blocks", onBlocksUpdated, true));
        function onBlocksUpdated(newVal, oldVal){
            for(const block of vm.blocks) {
                block.label = getBlockLabel(block);
            }
        }
        unsubscribe.push($scope.$watch(() => vm.blocks.length, validateLimits));

        $scope.$on("$destroy", function () {
            for (const subscription of unsubscribe) {
                subscription();
            }
        });


    }

})();
