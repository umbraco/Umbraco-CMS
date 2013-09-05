/*Contains multiple services for various helper tasks */

/**
 * @ngdoc function
 * @name umbraco.services.legacyJsLoader
 * @function
 *
 * @description
 * Used to lazy load in any JS dependencies that need to be manually loaded in
 */
function legacyJsLoader(assetsService, umbRequestHelper) {
    return {
        
        /** Called to load in the legacy tree js which is required on startup if a user is logged in or 
         after login, but cannot be called until they are authenticated which is why it needs to be lazy loaded. */
        loadLegacyTreeJs: function(scope) {
            return assetsService.loadJs(umbRequestHelper.getApiUrl("legacyTreeJs", "", ""), scope);
        }  
    };
}
angular.module('umbraco.services').factory('legacyJsLoader', legacyJsLoader);

/**
* @ngdoc service
* @name umbraco.services.umbPropertyEditorHelper
* @description A helper object used for property editors
**/
function umbPropEditorHelper() {
    return {
        /**
     * @ngdoc function
     * @name getImagePropertyValue
     * @methodOf umbraco.services.umbPropertyEditorHelper
     * @function    
     *
     * @description
     * Returns the correct view path for a property editor, it will detect if it is a full virtual path but if not then default to the internal umbraco one
     * 
     * @param {string} input the view path currently stored for the property editor
     */
        getViewPath: function (input, isPreValue) {
            var path = String(input);

            if (path.startsWith('/')) {

                //This is an absolute path, so just leave it
                return path;
            }
            else {
                
                if (path.indexOf("/") >= 0) {
                    //This is a relative path, so just leave it
                    return path;
                }
                else {
                    if (!isPreValue) {
                        //i.e. views/propertyeditors/fileupload/fileupload.html
                        return "views/propertyeditors/" + path + "/" + path + ".html";
                    }
                    else {
                        //i.e. views/prevalueeditors/requiredfield.html
                        return "views/prevalueeditors/" + path + ".html";
                    }
                }
                
            }
        }
    };
}
angular.module('umbraco.services').factory('umbPropEditorHelper', umbPropEditorHelper);

/**
* @ngdoc service
* @name umbraco.services.imageHelper
* @description A helper object used for parsing image paths
**/
function imageHelper() {
    return {
        /** Returns the actual image path associated with the image property if there is one */
        getImagePropertyVaue: function(options) {
            if (!options && !options.imageModel) {
                throw "The options objet does not contain the required parameters: imageModel";
            }
            if (options.imageModel.contentTypeAlias.toLowerCase() === "image") {

                //combine all props, TODO: we really need a better way then this
                var props = [];
                if(options.imageModel.properties){
                    props = options.imageModel.properties;
                }else{
                    $(options.imageModel.tabs).each(function(i, tab){
                        props = props.concat(tab.properties);
                    });    
                }

                var imageProp = _.find(props, function (item) {
                    return item.alias === 'umbracoFile';
                });
                var imageVal;

                //our default images might store one or many images (as csv)
                var split = imageProp.value.split(',');
                var self = this;
                imageVal = _.map(split, function(item) {
                    return { file: item, isImage: self.detectIfImageByExtension(item) };
                });
                
                //for now we'll just return the first image in the collection.
                //TODO: we should enable returning many to be displayed in the picker if the uploader supports many.
                if (imageVal.length && imageVal.length > 0 && imageVal[0].isImage) {
                    return imageVal[0].file;
                }
            }
            return "";
        },
        /** formats the display model used to display the content to the model used to save the content */
        getThumbnail: function (options) {
            
            if (!options && !options.imageModel) {
                throw "The options objet does not contain the required parameters: imageModel";
            }

            var imagePropVal = this.getImagePropertyVaue(options);
            if (imagePropVal !== "") {
                return this.getThumbnailFromPath(imagePropVal);
            }
            return "";
        },

        scaleToMaxSize: function(maxSize, width, height){
            var retval = {width: width, height: height};

            var maxWidth = maxSize; // Max width for the image
            var maxHeight = maxSize;    // Max height for the image
            var ratio = 0;  // Used for aspect ratio
           
            // Check if the current width is larger than the max
            if(width > maxWidth){
                ratio = maxWidth / width;   // get ratio for scaling image
                
                retval.width = maxWidth;
                retval.height = height * ratio;

                height = height * ratio;    // Reset height to match scaled image
                width = width * ratio;    // Reset width to match scaled image
            }

            // Check if current height is larger than max
            if(height > maxHeight){
                ratio = maxHeight / height; // get ratio for scaling image

                retval.height = maxHeight;
                retval.width = width * ratio;
                width = width * ratio;    // Reset width to match scaled image
            }

            return retval;
        },

        getThumbnailFromPath: function(imagePath) {
            var ext = imagePath.substr(imagePath.lastIndexOf('.'));
            return imagePath.substr(0, imagePath.lastIndexOf('.')) + "_thumb" + ".jpg";
        },
        detectIfImageByExtension: function(imagePath) {
            var lowered = imagePath.toLowerCase();
            var ext = lowered.substr(lowered.lastIndexOf(".") + 1);
            return ("," + Umbraco.Sys.ServerVariables.umbracoSettings.imageFileTypes + ",").indexOf("," + ext + ",") !== -1;
        }
    };
}
angular.module('umbraco.services').factory('imageHelper', imageHelper);

/**
* @ngdoc service
* @name umbraco.services.umbDataFormatter
* @description A helper object used to format/transform JSON Umbraco data, mostly used for persisting data to the server
**/
function umbDataFormatter() {
    return {
        
        /** formats the display model used to display the data type to the model used to save the data type */
        formatDataTypePostData: function(displayModel, preValues, action) {
            var saveModel = {
                parentId: -1,
                id: displayModel.id,
                name: displayModel.name,
                selectedEditor: displayModel.selectedEditor,
                //set the action on the save model
                action: action,
                preValues: []
            };
            for (var i = 0; i < preValues.length; i++) {

                saveModel.preValues.push({
                    key: preValues[i].alias,
                    value: preValues[i].value
                });
            }
            return saveModel;
        },

        /** formats the display model used to display the content to the model used to save the content */
        formatContentPostData: function (displayModel, action) {
            //NOTE: the display model inherits from the save model so we can in theory just post up the display model but 
            // we don't want to post all of the data as it is unecessary.
            var saveModel = {
                id: displayModel.id,
                properties: [],
                name: displayModel.name,
                contentTypeAlias : displayModel.contentTypeAlias,
                parentId: displayModel.parentId,
                //set the action on the save model
                action: action
            };
            
            _.each(displayModel.tabs, function (tab) {
                
                _.each(tab.properties, function (prop) {
                    
                    //don't include the custom generic tab properties
                    if (!prop.alias.startsWith("_umb_")) {
                        saveModel.properties.push({
                            id: prop.id,
                            alias: prop.alias,
                            value: prop.value
                        });
                    }
                    else {
                        //here we need to map some of our internal properties to the content save item

                        switch (prop.alias) {
                            case "_umb_expiredate":
                                saveModel.expireDate = prop.value;
                                break;
                            case "_umb_releasedate":
                                saveModel.releaseDate = prop.value;
                                break;
                            case "_umb_template":
                                saveModel.templateAlias = prop.value;
                                break;
                        }
                    }
                    
                });
            });

            return saveModel;
        }
    };
}
angular.module('umbraco.services').factory('umbDataFormatter', umbDataFormatter);

/**
* @ngdoc service
* @name umbraco.services.iconHelper
* @description A helper service for dealing with icons, mostly dealing with legacy tree icons
**/
function iconHelper() {

    var converter = [
        { oldIcon: ".sprNew", newIcon: "plus" },
        { oldIcon: ".sprDelete", newIcon: "remove" },
        { oldIcon: ".sprMove", newIcon: "move" },
        { oldIcon: ".sprCopy", newIcon: "copy" },
        { oldIcon: ".sprSort", newIcon: "sort" },
        { oldIcon: ".sprPublish", newIcon: "globe" },
        { oldIcon: ".sprRollback", newIcon: "undo" },
        { oldIcon: ".sprProtect", newIcon: "lock" },
        { oldIcon: ".sprAudit", newIcon: "time" },
        { oldIcon: ".sprNotify", newIcon: "envelope" },
        { oldIcon: ".sprDomain", newIcon: "home" },
        { oldIcon: ".sprPermission", newIcon: "group" },
        { oldIcon: ".sprRefresh", newIcon: "refresh" },
        { oldIcon: ".sprBinEmpty", newIcon: "trash" },
        { oldIcon: ".sprExportDocumentType", newIcon: "download-alt" },
        { oldIcon: ".sprImportDocumentType", newIcon: "upload-alt" },
        { oldIcon: ".sprLiveEdit", newIcon: "edit" },
        { oldIcon: ".sprCreateFolder", newIcon: "plus-sign-alt" },
        { oldIcon: ".sprPackage2", newIcon: "gift" },
        { oldIcon: ".sprLogout", newIcon: "signout" },
        { oldIcon: ".sprSave", newIcon: "save" },
        { oldIcon: ".sprSendToTranslate", newIcon: "envelope-alt" },
        { oldIcon: ".sprToPublish", newIcon: "mail-forward" },
        { oldIcon: ".sprTranslate", newIcon: "comments" },
        { oldIcon: ".sprUpdate", newIcon: "save" },
        
        { oldIcon: ".sprTreeSettingDomain", newIcon: "icon-home" },
        { oldIcon: ".sprTreeDoc", newIcon: "icon-file-alt" },
        { oldIcon: ".sprTreeDoc2", newIcon: "icon-file" },
        { oldIcon: ".sprTreeDoc3", newIcon: "icon-file-text" },
        { oldIcon: ".sprTreeDoc4", newIcon: "icon-file-text-alt" },
        { oldIcon: ".sprTreeDoc5", newIcon: "icon-book" },        
        { oldIcon: ".sprTreeDocPic", newIcon: "icon-picture" },        
        { oldIcon: ".sprTreeFolder", newIcon: "icon-folder" },
        { oldIcon: ".sprTreeFolder_o", newIcon: "icon-folder-open" },
        { oldIcon: ".sprTreeMediaFile", newIcon: "icon-music" },
        { oldIcon: ".sprTreeMediaMovie", newIcon: "icon-movie" },
        { oldIcon: ".sprTreeMediaPhoto", newIcon: "icon-picture" },
        
        { oldIcon: ".sprTreeMember", newIcon: "icon-mail" },
        { oldIcon: ".sprTreeMemberGroup", newIcon: "icon-group" },
        { oldIcon: ".sprTreeMemberType", newIcon: "icon-group" },
        
        { oldIcon: ".sprTreeNewsletter", newIcon: "icon-file-text-alt" },
        { oldIcon: ".sprTreePackage", newIcon: "icon-dropbox" },
        { oldIcon: ".sprTreeRepository", newIcon: "icon-github" },
        
        { oldIcon: ".sprTreeSettingDataType", newIcon: "icon-tasks" },

        //TODO:
        /*
        { oldIcon: ".sprTreeSettingAgent", newIcon: "" },
        { oldIcon: ".sprTreeSettingCss", newIcon: "" },
        { oldIcon: ".sprTreeSettingCssItem", newIcon: "" },
        
        { oldIcon: ".sprTreeSettingDataTypeChild", newIcon: "" },
        { oldIcon: ".sprTreeSettingDomain", newIcon: "" },
        { oldIcon: ".sprTreeSettingLanguage", newIcon: "" },
        { oldIcon: ".sprTreeSettingScript", newIcon: "" },
        { oldIcon: ".sprTreeSettingTemplate", newIcon: "" },
        { oldIcon: ".sprTreeSettingXml", newIcon: "" },
        { oldIcon: ".sprTreeStatistik", newIcon: "" },
        { oldIcon: ".sprTreeUser", newIcon: "" },
        { oldIcon: ".sprTreeUserGroup", newIcon: "" },
        { oldIcon: ".sprTreeUserType", newIcon: "" },
        */


        { oldIcon: ".sprTreeDeveloperCacheItem", newIcon: "icon-box" },
        { oldIcon: ".sprTreeDeveloperCacheTypes", newIcon: "icon-box" },
        { oldIcon: ".sprTreeDeveloperMacro", newIcon: "icon-cogs" },
        { oldIcon: ".sprTreeDeveloperRegistry", newIcon: "icon-windows" },
        { oldIcon: ".sprTreeDeveloperPython", newIcon: "icon-linux" },
        { oldIcon: "images/tray/contour.png", newIcon: "icon-umb-contour" }
    ];

    var imageConverter = [
            {oldImage: "contour.png", newIcon: "icon-umb-contour"}
            ];

    return {
        
        /** Used by the create dialogs for content/media types to format the data so that the thumbnails are styled properly */
        formatContentTypeThumbnails: function (contentTypes) {
            for (var i = 0; i < contentTypes.length; i++) {
                if (contentTypes[i].thumbnailIsClass === undefined || contentTypes[i].thumbnailIsClass) {
                    contentTypes[i].cssClass = this.convertFromLegacyIcon(contentTypes[i].thumbnail);
                }
                else {
                    contentTypes[i].style = "background-image: url('" + contentTypes[i].thumbnailFilePath + "');height:36px; background-position:4px 0px; background-repeat: no-repeat;background-size: 35px 35px;";
                    //we need an 'icon-' class in there for certain styles to work so if it is image based we'll add this
                    contentTypes[i].cssClass = "custom-file";
                }
            }
            return contentTypes;
        },
        formatContentTypeIcons: function (contentTypes) {
            for (var i = 0; i < contentTypes.length; i++) {
                contentTypes[i].icon = this.convertFromLegacyIcon(contentTypes[i].icon);
            }
            return contentTypes;
        },
        /** If the icon is file based (i.e. it has a file path) */
        isFileBasedIcon: function (icon) {
            //if it doesn't start with a '.' but contains one then we'll assume it's file based
            if (!icon.startsWith('.') && icon.indexOf('.') > 1) {
                return true;
            }
            return false;
        },
        /** If the icon is legacy */
        isLegacyIcon: function (icon) {
            if (icon.startsWith('.')) {
                return true;
            }
            return false;
        },
        /** If the tree node has a legacy icon */
        isLegacyTreeNodeIcon: function(treeNode){
            if (treeNode.iconIsClass) {
                return this.isLegacyIcon(treeNode.icon);
            }
            return false;
        },

        /** Return a list of icons, optionally filter them */
        /** It fetches them directly from the active stylesheet in the browser */
        getIcons: function(filter){

            var classes = document.styleSheets[0].rules || document.styleSheets[0].cssRules;
            var result = [];
            var f = filter || "";
            var c = ".icon-" + f;

            for(var x=0;x<classes.length;x++) {
                var cur = classes[x];
                if(cur.selectorText && cur.selectorText.indexOf(c) === 0 && cur.selectorText.indexOf("before") > 0 && cur.selectorText.indexOf(",") < 0) {
                    var s = cur.selectorText;
                    s = cur.selectorText.substring(1, s.indexOf(":"));
                    
                    if(result.indexOf(s) < 0){
                        result.push(s);   
                    }
                }
            }

            return result;
        },

        /** Converts the icon from legacy to a new one if an old one is detected */
        convertFromLegacyIcon: function (icon) {
            if (this.isLegacyIcon(icon)) {
                //its legacy so convert it if we can
                var found = _.find(converter, function (item) {
                    return item.oldIcon.toLowerCase() === icon.toLowerCase();
                });
                return (found ? found.newIcon : icon);
            }
            return icon;
        },

        convertFromLegacyImage: function (icon) {
                var found = _.find(imageConverter, function (item) {
                    return item.oldImage.toLowerCase() === icon.toLowerCase();
                });
                return (found ? found.newIcon : undefined);
        },

        /** If we detect that the tree node has legacy icons that can be converted, this will convert them */
        convertFromLegacyTreeNodeIcon: function (treeNode) {
            if (this.isLegacyTreeNodeIcon(treeNode)) {
                return this.convertFromLegacyIcon(treeNode.icon);
            }
            return treeNode.icon;
        }
    };
}
angular.module('umbraco.services').factory('iconHelper', iconHelper);




/**
 * @ngdoc service
 * @name umbraco.services.xmlhelper
 * @function
 *
 * @description
 * Used to convert legacy xml data to json and back again
 */
function xmlhelper($http) {
    /*
     Copyright 2011 Abdulla Abdurakhmanov
     Original sources are available at https://code.google.com/p/x2js/

     Licensed under the Apache License, Version 2.0 (the "License");
     you may not use this file except in compliance with the License.
     You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

     Unless required by applicable law or agreed to in writing, software
     distributed under the License is distributed on an "AS IS" BASIS,
     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     See the License for the specific language governing permissions and
     limitations under the License.
     */

    function X2JS() {
            var VERSION = "1.0.11";
            var escapeMode = false;

            var DOMNodeTypes = {
                    ELEMENT_NODE       : 1,
                    TEXT_NODE          : 3,
                    CDATA_SECTION_NODE : 4,
                    DOCUMENT_NODE      : 9
            };
            
            function getNodeLocalName( node ) {
                    var nodeLocalName = node.localName;                     
                    if(nodeLocalName == null){
                        nodeLocalName = node.baseName;
                    } // Yeah, this is IE!! 
                            
                    if(nodeLocalName === null || nodeLocalName===""){
                        nodeLocalName = node.nodeName;
                    } // =="" is IE too
                            
                    return nodeLocalName;
            }
            
            function getNodePrefix(node) {
                    return node.prefix;
            }
                    
            function escapeXmlChars(str) {
                    if(typeof(str) === "string"){
                            return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#x27;').replace(/\//g, '&#x2F;');
                    }else{
                        return str;
                    }
            }

            function unescapeXmlChars(str) {
                    return str.replace(/&amp;/g, '&').replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&quot;/g, '"').replace(/&#x27;/g, "'").replace(/&#x2F;/g, '\/');
            }      

            function parseDOMChildren( node ) {
                    var result,child, childName;

                    if(node.nodeType === DOMNodeTypes.DOCUMENT_NODE) {
                            result = {};
                            child = node.firstChild;
                            childName = getNodeLocalName(child);
                            result[childName] = parseDOMChildren(child);
                            return result;
                    }
                    else{

                    if(node.nodeType === DOMNodeTypes.ELEMENT_NODE) {
                            result = {};
                            result.__cnt=0;
                            var nodeChildren = node.childNodes;
                            
                            // Children nodes
                            for(var cidx=0; cidx <nodeChildren.length; cidx++) {
                                    child = nodeChildren.item(cidx); // nodeChildren[cidx];
                                    childName = getNodeLocalName(child);
                                    
                                    result.__cnt++;
                                    if(result[childName] === null) {
                                            result[childName] = parseDOMChildren(child);
                                            result[childName+"_asArray"] = new Array(1);
                                            result[childName+"_asArray"][0] = result[childName];
                                    }
                                    else {
                                            if(result[childName] !== null) {
                                                    if( !(result[childName] instanceof Array)) {
                                                            var tmpObj = result[childName];
                                                            result[childName] = [];
                                                            result[childName][0] = tmpObj;
                                                            
                                                            result[childName+"_asArray"] = result[childName];
                                                    }
                                            }
                                            var aridx = 0;
                                            while(result[childName][aridx]!==null){
                                                aridx++;
                                            } 

                                            (result[childName])[aridx] = parseDOMChildren(child);
                                    }                       
                            }
                            
                            // Attributes
                            for(var aidx=0; aidx <node.attributes.length; aidx++) {
                                    var attr = node.attributes.item(aidx); // [aidx];
                                    result.__cnt++;
                                    result["_"+attr.name]=attr.value;
                            }
                            
                            // Node namespace prefix
                            var nodePrefix = getNodePrefix(node);
                            if(nodePrefix!==null && nodePrefix!=="") {
                                    result.__cnt++;
                                    result.__prefix=nodePrefix;
                            }
                            
                            if( result.__cnt === 1 && result["#text"]!==null  ) {
                                    result = result["#text"];
                            }
                            
                            if(result["#text"]!==null) {
                                    result.__text = result["#text"];
                                    if(escapeMode){
                                        result.__text = unescapeXmlChars(result.__text);
                                    }
                                            
                                    delete result["#text"];
                                    delete result["#text_asArray"];
                            }
                            if(result["#cdata-section"]!=null) {
                                    result.__cdata = result["#cdata-section"];
                                    delete result["#cdata-section"];
                                    delete result["#cdata-section_asArray"];
                            }
                            
                            if(result.__text!=null || result.__cdata!=null) {
                                    result.toString = function() {
                                            return (this.__text!=null? this.__text:'')+( this.__cdata!=null ? this.__cdata:'');
                                    };
                            }
                            return result;
                    }
                    else{
                        if(node.nodeType === DOMNodeTypes.TEXT_NODE || node.nodeType === DOMNodeTypes.CDATA_SECTION_NODE) {
                                return node.nodeValue;
                        } 
                    }
                }     
            }
            
            function startTag(jsonObj, element, attrList, closed) {
                    var resultStr = "<"+ ( (jsonObj!=null && jsonObj.__prefix!=null)? (jsonObj.__prefix+":"):"") + element;
                    if(attrList!=null) {
                            for(var aidx = 0; aidx < attrList.length; aidx++) {
                                    var attrName = attrList[aidx];
                                    var attrVal = jsonObj[attrName];
                                    resultStr+=" "+attrName.substr(1)+"='"+attrVal+"'";
                            }
                    }
                    if(!closed){
                        resultStr+=">";
                    }else{
                        resultStr+="/>";
                    }
                            
                    return resultStr;
            }
            
            function endTag(jsonObj,elementName) {
                    return "</"+ (jsonObj.__prefix!==null? (jsonObj.__prefix+":"):"")+elementName+">";
            }
            
            function endsWith(str, suffix) {
                return str.indexOf(suffix, str.length - suffix.length) !== -1;
            }
            
            function jsonXmlSpecialElem ( jsonObj, jsonObjField ) {
                    if(endsWith(jsonObjField.toString(),("_asArray")) || jsonObjField.toString().indexOf("_")===0 || (jsonObj[jsonObjField] instanceof Function) ){
                        return true;
                    }else{
                        return false;
                    }
            }
            
            function jsonXmlElemCount ( jsonObj ) {
                    var elementsCnt = 0;
                    if(jsonObj instanceof Object ) {
                            for( var it in jsonObj  ) {
                                    if(jsonXmlSpecialElem ( jsonObj, it) ){
                                        continue;
                                    }                            
                                    elementsCnt++;
                            }
                    }
                    return elementsCnt;
            }
            
            function parseJSONAttributes ( jsonObj ) {
                    var attrList = [];
                    if(jsonObj instanceof Object ) {
                            for( var ait in jsonObj  ) {
                                    if(ait.toString().indexOf("__")=== -1 && ait.toString().indexOf("_")===0) {
                                            attrList.push(ait);
                                    }
                            }
                    }

                    return attrList;
            }
            
            function parseJSONTextAttrs ( jsonTxtObj ) {
                    var result ="";
                    
                    if(jsonTxtObj.__cdata!=null) {                                                                          
                            result+="<![CDATA["+jsonTxtObj.__cdata+"]]>";                                   
                    }
                    
                    if(jsonTxtObj.__text!=null) {                   
                            if(escapeMode){
                               result+=escapeXmlChars(jsonTxtObj.__text);     
                            }else{
                                result+=jsonTxtObj.__text;
                            } 
                    }
                    return result;
            }
            
            function parseJSONTextObject ( jsonTxtObj ) {
                    var result ="";

                    if( jsonTxtObj instanceof Object ) {
                            result+=parseJSONTextAttrs ( jsonTxtObj );
                    }
                    else{
                        if(jsonTxtObj!=null) {
                                if(escapeMode){
                                   result+=escapeXmlChars(jsonTxtObj);     
                                }else{
                                    result+=jsonTxtObj;
                                }
                        }
                    }
                            
                    
                    return result;
            }
            
            function parseJSONArray ( jsonArrRoot, jsonArrObj, attrList ) {
                    var result = ""; 
                    if(jsonArrRoot.length === 0) {
                            result+=startTag(jsonArrRoot, jsonArrObj, attrList, true);
                    }
                    else {
                            for(var arIdx = 0; arIdx < jsonArrRoot.length; arIdx++) {
                                    result+=startTag(jsonArrRoot[arIdx], jsonArrObj, parseJSONAttributes(jsonArrRoot[arIdx]), false);
                                    result+=parseJSONObject(jsonArrRoot[arIdx]);
                                    result+=endTag(jsonArrRoot[arIdx],jsonArrObj);                                          
                            }
                    }
                    return result;
            }
            
            function parseJSONObject ( jsonObj ) {
                    var result = "";        

                    var elementsCnt = jsonXmlElemCount ( jsonObj );
                    
                    if(elementsCnt > 0) {
                            for( var it in jsonObj ) {
                                if(jsonXmlSpecialElem ( jsonObj, it) ){
                                    continue;
                                }                            
                                
                                var subObj = jsonObj[it];                                               
                                var attrList = parseJSONAttributes( subObj );
                                
                                if(subObj === null || subObj === undefined) {
                                        result+=startTag(subObj, it, attrList, true);
                                }else{
                                    if(subObj instanceof Object) {
                                            
                                            if(subObj instanceof Array) {                                   
                                                    result+=parseJSONArray( subObj, it, attrList );
                                            }else {
                                                    var subObjElementsCnt = jsonXmlElemCount ( subObj );
                                                    if(subObjElementsCnt > 0 || subObj.__text!==null || subObj.__cdata!==null) {
                                                            result+=startTag(subObj, it, attrList, false);
                                                            result+=parseJSONObject(subObj);
                                                            result+=endTag(subObj,it);
                                                    }else{
                                                            result+=startTag(subObj, it, attrList, true);
                                                    }
                                            }

                                    }else {
                                            result+=startTag(subObj, it, attrList, false);
                                            result+=parseJSONTextObject(subObj);
                                            result+=endTag(subObj,it);
                                    }
                                }
                            }
                    }
                    result+=parseJSONTextObject(jsonObj);
                    
                    return result;
            }
            
            this.parseXmlString = function(xmlDocStr) {
                    var xmlDoc;
                    if (window.DOMParser) {
                            var parser=new window.DOMParser();
                            xmlDoc = parser.parseFromString( xmlDocStr, "text/xml" );
                    }
                    else {
                            // IE :(
                            if(xmlDocStr.indexOf("<?")===0) {
                                    xmlDocStr = xmlDocStr.substr( xmlDocStr.indexOf("?>") + 2 );
                            }
                            xmlDoc=new ActiveXObject("Microsoft.XMLDOM");
                            xmlDoc.async="false";
                            xmlDoc.loadXML(xmlDocStr);
                    }
                    return xmlDoc;
            };

            this.xml2json = function (xmlDoc) {
                    return parseDOMChildren ( xmlDoc );
            };
            
            this.xml_str2json = function (xmlDocStr) {
                    var xmlDoc = this.parseXmlString(xmlDocStr);    
                    return this.xml2json(xmlDoc);
            };

            this.json2xml_str = function (jsonObj) {
                    return parseJSONObject ( jsonObj );
            };

            this.json2xml = function (jsonObj) {
                    var xmlDocStr = this.json2xml_str (jsonObj);
                    return this.parseXmlString(xmlDocStr);
            };

            this.getVersion = function () {
                    return VERSION;
            };

            this.escapeMode = function(enabled) {
                    escapeMode = enabled;
            };
    }

    var x2js = new X2JS();
    return {
        /** Called to load in the legacy tree js which is required on startup if a user is logged in or 
         after login, but cannot be called until they are authenticated which is why it needs to be lazy loaded. */
        toJson: function(xml) {
            var json = x2js.xml_str2json( xml );
            return json;
        },
        fromJson: function(json) {
            var xml = x2js.json2xml_str( json );
            return xml;
        },
        parseFeed: function (url) {             
            return $http.jsonp('//ajax.googleapis.com/ajax/services/feed/load?v=1.0&num=50&callback=JSON_CALLBACK&q=' + encodeURIComponent(url));         
        }
    };
}
angular.module('umbraco.services').factory('xmlhelper', xmlhelper);
