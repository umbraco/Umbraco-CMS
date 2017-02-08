(function () {
    'use strict';

    function MiniListViewDirective($q, contentResource, memberResource, mediaResource, entityResource) {

        function link(scope, el, attr, ctrl) {

            scope.search = "";
            scope.miniListViews = [];
            scope.breadcrumb = [];

            var miniListViewsHistory = [];
            var goingForward = true;

            function onInit() {
                open(scope.node);
            }

            function open(node) {

                goingForward = true;

                var miniListView = {
                    node: node,
                    loading: true,
                    pagination: {
                        pageSize: 10,
                        pageNumber: 1,
                        filter: '',
                        orderDirection: "Ascending",
                        orderBy: "SortOrder",
                        orderBySystemField: true
                    }
                };

                // start loading animation on node
                node.loading = true;

                getChildrenForMiniListView(miniListView)
                    .then(function (data) {

                        // stop loading animation on node
                        node.loading = false;

                        // clear and push mini list view in dom so we only render 1 view
                        scope.miniListViews = [];
                        scope.miniListViews.push(miniListView);

                        // store in history so we quickly can navigate back
                        miniListViewsHistory.push(miniListView);

                    });

                // get ancestors
                getAncestors(node);

            }

            function getChildrenForMiniListView(miniListView) {

                // setup promise
                var deferred = $q.defer();

                // start loading animation list view
                miniListView.loading = true;

                // setup the correct resource depending on section
                var resource = "";

                if (scope.entityType === "Member") {
                    resource = memberResource.getPagedResults;
                } else if (scope.entityType === "Media") {
                    resource = mediaResource.getChildren;
                } else {
                    resource = contentResource.getChildren;
                }

                resource(miniListView.node.id, miniListView.pagination)
                    .then(function (data) {

                        // update children
                        miniListView.children = data.items;

                        // update pagination
                        miniListView.pagination.totalItems = data.totalItems;
                        miniListView.pagination.totalPages = data.totalPages;

                        // stop load indicator
                        miniListView.loading = false;

                        deferred.resolve(data);
                    });

                return deferred.promise;

            }

            scope.openNode = function(event, node) {
			    open(node);
                event.stopPropagation();
            };

            scope.selectNode = function(node) {
                if(scope.onSelect) {
                    scope.onSelect({'node': node});
                }
            };

            /* Pagination */
            scope.goToPage = function(pageNumber, miniListView) {
                // set new page number
                miniListView.pagination.pageNumber = pageNumber;
                // get children
                getChildrenForMiniListView(miniListView);
            };

            /* Breadcrumb */
            scope.clickBreadcrumb = function(ancestor) {

                var found = false;
                goingForward = false;

                angular.forEach(miniListViewsHistory, function(historyItem, index){
                    if(Number(historyItem.node.id) === Number(ancestor.id)) {
                        // load the list view from history
                        scope.miniListViews = [];
                        scope.miniListViews.push(historyItem);
                        // get ancestors
                        getAncestors(historyItem.node);
                        found = true;
                    }
                });

                if(!found) {
                    // if we can't find the view in the history 
                    miniListViewsHistory = [];
                    scope.miniListViews = [];
                }

            };

            function getAncestors(node) {
                entityResource.getAncestors(node.id, scope.entityType)
                    .then(function (ancestors) {

                        // if there is a start node remove all ancestors before that one
                        if(scope.startNodeId && scope.startNodeId !== -1) {
                            var found = false;
                            scope.breadcrumb = [];
                            angular.forEach(ancestors, function(ancestor){
                                if(Number(ancestor.id) === Number(scope.startNodeId)) {
                                    found = true;
                                }
                                if(found) {
                                    scope.breadcrumb.push(ancestor);
                                }
                            });

                        } else {
                            scope.breadcrumb = ancestors;
                        }

                    });
            }

            /* Search */
            scope.searchMiniListView = function(search, miniListView) {
                // set search value
                miniListView.pagination.filter = search;
                // start loading animation list view
                miniListView.loading = true;
                searchMiniListView(miniListView);
            };

            var searchMiniListView = _.debounce(function (miniListView) {
                scope.$apply(function () {
                    getChildrenForMiniListView(miniListView);
                });
            }, 500);

            /* Animation */
            scope.getMiniListViewAnimation = function() {
                if(goingForward) {
                    return 'umb-mini-list-view--forward';
                } else {
                    return 'umb-mini-list-view--backwards';
                }
            };

            onInit();

        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/umb-mini-list-view.html',
            scope: {
                node: "=",
                entityType: "@",
                startNodeId: "=",
                onSelect: "&"
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbMiniListView', MiniListViewDirective);

})();
