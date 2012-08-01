/**
 * $RCSfile: editor_plugin_src.js,v $
 * $Revision: 1.12 $
 * $Date: 2006/02/22 20:06:23 $
 *
 * @author Moxiecode
 * @copyright Copyright © 2004-2006, Moxiecode Systems AB, All rights reserved.
 */

/* Import plugin specific language pack */
tinyMCE.importPluginLanguagePack('umbracomacro', 'en,tr,he,nb,ru,ru_KOI8-R,ru_UTF-8,nn,fi,cy,es,is,pl'); // <- Add a comma separated list of all supported languages

/****
 * Steps for creating a plugin from this template:
 *
 * 1. Change all "template" to the name of your plugin.
 * 2. Remove all the callbacks in this file that you don't need.
 * 3. Remove the popup.htm file if you don't need any popups.
 * 4. Add your custom logic to the callbacks you needed.
 * 5. Write documentation in a readme.txt file on how to use the plugin.
 * 6. Upload it under the "Plugins" section at sourceforge.
 *
 ****/

// Singleton class
var TinyMCE_UmbracomacroPlugin = {
	/**
	 * Returns information about the plugin as a name/value array.
	 * The current keys are longname, author, authorurl, infourl and version.
	 *
	 * @returns Name/value array containing information about the plugin.
	 * @type Array 
	 */
	getInfo : function() {
		return {
			longname : 'umbracomacro',
			author : 'Your name',
			authorurl : 'http://www.yoursite.com',
			infourl : 'http://www.yoursite.com/docs/template.html',
			version : "1.0"
		};
	},

	/**
	 * Gets executed when a TinyMCE editor instance is initialized.
	 *
	 * @param {TinyMCE_Control} Initialized TinyMCE editor control instance. 
	 */
	initInstance : function(inst) {
		// You can take out plugin specific parameters
		//alert("Initialization parameter:" + tinyMCE.getParam("template_someparam", false));

		// Register custom keyboard shortcut
		//inst.addShortcut('ctrl', 't', 'lang_template_desc', 'mceTemplate');
		if (!tinyMCE.settings['flash_skip_plugin_css'])
			tinyMCE.importCSS(inst.getDoc(), tinyMCE.baseURL + "/plugins/umbracomacro/css/content.css");

	},

	/**
	 * Returns the HTML code for a specific control or empty string if this plugin doesn't have that control.
	 * A control can be a button, select list or any other HTML item to present in the TinyMCE user interface.
	 * The variable {$editor_id} will be replaced with the current editor instance id and {$pluginurl} will be replaced
	 * with the URL of the plugin. Language variables such as {$lang_somekey} will also be replaced with contents from
	 * the language packs.
	 *
	 * @param {string} cn Editor control/button name to get HTML for.
	 * @return HTML code for a specific control or empty string.
	 * @type string
	 */
	getControlHTML : function(cn) {
		switch (cn) {
			case "umbracomacro":
				return tinyMCE.getButtonHTML(cn, 'lang_macro_desc', '{$pluginurl}/images/macro.gif', 'mceUmbracomacro', true);
		}

		return "";
	},

	/**
	 * Executes a specific command, this function handles plugin commands.
	 *
	 * @param {string} editor_id TinyMCE editor instance id that issued the command.
	 * @param {HTMLElement} element Body or root element for the editor instance.
	 * @param {string} command Command name to be executed.
	 * @param {string} user_interface True/false if a user interface should be presented.
	 * @param {mixed} value Custom value argument, can be anything.
	 * @return true/false if the command was executed by this plugin or not.
	 * @type
	 */
	execCommand : function(editor_id, element, command, user_interface, value) {
		// Handle commands
		switch (command) {
			// Remember to have the "mce" prefix for commands so they don't intersect with built in ones in the browser.
			case "mceUmbracomacro":
				// Open a popup window and send in some custom data in a window argument
				var template = new Array();

                /* UMBRACO CHANGE */
				template['file']   = '../../../..' + tinyMCE.settings["umbracoPath"] + '/plugins/tinymce/insertMacro.aspx?editor=true&umbPageId=' + umbPageId + '&umbVersionId=' + umbVersionId;
                /* UMBRACO CHANGE END */
				template['width']  = 600;
				template['height'] = 550;

				// Is selection a Macro
				var inst = tinyMCE.getInstanceById(editor_id);
				var focusElm = inst.getFocusElement();
				
				//
				var macroElement = null;
				
				if (focusElm != null) {
					do {
						if (focusElm.nodeName == "DIV" && tinyMCE.getAttrib(focusElm, 'class').indexOf('umbMacroHolder') >= 0) {
							macroElement = focusElm;
							inst.selection.selectNode(focusElm, true, true);
						}
					} while ((focusElm = focusElm.parentNode));
					
					// is it Edit?
					if (macroElement != null) {
						var el = macroElement;
						template['file']   = '../../../../umbraco/plugins/tinymce/insertMacro.aspx?mode=edit&';
						
						var attrString = "";
						var attrName = "";
						
						
						for (var i=0; i<el.attributes.length;i++) {
							attrName = el.attributes[i].nodeName.toLowerCase();
							if(attrName != "mce_serialized") {
								if (el.attributes[i].nodeValue && (attrName != 'ismacro' && attrName != 'style' && attrName != 'contenteditable')) {
									attrString += el.attributes[i].nodeName + '=' + el.attributes[i].nodeValue.replace(/#/g, "%23").replace(/\</g, "%3C").replace(/\>/g, "%3E").replace(/\"/g, "%22") + '&';
								}
							}
						}	
	
						// vi trunkerer strengen ved at fjerne et evt. overskydende amp;
						if (attrString.length > 0)
							attrString = attrString.substr(0, attrString.length-1);
						
						template['file']   += attrString;
					}				
					// custom macro toolbar command
					else if (value != null && value != '') {
						template['file'] += '&umb_macroAlias=' + value;
					}
					
				}

				tinyMCE.openWindow(template, {editor_id : editor_id, scrollbars : "yes", some_custom_arg : "somecustomdata"});

				// Let TinyMCE know that something was modified
				tinyMCE.triggerNodeChange(false);
				return true;
		}

		// Pass to next handler in chain
		return false;
	},

	/**
	 * Gets called ones the cursor/selection in a TinyMCE instance changes. This is useful to enable/disable
	 * button controls depending on where the user are and what they have selected. This method gets executed
	 * alot and should be as performance tuned as possible.
	 *
	 * @param {string} editor_id TinyMCE editor instance id that was changed.
	 * @param {HTMLNode} node Current node location, where the cursor is in the DOM tree.
	 * @param {int} undo_index The current undo index, if this is -1 custom undo/redo is disabled.
	 * @param {int} undo_levels The current undo levels, if this is -1 custom undo/redo is disabled.
	 * @param {boolean} visual_aid Is visual aids enabled/disabled ex: dotted lines on tables.
	 * @param {boolean} any_selection Is there any selection at all or is there only a cursor.
	 */
	handleNodeChange : function(editor_id, node, undo_index, undo_levels, visual_aid, any_selection) {
		if (node == null)
			return;

		do {
			if (node.nodeName == "DIV" && tinyMCE.getAttrib(node, 'class').indexOf('umbMacroHolder') >= 0) {
				tinyMCE.switchClass(editor_id + '_umbracomacro', 'mceButtonSelected');
				return true;
			}
		} while ((node = node.parentNode));

		tinyMCE.switchClass(editor_id + '_umbracomacro', 'mceButtonNormal');

		return true;
	},

	/**
	 * Gets called when a TinyMCE editor instance gets filled with content on startup.
	 *
	 * @param {string} editor_id TinyMCE editor instance id that was filled with content.
	 * @param {HTMLElement} body HTML body element of editor instance.
	 * @param {HTMLDocument} doc HTML document instance.
	 */
	setupContent : function(editor_id, body, doc) {
	},

	/**
	 * Gets called when the contents of a TinyMCE area is modified, in other words when a undo level is
	 * added.
	 *
	 * @param {TinyMCE_Control} inst TinyMCE editor area control instance that got modified.
	 */
	onChange : function(inst) {
	},

	/**
	 * Gets called when TinyMCE handles events such as keydown, mousedown etc. TinyMCE
	 * doesn't listen on all types of events so custom event handling may be required for
	 * some purposes.
	 *
	 * @param {Event} e HTML editor event reference.
	 * @return true - pass to next handler in chain, false - stop chain execution
	 * @type boolean
	 */
	handleEvent : function(e) {

		return true; // Pass to next handler
	},

	/**
	 * Gets called when HTML contents is inserted or retrived from a TinyMCE editor instance.
	 * The type parameter contains what type of event that was performed and what format the content is in.
	 * Possible valuses for type is get_from_editor, insert_to_editor, get_from_editor_dom, insert_to_editor_dom.
	 *
	 * @param {string} type Cleanup event type.
	 * @param {mixed} content Editor contents that gets inserted/extracted can be a string or DOM element.
	 * @param {TinyMCE_Control} inst TinyMCE editor instance control that performes the cleanup.
	 * @return New content or the input content depending on action.
	 * @type string
	 */
	cleanup : function(type, content, inst) {
		return content;
		switch (type) {
			case "get_from_editor":
				break;
			case "insert_to_editor":
				break;
			case "get_from_editor_dom":
				break;
			case "insert_to_editor_dom":
				break;
		}
		return content;
	},

	// Private plugin internal methods

	/**
	 * This is just a internal plugin method, prefix all internal methods with a _ character.
	 * The prefix is needed so they doesn't collide with future TinyMCE callback functions.
	 *
	 * @param {string} a Some arg1.
	 * @param {string} b Some arg2.
	 * @return Some return.
	 * @type string
	 */
	_someInternalFunction : function(a, b) {
		return 1;
	}
};

// Adds the plugin class to the list of available TinyMCE plugins
tinyMCE.addPlugin("umbracomacro", TinyMCE_UmbracomacroPlugin);
