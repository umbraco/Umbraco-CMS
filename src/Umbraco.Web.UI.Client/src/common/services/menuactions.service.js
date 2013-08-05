/**
 * @ngdoc service
 * @name umbraco.services.umbracoMenuActions
 *
 * @requires q
 * @requires treeService
 *	
 * @description
 * Defines the methods that are called when menu items declare only an action to execute
 */
function umbracoMenuActions($q, treeService) {
    
    return {
        
        /**
         * @ngdoc method
         * @name umbraco.services.umbracoMenuActions#RefreshNodeMenuItem
         * @methodOf umbraco.services.umbracoMenuActions
         * @function
         *
         * @description
         * Clears all node children and then gets it's up-to-date children from the server and re-assigns them
         * @param {object} args An arguments object
         * @param {object} args.treeNode The tree node
         * @param {object} args.section The current section
         */
        "RefreshNodeMenuItem": function (args) {
            treeService.loadNodeChildren({ node: args.treeNode, section: args.section });
        }
    };
} 

angular.module('umbraco.services').factory('umbracoMenuActions', umbracoMenuActions);