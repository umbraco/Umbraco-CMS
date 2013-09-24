/**
 * @ngdoc service
 * @name umbraco.services.tinyMceService
 *
 *  
 * @description
 * A service containing all logic for all of the Umbraco TinyMCE plugins
 */
function tinyMceService(dialogService, $log, imageHelper, assetsService, $timeout, macroResource) {
    return {

        /**
        * @ngdoc method
        * @name umbraco.services.tinyMceService#createInsertEmbeddedMedia
        * @methodOf umbraco.services.tinyMceService
        *
        * @description
        * Creates the umbrco insert embedded media tinymce plugin
        *
        * @param {Object} editor the TinyMCE editor instance        
        * @param {Object} $scope the current controller scope
        */
        createInsertEmbeddedMedia: function (editor, $scope) {
            editor.addButton('umbembeddialog', {
                icon: 'media',
                tooltip: 'Embed',
                onclick: function () {
                    dialogService.embedDialog({
                        scope: $scope, callback: function (data) {
                            editor.insertContent(data);
                        }
                    });
                }
            });
        },

        /**
        * @ngdoc method
        * @name umbraco.services.tinyMceService#createMediaPicker
        * @methodOf umbraco.services.tinyMceService
        *
        * @description
        * Creates the umbrco insert media tinymce plugin
        *
        * @param {Object} editor the TinyMCE editor instance        
        * @param {Object} $scope the current controller scope
        */
        createMediaPicker: function (editor, $scope) {
            editor.addButton('umbmediapicker', {
                icon: 'media',
                tooltip: 'Media Picker',
                onclick: function () {
                    dialogService.mediaPicker({
                        scope: $scope, callback: function (img) {

                            if (img) {
                                var imagePropVal = imageHelper.getImagePropertyValue({ imageModel: img, scope: $scope });
                                var data = {
                                    src: (imagePropVal) ? imagePropVal : "nothing.jpg",
                                    id: '__mcenew'
                                };


                                editor.insertContent(editor.dom.createHTML('img', data));

                                $timeout(function () {
                                    var imgElm = editor.dom.get('__mcenew');
                                    var size = editor.dom.getSize(imgElm);
                                    $log.log(size);

                                    var newSize = imageHelper.scaleToMaxSize(500, size.w, size.h);
                                    var s = "width: " + newSize.width + "px; height:" + newSize.height + "px;";
                                    editor.dom.setAttrib(imgElm, 'style', s);
                                    editor.dom.setAttrib(imgElm, 'rel', newSize.width + "," + newSize.height);
                                    editor.dom.setAttrib(imgElm, 'id', null);

                                }, 500);
                            }
                        }
                    });
                }
            });
        },

        /**
        * @ngdoc method
        * @name umbraco.services.tinyMceService#createIconPicker
        * @methodOf umbraco.services.tinyMceService
        *
        * @description
        * Creates the umbrco insert icon tinymce plugin
        *
        * @param {Object} editor the TinyMCE editor instance    
        * @param {Object} $scope the current controller scope
        */
        createIconPicker: function (editor, $scope) {
            editor.addButton('umbiconpicker', {
                icon: 'media',
                tooltip: 'Icon Picker',
                onclick: function () {
                    dialogService.open({
                        show: true, template: "views/common/dialogs/iconpicker.html", scope: $scope, callback: function (c) {

                            var data = {
                                style: 'font-size: 60px'
                            };

                            var i = editor.dom.createHTML('i', data);
                            tinyMCE.activeEditor.dom.addClass(i, c);
                            editor.insertContent(i);
                        }
                    });
                }
            });
        },

        /**
        * @ngdoc method
        * @name umbraco.services.tinyMceService#createUmbracoMacro
        * @methodOf umbraco.services.tinyMceService
        *
        * @description
        * Creates the insert umbrco macro tinymce plugin
        *
        * @param {Object} editor the TinyMCE editor instance      
        * @param {Object} $scope the current controller scope
        */
        createInsertMacro: function (editor, $scope) {
            
            /** Adds custom rules for the macro plugin and custom serialization */
            editor.on('preInit', function (args) {
                //this is requires so that we tell the serializer that a 'div' is actually allowed in the root, otherwise the cleanup will strip it out
                editor.serializer.addRules('div');

                /** This checks if the div is a macro container, if so, checks if its wrapped in a p tag and then unwraps it (removes p tag) */
                editor.serializer.addNodeFilter('div', function (nodes, name) {
                    for (var i = 0; i < nodes.length; i++) {
                        if (nodes[i].attr("class") === "umb-macro-holder" && nodes[i].parent && nodes[i].parent.name.toUpperCase() === "P") {
                            nodes[i].parent.unwrap();
                        }
                    }
                });
            
            });

            /** Adds the button instance */
            editor.addButton('umbmacro', {
                icon: 'custom icon-settings-alt',
                tooltip: 'Insert macro',
                onPostRender: function () {

                    var ctrl = this;
                    var isOnMacroElement = false;

                    /**
                    * Because the macro gets wrapped in a P tag because of the way 'enter' works, this 
                    * method will return the macro element if not wrapped in a p, or the p if the macro
                    * element is the only one inside of it even if we are deep inside an element inside the macro
                    */
                    function getRealMacroElem(element) {
                        var e = $(element).closest(".umb-macro-holder");
                        if (e.length > 0) {
                            if (e.get(0).parentNode.nodeName === "P") {
                                //now check if we're the only element                    
                                if (element.parentNode.childNodes.length === 1) {
                                    return e.get(0).parentNode;
                                }
                            }
                            return e.get(0);
                        }
                        return null;
                    }

                    /**
                    * Add a node change handler, test if we're editing a macro and select the whole thing, then set our isOnMacroElement flag.
                    * If we change the selection inside this method, then we end up in an infinite loop, so we have to remove ourselves
                    * from the event listener before changing selection, however, it seems that putting a break point in this method
                    * will always cause an 'infinite' loop as the caret keeps changing.
                    */
                    function onNodeChanged(evt) {

                        //set our macro button active when on a node of class umb-macro-holder
                        var $macroElement = $(evt.element).closest(".umb-macro-holder");

                        ctrl.active($macroElement.length !== 0);

                        if ($macroElement.length > 0) {
                            var macroElement = $macroElement.get(0);

                            //remove the event listener before re-selecting
                            editor.off('NodeChange', onNodeChanged);
                            
                            // move selection to top element to ensure we can't edit this
                            editor.selection.select(macroElement);

                            // check if the current selection *is* the element (ie bug)
                            var currentSelection = editor.selection.getStart();
                            if (tinymce.isIE) {
                                if (!editor.dom.hasClass(currentSelection, 'umb-macro-holder')) {
                                    while (!editor.dom.hasClass(currentSelection, 'umb-macro-holder') && currentSelection.parentNode) {
                                        currentSelection = currentSelection.parentNode;
                                    }
                                    editor.selection.select(currentSelection);
                                }
                            }

                            //set the flag
                            isOnMacroElement = true;

                            //re-add the event listener
                            editor.on('NodeChange', onNodeChanged);
                        }
                        else {
                            isOnMacroElement = false;
                        }

                    }

                    /** This prevents any other commands from executing when the current element is the macro so the content cannot be edited */
                    editor.on('BeforeExecCommand', function (o) {                        
                        if (isOnMacroElement) {
                            if (o.preventDefault) {
                                o.preventDefault();
                            }
                            if (o.stopImmediatePropagation) {
                                o.stopImmediatePropagation();
                            }
                            return;
                        }
                    });
                    
                    /** This double checks and ensures you can't paste content into the rendered macro */
                    editor.on("Paste", function (o) {                        
                        if (isOnMacroElement) {
                            if (o.preventDefault) {
                                o.preventDefault();
                            }
                            if (o.stopImmediatePropagation) {
                                o.stopImmediatePropagation();
                            }
                            return;
                        }
                    });

                    //set onNodeChanged event listener
                    editor.on('NodeChange', onNodeChanged);

                    /** 
                    * Listen for the keydown in the editor, we'll check if we are currently on a macro element, if so
                    * we'll check if the key down is a supported key which requires an action, otherwise we ignore the request
                    * so the macro cannot be edited.
                    */
                    editor.on('KeyDown', function (e) {
                        if (isOnMacroElement) {
                            var macroElement = editor.selection.getNode();

                            //get the 'real' element (either p or the real one)
                            macroElement = getRealMacroElem(macroElement);

                            //prevent editing
                            e.preventDefault();
                            e.stopPropagation();

                            var moveSibling = function (element, isNext) {
                                var $e = $(element);
                                var $sibling = isNext ? $e.next() : $e.prev();
                                if ($sibling.length > 0) {
                                    editor.selection.select($sibling.get(0));
                                    editor.selection.collapse(true);
                                }
                                else {
                                    //if we're moving previous and there is no sibling, then lets recurse and just select the next one
                                    if (!isNext) {
                                        moveSibling(element, true);
                                        return;
                                    }

                                    //if there is no sibling we'll generate a new p at the end and select it
                                    editor.setContent(editor.getContent() + "<p>&nbsp;</p>");
                                    editor.selection.select($(editor.dom.getRoot()).children().last().get(0));
                                    editor.selection.collapse(true);

                                }
                            };

                            //supported keys to move to the next or prev element (13-enter, 27-esc, 38-up, 40-down, 39-right, 37-left)
                            //supported keys to remove the macro (8-backspace, 46-delete)
                            //TODO: Should we make the enter key insert a line break before or leave it as moving to the next element?
                            if ($.inArray(e.keyCode, [13, 40, 39]) !== -1) {
                                //move to next element
                                moveSibling(macroElement, true);
                            }
                            else if ($.inArray(e.keyCode, [27, 38, 37]) !== -1) {
                                //move to prev element
                                moveSibling(macroElement, false);
                            }
                            else if ($.inArray(e.keyCode, [8, 46]) !== -1) {
                                //delete macro element

                                //move first, then delete
                                moveSibling(macroElement, false);
                                editor.dom.remove(macroElement);
                            }
                            return ;
                        }
                    });

                },
                
                /** The insert macro button click event handler */
                onclick: function () {

                    dialogService.open({
                        show: true,
                        template: "views/common/dialogs/insertmacro.html",
                        scope: $scope,
                        callback: function(data) {

                            //put the macro syntax in comments, we will parse this out on the server side to be used
                            //for persisting.
                            var macroSyntaxComment = "<!-- " + data.syntax + " -->";
                            //create an id class for this element so we can re-select it after inserting
                            var uniqueId = "umb-macro-" + editor.dom.uniqueId();
                            var macroDiv = editor.dom.create('div',
                                {
                                    'class': 'umb-macro-holder ' + data.macroAlias + ' mceNonEditable ' + uniqueId
                                },
                                macroSyntaxComment + '<ins>Macro alias: <strong>' + data.macroAlias + '</strong></ins>');

                            editor.selection.setNode(macroDiv);
                            
                            var $macroDiv = $(editor.dom.select("div.umb-macro-holder." + uniqueId));
                            var $ins = $macroDiv.find("ins");

                            //show the throbber
                            $macroDiv.addClass("loading");
                            
                            macroResource.getMacroResultAsHtmlForEditor(data.macroAlias, 1234)
                                .then(function (htmlResult) {
                                    
                                    $macroDiv.removeClass("loading");
                                    htmlResult = htmlResult.trim();
                                    if (htmlResult !== "") {
                                        $ins.html(htmlResult);
                                    }
                                });
                        }
                    });

                }
            });
        }
    };
}

angular.module('umbraco.services').factory('tinyMceService', tinyMceService);