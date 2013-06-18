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
            { name: "child-of-" + treeItem.name, id: iLevel + "" + 1234, icon: "icon-file-alt", view: section + "/edit/" + iLevel + "" + 1234, children: [], expanded: false, hasChildren: true, level: iLevel, defaultAction: action },
            { name: "random-name-" + section, id: iLevel + "" + 1235, icon: "icon-file-alt", view: section + "/edit/" + iLevel + "" + 1235, children: [], expanded: false, hasChildren: true, level: iLevel, defaultAction: action  },
            { name: "random-name-" + section, id: iLevel + "" + 1236, icon: "icon-file-alt", view: section + "/edit/" + iLevel + "" + 1236, children: [], expanded: false, hasChildren: true, level: iLevel, defaultAction: action  },
            { name: "random-name-" + section, id: iLevel + "" + 1237, icon: "icon-file-alt", view: "common/legacy/1237?p=" + encodeURI("developer/contentType.aspx?idequal1234"), children: [], expanded: false, hasChildren: true, level: iLevel, defaultAction: action  }
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
                    { name: "My website", id: 1234, icon: "icon-home", view: section + "/edit/" + 1234, children: [], expanded: false, hasChildren: true, level: 1, defaultAction: "create" },
                    { name: "Components", id: 1235, icon: "icon-cogs", view: section + "/edit/" + 1235, children: [], expanded: false, hasChildren: true, level: 1, defaultAction: "create"  },
                    { name: "Archieve", id: 1236, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, hasChildren: true, level: 1, defaultAction: "create"  },
                    { name: "Recycle Bin", id: 1237, icon: "icon-trash", view: section + "/trash/view/", children: [], expanded: false, hasChildren: true, level: 1, defaultAction: "create"  }
                ];
            break;

            case "developer":
            t = [
                { name: "Data types", id: 1234, icon: "icon-folder-close", view: section + "/edit/" + 1234, children: [], expanded: false, hasChildren: true, level: 1 },
                { name: "Macros", id: 1235, icon: "icon-folder-close", view: section + "/edit/" + 1235, children: [], expanded: false, hasChildren: true, level: 1 },
                { name: "Pacakges", id: 1236, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, hasChildren: true, level: 1 },
                { name: "XSLT Files", id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1 },
                { name: "Razor Files", id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1 }
                ];
            break;
            case "settings":
            t = [
                { name: "Stylesheets", id: 1234, icon: "icon-folder-close", view: section + "/edit/" + 1234, children: [], expanded: false, hasChildren: true, level: 1 },
                { name: "Templates", id: 1235, icon: "icon-folder-close", view: section + "/edit/" + 1235, children: [], expanded: false, hasChildren: true, level: 1 },
                { name: "Dictionary", id: 1236, icon: "icon-folder-close", view: section + "/edit/" + 1236, children: [], expanded: false, hasChildren: true, level: 1 },
                { name: "Media types", id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1 },
                { name: "Document types", id: 1237, icon: "icon-folder-close", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1 }
                ];
            break;
            default: 
            t = [
                { name: "random-name-" + section, id: 1234, icon: "icon-home", defaultAction: "create", view: section + "/edit/" + 1234, children: [], expanded: false, hasChildren: true, level: 1 },
                { name: "random-name-" + section, id: 1235, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1235, children: [], expanded: false, hasChildren: true, level: 1 },
                { name: "random-name-" + section, id: 1236, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1236, children: [], expanded: false, hasChildren: true, level: 1 },
                { name: "random-name-" + section, id: 1237, icon: "icon-folder-close", defaultAction: "create", view: section + "/edit/" + 1237, children: [], expanded: false, hasChildren: true, level: 1 }
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