(function () {
    'use strict';

    function ContentNodeInfoDirective($timeout, $location, logResource, eventsService, userService, localizationService, dateHelper, redirectUrlsResource) {

        function link(scope, element, attrs, ctrl) {

            var evts = [];
            var isInfoTab = false;
            scope.publishStatus = {};

            scope.disableTemplates = Umbraco.Sys.ServerVariables.features.disabledFeatures.disableTemplates;

            function onInit() {
                // If logged in user has access to the settings section
                // show the open anchors - if the user doesn't have 
                // access, documentType is null, see ContentModelMapper
                scope.allowOpen = scope.node.documentType !== null;

                scope.datePickerConfig = {
                    pickDate: true,
                    pickTime: true,
                    useSeconds: false,
                    format: "YYYY-MM-DD HH:mm",
                    icons: {
                        time: "icon-time",
                        date: "icon-calendar",
                        up: "icon-chevron-up",
                        down: "icon-chevron-down"
                    }
                };

                scope.auditTrailOptions = {
                    "id": scope.node.id
                };

                // get available templates
                scope.availableTemplates = scope.node.allowedTemplates;

                // get document type details
                scope.documentType = scope.node.documentType;

                // make sure dates are formatted to the user's locale
                formatDatesToLocal();

                // Make sure to set the node status
                setNodePublishStatus(scope.node);

                //default setting for redirect url management
                scope.urlTrackerDisabled = false;

                // Declare a fallback URL for the <umb-node-preview/> directive
                if (scope.documentType !== null) {
                    scope.previewOpenUrl = '#/settings/documenttypes/edit/' + scope.documentType.id;
                }

                // only allow configuring scheduled publishing if the user has publish ("U") and unpublish ("Z") permissions on this node
                scope.allowScheduledPublishing = _.contains(scope.node.allowedActions, "U") && _.contains(scope.node.allowedActions, "Z");

                scope.node.urls = getUniqueUrls(scope.node.urls);
            }

            function getUniqueUrls(urls) {
                return urls.filter(function (a) {
                    if (!this[a]) {
                        this[a] = true;
                        return true;
                    } else {
                        return false;
                    }
                }, Object.create(null));
            }

            scope.auditTrailPageChange = function (pageNumber) {
                scope.auditTrailOptions.pageNumber = pageNumber;
                loadAuditTrail();
            };

            scope.openDocumentType = function (documentType) {
                var url = "/settings/documenttypes/edit/" + documentType.id;
                $location.url(url);
            };

            scope.openTemplate = function () {
                var url = "/settings/templates/edit/" + scope.node.templateId;
                $location.url(url);
            }

            scope.updateTemplate = function (templateAlias) {

                // update template value
                scope.node.template = templateAlias;

            };

            scope.datePickerChange = function (event, type) {
                if (type === 'publish') {
                    setPublishDate(event.date.format("YYYY-MM-DD HH:mm"));
                } else if (type === 'unpublish') {
                    setUnpublishDate(event.date.format("YYYY-MM-DD HH:mm"));
                }
            };

            scope.clearPublishDate = function () {
                clearPublishDate();
            };

            scope.clearUnpublishDate = function () {
                clearUnpublishDate();
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
                        case "UnPublish":
                        case "Delete":
                            item.logTypeColor = "danger";
                            break;
                        default:
                            item.logTypeColor = "gray";
                    }
                });
            }

            function setNodePublishStatus(node) {
                // deleted node
                if (node.trashed === true) {
                    scope.publishStatus.label = localizationService.localize("general_deleted");
                    scope.publishStatus.color = "danger";
                }

                // unpublished node
                if (node.published === false && node.trashed === false) {
                    scope.publishStatus.label = localizationService.localize("content_unpublished");
                    scope.publishStatus.color = "gray";
                }

                // published node
                if (node.hasPublishedVersion === true && node.publishDate && node.published === true) {
                    scope.publishStatus.label = localizationService.localize("content_published");
                    scope.publishStatus.color = "success";
                }

                // published node with pending changes
                if (node.hasPublishedVersion === true && node.publishDate && node.published === false) {
                    scope.publishStatus.label = localizationService.localize("content_publishedPendingChanges");
                    scope.publishStatus.color = "success";
                }

            }

            function setPublishDate(date) {

                if (!date) {
                    return;
                }

                //The date being passed in here is the user's local date/time that they have selected
                //we need to convert this date back to the server date on the model.

                var serverTime = dateHelper.convertToServerStringTime(moment(date), Umbraco.Sys.ServerVariables.application.serverTimeOffset);

                // update publish value
                scope.node.releaseDate = serverTime;

                // make sure dates are formatted to the user's locale
                formatDatesToLocal();

                // emit event
                var args = { node: scope.node, date: date };
                eventsService.emit("editors.content.changePublishDate", args);

            }

            function clearPublishDate() {

                // update publish value
                scope.node.releaseDate = null;

                // emit event
                var args = { node: scope.node, date: null };
                eventsService.emit("editors.content.changePublishDate", args);

            }

            function setUnpublishDate(date) {

                if (!date) {
                    return;
                }

                //The date being passed in here is the user's local date/time that they have selected
                //we need to convert this date back to the server date on the model.

                var serverTime = dateHelper.convertToServerStringTime(moment(date), Umbraco.Sys.ServerVariables.application.serverTimeOffset);

                // update publish value
                scope.node.removeDate = serverTime;

                // make sure dates are formatted to the user's locale
                formatDatesToLocal();

                // emit event
                var args = { node: scope.node, date: date };
                eventsService.emit("editors.content.changeUnpublishDate", args);

            }

            function clearUnpublishDate() {

                // update publish value
                scope.node.removeDate = null;

                // emit event
                var args = { node: scope.node, date: null };
                eventsService.emit("editors.content.changeUnpublishDate", args);

            }

            function ucfirst(string) {
                return string.charAt(0).toUpperCase() + string.slice(1);
            }

            function formatDatesToLocal() {
                // get current backoffice user and format dates
                userService.getCurrentUser().then(function (currentUser) {
                    scope.node.createDateFormatted = dateHelper.getLocalDate(scope.node.createDate, currentUser.locale, 'LLL');

                    scope.node.releaseDateYear = scope.node.releaseDate ? ucfirst(dateHelper.getLocalDate(scope.node.releaseDate, currentUser.locale, 'YYYY')) : null;
                    scope.node.releaseDateMonth = scope.node.releaseDate ? ucfirst(dateHelper.getLocalDate(scope.node.releaseDate, currentUser.locale, 'MMMM')) : null;
                    scope.node.releaseDateDayNumber = scope.node.releaseDate ? ucfirst(dateHelper.getLocalDate(scope.node.releaseDate, currentUser.locale, 'DD')) : null;
                    scope.node.releaseDateDay = scope.node.releaseDate ? ucfirst(dateHelper.getLocalDate(scope.node.releaseDate, currentUser.locale, 'dddd')) : null;
                    scope.node.releaseDateTime = scope.node.releaseDate ? ucfirst(dateHelper.getLocalDate(scope.node.releaseDate, currentUser.locale, 'HH:mm')) : null;

                    scope.node.removeDateYear = scope.node.removeDate ? ucfirst(dateHelper.getLocalDate(scope.node.removeDate, currentUser.locale, 'YYYY')) : null;
                    scope.node.removeDateMonth = scope.node.removeDate ? ucfirst(dateHelper.getLocalDate(scope.node.removeDate, currentUser.locale, 'MMMM')) : null;
                    scope.node.removeDateDayNumber = scope.node.removeDate ? ucfirst(dateHelper.getLocalDate(scope.node.removeDate, currentUser.locale, 'DD')) : null;
                    scope.node.removeDateDay = scope.node.removeDate ? ucfirst(dateHelper.getLocalDate(scope.node.removeDate, currentUser.locale, 'dddd')) : null;
                    scope.node.removeDateTime = scope.node.removeDate ? ucfirst(dateHelper.getLocalDate(scope.node.removeDate, currentUser.locale, 'HH:mm')) : null;
                });
            }

            // load audit trail and redirects when on the info tab
            evts.push(eventsService.on("app.tabChange", function (event, args) {
                $timeout(function () {
                    if (args.id === -1) {
                        isInfoTab = true;
                        loadAuditTrail();
                        loadRedirectUrls();
                    } else {
                        isInfoTab = false;
                    }
                });
            }));

            // watch for content updates - reload content when node is saved, published etc.
            scope.$watch('node.updateDate', function (newValue, oldValue) {

                if (!newValue) { return; }
                if (newValue === oldValue) { return; }

                if (isInfoTab) {
                    loadAuditTrail();
                    loadRedirectUrls();
                    formatDatesToLocal();
                    setNodePublishStatus(scope.node);
                    scope.node.urls = getUniqueUrls(scope.node.urls);
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
