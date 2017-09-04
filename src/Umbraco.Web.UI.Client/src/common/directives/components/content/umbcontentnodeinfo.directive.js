(function () {
    'use strict';

    function ContentNodeInfoDirective($timeout) {

        function link(scope, element, attrs, ctrl) {

            function onInit() {

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

                scope.auditTrail = [
                    {
                        "date": "03 December 2016 17:58PM",
                        "action": "publish",
                        "description": "Content was performed today by user",
                        "user": {
                            "name": "Zsolt Laszlo",
                            "avatars": []
                        }
                    },
                    {
                        "date": "24 December 2016 20:18PM",
                        "action": "unpublish",
                        "description": "Content was performed by user",
                        "user": {
                            "name": "Mads Rasmussen",
                            "avatars": [
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=30",
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=60",
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=90",
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=150",
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=300"
                            ]
                        }
                    },
                    {
                        "date": "19 November 2016 21:11AM",
                        "action": "save",
                        "description": "Content was performed yesteraday by user",
                        "user": {
                            "name": "Zsolt Laszlo",
                            "avatars": []
                        }
                    },
                    {
                        "date": "10 November 2016 10:41AM",
                        "action": "save",
                        "description": "Content was performed last week by user",
                        "user": {
                            "name": "Mads Rasmussen",
                            "avatars": [
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=30",
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=60",
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=90",
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=150",
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=300"
                            ]
                        }
                    },
                    {
                        "date": "02 November 2016 03:44PM",
                        "action": "save",
                        "description": "Content was performed last week by user",
                        "user": {
                            "name": "Zsolt Laszlo",
                            "avatars": []
                        }
                    },
                    {
                        "date": "19 September 2016 18:21AM",
                        "action": "publish",
                        "description": "Content was performed two weeks ago by user",
                        "user": {
                            "name": "Mads Rasmussen",
                            "avatars": [
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=30",
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=60",
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=90",
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=150",
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=300"
                            ]
                        }
                    },
                    {
                        "date": "19 September 2016 08:51AM",
                        "action": "save",
                        "description": "Content was performed last month by user",
                        "user": {
                            "name": "Mads Rasmussen",
                            "avatars": [
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=30",
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=60",
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=90",
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=150",
                                "https://www.gravatar.com/avatar/bc196379513a5efe165b9e1571b8d5a8?d=404&s=300"
                            ]
                        }
                    },
                    {
                        "date": "11 September 2016 13:28AM",
                        "action": "save",
                        "description": "Content was performed by user",
                        "user": {
                            "name": "Zsolt Laszlo",
                            "avatars": []
                        }
                    },
                    {
                        "date": "01 September 2016 23:19AM",
                        "action": "save",
                        "description": "Content was performed by user",
                        "user": {
                            "name": "Zsolt Laszlo",
                            "avatars": []
                        }
                    }
                ];

                scope.pagination = {
                    "pageNumber": 1,
                    "totalPages": 5
                };

                scope.template = {
                    "description": "Description for the template section"
                };

                scope.idSection = {
                    "name": "Id"
                };

                // get available templates
                scope.availableTemplates = getAvailableTemplates(scope.node);

                // get document type details
                scope.documentType = getDocumentType(scope.node);

                // get the auditTrail - fake loading
                scope.loadingAuditTrail = true;
                $timeout(function () {
                    setAuditTrailActionColor(scope.auditTrail);
                    scope.loadingAuditTrail = false;
                }, 2000);

            }

            scope.nextPage = function (pageNumber) {
                alert("next page" + pageNumber);
            };

            scope.prevPage = function (pageNumber) {
                alert("previous page" + pageNumber);
            };

            scope.goToPage = function (pageNumber) {
                alert("go to page" + pageNumber);
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
