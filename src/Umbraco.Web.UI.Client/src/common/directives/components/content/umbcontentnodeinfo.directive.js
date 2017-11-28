(function () {
    'use strict';

    function ContentNodeInfoDirective($timeout, $location, logResource, eventsService, userService, localizationService) {

        function link(scope, element, attrs, ctrl) {

            var evts = [];
            var isInfoTab = false;
            scope.publishStatus = {};
            
            function onInit() {

                scope.allowOpen = true;

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

                setNodePublishStatus(scope.node);
                
            }

            scope.auditTrailPageChange = function (pageNumber) {
                scope.auditTrailOptions.pageNumber = pageNumber;
                loadAuditTrail();
            };

            scope.openDocumentType = function (documentType) {               
                var url = "/settings/documenttypes/edit/" + documentType.id;
                $location.path(url);
            };

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
                            angular.forEach(data.items, function(item) {
                                item.timestampFormatted = getLocalDate(item.timestamp, currentUser.locale, 'LLL');
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
                if(node.trashed === true) {
                    scope.publishStatus.label = localizationService.localize("general_deleted");
                    scope.publishStatus.color = "danger";
                }

                // unpublished node
                if(node.published === false && node.trashed === false) {
                    scope.publishStatus.label = localizationService.localize("content_unpublished");
                    scope.publishStatus.color = "gray";
                }

                // published node
                if(node.hasPublishedVersion === true && node.publishDate && node.published === true) {
                    scope.publishStatus.label = localizationService.localize("content_published");
                    scope.publishStatus.color = "success";
                }

                // published node with pending changes
                if(node.hasPublishedVersion === true && node.publishDate && node.published === false) {
                    scope.publishStatus.label = localizationService.localize("content_publishedPendingChanges");
                    scope.publishStatus.color = "success"
                }

            }

            function setPublishDate(date) {

                // update publish value
                scope.node.releaseDate = date;

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

                // update publish value
                scope.node.removeDate = date;

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

            function getLocalDate(date, culture, format) {
                if (date) {
                    var dateVal;
                    var serverOffset = Umbraco.Sys.ServerVariables.application.serverTimeOffset;
                    var localOffset = new Date().getTimezoneOffset();
                    var serverTimeNeedsOffsetting = -serverOffset !== localOffset;
                    if (serverTimeNeedsOffsetting) {
                        dateVal = dateHelper.convertToLocalMomentTime(date, serverOffset);
                    } else {
                        dateVal = moment(date, 'YYYY-MM-DD HH:mm:ss');
                    }
                    return dateVal.locale(culture).format(format);
                }
            }

            function formatDatesToLocal() {
                // get current backoffice user and format dates
                userService.getCurrentUser().then(function (currentUser) {
                    scope.node.createDateFormatted = getLocalDate(scope.node.createDate, currentUser.locale, 'LLL');
                    scope.node.releaseDateMonth = scope.node.releaseDate ? ucfirst(getLocalDate(scope.node.releaseDate, currentUser.locale, 'MMMM')) : null;
                    scope.node.releaseDateDay = scope.node.releaseDate ? ucfirst(getLocalDate(scope.node.releaseDate, currentUser.locale, 'dddd')) : null;
                    scope.node.removeDateMonth = scope.node.removeDate ? ucfirst(getLocalDate(scope.node.removeDate, currentUser.locale, 'MMMM')) : null;
                    scope.node.removeDateDay = scope.node.removeDate ? ucfirst(getLocalDate(scope.node.removeDate, currentUser.locale, 'dddd')) : null;
                });
            }

            // load audit trail when on the info tab
            evts.push(eventsService.on("app.tabChange", function (event, args) {
                $timeout(function(){
                    if (args.id === -1) {
                        isInfoTab = true;
                        loadAuditTrail();
                    } else {
                        isInfoTab = false;
                    }
                });
            }));

            // watch for content updates - reload content when node is saved, published etc.
            scope.$watch('node.updateDate', function(newValue, oldValue){

                if(!newValue) { return; }
                if(newValue === oldValue) { return; }             
                
                if(isInfoTab) {
                    loadAuditTrail();
                    formatDatesToLocal();
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
