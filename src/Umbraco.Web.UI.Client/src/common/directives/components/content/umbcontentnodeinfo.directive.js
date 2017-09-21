(function () {
    'use strict';

    function ContentNodeInfoDirective($timeout, $location, logResource, eventsService) {

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
                scope.availableTemplates = getAvailableTemplates(scope.node);

                // get document type details
                scope.documentType = getDocumentType(scope.node);

                // load audit trail on tab change
                eventsService.on("tab change", function (event, args) {
                    if (args.context.innerHTML === "Properties" && args.context.hash === "#tab0") {
                        loadAuditTrail();
                    }
                });
            }

            scope.auditTrailPageChange = function (pageNumber) {
                scope.auditTrailOptions.pageNumber = pageNumber;
                loadAuditTrail();
            };

            scope.openDocumentType = function (documentType) {
                // remove first "#" from url if it is prefixed else the path won't work
                var url = documentType.url.replace(/^#/, "");
                $location.path(url);
            };

            scope.updateTemplate = function (templateAlias) {

                // update template value
                scope.node.template = templateAlias;

                // update template value on the correct tab
                angular.forEach(scope.node.tabs, function (tab) {
                    angular.forEach(tab.properties, function (property) {
                        if (property.alias === "_umb_template") {
                            property.value = templateAlias;
                        }
                    });
                });

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
                        scope.auditTrail = data.items;
                        scope.auditTrailOptions.pageNumber = data.pageNumber;
                        scope.auditTrailOptions.pageSize = data.pageSize;
                        scope.auditTrailOptions.totalItems = data.totalItems;
                        scope.auditTrailOptions.totalPages = data.totalPages;

                        scope.loadingAuditTrail = false;
                    });

            }

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

            function getAvailableTemplates(node) {

                var availableTemplates = {};

                // find the templates in the properties array
                angular.forEach(node.properties, function (property) {
                    if (property.alias === "_umb_template") {
                        if (property.config && property.config.items) {
                            availableTemplates = property.config.items;
                        }
                    }
                });

                return availableTemplates;

            }

            function getDocumentType(node) {

                var documentType = {};

                // find the document type in the properties array
                angular.forEach(node.properties, function (property) {
                    if (property.alias === "_umb_doctype") {
                        if (property.value && property.value.length > 0) {
                            documentType = property.value[0];
                        }
                    }
                });

                return documentType;

            }

            function setPublishDate(date) {

                // update publish value
                scope.node.releaseDate = date;

                // update template value on the correct tab
                angular.forEach(scope.node.tabs, function (tab) {
                    angular.forEach(tab.properties, function (property) {
                        if (property.alias === "_umb_releasedate") {
                            property.value = date;
                        }
                    });
                });

                // emit event
                var args = { node: scope.node, date: date };
                eventsService.emit("editors.content.changePublishDate", args);

            }

            function clearPublishDate() {

                // update publish value
                scope.node.releaseDate = null;

                // update template value on the correct tab
                angular.forEach(scope.node.tabs, function (tab) {
                    angular.forEach(tab.properties, function (property) {
                        if (property.alias === "_umb_releasedate") {
                            property.value = null;
                        }
                    });
                });

                // emit event
                var args = { node: scope.node, date: null };
                eventsService.emit("editors.content.changePublishDate", args);

            }

            function setUnpublishDate(date) {

                // update publish value
                scope.node.removeDate = date;

                // update template value on the correct tab
                angular.forEach(scope.node.tabs, function (tab) {
                    angular.forEach(tab.properties, function (property) {
                        if (property.alias === "_umb_expiredate") {
                            property.value = date;
                        }
                    });
                });

                // emit event
                var args = { node: scope.node, date: date };
                eventsService.emit("editors.content.changeUnpublishDate", args);

            }

            function clearUnpublishDate() {

                // update publish value
                scope.node.removeDate = null;

                // update template value on the correct tab
                angular.forEach(scope.node.tabs, function (tab) {
                    angular.forEach(tab.properties, function (property) {
                        if (property.alias === "_umb_expiredate") {
                            property.value = null;
                        }
                    });
                });

                // emit event
                var args = { node: scope.node, date: null };
                eventsService.emit("editors.content.changeUnpublishDate", args);

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
