angular.module("umbraco")
    .controller("Umbraco.Editors.RelatedLinksController", 
        function ($rootScope, $scope, $routeParams, contentResource, contentTypeResource, editorContextService, notificationsService) {

            $scope.newCaption = '';
            $scope.newLink = 'http://';
            $scope.newNewWindow = false;
            
            $scope.relatedLinks = [
                { caption: 'Google', link: "http://google.com", newWindow: false },
                { caption: 'Umbraco', link: "http://umbraco.com", newWindow: false },
                { caption: 'Nibble', link: "http://nibble.be", newWindow: false }
            ];

            $scope.delete = function (idx) {
                $scope.relatedLinks.splice($scope.relatedLinks[idx], 1);
                
            };

            $scope.add = function() {
                var newLink = new function() {
                    this.caption = $scope.newCaption;
                    this.link = $scope.newLink;
                    this.newWindow = $scope.newNewWindow;
                };

                $scope.newCaption = '';
                $scope.newLink = '';
                $scope.newNewWindow = false;
                
                $scope.relatedLinks.push(newLink);
            };
        });