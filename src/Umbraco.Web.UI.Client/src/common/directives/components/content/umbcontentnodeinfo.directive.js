(function () {
    'use strict';

    function ContentNodeInfoDirective($timeout, logResource, eventsService, userService, localizationService, dateHelper, editorService, redirectUrlsResource, overlayService, entityResource) {

        function link(scope) {

            var evts = [];
            var isInfoTab = false;
            var auditTrailLoaded = false;
            var labels = {};
            scope.publishStatus = [];
            scope.currentVariant = null;
            scope.currentUrls = [];

            scope.disableTemplates = Umbraco.Sys.ServerVariables.features.disabledFeatures.disableTemplates;
            scope.allowChangeDocumentType = false;
            scope.allowChangeTemplate = false;
            scope.allTemplates = [];

            scope.historyLabelKey = scope.node.variants && scope.node.variants.length === 1 ? "general_history" : "auditTrails_historyIncludingVariants";

            function onInit() {
                entityResource.getAll("Template").then(function (templates) {
                    scope.allTemplates = templates;
                });

                // set currentVariant
                scope.currentVariant = _.find(scope.node.variants, (v) => v.active);

                updateCurrentUrls();

                // if there are any infinite editors open we are in infinite editing
                scope.isInfiniteMode = editorService.getNumberOfEditors() > 0 ? true : false;

                userService.getCurrentUser().then(function (user) {
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
                    "prompt_doctypeChangeWarning",
                    "content_itemNotPublished",
                    "general_choose"
                ];

                localizationService.localizeMany(keys)
                    .then(function (data) {
                        [labels.deleted, 
                        labels.unpublished, 
                        labels.published,
                        labels.publishedPendingChanges,
                        labels.notCreated,
                        labels.unsavedChanges,
                        labels.doctypeChangeWarning,
                        labels.notPublished,
                        scope.chooseLabel] = data;

                        setNodePublishStatus();

                        if (scope.currentUrls && scope.currentUrls.length === 0) {
                            if (scope.node.id > 0) {
                                //it's created but not published
                                scope.currentUrls.push({ text: labels.notPublished, isUrl: false });
                            }
                            else {
                                //it's new
                                scope.currentUrls.push({ text: labels.notCreated, isUrl: false })
                            }
                        }

                    });

                scope.auditTrailOptions = {
                    "id": scope.node.id
                };

                // make sure dates are formatted to the user's locale
                formatDatesToLocal();

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

                var activeApp = _.find(scope.node.apps, (a) => a.active);
                if (activeApp.alias === "umbInfo") {
                    loadRedirectUrls();
                    loadAuditTrail();
                }

                // never show templates for element types (if they happen to have been created in the content tree)
                scope.disableTemplates = scope.disableTemplates || scope.node.isElement;
            }

            scope.auditTrailPageChange = function (pageNumber) {
                scope.auditTrailOptions.pageNumber = pageNumber;
                loadAuditTrail(true);
            };

            scope.openDocumentType = function (documentType) {

                const variantIsDirty = _.some(scope.node.variants, function (variant) {
                    return variant.isDirty;
                });

                // add confirmation dialog before opening the doc type editor
                if (variantIsDirty) {
                    const confirm = {
                        title: labels.unsavedChanges,
                        view: "default",
                        content: labels.doctypeChangeWarning,
                        submitButtonLabelKey: "general_continue",
                        submitButtonStyle: "warning",
                        closeButtonLabelKey: "general_cancel",
                        submit: function () {
                            openDocTypeEditor(documentType);
                            overlayService.close();
                        },
                        close: function () {
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
                    submit: function (model) {
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.documentTypeEditor(editor);
            }

            scope.openTemplate = function () {
                var template = _.findWhere(scope.allTemplates, { alias: scope.node.template })
                if (!template) {
                    return;
                }
                var templateEditor = {
                    id: template.id,
                    submit: function (model) {
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.templateEditor(templateEditor);
            }

            scope.updateTemplate = function (templateAlias) {
                // update template value
                scope.node.template = templateAlias;
            };

            scope.openRollback = function () {

                var rollback = {
                    node: scope.node,
                    submit: function (model) {
                        const args = { node: scope.node };
                        eventsService.emit("editors.content.reload", args);
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.rollback(rollback);
            };

            function loadAuditTrail(forceReload) {

                //don't load this if it's already done
                if (auditTrailLoaded && !forceReload) {
                    return;
                }

                scope.loadingAuditTrail = true;

                logResource.getPagedEntityLog(scope.auditTrailOptions)
                    .then(data => {

                        // get current backoffice user and format dates
                        userService.getCurrentUser().then(currentUser => {
                            Utilities.forEach(data.items, item => {
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
                //check if Redirect URL Management is enabled
                redirectUrlsResource.getEnableState().then(response => {
                    scope.urlTrackerDisabled = response.enabled !== true;
                    if (scope.urlTrackerDisabled === false) {

                        redirectUrlsResource.getRedirectsForContentItem(scope.node.udi)
                            .then(data => {
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
                Utilities.forEach(auditTrail, item => {

                    switch (item.logType) {
                        case "Save":
                            item.logTypeColor = "primary";
                            break;
                        case "Publish":
                        case "PublishVariant":
                            item.logTypeColor = "success";
                            break;
                        case "Unpublish":
                        case "UnpublishVariant":
                            item.logTypeColor = "warning";
                            break;
                        case "Delete":
                            item.logTypeColor = "danger";
                            break;
                        default:
                            item.logTypeColor = "gray";
                    }
                });
            }

            function setNodePublishStatus() {

                scope.status = {};

                // deleted node
                if (scope.node.trashed === true) {
                    scope.status.color = "danger";
                    return;
                }

                // variant status
                if (scope.currentVariant.state === "NotCreated") {
                    // not created
                    scope.status.color = "gray";
                }
                else if (scope.currentVariant.state === "Draft") {
                    // draft node
                    scope.status.color = "gray";
                }
                else if (scope.currentVariant.state === "Published") {
                    // published node
                    scope.status.color = "success";
                }
                else if (scope.currentVariant.state === "PublishedPendingChanges") {
                    // published node with pending changes
                    scope.status.color = "success";
                }
            }

            function formatDatesToLocal() {
                // get current backoffice user and format dates
                userService.getCurrentUser().then(currentUser => {
                    scope.currentVariant.createDateFormatted = dateHelper.getLocalDate(scope.currentVariant.createDate, currentUser.locale, 'LLL');
                    scope.currentVariant.releaseDateFormatted = dateHelper.getLocalDate(scope.currentVariant.releaseDate, currentUser.locale, 'LLL');
                    scope.currentVariant.expireDateFormatted = dateHelper.getLocalDate(scope.currentVariant.expireDate, currentUser.locale, 'LLL');
                });
            }

            function updateCurrentUrls() {
                // never show URLs for element types (if they happen to have been created in the content tree)
                if (scope.node.isElement || scope.node.urls === null) {
                    scope.currentUrls = null;
                    return;
                }

                // find the urls for the currently selected language
                // when there is no selected language (allow vary by culture == false), show all urls of the node.
                scope.currentUrls = _.filter(scope.node.urls, (url) => (scope.currentVariant.language == null || scope.currentVariant.language.culture === url.culture));

                // figure out if multiple cultures apply across the content URLs
                scope.currentUrlsHaveMultipleCultures = _.keys(_.groupBy(scope.currentUrls, url => url.culture)).length > 1;
            }

            // load audit trail and redirects when on the info tab
            evts.push(eventsService.on("app.tabChange", function (event, args) {
                $timeout(function () {
                    if (args.alias === "umbInfo") {
                        isInfoTab = true;
                        loadAuditTrail();
                        loadRedirectUrls();
                        setNodePublishStatus();
                        formatDatesToLocal();
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
                    loadAuditTrail(true);
                    loadRedirectUrls();
                    setNodePublishStatus();
                    formatDatesToLocal();
                }
                updateCurrentUrls();
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
