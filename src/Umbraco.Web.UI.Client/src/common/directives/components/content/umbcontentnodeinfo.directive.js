(function () {
    'use strict';

    function ContentNodeInfoDirective($timeout, $location, logResource, eventsService, userService) {

        function link(scope, element, attrs, ctrl) {
            
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

                // load audit trail when on the info tab
                eventsService.on("app.tabChange", function (event, args) {
                    if (args.id === -1) {
                        loadAuditTrail();
                    }
                });

                // make sure dates are formatted to the user's locale
                formatDatesToLocal();
                
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
                            item.logTypeColor = "danger";
                            break;
                        default:
                            item.logTypeColor = "gray";
                    }
                });
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
