(function () {
    'use strict';

    angular
        .module('umbraco')
        .component('blockListPropertyEditor', {
            templateUrl: 'views/propertyeditors/blocklist/blocklist.component.html',
            controller: BlockListController,
            controllerAs: 'vm',
            bindings: {
                
            },
            require: {
                umbProperty: '?^umbProperty',
                propertyForm: '?^propertyForm'
            }
        });

    function BlockListController($scope, $interpolate, editorService, clipboardService, localizationService, overlayService) {
        
        var vm = this;
        var model = $scope.$parent.$parent.model;

        $scope.moveFocusToBlock = null;

        vm.quickMenuVisible = false;
        vm.quickMenuIndex = 0;
        
        vm.quickMenuAddNewBlock = function(type) {
            addNewBlock(vm.quickMenuIndex, type);
            vm.quickMenuVisible = false;
        }
        
        vm.availableBlockTypes = [
            {
                alias: "pageModule",
                name: "Module",
                icon: "icon-document",
                prototype_paste_data: {
                    
                    elementType: {
                        alias: 'contentTypeAlias',
                        icon: "icon-document",
                        label: "Text"
                    },
                    label: "{{pageTitle | truncate:true:36}}",
                    labelInterpolate: $interpolate("{{pageTitle | truncate:true:36}}"),
                    editor: "views/blockelements/labelblock/labelblock.editor.html",
                    content: {
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
                                label: "Group 1",
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
                                    }
                                ]
                            }
                        ]
                    }
                }
            },
            {
                alias: "contentTypeAlias",
                name: "contentTypeName",
                icon: "icon-text",
                prototype_paste_data: {
                    elementType: {
                        alias: 'contentTypeAlias',
                        icon: "icon-document",
                        label: "Text"
                    },
                    label: "Label",
                    editor: "views/blockelements/textareablock/textareablock.editor.html",
                    content: {
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
                                label: "Group 1",
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
                name: "contentTypeName",
                icon: "icon-picture",
                prototype_paste_data: {
                    elementType: {
                        alias: 'contentTypeAlias',
                        icon: "icon-document",
                        label: "Text"
                    },
                    label: "Label",
                    editor: "views/blockelements/imageblock/imageblock.editor.html",
                    content: {
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
                                label: "Group 1",
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
                                        value: "Let's have a chat",
                                        alias: "pageTitle",
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
                    alias: 'contentTypeAlias',
                    icon: "icon-document",
                    label: "Text"
                },
                label: "{{pageTitle | truncate:true:36}}",
                labelInterpolate: $interpolate("{{pageTitle | truncate:true:36}}"),
                key: 1,
                editor: "views/blockelements/labelblock/labelblock.editor.html",
                content: {
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
                            label: "Group 1",
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
                                    value: "The purpose of lorem ipsum is to create a natural looking block of text (sentence, paragraph, page, etc.) that doesn't distract from the layout. A practice not without controversy, laying out pages with meaningless filler text can be very useful when the focus is meant to be on design, not content.",
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
                    alias: 'contentTypeAlias',
                    icon: "icon-document",
                    label: "Text"
                },
                label: "{{pageTitle | truncate:true:36}}",
                labelInterpolate: $interpolate("{{pageTitle | truncate:true:36}}"),
                key: 2,
                editor: "views/blockelements/labelblock/labelblock.editor.html",
                content: {
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
                            label: "Group 1",
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
                                }
                            ]
                        }
                    ]
                }
            },
            {
                
                elementType: {
                    alias: 'contentTypeAlias',
                    icon: "icon-document",
                    label: "Text"
                },
                label: "{{pageTitle | truncate:true:36}}",
                labelInterpolate: $interpolate("{{pageTitle | truncate:true:36}}"),
                key: 3,
                editor: "views/blockelements/labelblock/labelblock.editor.html",
                content: {
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
                            label: "Group 1",
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
        /*
        function moveFocusToNextBlock(blockModel, $event) {
            var index = vm.blocks.indexOf(blockModel);
            if(index < vm.blocks.length) {
                var nextBlock = vm.blocks[index+1];
                $scope.moveFocusToBlock = nextBlock;
            } else {
                showCreateOptions(blockModel, $event);
            }
        }
        */

        vm.showCreateOptionsFor = function(blockModel, $event) {
            var index = vm.blocks.indexOf(blockModel);
            $event.preventDefault();
            showCreateOptionsAt(index);
        }
        function showCreateOptionsAt(index) {
            vm.quickMenuIndex = index;
            vm.quickMenuVisible = true;
            window.addEventListener("keydown", handleTypingInCreateOptions);
        }

        function handleTypingInCreateOptions(event) {
            if (event.ctrlKey || event.metaKey || event.altKey)
                return;

            if (
                (event.keyCode === 13) // enter
                ||
                (event.keyCode >= 48 && event.keyCode <= 90)// 0 to z
                ||
                (event.keyCode >= 96 && event.keyCode <= 111)// numpads
                ||
                (event.keyCode >= 186 && event.keyCode <= 222)// semi-colon and a lot of other special characters
             ) {
                // Continue writting... needs to know default text-element. if we have one.
            }
        }

        function hideCreateOptions() {
            vm.quickMenuVisible = false;
            window.removeEventListener("keydown", handleTypingInCreateOptions);
        }

        vm.onCreateOptionsBlur = function($event) {

            if(!$($event.relatedTarget).is(".umb-block-list__block--create-bar > button")) {
                hideCreateOptions();
            }

        }
        
        vm.getBlockLabel = function(block) {

            var name = "";

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
                size: "large",
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
                size: vm.availableBlockTypes.length > 6 ? "medium" : "small",
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
            localizationService.localizeMany(["content_nestedContentDeleteItem", "general_delete", "general_cancel", "contentTypeEditor_yesDelete"]).then(function (data) {
                const overlay = {
                    title: data[1],
                    content: data[0],
                    closeButtonLabel: data[2],
                    submitButtonLabel: data[3],
                    submitButtonStyle: "danger",
                    close: function () {
                        overlayService.close();
                    },
                    submit: function () {
                        vm.deleteBlock(block);
                        overlayService.close();
                    }
                };

                overlayService.open(overlay);
            });
        }

        vm.showCopy = clipboardService.isSupported();

        

        vm.sorting = false;
        vm.sortableOptions = {
            axis: "y",
            cursor: "grabbing",
            handle: '.umb-block-list__block',
            cancel: 'input,textarea,select,option',
            classes: '.blockelement--dragging',
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
            showCreateOptionsFor: vm.showCreateOptionsFor,
            removeBlock: vm.removeBlock
        }


        var copyAllEntriesAction = {
            labelKey: 'clipboard_labelForCopyAllEntries',
            labelTokens: [model.label],
            icon: 'documents',
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


    }

})();
