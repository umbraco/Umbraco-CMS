(function () {
    'use strict';

    function ContentNodeInfoDirective($timeout, $location, logResource, eventsService, userService, localizationService, dateHelper, editorService, redirectUrlsResource) {

        function link(scope, element, attrs, ctrl) {

            var evts = [];
            var isInfoTab = false;
            var labels = {};
            scope.publishStatus = [];

            scope.disableTemplates = Umbraco.Sys.ServerVariables.features.disabledFeatures.disableTemplates;
            scope.allowChangeDocumentType = false;
            scope.allowChangeTemplate = false;

            function onInit() {

                userService.getCurrentUser().then(function(user){
                        // only allow change of media type if user has access to the settings sections
                        angular.forEach(user.sections, function(section){
                            if(section.alias === "settings") {
                                scope.allowChangeDocumentType = true;
                                scope.allowChangeTemplate = true;
                            }
                        });
                    });

                var keys = [
                    "general_deleted", 
                    "content_unpublished", 
                    "content_published",
                    "content_publishedPendingChanges",
                    "content_notCreated"
                ];

                localizationService.localizeMany(keys)
                    .then(function(data){
                        labels.deleted = data[0];
                        labels.unpublished = data[1]; //aka draft
                        labels.published = data[2];
                        labels.publishedPendingChanges = data[3];
                        labels.notCreated = data[4];

                        setNodePublishStatus(scope.node);

                    });

                scope.auditTrailOptions = {
                    "id": scope.node.id
                };

                // get available templates
                scope.availableTemplates = scope.node.allowedTemplates;

                // get document type details
                scope.documentType = scope.node.documentType;

                //default setting for redirect url management
                scope.urlTrackerDisabled = false;

                // Declare a fallback URL for the <umb-node-preview/> directive
                if (scope.documentType !== null) {
                    scope.previewOpenUrl = '#/settings/documenttypes/edit/' + scope.documentType.id;
                }
            }

            scope.auditTrailPageChange = function (pageNumber) {
                scope.auditTrailOptions.pageNumber = pageNumber;
                loadAuditTrail();
            };

            scope.openDocumentType = function (documentType) {
                var editor = {
                    id: documentType.id,
                    submit: function(model) {
                        editorService.close();
                    },
                    close: function() {
                        editorService.close();
                    }
                };
                editorService.documentTypeEditor(editor);
            };

            scope.openTemplate = function () {
                var templateEditor = {
                    id: scope.node.templateId,
                    submit: function(model) {
                        editorService.close();
                    },
                    close: function() {
                        editorService.close();
                    }
                };
                editorService.templateEditor(templateEditor);
            }

            scope.updateTemplate = function (templateAlias) {
                // update template value
                scope.node.template = templateAlias;
            };

            function loadAuditTrail() {

                scope.loadingAuditTrail = true;

                logResource.getPagedEntityLog(scope.auditTrailOptions)
                    .then(function (data) {

                        // get current backoffice user and format dates
                        userService.getCurrentUser().then(function (currentUser) {
                            angular.forEach(data.items, function (item) {
                                item.timestampFormatted = dateHelper.getLocalDate(item.timestamp, currentUser.locale, 'LLL');
                            });
                        });

                        scope.auditTrail = data.items;
                        scope.auditTrailOptions.pageNumber = data.pageNumber;
                        scope.auditTrailOptions.pageSize = data.pageSize;
                        scope.auditTrailOptions.totalItems = data.totalItems;
                        scope.auditTrailOptions.totalPages = data.totalPages;

                        setAuditTrailLogTypeColor(scope.auditTrail);

                        scope.loadingAuditTrail = false;
                    });

            }
            function loadRedirectUrls() {
                scope.loadingRedirectUrls = true;
                //check if Redirect Url Management is enabled
                redirectUrlsResource.getEnableState().then(function (response) {
                    scope.urlTrackerDisabled = response.enabled !== true;
                    if (scope.urlTrackerDisabled === false) {

                        redirectUrlsResource.getRedirectsForContentItem(scope.node.udi)
                            .then(function (data) {
                                scope.redirectUrls = data.searchResults;
                                scope.hasRedirects = (typeof data.searchResults !== 'undefined' && data.searchResults.length > 0);
                                scope.loadingRedirectUrls = false;
                            });
                    }
                    else {
                        scope.loadingRedirectUrls = false;
                    }
                });
            }

            function setAuditTrailLogTypeColor(auditTrail) {
                angular.forEach(auditTrail, function (item) {

                    switch (item.logType) {
                        case "Publish":
                            item.logTypeColor = "success";
                            break;
                        case "Unpublish":
                        case "Delete":
                            item.logTypeColor = "danger";
                            break;
                        default:
                            item.logTypeColor = "gray";
                    }
                });
            }

            function setNodePublishStatus(node) {

                scope.publishStatus = [];

                // deleted node
                if (node.trashed === true) {
                    scope.publishStatus.push({
                        label: labels.deleted,
                        color: "danger"
                    });
                    return;
                }

                if (node.variants) {
                    for (var i = 0; i < node.variants.length; i++) {

                        var variant = node.variants[i];

                        var status = {
                            culture: variant.language ? variant.language.culture : null
                        };

                        if (variant.state === "NotCreated") {
                            status.label = labels.notCreated;
                            status.color = "gray";
                        }
                        else if (variant.state === "Draft") {
                            // draft node
                            status.label = labels.unpublished;
                            status.color = "gray";
                        }
                        else if (variant.state === "Published") {
                            // published node
                            status.label = labels.published;
                            status.color = "success";
                        }
                        else if (variant.state === "PublishedPendingChanges") {
                            // published node with pending changes
                            status.label = labels.publishedPendingChanges;
                            status.color = "success";
                        }
                        
                        scope.publishStatus.push(status);
                    }
                }
            }

            // load audit trail and redirects when on the info tab
            evts.push(eventsService.on("app.tabChange", function (event, args) {
                $timeout(function () {
                    if (args.alias === "umbInfo") {
                        isInfoTab = true;
                        loadAuditTrail();
                        loadRedirectUrls();
                    } else {
                        isInfoTab = false;
                    }
                });
            }));

            // watch for content state updates
            scope.$watch('node.updateDate', function (newValue, oldValue) {

                if (!newValue) { return; }
                if (newValue === oldValue) { return; }

                if(isInfoTab) {
                    loadAuditTrail();
                    loadRedirectUrls();
                    setNodePublishStatus(scope.node);
                }
            });

            //ensure to unregister from all events!
            scope.$on('$destroy', function () {
                for (var e in evts) {
                    eventsService.unsubscribe(evts[e]);
                }
            });

            onInit();

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/content/umb-content-node-info.html',
            scope: {
                node: "="
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbContentNodeInfo', ContentNodeInfoDirective);

})();
