function ExamineManagementController($http, $q, $timeout, umbRequestHelper, localizationService, overlayService, editorService) {

    var vm = this;

    vm.indexerDetails = [];
    vm.searcherDetails = [];
    vm.loading = true;
    vm.viewState = "list";
    vm.selectedIndex = null;
    vm.selectedSearcher = null;
    vm.searchResults = null;
    vm.showSearchResultFields = [];

    vm.showSelectFieldsDialog = showSelectFieldsDialog;
    vm.showSearchResultDialog = showSearchResultDialog;
    vm.showIndexInfo = showIndexInfo;
    vm.showSearcherInfo = showSearcherInfo;
    vm.search = search;
    vm.toggle = toggle;
    vm.rebuildIndex = rebuildIndex;
    vm.setViewState = setViewState;
    vm.nextSearchResultPage = nextSearchResultPage;
    vm.prevSearchResultPage = prevSearchResultPage;
    vm.goToPageSearchResultPage = goToPageSearchResultPage;
    vm.goToResult = goToResult;

    vm.infoOverlay = null;

    function showSelectFieldsDialog() {
        if (vm.searchResults) {

            // build list of available fields
            var availableFields = [];
            vm.searchResults.results.map(r => Object.keys(r.values).map(key => {
                if (availableFields.indexOf(key) == -1 && key != "__NodeId" && key != "nodeName") {
                    availableFields.push(key);
                }
            }));

            availableFields.sort();

            editorService.open({
                title: "Fields",
                availableFields: availableFields,
                fieldIsSelected: function(key) {
                    return vm.showSearchResultFields.indexOf(key) > -1;
                },
                toggleField: vm.toggleField,
                size: "small",
                view: "views/dashboard/settings/examinemanagementfields.html",
                close: function () {
                    editorService.close();
                }
            });
        }
    }

    function showSearchResultDialog(values) {
        if (vm.searchResults) {
            localizationService.localize("examineManagement_fieldValues").then(function (value) {
                editorService.open({
                    title: value,
                    searchResultValues: values,
                    size: "medium",
                    view: "views/dashboard/settings/examinemanagementresults.html",
                    close: function () {
                        editorService.close();
                    }
                });
            });
        }
    }

    vm.toggleField = function(key) {
        if (vm.showSearchResultFields.indexOf(key) > -1) {
            vm.showSearchResultFields = vm.showSearchResultFields.filter(field => field != key);
        }
        else {
            vm.showSearchResultFields.push(key);
        }

        vm.showSearchResultFields.sort();
    };

    function nextSearchResultPage(pageNumber) {
        search(vm.selectedIndex ? vm.selectedIndex : vm.selectedSearcher, null, pageNumber);
    }
    function prevSearchResultPage(pageNumber) {
        search(vm.selectedIndex ? vm.selectedIndex : vm.selectedSearcher, null, pageNumber);
    }
    function goToPageSearchResultPage(pageNumber) {
        search(vm.selectedIndex ? vm.selectedIndex : vm.selectedSearcher, null, pageNumber);
    }

    function goToResult(result, event) {
        if (!result.editUrl) {
            return;
        }
        // targeting a new tab/window?
        if (event.ctrlKey ||
                event.shiftKey ||
                event.metaKey || // apple
                (event.button && event.button === 1) // middle click, >IE9 + everyone else
        ) {
            // yes, let the link open itself
            return;
        }

        const editor = {
            id: result.editId,
            submit: function (model) {
                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        };
        switch (result.editSection) {
            case "content":
                editorService.contentEditor(editor);
                break;
            case "media":
                editorService.mediaEditor(editor);
                break;
            case "member":
                editorService.memberEditor(editor);
                break;
        }

        event.stopPropagation();
        event.preventDefault();
    } 

    function setViewState(state) {
        vm.searchResults = null;
        vm.viewState = state;
    }

    function showIndexInfo(index) {
        vm.selectedIndex = index;
        setViewState("index-details");
    }

    function showSearcherInfo(searcher) {
        vm.selectedSearcher = searcher;
        setViewState("searcher-details");
    }

    function checkProcessing(index, checkActionName) {
        umbRequestHelper.resourcePromise(
                $http.post(umbRequestHelper.getApiUrl("examineMgmtBaseUrl",
                    checkActionName,
                    { indexName: index.name })),
                'Failed to check index processing')
            .then(function(data) {

                if (data !== null && data !== "null") {

                    //copy all resulting properties
                    for (var k in data) {
                        index[k] = data[k];
                    }
                    index.isProcessing = false;
                } else {
                    $timeout(() => {
                            //don't continue if we've tried 100 times
                            if (index.processingAttempts < 100) {
                                checkProcessing(index, checkActionName);
                                //add an attempt
                                index.processingAttempts++;
                            } else {
                                //we've exceeded 100 attempts, stop processing
                                index.isProcessing = false;
                            }
                        },
                        1000);
                }
            });
    }

    function search(searcher, e, pageNumber) {

        //deal with accepting pressing the enter key
        if (e && e.keyCode !== 13) {
            return;
        }

        if (!searcher) {
            throw "searcher parameter is required";
        }

        searcher.isProcessing = true;

        umbRequestHelper.resourcePromise(
                $http.get(umbRequestHelper.getApiUrl("examineMgmtBaseUrl",
                    "GetSearchResults",
                    {
                        searcherName: searcher.name,
                        query: encodeURIComponent(vm.searchText),
                        pageIndex: pageNumber ? (pageNumber - 1) : 0
                    })),
                'Failed to search')
            .then(searchResults => {
                searcher.isProcessing = false;
                vm.searchResults = searchResults
                vm.searchResults.pageNumber = pageNumber ? pageNumber : 1;
                //20 is page size
                vm.searchResults.totalPages = Math.ceil(vm.searchResults.totalRecords / 20);
                // add URLs to edit well known entities
                _.each(vm.searchResults.results, function (result) {
                    var section = result.values["__IndexType"][0];
                    switch (section) {
                        case "content":
                        case "media":
                            result.editUrl = "/" + section + "/" + section + "/edit/" + result.values["__NodeId"][0];
                            result.editId = result.values["__NodeId"][0];
                            result.editSection = section;
                            break;
                        case "member":
                            result.editUrl = "/member/member/edit/" + result.values["__Key"][0];
                            result.editId = result.values["__Key"][0];
                            result.editSection = section;
                            break;
                    }
                });
            });
    }

    function toggle(provider, propName) {
        if (provider[propName] !== undefined) {
            provider[propName] = !provider[propName];
        } else {
            provider[propName] = true;
        }
    }

    function rebuildIndex(index, event) {

        const dialog = {
            view: "views/dashboard/settings/overlays/examinemanagement.rebuild.html",
            index: index,
            submitButtonLabelKey: "general_ok",
            submitButtonStyle :"danger",
            submit: function (model) {
                performRebuild(model.index);
                overlayService.close();
            },
            close: function () {
                overlayService.close();
            }
        };

        localizationService.localize("examineManagement_rebuildIndex").then(value => {
            dialog.title = value;
            overlayService.open(dialog);
        });

        event.preventDefault()
        event.stopPropagation();
    }

    function performRebuild(index) {
        index.isProcessing = true;
        index.processingAttempts = 0;

        umbRequestHelper.resourcePromise(
            $http.post(umbRequestHelper.getApiUrl("examineMgmtBaseUrl",
                "PostRebuildIndex",
                { indexName: index.name })),
            'Failed to rebuild index')
            .then(function () {

                // rebuilding has started, nothing is returned accept a 200 status code.
                // lets poll to see if it is done.
                $timeout(() => { checkProcessing(index, "PostCheckRebuildIndex"), 1000 });

            });
    }

    function init() {
        // go get the data

        //combine two promises and execute when they are both done
        $q.all([

                //get the indexer details
                umbRequestHelper.resourcePromise(
                    $http.get(umbRequestHelper.getApiUrl("examineMgmtBaseUrl", "GetIndexerDetails")),
                    'Failed to retrieve indexer details')
                .then(function (data) {
                    vm.indexerDetails = data;
                }),

                //get the searcher details
                umbRequestHelper.resourcePromise(
                    $http.get(umbRequestHelper.getApiUrl("examineMgmtBaseUrl", "GetSearcherDetails")),
                    'Failed to retrieve searcher details')
                .then(data => {
                    vm.searcherDetails = data;
                })
            ])
            .then(() => { vm.loading = false });
    }

    init();
}

angular.module("umbraco").controller("Umbraco.Dashboard.ExamineManagementController", ExamineManagementController);
