(function () {
  'use strict';

  function ContentEditController($rootScope, $scope, $routeParams, $q, $timeout, $window, $location, appState, contentResource, entityResource, navigationService, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, treeService, fileManager, formHelper, umbRequestHelper, keyboardService, umbModelMapper, editorState, $http, eventsService, relationResource) {

    var evts = [];

    //setup scope vars
    $scope.defaultButton = null;
    $scope.subButtons = [];

    $scope.page = {};
    $scope.page.loading = false;
    $scope.page.menu = {};
    $scope.page.menu.currentNode = null;
    $scope.page.menu.currentSection = appState.getSectionState("currentSection");
    $scope.page.listViewPath = null;
    $scope.page.isNew = $scope.isNew ? true : false;
      $scope.page.buttonGroupState = "init";     
    $scope.allowOpen = true;


    function init(content) {

      createButtons(content);

      editorState.set($scope.content);

      //We fetch all ancestors of the node to generate the footer breadcrumb navigation
      if (!$scope.page.isNew) {
        if (content.parentId && content.parentId !== -1) {
          entityResource.getAncestors(content.id, "document")
            .then(function (anc) {
              $scope.ancestors = anc;
            });
        }
      }

      evts.push(eventsService.on("editors.content.changePublishDate", function (event, args) {
        createButtons(args.node);
      }));

      evts.push(eventsService.on("editors.content.changeUnpublishDate", function (event, args) {
        createButtons(args.node);
      }));

      // We don't get the info tab from the server from version 7.8 so we need to manually add it
      contentEditingHelper.addInfoTab($scope.content.tabs);

    }

    function getNode() {

      $scope.page.loading = true;

      //we are editing so get the content item from the server
      $scope.getMethod()($scope.contentId)
        .then(function (data) {

          $scope.content = data;

          if (data.isChildOfListView && data.trashed === false) {
            $scope.page.listViewPath = ($routeParams.page) ?
              "/content/content/edit/" + data.parentId + "?page=" + $routeParams.page :
              "/content/content/edit/" + data.parentId;
          }

          init($scope.content);           

          //in one particular special case, after we've created a new item we redirect back to the edit
          // route but there might be server validation errors in the collection which we need to display
          // after the redirect, so we will bind all subscriptions which will show the server validation errors
          // if there are any and then clear them so the collection no longer persists them.
          serverValidationManager.executeAndClearAllSubscriptions();

          syncTreeNode($scope.content, data.path, true);

          resetLastListPageNumber($scope.content);

          eventsService.emit("content.loaded", { content: $scope.content });

          $scope.page.loading = false;

        });

    }

    function createButtons(content) {
      $scope.page.buttonGroupState = "init";
      var buttons = contentEditingHelper.configureContentEditorButtons({
        create: $scope.page.isNew,
        content: content,
        methods: {
          saveAndPublish: $scope.saveAndPublish,
          sendToPublish: $scope.sendToPublish,
          save: $scope.save,
          unPublish: $scope.unPublish
        }
        });



      $scope.defaultButton = buttons.defaultButton;
      $scope.subButtons = buttons.subButtons;      
    }

    /** Syncs the content item to it's tree node - this occurs on first load and after saving */
    function syncTreeNode(content, path, initialLoad) {

      if (!$scope.content.isChildOfListView) {
        navigationService.syncTree({ tree: $scope.treeAlias, path: path.split(","), forceReload: initialLoad !== true }).then(function (syncArgs) {
          $scope.page.menu.currentNode = syncArgs.node;
        });
      }
      else if (initialLoad === true) {

        //it's a child item, just sync the ui node to the parent
        navigationService.syncTree({ tree: $scope.treeAlias, path: path.substring(0, path.lastIndexOf(",")).split(","), forceReload: initialLoad !== true });

        //if this is a child of a list view and it's the initial load of the editor, we need to get the tree node 
        // from the server so that we can load in the actions menu.
        umbRequestHelper.resourcePromise(
          $http.get(content.treeNodeUrl),
          'Failed to retrieve data for child node ' + content.id).then(function (node) {
          $scope.page.menu.currentNode = node;
        });
      }
    }

    // This is a helper method to reduce the amount of code repitition for actions: Save, Publish, SendToPublish
    function performSave(args) {
      var deferred = $q.defer();

      $scope.page.buttonGroupState = "busy";

      eventsService.emit("content.saving", { content: $scope.content, action: args.action });

      contentEditingHelper.contentEditorPerformSave({
        statusMessage: args.statusMessage,
        saveMethod: args.saveMethod,
        scope: $scope,
        content: $scope.content,
        action: args.action
      }).then(function (data) {
        //success            
        init($scope.content);
        syncTreeNode($scope.content, data.path);

        $scope.page.buttonGroupState = "success";

        deferred.resolve(data);

        eventsService.emit("content.saved", { content: $scope.content, action: args.action });

      }, function (err) {
        //error
        if (err) {
          editorState.set($scope.content);
        }

        $scope.page.buttonGroupState = "error";

        deferred.reject(err);
      });

      return deferred.promise;
    }

    function resetLastListPageNumber(content) {
      // We're using rootScope to store the page number for list views, so if returning to the list
      // we can restore the page.  If we've moved on to edit a piece of content that's not the list or it's children
      // we should remove this so as not to confuse if navigating to a different list
      if (!content.isChildOfListView && !content.isContainer) {
        $rootScope.lastListViewPageViewed = null;
      }
    }

    if ($scope.page.isNew) {

      $scope.page.loading = true;

      //we are creating so get an empty content item
      $scope.getScaffoldMethod()()
        .then(function (data) {

          $scope.content = data;

          init($scope.content);

          resetLastListPageNumber($scope.content);

          $scope.page.loading = false;

          eventsService.emit("content.newReady", { content: $scope.content });

          });
    }
    else {

      getNode();

    }     

    $scope.unPublish = function () {								
			// raising the event triggers the confirmation dialog			
			if (!notificationsService.hasView()) {
				notificationsService.add({ view: "confirmunpublish" });
			}				
				
			$scope.page.buttonGroupState = "busy";

			// actioning the dialog raises the confirmUnpublish event, act on it here
			var actioned = $rootScope.$on("content.confirmUnpublish", function(event, confirmed) {
				if (confirmed && formHelper.submitForm({ scope: $scope, statusMessage: "Unpublishing...", skipValidation: true })) {
							
					eventsService.emit("content.unpublishing", { content: $scope.content });

					contentResource.unPublish($scope.content.id)
						.then(function (data) {

							formHelper.resetForm({ scope: $scope, notifications: data.notifications });

							contentEditingHelper.handleSuccessfulSave({
								scope: $scope,
								savedContent: data,
								rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
							});

							init($scope.content);

							syncTreeNode($scope.content, data.path);

							$scope.page.buttonGroupState = "success";
							eventsService.emit("content.unpublished", { content: $scope.content });

						}, function(err) {
							formHelper.showNotifications(err.data);
							$scope.page.buttonGroupState = 'error';
						});	
					
				} else {
					$scope.page.buttonGroupState = "init";
				}
				
				// unsubscribe to avoid queueing notifications
				// listener is re-bound when the unpublish button is clicked so it is created just-in-time				
				actioned();
				
			}); 
    };

    $scope.sendToPublish = function () {
      return performSave({ saveMethod: contentResource.sendToPublish, statusMessage: "Sending...", action: "sendToPublish" });
    };

    $scope.saveAndPublish = function () {
      return performSave({ saveMethod: contentResource.publish, statusMessage: "Publishing...", action: "publish" });
    };

    $scope.save = function () {
      return performSave({ saveMethod: $scope.saveMethod(), statusMessage: "Saving...", action: "save" });
    };

    $scope.preview = function (content) {


      if (!$scope.busy) {

        // Chromes popup blocker will kick in if a window is opened 
        // without the initial scoped request. This trick will fix that.
        //  
        var previewWindow = $window.open('preview/?init=true&id=' + content.id, 'umbpreview');

        // Build the correct path so both /#/ and #/ work.
        var redirect = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + '/preview/?id=' + content.id;

        //The user cannot save if they don't have access to do that, in which case we just want to preview
        //and that's it otherwise they'll get an unauthorized access message
        if (!_.contains(content.allowedActions, "A")) {
            previewWindow.location.href = redirect;
        }
        else {
            $scope.save().then(function (data) {
                previewWindow.location.href = redirect;
            });
        }
      }
    };

    $scope.restore = function (content) {

      $scope.page.buttonRestore = "busy";

      relationResource.getByChildId(content.id, "relateParentDocumentOnDelete").then(function (data) {

        var relation = null;
        var target = null;
        var error = { headline: "Cannot automatically restore this item", content: "Use the Move menu item to move it manually"};

        if (data.length == 0) {
          notificationsService.error(error.headline, "There is no 'restore' relation found for this node. Use the Move menu item to move it manually.");
          $scope.page.buttonRestore = "error";
          return;
        }

        relation = data[0];

        if (relation.parentId == -1) {
          target = { id: -1, name: "Root" };
          moveNode(content, target);
        } else {
          contentResource.getById(relation.parentId).then(function (data) {
            target = data;

            // make sure the target item isn't in the recycle bin
            if(target.path.indexOf("-20") !== -1) {
              notificationsService.error(error.headline, "The item you want to restore it under (" + target.name + ") is in the recycle bin. Use the Move menu item to move the item manually.");
              $scope.page.buttonRestore = "error";              
              return;
            }

            moveNode(content, target);

          }, function (err) {
            $scope.page.buttonRestore = "error";
            notificationsService.error(error.headline, error.content);
          });
        }

      }, function (err) {
        $scope.page.buttonRestore = "error";
        notificationsService.error(error.headline, error.content);
      });


    };

    function moveNode(node, target) {

      contentResource.move({ "parentId": target.id, "id": node.id })
        .then(function (path) {

          // remove the node that we're working on
          if($scope.page.menu.currentNode) {
            treeService.removeNode($scope.page.menu.currentNode);
          }

          // sync the destination node
          navigationService.syncTree({ tree: "content", path: path, forceReload: true, activate: false });

          $scope.page.buttonRestore = "success";
          notificationsService.success("Successfully restored " + node.name + " to " + target.name);

          // reload the node
          getNode();

        }, function (err) {
          $scope.page.buttonRestore = "error";
          notificationsService.error("Cannot automatically restore this item", err);
        });

    }

    //ensure to unregister from all events!
    $scope.$on('$destroy', function () {
      for (var e in evts) {
        eventsService.unsubscribe(evts[e]);
      }
    });

  }

  function createDirective() {

    var directive = {
      restrict: 'E',
      replace: true,
      templateUrl: 'views/components/content/edit.html',
      controller: 'Umbraco.Editors.Content.EditorDirectiveController',
      scope: {
        contentId: "=",
        isNew: "=?",
        treeAlias: "@",
        page: "=?",
        saveMethod: "&",
        getMethod: "&",
        getScaffoldMethod: "&?"
      }
    };

    return directive;

  }

  angular.module('umbraco.directives').controller('Umbraco.Editors.Content.EditorDirectiveController', ContentEditController);
  angular.module('umbraco.directives').directive('contentEditor', createDirective);

})();
