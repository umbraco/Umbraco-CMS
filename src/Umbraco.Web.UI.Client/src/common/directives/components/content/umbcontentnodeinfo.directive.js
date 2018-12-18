(function () {
    'use strict';

    function ContentNodeInfoDirective($timeout, logResource, eventsService, userService, localizationService, dateHelper, editorService, redirectUrlsResource, overlayService) {

        function link(scope, element, attrs, umbVariantContentCtrl) {

            var evts = [];
            var isInfoTab = false;
            var auditTrailLoaded = false;
            var labels = {};
            scope.publishStatus = [];

            scope.disableTemplates = Umbraco.Sys.ServerVariables.features.disabledFeatures.disableTemplates;
            scope.allowChangeDocumentType = false;
            scope.allowChangeTemplate = false;

            function onInit() {

                // if there are any infinite editors open we are in infinite editing
                scope.isInfiniteMode = editorService.getNumberOfEditors() > 0 ? true : false;

                userService.getCurrentUser().then(function(user){
                    // only allow change of media type if user has access to the settings sections
                    const hasAccessToSettings = user.allowedSections.indexOf("settings") !== -1 ? true : false;
                    scope.allowChangeDocumentType = hasAccessToSettings;
                    scope.allowChangeTemplate = hasAccessToSettings;
                });

                var keys = [
                    "general_deleted", 
                    "content_unpublished", 
                    "content_published",
                    "content_publishedPendingChanges",
                    "content_notCreated",
                    "prompt_unsavedChanges",
                    "prompt_doctypeChangeWarning"
                ];

                localizationService.localizeMany(keys)
                    .then(function(data){
                        labels.deleted = data[0];
                        labels.unpublished = data[1]; //aka draft
                        labels.published = data[2];
                        labels.publishedPendingChanges = data[3];
                        labels.notCreated = data[4];
                        labels.unsavedChanges = data[5];
                        labels.doctypeChangeWarning = data[6];

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

                //load in the audit trail if we are currently looking at the INFO tab
                if (umbVariantContentCtrl) {
                    var activeApp = _.find(umbVariantContentCtrl.editor.content.apps, a => a.active);
                    if (activeApp.alias === "umbInfo") {
                        isInfoTab = true;
                        loadAuditTrail();
                        loadRedirectUrls();
                    }
                }

            }

            scope.auditTrailPageChange = function (pageNumber) {
                scope.auditTrailOptions.pageNumber = pageNumber;
                auditTrailLoaded = false;
                loadAuditTrail();
            };

            scope.openDocumentType = function (documentType) {

                const variantIsDirty = _.some(scope.node.variants, function(variant) {
                    return variant.isDirty;
                });

                // add confirmation dialog before opening the doc type editor
                if(variantIsDirty) {
                    const confirm = {
                        title: labels.unsavedChanges,
                        view: "default",
                        content: labels.doctypeChangeWarning,
                        submitButtonLabelKey: "general_continue",
                        closeButtonLabelKey: "general_cancel",
                        submit: function() {
                            openDocTypeEditor(documentType);
                            overlayService.close();
                        },
                        close: function() {
                            overlayService.close();
                        }
                    };
                    overlayService.open(confirm);
                } else {
                    openDocTypeEditor(documentType);
                }

            };

            function openDocTypeEditor(documentType) {
                const editor = {
                    id: documentType.id,
                    submit: function(model) {
                        const args = { node: scope.node };
                        eventsService.emit('editors.content.reload', args);
                        editorService.close();
                    },
                    close: function() {
                        editorService.close();
                    }
                };
                editorService.documentTypeEditor(editor);
            }

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

            scope.openRollback = function() {
                
                var rollback = {
                    node: scope.node,
                    submit: function(model) {
                        const args = { node: scope.node };
                        eventsService.emit("editors.content.reload", args);
                        editorService.close();
                    },
                    close: function() {
                        editorService.close();
                    }
                };
                editorService.rollback(rollback);
            };

            function loadAuditTrail() {

                //don't load this if it's already done
                if (auditTrailLoaded) { return; };

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

                        auditTrailLoaded = true;
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

                if (isInfoTab) {
                    auditTrailLoaded = false;
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
            require: '^^umbVariantContent',
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
