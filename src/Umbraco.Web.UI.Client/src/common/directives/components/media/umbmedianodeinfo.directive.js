(function () {
    'use strict';

    function MediaNodeInfoDirective($timeout, $location, $q, eventsService, userService, dateHelper, editorService, mediaHelper, mediaResource) {

        function link(scope, element, attrs, ctrl) {

            var evts = [];

            scope.allowChangeMediaType = false;
            scope.loading = true;

            scope.changeContentPageNumber = changeContentPageNumber;
            scope.contentOptions = {};
            scope.contentOptions.entityType = "DOCUMENT";
            scope.hasContentReferences = false;

            scope.changeMediaPageNumber = changeMediaPageNumber;
            scope.mediaOptions = {};
            scope.mediaOptions.entityType = "MEDIA";
            scope.hasMediaReferences = false;

            scope.changeMemberPageNumber = changeMemberPageNumber;
            scope.memberOptions = {};
            scope.memberOptions.entityType = "MEMBER";
            scope.hasMemberReferences = false;

            function onInit() {

                userService.getCurrentUser().then(user => {
                    // only allow change of media type if user has access to the settings sections
                    Utilities.forEach(user.sections, section => {
                        if (section.alias === "settings") {
                            scope.allowChangeMediaType = true;
                        }
                    });
                });

                // get media type details
                scope.mediaType = scope.node.contentType;

                // set the media link initially
                setMediaLink();

                // make sure dates are formatted to the user's locale
                formatDatesToLocal();

                // set media file extension initially
                setMediaExtension();
            }

            function formatDatesToLocal() {
                // get current backoffice user and format dates
                userService.getCurrentUser().then(currentUser => {
                    scope.node.createDateFormatted = dateHelper.getLocalDate(scope.node.createDate, currentUser.locale, 'LLL');
                    scope.node.updateDateFormatted = dateHelper.getLocalDate(scope.node.updateDate, currentUser.locale, 'LLL');
                });
            }

            function setMediaLink(){
                scope.nodeUrl = scope.node.mediaLink;
                // grab the file name from the URL and use it as the display name in the file link
                var match = /.*\/(.*)/.exec(scope.nodeUrl);
                if (match) {
                    scope.nodeFileName = match[1];
                } else {
                    scope.nodeFileName = scope.nodeUrl;
                }
            }

            function setMediaExtension() {
                scope.node.extension = mediaHelper.getFileExtension(scope.nodeUrl);
            }

            scope.openMediaType = mediaType => {
                var editor = {
                    id: mediaType.id,
                    submit: model => {
                        editorService.close();
                    },
                    close: () => {
                        editorService.close();
                    }
                };
                editorService.mediaTypeEditor(editor);
            };

            scope.openSVG = () => {
                var popup = window.open('', '_blank');
                var html = '<!DOCTYPE html><body><img src="' + scope.nodeUrl + '"/>' +
                    '<script>history.pushState(null, null,"' + $location.$$absUrl + '");</script></body>';
                
                popup.document.open();
                popup.document.write(html);
                popup.document.close();
            }

            // watch for content updates - reload content when node is saved, published etc.
            scope.$watch('node.updateDate', function(newValue, oldValue){
                if(!newValue) { return; }
                if(newValue === oldValue) { return; }

                // Update the media link
                setMediaLink();

                // Update the create and update dates
                formatDatesToLocal();

                //Update the media file format
                setMediaExtension();
            });

            function changeContentPageNumber(pageNumber) {
                scope.contentOptions.pageNumber = pageNumber;
                loadContentRelations();
            }

            function changeMediaPageNumber(pageNumber) {
                scope.mediaOptions.pageNumber = pageNumber;
                loadMediaRelations();
            }

            function changeMemberPageNumber(pageNumber) {
                scope.memberOptions.pageNumber = pageNumber;
                loadMemberRelations();
            }

            function loadContentRelations() {
                return mediaResource.getPagedReferences(scope.node.id, scope.contentOptions)
                    .then(function (data) {
                        scope.contentReferences = data;
                        scope.hasContentReferences = data.items.length > 0;
                    });
            }

            function loadMediaRelations() {
                return mediaResource.getPagedReferences(scope.node.id, scope.mediaOptions)
                    .then(data => {
                        scope.mediaReferences = data;
                        scope.hasMediaReferences = data.items.length > 0;
                    });
            }

            function loadMemberRelations() {
                return mediaResource.getPagedReferences(scope.node.id, scope.memberOptions)
                    .then(data => {
                        scope.memberReferences = data;
                        scope.hasMemberReferences = data.items.length > 0;
                    });
            }

            //ensure to unregister from all events!
            scope.$on('$destroy', function () {
                for (var e in evts) {
                    eventsService.unsubscribe(evts[e]);
                }
            });

            onInit();

            // load media type references when the 'info' tab is first activated/switched to
            evts.push(eventsService.on("app.tabChange", function (event, args) {
                $timeout(function () {
                    if (args.alias === "umbInfo") {

                        $q.all([loadContentRelations(), loadMediaRelations(), loadMemberRelations()]).then(function () {
                            scope.loading = false;
                        });
                    }
                });
            }));
        }

        var directive = {
            restrict: 'E',
            replace: true,
            templateUrl: 'views/components/media/umb-media-node-info.html',
            scope: {
                node: "="
            },
            link: link
        };

        return directive;
    }

    angular.module('umbraco.directives').directive('umbMediaNodeInfo', MediaNodeInfoDirective);

})();
