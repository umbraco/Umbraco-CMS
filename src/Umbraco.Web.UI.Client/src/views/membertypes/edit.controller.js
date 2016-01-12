/**
 * @ngdoc controller
 * @name Umbraco.Editors.MemberType.EditController
 * @function
 *
 * @description
 * The controller for the member type editor
 */
(function () {
    "use strict";

    function MemberTypesEditController($scope, $rootScope, $routeParams, $log, $filter, memberTypeResource, dataTypeResource, editorState, iconHelper, formHelper, navigationService, contentEditingHelper, notificationsService, $q, localizationService, overlayHelper) {

        var vm = this;
        var localizeSaving = localizationService.localize("general_saving").then(function (value) {return value;});

        vm.save = save;

        vm.currentNode = null;
        vm.contentType = {};
        vm.page = {};
        vm.page.loading = false;
        vm.page.saveButtonState = "init";
        vm.page.navigation = [
			{
			    "name": localizationService.localize("general_design").then(function (value) {return value;}),
			    "icon": "icon-document-dashed-line",
			    "view": "views/membertypes/views/design/design.html",
			    "active": true
			}
        ];

        vm.page.keyboardShortcutsOverview = [
			{
                "name": localizationService.localize("shortcuts_shortcut").then(function (value) {return value;}),
			    "shortcuts": [
					{
					    "description": localizationService.localize("shortcuts_addTab").then(function (value) {return value;}),
					    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "t" }]
					},
					{
					    "description": localizationService.localize("shortcuts_addProperty").then(function (value) {return value;}),
					    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "p" }]
					},
					{
					    "description": localizationService.localize("shortcuts_addEditor").then(function (value) {return value;}),
					    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "e" }]
					},
					{
					    "description": localizationService.localize("shortcuts_editDataType").then(function (value) {return value;}),
					    "keys": [{ "key": "alt" }, { "key": "shift" }, { "key": "d" }]
					}
			    ]
			}
        ];

        if ($routeParams.create) {

            vm.page.loading = true;

            //we are creating so get an empty data type item
            memberTypeResource.getScaffold($routeParams.id)
				.then(function (dt) {
				    init(dt);

				    vm.page.loading = false;
				});
        }
        else {

            vm.page.loading = true;

            memberTypeResource.getById($routeParams.id).then(function (dt) {
                init(dt);

                syncTreeNode(vm.contentType, dt.path, true);

                vm.page.loading = false;
            });
        }

        function save() {
            // only save if there is no overlays open
            if(overlayHelper.getNumberOfOverlays() === 0) {

                var deferred = $q.defer();

                vm.page.saveButtonState = "busy";

                contentEditingHelper.contentEditorPerformSave({
                    statusMessage: localizeSaving,
                    saveMethod: memberTypeResource.save,
                    scope: $scope,
                    content: vm.contentType,
                    //We do not redirect on failure for doc types - this is because it is not possible to actually save the doc
                    // type when server side validation fails - as opposed to content where we are capable of saving the content
                    // item if server side validation fails
                    redirectOnFailure: false,
                    //no-op for rebind callback... we don't really need to rebind for content types
                    rebindCallback: angular.noop
                }).then(function (data) {
                    //success
                    syncTreeNode(vm.contentType, data.path);

                    vm.page.saveButtonState = "success";

                    deferred.resolve(data);
                }, function (err) {
                    //error
                    if (err) {
                        editorState.set($scope.content);
                    }
                    else {
                        localizationService.localize("speechBubbles_validationFailedHeader").then(function (headerValue) {
                            localizationService.localize("speechBubbles_validationFailedMessage").then(function (msgValue) {
                                notificationsService.error(headerValue, msgValue);
                            });
                        });
                    }

                    vm.page.saveButtonState = "error";

                    deferred.reject(err);
                });

                return deferred.promise;
            }

        }

        function init(contentType) {

            // set all tab to inactive
            if (contentType.groups.length !== 0) {
                angular.forEach(contentType.groups, function (group) {

                    angular.forEach(group.properties, function (property) {
                        // get data type details for each property
                        getDataTypeDetails(property);
                    });

                });
            }

            // convert legacy icons
            convertLegacyIcons(contentType);

            //set a shared state
            editorState.set(contentType);

            vm.contentType = contentType;

        }

        function convertLegacyIcons(contentType) {

            // make array to store contentType icon
            var contentTypeArray = [];

            // push icon to array
            contentTypeArray.push({ "icon": contentType.icon });

            // run through icon method
            iconHelper.formatContentTypeIcons(contentTypeArray);

            // set icon back on contentType
            contentType.icon = contentTypeArray[0].icon;

        }

        function getDataTypeDetails(property) {

            if (property.propertyState !== "init") {

                dataTypeResource.getById(property.dataTypeId)
					.then(function (dataType) {
					    property.dataTypeIcon = dataType.icon;
					    property.dataTypeName = dataType.name;
					});
            }
        }

        /** Syncs the content type  to it's tree node - this occurs on first load and after saving */
        function syncTreeNode(dt, path, initialLoad) {

            navigationService.syncTree({ tree: "membertypes", path: path.split(","), forceReload: initialLoad !== true }).then(function (syncArgs) {
                vm.currentNode = syncArgs.node;
            });

        }


    }

    angular.module("umbraco").controller("Umbraco.Editors.MemberTypes.EditController", MemberTypesEditController);

})();
