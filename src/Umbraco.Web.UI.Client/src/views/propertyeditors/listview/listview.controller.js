function listViewController($rootScope, $scope, $routeParams, $injector, notificationsService, iconHelper, dialogService, editorState, localizationService, $location, appState, $timeout, $q) {

    //this is a quick check to see if we're in create mode, if so just exit - we cannot show children for content 
    // that isn't created yet, if we continue this will use the parent id in the route params which isn't what
    // we want. NOTE: This is just a safety check since when we scaffold an empty model on the server we remove
    // the list view tab entirely when it's new.
    if ($routeParams.create) {
        $scope.isNew = true;
        return;
    }

    //Now we need to check if this is for media, members or content because that will depend on the resources we use
    var contentResource, getContentTypesCallback, getListResultsCallback, deleteItemCallback, getIdCallback, createEditUrlCallback;
    
    //check the config for the entity type, or the current section name (since the config is only set in c#, not in pre-vals)
    if (($scope.model.config.entityType && $scope.model.config.entityType === "member") || (appState.getSectionState("currentSection") === "member")) {
        $scope.entityType = "member";
        contentResource = $injector.get('memberResource');
        getContentTypesCallback = $injector.get('memberTypeResource').getTypes;
        getListResultsCallback = contentResource.getPagedResults;
        deleteItemCallback = contentResource.deleteByKey;
        getIdCallback = function(selected) {
            return selected.key;
        };
        createEditUrlCallback = function(item) {
            return "/" + $scope.entityType + "/" + $scope.entityType + "/edit/" + item.key + "?page=" + $scope.options.pageNumber + "&listName=" + $scope.contentId;
        };
    }
    else {
        //check the config for the entity type, or the current section name (since the config is only set in c#, not in pre-vals)
        if (($scope.model.config.entityType && $scope.model.config.entityType === "media") || (appState.getSectionState("currentSection") === "media")) {
            $scope.entityType = "media";
            contentResource = $injector.get('mediaResource');
            getContentTypesCallback = $injector.get('mediaTypeResource').getAllowedTypes;                        
        }
        else {
            $scope.entityType = "content";
            contentResource = $injector.get('contentResource');
            getContentTypesCallback = $injector.get('contentTypeResource').getAllowedTypes;            
        }
        getListResultsCallback = contentResource.getChildren;
        deleteItemCallback = contentResource.deleteById;
        getIdCallback = function(selected) {
            return selected.id;
        };
        createEditUrlCallback = function(item) {
            return "/" + $scope.entityType + "/" + $scope.entityType + "/edit/" + item.id + "?page=" + $scope.options.pageNumber;
        };
    }

    $scope.pagination = [];
    $scope.isNew = false;
    $scope.actionInProgress = false;
    $scope.listViewResultSet = {
        totalPages: 0,
        items: []
    };

    $scope.options = {
        pageSize: $scope.model.config.pageSize ? $scope.model.config.pageSize : 10,
        pageNumber: ($routeParams.page && Number($routeParams.page) != NaN && Number($routeParams.page) > 0) ? $routeParams.page : 1,
        filter: '',
        orderBy: ($scope.model.config.orderBy ? $scope.model.config.orderBy : 'VersionDate').trim(),
        orderDirection: $scope.model.config.orderDirection ? $scope.model.config.orderDirection.trim() : "desc",
        includeProperties: $scope.model.config.includeProperties ? $scope.model.config.includeProperties : [
            { alias: 'updateDate', header: 'Last edited', isSystem: 1 },
            { alias: 'updater', header: 'Last edited by', isSystem: 1 }
        ],
        allowBulkPublish: true,
        allowBulkUnpublish: true,
        allowBulkDelete: true,        
    };

    //update all of the system includeProperties to enable sorting
    _.each($scope.options.includeProperties, function(e, i) {
        
        if (e.isSystem) {

            //NOTE: special case for contentTypeAlias, it's a system property that cannot be sorted
            // to do that, we'd need to update the base query for content to include the content type alias column
            // which requires another join and would be slower. BUT We are doing this for members so not sure it makes a diff?
            if (e.alias != "contentTypeAlias") {
                e.allowSorting = true;
            }
            
            //localize the header
            var key = getLocalizedKey(e.alias);
            localizationService.localize(key).then(function (v) {
                e.header = v;
            });
        }
    });

    function showNotificationsAndReset(err, reload, successMsg) {

        //check if response is ysod
        if(err.status && err.status >= 500) {
            dialogService.ysodDialog(err);
        }

        $timeout(function() {
            $scope.bulkStatus = "";
            $scope.actionInProgress = false;
        }, 500);

        if (reload === true) {
            $scope.reloadView($scope.contentId);
        }

        if (err.data && angular.isArray(err.data.notifications)) {
            for (var i = 0; i < err.data.notifications.length; i++) {
                notificationsService.showNotification(err.data.notifications[i]);
            }
        }
        else if (successMsg) {
            notificationsService.success("Done", successMsg);
        }
    }

    $scope.isSortDirection = function (col, direction) {
        return $scope.options.orderBy.toUpperCase() == col.toUpperCase() && $scope.options.orderDirection == direction;
    }

    $scope.next = function() {
        if ($scope.options.pageNumber < $scope.listViewResultSet.totalPages) {
            $scope.options.pageNumber++;
            $scope.reloadView($scope.contentId);
            //TODO: this would be nice but causes the whole view to reload
            //$location.search("page", $scope.options.pageNumber);
        }
    };

    $scope.goToPage = function(pageNumber) {
        $scope.options.pageNumber = pageNumber + 1;
        $scope.reloadView($scope.contentId);
        //TODO: this would be nice but causes the whole view to reload
        //$location.search("page", $scope.options.pageNumber);
    };

    $scope.sort = function(field, allow) {
        if (allow) {
            $scope.options.orderBy = field;

            if ($scope.options.orderDirection === "desc") {
                $scope.options.orderDirection = "asc";
            }
            else {
                $scope.options.orderDirection = "desc";
            }

            $scope.reloadView($scope.contentId);
        }
    };

    $scope.prev = function() {
        if ($scope.options.pageNumber > 1) {
            $scope.options.pageNumber--;
            $scope.reloadView($scope.contentId);
            //TODO: this would be nice but causes the whole view to reload
            //$location.search("page", $scope.options.pageNumber);
        }
    };
    

    /*Loads the search results, based on parameters set in prev,next,sort and so on*/
    /*Pagination is done by an array of objects, due angularJS's funky way of monitoring state
    with simple values */

    $scope.reloadView = function (id) {

        getListResultsCallback(id, $scope.options).then(function (data) {

            $scope.actionInProgress = false;

            $scope.listViewResultSet = data;

            //update all values for display
            if ($scope.listViewResultSet.items) {
                _.each($scope.listViewResultSet.items, function (e, index) {
                    setPropertyValues(e);
                });
            }

            //NOTE: This might occur if we are requesting a higher page number than what is actually available, for example
            // if you have more than one page and you delete all items on the last page. In this case, we need to reset to the last
            // available page and then re-load again
            if ($scope.options.pageNumber > $scope.listViewResultSet.totalPages) {
                $scope.options.pageNumber = $scope.listViewResultSet.totalPages;

                //reload!
                $scope.reloadView(id);
            }

            $scope.pagination = [];

            //list 10 pages as per normal
            if ($scope.listViewResultSet.totalPages <= 10) {
                for (var i = 0; i < $scope.listViewResultSet.totalPages; i++) {
                    $scope.pagination.push({
                        val: (i + 1),
                        isActive: $scope.options.pageNumber == (i + 1)
                    });
                }
            }
            else {
                //if there is more than 10 pages, we need to do some fancy bits

                //get the max index to start
                var maxIndex = $scope.listViewResultSet.totalPages - 10;
                //set the start, but it can't be below zero
                var start = Math.max($scope.options.pageNumber - 5, 0);
                //ensure that it's not too far either
                start = Math.min(maxIndex, start);

                for (var i = start; i < (10 + start) ; i++) {
                    $scope.pagination.push({
                        val: (i + 1),
                        isActive: $scope.options.pageNumber == (i + 1)
                    });
                }

                //now, if the start is greater than 0 then '1' will not be displayed, so do the elipses thing
                if (start > 0) {
                    $scope.pagination.unshift({ name: "First", val: 1, isActive: false }, {val: "...",isActive: false});
                }

                //same for the end
                if (start < maxIndex) {
                    $scope.pagination.push({ val: "...", isActive: false }, { name: "Last", val: $scope.listViewResultSet.totalPages, isActive: false });
                }
            }

        });
    };

    $scope.$watch(function() {
        return $scope.options.filter;
    }, _.debounce(function(newVal, oldVal) {
        $scope.$apply(function() {
            if (newVal !== null && newVal !== undefined && newVal !== oldVal) {
                $scope.options.pageNumber = 1;
                $scope.actionInProgress = true;
                $scope.reloadView($scope.contentId);
            }
        });
    }, 1000));

    $scope.filterResults = function (ev) {
        //13: enter

        switch (ev.keyCode) {
            case 13:
                $scope.options.pageNumber = 1;
                $scope.actionInProgress = true;
                $scope.reloadView($scope.contentId);
                break;
        }
    };

    $scope.enterSearch = function ($event) {
        $($event.target).next().focus();
    };

    $scope.selectAll = function($event) {
        var checkbox = $event.target;
        if (!angular.isArray($scope.listViewResultSet.items)) {
            return;
        }
        for (var i = 0; i < $scope.listViewResultSet.items.length; i++) {
            var entity = $scope.listViewResultSet.items[i];
            entity.selected = checkbox.checked;
        }
    };

    $scope.isSelectedAll = function() {
        if (!angular.isArray($scope.listViewResultSet.items)) {
            return false;
        }
        return _.every($scope.listViewResultSet.items, function(item) {
            return item.selected;
        });
    };

    $scope.isAnythingSelected = function() {
        if (!angular.isArray($scope.listViewResultSet.items)) {
            return false;
        }
        return _.some($scope.listViewResultSet.items, function(item) {
            return item.selected;
        });
    };

    $scope.getIcon = function(entry) {
        return iconHelper.convertFromLegacyIcon(entry.icon);
    };

    function serial(selected, fn, getStatusMsg, index) {
        return fn(selected, index).then(function (content) {
            index++;
            $scope.bulkStatus = getStatusMsg(index, selected.length);
            return index < selected.length ? serial(selected, fn, getStatusMsg, index) : content;
        }, function (err) {
            var reload = index > 0;
            showNotificationsAndReset(err, reload);
            return err;
        });
    }

    function applySelected(fn, getStatusMsg, getSuccessMsg, confirmMsg) {
        var selected = _.filter($scope.listViewResultSet.items, function (item) {
            return item.selected;
        });
        if (selected.length === 0)
            return;
        if (confirmMsg && !confirm(confirmMsg))
            return;

        $scope.actionInProgress = true;
        $scope.bulkStatus = getStatusMsg(0, selected.length);

        serial(selected, fn, getStatusMsg, 0).then(function (result) {
            // executes once the whole selection has been processed
            // in case of an error (caught by serial), result will be the error
            if (!(result.data && angular.isArray(result.data.notifications)))
                showNotificationsAndReset(result, true, getSuccessMsg(selected.length));
        });
    };

    $scope.delete = function () {
        applySelected(
            function (selected, index) { return deleteItemCallback(getIdCallback(selected[index])) },
            function (count, total) { return "Deleted " + count + " out of " + total + " document" + (total > 1 ? "s" : "") },
            function (total) { return "Deleted " + total + " document" + (total > 1 ? "s" : "") },
            "Sure you want to delete?");
    };

    $scope.publish = function () {
        applySelected(
            function (selected, index) { return contentResource.publishById(getIdCallback(selected[index])); },
            function (count, total) { return "Published " + count + " out of " + total + " document" + (total > 1 ? "s" : "") },
            function (total) { return "Published " + total + " document" + (total > 1 ? "s" : "") });
    };

    $scope.unpublish = function() {
        applySelected(
            function (selected, index) { return contentResource.unPublish(getIdCallback(selected[index])); },
            function (count, total) { return "Unpublished " + count + " out of " + total + " document" + (total > 1 ? "s" : "") },
            function (total) { return "Unpublished " + total + " document" + (total > 1 ? "s" : "") });
    };

    function getCustomPropertyValue(alias, properties) {
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

    /** This ensures that the correct value is set for each item in a row, we don't want to call a function during interpolation or ng-bind as performance is really bad that way */
    function setPropertyValues(result) {

        //set the edit url
        result.editPath = createEditUrlCallback(result);

        _.each($scope.options.includeProperties, function (e, i) {

            var alias = e.alias;

            // First try to pull the value directly from the alias (e.g. updatedBy)        
            var value = result[alias];
            
            // If this returns an object, look for the name property of that (e.g. owner.name)
            if (value === Object(value)) {
                value = value['name'];
            }

            // If we've got nothing yet, look at a user defined property
            if (typeof value === 'undefined') {
                value = getCustomPropertyValue(alias, result.properties);
            }

            // If we have a date, format it
            if (isDate(value)) {
                value = value.substring(0, value.length - 3);
            }

            // set what we've got on the result
            result[alias] = value;
        });


    };

    function isDate(val) {
        if (angular.isString(val)) {
            return val.match(/^(\d{4})\-(\d{2})\-(\d{2})\ (\d{2})\:(\d{2})\:(\d{2})$/);
        }
        return false;
    };

    function initView() {
        if ($routeParams.id) {
            $scope.listViewAllowedTypes = getContentTypesCallback($routeParams.id);

            $scope.contentId = $routeParams.id;
            $scope.isTrashed = $routeParams.id === "-20" || $routeParams.id === "-21";

            $scope.reloadView($scope.contentId);
        }
    };

    function getLocalizedKey(alias) {

        switch (alias) {
            case "sortOrder":
                return "general_sort";
            case "updateDate":
                return "content_updateDate";
            case "updater":
                return "content_updatedBy";
            case "createDate":
                return "content_createDate";
            case "owner":
                return "content_createBy";
            case "published":
                return "content_isPublished";
            case "contentTypeAlias":
                //TODO: Check for members
                return $scope.entityType === "content" ? "content_documentType" : "content_mediatype";
            case "email":
                return "general_email";
            case "username":
                return "general_username";
        }
        return alias;
    }

    //GO!
    initView();
}


angular.module("umbraco").controller("Umbraco.PropertyEditors.ListViewController", listViewController);