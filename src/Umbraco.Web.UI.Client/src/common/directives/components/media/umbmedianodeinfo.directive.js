(function () {
    'use strict';

  function MediaNodeInfoDirective($timeout, logResource, $location, $q, eventsService, userService, dateHelper, editorService, mediaHelper, mediaResource) {

        function link(scope, element, attrs, ctrl) {

            var evts = [];
            var isInfoTab = false;
            var auditTrailLoaded = false;

            scope.allowChangeMediaType = false;
            scope.historyLabelKey = "general_history";
            scope.auditTrailOptions = {
              id: scope.node.id
            };

            scope.auditTrailPageChange = function (pageNumber) {
              scope.auditTrailOptions.pageNumber = pageNumber;
              loadAuditTrail(true);
            };

            function loadAuditTrail(forceReload) {

              //don't load this if it's already done
              if (auditTrailLoaded && !forceReload) {
                return;
              }

              scope.loadingAuditTrail = true;

              logResource.getPagedEntityLog(scope.auditTrailOptions)
                .then(data => {

                  // get current backoffice user and format dates
                  userService.getCurrentUser().then(currentUser => {
                    data.items.forEach(item => {
                      item.timestampFormatted = dateHelper.getLocalDate(item.timestamp, currentUser.locale, 'LLL');
                    });
                  });

                  scope.auditTrail = data.items;
                  scope.auditTrailOptions.pageNumber = data.pageNumber;
                  scope.auditTrailOptions.pageSize = data.pageSize;
                  scope.auditTrailOptions.totalItems = data.totalItems;
                  scope.auditTrailOptions.totalPages = data.totalPages;

                  setAuditTrailLogTypeColor(scope.auditTrail);

                  scope.loadingAuditTrail = false;

                  auditTrailLoaded = true;
                });

            }

            function setAuditTrailLogTypeColor(auditTrail) {
              auditTrail.forEach(item => {

                switch (item.logType) {
                  case "Save":
                    item.logTypeColor = "success";
                    break;
                  case "Delete":
                    item.logTypeColor = "danger";
                    break;
                  default:
                    item.logTypeColor = "gray";
                }
              });
            }

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

                var activeApp = scope.node.apps.find(a => a.active);
                if (activeApp.alias === "umbInfo") {
                  loadAuditTrail();
                  isInfoTab = true;
                }
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

                loadAuditTrail(true);
            });

            //ensure to unregister from all events!
            scope.$on('$destroy', function () {
                for (var e in evts) {
                    eventsService.unsubscribe(evts[e]);
                }
            });

            evts.push(eventsService.on("app.tabChange", function (event, args) {
              $timeout(function () {
                if (args.alias === "umbInfo") {
                  isInfoTab = true;
                  loadAuditTrail();
                  formatDatesToLocal();
                } else {
                  isInfoTab = false;
                }
              });
            }));

            onInit();
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
