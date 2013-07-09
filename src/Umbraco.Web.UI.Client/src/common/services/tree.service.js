
/**
 * @ngdoc factory
 * @name treeService
 * @function
 *
 * @description
 * The tree service factory

 * @param myParam {object} Enter param description here
 */
function treeService($q, treeResource, iconHelper) {
    //implement this in local storage
    var treeArray = [];
    var currentSection = "content";

    /** ensures there's a view and level property on each tree node */
    function ensureLevelAndView(treeNodes, section, level) {
        //if no level is set, then we make it 1   
        var childLevel = (level ? level : 1);
        for (var i = 0; i < treeNodes.length; i++) {
            treeNodes[i].level = childLevel;
            treeNodes[i].view = section + "/edit/" + treeNodes[i].id;
        }
    }

    return {
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
                        children: data
                    };
                    //ensure the view is added to each tree node
                    ensureLevelAndView(result.children, section);
                    //cache this result
                    //TODO: We'll need to un-cache this in many circumstances
                    treeArray[cacheKey] = result;
                    //return the data result as promised
                    //deferred.resolve(treeArray[cacheKey]);
                    return treeArray[cacheKey];
                });
        },

        getActions: function(treeItem, section) {

            return treeResource.loadMenu(treeItem.node)
                .then(function(data) {
                    //need to convert the icons to new ones
                    for (var i = 0; i < data.length; i++) {
                        data[i].cssclass = iconHelper.convertFromLegacyIcon(data[i].cssclass);
                    }
                    return data;
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
                    ensureLevelAndView(data, section, treeItem.level + 1);
                    return data;
                });
        }
    };
}

angular.module('umbraco.services').factory('treeService', treeService);