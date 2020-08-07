/**
 * @ngdoc controller
 * @name Umbraco.Editors.Member.EditController
 * @function
 *
 * @description
 * The controller for the member editor
 */
function MemberEditController($scope, $routeParams, $location, appState, memberResource, entityResource, navigationService, notificationsService, localizationService, serverValidationManager, contentEditingHelper, fileManager, formHelper, editorState, umbRequestHelper, $http) {

    var infiniteMode = $scope.model && $scope.model.infiniteMode;
    var id = infiniteMode ? $scope.model.id : $routeParams.id;
    var create = infiniteMode ? $scope.model.create : $routeParams.create;
    var listName = infiniteMode ? $scope.model.listname : $routeParams.listName;
    var docType = infiniteMode ? $scope.model.doctype : $routeParams.doctype;

    $scope.header = {};
    $scope.header.editorfor = "visuallyHiddenTexts_newMember";
    $scope.header.setPageTitle = true;

    //setup scope vars
    $scope.page = {};
    $scope.page.loading = true;
    $scope.page.menu = {};
    $scope.page.menu.currentSection = appState.getSectionState("currentSection");
    $scope.page.menu.currentNode = null; //the editors affiliated node
    $scope.page.nameLocked = false;
    $scope.page.saveButtonState = "init";
    $scope.page.exportButton = "init";

    //build a path to sync the tree with
    function buildTreePath(data) {
        return listName ? "-1," + listName : "-1";
    }

    if (create) {

        //if there is no doc type specified then we are going to assume that
        // we are not using the umbraco membership provider
        if (docType) {

            //we are creating so get an empty member item
            memberResource.getScaffold(docType)
                .then(function(data) {

                    $scope.content = data;

                    init();

                    $scope.page.loading = false;

                });
        }
        else {

            memberResource.getScaffold()
                .then(function (data) {
                    $scope.content = data;

                    init();

                    $scope.page.loading = false;

                });
        }

    }
    else {
        //so, we usually refernce all editors with the Int ID, but with members we have
        //a different pattern, adding a route-redirect here to handle this just in case.
        //(isNumber doesnt work here since its seen as a string)
        //The reason this might be an INT is due to the routing used for the member list view
        //but this is now configured to use the key, so this is just a fail safe

        if (id && id.length < 9) {

            entityResource.getById(id, "Member").then(function(entity) {
                $location.path("/member/member/edit/" + entity.key);
            });
        }
        else {

            //we are editing so get the content item from the server
            memberResource.getByKey(id)
                .then(function(data) {

                    $scope.content = data;

                    init();

                    if (!infiniteMode) {
                        var path = buildTreePath(data);

                        //sync the tree (only for ui purposes)
                        navigationService.syncTree({ tree: "member", path: path.split(",") });
                    }
                    //it's the initial load of the editor, we need to get the tree node
                    // from the server so that we can load in the actions menu.
                    umbRequestHelper.resourcePromise(
                        $http.get(data.treeNodeUrl),
                        'Failed to retrieve data for child node ' + data.key).then(function (node) {
                            $scope.page.menu.currentNode = node;
                        });

                    //in one particular special case, after we've created a new item we redirect back to the edit
                    // route but there might be server validation errors in the collection which we need to display
                    // after the redirect, so we will bind all subscriptions which will show the server validation errors
                    // if there are any and then clear them so the collection no longer persists them.
                    serverValidationManager.notifyAndClearAllSubscriptions();

                    $scope.page.loading = false;

                });
        }

    }

    function init() {

        var content = $scope.content;

        // we need to check wether an app is present in the current data, if not we will present the default app.
        var isAppPresent = false;

        // on first init, we dont have any apps. but if we are re-initializing, we do, but ...
        if ($scope.app) {

            // lets check if it still exists as part of our apps array. (if not we have made a change to our docType, even just a re-save of the docType it will turn into new Apps.)
            _.forEach(content.apps, function (app) {
                if (app === $scope.app) {
                    isAppPresent = true;
                }
            });

            // if we did reload our DocType, but still have the same app we will try to find it by the alias.
            if (isAppPresent === false) {
                _.forEach(content.apps, function (app) {
                    if (app.alias === $scope.app.alias) {
                        isAppPresent = true;
                        app.active = true;
                        $scope.appChanged(app);
                    }
                });
            }

        }

        // if we still dont have a app, lets show the first one:
        if (isAppPresent === false) {
            content.apps[0].active = true;
            $scope.appChanged(content.apps[0]);
        }

        if (content.membershipScenario === 0) {
            $scope.page.nameLocked = true;
        }

        editorState.set($scope.content);

    }

    /** Just shows a simple notification that there are client side validation issues to be fixed */
    function showValidationNotification() {
        //TODO: We need to make the validation UI much better, there's a lot of inconsistencies in v8 including colors, issues with the property groups and validation errors between variants

        //need to show a notification else it's not clear there was an error.
        localizationService.localizeMany([
                "speechBubbles_validationFailedHeader",
                "speechBubbles_validationFailedMessage"
            ]
        ).then(function (data) {
            notificationsService.error(data[0], data[1]);
        });
    }

    $scope.save = function() {

        if (formHelper.submitForm({ scope: $scope })) {

            $scope.page.saveButtonState = "busy";

            //anytime a user is changing a member's password without the oldPassword, we are in effect resetting it so we need to set that flag here
            var passwordProp = _.find(contentEditingHelper.getAllProps($scope.content), function (e) { return e.alias === '_umb_password' });
            if (passwordProp && passwordProp.value && (typeof passwordProp.value.reset !== 'undefined') && !passwordProp.value.reset) {
                //so if the admin is not explicitly resetting the password, flag it for resetting if a new password is being entered
                passwordProp.value.reset = !passwordProp.value.oldPassword && passwordProp.config.allowManuallyChangingPassword;
            }

            memberResource.save($scope.content, create, fileManager.getFiles())
                .then(function(data) {

                    formHelper.resetForm({ scope: $scope });

                    contentEditingHelper.handleSuccessfulSave({
                        scope: $scope,
                        savedContent: data,
                        //specify a custom id to redirect to since we want to use the GUID
                        redirectId: data.key,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, data)
                    });

                    editorState.set($scope.content);
                    $scope.page.saveButtonState = "success";

                    var path = buildTreePath(data);

                    //sync the tree (only for ui purposes)
                    navigationService.syncTree({ tree: "member", path: path.split(","), forceReload: true });

            }, function (err) {
                    formHelper.resetForm({ scope: $scope, hasErrors: true });
                    contentEditingHelper.handleSaveError({
                        err: err,
                        rebindCallback: contentEditingHelper.reBindChangedProperties($scope.content, err.data)
                    });

                    editorState.set($scope.content);
                    $scope.page.saveButtonState = "error";

                });
        }
        else {
            showValidationNotification();
        }

    };

    $scope.appChanged = function (app) {
        $scope.app = app;

        // setup infinite mode
        if (infiniteMode) {
            $scope.page.submitButtonLabelKey = "buttons_saveAndClose";
        }
    }

    $scope.showBack = function () {
        return !!listName;
    }

    /** Callback for when user clicks the back-icon */
    $scope.onBack = function () {
        $location.path("/member/member/list/" + listName);
        $location.search("listName", null);
        if ($routeParams.page) {
            $location.search("page", $routeParams.page);
        }
    };

    $scope.export = function() {
        var memberKey = $scope.content.key;
        memberResource.exportMemberData(memberKey);
    }

}

angular.module("umbraco").controller("Umbraco.Editors.Member.EditController", MemberEditController);
