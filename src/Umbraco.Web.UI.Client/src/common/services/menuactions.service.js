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
function umbracoMenuActions($q, treeService, $location, navigationService) {
    
    return {
        
        /**
         * @ngdoc method
         * @name umbraco.services.umbracoMenuActions#RefreshNode
         * @methodOf umbraco.services.umbracoMenuActions
         * @function
         *
         * @description
         * Clears all node children and then gets it's up-to-date children from the server and re-assigns them
         * @param {object} args An arguments object
         * @param {object} args.treeNode The tree node
         * @param {object} args.section The current section
         */
        "RefreshNode": function (args) {
            treeService.loadNodeChildren({ node: args.treeNode, section: args.section });
        },
        
        /**
         * @ngdoc method
         * @name umbraco.services.umbracoMenuActions#CreateChildEntity
         * @methodOf umbraco.services.umbracoMenuActions
         * @function
         *
         * @description
         * This will re-route to a route for creating a new entity as a child of the current node
         * @param {object} args An arguments object
         * @param {object} args.treeNode The tree node
         * @param {object} args.section The current section
         */
        "CreateChildEntity": function (args) {

            navigationService.hideNavigation();

            var route = "/" + args.section + "/" + treeService.getTreeAlias(args.treeNode) + "/edit/" + args.treeNode.id;
            //change to new path
            $location.path(route).search({ create: true });
            
        }
    };
} 

angular.module('umbraco.services').factory('umbracoMenuActions', umbracoMenuActions);