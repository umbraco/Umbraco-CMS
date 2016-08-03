angular.module("umbraco").controller("Umbraco.RedirectUrlSearch", function ($scope, $http, angularHelper, notificationsService, entityResource, $routeParams) {
    //...todo
    //search by url or url part
    //search by domain
    //display domain in dashboard results?

    $scope.isSearch = false;
    $scope.hasResults = false;

    $scope.pagination = [];
    $scope.isNew = false;
    $scope.actionInProgress = false;
    $scope.listViewResultSet = {
        totalPages: 0,
        items: []
    };

    $scope.options = {
        pageSize: 20,
        pageNumber: ($routeParams.page && Number($routeParams.page) !== NaN && Number($routeParams.page) > 0) ? $routeParams.page : 1
    };

    $scope.next = function () {
        if ($scope.options.pageNumber < $scope.pageCount) {
            $scope.options.pageNumber++;
            $scope.load();
        }
    };

    $scope.goToPage = function (pageNumber) {
        $scope.options.pageNumber = pageNumber + 1;
        $scope.load();
    };

    $scope.prev = function () {
        if ($scope.options.pageNumber > 1) {
            $scope.options.pageNumber--;
            $scope.load();
        }
    };

    $scope.search = function () {

        //do we want to search by url and by domain...

        var searchTerm = $scope.searchTerm;

        $http.get("backoffice/api/RedirectUrlManagement/SearchRedirectUrls/?searchTerm=" + searchTerm + "&page=" + $scope.options.pageNumber + "&pageSize=" + $scope.options.pageSize).then(function (response) {
            var matchingItems = response.data;   
            $scope.isSearch = true;
            $scope.StatusMessage = matchingItems.StatusMessage;
            $scope.hasResults = matchingItems.HasSearchResults;
            $scope.redirectUrls = matchingItems.SearchResults;

            $scope.urlTrackerDisabled = matchingItems.UrlTrackerDisabled;
            $scope.action = "";
            if ($scope.urlTrackerDisabled !== true) {
                $scope.action = "Disable";
            } else {
                $scope.action = "Enable";
            }

            $scope.pageCount = matchingItems.PageCount;
            $scope.totalCount = matchingItems.TotalCount;
            $scope.options.pageNumber = matchingItems.CurrentPage;
            
            if ($scope.options.pageNumber > $scope.pageCount) {
                $scope.options.pageNumber = $scope.pageCount;
            }

            $scope.pagination = [];

            var i;
            if ($scope.pageCount <= 10) {
                for (i = 0; i < $scope.pageCount; i++) {
                    $scope.pagination.push({
                        val: (i + 1),
                        isActive: $scope.options.pageNumber === (i + 1)
                    });
                }
            }
            else {
                //if there is more than 10 pages, we need to do some fancy bits

                //get the max index to start
                var maxIndex = $scope.pageCount - 10;
                //set the start, but it can't be below zero
                var start = Math.max($scope.options.pageNumber - 5, 0);
                //ensure that it's not too far either
                start = Math.min(maxIndex, start);

                for (i = start; i < (10 + start) ; i++) {
                    $scope.pagination.push({
                        val: (i + 1),
                        isActive: $scope.options.pageNumber === (i + 1)
                    });
                }

                //now, if the start is greater than 0 then '1' will not be displayed, so do the elipses thing
                if (start > 0) {
                    $scope.pagination.unshift({ name: "First", val: 1, isActive: false }, { val: "...", isActive: false });
                }

                //same for the end
                if (start < maxIndex) {
                    $scope.pagination.push({ val: "...", isActive: false }, { name: "Last", val: $scope.pageCount, isActive: false });
                }
            }

            angular.forEach($scope.redirectUrls, function (item) {     
                $http.get("backoffice/api/RedirectUrlManagement/GetPublishedUrl/?id=" + item.ContentId).then(function (response) {
                    item.ContentUrl = response.data;
                });               
            });
        });
    };

    $scope.load = function () {   
        // $scope.searchTerm = "";
        $scope.search();
    };

    $scope.removeRedirect = function (redirectToDelete) {
        $http.post("backoffice/api/RedirectUrlManagement/DeleteRedirectUrl/" + redirectToDelete.Id).then(function (response) {
            if (response.status === 200) {
                notificationsService.success("Redirect Url Removed!", "Redirect Url " + redirectToDelete.Url + " has been deleted");
                // now remove from table client sides
                var index = -1;
                var urlArr = eval($scope.redirectUrls);
                for (var i = 0; i < urlArr.length; i++) {
                    if (urlArr[i].Id === redirectToDelete.Id) {
                        index = i;
                        break;
                    }
                }
                if (index === -1) {
                    notificationsService.warning("Redirect Url Removal Error!", "Redirect Url " + redirectToDelete.Url + " may have already been removed");
                }
                $scope.redirectUrls.splice(index, 1);
            }
            else {
                notificationsService.warning("Redirect Url Error!", "Redirect Url " + redirectToDelete.Url + " was not deleted");
            }   
        }); 
    };

    $scope.toggleUrlTracker = function () {
        var toggleConfirm = confirm("Are you sure you want to " + $scope.action.toLowerCase() + " the URL tracker?");
        if (toggleConfirm) {
            $http.post("backoffice/api/RedirectUrlManagement/ToggleUrlTracker/?disable=" + (!$scope.urlTrackerDisabled)).then(function (response) {
                    if (response.status === 200) {
                        notificationsService.success("URL Tracker has now been " + $scope.action.toLowerCase() + "d.");

                        $scope.load();
                    } else {
                        notificationsService.warning("Error " + $scope.action.toLowerCase() + "ing the URL Tracker, more information can be found in your log file.");
                    }
            });            
        }
    };

    $scope.load();
});