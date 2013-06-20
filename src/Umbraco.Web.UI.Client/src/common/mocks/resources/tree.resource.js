/**
* @ngdoc factory 
* @name umbraco.resources.treeResource     
* @description Loads in data for trees
**/
function treeResource($q) {

    function _getChildren(options){
        if(options === undefined){
            options = {};
        }
        var section = options.section || 'content';
        var treeItem = options.node;

        var iLevel = treeItem.level + 1;

        //hack to have create as default content action
        var action;
        if(section === "content"){
            action = "create";
        }

        return [
            { name: "child-of-" + treeItem.name, id: iLevel + "" + 1234, icon: "icon-file-alt", view: section + "/edit/" + iLevel + "" + 1234, children: [], expanded: false, hasChildren: true, level: iLevel, defaultAction: action, menu: getMenuItems() },
            { name: "random-name-" + section, id: iLevel + "" + 1235, icon: "icon-file-alt", view: section + "/edit/" + iLevel + "" + 1235, children: [], expanded: false, hasChildren: true, level: iLevel, defaultAction: action, menu: getMenuItems() },
            { name: "random-name-" + section, id: iLevel + "" + 1236, icon: "icon-file-alt", view: section + "/edit/" + iLevel + "" + 1236, children: [], expanded: false, hasChildren: true, level: iLevel, defaultAction: action, menu: getMenuItems() },
            { name: "random-name-" + section, id: iLevel + "" + 1237, icon: "icon-file-alt", view: "common/legacy/1237?p=" + encodeURI("developer/contentType.aspx?idequal1234"), children: [], expanded: false, hasChildren: true, level: iLevel, defaultAction: action, menu: getMenuItems() }
        ];
    }
    
    function getMenuItems() {
        return [
        { name: "Create", cssclass: "plus", alias: "create" },

        { seperator: true, name: "Delete", cssclass: "remove", alias: "delete" },
        { name: "Move", cssclass: "move",  alias: "move" },
        { name: "Copy", cssclass: "copy", alias: "copy" },
        { name: "Sort", cssclass: "sort", alias: "sort" },

        { seperator: true, name: "Publish", cssclass: "globe", alias: "publish" },
        { name: "Rollback", cssclass: "undo", alias: "rollback" },

        { seperator: true, name: "Permissions", cssclass: "lock", alias: "permissions" },
        { name: "Audit Trail", cssclass: "time", alias: "audittrail" },
        { name: "Notifications", cssclass: "envelope", alias: "notifications" },

        { seperator: true, name: "Hostnames", cssclass: "home", alias: "hostnames" },
        { name: "Public Access", cssclass: "group", alias: "publicaccess" },

        { seperator: true, name: "Reload", cssclass: "refresh", alias: "users" }
        ];
    }

    var treeArray = [];
    function _getApplication(options){
        if(options === undefined){
            options = {};
        }

        var section = options.section || 'content';
        var cacheKey = options.cachekey || '';
        cacheKey += "_" + section; 

        if (treeArray[cacheKey] !== undefined){
            return treeArray[cacheKey];
        }
        
        var t;
        switch(section){

            case "content":
            t = [
                    { name: "My website", id: 1234, icon: "icon-home", view: section + "/edit/" + 1234, children: [], expanded: false, hasChildren: true, level: 1, defaultAction: "create", menu: getMenuItems() },
                    { name: "Components", id: 1235, icon: "icon-cogs", view: section + "/edit/" + 1235, children: [], expanded: false, hasChildren: true, level: 1, defaultAction: "create", menu: getMenuItems() },
                    { name: "Archieve", id: 1236, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, hasChildren: true, level: 1, defaultAction: "create", menu: getMenuItems() },
                    { name: "Recycle Bin", id: 1237, icon: "icon-trash", view: section + "/trash/view/", children: [], expanded: false, hasChildren: true, level: 1, defaultAction: "create", menu: getMenuItems() }
                ];
            break;

            case "developer":
            t = [
                { name: "Data types", id: 1234, icon: "icon-folder-close", view: section + "/edit/" + 1234, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                { name: "Macros", id: 1235, icon: "icon-folder-close", view: section + "/edit/" + 1235, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                { name: "Pacakges", id: 1236, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                { name: "XSLT Files", id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                { name: "Razor Files", id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() }
                ];
            break;
            case "settings":
            t = [
                { name: "Stylesheets", id: 1234, icon: "icon-folder-close", view: section + "/edit/" + 1234, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                { name: "Templates", id: 1235, icon: "icon-folder-close", view: section + "/edit/" + 1235, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                { name: "Dictionary", id: 1236, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                { name: "Media types", id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                { name: "Document types", id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() }
                ];
            break;
            default: 
            t = [
                { name: "random-name-" + section, id: 1234, icon: "icon-home", defaultAction: "create", view: section + "/edit/" + 1234, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                { name: "random-name-" + section, id: 1235, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1235, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                { name: "random-name-" + section, id: 1236, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1236, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() },
                { name: "random-name-" + section, id: 1237, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1, menu: getMenuItems() }
                ];
            break;
        }               

        treeArray[cacheKey] = t;
        return treeArray[cacheKey];
    }


    //the factory object returned
    return {
        /** Loads in the data to display the nodes for an application */
        loadApplication: function (options) {
            var deferred = $q.defer();
            deferred.resolve(_getApplication(options));
            return deferred.promise;
        },

        /** Loads in the data to display the child nodes for a given node */
        loadNodes: function (options) {

            var deferred = $q.defer();
            var data = _getChildren(options);
            deferred.resolve(data);
            return deferred.promise;
        }
    };
}

angular.module('umbraco.mocks.resources').factory('treeResource', treeResource);