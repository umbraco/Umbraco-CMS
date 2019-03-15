/**
 * @ngdoc service
 * @name umbraco.services.tinyMceService
 *
 *  
 * @description
 * A service containing all logic for all of the Umbraco TinyMCE plugins
 */
function tinyMceService($log, imageHelper, $http, $timeout, macroResource, macroService, $routeParams, umbRequestHelper, angularHelper, userService) {
	return {

		/**
		 * @ngdoc method
		 * @name umbraco.services.tinyMceService#configuration
		 * @methodOf umbraco.services.tinyMceService
		 *
		 * @description
		 * Returns a collection of plugins available to the tinyMCE editor
		 *
		 */
		configuration: function () {
			return umbRequestHelper.resourcePromise(
				$http.get(
					umbRequestHelper.getApiUrl(
						"rteApiBaseUrl",
						"GetConfiguration"), {
						cache: true
					}),
				'Failed to retrieve tinymce configuration');
		},

		/**
		 * @ngdoc method
		 * @name umbraco.services.tinyMceService#defaultPrevalues
		 * @methodOf umbraco.services.tinyMceService
		 *
		 * @description
		 * Returns a default configration to fallback on in case none is provided
		 *
		 */
		defaultPrevalues: function () {
			var cfg = {};
			cfg.toolbar = ["code", "bold", "italic", "styleselect", "alignleft", "aligncenter", "alignright", "bullist", "numlist", "outdent", "indent", "link", "image", "umbmediapicker", "umbembeddialog", "umbmacro"];
			cfg.stylesheets = [];
			cfg.dimensions = {
				height: 500
			};
			cfg.maxImageSize = 500;
			return cfg;
		},

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
		createInsertEmbeddedMedia: function (editor, scope, callback) {
			editor.addButton('umbembeddialog', {
				icon: 'custom icon-tv',
				tooltip: 'Embed',
				onclick: function () {
					if (callback) {
						callback();
					}
				}
			});
		},

		insertEmbeddedMediaInEditor: function (editor, preview) {
			editor.insertContent(preview);
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
		createMediaPicker: function (editor, scope, callback) {
			editor.addButton('umbmediapicker', {
				icon: 'custom icon-picture',
				tooltip: 'Media Picker',
				stateSelector: 'img',
				onclick: function () {

					var selectedElm = editor.selection.getNode(),
						currentTarget;


					if (selectedElm.nodeName === 'IMG') {
						var img = $(selectedElm);

						var hasUdi = img.attr("data-udi") ? true : false;

						currentTarget = {
							altText: img.attr("alt"),
							url: img.attr("src")
						};

						if (hasUdi) {
							currentTarget["udi"] = img.attr("data-udi");
						} else {
							currentTarget["id"] = img.attr("rel");
						}
					}

					userService.getCurrentUser().then(function (userData) {
						if (callback) {
							callback(currentTarget, userData);
						}
					});

				}
			});
		},

		insertMediaInEditor: function (editor, img) {
			if (img) {

				var hasUdi = img.udi ? true : false;

				var data = {
					alt: img.altText || "",
					src: (img.url) ? img.url : "nothing.jpg",
					id: '__mcenew'
				};

				if (hasUdi) {
					data["data-udi"] = img.udi;
				} else {
					//Considering these fixed because UDI will now be used and thus
					// we have no need for rel http://issues.umbraco.org/issue/U4-6228, http://issues.umbraco.org/issue/U4-6595
					data["rel"] = img.id;
					data["data-id"] = img.id;
				}

				editor.insertContent(editor.dom.createHTML('img', data));

				$timeout(function () {
					var imgElm = editor.dom.get('__mcenew');
					var size = editor.dom.getSize(imgElm);

					if (editor.settings.maxImageSize && editor.settings.maxImageSize !== 0) {
						var newSize = imageHelper.scaleToMaxSize(editor.settings.maxImageSize, size.w, size.h);

						var s = "width: " + newSize.width + "px; height:" + newSize.height + "px;";
						editor.dom.setAttrib(imgElm, 'style', s);

						if (img.url) {
							var src = img.url + "?width=" + newSize.width + "&height=" + newSize.height;
							editor.dom.setAttrib(imgElm, 'data-mce-src', src);
						}
                    }
				    editor.dom.setAttrib(imgElm, 'id', null);
				}, 500);
			}
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
		createInsertMacro: function (editor, $scope, callback) {

			var createInsertMacroScope = this;

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

			/** Adds the button instance */
			editor.addButton('umbmacro', {
				icon: 'custom icon-settings-alt',
				tooltip: 'Insert macro',
				onPostRender: function () {

					var ctrl = this;
					var isOnMacroElement = false;

					/**
					 if the selection comes from a different element that is not the macro's
					 we need to check if the selection includes part of the macro, if so we'll force the selection
					 to clear to the next element since if people can select part of the macro markup they can then modify it.
					*/
					function handleSelectionChange() {

						if (!editor.selection.isCollapsed()) {
							var endSelection = tinymce.activeEditor.selection.getEnd();
							var startSelection = tinymce.activeEditor.selection.getStart();
							//don't proceed if it's an entire element selected
							if (endSelection !== startSelection) {

								//if the end selection is a macro then move the cursor
								//NOTE: we don't have to handle when the selection comes from a previous parent because
								// that is automatically taken care of with the normal onNodeChanged logic since the 
								// evt.element will be the macro once it becomes part of the selection.
								var $testForMacro = $(endSelection).closest(".umb-macro-holder");
								if ($testForMacro.length > 0) {

									//it came from before so move after, if there is no after then select ourselves
									var next = $testForMacro.next();
									if (next.length > 0) {
										editor.selection.setCursorLocation($testForMacro.next().get(0));
									} else {
										selectMacroElement($testForMacro.get(0));
									}

								}
							}
						}
					}

					/** helper method to select the macro element */
					function selectMacroElement(macroElement) {

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

						handleSelectionChange();

						//set the button active
						ctrl.active($macroElement.length !== 0);

						if ($macroElement.length > 0) {
							var macroElement = $macroElement.get(0);

							//remove the event listener before re-selecting
							editor.off('NodeChange', onNodeChanged);

							selectMacroElement(macroElement);

							//set the flag
							isOnMacroElement = true;

							//re-add the event listener
							editor.on('NodeChange', onNodeChanged);
						} else {
							isOnMacroElement = false;
						}

					}

					/** when the contents load we need to find any macros declared and load in their content */
					editor.on("LoadContent", function (o) {

						//get all macro divs and load their content
						$(editor.dom.select(".umb-macro-holder.mceNonEditable")).each(function () {
							createInsertMacroScope.loadMacroContent($(this), null, $scope);
						});

					});

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
								} else {
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
							} else if ($.inArray(e.keyCode, [27, 38, 37]) !== -1) {
								//move to prev element
								moveSibling(macroElement, false);
							} else if ($.inArray(e.keyCode, [8, 46]) !== -1) {
								//delete macro element

								//move first, then delete
								moveSibling(macroElement, false);
								editor.dom.remove(macroElement);
							}
							return;
						}
					});

				},

				/** The insert macro button click event handler */
				onclick: function () {

					var dialogData = {
						//flag for use in rte so we only show macros flagged for the editor
						richTextEditor: true
					};

					//when we click we could have a macro already selected and in that case we'll want to edit the current parameters
					//so we'll need to extract them and submit them to the dialog.
					var macroElement = editor.selection.getNode();
					macroElement = getRealMacroElem(macroElement);
					if (macroElement) {
						//we have a macro selected so we'll need to parse it's alias and parameters
						var contents = $(macroElement).contents();
						var comment = _.find(contents, function (item) {
							return item.nodeType === 8;
						});
						if (!comment) {
							throw "Cannot parse the current macro, the syntax in the editor is invalid";
						}
						var syntax = comment.textContent.trim();
						var parsed = macroService.parseMacroSyntax(syntax);
						dialogData = {
							macroData: parsed
						};
					}

					if (callback) {
						callback(dialogData);
					}

				}
			});
		},

		insertMacroInEditor: function (editor, macroObject, $scope) {

			//put the macro syntax in comments, we will parse this out on the server side to be used
			//for persisting.
			var macroSyntaxComment = "<!-- " + macroObject.syntax + " -->";
			//create an id class for this element so we can re-select it after inserting
			var uniqueId = "umb-macro-" + editor.dom.uniqueId();
			var macroDiv = editor.dom.create('div', {
					'class': 'umb-macro-holder ' + macroObject.macroAlias + ' mceNonEditable ' + uniqueId
				},
				macroSyntaxComment + '<ins>Macro alias: <strong>' + macroObject.macroAlias + '</strong></ins>');

			editor.selection.setNode(macroDiv);

			var $macroDiv = $(editor.dom.select("div.umb-macro-holder." + uniqueId));

			//async load the macro content
			this.loadMacroContent($macroDiv, macroObject, $scope);

		},

		/** loads in the macro content async from the server */
		loadMacroContent: function ($macroDiv, macroData, $scope) {

			//if we don't have the macroData, then we'll need to parse it from the macro div
			if (!macroData) {
				var contents = $macroDiv.contents();
				var comment = _.find(contents, function (item) {
					return item.nodeType === 8;
				});
				if (!comment) {
					throw "Cannot parse the current macro, the syntax in the editor is invalid";
				}
				var syntax = comment.textContent.trim();
				var parsed = macroService.parseMacroSyntax(syntax);
				macroData = parsed;
			}

			var $ins = $macroDiv.find("ins");

			//show the throbber
			$macroDiv.addClass("loading");

			var contentId = $routeParams.id;

			//need to wrap in safe apply since this might be occuring outside of angular
			angularHelper.safeApply($scope, function () {
				macroResource.getMacroResultAsHtmlForEditor(macroData.macroAlias, contentId, macroData.macroParamsDictionary)
					.then(function (htmlResult) {

						$macroDiv.removeClass("loading");
						htmlResult = htmlResult.trim();
						if (htmlResult !== "") {
							$ins.html(htmlResult);
						}
					});
			});

		},

		createLinkPicker: function (editor, $scope, onClick) {

			function createLinkList(callback) {
				return function () {
					var linkList = editor.settings.link_list;

					if (typeof (linkList) === "string") {
						tinymce.util.XHR.send({
							url: linkList,
							success: function (text) {
								callback(tinymce.util.JSON.parse(text));
							}
						});
					} else {
						callback(linkList);
					}
				};
			}

			function showDialog(linkList) {
				var data = {},
					selection = editor.selection,
					dom = editor.dom,
					selectedElm, anchorElm, initialText;
				var win, linkListCtrl, relListCtrl, targetListCtrl;

				function linkListChangeHandler(e) {
					var textCtrl = win.find('#text');

					if (!textCtrl.value() || (e.lastControl && textCtrl.value() === e.lastControl.text())) {
						textCtrl.value(e.control.text());
					}

					win.find('#href').value(e.control.value());
				}

				function buildLinkList() {
					var linkListItems = [{
						text: 'None',
						value: ''
                    }];

					tinymce.each(linkList, function (link) {
						linkListItems.push({
							text: link.text || link.title,
							value: link.value || link.url,
							menu: link.menu
						});
					});

					return linkListItems;
				}

				function buildRelList(relValue) {
					var relListItems = [{
						text: 'None',
						value: ''
                    }];

					tinymce.each(editor.settings.rel_list, function (rel) {
						relListItems.push({
							text: rel.text || rel.title,
							value: rel.value,
							selected: relValue === rel.value
						});
					});

					return relListItems;
				}

				function buildTargetList(targetValue) {
					var targetListItems = [{
						text: 'None',
						value: ''
                    }];

					if (!editor.settings.target_list) {
						targetListItems.push({
							text: 'New window',
							value: '_blank'
						});
					}

					tinymce.each(editor.settings.target_list, function (target) {
						targetListItems.push({
							text: target.text || target.title,
							value: target.value,
							selected: targetValue === target.value
						});
					});

					return targetListItems;
				}

				function buildAnchorListControl(url) {
					var anchorList = [];

					tinymce.each(editor.dom.select('a:not([href])'), function (anchor) {
						var id = anchor.name || anchor.id;

						if (id) {
							anchorList.push({
								text: id,
								value: '#' + id,
								selected: url.indexOf('#' + id) !== -1
							});
						}
					});

					if (anchorList.length) {
						anchorList.unshift({
							text: 'None',
							value: ''
						});

						return {
							name: 'anchor',
							type: 'listbox',
							label: 'Anchors',
							values: anchorList,
							onselect: linkListChangeHandler
						};
					}
				}

				function updateText() {
					if (!initialText && data.text.length === 0) {
						this.parent().parent().find('#text')[0].value(this.value());
					}
				}

				selectedElm = selection.getNode();
				anchorElm = dom.getParent(selectedElm, 'a[href]');

				data.text = initialText = anchorElm ? (anchorElm.innerText || anchorElm.textContent) : selection.getContent({
					format: 'text'
				});
				data.href = anchorElm ? dom.getAttrib(anchorElm, 'href') : '';
				data.target = anchorElm ? dom.getAttrib(anchorElm, 'target') : '';
				data.rel = anchorElm ? dom.getAttrib(anchorElm, 'rel') : '';

				if (selectedElm.nodeName === "IMG") {
					data.text = initialText = " ";
				}

				if (linkList) {
					linkListCtrl = {
						type: 'listbox',
						label: 'Link list',
						values: buildLinkList(),
						onselect: linkListChangeHandler
					};
				}

				if (editor.settings.target_list !== false) {
					targetListCtrl = {
						name: 'target',
						type: 'listbox',
						label: 'Target',
						values: buildTargetList(data.target)
					};
				}

				if (editor.settings.rel_list) {
					relListCtrl = {
						name: 'rel',
						type: 'listbox',
						label: 'Rel',
						values: buildRelList(data.rel)
					};
				}

				var currentTarget = null;

				//if we already have a link selected, we want to pass that data over to the dialog
				if (anchorElm) {
					var anchor = $(anchorElm);
					currentTarget = {
						name: anchor.attr("title"),
						url: anchor.attr("href"),
						target: anchor.attr("target")
					};

					// drop the lead char from the anchor text, if it has a value
					var anchorVal = anchor[0].dataset.anchor;
					if (anchorVal) {
						currentTarget.anchor = anchorVal.substring(1);
					}

					//locallink detection, we do this here, to avoid poluting the dialogservice
					//so the dialog service can just expect to get a node-like structure
					if (currentTarget.url.indexOf("localLink:") > 0) {
						// if the current link has an anchor, it needs to be considered when getting the udi/id
						// if an anchor exists, reduce the substring max by its length plus two to offset the removed prefix and trailing curly brace
						var linkId = currentTarget.url.substring(currentTarget.url.indexOf(":") + 1, currentTarget.url.lastIndexOf("}"));

						//we need to check if this is an INT or a UDI
						var parsedIntId = parseInt(linkId, 10);
						if (isNaN(parsedIntId)) {
							//it's a UDI
							currentTarget.udi = linkId;
						} else {
							currentTarget.id = linkId;
						}
					}
				}

				if (onClick) {
					onClick(currentTarget, anchorElm);
				}

			}

			editor.addButton('link', {
				icon: 'link',
				tooltip: 'Insert/edit link',
				shortcut: 'Ctrl+K',
				onclick: createLinkList(showDialog),
				stateSelector: 'a[href]'
			});

			editor.addButton('unlink', {
				icon: 'unlink',
				tooltip: 'Remove link',
				cmd: 'unlink',
				stateSelector: 'a[href]'
			});

			editor.addShortcut('Ctrl+K', '', createLinkList(showDialog));
			this.showDialog = showDialog;

			editor.addMenuItem('link', {
				icon: 'link',
				text: 'Insert link',
				shortcut: 'Ctrl+K',
				onclick: createLinkList(showDialog),
				stateSelector: 'a[href]',
				context: 'insert',
				prependToContext: true
			});

		},

		/**
		 * @ngdoc method
		 * @name umbraco.services.tinyMceService#getAnchorNames
		 * @methodOf umbraco.services.tinyMceService
		 *
		 * @description
		 * From the given string, generates a string array where each item is the id attribute value from a named anchor
		 * 'some string <a id="anchor"></a>with a named anchor' returns ['anchor']
		 *
		 * @param {string} input the string to parse      
		 */
		getAnchorNames: function (input) {
      if (!input) return [];
        
			var anchorPattern = /<a id=\\"(.*?)\\">/gi;
			var matches = input.match(anchorPattern);
			var anchors = [];

			if (matches) {
				anchors = matches.map(function (v) {
					return v.substring(v.indexOf('"') + 1, v.lastIndexOf('\\'));
				});
			}

			return anchors.filter(function(val, i, self) {
          return self.indexOf(val) === i;
      });
		},

		insertLinkInEditor: function (editor, target, anchorElm) {

			var href = target.url;
			// We want to use the Udi. If it is set, we use it, else fallback to id, and finally to null
			var hasUdi = target.udi ? true : false;
			var id = hasUdi ? target.udi : (target.id ? target.id : null);

			// if an anchor exists, check that it is appropriately prefixed
			if (target.anchor && target.anchor[0] !== '?' && target.anchor[0] !== '#') {
				target.anchor = (target.anchor.indexOf('=') === -1 ? '#' : '?') + target.anchor;
			}
 
			// the href might be an external url, so check the value for an anchor/qs
			// href has the anchor re-appended later, hence the reset here to avoid duplicating the anchor
			if (!target.anchor) {
				var urlParts = href.split(/(#|\?)/);
				if (urlParts.length === 3) {
					href = urlParts[0];
					target.anchor = urlParts[1] + urlParts[2];
				}
			}
			
			//Create a json obj used to create the attributes for the tag
			function createElemAttributes() {
				var a = {
					href: href,
					title: target.name,
					target: target.target ? target.target : null,
					rel: target.rel ? target.rel : null
				};

				if (hasUdi) {
					a["data-udi"] = target.udi;
				} else if (target.id) {
					a["data-id"] = target.id;
				}

				if (target.anchor) {
					a["data-anchor"] = target.anchor;
					a.href = a.href + target.anchor;
				} else {
					a["data-anchor"] = null;
				}

				return a;
			}

			function insertLink() {
				if (anchorElm) {
					editor.dom.setAttribs(anchorElm, createElemAttributes());

					editor.selection.select(anchorElm);
					editor.execCommand('mceEndTyping');
				} else {
					editor.execCommand('mceInsertLink', false, createElemAttributes());
				}
			}

			if (!href) {
				editor.execCommand('unlink');
				return;
			}

			//if we have an id, it must be a locallink:id, aslong as the isMedia flag is not set
			if (id && (angular.isUndefined(target.isMedia) || !target.isMedia)) {

				href = "/{localLink:" + id + "}";

				insertLink();
				return;
			}

		    // Is email and not //user@domain.com and protocol (e.g. mailto:, sip:) is not specified
		    if (href.indexOf('@') > 0 && href.indexOf('//') === -1 && href.indexOf(':') === -1) {
		        // assume it's a mailto link
		        href = 'mailto:' + href;
				insertLink();
				return;
			}

			// Is www. prefixed
			if (/^\s*www\./i.test(href)) {
				href = 'http://' + href;
				insertLink();
				return;
			}

			insertLink();

		}

	};
}

angular.module('umbraco.services').factory('tinyMceService', tinyMceService);
