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
function umbracoMenuActions(treeService, $location, navigationService, appState, localizationService, usersResource, umbRequestHelper, notificationsService) {
    
    return {

        "ExportMember": function(args) {
            var url = umbRequestHelper.getApiUrl(
                "memberApiBaseUrl",
                "ExportMemberData",
                [{ key: args.entity.id }]);

            umbRequestHelper.downloadFile(url).then(function() {
                localizationService.localize("speechBubbles_memberExportedSuccess").then(function (value) {
                    notificationsService.success(value);
                })    
            }, function(data) {
                localizationService.localize("speechBubbles_memberExportedError").then(function (value) {
                    notificationsService.error(value);
                })    
            });
            
        },
        
        "DisableUser": function(args) {
            localizationService.localize("defaultdialogs_confirmdisable").then(function (txtConfirmDisable) {
                var currentMenuNode = UmbClientMgr.mainTree().getActionNode();
                if (confirm(txtConfirmDisable + ' "' + args.entity.name + '"?\n\n')) {
                    usersResource.disableUser(args.entity.id).then(function () {
                        navigationService.syncTree({ tree: args.treeAlias, path: [args.entity.parentId, args.entity.id], forceReload: true });
                    });
                }
            });
        },

        /**
         * @ngdoc method
         * @name umbraco.services.umbracoMenuActions#RefreshNode
         * @methodOf umbraco.services.umbracoMenuActions
         * @function
         *
         * @description
         * Clears all node children and then gets it's up-to-date children from the server and re-assigns them
         * @param {object} args An arguments object
         * @param {object} args.entity The basic entity being acted upon
         * @param {object} args.treeAlias The tree alias associated with this entity
         * @param {object} args.section The current section
         */
        "RefreshNode": function (args) {
            
            ////just in case clear any tree cache for this node/section
            //treeService.clearCache({
            //    cacheKey: "__" + args.section, //each item in the tree cache is cached by the section name
            //    childrenOf: args.entity.parentId //clear the children of the parent
            //});

            //since we're dealing with an entity, we need to attempt to find it's tree node, in the main tree
            // this action is purely a UI thing so if for whatever reason there is no loaded tree node in the UI
            // we can safely ignore this process.
            
            //to find a visible tree node, we'll go get the currently loaded root node from appState
            var treeRoot = appState.getTreeState("currentRootNode");
            if (treeRoot && treeRoot.root) {
                var treeNode = treeService.getDescendantNode(treeRoot.root, args.entity.id, args.treeAlias);
                if (treeNode) {
                    treeService.loadNodeChildren({ node: treeNode, section: args.section }).then(function () {
                        navigationService.hideMenu();
                    });
                }                
            }

            
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
         * @param {object} args.entity The basic entity being acted upon
         * @param {object} args.treeAlias The tree alias associated with this entity
         * @param {object} args.section The current section
         */
        "CreateChildEntity": function (args) {

            navigationService.hideNavigation();

            var route = "/" + args.section + "/" + args.treeAlias + "/edit/" + args.entity.id;
            //change to new path
            $location.path(route).search({ create: true });
            
        }
    };
} 

angular.module('umbraco.services').factory('umbracoMenuActions', umbracoMenuActions);
