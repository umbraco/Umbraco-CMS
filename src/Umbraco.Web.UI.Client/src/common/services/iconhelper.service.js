/**
* @ngdoc service
* @name umbraco.services.iconHelper
* @description A helper service for dealing with icons, mostly dealing with legacy tree icons
**/
function iconHelper($http, $q, $sce, $timeout) {

    const converter = [
        { oldIcon: ".sprNew", newIcon: "add" },
        { oldIcon: ".sprDelete", newIcon: "remove" },
        { oldIcon: ".sprMove", newIcon: "enter" },
        { oldIcon: ".sprCopy", newIcon: "documents" },
        { oldIcon: ".sprSort", newIcon: "navigation-vertical" },
        { oldIcon: ".sprPublish", newIcon: "globe" },
        { oldIcon: ".sprRollback", newIcon: "undo" },
        { oldIcon: ".sprProtect", newIcon: "lock" },
        { oldIcon: ".sprAudit", newIcon: "time" },
        { oldIcon: ".sprNotify", newIcon: "envelope" },
        { oldIcon: ".sprDomain", newIcon: "home" },
        { oldIcon: ".sprPermission", newIcon: "lock" },
        { oldIcon: ".sprRefresh", newIcon: "refresh" },
        { oldIcon: ".sprBinEmpty", newIcon: "trash" },
        { oldIcon: ".sprExportDocumentType", newIcon: "download-alt" },
        { oldIcon: ".sprImportDocumentType", newIcon: "page-up" },
        { oldIcon: ".sprLiveEdit", newIcon: "edit" },
        { oldIcon: ".sprCreateFolder", newIcon: "add" },
        { oldIcon: ".sprPackage2", newIcon: "box" },
        { oldIcon: ".sprLogout", newIcon: "logout" },
        { oldIcon: ".sprSave", newIcon: "save" },
        { oldIcon: ".sprSendToTranslate", newIcon: "envelope-alt" },
        { oldIcon: ".sprToPublish", newIcon: "mail-forward" },
        { oldIcon: ".sprTranslate", newIcon: "comments" },
        { oldIcon: ".sprUpdate", newIcon: "save" },

        { oldIcon: ".sprTreeSettingDomain", newIcon: "icon-home" },
        { oldIcon: ".sprTreeDoc", newIcon: "icon-document" },
        { oldIcon: ".sprTreeDoc2", newIcon: "icon-diploma-alt" },
        { oldIcon: ".sprTreeDoc3", newIcon: "icon-notepad" },
        { oldIcon: ".sprTreeDoc4", newIcon: "icon-newspaper-alt" },
        { oldIcon: ".sprTreeDoc5", newIcon: "icon-notepad-alt" },

        { oldIcon: ".sprTreeDocPic", newIcon: "icon-picture" },
        { oldIcon: ".sprTreeFolder", newIcon: "icon-folder" },
        { oldIcon: ".sprTreeFolder_o", newIcon: "icon-folder" },
        { oldIcon: ".sprTreeMediaFile", newIcon: "icon-music" },
        { oldIcon: ".sprTreeMediaMovie", newIcon: "icon-movie" },
        { oldIcon: ".sprTreeMediaPhoto", newIcon: "icon-picture" },

        { oldIcon: ".sprTreeMember", newIcon: "icon-user" },
        { oldIcon: ".sprTreeMemberGroup", newIcon: "icon-users" },
        { oldIcon: ".sprTreeMemberType", newIcon: "icon-users" },

        { oldIcon: ".sprTreeNewsletter", newIcon: "icon-file-text-alt" },
        { oldIcon: ".sprTreePackage", newIcon: "icon-box" },
        { oldIcon: ".sprTreeRepository", newIcon: "icon-server-alt" },

        { oldIcon: ".sprTreeSettingDataType", newIcon: "icon-autofill" },

        // TODO: Something needs to be done with the old tree icons that are commented out.
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

        { oldIcon: "folder.png", newIcon: "icon-folder" },
        { oldIcon: "mediaphoto.gif", newIcon: "icon-picture" },
        { oldIcon: "mediafile.gif", newIcon: "icon-document" },

        { oldIcon: ".sprTreeDeveloperCacheItem", newIcon: "icon-box" },
        { oldIcon: ".sprTreeDeveloperCacheTypes", newIcon: "icon-box" },
        { oldIcon: ".sprTreeDeveloperMacro", newIcon: "icon-cogs" },
        { oldIcon: ".sprTreeDeveloperRegistry", newIcon: "icon-windows" },
        { oldIcon: ".sprTreeDeveloperPython", newIcon: "icon-linux" }
    ];

    let collectedIcons;

    let imageConverter = [
        {oldImage: "contour.png", newIcon: "icon-umb-contour"}
    ];

    const iconCache = [];
    const promiseQueue = [];
    let resourceLoadStatus = "none";

    /**
     * This is the same approach as use for loading the localized text json
     * We don't want multiple requests for the icon collection, so need to track
     * the current request state, and resolve the queued requests once the icons arrive
     * Subsequent requests are returned immediately as the icons are cached into
     */
    function init() {
        const deferred = $q.defer();

        if (resourceLoadStatus === "loaded") {
            deferred.resolve(iconCache);
            return deferred.promise;
        }

        if (resourceLoadStatus === "loading") {
            promiseQueue.push(deferred);
            return deferred.promise;
        }

        resourceLoadStatus = "loading";

        $http({ method: "GET", url: Umbraco.Sys.ServerVariables.umbracoUrls.iconApiBaseUrl + 'GetIcons' })
            .then(function (response) {
                resourceLoadStatus = "loaded";

                for (const [key, value] of Object.entries(response.data)) {
                    iconCache.push({name: key, svgString: $sce.trustAsHtml(value)})
                }

                deferred.resolve(iconCache);

                //ensure all other queued promises are resolved
                for (let p in promiseQueue) {
                    promiseQueue[p].resolve(iconCache);
                }
            }, function (err) {
                deferred.reject("Something broke");
                //ensure all other queued promises are resolved
                for (let p in promiseQueue) {
                    promiseQueue[p].reject("Something broke");
                }
            });

        return deferred.promise;
    }

    return {

        /** Used by the create dialogs for content/media types to format the data so that the thumbnails are styled properly */
        formatContentTypeThumbnails: function (contentTypes) {
            for (var i = 0; i < contentTypes.length; i++) {

                if (contentTypes[i].thumbnailIsClass === undefined || contentTypes[i].thumbnailIsClass) {
                    contentTypes[i].cssClass = this.convertFromLegacyIcon(contentTypes[i].thumbnail);
                }else {
                    contentTypes[i].style = "background-image: url('" + contentTypes[i].thumbnailFilePath + "');height:36px; background-position:4px 0px; background-repeat: no-repeat;background-size: 35px 35px;";
                    //we need an 'icon-' class in there for certain styles to work so if it is image based we'll add this
                    contentTypes[i].cssClass = "custom-file";
                }
            }
            return contentTypes;
        },
        formatContentTypeIcons: function (contentTypes) {
            for (var i = 0; i < contentTypes.length; i++) {
                if (!contentTypes[i].icon) {
                    //just to be safe (e.g. when focus was on close link and hitting save)
                    contentTypes[i].icon = "icon-document"; // default icon
                } else {
                    contentTypes[i].icon = this.convertFromLegacyIcon(contentTypes[i].icon);
                }

                //couldnt find replacement
                if(contentTypes[i].icon.indexOf(".") > 0){
                     contentTypes[i].icon = "icon-document-dashed-line";
                }
            }
            return contentTypes;
        },
        /** If the icon is file based (i.e. it has a file path) */
        isFileBasedIcon: function (icon) {
            //if it doesn't start with a '.' but contains one then we'll assume it's file based
            if (icon.startsWith('..') || (!icon.startsWith('.') && icon.indexOf('.') > 1)) {
                return true;
            }
            return false;
        },
        /** If the icon is legacy */
        isLegacyIcon: function (icon) {
            if(!icon) {
                return false;
            }

            if(icon.startsWith('..')){
                return false;
            }

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
        },

        /** Gets a single IconModel */
        getIcon: function(iconName) {
            return init().then(icons => icons.find(i => i.name === iconName));
        },

        /** Gets all the available icons in the backoffice icon folder and returns them as an array of IconModels */
         getAllIcons: function() {
            return init().then(icons => icons);
        },

        /** LEGACY - Return a list of icons from icon fonts, optionally filter them */
        /** It fetches them directly from the active stylesheets in the browser */
        getIcons: function(){
            var deferred = $q.defer();
            $timeout(function(){
                if(collectedIcons){
                    deferred.resolve(collectedIcons);
                }else{
                    collectedIcons = [];
                    var c = ".icon-";

                    for (var i = document.styleSheets.length - 1; i >= 0; i--) {
                        var classes = null;
                        try {
                            classes = document.styleSheets[i].rules || document.styleSheets[i].cssRules;
                        } catch (e) {
                            console.warn("Can't read the css rules of: " + document.styleSheets[i].href, e);
                            continue;
                        }

                        if (classes !== null) {
                            for(var x=0;x<classes.length;x++) {
                                var cur = classes[x];
                                if(cur.selectorText && cur.selectorText.indexOf(c) === 0) {
                                    var s = cur.selectorText.substring(1);
                                    var hasSpace = s.indexOf(" ");
                                    if(hasSpace>0){
                                        s = s.substring(0, hasSpace);
                                    }
                                    var hasPseudo = s.indexOf(":");
                                    if(hasPseudo>0){
                                        s = s.substring(0, hasPseudo);
                                    }

                                    if(collectedIcons.indexOf(s) < 0){
                                        collectedIcons.push(s);
                                    }
                                }
                            }
                        }
                    }
                    deferred.resolve(collectedIcons);
                }
            }, 100);

            return deferred.promise;
        },

        /** Creates a icon object, and caches it in a runtime cache */
        defineIcon: function(name, svg) {
            var icon = iconCache.find(x => x.name === name);
            if(icon === undefined) {
                icon = {
                    name: name,
                    svgString: $sce.trustAsHtml(svg)
                };
                iconCache.push(icon);
            }
            return icon;
        },

         /** Returns the cached icon or undefined */
        _getIconFromCache: iconName => iconCache.find(icon => icon.name === iconName)

    };
}
angular.module('umbraco.services').factory('iconHelper', iconHelper);
