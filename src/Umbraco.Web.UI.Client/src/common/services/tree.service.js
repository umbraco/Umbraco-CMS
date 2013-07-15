
/**
 * @ngdoc service
 * @name umbraco.services.treeService
 * @function
 *
 * @description
 * The tree service factory, used internally by the umbTree and umbTreeItem directives
 */
function treeService($q, treeResource, iconHelper) {
    //implement this in local storage
    var treeArray = [];
    var currentSection = "content";

    /** ensures there's a view and level property on each tree node */
    function ensureParentLevelAndView(parentNode, treeNodes, section, level) {
        //if no level is set, then we make it 1   
        var childLevel = (level ? level : 1);
        for (var i = 0; i < treeNodes.length; i++) {
            treeNodes[i].level = childLevel;
            treeNodes[i].view = section + "/edit/" + treeNodes[i].id;
            treeNodes[i].parent = parentNode;
        }
    }

    return {
        
        removeNode: function(treeNode) {
            var asdf = treeNode;
        },

        getTree: function (options) {

            if(options === undefined){
                options = {};
            }

            var section = options.section || 'content';
            var cacheKey = options.cachekey || '';
            cacheKey += "_" + section;	

            //return the cache if it exists
            if (treeArray[cacheKey] !== undefined){
                return treeArray[cacheKey];
            }
             
            return treeResource.loadApplication(options)
                .then(function(data) {
                    //this will be called once the tree app data has loaded
                    var result = {
                        name: section,
                        alias: section,
                        root: data
                    };
                    //ensure the view is added to each tree node
                    ensureParentLevelAndView(result.root, result.root.children, section);
                    //cache this result
                    //TODO: We'll need to un-cache this in many circumstances
                    treeArray[cacheKey] = result;
                    //return the data result as promised
                    //deferred.resolve(treeArray[cacheKey]);
                    return treeArray[cacheKey];
                });
        },

        getMenu: function (args) {

            if (!args) {
                throw "args cannot be null";
            }
            if (!args.treeNode) {
                throw "args.treeNode cannot be null";
            }

            return treeResource.loadMenu(args.treeNode)
                .then(function(data) {
                    //need to convert the icons to new ones
                    for (var i = 0; i < data.length; i++) {
                        data[i].cssclass = iconHelper.convertFromLegacyIcon(data[i].cssclass);
                    }
                    return data;
                });
        },
        
        /**
         * @ngdoc method
         * @name umbraco.services.treeService#getMenuItemByAlias
         * @methodOf umbraco.services.treeService
         * @function
         *
         * @description
         * Attempts to return a tree node's menu item based on the alias supplied, otherwise returns null.

         * @param {object} args An arguments object
         * @param {object} args.treeNode The tree node to get the menu item for
         * @param {object} args.menuItemAlias The menu item alias to attempt to find
         */
        getMenuItemByAlias: function (args) {

            if (!args) {
                throw "args cannot be null";
            }
            if (!args.treeNode) {
                throw "args.treeNode cannot be null";                
            }
            if (!args.menuItemAlias) {
                throw "args.menuItemAlias cannot be null";
            }

            return this.getMenu(args)
                .then(function (menuItems) {
                    //try to find the node with the alias
                    return _.find(menuItems, function(item) {
                        return item.alias === args.menuItemAlias;
                    });
                });
        },
        
        getChildren: function (options) {

            if(options === undefined){
                throw "No options object defined for getChildren";
            }
            if (options.node === undefined) {
                throw "No node defined on options object for getChildren";
            }

            var section = options.section || 'content';
            var treeItem = options.node;

            //hack to have create as default content action
            var action;
            if(section === "content"){
                action = "create";
            }

            if (!options.node) {
                throw "No node defined";
            }

            return treeResource.loadNodes({ section: section, node: treeItem })
                .then(function(data) {
                    //now that we have the data, we need to add the level property to each item and the view
                    ensureParentLevelAndView(treeItem, data, section, treeItem.level + 1);
                    return data;
                });
        }
    };
}

angular.module('umbraco.services').factory('treeService', treeService);