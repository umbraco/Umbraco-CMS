
/**
 * @ngdoc controller
 * @name Umbraco.NodeEditsController
 * @function
 * 
 * @description
 * The controller for managing node edits
 * 
 */
function NodeEditsController($scope, $rootScope, $routeParams, eventsService, appState, nodeEditsNotificationsService, nodeEditsService) {
    
    if (Umbraco.Sys.ServerVariables.umbracoSettings.enableNodeEdits !== true) {
        //Disabled so do nothing
        return;
    }

    var evts = [];
    var currentUser = null;
    var otherUserEditsForCurrentNode = [];
    var connectionId = null;

    //Config properties
    var applyTreeStyling = true;

    function editsListChanged(currentNodeId, claimAfterReconnect) {
        if (applyTreeStyling) {
            //Remove all (possibly) added styling
            var treeItem = $("i[title*='content/content/edit']").closest("li").children("div");
            treeItemRemoveStyling(treeItem);
        }
        var newUserEditsForCurrentNode = [];

        for (var i = 0; i < allEdits.length; i++) {
            var edit = allEdits[i];
            if (edit.NodeId == 0) {
                continue;
            }
            if (currentNodeId == edit.NodeId) {
                if (currentUser.id != edit.UserId) {
                    if (claimAfterReconnect !== true) {
                        //Show notification, only if we aren't reconnecting
                        nodeEditsNotificationsService.setCurrentEditNotification(edit);
                    }
                    //Remember this edit for later use
                    newUserEditsForCurrentNode.push(edit);
                }
                else if (currentUser.id === edit.UserId && edit.ConnectionId !== connectionId) {
                    //User is editing this node on multiple browsers/tabs
                }
            }

            if (applyTreeStyling == true && currentUser.id != edit.UserId) {
                var treeItem = $("i[title*='" + edit.NodeId + "']").closest("li").children("div");
                treeItemApplyStyling(treeItem, edit);
            }
        }

        if (claimAfterReconnect !== true) {
            for (var i = 0; i < otherUserEditsForCurrentNode.length; i++) {
                var edit = otherUserEditsForCurrentNode[i];
                var userId = edit.UserId;

                if (_.where(newUserEditsForCurrentNode, { UserId: userId }).length === 0) {
                    //Release this users claim on current node
                    nodeEditsNotificationsService.setReleasedNotification(edit);
                }
            }
        }

        otherUserEditsForCurrentNode = newUserEditsForCurrentNode;
    }

    function treeItemApplyStyling(treeItem, edit) {
        treeItem.addClass("being-edited");
    }

    function treeItemRemoveStyling(treeItem) {
        treeItem.removeClass("being-edited");
    }

    evts.push($rootScope.$on("broadcastPublished", function (event, data) {
        if ($routeParams.id == data.NodeId && currentUser.id != data.UserId) {
            nodeEditsNotificationsService.setPublishedNotification(data.UserName, data.Time);
        }
    }));

    evts.push($rootScope.$on("broadcastUserDisconnected", function (event, data) {
        
    }));

    evts.push($rootScope.$on("broadcastEditsChanged", function (event, edits) {
        allEdits = edits;
        editsListChanged($routeParams.id);
    }));

    evts.push($rootScope.$on("reconnected", function (event) {
        claimCurrentNodeIfPossible($routeParams.id, true);
    }));

    evts.push($rootScope.$on('$locationChangeSuccess', function (a, b, c) {
        if ($routeParams.section == "content") {

            var activeNode = appState.getTreeState("selectedNode");
            if (activeNode) {
                claimCurrentNodeIfPossible(activeNode.id, false);
            }
        }
    }));

    evts.push(eventsService.on('app.authenticated', function (evt, data) {
        currentUser = data.user;
        
        nodeEditsService.start(currentUser).then(function (connId) {
            connectionId = connId;
            claimCurrentNodeIfPossible($routeParams.id, false);
        });
    }));

    //when a user logs out or timesout
    evts.push(eventsService.on("app.notAuthenticated", function () {
        //Call disconnect even though it will return a 403 (access denied because we are allready logged out),
        //but it kills the client side connection causing a timeout and therefore releasing the node edit.
        try {
            nodeEditsService.disconnect();
        } catch (e) {

        }
        
    }));
     
    $scope.$on('$destroy', function () {
        for (var e in evts) {
            eventsService.unsubscribe(evts[e]);
        }

        nodeEditsService.disconnect();
    });
    
    function claimCurrentNodeIfPossible(nodeId, isClaimAfterReconnect) {
        var pageId = nodeId ? nodeId : ($routeParams.id ? parseInt($routeParams.id) : 0);

        nodeEditsService.tryClaimSingleNode(currentUser.id, currentUser.name, pageId).then(function (edits) {
            if (edits) {
                allEdits = edits;
            }
            else {
                allEdits = [];
            }

            //We changed or reclaimed node, so we don't care about other editors leaving
            otherUserEditsForCurrentNode = [];

            //Clear any old notifications (only the once we created)
            nodeEditsNotificationsService.removeAll();

            editsListChanged(nodeId, isClaimAfterReconnect);
        });
    }
};


//register it
angular.module('umbraco').controller("Umbraco.NodeEditsController", NodeEditsController);
