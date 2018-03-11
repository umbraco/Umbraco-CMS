/**
 * @ngdoc controller
 * @name Umbraco.Editors.DocumentType.RedirectUrlManagement
 * @function
 *
 * @description
 * The controller for the redirect url management overlay dialog
 */
function RedirectUrlManagementOverlay($scope, $http, notificationsService, redirectUrlsResource, localizationService, navigationService) {

    var vm = this;

    vm.status = {
        loading: false,
        loaded: false,
        hasRedirects: false,
        currentContentItem: null
    };
    vm.data = {
        redirects: [],
        redirectFromUrl: ''
    }


    vm.addRedirect = addRedirect;
    vm.removeRedirect = removeRedirect;

    function removeRedirect(redirectToDelete) {
        
        localizationService.localize("redirectUrls_confirmRemove", [redirectToDelete.originalUrl, vm.status.currentContentItem.name]).then(function (value) {
            var toggleConfirm = confirm(value);
            var deletedRedirectUrl = redirectToDelete.url;
            if (toggleConfirm) {
                redirectUrlsResource.deleteRedirectUrl(redirectToDelete.redirectId).then(function () {
                    var index = vm.data.redirects.indexOf(redirectToDelete);
                    vm.data.redirects.splice(index, 1);
                    //close the dialog
                    $scope.submit({ redirectAction: "Remove",actionSuccess: true, statusMessage: "deleted '" + deletedRedirectUrl + "'"});
             
                }, function (error) {
                    //close the dialog
                    $scope.submit({
                        redirectAction: "Remove",actionSuccess: false, statusMessage: "error deleting '" + deletedRedirectUrl + "'", error: error });
                });
            }
        });

    }
    function checkUrl(url) {
        //this could do with some thought, perhaps should be 'somewhere' else more reusable
        //also probably more thought given to rules
        //also how best to localize the text of  these messages
        var validUrlStatus = {
            isValidUrl: false,
            statusMessage: 'InValid Url',
            url: url
        };
        if (url.length > 0) {
            //assume will be valid
            validUrlStatus.isValidUrl = true;
            validUrlStatus.statusMessage = "Valid Url";
            if (url.length < 3) {
                validUrlStatus.statusMessage = "'Url to Redirect From' must be a little longer than maybe 2 chars?";
                validUrlStatus.isValidUrl = false;
            }
            // only relative urls, like the tracker? or does the tracker add the domain if one is set?
            if (url.startsWith('http')) {
                validUrlStatus.statusMessage = "'Url to Redirect From' must be relative, and start with a leading /, not http or https";
                validUrlStatus.isValidUrl = false;
            }
            if (!url.startsWith('/')) {
                validUrlStatus.statusMessage = "'Url to Redirect From' must be relative, and start with a leading /";
                validUrlStatus.isValidUrl = false;
            }
            // you can't have any spaces in the url
            if (url.indexOf(' ') >= 0) {
                validUrlStatus.statusMessage = "'Url to Redirect From' cannot contain whitespace";
                validUrlStatus.isValidUrl = false;
            }
            // Url has a file extension eg /oldpage.asp ?
            // we don't want to use this mechnism for redirect hundreds of old system urls, that's more of a rewrite rule thing, create a static rewrite map if necessary
            // so not really thinking this through but can we safely disallow any url with . in it?
            if (url.indexOf('.') >= 0) {
                validUrlStatus.statusMessage = "'Url to Redirect From' cannot contain a dot/full stop - this is just for handling simple renames";
                validUrlStatus.isValidUrl = false;
            }
             //Other things still to consider...
                // Url contains invalid characters?    
                // Url already exists in dashboard and redirects somewhere else - do we replace the entry already there? or error?
                // Url is a redirect to itself
            // the core redirectservice is adding a slash on the end, (regardless of  useTrailingSlash value - so we're stripping the trailing slash here...
            if (url.endsWith('/')) {
                validUrlStatus.url = url.slice(0, -1);
            }
        }
        return validUrlStatus;
    }
    function addRedirect() {
        //check if url is in a valid format
        //eg relative, without the 'protocol' and without extension
        //probably a clever regex here..
        // or a redirectUrlCheckService...

        var urlStatus = checkUrl(vm.data.redirectFromUrl);

        if (urlStatus.isValidUrl) {
            //Add Service to add a Url?
            redirectUrlsResource.createRedirectUrl(urlStatus.url, vm.status.currentContentItem.udi).then(function (data) {
                console.log(data);
                //close the dialog
                $scope.submit({ redirectAction: "Create", actionSuccess: true, statusMessage: "Redirect '" + vm.data.redirectFromUrl + "' added" });

            }, function (error) {
                //close the dialog
                $scope.submit({ redirectAction: "Create", actionSuccess: false, statusMessage: urlStatus.statusMessage, error: error });
            });        
          
        }
        else {
            $scope.submit({ redirectAction: "Create", actionSuccess: false, statusMessage: urlStatus.statusMessage });
        }
    };

    function getRedirects(udi) {

        vm.status.loading = true;
        redirectUrlsResource.getRedirectsForContentItem(udi)
            .then(function (data) {
                console.log(data);
                vm.data.redirects = data.searchResults;
                vm.status.hasRedirects = (typeof data.searchResults !== 'undefined' && data.searchResults.length > 0);
                vm.status.loading = false;
                vm.status.loaded = true;
            });
    }

    function init() {
        // go off and get all redirects for this node
        vm.status.currentContentItem = $scope.dialogData;
        getRedirects($scope.dialogData.udi);
        vm.data.redirectFromUrl = '';
    }

    init();

}

angular.module("umbraco").controller("Umbraco.Overlays.RedirectUrlManagementOverlay", RedirectUrlManagementOverlay);
