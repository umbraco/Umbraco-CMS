(function () {
  'use strict';

  function ContentEditController($rootScope, $scope, $routeParams, $q, $timeout, $window, appState, contentResource, entityResource, navigationService, notificationsService, angularHelper, serverValidationManager, contentEditingHelper, treeService, fileManager, formHelper, umbRequestHelper, keyboardService, umbModelMapper, editorState, $http) {

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

      $scope.auditTrail = [
        {
          "date": "03 December 2016 17:58PM",
          "action": "publish",
          "description": "Content was performed today by user",
          "user": {
            "name": "Zsolt Laszlo",
            "avatars": []
          }
        },
        {
          "date": "24 December 2016 20:18PM",
          "action": "unpublish",
          "description": "Content was performed by user",
          "user": {
            "name": "Mads Rasmussen",
            "avatars": [
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=30",
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=60",
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=90",
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=150",
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=300"
            ]
          }
        },
        {
          "date": "19 November 2016 21:11AM",
          "action": "save",
          "description": "Content was performed yesteraday by user",
          "user": {
            "name": "Zsolt Laszlo",
            "avatars": []
          }
        },
        {
          "date": "10 November 2016 10:41AM",
          "action": "save",
          "description": "Content was performed last week by user",
          "user": {
            "name": "Mads Rasmussen",
            "avatars": [
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=30",
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=60",
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=90",
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=150",
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=300"
            ]
          }
        },
        {
          "date": "02 November 2016 03:44PM",
          "action": "save",
          "description": "Content was performed last week by user",
          "user": {
            "name": "Zsolt Laszlo",
            "avatars": []
          }
        },
        {
          "date": "19 September 2016 18:21AM",
          "action": "publish",
          "description": "Content was performed two weeks ago by user",
          "user": {
            "name": "Mads Rasmussen",
            "avatars": [
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=30",
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=60",
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=90",
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=150",
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=300"
            ]
          }
        },
        {
          "date": "19 September 2016 08:51AM",
          "action": "save",
          "description": "Content was performed last month by user",
          "user": {
            "name": "Mads Rasmussen",
            "avatars": [
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=30",
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=60",
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=90",
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=150",
              "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=300"
            ]
          }
        },
        {
          "date": "11 September 2016 13:28AM",
          "action": "save",
          "description": "Content was performed by user",
          "user": {
            "name": "Zsolt Laszlo",
            "avatars": []
          }
        },
        {
          "date": "01 September 2016 23:19AM",
          "action": "save",
          "description": "Content was performed by user",
          "user": {
            "name": "Zsolt Laszlo",
            "avatars": []
          }
        }
      ];

      $scope.pagination = {
        "pageNumber": 1,
        "totalPages": 5
      };

      $scope.docType = 
        {
          "icon": "icon-item-arrangement",
          "name": "Project",
          "description": "Description for the doctype section"
        };

      $scope.template = {
        "description": "Description for the template section"
      };

      $scope.idSection = {
        "name": "Id"
      };



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

      // get the auditTrail
      setAuditTrailActionColor($scope.auditTrail);

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

        });
    }
    else {

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

          $scope.page.loading = false;

        });
    }


    $scope.unPublish = function () {

      if (formHelper.submitForm({ scope: $scope, statusMessage: "Unpublishing...", skipValidation: true })) {

        $scope.page.buttonGroupState = "busy";

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

          });
      }

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
        // outwith the initial scoped request. This trick will fix that.
        //  
        var previewWindow = $window.open('preview/?id=' + content.id, 'umbpreview');

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

    $scope.nextPage = function(pageNumber) {
      alert("next page" + pageNumber);
    };

    $scope.prevPage = function(pageNumber) {
      alert("previous page" + pageNumber);
    };

    $scope.goToPage = function(pageNumber) {
      alert("go to page" + pageNumber);
    };

    $scope.openTemplate = function(template) {
      alert("you opened template");
      console.log("this is not the doctype");
    };

    $scope.openDocType = function(docType) {
      alert("you opened docType");
      console.log("this is not the template");
    };

    function setAuditTrailActionColor(auditTrail) {
      angular.forEach(auditTrail, function (item) {
        switch (item.action) {
          case "publish":
            item.actionColor = "success";
            break;
          case "unpublish":
            item.actionColor = "danger";
            break;
          default:
            item.actionColor = "gray";
        }
      });
    }



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
