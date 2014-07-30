function listViewController($rootScope, $scope, $routeParams, $injector, notificationsService, iconHelper, dialogService) {

    //this is a quick check to see if we're in create mode, if so just exit - we cannot show children for content 
    // that isn't created yet, if we continue this will use the parent id in the route params which isn't what
    // we want. NOTE: This is just a safety check since when we scaffold an empty model on the server we remove
    // the list view tab entirely when it's new.
    if ($routeParams.create) {
        $scope.isNew = true;
        return;
    }

    //Now we need to check if this is for media or content because that will depend on the resources we use
    var contentResource, contentTypeResource;
    if ($scope.model.config.entityType && $scope.model.config.entityType === "media") {
        contentResource = $injector.get('mediaResource');
        contentTypeResource = $injector.get('mediaTypeResource');
        $scope.entityType = "media";
    }
    else {
        contentResource = $injector.get('contentResource');
        contentTypeResource = $injector.get('contentTypeResource');
        $scope.entityType = "content";
    }

    $scope.isNew = false;
    $scope.actionInProgress = false;
    $scope.listViewResultSet = {
        totalPages: 0,
        items: []
    };

    // Set "default default" options (i.e. those if no container configuration has been saved)
    $scope.options = {
        pageSize: 10,
        pageNumber: 1,
        filter: '',
        orderBy: 'updateDate',
        orderDirection: "desc",
        allowBulkPublish: true,
        allowBulkUnpublish: true,
        allowBulkDelete: true,
        additionalColumns: [
                { alias: 'UpdateDate', header: 'Last edited', localizationKey: 'defaultdialogs_lastEdited' },
                { alias: 'Updator', header: 'Last edited', localizationKey: 'content_updatedBy' }
            ]
    };

    // Retrieve the container configuration for the content type and set options before presenting initial view
    contentTypeResource.getContainerConfig($routeParams.id)
        .then(function (config) {
            if (typeof config.pageSize !== 'undefined') {
                $scope.options.pageSize = config.pageSize;
            }
            if (typeof config.additionalColumns !== 'undefined') {
                $scope.options.additionalColumns = config.additionalColumns;
            }
            if (typeof config.orderBy !== 'undefined') {
                $scope.options.orderBy = config.orderBy;
            }
            if (typeof config.orderDirection !== 'undefined') {
                $scope.options.orderDirection = config.orderDirection;
            }
            if (typeof config.allowBulkPublish !== 'undefined') {
                $scope.options.allowBulkPublish = config.allowBulkPublish;
            }
            if (typeof config.allowBulkUnpublish !== 'undefined') {
                $scope.options.allowBulkUnpublish = config.allowBulkUnpublish;
            }
            if (typeof config.allowBulkDelete !== 'undefined') {
                $scope.options.allowBulkDelete = config.allowBulkDelete;
            }

            $scope.initView();
        });

    $scope.next = function () {
        if ($scope.options.pageNumber < $scope.listViewResultSet.totalPages) {
            $scope.options.pageNumber++;
            $scope.reloadView($scope.contentId);
        }
    };

    $scope.goToPage = function (pageNumber) {
        $scope.options.pageNumber = pageNumber + 1;
        $scope.reloadView($scope.contentId);
    };

    $scope.sort = function (field, allow) {
        if (allow) {
            $scope.options.orderBy = field;

            if ($scope.options.orderDirection === "desc") {
                $scope.options.orderDirection = "asc";
            } else {
                $scope.options.orderDirection = "desc";
            }

            $scope.reloadView($scope.contentId);
        }
    };

    $scope.prev = function () {
        if ($scope.options.pageNumber > 1) {
            $scope.options.pageNumber--;
            $scope.reloadView($scope.contentId);
        }
    };

    $scope.initView = function () {
        if ($routeParams.id) {
            $scope.pagination = new Array(10);
            $scope.listViewAllowedTypes = contentTypeResource.getAllowedTypes($routeParams.id);
            $scope.reloadView($routeParams.id);

            $scope.contentId = $routeParams.id;
            $scope.isTrashed = $routeParams.id === "-20" || $routeParams.id === "-21";
        }
    };

    /*Loads the search results, based on parameters set in prev,next,sort and so on*/
    /*Pagination is done by an array of objects, due angularJS's funky way of monitoring state
    with simple values */

    $scope.reloadView = function (id) {
        contentResource.getChildren(id, $scope.options).then(function (data) {
            $scope.listViewResultSet = data;
            $scope.pagination = [];

            for (var i = $scope.listViewResultSet.totalPages - 1; i >= 0; i--) {
                $scope.pagination[i] = { index: i, name: i + 1 };
            }

            if ($scope.options.pageNumber > $scope.listViewResultSet.totalPages) {
                $scope.options.pageNumber = $scope.listViewResultSet.totalPages;
            }

        });
    };

    $scope.getColumnName = function (index) {
        return $scope.options.additionalColumns[index].header;
    };

    $scope.getColumnLocalizationKey = function (index) {
        return $scope.options.additionalColumns[index].localizationKey;
    };

    $scope.getPropertyValue = function (alias, result) {

        // Camel-case the alias
        alias = alias.charAt(0).toLowerCase() + alias.slice(1);

        // First try to pull the value directly from the alias (e.g. updatedBy)        
        var value = result[alias];

        // If this returns an object, look for the name property of that (e.g. owner.name)
        if (value === Object(value)) {
            value = value['name'];
        }

        // If we've got nothing yet, look at a user defined property
        if (typeof value === 'undefined') {
            value = $scope.getCustomPropertyValue(alias, result.properties);
        }

        // If we have a date, format it
        if (isDate(value)) {
            value = value.substring(0, value.length - 3);
        }

        // Return what we've got
        return value;

    };

    isDate = function (val) {
        return val.match(/^(\d{4})\-(\d{2})\-(\d{2})\ (\d{2})\:(\d{2})\:(\d{2})$/);
    };

    $scope.getCustomPropertyValue = function (alias, properties) {
        var value = '';
        var index = 0;
        var foundAlias = false;
        for (var i = 0; i < properties.length; i++) {            
            if (properties[i].alias == alias) {
                foundAlias = true;
                break;
            }
            index++;
        }

        if (foundAlias) {
            value = properties[index].value;
        }

        return value;
    };

    //assign debounce method to the search to limit the queries
    $scope.search = _.debounce(function () {
        $scope.options.pageNumber = 1;
        $scope.reloadView($scope.contentId);
    }, 100);

    $scope.selectAll = function ($event) {
        var checkbox = $event.target;
        if (!angular.isArray($scope.listViewResultSet.items)) {
            return;
        }
        for (var i = 0; i < $scope.listViewResultSet.items.length; i++) {
            var entity = $scope.listViewResultSet.items[i];
            entity.selected = checkbox.checked;
        }
    };

    $scope.isSelectedAll = function () {
        if (!angular.isArray($scope.listViewResultSet.items)) {
            return false;
        }
        return _.every($scope.listViewResultSet.items, function (item) {
            return item.selected;
        });
    };

    $scope.isAnythingSelected = function () {
        if (!angular.isArray($scope.listViewResultSet.items)) {
            return false;
        }
        return _.some($scope.listViewResultSet.items, function (item) {
            return item.selected;
        });
    };

    $scope.getIcon = function (entry) {
        return iconHelper.convertFromLegacyIcon(entry.icon);
    };

    $scope.delete = function () {
        var selected = _.filter($scope.listViewResultSet.items, function (item) {
            return item.selected;
        });
        var total = selected.length;
        if (total === 0) {
            return;
        }

        if (confirm("Sure you want to delete?") == true) {
            $scope.actionInProgress = true;
            $scope.bulkStatus = "Starting with delete";
            var current = 1;

            for (var i = 0; i < selected.length; i++) {
                $scope.bulkStatus = "Deleted doc " + current + " out of " + total + " documents";
                contentResource.deleteById(selected[i].id).then(function (data) {
                    if (current === total) {
                        notificationsService.success("Bulk action", "Deleted " + total + "documents");
                        $scope.bulkStatus = "";
                        $scope.reloadView($scope.contentId);
                        $scope.actionInProgress = false;
                    }
                    current++;
                });
            }
        }

    };

    $scope.publish = function () {
        var selected = _.filter($scope.listViewResultSet.items, function (item) {
            return item.selected;
        });
        var total = selected.length;
        if (total === 0) {
            return;
        }

        $scope.actionInProgress = true;
        $scope.bulkStatus = "Starting with publish";
        var current = 1;

        for (var i = 0; i < selected.length; i++) {
            $scope.bulkStatus = "Publishing " + current + " out of " + total + " documents";

            contentResource.publishById(selected[i].id)
                .then(function (content) {
                    if (current == total) {
                        notificationsService.success("Bulk action", "Published " + total + "documents");
                        $scope.bulkStatus = "";
                        $scope.reloadView($scope.contentId);
                        $scope.actionInProgress = false;
                    }
                    current++;
                }, function (err) {

                    $scope.bulkStatus = "";
                    $scope.reloadView($scope.contentId);
                    $scope.actionInProgress = false;

                    //if there are validation errors for publishing then we need to show them
                    if (err.status === 400 && err.data && err.data.Message) {
                        notificationsService.error("Publish error", err.data.Message);
                    }
                    else {
                        dialogService.ysodDialog(err);
                    }
                });

        }
    };

    $scope.unpublish = function () {
        var selected = _.filter($scope.listViewResultSet.items, function (item) {
            return item.selected;
        });
        var total = selected.length;
        if (total === 0) {
            return;
        }

        $scope.actionInProgress = true;
        $scope.bulkStatus = "Starting with publish";
        var current = 1;

        for (var i = 0; i < selected.length; i++) {
            $scope.bulkStatus = "Unpublishing " + current + " out of " + total + " documents";

            contentResource.unPublish(selected[i].id)
                .then(function (content) {

                    if (current == total) {
                        notificationsService.success("Bulk action", "Published " + total + "documents");
                        $scope.bulkStatus = "";
                        $scope.reloadView($scope.contentId);
                        $scope.actionInProgress = false;
                    }

                    current++;
                });
        }
    };

}

angular.module("umbraco").controller("Umbraco.PropertyEditors.ListViewController", listViewController);