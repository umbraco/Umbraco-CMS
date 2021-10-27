(function () {
    'use strict';

    function MiniListViewDirective(entityResource, iconHelper) {

        function link(scope, el, attr, ctrl) {

            scope.search = "";
            scope.miniListViews = [];
            scope.breadcrumb = [];
            scope.listViewAnimation = "";

            var miniListViewsHistory = [];

            function onInit() {
                open(scope.node);
            }

            function open(node) {

                // convert legacy icon for node
                if(node && node.icon) {
                    node.icon = iconHelper.convertFromLegacyIcon(node.icon);
                }

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

                // clear and push mini list view in dom so we only render 1 view
                scope.miniListViews = [];
                scope.listViewAnimation = "in";
                scope.miniListViews.push(miniListView);

                // store in history so we quickly can navigate back
                miniListViewsHistory.push(miniListView);

                // get children
                getChildrenForMiniListView(miniListView);

                makeBreadcrumb();

            }

            function getChildrenForMiniListView(miniListView) {

                // start loading animation list view
                miniListView.loading = true;
                
                entityResource.getPagedChildren(miniListView.node.id, scope.entityType, miniListView.pagination)
                    .then(function (data) {
                        if (!data.items) {
                            data.items = [];
                        }
                        if (scope.onItemsLoaded) {
                            scope.onItemsLoaded({items: data.items});
                        }
                        // update children
                        miniListView.children = data.items;
                        miniListView.children.forEach(c => {
                            // child allowed by default
                            c.allowed = true;
 
                            // convert legacy icon for node
                            if(c.icon) {
                                c.icon = iconHelper.convertFromLegacyIcon(c.icon);
                            }
                            // set published state for content
                            if (c.metaData) {
                                c.hasChildren = c.metaData.hasChildren;
                                if(scope.entityType === "Document") {
                                    c.published = c.metaData.IsPublished;
                                }
                            }
                             
                            // filter items if there is a filter and it's not advanced (advanced filtering is handled below)
                            if (scope.entityTypeFilter && scope.entityTypeFilter.filter && !scope.entityTypeFilter.filterAdvanced) {
                                var a = scope.entityTypeFilter.filter.toLowerCase().replace(/\s/g, '').split(',');
                                var found = a.indexOf(c.metaData.ContentTypeAlias.toLowerCase()) >= 0;
                                
                                if (!scope.entityTypeFilter.filterExclude && !found || scope.entityTypeFilter.filterExclude && found) {
                                    c.allowed = false;
                                }
                            }
                        });

                        // advanced item filtering is handled here
                        if (scope.entityTypeFilter && scope.entityTypeFilter.filter && scope.entityTypeFilter.filterAdvanced) {
                            var filtered = Utilities.isFunction(scope.entityTypeFilter.filter)
                                ? _.filter(miniListView.children, scope.entityTypeFilter.filter)
                                : _.where(miniListView.children, scope.entityTypeFilter.filter);
                            
                            filtered.forEach(node => node.allowed = false);
                        }

                        // update pagination
                        miniListView.pagination.totalItems = data.totalItems;
                        miniListView.pagination.totalPages = data.totalPages;
                        // stop load indicator
                        miniListView.loading = false;
                    });
            }

            scope.openNode = function(event, node) {
			    open(node);
                event.stopPropagation();
            };

            scope.selectNode = function(node) {
                if (scope.onSelect && node.allowed) {
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
                scope.listViewAnimation = "out";

                Utilities.forEach(miniListViewsHistory, (historyItem, index) => {
                    // We need to make sure we can compare the two id's. 
                    // Some id's are integers and others are strings.
                    // Members have string ids like "all-members".
                    if (historyItem.node.id.toString() === ancestor.id.toString()) {
                        // load the list view from history
                        scope.miniListViews = [];
                        scope.miniListViews.push(historyItem);
                        // clean up history - remove all children after
                        miniListViewsHistory.splice(index + 1, miniListViewsHistory.length);
                        found = true;
                    }
                });

                if (!found) {
                    // if we can't find the view in the history - close the list view
                    scope.exitMiniListView();
                }

                // update the breadcrumb
                makeBreadcrumb();

            };

            scope.showBackButton = function() {
                // don't show the back button if the start node is a list view
                if (scope.node.metaData && scope.node.metaData.IsContainer || scope.node.isContainer) {
                    return false;
                } else {
                    return true;
                }
            };

            scope.exitMiniListView = function() {
                miniListViewsHistory = [];
                scope.miniListViews = [];
                if(scope.onClose) {
                    scope.onClose();
                }
            };

            function makeBreadcrumb() {
                scope.breadcrumb = [];
                Utilities.forEach(miniListViewsHistory, historyItem => {
                    scope.breadcrumb.push(historyItem.node);
                });
            }

            /* Search */
            scope.searchMiniListView = function(search, miniListView) {
                // set search value
                miniListView.pagination.filter = search;
                // reset pagination
                miniListView.pagination.pageNumber = 1;
                // start loading animation list view
                miniListView.loading = true;
                searchMiniListView(miniListView);
            };

            var searchMiniListView = _.debounce(function (miniListView) {
                scope.$apply(function () {
                    getChildrenForMiniListView(miniListView);
                });
            }, 500);

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
                onSelect: "&",
                onClose: "&",
                onItemsLoaded: "&",
                entityTypeFilter: "="
            },
            link: link
        };

        return directive;

    }

    angular.module('umbraco.directives').directive('umbMiniListView', MiniListViewDirective);

})();
