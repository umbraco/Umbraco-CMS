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
                scope.availableTemplates = scope.node.allowedTemplates.items;

                // get document type details
                scope.documentType = scope.node.docTypeValue[0];

                loadAuditTrail();

            }

            scope.auditTrailPageChange = function(pageNumber) {
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

            function setPublishDate(date) {

                // update publish value
                scope.node.releaseDate = date;

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
