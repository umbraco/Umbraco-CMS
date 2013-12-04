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

    $scope.options = {
        pageSize: 10,
        pageNumber: 1,
        filter: '',
        orderBy: 'SortOrder',
        orderDirection: "asc"
    };


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

    $scope.sort = function (field) {

        $scope.options.orderBy = field;


        if ($scope.options.orderDirection === "desc") {
            $scope.options.orderDirection = "asc";
        } else {
            $scope.options.orderDirection = "desc";
        }


        $scope.reloadView($scope.contentId);
    };

    $scope.prev = function () {
        if ($scope.options.pageNumber > 1) {
            $scope.options.pageNumber--;
            $scope.reloadView($scope.contentId);
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

    //assign debounce method to the search to limit the queries
    $scope.search = _.debounce(function() {
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

    if ($routeParams.id) {
        $scope.pagination = new Array(10);
        $scope.listViewAllowedTypes = contentTypeResource.getAllowedTypes($routeParams.id);
        $scope.reloadView($routeParams.id);

        $scope.contentId = $routeParams.id;

    }

}

angular.module("umbraco").controller("Umbraco.PropertyEditors.ListViewController", listViewController);